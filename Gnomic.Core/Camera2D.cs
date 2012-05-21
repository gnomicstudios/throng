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
            Origin = new Vector2(viewport.Width / 2.0f, viewport.Height / 2.0f);
			Zoom = 1.0f;
			DefaultZoom = 1.0f;

			// Calculate default zoom required to scale screen from given original size
			float originalAspect = originalScreenSizeToScaleFrom.X / originalScreenSizeToScaleFrom.Y;
			float targetAspect = viewport.AspectRatio;
			if (originalAspect > targetAspect)
				DefaultZoom = viewport.Width / originalScreenSizeToScaleFrom.X;
			else
				DefaultZoom = viewport.Height / originalScreenSizeToScaleFrom.Y;
        }


        public Vector2 Position { get; set; }
        public Vector2 Origin { get; set; }
		public float Zoom { get; set; }
		public float DefaultZoom { get; set; }
        public float Rotation { get; set; }


        public Matrix GetViewMatrix()
        {
			return GetViewMatrix(Vector2.One);
        }

        public Matrix GetViewMatrix(Vector2 parallax)
        {
            return Matrix.CreateTranslation(new Vector3(-Position * parallax, 0.0f)) *
                   Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
                   Matrix.CreateRotationZ(Rotation) *
				   Matrix.CreateScale(Zoom * DefaultZoom, Zoom * DefaultZoom, 1.0f) *
                   Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
        }

        public Matrix GetViewMatrixInvertY()
        {
			return GetViewMatrixInvertY(Vector2.One);
        }

        public Matrix GetViewMatrixInvertY(Vector2 parallax)
        {
            return Matrix.CreateTranslation(new Vector3(new Vector2(-Position.X, Position.Y) * parallax, 0.0f)) *
                   Matrix.CreateTranslation(new Vector3(new Vector2(-Origin.X, Origin.Y), 0.0f)) *
                   Matrix.CreateRotationZ(Rotation) *
				   Matrix.CreateScale(Zoom * DefaultZoom, Zoom * DefaultZoom, 1.0f) *
                   Matrix.CreateTranslation(new Vector3(Origin.X, -Origin.Y, 0.0f));
        }

		public Camera2D Clone()
		{
			Camera2D c = new Camera2D();
			c.Position = Position;
			c.Origin = Origin;
			c.Zoom = Zoom;
			c.DefaultZoom = DefaultZoom;
			c.Rotation = Rotation;

			return c;
		}
    }
}
