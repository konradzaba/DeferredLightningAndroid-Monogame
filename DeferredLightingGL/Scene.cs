using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeferredLightingGL
{
    public class Scene
    {
        private Game game;
        Model[] models;

        private Texture2D groundDiffuse, groundSpecular, groundNormal;
        private Texture2D ship1Diffuse, ship1Specular, ship1Normal;
        private Texture2D ship2Diffuse, ship2Specular, ship2Normal;
        private Texture2D lizardDiffuse, lizardSpecular, lizardNormal;
        private Texture2D crateDiffuse, crateSpecular, crateNormal;

        private List<Texture2D> _texSamples = new List<Texture2D>();

        public Scene(Game game)
        {
            this.game = game;
        }
        public void InitializeScene(GraphicsDevice device)
        {
            models = new Model[5];
            models[0] = game.Content.Load<Model>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}ship1");
            models[1] = game.Content.Load<Model>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}ship2");
            models[2] = game.Content.Load<Model>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}lizard");
            models[3] = game.Content.Load<Model>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}Ground");
            models[4] = game.Content.Load<Model>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}crate");

            groundDiffuse = game.Content.Load<Texture2D>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}ground_diffuse");
            groundSpecular = game.Content.Load<Texture2D>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}ground_specular");
            groundNormal = game.Content.Load<Texture2D>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}ground_normal");

            ship1Diffuse = game.Content.Load<Texture2D>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}ship1_c");
            ship1Specular = game.Content.Load<Texture2D>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}ship1_s");
            ship1Normal = game.Content.Load<Texture2D>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}ship1_n");

            ship2Diffuse = game.Content.Load<Texture2D>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}ship2_c");
            ship2Specular = game.Content.Load<Texture2D>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}ship2_s");
            ship2Normal = game.Content.Load<Texture2D>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}ship2_n");

            lizardDiffuse = game.Content.Load<Texture2D>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}lizard_diff");
            lizardSpecular = game.Content.Load<Texture2D>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}lizard_geo_s");
            lizardNormal = game.Content.Load<Texture2D>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}lizard_norm");

            crateDiffuse = game.Content.Load<Texture2D>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}BOX01");
            crateSpecular = game.Content.Load<Texture2D>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}BOX01_SPEC");
            crateNormal = game.Content.Load<Texture2D>($"Data{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}BOX01_NORM");
        }

        public void OverwriteEffects(Effect effect)
        {
            foreach (var newModel in models)
                foreach (ModelMesh mesh in newModel.Meshes)
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                        meshPart.Effect = effect.Clone();
        }

        public void DrawScene(Camera camera, GameTime gameTime, string technique)
        {
            game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            game.GraphicsDevice.BlendState = BlendState.Opaque;


            DrawModel(models[3], Matrix.CreateTranslation(0, -10, 0), camera, technique, groundDiffuse, groundSpecular, groundNormal);
            DrawModel(models[4], Matrix.CreateScale(10f) * Matrix.CreateTranslation(30, 0, -20), camera, technique, crateDiffuse, crateSpecular, crateNormal);
            DrawModel(models[0], Matrix.CreateTranslation(-30, 0, -20), camera, technique, ship1Diffuse, ship1Specular, ship1Normal);
            DrawModel(models[1], Matrix.CreateTranslation(30, 0, -20), camera, technique, ship2Diffuse, ship2Specular, ship2Normal);
            DrawModel(models[2], Matrix.CreateScale(0.05f) * Matrix.CreateTranslation(0, 0, 27), camera, technique, lizardDiffuse, lizardSpecular, lizardNormal);

        }
        private void DrawModel(Model model, Matrix world, Camera camera, string technique, Texture2D diffuse, Texture2D specular, Texture2D normal)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques[technique];

                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["View"].SetValue(camera.View);
                    effect.Parameters["Projection"].SetValue(camera.Projection);
                    effect.Parameters["Texture"].SetValue(diffuse);
                    effect.Parameters["SpecularMap"].SetValue(specular);
                    effect.Parameters["NormalMap"].SetValue(normal);
                }
                mesh.Draw();
            }
        }
    }
}