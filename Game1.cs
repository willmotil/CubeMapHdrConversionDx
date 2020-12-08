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
        float elapsed;
        float elapsedSecond;
        float elapsedTenSecond;

        private BasicEffect _basicEffect;
        private Effect _textureCubeBuildEffect;
        private Effect _drawingEffect;

        private Texture2D _sphericalTexture2DEnviromentalMap;
        private Texture2D _generatedSphericalTexture2DFromCube;
        private Texture2D _generatedSphericalTexture2DFromSphericalTexture2D;
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
        public Rectangle r_generatedSphericalTexture2DFromSphericalTexture2D;

        Primitive2dQuadBuffer scrQuads = new Primitive2dQuadBuffer();

        private string[] _currentCubeMapShown = new string[] { "_textureCubeEnviroment", "_textureCubeIblDiffuseIllumination", "_generatedTextureCubeFromFaceArray" };

        private PrimitiveCube skyCube;
        private PrimitiveCube[] cubes = new PrimitiveCube[5];

        private PrimitiveSphere skySphere;

        private float _mipLevelTestValue = 0;
        private int _whichCubeMapToDraw = 0; // enviromentalMapFromHdr =1,  enviromentalDiffuseIlluminationMap = 2, 
        private SurfaceFormat _initialLoadedFormat;

        private bool _wireframe = false;
        private RasterizerState rs_wireframe = new RasterizerState() { FillMode = FillMode.WireFrame };
        private RasterizerState rs_solid = new RasterizerState() { FillMode = FillMode.Solid };
        private RasterizerState rs_wireframe_cullnone = new RasterizerState() { FillMode = FillMode.WireFrame, CullMode = CullMode.None };
        private RasterizerState rs_solid_cullnone = new RasterizerState() { FillMode = FillMode.Solid, CullMode = CullMode.None };

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
            _sphericalTexture2DEnviromentalMap = Content.Load<Texture2D>("hdr_colorful_studio_2k");  // "hdr_colorful_studio_2k" "schadowplatz_2k" "hdr_royal_esplanade_2k"
            _initialLoadedFormat = _sphericalTexture2DEnviromentalMap.Format;

            SetupTheCameras();
            CreateRectanglePositionsAndAddThemToQuads();
            CreatePrimitiveSceneCubes();
            CreatePrimitiveSpheres();
            CreateSphericalArraysAndCubeMapTextures();
        }

        public void ClientResize(object sender, EventArgs e)
        {
            //SetupTheCameras();
        }

        public void SetupTheCameras()
        {
            // a 90 degree field of view is needed for the projection matrix.
            var f90 = 90.0f * (3.14159265358f / 180f);
            //_projectionBuildSkyCubeMatrix = Matrix.CreatePerspectiveFieldOfView(f90, GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height, 0.01f, 1000f);

            //_cameraCinematic = new DemoCamera(GraphicsDevice, _spriteBatch, null, new Vector3(0, 0, 0), new Vector3(0, 0, 1), Vector3.Down, 0.01f, 10000f, f90, false, true, false);

            _cameraCinematic = new DemoCamera(GraphicsDevice, _spriteBatch, null, new Vector3(0, 0, 0), Vector3.Forward, Vector3.Up, 0.01f, 10000f, f90, true, false, false);
            ////_cameraCinematic.TransformCamera(_cameraCinematic.World.Translation, _targetLookAt, _cameraCinematic.World.Up);
            ////_cameraCinematic.Up = Vector3.Up;
            _cameraCinematic.WayPointCycleDurationInTotalSeconds = 30f;
            _cameraCinematic.MovementSpeedPerSecond = 8f;
            _cameraCinematic.SetWayPoints(_cameraWayPoints, true, 30);

            Orthographic(GraphicsDevice);
        }

        public void Orthographic(GraphicsDevice device)
        {
            float forwardDepthDirection = 1f;
            _basicEffect = new BasicEffect(GraphicsDevice);
            _basicEffect.VertexColorEnabled = true;
            _basicEffect.TextureEnabled = true;
            _basicEffect.World = Matrix.Identity;
            _basicEffect.View = Matrix.Invert(Matrix.CreateWorld(new Vector3(0,0,0), new Vector3(0, 0, 1), Vector3.Down));
            _basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width, -device.Viewport.Height, 0, forwardDepthDirection * 0, forwardDepthDirection * 1f);
        }

        public void CreatePrimitiveSpheres()
        {
            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            skySphere = new PrimitiveSphere(5, 2, 1f, true, false, true);
            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++
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

                // spherical to spherical ...
                _generatedSphericalTexture2DFromSphericalTexture2D = TextureTypeConverter.ConvertSphericalTexture2DToSphericalTexture2D(GraphicsDevice, _textureCubeBuildEffect, _sphericalTexture2DEnviromentalMap, false, true, 256);

                // cubemap to...
                // to spherical map
                _generatedSphericalTexture2DFromCube = TextureTypeConverter.ConvertTextureCubeToSphericalTexture2D(GraphicsDevice, _textureCubeBuildEffect, _textureCubeEnviroment, false, true, 512);

                // texture cube  ...
                // to cube                                     CubemapToCubemap  DiffuseIlluminationCubeMap
                //_textureCubeIblDiffuseIllumination = TextureTypeConverter.ConvertTextureCubeToTextureCube(GraphicsDevice, _textureCubeBuildEffect, _textureCubeEnviroment, false, true, 512);
                _textureCubeIblDiffuseIllumination = TextureTypeConverter.ConvertTextureCubeToIrradianceMap(GraphicsDevice, _textureCubeBuildEffect, _textureCubeEnviroment, false, true, 512);
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
            r_generatedSphericalTexture2DFromSphericalTexture2D = new Rectangle(xoffset, 105, 200, 100);

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
            scrQuads.AddVertexRectangleToBuffer(GraphicsDevice, r_generatedSphericalTexture2DFromSphericalTexture2D, depth, _cameraCinematic.IsPerspectiveStyled);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            elapsedSecond += elapsed;
            if (elapsedSecond > 1f)
                elapsedSecond -= 1f;

            elapsedTenSecond += elapsed * .1f;
            if (elapsedTenSecond > 1f)
                elapsedTenSecond -= 1f;

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
                _cameraCinematic.TransformCamera(new Vector3(0, 0, 0), Vector3.Forward, Vector3.Up);

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
            $"\n Camera.World.Translation: \n  { _cameraCinematic.World.Translation.X.ToString("N3") } { _cameraCinematic.World.Translation.Y.ToString("N3") } { _cameraCinematic.World.Translation.Z.ToString("N3") }" +
            $"\n Camera.Forward: \n  { _cameraCinematic.Forward.X.ToString("N3") } { _cameraCinematic.Forward.Y.ToString("N3") } { _cameraCinematic.Forward.Z.ToString("N3") }" + 
            $"\n Up: \n { _cameraCinematic.Up.X.ToString("N3") } { _cameraCinematic.Up.Y.ToString("N3") } { _cameraCinematic.Up.Z.ToString("N3") } "+
            $"\n Camera IsSpriteBatchStyled {_cameraCinematic.IsSpriteBatchStyled}" +
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

            //DrawPrimitiveSkyCube(gameTime);

            //DrawPrimitiveSceneCubes(gameTime);

            //PrimitiveDrawLoadedAndGeneratedTextures();

            DrawPrimitiveSkySphere(gameTime);
        }

        #region draw primitive scene camera geometry.

        private void DrawPrimitiveSkySphere(GameTime gameTime)
        {
            _drawingEffect.CurrentTechnique = _drawingEffect.Techniques["RenderCubeMap"];
            _drawingEffect.Parameters["Projection"].SetValue(_cameraCinematic.Projection);
            _drawingEffect.Parameters["View"].SetValue(_cameraCinematic.View);
            _drawingEffect.Parameters["CameraPosition"].SetValue(_cameraCinematic.Position); // _cameraCinematic.Position Vector3.Zero
            _drawingEffect.Parameters["World"].SetValue(Matrix.Identity);

            if (_wireframe)
                GraphicsDevice.RasterizerState = rs_wireframe_cullnone;
            else
                GraphicsDevice.RasterizerState = rs_solid_cullnone;   //rs_solid;

            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            skySphere.DrawPrimitiveSphere(GraphicsDevice, _drawingEffect, _textureCubeEnviroment);
            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        }

        private void DrawPrimitiveSkyCube(GameTime gameTime)
        {
            _drawingEffect.Parameters["Projection"].SetValue(_cameraCinematic.Projection);
            _drawingEffect.Parameters["View"].SetValue(_cameraCinematic.View);
            _drawingEffect.Parameters["CameraPosition"].SetValue(_cameraCinematic.Position); // _cameraCinematic.Position Vector3.Zero
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

            float scale = 3f;
            Matrix scaleMatrix = Matrix.Identity;
            if (_cameraCinematic.IsSpriteBatchStyled)
                    scaleMatrix = Matrix.CreateScale(scale);
                else
                    scaleMatrix = Matrix.CreateScale(scale);
            int i = 0;
            if (_textureCubeIblDiffuseIllumination != null)
            {
                _drawingEffect.Parameters["World"].SetValue(scaleMatrix * Matrix.CreateTranslation(GetScreenFaceDrawPositions(i, scale, _cameraCinematic.IsSpriteBatchStyled)));
                cubes[1].DrawPrimitiveCube(GraphicsDevice, _drawingEffect, _textureCubeIblDiffuseIllumination);
                i++;
            }

            if (_textureCubeEnviroment != null)
            {
                _drawingEffect.Parameters["World"].SetValue(scaleMatrix * Matrix.CreateTranslation(GetScreenFaceDrawPositions(i, scale, _cameraCinematic.IsSpriteBatchStyled)) );
                cubes[1].DrawPrimitiveCube(GraphicsDevice, _drawingEffect, _textureCubeEnviroment);
                i++;
            }
            
            if (_generatedTextureCubeFromFaceArray != null)
            {
                _drawingEffect.Parameters["World"].SetValue(scaleMatrix * Matrix.CreateTranslation(GetScreenFaceDrawPositions(i, scale, _cameraCinematic.IsSpriteBatchStyled)));
                cubes[1].DrawPrimitiveCube(GraphicsDevice, _drawingEffect, _generatedTextureCubeFromFaceArray);
                i++;
            }
        }

        public Vector3 GetScreenFaceDrawPositions(int i, float scale, bool spriteBatchPlacement)
        {
            int x = i;
            int y = 0;
            int z = i;
            if (spriteBatchPlacement)
                return new Vector3(x * 3 + 10, y * 2, 0.0f) * scale;
            else
                return new Vector3(x * 3 + 10, y * 2, z * -2.5f + -8) * scale;
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

            xoffset += 220;
            if (_generatedSphericalTexture2DFromFaceArray != null)
            {
                _drawingEffect.Parameters["TextureA"].SetValue(_generatedSphericalTexture2DFromSphericalTexture2D);
                scrQuads.DrawQuadRangeInBuffer(GraphicsDevice, _drawingEffect, 9, 1);
            }
        }

        #endregion

        #region spritebatch drawing

        public void DrawSpriteBatches(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, _basicEffect, null);

            SpriteBatchDrawLoadedAndGeneratedTextures();

            SpriteBatchDrawVectorRotations(new Vector3(50, 120, 0) , gameTime);

            _cameraCinematic.DrawCurveThruWayPointsWithSpriteBatch(1.5f, new Vector3(GraphicsDevice.Viewport.Bounds.Right - 100, 1, GraphicsDevice.Viewport.Bounds.Bottom - 100), 1, gameTime);

            _spriteBatch.DrawString(_font, msg, new Vector2(10, 210), Color.Moccasin);

            _spriteBatch.End();
        }

        //https://automaticaddison.com/how-to-convert-a-quaternion-to-a-rotation-matrix/
        //https://personal.utdallas.edu/~sxb027100/dock/quaternion.html


        public void SpriteBatchDrawVectorRotations(Vector3 offset, GameTime gameTime)
        {
            var radiansX = elapsedTenSecond * 3.14159265358f * 2f;
            var rotMatx = Matrix.CreateRotationX(radiansX);
            var normal = Vector3.Normalize(new Vector3(1f, .3f, 1f));
            normal = Vector3.Transform(normal, rotMatx);
            normal.Normalize();

            DrawHelpers.DrawCrossHair(offset.ToVector2(), 10, Color.White);

            var vectorPositionForward = normal * 100;

            var vectorPositionUp = new Vector3(vectorPositionForward.Z, vectorPositionForward.Y, vectorPositionForward.X) + offset;
            var vectorPositionRight = new Vector3(vectorPositionForward.Y, vectorPositionForward.X, vectorPositionForward.Z) + offset;
            
            vectorPositionForward += offset;
            DrawHelpers.DrawBasicLine(offset.ToVector2(), vectorPositionForward.ToVector2(), 1, Color.Green);

            DrawHelpers.DrawBasicLine(offset.ToVector2(), vectorPositionUp.ToVector2(), 1, Color.Red);
            DrawHelpers.DrawBasicLine(offset.ToVector2(), vectorPositionRight.ToVector2(), 1, Color.Blue);

            var yrot = Matrix.CreateRotationX(.4f);
            var normal2 = Vector3.Transform(normal, yrot);
            var vectorPosition2 = normal2 * 100;
            vectorPosition2 += offset;
            DrawHelpers.DrawCrossHair(offset.ToVector2(), 10, Color.Yellow);
            DrawHelpers.DrawBasicLine(offset.ToVector2(), vectorPosition2.ToVector2(), 1, Color.Aqua);

            var radiansZ = elapsedSecond * 3.14159265358f * 2f;
            var aarot = Matrix.CreateFromAxisAngle(normal, radiansZ);
            var normal3 = Vector3.Transform(normal2, aarot);
            var vectorPosition3 = normal3 * 100;
            vectorPosition3 += offset;
            DrawHelpers.DrawCrossHair(offset.ToVector2(), 10, Color.Red);
            DrawHelpers.DrawBasicLine(offset.ToVector2(), vectorPosition3.ToVector2(), 1, Color.White);
        }

        public void SpriteBatchDrawLoadedAndGeneratedTextures()
        {
            int xoffset = 0;
            int xoffsetText = 10;
            int yoffsetText = 50;
            Color textColor = Color.White;
            if (_sphericalTexture2DEnviromentalMap != null)
            {
                _spriteBatch.Draw(_sphericalTexture2DEnviromentalMap, new Rectangle(xoffset, 0, 200, 100), Color.White);
                _spriteBatch.DrawString(_font, $"Loaded Spherical2D \n format {_initialLoadedFormat}", new Vector2(xoffsetText + xoffset, yoffsetText + 10), textColor);
            }

            xoffset += 120;
            if (_generatedTextureFaceArray != null)
            {
                _spriteBatch.Draw(_generatedTextureFaceArray[4], new Rectangle(xoffset + 0, 105, 95, 95), Color.White);
                _spriteBatch.Draw(_generatedTextureFaceArray[0], new Rectangle(xoffset + 100, 105, 95, 95), Color.White);
                _spriteBatch.Draw(_generatedTextureFaceArray[5], new Rectangle(xoffset + 200, 105, 95, 95), Color.White);
                _spriteBatch.Draw(_generatedTextureFaceArray[1], new Rectangle(xoffset + 300, 105, 95, 95), Color.White);
                _spriteBatch.Draw(_generatedTextureFaceArray[2], new Rectangle(xoffset + 100, 5, 95, 95), Color.White);
                _spriteBatch.Draw(_generatedTextureFaceArray[3], new Rectangle(xoffset + 100, 205, 95, 95), Color.White);

                _spriteBatch.DrawString(_font, "Left", new Vector2(xoffsetText + xoffset + 0, yoffsetText + 100), textColor);
                _spriteBatch.DrawString(_font, "Forward", new Vector2(xoffsetText + xoffset + 100, yoffsetText + 110), textColor);
                _spriteBatch.DrawString(_font, "Right", new Vector2(xoffsetText + xoffset + 200, yoffsetText + 110), textColor);
                _spriteBatch.DrawString(_font, "Back", new Vector2(xoffsetText + xoffset + 300, yoffsetText + 110), textColor);
                _spriteBatch.DrawString(_font, "Top", new Vector2(xoffsetText + xoffset + 100, yoffsetText + 10), textColor);
                _spriteBatch.DrawString(_font, "Bottom", new Vector2(xoffsetText + xoffset + 100, yoffsetText + 210), textColor);
            }

            xoffset += 220;
            if (_generatedSphericalTexture2DFromCube != null)
            {
                _spriteBatch.Draw(_generatedSphericalTexture2DFromCube, new Rectangle(xoffset, 0, 200, 100), Color.White);
                _spriteBatch.DrawString(_font, "Spherical2D \nFromCube", new Vector2(xoffsetText + xoffset + 0, yoffsetText + 10), textColor);
            }

            xoffset += 220;
            if (_generatedSphericalTexture2DFromFaceArray != null)
            {
                _spriteBatch.Draw(_generatedSphericalTexture2DFromFaceArray, new Rectangle(xoffset, 0, 200, 100), Color.White);
                _spriteBatch.DrawString(_font, "Spherical2D \nFromFaceArray", new Vector2(xoffsetText + xoffset, yoffsetText + 10), textColor);
            }

            if (_generatedSphericalTexture2DFromFaceArray != null)
            {
                _spriteBatch.Draw(_generatedSphericalTexture2DFromSphericalTexture2D, new Rectangle(xoffset, 105, 200, 100), Color.White);
                _spriteBatch.DrawString(_font, "Spherical2D \nFromSpherical2D", new Vector2(xoffsetText + xoffset, yoffsetText + 110), textColor);
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
