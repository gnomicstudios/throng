using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gnomic.Anim
{
    public struct Transform2D
    {
        public Vector2 Pos;
        public Vector2 Scale;
        public float Rot;

        private static Transform2D _identity;
        public static Transform2D Identity { get { return _identity; } }

        static Transform2D()
        {
            _identity = new Transform2D();
            _identity.Scale = Vector2.One;
        }

        public static Transform2D Compose(Transform2D key1, Transform2D key2)
        {
            Transform2D result = new Transform2D();
            Vector2 transformedPos = key1.TransformPoint(key2.Pos);
            result.Pos = transformedPos;
            result.Rot = key1.Rot + key2.Rot;
            result.Scale = key1.Scale * key2.Scale;
            return result;
        }

        public static void Compose(ref Transform2D key1, ref Transform2D key2, out Transform2D result)
        {
            Vector2 transformedPos = key1.TransformPoint(key2.Pos);
            result.Pos = transformedPos;
            result.Rot = key1.Rot + key2.Rot;
            result.Scale = key1.Scale * key2.Scale;
        }

        public static void Lerp(ref Transform2D key1, ref Transform2D key2, float amount, ref Transform2D result)
        {
            result.Pos = Vector2.Lerp(key1.Pos, key2.Pos, amount);
            result.Scale = Vector2.Lerp(key1.Scale, key2.Scale, amount);
            result.Rot = MathHelper.Lerp(key1.Rot, key2.Rot, amount);
        }

        public Vector2 TransformPoint(Vector2 point)
        {
            Matrix rotZ = Matrix.CreateRotationZ(Rot);
            Vector2 result;
            Vector2.Transform(ref point, ref rotZ, out result);
            return new Vector2(result.X * Scale.X + Pos.X, result.Y * Scale.Y + Pos.Y);
        }

        public Transform2D Translate(Vector2 translation)
        {
            Transform2D result = this;
            result.Pos += translation;
            return result;
        }
    }
}
