using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredLightingAndroid
{
    public partial class QuadRenderComponent : DrawableGameComponent
    {
        VertexPositionTexture[] verts = null;

        public QuadRenderComponent(Game game)
            : base(game)
        {
            IGraphicsDeviceService graphicsService =
    (IGraphicsDeviceService)base.Game.Services.GetService(
                                typeof(IGraphicsDeviceService));

            verts = new VertexPositionTexture[]
                    {
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
                                new Vector2(1,1)),
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
                                new Vector2(0,1)),
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
                                new Vector2(0,0)),
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
                                new Vector2(0,0)),
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
                                new Vector2(1,0)),
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
                                new Vector2(1,1)),
                    };
        }

        protected override void LoadContent()
        {
        }

        public void Render(Vector2 v1, Vector2 v2)
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)
                base.Game.Services.GetService(typeof(IGraphicsDeviceService));

            GraphicsDevice device = graphicsService.GraphicsDevice;

            verts[0].Position.X = v2.X;
            verts[0].Position.Y = v1.Y;

            verts[1].Position.X = v1.X;
            verts[1].Position.Y = v1.Y;

            verts[2].Position.X = v1.X;
            verts[2].Position.Y = v2.Y;

            verts[3].Position.X = v1.X;
            verts[3].Position.Y = v2.Y;

            verts[4].Position.X = v2.X;
            verts[4].Position.Y = v2.Y;

            verts[5].Position.X = v2.X;
            verts[5].Position.Y = v1.Y;

            device.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, 2);
        }
    }
}
