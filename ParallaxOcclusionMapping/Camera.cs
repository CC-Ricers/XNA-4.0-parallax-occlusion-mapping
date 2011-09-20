/*
 * Created by Javier Cantón Ferrero.
 * MVP Windows-DirectX 2007/2008
 * DotnetClub Sevilla
 * Date 26/12/2007
 * Web www.codeplex.com/XNACommunity
 * Email javiuniversidad@gmail.com
 * blog: mirageproject.blogspot.com
 */
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace ParallaxOcclusionMapping
{
    public class Camera
    {
        private Matrix world;
        private Matrix view;
        private Matrix projection;
        protected Vector3 position;
        private Vector3 lookat;
        private float aspectRatio;
        private float nearPlane;
        private float farPlane;

        #region Propeties
        public float FarPlane
        {
            get { return farPlane; }
            set
            {
                farPlane = value;
                InitCamera();
            }
        }

        public float NearPlane
        {
            get { return nearPlane; }
            set
            {
                nearPlane = value;
                InitCamera();
            }
        }

        public float AspectRatio
        {
            get { return aspectRatio; }
            set
            {
                aspectRatio = value;
                InitCamera();
            }
        }

        public Vector3 Lookat
        {
            get { return lookat; }
            set
            {
                lookat = value;
                InitCamera();
            }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; }
        }

        public Matrix View
        {
            get { return view; }
            set { view = value; }
        }


        public Matrix World
        {
            get { return world; }
            set { world = value; }
        }

        public Matrix WoldViewProj
        {
            get { return (world * view * projection); }
        }
        #endregion

        public Camera(Vector3 position)
        {
            this.position = position;
            this.lookat = Vector3.Zero;
            aspectRatio = 640f / 480f;
            nearPlane = 1f;
            farPlane = 1000f;
            InitCamera();
        }

        public Camera(Vector3 position, Vector3 lookat)
        {
            this.position = position;
            this.lookat = lookat;
            aspectRatio = 640f / 480f;
            nearPlane = 1f;
            farPlane = 1000f;
            InitCamera();
        }

        private void InitCamera()
        {
            this.view = Matrix.CreateLookAt(position, lookat, Vector3.Up);
            this.projection = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 3, aspectRatio, nearPlane, farPlane);
            this.world = Matrix.Identity;
        }
    }
}
