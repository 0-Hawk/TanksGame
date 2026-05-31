using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TanksGame
{
    public class Bullet
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Direction Direction { get; set; }
        public double Speed { get; set; } = 10;
        public double Size { get; set; } = 6;
        public TankType Owner { get; set; }
        public bool IsActive { get; set; } = true;

        public Rect Bounds => new Rect(X, Y, Size, Size);

        public void Move()
        {
            switch (Direction)
            {
                case Direction.Up: Y -= Speed; break;
                case Direction.Down: Y += Speed; break;
                case Direction.Left: X -= Speed; break;
                case Direction.Right: X += Speed; break;
            }
        }
    }

    public class Wall
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; } = 40;
        public double Height { get; set; } = 40;
        public bool IsDestructible { get; set; } = false;

        public Rect Bounds => new Rect(X, Y, Width, Height);

        public ImageSource Sprite { get; set; }
    }

    public abstract class Tank
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Direction Direction { get; set; } = Direction.Up;
        public double Speed { get; set; } = 4;
        public double Size { get; set; } = 36;
        public bool IsAlive { get; set; } = true;

        protected int ShootCooldown { get; set; } = 0;
        protected const int MaxCooldown = 30;

        public Rect Bounds => new Rect(X, Y, Size, Size);

        public virtual void Move(Direction dir, System.Collections.Generic.List<Wall> walls, double fieldSize)
        {
            Direction = dir;
            double nextX = X;
            double nextY = Y;

            switch (dir)
            {
                case Direction.Up: nextY -= Speed; break;
                case Direction.Down: nextY += Speed; break;
                case Direction.Left: nextX -= Speed; break;
                case Direction.Right: nextX += Speed; break;
            }

            if (nextX < 0) nextX = 0;
            if (nextY < 0) nextY = 0;
            if (nextX + Size > fieldSize) nextX = fieldSize - Size;
            if (nextY + Size > fieldSize) nextY = fieldSize - Size;

            Rect nextBounds = new Rect(nextX, nextY, Size, Size);
            if (!CollisionDetector.CheckWallCollision(nextBounds, walls))
            {
                X = nextX;
                Y = nextY;
            }
        }

        public Bullet Shoot(TankType owner)
        {
            if (ShootCooldown > 0) return null;

            ShootCooldown = MaxCooldown;

            double bx = X + Size / 2 - 3;
            double by = Y + Size / 2 - 3;

            return new Bullet
            {
                X = bx,
                Y = by,
                Direction = this.Direction,
                Owner = owner
            };
        }

        public void UpdateCooldown()
        {
            if (ShootCooldown > 0) ShootCooldown--;
        }
    }

    public class PlayerTank : Tank
    {
        public int InvincibleFrames { get; set; } = 0;

        public void TakeDamage()
        {
            if (InvincibleFrames <= 0)
            {
                InvincibleFrames = 60;
            }
        }

        public void UpdateInvincibility()
        {
            if (InvincibleFrames > 0) InvincibleFrames--;
        }
    }

    public class EnemyTank : Tank
    {
        public int AIChangeDirTimer { get; set; } = 0;
        public Random Random { get; set; } = new Random();

        public void UpdateAI(System.Collections.Generic.List<Wall> walls, double fieldSize, PlayerTank player)
        {
            UpdateCooldown();

            if (AIChangeDirTimer <= 0)
            {
                DecideDirection(player);
                AIChangeDirTimer = Random.Next(30, 90);
            }
            else
            {
                AIChangeDirTimer--;
            }

            double oldX = X;
            double oldY = Y;
            Move(Direction, walls, fieldSize);

            if (Math.Abs(X - oldX) < 0.1 && Math.Abs(Y - oldY) < 0.1)
            {
                DecideDirection(player);
            }

            if (Random.Next(0, 100) < 2)
            {
            }
        }

        private void DecideDirection(PlayerTank player)
        {
            if (Random.Next(0, 100) < 30 && player.IsAlive)
            {
                if (Math.Abs(player.X - X) > Math.Abs(player.Y - Y))
                {
                    Direction = player.X > X ? Direction.Right : Direction.Left;
                }
                else
                {
                    Direction = player.Y > Y ? Direction.Down : Direction.Up;
                }
            }
            else
            {
                Direction = (Direction)Random.Next(0, 4);
            }
        }

        public Bullet TryShoot()
        {
            if (ShootCooldown == 0)
            {
                return Shoot(TankType.Enemy);
            }
            return null;
        }
    }
}