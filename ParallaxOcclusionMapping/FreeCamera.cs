/*
 * Created by Javier Cantón Ferrero.
 * MVP Windows-DirectX 2007/2008
 * DotnetClub Sevilla
 * Date 17/02/2007
 * Web www.codeplex.com/XNACommunity
 * Email javiuniversidad@gmail.com
 * blog: mirageproject.blogspot.com
 */


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace ParallaxOcclusionMapping
{
     public class FreeCamera : Camera
    {
        private Vector3 angle;
        private float speed;
        private float turnSpeed;

        #region Properties
        public float TurnSpeed
        {
            get { return turnSpeed; }
            set { turnSpeed = value; }
        }

        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        public Vector3 Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        #endregion

        public FreeCamera(Vector3 position)
            : base(position)
        {
            speed = 250f;
            turnSpeed = 90f;
        }

        public FreeCamera(Vector3 position, float speed, float turnSpeed)
            : base(position)
        {
            this.speed = speed;
            this.turnSpeed = turnSpeed;
        }

        /// <summary>
        /// Actualiza la matriz de vista
        /// </summary>
        /// <param name="gameTime">objecto gameTime</param>
        /// <param name="center">"Game.Window.ClientBounds.Width / 2"</param>
        public void Update(int Width, float TotalSeconds)
        {
            int center = Width / 2;
            float delta = TotalSeconds;
            GamePadState gamePad = GamePad.GetState(PlayerIndex.One);
            Vector3 forward;
            Vector3 left;

            if (gamePad.IsConnected)
            {
                angle.X -= gamePad.ThumbSticks.Right.Y * turnSpeed * 0.001f;
                angle.Y += gamePad.ThumbSticks.Right.X * turnSpeed * 0.001f;

                forward = Vector3.Normalize(new Vector3((float)Math.Sin(-angle.Y), (float)Math.Sin(angle.X), (float)Math.Cos(-angle.Y)));
                left = Vector3.Normalize(new Vector3((float)Math.Cos(angle.Y), 0f, (float)Math.Sin(angle.Y)));

                position -= forward * speed * gamePad.ThumbSticks.Left.Y * delta;
                position += left * speed * gamePad.ThumbSticks.Left.X * delta;

                View = Matrix.Identity;
                View *= Matrix.CreateTranslation(-position);
                View *= Matrix.CreateRotationZ(angle.Z);
                View *= Matrix.CreateRotationY(angle.Y);
                View *= Matrix.CreateRotationX(angle.X);
            }
            else
            {
                KeyboardState keyboard = Keyboard.GetState();
                MouseState mouse = Mouse.GetState();

                int centerX = center;
                int centerY = center;

                Mouse.SetPosition(centerX, centerY);

                angle.X += MathHelper.ToRadians((mouse.Y - centerY) * turnSpeed * 0.01f); // pitch
                angle.Y += MathHelper.ToRadians((mouse.X - centerX) * turnSpeed * 0.01f); // yaw

                forward = Vector3.Normalize(new Vector3((float)Math.Sin(-angle.Y), (float)Math.Sin(angle.X), (float)Math.Cos(-angle.Y)));
                left = Vector3.Normalize(new Vector3((float)Math.Cos(angle.Y), 0f, (float)Math.Sin(angle.Y)));

                if (keyboard.IsKeyDown(Keys.W))
                    position -= forward * speed * delta;

                if (keyboard.IsKeyDown(Keys.S))
                    position += forward * speed * delta;

                if (keyboard.IsKeyDown(Keys.A))
                    position -= left * speed * delta;

                if (keyboard.IsKeyDown(Keys.D))
                    position += left * speed * delta;

                if (keyboard.IsKeyDown(Keys.Z))
                    position += Vector3.Down * speed * delta;

                if (keyboard.IsKeyDown(Keys.X))
                    position += Vector3.Up * speed * delta;

                if (keyboard.IsKeyDown(Keys.Left))
                    angle.Y -= speed * delta * 0.2f;
                if (keyboard.IsKeyDown(Keys.Right))
                    angle.Y += speed * delta * 0.2f;

                View = Matrix.Identity;
                View *= Matrix.CreateTranslation(-position);
                View *= Matrix.CreateRotationZ(angle.Z);
                View *= Matrix.CreateRotationY(angle.Y);
                View *= Matrix.CreateRotationX(angle.X);
            }
        }
    }
}
