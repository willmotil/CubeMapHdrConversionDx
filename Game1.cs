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

        private PrimitiveCube skyCube;
        private PrimitiveCube[] cubes = new PrimitiveCube[5];

        private float _mipLevelTestValue = 0;
        private int _whichCubeMapToDraw = 0; // enviromentalMapFromHdr =1,  enviromentalDiffuseIlluminationMap = 2, 
        private SurfaceFormat _initialLoadedFormat;

        private bool _wireframe = false;
        private RasterizerState rs_wireframe = new RasterizerState() { FillMode = FillMode.WireFrame };

        //private DemoCamera _cameraSpritebatch;
        private DemoCamera _cameraCinematic;
        private Vector3[] _cameraWayPoints = new Vector3[] { new Vector3(-50, 15, 5), new Vector3(20, 0, -50), new Vector3(50, 0, 5), new Vector3(20, -15, 50) };
        private bool _useDemoWaypoints = false;
        private Vector3 _targetLookAt = new Vector3(0, 0, 0);

        private Matrix _projectionBuildSkyCubeMatrix;
        //private static Vector3 sbpPos;
        //private Matrix sbpWorld;

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

            _graphics.PreferredBackBufferWidth = 1200;
            _graphics.PreferredBackBufferHeight = 800;
            _graphics.ApplyChanges();

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

            SetupTheCameras();
            CreateRectanglePositionsAndAddThemToQuads();
            CreatePrimitiveSceneCubes();
            CreateSphericalArraysAndCubeMapTextures();
        }

        public void ClientResize(object sender, EventArgs e)
        {
            SetupTheCameras();
        }

        public void SetupTheCameras()
        {
            // a 90 degree field of view is needed for the projection matrix.
            var f90 = 90.0f * (3.14159265358f / 180f);
            //_projectionBuildSkyCubeMatrix = Matrix.CreatePerspectiveFieldOfView(f90, GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height, 0.01f, 1000f);

            //_cameraCinematic = new DemoCamera(GraphicsDevice, _spriteBatch, null, new Vector3(0, 0, 0), new Vector3(0, 0, 1), Vector3.Down, 0.01f, 10000f, f90, false, true, false);

            _cameraCinematic = new DemoCamera(GraphicsDevice, _spriteBatch, null, new Vector3(0, 0, 0), new Vector3(0, 0, 1), Vector3.Up, 0.01f, 10000f, f90, true, false, false);
            ////_cameraCinematic.TransformCamera(_cameraCinematic.World.Translation, _targetLookAt, _cameraCinematic.World.Up);
            ////_cameraCinematic.Up = Vector3.Up;
            _cameraCinematic.WayPointCycleDurationInTotalSeconds = 30f;
            _cameraCinematic.MovementSpeedPerSecond = 8f;
            _cameraCinematic.SetWayPoints(_cameraWayPoints, true, 30);
        }

        public void CreatePrimitiveSceneCubes()
        {
            skyCube = new PrimitiveCube(1000, true, false, true);
            for (int i = 0; i < 5; i++)
                cubes[i] = new PrimitiveCube(1, false, false, true);
        }

        public void CreateSphericalArraysAndCubeMapTextures()
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

        public void CreateRectanglePositionsAndAddThemToQuads()
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

            float depth = 0;
            if (_cameraCinematic.IsPerspectiveStyled)
                depth = 400f;

            scrQuads.AddVertexRectangleToBuffer(GraphicsDevice, r_sphericalTexture2DEnviromentalMap, depth, _cameraCinematic.IsPerspectiveStyled);
            scrQuads.AddVertexRectangleToBuffer(GraphicsDevice, r_face4_Left, depth, _cameraCinematic.IsPerspectiveStyled);
            scrQuads.AddVertexRectangleToBuffer(GraphicsDevice, r_face0_Forward, depth, _cameraCinematic.IsPerspectiveStyled);
            scrQuads.AddVertexRectangleToBuffer(GraphicsDevice, r_face5_Right, depth, _cameraCinematic.IsPerspectiveStyled);
            scrQuads.AddVertexRectangleToBuffer(GraphicsDevice, r_face1_Back, depth, _cameraCinematic.IsPerspectiveStyled);
            scrQuads.AddVertexRectangleToBuffer(GraphicsDevice, r_face3_Top, depth, _cameraCinematic.IsPerspectiveStyled);
            scrQuads.AddVertexRectangleToBuffer(GraphicsDevice, r_face2_Bottom, depth, _cameraCinematic.IsPerspectiveStyled);
            scrQuads.AddVertexRectangleToBuffer(GraphicsDevice, r_generatedSphericalTexture2DFromCube, depth, _cameraCinematic.IsPerspectiveStyled);
            scrQuads.AddVertexRectangleToBuffer(GraphicsDevice, r_generatedSphericalTexture2DFromFaceArray, depth, _cameraCinematic.IsPerspectiveStyled);
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
                _cameraCinematic.TransformCamera(new Vector3(0, 0, 0), _cameraCinematic.Forward, Vector3.Up);

            if (IsPressedWithDelay(Keys.F6, gameTime))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                CreateSphericalArraysAndCubeMapTextures();
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

            _cameraCinematic.Update(_targetLookAt, _useDemoWaypoints, gameTime);

            msg =
            $" Camera IsSpriteBatchStyled {_cameraCinematic.IsSpriteBatchStyled}" +
            $"\n Camera.View.Translation: \n  { _cameraCinematic.View.Translation.X.ToString("N3") } { _cameraCinematic.View.Translation.Y.ToString("N3") } { _cameraCinematic.View.Translation.Z.ToString("N3") }" +
            $"\n Camera.Forward: \n  { _cameraCinematic.Forward.X.ToString("N3") } { _cameraCinematic.Forward.Y.ToString("N3") } { _cameraCinematic.Forward.Z.ToString("N3") }" + 
            $"\n Up: \n { _cameraCinematic.Up.X.ToString("N3") } { _cameraCinematic.Up.Y.ToString("N3") } { _cameraCinematic.Up.Z.ToString("N3") } "+
            $"\n Cullmode \n {GraphicsDevice.RasterizerState.CullMode} \n MipLevelTestValue {_mipLevelTestValue} " +
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
            _drawingEffect.CurrentTechnique = _drawingEffect.Techniques["RenderCubeMap"];
            _drawingEffect.Parameters["View"].SetValue(_cameraCinematic.View);
            _drawingEffect.Parameters["testValue1"].SetValue((int)_mipLevelTestValue);

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            //GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            DrawPrimitiveSkyCube(gameTime);

            DrawPrimitiveSceneCubes(gameTime);

            PrimitiveDrawLoadedAndGeneratedTextures();
        }

        #region draw primitive scene camera geometry.

        private void DrawPrimitiveSkyCube(GameTime gameTime)
        {
            _drawingEffect.Parameters["Projection"].SetValue(_cameraCinematic.Projection);
            _drawingEffect.Parameters["View"].SetValue(_cameraCinematic.View);
            _drawingEffect.Parameters["CameraPosition"].SetValue(_cameraCinematic.Position); // _cameraCinematic.Position Vector3.Zero
            //_drawingEffect.Parameters["World"].SetValue(Matrix.CreateWorld(Vector3.Zero, _cameraCinematic.Forward, _cameraCinematic.Up));
            _drawingEffect.Parameters["World"].SetValue(Matrix.Identity);

            if (_whichCubeMapToDraw == 0 && _textureCubeEnviroment != null)
                skyCube.DrawPrimitiveCube(GraphicsDevice, _drawingEffect, _textureCubeEnviroment);
            if (_whichCubeMapToDraw == 1 && _textureCubeIblDiffuseIllumination != null)
                skyCube.DrawPrimitiveCube(GraphicsDevice, _drawingEffect, _textureCubeIblDiffuseIllumination);
            if (_whichCubeMapToDraw == 2 && _generatedTextureCubeFromFaceArray != null)
                skyCube.DrawPrimitiveCube(GraphicsDevice, _drawingEffect, _generatedTextureCubeFromFaceArray);
        }

        private void DrawPrimitiveSceneCubes(GameTime gameTime)
        {
            _drawingEffect.Parameters["Projection"].SetValue(_cameraCinematic.Projection);
            _drawingEffect.Parameters["CameraPosition"].SetValue(_cameraCinematic.Position);

            Matrix scaleMatrix = Matrix.Identity;
            if (_cameraCinematic.IsSpriteBatchStyled)
                    scaleMatrix = Matrix.CreateScale(1f);
                else
                    scaleMatrix = Matrix.CreateScale(10f);
            int i = 0;
            if (_textureCubeEnviroment != null)
            {
                _drawingEffect.Parameters["World"].SetValue(scaleMatrix * Matrix.CreateTranslation(GetScreenFaceDrawPositions(i, _cameraCinematic.IsSpriteBatchStyled)) );
                cubes[1].DrawPrimitiveCube(GraphicsDevice, _drawingEffect, _textureCubeEnviroment);
                i++;
            }
            if (_textureCubeIblDiffuseIllumination != null)
            {
                _drawingEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(GetScreenFaceDrawPositions(i, _cameraCinematic.IsSpriteBatchStyled)) * scaleMatrix);
                cubes[1].DrawPrimitiveCube(GraphicsDevice, _drawingEffect, _textureCubeIblDiffuseIllumination);
                i++;
            }
            if (_generatedTextureCubeFromFaceArray != null)
            {
                _drawingEffect.Parameters["World"].SetValue(scaleMatrix * Matrix.CreateTranslation(GetScreenFaceDrawPositions(i, _cameraCinematic.IsSpriteBatchStyled)) * scaleMatrix);
                cubes[1].DrawPrimitiveCube(GraphicsDevice, _drawingEffect, _generatedTextureCubeFromFaceArray);
                i++;
            }
        }

        public Vector3 GetScreenFaceDrawPositions(int i, bool spriteBatchPlacement)
        {
            if (spriteBatchPlacement)
                return new Vector3(i * 2 + 70, i * 2, i * -2.5f + -8);
            else
                return new Vector3(i * 2 + 70, i * 2, 0.0f);
        }

        #endregion

        #region draw primitive 2d map output.

        public void PrimitiveDrawLoadedAndGeneratedTextures()
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            _drawingEffect.CurrentTechnique = _drawingEffect.Techniques["QuadDraw"];
            _drawingEffect.Parameters["Projection"].SetValue(_cameraCinematic.Projection);
            _drawingEffect.Parameters["View"].SetValue(_cameraCinematic.View);
            _drawingEffect.Parameters["CameraPosition"].SetValue(_cameraCinematic.Position); // _camera.Position // Vector3.Zero

             _drawingEffect.Parameters["World"].SetValue(Matrix.Identity);


            

            //var m = _cameraCinematic.View;
            //m.Translation = new Vector3(0, 0, 0);
            //m.Forward = new Vector3(0, 0, 1);
            //m.Right = Vector3.Cross(m.Up, m.Forward);
            //m.Up = Vector3.Cross(m.Right, m.Forward);


            //_drawingEffect.CurrentTechnique = _drawingEffect.Techniques["QuadDraw"];
            //_drawingEffect.Parameters["Projection"].SetValue(_cameraCinematic.Projection);
            //var m = _cameraCinematic.View;
            //m.Right = new Vector3          (1, 0, 0);
            //m.Up = new Vector3             (0, -1, 0);
            //m.Forward = new Vector3      (0, 0, 1);
            //m.Translation = new Vector3 (-1f, +1f, 0);
            //_drawingEffect.Parameters["View"].SetValue(m);
            //_drawingEffect.Parameters["CameraPosition"].SetValue(Vector3.Zero); // _camera.Position // Vector3.Zero
            //_drawingEffect.Parameters["World"].SetValue(Matrix.Identity);

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

        #endregion

        #region spritebatch drawing

        public void DrawSpriteBatches(GameTime gameTime)
        {
            _spriteBatch.Begin();

            //SpriteBatchDrawLoadedAndGeneratedTextures();

            _cameraCinematic.DrawCurveThruWayPointsWithSpriteBatch(1.5f, new Vector3(GraphicsDevice.Viewport.Bounds.Right - 100, 1, GraphicsDevice.Viewport.Bounds.Bottom - 100), 1, gameTime);

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


        #endregion

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
