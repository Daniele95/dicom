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

namespace VolumeRayCasting
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class VolumetricRayCasting : Microsoft.Xna.Framework.Game
    {
        #region Fields

        private GraphicsDeviceManager mGraphics;
        private SpriteBatch mSpriteBatch;
        private SpriteFont mSpriteFont;

        private OrbitCamera mCamera;

        private float mEllapsedTime;
        private float mPrevTimeDelta;
        private float mPrevPrevTD;
        private int mFrameCount;
        private int mFrameRate;
        private int mSavedCount;

        private float mMouseScale;

        private KeyboardState mPrevKBState;
        private MouseState mPrevMouseState;

        private GameTime mGameTime;
        private bool mTakeScreenCap = false;

        private Color mClearColor;

        #endregion

        public VolumetricRayCasting()
        {
            mGraphics = new GraphicsDeviceManager(this);
            mGraphics.PreferredBackBufferWidth = 800;
            mGraphics.PreferredBackBufferHeight = 600;
            mGraphics.PreferMultiSampling = false;
            mGraphics.SynchronizeWithVerticalRetrace = false;
            mGraphics.IsFullScreen = false;

            this.IsFixedTimeStep = false;
            this.IsMouseVisible = true;

            Content.RootDirectory = "Content";

            mFrameCount = 0;
            mEllapsedTime = 0.0f;
            mFrameRate = 0;

            mMouseScale = 0.25f;

            mClearColor = Color.Black;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            #region Camera Setup
            //Setup the camera used to render with
            mCamera = new OrbitCamera(GraphicsDevice.Viewport);

            Vector3 target = new Vector3(2.5f, 2.5f, 2.5f);

            mCamera.Position = new Vector3(2.5f, 2.5f, -3.0f);
            mCamera.Target = target;
            mCamera.Update();
            mCamera.Revolve(new Vector3(1, 0, 0), MathHelper.ToRadians(180.0f));
            mCamera.RevolveGlobal(new Vector3(0, 1, 0), MathHelper.ToRadians(180.0f));
            mCamera.Update();
            #endregion

            #region Scene Setup
            //the teapot, foot, bonsai, engine, and aneurism models can be found at volvis.org

            Vector3 extents = new Vector3(256, 256, 178); // teapot;
            //Vector3 extents = new Vector3(256, 256, 256); // foot, bonsai, aneurism;
            //Vector3 extents = new Vector3(256, 256, 128); // engine;

            Volume mesh = new Volume(this, "Content/Models/teapot.raw", ref extents);
            mesh.MeshAsset = "Models/box";
            mesh.EffectAsset = "Shaders/RayCasting";
            mesh.EnableLighting = false;
            mesh.Scale = 5.0f;
            mesh.Technique = "RayCastSimple";
            mesh.DrawBoundingBox = true;
            mesh.Ratios = Vector3.One; //teapot, foot, bonzai, engine, aneurism
            mesh.StepScale = 0.5f; // the size of the step to take. Lower means shorter steps. .5f or 1.0f are good values.

            Components.Add(mesh);
            #endregion

            GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            // Create a new SpriteBatch, which can be used to draw textures.
            mSpriteBatch = new SpriteBatch(GraphicsDevice);

            //load the sprie font
            mSpriteFont = Content.Load<SpriteFont>("Font");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            Content.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            mGameTime = gameTime;

            int milliseconds = (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            timeDelta = (mPrevTimeDelta + mPrevPrevTD + timeDelta) / 3.0f;
            mEllapsedTime += timeDelta;

            updateInput(timeDelta, milliseconds);

            mPrevPrevTD = mPrevTimeDelta;
            mPrevTimeDelta = timeDelta;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //update the frame rate
            if (mEllapsedTime >= 1.0f)
            {
                mFrameRate = mFrameCount;
                mEllapsedTime = 0.0f;
                mFrameCount = 0;
            }

            foreach (Mesh mesh in Components)
            {
                mesh.View = mCamera.View;
                mesh.Projection = mCamera.Projection;
            }

            GraphicsDevice.RenderState.AlphaBlendEnable = true;
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, mClearColor, 1.0f, 1);

            base.Draw(gameTime);

            #region Draw the Frame Rate and Message
            mFrameCount++;

            string fps = String.Format("FPS: {0}", mFrameRate);

            mSpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);

            mSpriteBatch.DrawString(mSpriteFont, fps, new Vector2(30, 30), Color.Red);

            mSpriteBatch.End();


            #endregion

            if (mTakeScreenCap)
            {
                PresentationParameters pp = GraphicsDevice.PresentationParameters;
                SurfaceFormat format = pp.BackBufferFormat;
                MultiSampleType msType = pp.MultiSampleType;
                int msQuality = pp.MultiSampleQuality;

                using (ResolveTexture2D texture = new ResolveTexture2D(GraphicsDevice,
                                      pp.BackBufferWidth, pp.BackBufferHeight, 1, format))
                {
                    mGraphics.GraphicsDevice.ResolveBackBuffer(texture);
                    texture.Save("capture" + mSavedCount++ + ".bmp", ImageFileFormat.Bmp);
                }

                mTakeScreenCap = false;
            }
        }

        private void updateInput(float timeDelta, int milliseconds)
        {
            KeyboardState keyState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            else if (keyState.IsKeyDown(Keys.Escape))
                this.Exit();

            float x = mouseState.X - mPrevMouseState.X;
            float y = mPrevMouseState.Y - mouseState.Y;

            Vector3 mouseVector = new Vector3(x, y, 0.0f);

            Vector3 view = Vector3.UnitZ;

            Vector3 axis = Vector3.Cross(mouseVector, view);
            axis.Normalize();

            if (this.IsActive && mouseState.LeftButton == ButtonState.Pressed && (x != 0.0f || y != 0.0f))
            {
                axis = Vector3.Cross(Vector3.UnitX, mCamera.View.Right);
                axis.Normalize();

                mCamera.Revolve(Vector3.UnitX, y * mMouseScale * timeDelta);

                axis = Vector3.Cross(Vector3.UnitX, mCamera.View.Forward);
                axis.Normalize();

                mCamera.RevolveGlobal(Vector3.UnitY, -x * mMouseScale * timeDelta);
            }

            if (mouseState.RightButton == ButtonState.Pressed)
            {
                mCamera.Translate(new Vector3(0, 0, y * timeDelta * mMouseScale));
            }

            if (keyState.IsKeyDown(Keys.P) && mPrevKBState.IsKeyUp(Keys.P))
            {
                mTakeScreenCap = true;
            }

            mPrevKBState = keyState;
            mPrevMouseState = mouseState;
        }
    }
}
