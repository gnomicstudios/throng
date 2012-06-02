using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gnomic.Core
{
    public class Camera2D
    {
		public Camera2D()
        {
			Zoom = 1.0f;
			DefaultZoom = 1.0f;
		}

        public Camera2D(Viewport viewport)
        {
            Origin = new Vector2(viewport.Width / 2.0f, viewport.Height / 2.0f);
			Zoom = 1.0f;
			DefaultZoom = 1.0f;
        }
        public Camera2D(Viewport viewport, Vector2 originalScreenSizeToScaleFrom)
        {
            Origin = originalScreenSizeToScaleFrom * 0.5f;
			Zoom = 1.0f;
			DefaultZoom = 1.0f;

			// Calculate default zoom required to scale screen from given original size
			float originalAspect = originalScreenSizeToScaleFrom.X / originalScreenSizeToScaleFrom.Y;
			float targetAspect = viewport.AspectRatio;
			if (originalAspect > targetAspect)
            {
                DefaultZoom = viewport.Width / originalScreenSizeToScaleFrom.X;
                ExtraOffset = new Vector2(0.0f, 0.5f * (viewport.Height - DefaultZoom * originalScreenSizeToScaleFrom.Y));
            }
			else
            {
                DefaultZoom = viewport.Height / originalScreenSizeToScaleFrom.Y;
                ExtraOffset = new Vector2(0.5f * (viewport.Width - DefaultZoom * originalScreenSizeToScaleFrom.X), 0.0f);
            }
        }


        public Vector2 Position { get; set; }
        public Vector2 Origin { get; set; }
		public float Zoom { get; set; }
		public float DefaultZoom { get; set; }
        public float Rotation { get; set; }
        public Vector2 ExtraOffset { get; set; }

        public Matrix GetViewMatrix()
        {
			return GetViewMatrix(Vector2.One);
        }

        public Matrix GetViewMatrix(Vector2 parallax)
        {
            return Matrix.CreateTranslation(new Vector3(-Position * parallax, 0.0f)) *
                   Matrix.CreateTranslation(new Vector3((-Origin + ExtraOffset) / (Zoom * DefaultZoom), 0.0f)) *
                   //Matrix.CreateRotationZ(Rotation) *
				   Matrix.CreateScale(Zoom * DefaultZoom, Zoom * DefaultZoom, 1.0f) *
                   Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
        }

        //public Matrix GetViewMatrixInvertY()
        //{
        //    return GetViewMatrixInvertY(Vector2.One);
        //}

        //public Matrix GetViewMatrixInvertY(Vector2 parallax)
        //{
        //    return Matrix.CreateTranslation(new Vector3(new Vector2(-Position.X, Position.Y) / (parallax * Zoom * DefaultZoom), 0.0f)) *
        //           Matrix.CreateTranslation(new Vector3(new Vector2(-Origin.X, Origin.Y), 0.0f)) *
        //           Matrix.CreateRotationZ(Rotation) *
        //           Matrix.CreateScale(Zoom * DefaultZoom, Zoom * DefaultZoom, 1.0f) *
        //           Matrix.CreateTranslation(new Vector3(Origin.X, -Origin.Y, 0.0f));
        //}

		public Camera2D Clone()
		{
			Camera2D c = new Camera2D();
			c.Position = Position;
			c.Origin = Origin;
			c.Zoom = Zoom;
			c.DefaultZoom = DefaultZoom;
			c.Rotation = Rotation;
            c.ExtraOffset = ExtraOffset;

			return c;
		}
    }
}
