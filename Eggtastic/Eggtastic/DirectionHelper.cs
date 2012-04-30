using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Eggtastic
{
    public enum AnimationDirection
    {
        Any,
        Up,
        Left,
        Right,
        Down
    }

    static class DirectionHelper
    {
        public static AnimationDirection GetDirectionFromHeading(Vector2 heading)
        {
            if (Math.Abs(heading.X) > Math.Abs(heading.Y))
            {
                return (heading.X > 0) ? AnimationDirection.Right : AnimationDirection.Left;
            }
            else
            {
                return (heading.Y > 0) ? AnimationDirection.Down : AnimationDirection.Up;
            }
        }
        public static AnimationDirection GetDirectionFromHeadingBiasHorizontal(Vector2 heading, float biasAmount)
        {
            heading.Normalize();
            if (Math.Abs(heading.X) > Math.Abs(heading.Y) - biasAmount)
            {
                return (heading.X > 0) ? AnimationDirection.Right : AnimationDirection.Left;
            }
            else
            {
                return (heading.Y > 0) ? AnimationDirection.Down : AnimationDirection.Up;
            }
        }

        public static Vector2 GetFacingFromDirection(AnimationDirection direction)
        {
            switch (direction)
            {
                case AnimationDirection.Right:
                    return new Vector2(1f, 0f);
                case AnimationDirection.Left:
                    return new Vector2(-1f, 0f);
                case AnimationDirection.Up:
                    return new Vector2(0f, -1f);
                case AnimationDirection.Down:
                    return new Vector2(0f, 1f);
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
                case AnimationDirection.Up:
                    return AnimationDirection.Down;
                case AnimationDirection.Down:
                    return AnimationDirection.Up;
            }
            return AnimationDirection.Down;
        }

    }
}
