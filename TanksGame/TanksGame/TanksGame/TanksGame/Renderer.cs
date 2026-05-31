using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TanksGame
{
    public class Renderer
    {
        private Canvas _canvas;

        private Dictionary<string, ImageSource> _sprites = new Dictionary<string, ImageSource>();

        public Renderer(Canvas canvas)
        {
            _canvas = canvas;
            LoadSprites();
        }

        private void LoadSprites()
        {
            string[] names = {
                "player_up", "player_down", "player_left", "player_right",
                "enemy_up", "enemy_down", "enemy_left", "enemy_right",
                "wall", "bullet"
            };

            foreach (var name in names)
            {
                try
                {
                    var uri = new Uri($"pack://application:,,,/Assets/{name}.png");
                    _sprites[name] = new BitmapImage(uri);
                }
                catch {}
            }
        }

        public void ClearCanvas()
        {
            _canvas.Children.Clear();
        }

        public void Draw(GameEngine engine)
        {
            ClearCanvas();

            foreach (var wall in engine.World.Walls)
            {
                DrawWall(wall);
            }

            foreach (var bullet in engine.Bullets)
            {
                if (bullet.IsActive) DrawBullet(bullet);
            }

            if (engine.PlayerTank.IsAlive)
            {
                DrawTank(engine.PlayerTank, TankType.Player);
            }

            foreach (var enemy in engine.EnemyTanks)
            {
                if (enemy.IsAlive)
                {
                    DrawTank(enemy, TankType.Enemy);
                }
            }
        }

        private void DrawWall(Wall wall)
        {
            if (_sprites.ContainsKey("wall") && _sprites["wall"] != null)
            {
                var img = new Image
                {
                    Source = _sprites["wall"],
                    Width = wall.Width,
                    Height = wall.Height
                };
                Canvas.SetLeft(img, wall.X);
                Canvas.SetTop(img, wall.Y);
                _canvas.Children.Add(img);
            }
            else
            {
                var rect = new Rectangle
                {
                    Width = wall.Width,
                    Height = wall.Height,
                    Fill = Brushes.Gray
                };
                Canvas.SetLeft(rect, wall.X);
                Canvas.SetTop(rect, wall.Y);
                _canvas.Children.Add(rect);
            }
        }

        private void DrawTank(Tank tank, TankType type)
        {
            string key = $"{type.ToString().ToLower()}_{tank.Direction.ToString().ToLower()}";

            if (type == TankType.Player && tank is PlayerTank pt && pt.InvincibleFrames > 0)
            {
                if ((pt.InvincibleFrames / 5) % 2 == 0) return; 
            }

            if (_sprites.ContainsKey(key) && _sprites[key] != null)
            {
                var img = new Image
                {
                    Source = _sprites[key],
                    Width = tank.Size,
                    Height = tank.Size
                };
                Canvas.SetLeft(img, tank.X);
                Canvas.SetTop(img, tank.Y);
                _canvas.Children.Add(img);
            }
            else
            {
                var rect = new Rectangle
                {
                    Width = tank.Size,
                    Height = tank.Size,
                    Fill = type == TankType.Player ? Brushes.Green : Brushes.Red
                };
                Canvas.SetLeft(rect, tank.X);
                Canvas.SetTop(rect, tank.Y);
                _canvas.Children.Add(rect);

                var indicator = new Rectangle { Width = 4, Height = 4, Fill = Brushes.White };
                double ix = tank.X + tank.Size / 2 - 2;
                double iy = tank.Y + tank.Size / 2 - 2;
                if (tank.Direction == Direction.Up) iy -= 10;
                if (tank.Direction == Direction.Down) iy += 10;
                if (tank.Direction == Direction.Left) ix -= 10;
                if (tank.Direction == Direction.Right) ix += 10;

                Canvas.SetLeft(indicator, ix);
                Canvas.SetTop(indicator, iy);
                _canvas.Children.Add(indicator);
            }
        }

        private void DrawBullet(Bullet bullet)
        {
            if (_sprites.ContainsKey("bullet") && _sprites["bullet"] != null)
            {
                var img = new Image
                {
                    Source = _sprites["bullet"],
                    Width = bullet.Size,
                    Height = bullet.Size
                };
                Canvas.SetLeft(img, bullet.X);
                Canvas.SetTop(img, bullet.Y);
                _canvas.Children.Add(img);
            }
            else
            {
                var ellipse = new Ellipse
                {
                    Width = bullet.Size,
                    Height = bullet.Size,
                    Fill = Brushes.Yellow
                };
                Canvas.SetLeft(ellipse, bullet.X);
                Canvas.SetTop(ellipse, bullet.Y);
                _canvas.Children.Add(ellipse);
            }
        }
    }
}