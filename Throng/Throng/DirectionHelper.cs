using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Throng
{
    public enum AnimationDirection
    {
        Any,
        Left,
        Right
    }

    static class DirectionHelper
    {
        public static AnimationDirection GetDirectionFromHeading(Vector2 heading)
        {
            return (heading.X > 0) ? AnimationDirection.Right : AnimationDirection.Left;
        }
       
        public static Vector2 GetFacingFromDirection(AnimationDirection direction)
        {
            switch (direction)
            {
                case AnimationDirection.Right:
                    return new Vector2(1f, 0f);
                case AnimationDirection.Left:
                    return new Vector2(-1f, 0f);
            }
            return Vector2.Zero;
        }

        public static AnimationDirection GetOppositeDirection(AnimationDirection direction)
        {
            switch (direction)
            {
                case AnimationDirection.Left:
                    return AnimationDirection.Right;
                case AnimationDirection.Right:
                    return AnimationDirection.Left;
            }
            return AnimationDirection.Left;
        }
    }
}
