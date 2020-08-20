using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace DeferredLightingGL
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;

        private Camera _camera;
        private QuadRenderComponent _quadRenderer;
        private Scene _scene;

        private RenderTarget2D _colorRT; //color and specular intensity
        private RenderTarget2D _normalRT; //normals + specular power
        private RenderTarget2D _depthRT; //depth
        private RenderTarget2D _lightRT; //lighting

        private Effect _clearBufferEffect;
        private Effect _directionalLightEffect;
        private Effect _pointLightEffect;
        private Effect _renderGBufferEffect;

        private Model _sphereModel; //point light volume

        private Effect _finalCombineEffect;

        private Vector2 _halfPixel;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //graphics.IsFullScreen = true;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            _graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            _scene = new Scene(this);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _camera = new Camera(this);
            _camera.CameraArc = -30;
            _camera.CameraDistance = 50;
            _quadRenderer = new QuadRenderComponent(this);
            this.Components.Add(_camera);
            //this.Components.Add(quadRenderer);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _halfPixel = new Vector2()
            {
                X = 0.5f / (float)GraphicsDevice.PresentationParameters.BackBufferWidth,
                Y = 0.5f / (float)GraphicsDevice.PresentationParameters.BackBufferHeight
            };

            int backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

            _colorRT = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            _normalRT = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            _depthRT = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Single, DepthFormat.None);
            _lightRT = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);

            _scene.InitializeScene(_graphics.GraphicsDevice);

            _clearBufferEffect = LoadEffect("ClearGBuffer.mgfx", Content, GraphicsDevice);
            _directionalLightEffect = LoadEffect("DirectionalLight.mgfx", Content, GraphicsDevice); //this.Content.Load<Effect>("DirectionalLight");
            _finalCombineEffect = LoadEffect("CombineFinal.mgfx", Content, GraphicsDevice); //this.Content.Load<Effect>("CombineFinal");
            _pointLightEffect = LoadEffect("PointLight.mgfx", Content, GraphicsDevice); //this.Content.Load<Effect>("PointLight");
            _renderGBufferEffect = LoadEffect("RenderGBuffer.mgfx", Content, GraphicsDevice);//, out RenderHBufferEffectBytes); //this.Content.Load<Effect>("PointLight");
            _sphereModel = this.Content.Load<Model>(@"Data\Models\sphere");

            _scene.OverwriteEffects(_renderGBufferEffect);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            base.LoadContent();
        }

        public Effect LoadEffect(string effectFileName, ContentManager content, GraphicsDevice device)
        {
            string pathToEffects = $"Content{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}Effects{Path.DirectorySeparatorChar}";
            string effectPath = pathToEffects + effectFileName;
            var folderPath = effectPath.Substring(0, effectPath.LastIndexOf('/') + 1);
            var filePath = effectPath.Substring(effectPath.LastIndexOf('/') + 1);
            byte[] bytecode = GetFileBytes(folderPath, filePath);
            return new Effect(device, bytecode);
        }

        public byte[] GetFileBytes(string path, string fileName)
        {
            return File.ReadAllBytes(Path.Combine(path, fileName));
        }
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        //MG on Android does not support setting multiple render targets
        //private void SetGBuffer()
        //{
        //    GraphicsDevice.SetRenderTargets(colorRT, normalRT, depthRT);
        //}
        private void SetGBuffer()
        {
            GraphicsDevice.SetRenderTargets(_colorRT, _normalRT, _depthRT);
        }

        private void ResolveGBuffer()
        {
            GraphicsDevice.SetRenderTarget(null);
        }

        private void ClearGBuffer()
        {
            _clearBufferEffect.Techniques[0].Passes[0].Apply();
            _quadRenderer.Render(Vector2.One * -1, Vector2.One);
        }

        private void DrawDirectionalLight(Vector3 lightDirection, Color color)
        {
            _directionalLightEffect.Parameters["colorMap"].SetValue(_colorRT);
            _directionalLightEffect.Parameters["normalMap"].SetValue(_normalRT);
            _directionalLightEffect.Parameters["depthMap"].SetValue(_depthRT);

            _directionalLightEffect.Parameters["lightDirection"].SetValue(lightDirection);
            _directionalLightEffect.Parameters["Color"].SetValue(color.ToVector3());

            _directionalLightEffect.Parameters["cameraPosition"].SetValue(_camera.Position);
            _directionalLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(_camera.View * _camera.Projection));

            _directionalLightEffect.Parameters["halfPixel"].SetValue(_halfPixel);

            _directionalLightEffect.Techniques[0].Passes[0].Apply();
            _quadRenderer.Render(Vector2.One * -1, Vector2.One);
        }

        private void DrawPointLight(Vector3 lightPosition, Color color, float lightRadius, float lightIntensity)
        {
            //set the G-Buffer parameters
            _pointLightEffect.Parameters["colorMap"].SetValue(_colorRT);
            _pointLightEffect.Parameters["normalMap"].SetValue(_normalRT);
            _pointLightEffect.Parameters["depthMap"].SetValue(_depthRT);

            //compute the light world matrix
            //scale according to light radius, and translate it to light position
            Matrix sphereWorldMatrix = Matrix.CreateScale(lightRadius) * Matrix.CreateTranslation(lightPosition);
            _pointLightEffect.Parameters["World"].SetValue(sphereWorldMatrix);
            _pointLightEffect.Parameters["View"].SetValue(_camera.View);
            _pointLightEffect.Parameters["Projection"].SetValue(_camera.Projection);
            //light position
            _pointLightEffect.Parameters["lightPosition"].SetValue(lightPosition);

            //set the color, radius and Intensity
            _pointLightEffect.Parameters["Color"].SetValue(color.ToVector3());
            _pointLightEffect.Parameters["lightRadius"].SetValue(lightRadius);
            _pointLightEffect.Parameters["lightIntensity"].SetValue(lightIntensity);

            //parameters for specular computations
            _pointLightEffect.Parameters["cameraPosition"].SetValue(_camera.Position);
            _pointLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(_camera.View * _camera.Projection));
            //size of a halfpixel, for texture coordinates alignment
            _pointLightEffect.Parameters["halfPixel"].SetValue(_halfPixel);
            //calculate the distance between the camera and light center
            float cameraToCenter = Vector3.Distance(_camera.Position, lightPosition);
            //if we are inside the light volume, draw the sphere's inside face
            if (cameraToCenter < lightRadius)
                GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            else
                GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            _pointLightEffect.Techniques[0].Passes[0].Apply();
            foreach (ModelMesh mesh in _sphereModel.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    GraphicsDevice.Indices = meshPart.IndexBuffer;
                    GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);

                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }

            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            //SetGBuffer();
            //ClearGBuffer();
            //scene.DrawScene(camera, gameTime);
            //ResolveGBuffer();

            //GraphicsDevice.SetRenderTargets(colorRT, normalRT, depthRT);
            GraphicsDevice.SetRenderTargets(_colorRT);
            ClearGBuffer();
            _scene.DrawScene(_camera, gameTime, "Color");
            ResolveGBuffer();
            GraphicsDevice.SetRenderTargets(_normalRT);
            ClearGBuffer();
            _scene.DrawScene(_camera, gameTime, "Normal");
            ResolveGBuffer();
            GraphicsDevice.SetRenderTargets(_depthRT);
            ClearGBuffer();
            _scene.DrawScene(_camera, gameTime, "Depth");
            ResolveGBuffer();

            DrawLights(gameTime);

            base.Draw(gameTime);
        }

        private void DrawLights(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_lightRT);
            GraphicsDevice.Clear(Color.Transparent);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            Color[] colors = new Color[10];
            colors[0] = Color.Red; colors[1] = Color.Blue;
            colors[2] = Color.IndianRed; colors[3] = Color.CornflowerBlue;
            colors[4] = Color.Gold; colors[5] = Color.Green;
            colors[6] = Color.Crimson; colors[7] = Color.SkyBlue;
            colors[8] = Color.Red; colors[9] = Color.ForestGreen;
            float angle = (float)gameTime.TotalGameTime.TotalSeconds;
            int n = 15;

            for (int i = 0; i < n; i++)
            {
                Vector3 pos = new Vector3((float)Math.Sin(i * MathHelper.TwoPi / n + angle), 0.30f, (float)Math.Cos(i * MathHelper.TwoPi / n + angle));
                DrawPointLight(pos * 40, colors[i % 10], 15, 2);
                pos = new Vector3((float)Math.Cos((i + 5) * MathHelper.TwoPi / n - angle), 0.30f, (float)Math.Sin((i + 5) * MathHelper.TwoPi / n - angle));
                DrawPointLight(pos * 20, colors[i % 10], 20, 1);
                pos = new Vector3((float)Math.Cos(i * MathHelper.TwoPi / n + angle), 0.10f, (float)Math.Sin(i * MathHelper.TwoPi / n + angle));
                DrawPointLight(pos * 75, colors[i % 10], 45, 2);
                pos = new Vector3((float)Math.Cos(i * MathHelper.TwoPi / n + angle), -0.3f, (float)Math.Sin(i * MathHelper.TwoPi / n + angle));
                DrawPointLight(pos * 20, colors[i % 10], 20, 2);
            }

            DrawPointLight(new Vector3(0, (float)Math.Sin(angle * 0.8) * 40, 0), Color.Red, 30, 5);
            DrawPointLight(new Vector3(0, 25, 0), Color.White, 30, 1);
            DrawPointLight(new Vector3(0, 0, 70), Color.Wheat, 55 + 10 * (float)Math.Sin(5 * angle), 3);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            GraphicsDevice.SetRenderTarget(null);

            //Combine everything
            _finalCombineEffect.Parameters["colorMap"].SetValue(_colorRT);
            _finalCombineEffect.Parameters["lightMap"].SetValue(_lightRT);
            _finalCombineEffect.Parameters["halfPixel"].SetValue(_halfPixel);

            _finalCombineEffect.Techniques[0].Passes[0].Apply();
            _quadRenderer.Render(Vector2.One * -1, Vector2.One);

            //Output FPS and 'credits'
            double fps = (1000 / gameTime.ElapsedGameTime.TotalMilliseconds);
            fps = Math.Round(fps, 0);
            this.Window.Title = "Deferred Rendering by Catalin Zima, converted to XNA4 by Roy Triesscheijn. Drawing " + (n * 4 + 3) + " lights at " + fps.ToString() + " FPS";
        }
    }
}
