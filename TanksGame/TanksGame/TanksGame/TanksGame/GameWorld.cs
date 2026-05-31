using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TanksGame
{
    public class GameWorld
    {
        public double Width { get; set; } = 600;
        public double Height { get; set; } = 600;
        public List<Wall> Walls { get; set; } = new List<Wall>();
        public List<Point> SpawnPoints { get; set; } = new List<Point>();

        public void LoadLevel(int levelNumber)
        {
            Walls.Clear();
            SpawnPoints.Clear();

            ImageSource wallSprite = null;
            try { wallSprite = new BitmapImage(new Uri("pack://application:,,,/Assets/wall.png")); } catch { }

            Random rnd = new Random();
            int cellSize = 40;
            int cols = (int)Width / cellSize;
            int rows = (int)Height / cellSize;

            for (int i = 0; i < 25; i++)
            {
                int x = rnd.Next(1, cols - 1) * cellSize;
                int y = rnd.Next(1, rows - 1) * cellSize;

                if (IsSafeZone(x, y)) continue;

                Walls.Add(new Wall
                {
                    X = x,
                    Y = y,
                    Sprite = wallSprite
                });
            }

            SpawnPoints.Add(new Point(20, 20));
            SpawnPoints.Add(new Point(Width - 60, 20));
            SpawnPoints.Add(new Point(Width / 2, Height - 60));
        }

        private bool IsSafeZone(int x, int y)
        {
            if (x > 250 && x < 350 && y > 250 && y < 350) return true;
            if (x < 80 && y < 80) return true;
            if (x > 500 && y < 80) return true;
            if (x > 250 && x < 350 && y > 500) return true;

            return false;
        }
    }
}