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
        Up,
        Left,
        Right,
        Down,
        DiagonalUpLeft,
        DiagonalUpRight,
        DiagonalDownLeft,
        DiagonalDownRight
    }

    static class DirectionHelper
    {
        public static AnimationDirection GetDirectionFromHeading(Vector2 heading)
        {
            // heading is normalized. 45 degrees diagonal is ~ (0.71, 0.71)
            // Diagonal segment is 45deg, so half on either side = 45 / 2 = 22.5
            // Opposite / Hypotenuse = sin(22.5) / 1.0 = 0.382;
            const float DIAGONAL_CUT_IN = 0.382f;

            if (heading.X < -DIAGONAL_CUT_IN && heading.Y < -DIAGONAL_CUT_IN)
                return AnimationDirection.DiagonalUpLeft;
            if (heading.X > DIAGONAL_CUT_IN && heading.Y < -DIAGONAL_CUT_IN)
                return AnimationDirection.DiagonalUpRight;
            if (heading.X < -DIAGONAL_CUT_IN && heading.Y > DIAGONAL_CUT_IN)
                return AnimationDirection.DiagonalDownLeft;
            if (heading.X > DIAGONAL_CUT_IN && heading.Y > DIAGONAL_CUT_IN)
                return AnimationDirection.DiagonalDownRight;

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
            if (heading.X < -5 && heading.Y < -5)
                return AnimationDirection.DiagonalUpLeft;
            if (heading.X > 5 && heading.Y < -5)
                return AnimationDirection.DiagonalUpRight;
            if (heading.X < -5 && heading.Y > 5)
                return AnimationDirection.DiagonalDownLeft;
            if (heading.X > 5 && heading.Y > 5)
                return AnimationDirection.DiagonalDownRight;

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

                case AnimationDirection.DiagonalUpLeft:
                    return new Vector2(-1f, -1f);
                case AnimationDirection.DiagonalUpRight:
                    return new Vector2(1f, -1f);
                case AnimationDirection.DiagonalDownLeft:
                    return new Vector2(-1f, 1f);
                case AnimationDirection.DiagonalDownRight:
                    return new Vector2(1f, 1f);
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

                case AnimationDirection.DiagonalUpLeft:
                    return AnimationDirection.DiagonalDownRight;
                case AnimationDirection.DiagonalUpRight:
                    return AnimationDirection.DiagonalDownLeft;
                case AnimationDirection.DiagonalDownLeft:
                    return AnimationDirection.DiagonalUpRight;
                case AnimationDirection.DiagonalDownRight:
                    return AnimationDirection.DiagonalUpLeft;
            }
            return AnimationDirection.Down;
        }
    }
}
