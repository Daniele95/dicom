/*
 * OrbitCamera is originally by Mike @ http://www.thehazymind.com/
 * 
 */

using System;
using System.Collections;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace VolumeRayCasting
{
    public class OrbitCamera
    {
        private Viewport myViewport;

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Matrix World { get; private set; }
        public Matrix View { get; set; }
        public Matrix Projection { get; private set; }
        public Matrix ViewProj { get { return View * Projection; } }
        public float FieldOfView { get; private set; }
        public Vector3 Target { get; set; }
        public Viewport Viewport
        {
            get { return myViewport; }
            set
            {
                myViewport = value;
                myViewport.MinDepth = 1.0f;
                myViewport.MaxDepth = 1000.0f;
            }
        }

        public OrbitCamera(Viewport newViewport)
        {
            Position = new Vector3(0, 0, 1);
            Rotation = new Quaternion(0, 0, 0, 1);
            FieldOfView = MathHelper.Pi / 3.0f;
            Target = new Vector3(0, 0, 0);

            SetViewport(newViewport);
        }

        public void SetViewport(Viewport newViewport)
        {
            newViewport.MinDepth = .1f;
            newViewport.MaxDepth = 100.0f;

            Viewport = newViewport;

            float yScale = 1.0f / (float)Math.Tan(FieldOfView / 2.0f);
            float xScale = yScale / Viewport.AspectRatio;

            float farZ = Viewport.MaxDepth;
            float nearZ = Viewport.MinDepth;

            Matrix proj;
            proj.M11 = xScale; proj.M12 = 0.0f; proj.M13 = 0.0f; proj.M14 = 0.0f;
            proj.M21 = 0.0f; proj.M22 = yScale; proj.M23 = 0.0f; proj.M24 = 0.0f;
            proj.M31 = 0.0f; proj.M32 = 0.0f; proj.M33 = farZ / (farZ - nearZ); proj.M34 = 1.0f;
            proj.M41 = 0.0f; proj.M42 = 0.0f; proj.M43 = -nearZ * farZ / (farZ - nearZ); proj.M44 = 0.0f;

            //Projection = Matrix.CreatePerspectiveFieldOfView(
            //    FieldOfView,
            //    Viewport.AspectRatio,
            //    Viewport.MinDepth, Viewport.MaxDepth
            //);
            Projection = proj;
        }

        public void Rotate(Vector3 axis, float angle)
        {
            axis = Vector3.Transform(axis, Matrix.CreateFromQuaternion(Rotation));
            Rotation = Quaternion.Normalize(Quaternion.CreateFromAxisAngle(axis, angle) * Rotation);

            Update();
        }

        public void Translate(Vector3 distance)
        {
            Position += Vector3.Transform(distance, Matrix.CreateFromQuaternion(Rotation));

            Update();
        }

        public void Revolve(Vector3 axis, float angle)
        {
            Vector3 revolveAxis = Vector3.Transform(axis, Matrix.CreateFromQuaternion(Rotation));
            Quaternion rotate = Quaternion.CreateFromAxisAngle(revolveAxis, angle);
            Position = Vector3.Transform(Position - Target, Matrix.CreateFromQuaternion(rotate)) + Target;

            Rotate(axis, angle);
        }

        public void RotateGlobal(Vector3 axis, float angle)
        {
            Rotation = Quaternion.Normalize(Quaternion.CreateFromAxisAngle(axis, angle) * Rotation);

            Update();
        }

        public void TranslateGlobal(Vector3 distance)
        {
            Position += distance;

            Update();
        }

        public void RevolveGlobal(Vector3 axis, float angle)
        {
            Quaternion rotate = Quaternion.CreateFromAxisAngle(axis, angle);
            Position = Vector3.Transform(Position - Target, Matrix.CreateFromQuaternion(rotate)) + Target;

            RotateGlobal(axis, angle);
        }

        public void Update()
        {
            World = Matrix.Identity;

            View = Matrix.Invert(
                Matrix.CreateFromQuaternion(Rotation) *
                Matrix.CreateTranslation(Position)
            );
        }
    }
}