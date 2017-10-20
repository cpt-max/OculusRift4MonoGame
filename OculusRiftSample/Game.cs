using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace OculusRiftSample
{
    public class TheGame : Game
    {
		GraphicsDeviceManager gdm;

        OculusRift rift = new OculusRift();
        RenderTarget2D[] renderTargetEye = new RenderTarget2D[2];

        ManyCubes manyCubes;     
        SpriteBatch spriteBatch;

        const float PlayerSize = 1;
        Matrix playerMatrix = Matrix.CreateScale(PlayerSize);

        public TheGame()
        {
			gdm = new GraphicsDeviceManager(this);
			gdm.PreferredBackBufferWidth = 800;
			gdm.PreferredBackBufferHeight = 600;
            IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            // initialize the Rift
            int result = rift.Init(GraphicsDevice);
            if(result != 0)
                throw new InvalidOperationException("rift.Init result: " + result);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // create one rendertarget for each eye
            for (int eye = 0; eye < 2; eye++)
                renderTargetEye[eye] = rift.CreateRenderTargetForEye(eye);

            manyCubes = new ManyCubes(GraphicsDevice);
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // update head tracking
            rift.TrackHead();

            DoPlayerMotion(); 
        }

        protected override void Draw(GameTime gameTime)
        {
            // draw scene for both eyes into respective rendertarget
            for (int eye = 0; eye < 2; eye++)
            {
                GraphicsDevice.SetRenderTarget(renderTargetEye[eye]);
                GraphicsDevice.Clear(new Color(130, 180, 255));

                Matrix view = rift.GetEyeViewMatrix(eye, playerMatrix);
                Matrix projection = rift.GetProjectionMatrix(eye);

                manyCubes.Draw(view, projection);

                //base.Draw(gameTime);
            } 

            // submit rendertargets to the Rift
            int result = rift.SubmitRenderTargets(renderTargetEye[0], renderTargetEye[1]);

            // show left eye view also on the monitor screen 
            DrawEyeViewIntoBackbuffer(0);
        }

        void DrawEyeViewIntoBackbuffer(int eye)
        {
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            var pp = GraphicsDevice.PresentationParameters;

            int height = pp.BackBufferHeight;
            int width = Math.Min(pp.BackBufferWidth, (int)(height * rift.GetRenderTargetAspectRatio(eye)));
            int offset = (pp.BackBufferWidth - width) / 2;

            spriteBatch.Begin();
            spriteBatch.Draw(renderTargetEye[eye], new Rectangle(offset, 0, width, height), Color.White);
            spriteBatch.End();
        }

        void DoPlayerMotion()
        {
            var mouse = Mouse.GetState();      

            var pp = GraphicsDevice.PresentationParameters;

            playerMatrix.Translation = new Vector3(
                0.01f * (mouse.X - pp.BackBufferWidth/2), 
                0.7f,
                0.01f * (mouse.Y - pp.BackBufferHeight / 2));     
        }
    }
}