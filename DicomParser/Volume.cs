using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace VolumeRayCasting
{
    /// <summary>
    /// This cass represents a 3D volume. It loads a 3D cube model and fits it to the 3D volume.
    /// It is also responsible for performing the volume ray-casting pass on the volume.
    /// </summary>
    public class Volume : Mesh
    {
        #region Fields
        private RenderTarget2D mFront;
        private RenderTarget2D mBack;

        private Texture3D mVolume;

        private float[] mScalars;

        private float mStepScale = 1.0f;
        private Vector3 mRatios;

        private string mCurrTechnique;

        string mVolumeFile;
        int mWidth, mHeight, mDepth;
        #endregion

        #region Properties

        public float StepScale
        {
            get { return mStepScale; }
            set
            {
                mStepScale = value;
                if (mEffect != null)
                {
                    float maxSize = (float)Math.Max(mWidth, Math.Max(mHeight, mDepth));
                    Vector3 stepSize = new Vector3(1.0f / mWidth, 1.0f / mHeight, 1.0f / mDepth);
                    mEffect.Parameters["StepSize"].SetValue(stepSize * mStepScale);
                    mEffect.Parameters["Iterations"].SetValue((int)maxSize * (1.0f / mStepScale));
                }
            }
        }

        public Vector3 Ratios
        {
            get { return mRatios; }
            set { mRatios = value; }
        }

        public string Technique
        {
            get { return mCurrTechnique; }
            set { mCurrTechnique = value; }
        }

        public bool DrawBoundingBox { get; set; }
        #endregion

        public Volume(Game game, string volFile, int width, int height, int depth)
            : base(game)
        {
            mVolumeFile = volFile;

            mWidth = width;
            mHeight = height;
            mDepth = depth;

            mCurrTechnique = "RayCastSimple";
            DrawBoundingBox = true;
        }

        public Volume(Game game, string volFile, ref Vector3 sizes)
            : base(game)
        {
            mVolumeFile = volFile;

            mWidth = (int)sizes.X;
            mHeight = (int)sizes.Y;
            mDepth = (int)sizes.Z;

            mCurrTechnique = "RayCastSimple";
            DrawBoundingBox = true;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            PresentationParameters pp = Game.GraphicsDevice.PresentationParameters;
            SurfaceFormat format = pp.BackBufferFormat;
            MultiSampleType msType = pp.MultiSampleType;
            int msQuality = pp.MultiSampleQuality;

            //create the front and back position textures
            //check to make sure that there is a sutiable format supported
            SurfaceFormat rtFormat = SurfaceFormat.HalfVector4;
            if (isFormatSupported(SurfaceFormat.HalfVector4))
            {
                rtFormat = SurfaceFormat.HalfVector4;
            }
            else if (isFormatSupported(SurfaceFormat.Vector4))
            {
                rtFormat = SurfaceFormat.Vector4;
            }
            else if (isFormatSupported(SurfaceFormat.Rgba64))
            {
                rtFormat = SurfaceFormat.Rgba64;
            }
            else //no sutiable format found
            {
                System.Windows.Forms.MessageBox.Show("Hardware must be SM 3.0 compliant and support RGBA16F, RGBA32F, or RGBA64.",
                                "Error creating position render targets", System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Exclamation);
                Game.Exit();
            }

            mFront = new RenderTarget2D(Game.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight,
                                        1, rtFormat, msType, msQuality);
            mBack = new RenderTarget2D(Game.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight,
                                        1, rtFormat, msType, msQuality);

            //create the scalar volume texture
            mVolume = new Texture3D(Game.GraphicsDevice, mWidth, mHeight, mDepth, 0,
                                    TextureUsage.Linear, SurfaceFormat.Single);

            //compute the step size and number of iterations to use
            //the step size for each component needs to be a ratio of the largest component
            float maxSize = (float)Math.Max(mWidth, Math.Max(mHeight, mDepth));
            Vector3 stepSize = new Vector3(1.0f / (mWidth * (maxSize / mWidth)),
                                           1.0f / (mHeight * (maxSize / mHeight)),
                                           1.0f / (mDepth * (maxSize / mDepth)));

            mEffect.Parameters["StepSize"].SetValue(stepSize * mStepScale);
            mEffect.Parameters["Iterations"].SetValue((int)maxSize * (1.0f / mStepScale) * 2.0f);

            //calculate the scale factor
            //volumes are not always perfect cubes. so we need to scale our cube
            //by the sizes of the volume. Also, scalar data is not always sampled
            //at equidistant steps. So we also need to scale the cube model by mRatios.
            Vector3 sizes = new Vector3(mWidth, mHeight, mDepth);
            Vector3 scaleFactor = Vector3.One / ((Vector3.One * maxSize) / (sizes * mRatios));
            mEffect.Parameters["ScaleFactor"].SetValue(new Vector4(scaleFactor, 1.0f));

            //load the raw file
            loadRAWFile(mVolumeFile);

            //no longer used
            mScalars = null;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            //set the technique to draw positions
            mEffect.CurrentTechnique = mEffect.Techniques["RenderPosition"];

            Game.GraphicsDevice.RenderState.AlphaBlendEnable = false;

            //draw front faces
            //draw the pixel positions to the texture
            Game.GraphicsDevice.SetRenderTarget(0, mFront);
            Game.GraphicsDevice.Clear(Color.Black);

            base.DrawCustomEffect();

            Game.GraphicsDevice.SetRenderTarget(0, null);

            //draw back faces
            //draw the pixel positions to the texture
            Game.GraphicsDevice.SetRenderTarget(0, mBack);
            Game.GraphicsDevice.Clear(Color.Black);
            Game.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            base.DrawCustomEffect();

            Game.GraphicsDevice.SetRenderTarget(0, null);
            Game.GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
        }

        protected override void DrawCustomEffect()
        {
            //Game.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            Game.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            Game.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

            //draw wireframe
            if (DrawBoundingBox)
            {
                Game.GraphicsDevice.RenderState.CullMode = CullMode.None;
                Game.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;

                mEffect.CurrentTechnique = mEffect.Techniques["WireFrame"];
                base.DrawCustomEffect();

                Game.GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
                Game.GraphicsDevice.RenderState.FillMode = FillMode.Solid;
            }

            mEffect.CurrentTechnique = mEffect.Techniques[mCurrTechnique];
            mEffect.Parameters["Front"].SetValue(mFront.GetTexture());
            mEffect.Parameters["Back"].SetValue(mBack.GetTexture());

            base.DrawCustomEffect();
        }

        #region File Loading Helpers
        private void loadRAWFile(string filename)
        {
            FileStream file = new FileStream(filename, FileMode.Open);
            long length = file.Length;

            if (length > mWidth * mHeight * mDepth)
            {
                loadRAWFile16(file);
            }
            else
            {
                loadRAWFile8(file);
            }

            file.Close();
        }

        /// <summary>
        /// Loads an 8-bit RAW file.
        /// </summary>
        /// <param name="file"></param>
        private void loadRAWFile8(FileStream file)
        {
            BinaryReader reader = new BinaryReader(file);

            byte[] buffer = new byte[mWidth * mHeight * mDepth];
            int size = sizeof(byte);

            reader.Read(buffer, 0, size * buffer.Length);

            reader.Close();

            //scale the scalar values to [0, 1]
            mScalars = new float[buffer.Length];
            for (int i = 0; i < buffer.Length; i++)
            {
                mScalars[i] = (float)buffer[i] / byte.MaxValue;
            }

            mVolume.SetData(mScalars);
            mEffect.Parameters["Volume"].SetValue(mVolume);
        }

        /// <summary>
        /// Loads a 16-bit RAW file.
        /// </summary>
        /// <param name="file"></param>
        private void loadRAWFile16(FileStream file)
        {
            BinaryReader reader = new BinaryReader(file);

            ushort[] buffer = new ushort[mWidth * mHeight * mDepth];

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = reader.ReadUInt16();

            reader.Close();

            //scale the scalar values to [0, 1]
            mScalars = new float[buffer.Length];
            for (int i = 0; i < buffer.Length; i++)
            {
                mScalars[i] = (float)buffer[i] / ushort.MaxValue;
            }

            mVolume.SetData(mScalars);
            mEffect.Parameters["Volume"].SetValue(mVolume);
        }

        private bool isFormatSupported(SurfaceFormat format)
        {
            return GraphicsDevice.CreationParameters.Adapter.CheckDeviceFormat(
                                         GraphicsDevice.CreationParameters.DeviceType,
                                         GraphicsDevice.DisplayMode.Format,
                                         TextureUsage.None,
                                         QueryUsages.None,
                                         ResourceType.RenderTarget,
                                         format);
        }
        #endregion
    }
}
