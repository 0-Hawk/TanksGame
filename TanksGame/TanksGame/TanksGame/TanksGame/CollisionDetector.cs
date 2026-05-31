using System.Windows;

namespace TanksGame
{
    public static class CollisionDetector
    {
        public static bool IsColliding(Rect a, Rect b)
        {
            return a.IntersectsWith(b);
        }

        public static bool CheckWallCollision(Rect tankRect, System.Collections.Generic.List<Wall> walls)
        {
            foreach (var wall in walls)
            {
                if (IsColliding(tankRect, wall.Bounds))
                    return true;
            }
            return false;
        }
    }
}