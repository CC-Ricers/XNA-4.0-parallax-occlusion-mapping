/*
 * Parallax Occlusion Mapping demo
 * 
 * Alex Urbano Álvarez
 * goefuika@gmail.com
 * 
 * http://elgoe.blogspot.com
 *
 *
 * XNA 4.0 port by Chris Cajas
 * ccricers@gmail.com
 *
 */

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace ParallaxOcclusionMapping
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class POMDemo : Microsoft.Xna.Framework.Game
    {
        #region Properties

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Model sceneModel;

        FreeCamera camera;
        FPSCounter fpsCounter;

        Vector3 LightPosition = new Vector3(0.0f, 28.0f, 6.0f);

        float _linAtt;
        float _quadAtt;
        Random randomizer = new Random();

        ParticleSystem fireParticles;
        ParticleSystem smokePlumeParticles;
        SparkParticleSystem sparks;

        private enum Scene
        {
            TextureMapping = 0,
            NormalMapping = 1,
            ParallaxOcclusionMapping = 2
         }

        Scene currentScene;
        KeyboardState previousState;

        const int fireParticlesPerFrame = 10;

        SpriteFont Font;
        Vector2 FontPosition;

        bool _showParticles;

        #endregion

        #region Constructor

        public POMDemo()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferMultiSampling = false;
            graphics.SynchronizeWithVerticalRetrace = true;
			graphics.PreferMultiSampling = true;
			graphics.PreferredBackBufferWidth = 1280;
			graphics.PreferredBackBufferHeight = 720;

            this.IsFixedTimeStep = true;

            currentScene = Scene.TextureMapping;
            _linAtt = 0.0f;

            fireParticles = new FireParticleSystem(this, Content);
            Components.Add(fireParticles);
            sparks = new SparkParticleSystem(this, Content);
            Components.Add(sparks);
            smokePlumeParticles = new SmokePlumeParticleSystem(this, Content);
            Components.Add(smokePlumeParticles);
            
            smokePlumeParticles.DrawOrder = 100;
            fireParticles.DrawOrder = 500;
            sparks.DrawOrder = 500;

            _showParticles = false;
        }

        #endregion

        #region Initialize
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            camera = new FreeCamera(new Vector3(0.0f, -20.0f, -120.0f));
            camera.FarPlane = 5000f;
            camera.NearPlane = 1.0f;
            camera.Speed = 75;
            camera.TurnSpeed = 10;
            camera.Angle = new Vector3(-0.2f, 3.13f, 0.0f);

            fpsCounter = new FPSCounter(this, "Parallax Occlusion Mapping Demo");
            Components.Add(fpsCounter);

            base.Initialize();
        }

        #endregion

        #region Load
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            sceneModel = Content.Load<Model>("Models/Kupol/kupol");
            Effect myEffect = Content.Load<Effect>("Effects/Textured");

            Font = Content.Load<SpriteFont>("Fuente");
            FontPosition = new Vector2(630, 40);
        }

        #endregion

        #region Unload
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        #endregion

        #region Update
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Escape))
                this.Exit();
            if (keyState.IsKeyDown(Keys.Space) && !previousState.IsKeyDown(Keys.Space))
            {
                currentScene++;
                if ((int)currentScene == 3)
                    currentScene = Scene.TextureMapping;
            }

            if (keyState.IsKeyDown(Keys.Enter) && !previousState.IsKeyDown(Keys.Enter))
            {
                if (_showParticles)
                {
                    Components.Remove(fireParticles);
                    Components.Remove(sparks);
                    Components.Remove(smokePlumeParticles);
                }
                else
                {
                    Components.Add(fireParticles);
                    Components.Add(sparks);
                    Components.Add(smokePlumeParticles);
                }
                _showParticles = !_showParticles;
            }

            _linAtt = 0.002f;
            if (_showParticles)
                _linAtt = 0.002f *(float)randomizer.NextDouble();
            _quadAtt = 0.00015f;// *((float)randomizer.NextDouble() + 0.8f);

            camera.Update(this.graphics.GraphicsDevice.Viewport.Width, (float)gameTime.ElapsedGameTime.TotalSeconds);

            if (_showParticles)
                UpdateFire();

            base.Update(gameTime);

            previousState = keyState;
        }

        void UpdateFire()
        {
            for (int i = 0; i < fireParticlesPerFrame; i++)
            {
                fireParticles.AddParticle(new Vector3(0.0f, 20.0f, 6.0f), Vector3.Zero);
            }

            smokePlumeParticles.AddParticle(new Vector3(0.0f, 20.0f, 6.0f), Vector3.Zero);
            sparks.AddParticle(new Vector3(0.0f, 20.0f, 6.0f), Vector3.Zero);
        }

        #endregion

        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            #region Draw model
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
			graphics.GraphicsDevice.BlendState = BlendState.Opaque;
			graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Matrix[] boneTransforms = new Matrix[this.sceneModel.Bones.Count];
            this.sceneModel.CopyAbsoluteBoneTransformsTo(boneTransforms);
          
            Vector3 Position = camera.Position;
            Matrix View = camera.View;
            Matrix Projection = camera.Projection;

            foreach (ModelMesh mesh in this.sceneModel.Meshes)
            {
                Matrix World = boneTransforms[mesh.ParentBone.Index] * 
                    Matrix.CreateScale(0.13f);

                foreach (Effect effect in mesh.Effects)
                {
                    switch (currentScene)
                    {
                        case Scene.TextureMapping:
                            effect.CurrentTechnique = effect.Techniques["Textured"];
                            break;
                        case Scene.NormalMapping:
                            effect.CurrentTechnique = effect.Techniques["NormalMapping"];
                            break;
                        case Scene.ParallaxOcclusionMapping:
                            effect.CurrentTechnique = effect.Techniques["POM"];
                            break;
                    }

                    effect.Parameters["World"].SetValue(World);
                    effect.Parameters["WorldViewProjection"].SetValue(World * View * Projection);
                    effect.Parameters["LightPosition"].SetValue(LightPosition);
                    effect.Parameters["CameraPos"].SetValue(camera.Position);
                    effect.Parameters["g_materialAmbientColor"].SetValue(new Vector4(.05f, .05f, .05f, 0));
                    effect.Parameters["g_materialDiffuseColor"].SetValue(new Vector4(.95f, .95f, .95f, 1));
                    effect.Parameters["g_materialSpecularColor"].SetValue(new Vector4(1, 1, 1, 1));
                    effect.Parameters["g_fSpecularExponent"].SetValue(100.0f);
                    effect.Parameters["g_bAddSpecular"].SetValue(false);
                    effect.Parameters["g_LightDiffuse"].SetValue(new Vector4(1.0f, 0.7f, 0.5f, 1));
                    effect.Parameters["g_fHeightMapScale"].SetValue(0.06f);
                    effect.Parameters["g_nMinSamples"].SetValue(10);
                    effect.Parameters["g_nMaxSamples"].SetValue(100);
                    effect.Parameters["linearAttenuation"].SetValue(_linAtt);
                    effect.Parameters["quadraticAttenuation"].SetValue(_quadAtt);
                }
                mesh.Draw();
            }
            #endregion
			/*
            // Update particles matrices
            if (_showParticles)
            {
                fireParticles.SetCamera(camera.View, camera.Projection);
                smokePlumeParticles.SetCamera(camera.View, camera.Projection);
                sparks.SetCamera(camera.View, camera.Projection);
            }
			*/
            // Draw components (particles and framerate counter)
            base.Draw(gameTime);

            // Draw scene mode
			this.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            Vector2 FontOrigin = Font.MeasureString(currentScene.ToString()) / 2;
            spriteBatch.DrawString(Font, currentScene.ToString(), FontPosition, Color.White,
                0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.End();

        }

        #endregion


    }
}
