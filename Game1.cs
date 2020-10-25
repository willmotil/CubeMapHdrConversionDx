using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// https://hdrihaven.com/hdri/?h=schadowplatz


namespace CubeMapHdrConversionDx
{
    public class Game1 : Game
    {
        #region class fields and stuff

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        string msg = "";

        private Effect _textureCubeBuildEffect;
        private Effect _drawingEffect;

        private Texture2D _sphericalTexture2DEnviromentalMap;
        private Texture2D _generatedSphericalTexture2DFromCube;
        private Texture2D[] _generatedTextureFaceArray;
        private Texture2D _generatedSphericalTexture2DFromFaceArray;
        private TextureCube _textureCubeEnviroment;
        private TextureCube _textureCubeIblDiffuseIllumination;
        private TextureCube _textureCubeIblSpecularPrefilter;
        private TextureCube _generatedTextureCubeFromFaceArray;

        public Rectangle r_sphericalTexture2DEnviromentalMap;
        public Rectangle r_face4_Left;
        public Rectangle r_face0_Forward;
        public Rectangle r_face5_Right;
        public Rectangle r_face1_Back;
        public Rectangle r_face3_Top;
        public Rectangle r_face2_Bottom;
        public Rectangle r_generatedSphericalTexture2DFromCube;
        public Rectangle r_generatedSphericalTexture2DFromFaceArray;

        Primitive2dQuadBuffer scrQuads = new Primitive2dQuadBuffer();

        private string[] _currentCubeMapShown = new string[] { "_textureCubeEnviroment", "_textureCubeIblDiffuseIllumination", "_generatedTextureCubeFromFaceArray" };

        private PrimitiveCube skyCube = new PrimitiveCube(500, false, false, true);
        private PrimitiveCube[] cubes = new PrimitiveCube[5];

        private float _mipLevelTestValue = 0;
        private int _whichCubeMapToDraw = 0; // enviromentalMapFromHdr =1,  enviromentalDiffuseIlluminationMap = 2, 
        private SurfaceFormat _initialLoadedFormat;

        private bool _wireframe = false;
        private RasterizerState rs_wireframe = new RasterizerState() { FillMode = FillMode.WireFrame };

        private DemoCamera _camera;
        private Vector3[] _cameraWayPoints = new Vector3[] { new Vector3(-50, 15, 5), new Vector3(20, 0, -50), new Vector3(50, 0, 5), new Vector3(20, -15, 50) };
        private bool _useDemoWaypoints = false;
        private Vector3 _targetLookAt = new Vector3(0, 0, 0);

        private Matrix _projectionBuildSkyCubeMatrix;
        private static Vector3 sbpPos;
        private Matrix sbpWorld;

        #endregion




        //____________________________________________
        //

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            IsMouseVisible = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(Microsoft.Xna.Framework.Game).Assembly.Location).FileVersion;
            this.Window.Title = $"SphereCube Image Hdr Mapping Effect Tests  {version}";
            this.Window.AllowUserResizing = true;
            this.Window.AllowAltF4 = true;
            this.Window.ClientSizeChanged += ClientResize;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = new HardCodedSpriteFont().LoadHardCodeSpriteFont(GraphicsDevice);
            _textureCubeBuildEffect = Content.Load<Effect>("TextureCubeBuildEffect");
            _drawingEffect = Content.Load<Effect>("TextureCubeDrawEffect");

            // Hdr's TextureFormat option in the content pipeline editor needs to be set to none instead of color.
            _sphericalTexture2DEnviromentalMap = Content.Load<Texture2D>("hdr_royal_esplanade_2k");  // "hdr_colorful_studio_2k" "schadowplatz_2k"
            _initialLoadedFormat = _sphericalTexture2DEnviromentalMap.Format;

            LoadPrimitives();
            CreateIblCubeMaps();
            SetupCamera();
            DetermineRectanglePositionsAddToQuads();
        }

        public void ClientResize(object sender, EventArgs e)
        {
            _projectionBuildSkyCubeMatrix = Matrix.CreatePerspectiveFieldOfView(90 * (float)((3.14159265358f) / 180f), GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height, 0.01f, 1000f);
            sbpPos = _camera.CameraWorldPositionVectorForPerspectiveSpriteBatch(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 1f);
        }

        public void DetermineRectanglePositionsAddToQuads()
        {
            int xoffset = 0;
            r_sphericalTexture2DEnviromentalMap = new Rectangle(xoffset, 0, 200, 100);
            xoffset += 120;
            r_face4_Left = new Rectangle(xoffset + 0, 105, 95, 95);
            r_face0_Forward = new Rectangle(xoffset + 100, 105, 95, 95);
            r_face5_Right = new Rectangle(xoffset + 200, 105, 95, 95);
            r_face1_Back = new Rectangle(xoffset + 300, 105, 95, 95);
            r_face3_Top = new Rectangle(xoffset + 100, 5, 95, 95);
            r_face2_Bottom = new Rectangle(xoffset + 100, 205, 95, 95);
            xoffset += 220;
            r_generatedSphericalTexture2DFromCube = new Rectangle(xoffset, 0, 200, 100);
            xoffset += 220;
            r_generatedSphericalTexture2DFromFaceArray = new Rectangle(xoffset, 0, 200, 100);

            float depth = 1;
            scrQuads.AddVertexRectangleToBuffer(r_sphericalTexture2DEnviromentalMap, depth);
            scrQuads.AddVertexRectangleToBuffer(r_face4_Left, depth);
            scrQuads.AddVertexRectangleToBuffer(r_face0_Forward, depth);
            scrQuads.AddVertexRectangleToBuffer(r_face5_Right, depth);
            scrQuads.AddVertexRectangleToBuffer(r_face1_Back, depth);
            scrQuads.AddVertexRectangleToBuffer(r_face3_Top, depth);
            scrQuads.AddVertexRectangleToBuffer(r_face2_Bottom, depth);
            scrQuads.AddVertexRectangleToBuffer(r_generatedSphericalTexture2DFromCube, depth);
            scrQuads.AddVertexRectangleToBuffer(r_generatedSphericalTexture2DFromFaceArray, depth);
        }

        public void LoadPrimitives()
        {
            for (int i = 0; i < 5; i++)
                cubes[i] = new PrimitiveCube(1, true, false, true);
        }

        public void CreateIblCubeMaps()
        {
            Console.WriteLine($"\n Rendered to scene.");
            if (_sphericalTexture2DEnviromentalMap != null)
            {
                // spherical map to  texture cube    SphericalToCubeMap
                _textureCubeEnviroment = TextureTypeConverter.ConvertSphericalTexture2DToTextureCube(GraphicsDevice, _textureCubeBuildEffect, _sphericalTexture2DEnviromentalMap, true, true, 512);

                //// to texture face.                       SphericaToTextureArrayFaces
                //_generatedTextureFaceArray = TextureTypeConverter.ConvertSphericalTexture2DToTexture2DArray(GraphicsDevice, _textureCubeBuildEffect, _sphericalTexture2DEnviromentalMap, true, true, 512);

                // to array                                     CubeMapToTextureArrayFaces
                _generatedTextureFaceArray = TextureTypeConverter.ConvertTextureCubeToTexture2DArray(GraphicsDevice, _textureCubeBuildEffect, _textureCubeEnviroment, false, false, 256);

                // array to... 
                // to another cube.                         FaceArrayToCubeFaces
                _generatedTextureCubeFromFaceArray = TextureTypeConverter.ConvertTexture2DArrayToTextureCube(GraphicsDevice, _textureCubeBuildEffect, _generatedTextureFaceArray, false, true, 256);

                // array to... 
                // a spherical map
                _generatedSphericalTexture2DFromFaceArray = TextureTypeConverter.ConvertTexture2DArrayToSphericalTexture2D(GraphicsDevice, _textureCubeBuildEffect, _generatedTextureFaceArray, false, true, 256);

                // cubemap to...
                // to spherical map
                _generatedSphericalTexture2DFromCube = TextureTypeConverter.ConvertTextureCubeToSphericalTexture2D(GraphicsDevice, _textureCubeBuildEffect, _textureCubeEnviroment, false, true, 512);

                // texture cube  ...
                // to cube                                     CubemapToCubemap  DiffuseIlluminationCubeMap
                _textureCubeIblDiffuseIllumination = TextureTypeConverter.ConvertTextureCubeToTextureCube(GraphicsDevice, _textureCubeBuildEffect, _textureCubeEnviroment, false, true, 512);

            }
        }

        //public void SetupCamera()
        //{
        //    // a 90 degree field of view is needed for the projection matrix.
        //    _projectionBuildSkyCubeMatrix = Matrix.CreatePerspectiveFieldOfView(90.0f * (3.14159265358f / 180f), GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height, 0.01f, 1000f);
        //    _camera = new DemoCamera(GraphicsDevice, _spriteBatch, null, new Vector3(2, 2, 10), new Vector3(0, 0, 0), Vector3.UnitY, 0.1f, 10000f, 1f, true, false);
        //    _camera.TransformCamera(_camera.World.Translation, _targetLookAt, _camera.World.Up);
        //    _camera.Up = Vector3.Up;
        //    _camera.WayPointCycleDurationInTotalSeconds = 25f;
        //    _camera.MovementSpeedPerSecond = 8f;
        //    _camera.SetWayPoints(_cameraWayPoints, true, 30);
        //}

        public void SetupCamera()
        {
            // a 90 degree field of view is needed for the projection matrix.
            _projectionBuildSkyCubeMatrix = Matrix.CreatePerspectiveFieldOfView(90.0f * (3.14159265358f / 180f), GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height, 0.01f, 1000f);
            _camera = new DemoCamera(GraphicsDevice, _spriteBatch, null, new Vector3(2, 2, 10), new Vector3(0, 0, 0), Vector3.UnitY, 0.1f, 10000f, 1f, true, false);
            _camera.TransformCamera(_camera.World.Translation, _targetLookAt, _camera.World.Up);
            _camera.Up = Vector3.Up;
            _camera.WayPointCycleDurationInTotalSeconds = 25f;
            _camera.MovementSpeedPerSecond = 8f;
            _camera.SetWayPoints(_cameraWayPoints, true, 30);

            sbpPos = _camera.CameraWorldPositionVectorForPerspectiveSpriteBatch(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 1f);
            sbpWorld = _camera.GetWorldMatrixForPerspectiveProjectionAlignedToSpritebatch(GraphicsDevice, Vector3.Up);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (IsPressedWithDelay(Keys.Space, gameTime))
                _useDemoWaypoints = !_useDemoWaypoints;

            if (IsPressedWithDelay(Keys.F1, gameTime))
                _whichCubeMapToDraw -= 1;

            if (IsPressedWithDelay(Keys.F2, gameTime))
                _whichCubeMapToDraw += 1;

            if (IsPressedWithDelay(Keys.F3, gameTime))
                UpdateMipLevel(gameTime);

            if (IsPressedWithDelay(Keys.F4, gameTime))
                _wireframe = !_wireframe;

            if (IsPressedWithDelay(Keys.F5, gameTime))
                _camera.TransformCamera(new Vector3(0, 0, 0), _camera.Forward, Vector3.Up);

            if (IsPressedWithDelay(Keys.F6, gameTime))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                CreateIblCubeMaps();
                stopwatch.Stop();
                Console.WriteLine($" Time elapsed: { stopwatch.Elapsed.TotalMilliseconds}ms   {stopwatch.Elapsed.TotalSeconds}sec ");
            }

            if (IsPressedWithDelay(Keys.F7, gameTime))
            {

                for (int i = 0; i < 6; i++)
                {
                    var path = @"C:\Users\will\MainFiles\Output\face" + i.ToString()+@".png";
                    Console.WriteLine(path);
                    TextureTypeConverter.SaveTexture2D(path, _generatedTextureFaceArray[i]);
                }
            }

            if (_whichCubeMapToDraw > 2)
                _whichCubeMapToDraw = 0;
            if (_whichCubeMapToDraw < 0)
                _whichCubeMapToDraw = 2;

            _camera.Update(_targetLookAt, _useDemoWaypoints, gameTime);

            msg =
            $" Camera.Forward: \n  { _camera.Forward.X.ToString("N3") } { _camera.Forward.Y.ToString("N3") } { _camera.Forward.Z.ToString("N3") } \n Cullmode \n {GraphicsDevice.RasterizerState.CullMode} \n MipLevelTestValue {_mipLevelTestValue} " +
            $"\n\n KeyBoard Commands: \n F1 F2 - Display CubeMap {_currentCubeMapShown[_whichCubeMapToDraw]} \n F3 _ Change Mip Level \n F4 - Wireframe or Solid \n F5 - Set Camera to Center \n F6 - Rebuild and time EnvIlluminationMap \n SpaceBar - Camera toggle manual or waypoint \n Q thru C + arrow keys - Manual Camera Control"
            ;

            base.Update(gameTime);
        }

        public void UpdateMipLevel(GameTime gameTime)
        {
            _mipLevelTestValue++;
            if (_mipLevelTestValue > _textureCubeEnviroment.LevelCount)
                _mipLevelTestValue = 0;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            if (_wireframe)
                GraphicsDevice.RasterizerState = rs_wireframe;

            DrawPrimitives(gameTime);

            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        protected void DrawPrimitives(GameTime gameTime)
        {
            _drawingEffect.Parameters["View"].SetValue(_camera.View);
            _drawingEffect.Parameters["testValue1"].SetValue((int)_mipLevelTestValue);

            _drawingEffect.CurrentTechnique = _drawingEffect.Techniques["RenderCubeMap"];

            //DrawPrimitiveSkyCube(gameTime);

            //DrawPrimitiveSceneCubes(gameTime);

            PrimitiveDrawLoadedAndGeneratedTextures();
        }

        private void DrawPrimitiveSkyCube(GameTime gameTime)
        {
            _drawingEffect.Parameters["Projection"].SetValue(_projectionBuildSkyCubeMatrix);
            _drawingEffect.Parameters["CameraPosition"].SetValue(Vector3.Zero); // _camera.Position
            _drawingEffect.Parameters["World"].SetValue(Matrix.CreateWorld(Vector3.Zero, _camera.Forward, _camera.Up)); //_camera.World

            if (_whichCubeMapToDraw == 0 && _textureCubeEnviroment != null)
                skyCube.DrawPrimitiveCube(GraphicsDevice, _drawingEffect, _textureCubeEnviroment);
            if (_whichCubeMapToDraw == 1 && _textureCubeIblDiffuseIllumination != null)
                skyCube.DrawPrimitiveCube(GraphicsDevice, _drawingEffect, _textureCubeIblDiffuseIllumination);
            if (_whichCubeMapToDraw == 2 && _generatedTextureCubeFromFaceArray != null)
                skyCube.DrawPrimitiveCube(GraphicsDevice, _drawingEffect, _generatedTextureCubeFromFaceArray);
        }

        private void DrawPrimitiveSceneCubes(GameTime gameTime)
        {
            _drawingEffect.Parameters["Projection"].SetValue(_camera.Projection);
            _drawingEffect.Parameters["CameraPosition"].SetValue(_camera.Position);

            int i = 0;
            if (_textureCubeEnviroment != null)
            {
                _drawingEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(i * 2 + 70, i * 2, i * -2.5f + -8)));
                cubes[1].DrawPrimitiveCube(GraphicsDevice, _drawingEffect, _textureCubeEnviroment);
                i++;
            }
            if (_textureCubeIblDiffuseIllumination != null)
            {
                _drawingEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(i * 2 + 70, i * 2, i * -2.5f + -8)));
                cubes[1].DrawPrimitiveCube(GraphicsDevice, _drawingEffect, _textureCubeIblDiffuseIllumination);
                i++;
            }
            if (_generatedTextureCubeFromFaceArray != null)
            {
                _drawingEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(i * 2 + 70, i * 2, i * -2.5f + -8)));
                cubes[1].DrawPrimitiveCube(GraphicsDevice, _drawingEffect, _generatedTextureCubeFromFaceArray);
                i++;
            }

        }

        public void PrimitiveDrawLoadedAndGeneratedTextures()
        {
            sbpPos = _camera.CameraWorldPositionVectorForPerspectiveSpriteBatch(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 1f);
            //sbpView = _camera.ViewMatrixForPerspectiveSpriteBatch(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 90 * (float)((3.14159265358f) / 180f), Vector3.Forward,Vector3.Down);
            sbpWorld = _camera.GetWorldMatrixForPerspectiveProjectionAlignedToSpritebatch(GraphicsDevice,Vector3.Up);

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            _drawingEffect.CurrentTechnique = _drawingEffect.Techniques["QuadDraw"];
            _drawingEffect.Parameters["Projection"].SetValue(_projectionBuildSkyCubeMatrix);
            _drawingEffect.Parameters["View"].SetValue(Matrix.Invert(sbpWorld));
            _drawingEffect.Parameters["CameraPosition"].SetValue(sbpPos); // _camera.Position // Vector3.Zero
            _drawingEffect.Parameters["World"].SetValue(Matrix.Identity);   //Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Down)); //_camera.World

            int xoffset = 0;
            Color textColor = Color.White;

            if (_sphericalTexture2DEnviromentalMap != null)
            {
                _drawingEffect.Parameters["TextureA"].SetValue(_sphericalTexture2DEnviromentalMap);
                scrQuads.DrawQuadRangeInBuffer(GraphicsDevice, _drawingEffect, 0, 1);
            }

            xoffset += 120;
            if (_generatedTextureFaceArray != null)
            {
                _drawingEffect.Parameters["TextureA"].SetValue(_generatedTextureFaceArray[4]);
                scrQuads.DrawQuadRangeInBuffer(GraphicsDevice, _drawingEffect, 1, 1);
                _drawingEffect.Parameters["TextureA"].SetValue(_generatedTextureFaceArray[0]);
                scrQuads.DrawQuadRangeInBuffer(GraphicsDevice, _drawingEffect, 2, 1);
                _drawingEffect.Parameters["TextureA"].SetValue(_generatedTextureFaceArray[5]);
                scrQuads.DrawQuadRangeInBuffer(GraphicsDevice, _drawingEffect, 3, 1);
                _drawingEffect.Parameters["TextureA"].SetValue(_generatedTextureFaceArray[1]);
                scrQuads.DrawQuadRangeInBuffer(GraphicsDevice, _drawingEffect, 4, 1);
                _drawingEffect.Parameters["TextureA"].SetValue(_generatedTextureFaceArray[2]);
                scrQuads.DrawQuadRangeInBuffer(GraphicsDevice, _drawingEffect, 5, 1);
                _drawingEffect.Parameters["TextureA"].SetValue(_generatedTextureFaceArray[3]);
                scrQuads.DrawQuadRangeInBuffer(GraphicsDevice, _drawingEffect, 6, 1);
            }

            xoffset += 220;
            if (_generatedSphericalTexture2DFromCube != null)
            {
                _drawingEffect.Parameters["TextureA"].SetValue(_generatedSphericalTexture2DFromCube);
                scrQuads.DrawQuadRangeInBuffer(GraphicsDevice, _drawingEffect, 7, 1);
            }

            xoffset += 220;
            if (_generatedSphericalTexture2DFromFaceArray != null)
            {
                _drawingEffect.Parameters["TextureA"].SetValue(_generatedSphericalTexture2DFromFaceArray);
                scrQuads.DrawQuadRangeInBuffer(GraphicsDevice, _drawingEffect, 8, 1);
            }
        }

        public void DrawSpriteBatches(GameTime gameTime)
        {
            _spriteBatch.Begin();

            //SpriteBatchDrawLoadedAndGeneratedTextures();

            _camera.DrawCurveThruWayPointsWithSpriteBatch(1.5f, new Vector3(GraphicsDevice.Viewport.Bounds.Right -100, 1, GraphicsDevice.Viewport.Bounds.Bottom - 100), 1, gameTime);

            _spriteBatch.DrawString(_font, msg, new Vector2(10, 210), Color.Moccasin);

            _spriteBatch.End();
        }

        public void SpriteBatchDrawLoadedAndGeneratedTextures()
        {
            int xoffset = 0;
            Color textColor = Color.White;
            if (_sphericalTexture2DEnviromentalMap != null)
            {
                _spriteBatch.Draw(_sphericalTexture2DEnviromentalMap, new Rectangle(xoffset, 0, 200, 100), Color.White);
                _spriteBatch.DrawString(_font, $"Loaded SphericalTexture2D \n format {_initialLoadedFormat}", new Vector2(xoffset, 10), textColor);
            }

            xoffset += 120;
            if (_generatedTextureFaceArray != null)
            {
                _spriteBatch.Draw(_generatedTextureFaceArray[4], new Rectangle(xoffset + 0, 105, 95, 95), Color.White);
                _spriteBatch.DrawString(_font, "face Left", new Vector2(xoffset + 0, 105), textColor);
                _spriteBatch.Draw(_generatedTextureFaceArray[0], new Rectangle(xoffset + 100, 105, 95, 95), Color.White);
                _spriteBatch.DrawString(_font, "face Forward", new Vector2(xoffset + 100, 105), textColor);
                _spriteBatch.Draw(_generatedTextureFaceArray[5], new Rectangle(xoffset + 200, 105, 95, 95), Color.White);
                _spriteBatch.DrawString(_font, "face Right", new Vector2(xoffset + 200, 105), textColor);
                _spriteBatch.Draw(_generatedTextureFaceArray[1], new Rectangle(xoffset + 300, 105, 95, 95), Color.White);
                _spriteBatch.DrawString(_font, "face Back", new Vector2(xoffset + 300, 105), textColor);
                _spriteBatch.Draw(_generatedTextureFaceArray[2], new Rectangle(xoffset + 100, 5, 95, 95), Color.White);
                _spriteBatch.DrawString(_font, "face Top", new Vector2(xoffset + 100, 5), textColor);
                _spriteBatch.Draw(_generatedTextureFaceArray[3], new Rectangle(xoffset + 100, 205, 95, 95), Color.White);
                _spriteBatch.DrawString(_font, "face Bottom", new Vector2(xoffset + 100, 205), textColor);
            }

            xoffset += 220;
            if (_generatedSphericalTexture2DFromCube != null)
            {
                _spriteBatch.Draw(_generatedSphericalTexture2DFromCube, new Rectangle(xoffset, 0, 200, 100), Color.White);
                _spriteBatch.DrawString(_font, "SphericalTexture2D \nFromCube", new Vector2(xoffset + 0, 10), textColor);
            }

            xoffset += 220;
            if (_generatedSphericalTexture2DFromFaceArray != null)
            {
                _spriteBatch.Draw(_generatedSphericalTexture2DFromFaceArray, new Rectangle(xoffset, 0, 200, 100), Color.White);
                _spriteBatch.DrawString(_font, "SphericalTexture2D \nFromFaceArray", new Vector2(xoffset, 10), textColor);
            }
        }

        #region helper functions

        public bool IsPressedWithDelay(Keys key, GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(key) && IsUnDelayed(gameTime))
                return true;
            else
                return false;
        }

        float delay = 0f;
        bool IsUnDelayed(GameTime gametime)
        {
            if (delay < 0)
            {
                delay = .25f;
                return true;
            }
            else
            {
                delay -= (float)gametime.ElapsedGameTime.TotalSeconds;
                return false;
            }
        }

        #endregion

    }
}
