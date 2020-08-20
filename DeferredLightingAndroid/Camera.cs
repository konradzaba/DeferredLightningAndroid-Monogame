using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DeferredLightingAndroid
{
    public class Camera : Microsoft.Xna.Framework.GameComponent
    {
        public float CameraArc { get; set; } = -30;

        public float CameraRotation { get; set; } = 0;

        public float CameraDistance { get; set; } = 1000;

        public Vector3 Position { get; private set; }
        public float NearPlaneDistance { get; set; } = 1;
        public float FarPlaneDistance { get; set; } = 3000;


        public Matrix View { get; private set; }

        public Matrix Projection { get; private set; }

        KeyboardState _currentKeyboardState = new KeyboardState();
        GamePadState _currentGamePadState = new GamePadState();

        public Camera(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            _currentKeyboardState = Keyboard.GetState();
            _currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // TODO: Add your update code here

            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            CameraRotation += time * 0.02f;

            View = Matrix.CreateTranslation(0, -10, 0) *
                      Matrix.CreateRotationY(MathHelper.ToRadians(CameraRotation)) *
                      Matrix.CreateRotationX(MathHelper.ToRadians(CameraArc)) *
                      Matrix.CreateLookAt(new Vector3(0, 0, -CameraDistance),
                                          new Vector3(0, 0, 0), Vector3.Up);

            Position = Vector3.Transform(Vector3.Zero, Matrix.Invert(View));

            float aspectRatio = (float)Game.Window.ClientBounds.Width /
                                (float)Game.Window.ClientBounds.Height;
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    aspectRatio,
                                                                    NearPlaneDistance,
                                                                    FarPlaneDistance);
            base.Update(gameTime);
        }
    }
}