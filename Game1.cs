using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


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
        private Effect _cubeDrawEffect;

        private Texture2D _sphericalTexture2DEnviromentalMap;
        private Texture2D _generatedSphericalTexture2DFromCube;
        private Texture2D[] _generatedTextureFaceArray;
        private Texture2D _generatedSphericalTexture2DFromFaceArray;
        private TextureCube _textureCubeEnviroment;
        private TextureCube _textureCubeIblDiffuseIllumination;
        private TextureCube _textureCubeIblSpecularPrefilter;
        private TextureCube _generatedTextureCubeFromFaceArray;

        private string[] _currentCubeMapShown = new string[] { "_textureCubeEnviroment", "_textureCubeIblDiffuseIllumination", "_generatedTextureCubeFromFaceArray" };

        private Matrix _projectionBuildSkyCubeMatrix;

        private PrimitiveCube skyCube = new PrimitiveCube(100, false, false, true);
        private PrimitiveCube[] cubes = new PrimitiveCube[5];

        private float _mipLevelTestValue = 0;
        private int _whichCubeMapToDraw = 0; // enviromentalMapFromHdr =1,  enviromentalDiffuseIlluminationMap = 2, 
        private SurfaceFormat _initialLoadedFormat;

        private bool _wireframe = false;
        private RasterizerState rs_wireframe = new RasterizerState() { FillMode = FillMode.WireFrame };

        private DemoCamera _camera;
        private Vector3[] _cameraWayPoints = new Vector3[] { new Vector3(-50, 15, 5), new Vector3(20, 0, -50), new Vector3(50, 0, 5), new Vector3(20, -15, 50) };
        private bool _useDemoWaypoints = true;
        private Vector3 _targetLookAt = new Vector3(0, 0, 0);

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
            this.Window.Title = "SphereCube Image Hdr Mapping Effect Tests";
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
            _cubeDrawEffect = Content.Load<Effect>("TextureCubeDrawEffect");

            // Hdr's TextureFormat option in the content pipeline editor needs to be set to none instead of color.
            _sphericalTexture2DEnviromentalMap = Content.Load<Texture2D>("hdr_colorful_studio_2k");  
            _initialLoadedFormat = _sphericalTexture2DEnviromentalMap.Format;

            LoadPrimitives();
            CreateIblCubeMaps();
            SetupCamera();
        }

        public void ClientResize(object sender, EventArgs e)
        {
            _projectionBuildSkyCubeMatrix = Matrix.CreatePerspectiveFieldOfView(90 * (float)((3.14159265358f) / 180f), GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height, 0.01f, 1000f);
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
                _textureCubeEnviroment = TextureTypeConverter.ConvertSphericalTexture2DToTextureCube(GraphicsDevice, _textureCubeBuildEffect, _sphericalTexture2DEnviromentalMap, true, true, 512);             
                _textureCubeIblDiffuseIllumination = TextureTypeConverter.ConvertTextureCubeToTextureCube(GraphicsDevice, _textureCubeBuildEffect, _textureCubeEnviroment, false, true, 512);
                _generatedSphericalTexture2DFromCube = TextureTypeConverter.ConvertTextureCubeToSphericalTexture2D(GraphicsDevice, _textureCubeBuildEffect, _textureCubeEnviroment, false, true, 512);
                _generatedTextureFaceArray = TextureTypeConverter.ConvertTextureCubeToTexture2DArray(GraphicsDevice, _textureCubeBuildEffect, _textureCubeEnviroment, false, true, 256);
                _generatedSphericalTexture2DFromFaceArray = TextureTypeConverter.ConvertTexture2DArrayToSphericalTexture2D(GraphicsDevice, _textureCubeBuildEffect, _generatedTextureFaceArray, false, true, 256);
                _generatedTextureCubeFromFaceArray = TextureTypeConverter.ConvertTexture2DArrayToTextureCube(GraphicsDevice, _textureCubeBuildEffect, _generatedTextureFaceArray, false, true, 256);
            }
        }

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
            _cubeDrawEffect.Parameters["View"].SetValue(_camera.View);
            _cubeDrawEffect.Parameters["testValue1"].SetValue((int)_mipLevelTestValue);

            DrawPrimitiveSkyCube(gameTime);

            DrawPrimitiveSceneCubes(gameTime);
        }

        private void DrawPrimitiveSkyCube(GameTime gameTime)
        {
            _cubeDrawEffect.Parameters["Projection"].SetValue(_projectionBuildSkyCubeMatrix);
            _cubeDrawEffect.Parameters["CameraPosition"].SetValue(_camera.Position);
            _cubeDrawEffect.Parameters["World"].SetValue(_camera.World);

            if (_whichCubeMapToDraw == 0 && _textureCubeEnviroment != null)
                skyCube.DrawPrimitiveCube(GraphicsDevice, _cubeDrawEffect, _textureCubeEnviroment);
            if (_whichCubeMapToDraw == 1 && _textureCubeIblDiffuseIllumination != null)
                skyCube.DrawPrimitiveCube(GraphicsDevice, _cubeDrawEffect, _textureCubeIblDiffuseIllumination);
            if (_whichCubeMapToDraw == 2 && _generatedTextureCubeFromFaceArray != null)
                skyCube.DrawPrimitiveCube(GraphicsDevice, _cubeDrawEffect, _generatedTextureCubeFromFaceArray);
        }

        private void DrawPrimitiveSceneCubes(GameTime gameTime)
        {
            _cubeDrawEffect.Parameters["Projection"].SetValue(_camera.Projection);
            _cubeDrawEffect.Parameters["CameraPosition"].SetValue(_camera.Position);

            int i = 0;
            if (_textureCubeEnviroment != null)
            {
                _cubeDrawEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(i * 2 + 70, i * 2, i * -2.5f + -8)));
                cubes[1].DrawPrimitiveCube(GraphicsDevice, _cubeDrawEffect, _textureCubeEnviroment);
                i++;
            }
            if (_textureCubeIblDiffuseIllumination != null)
            {
                _cubeDrawEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(i * 2 + 70, i * 2, i * -2.5f + -8)));
                cubes[1].DrawPrimitiveCube(GraphicsDevice, _cubeDrawEffect, _textureCubeIblDiffuseIllumination);
                i++;
            }
            if (_generatedTextureCubeFromFaceArray != null)
            {
                _cubeDrawEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(i * 2 + 70, i * 2, i * -2.5f + -8)));
                cubes[1].DrawPrimitiveCube(GraphicsDevice, _cubeDrawEffect, _generatedTextureCubeFromFaceArray);
                i++;
            }

        }

        public void DrawSpriteBatches(GameTime gameTime)
        {
            _spriteBatch.Begin();

            DrawLoadedAndGeneratedTextures();

            _camera.DrawCurveThruWayPointsWithSpriteBatch(1.5f, new Vector3(GraphicsDevice.Viewport.Bounds.Right -100, 1, GraphicsDevice.Viewport.Bounds.Bottom - 100), 1, gameTime);

            _spriteBatch.DrawString(_font, msg, new Vector2(10, 210), Color.Moccasin);

            _spriteBatch.End();
        }

        public void DrawLoadedAndGeneratedTextures()
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
