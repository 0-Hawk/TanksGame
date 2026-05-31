using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace TanksGame
{
    public class GameEngine
    {
        public GameState State { get; private set; } = GameState.Playing;
        public int Score { get; private set; } = 0;
        public int Lives { get; private set; } = 3;

        public int HighScore { get; private set; } = 0;

        public PlayerTank PlayerTank { get; private set; }
        public List<EnemyTank> EnemyTanks { get; private set; } = new List<EnemyTank>();
        public List<Bullet> Bullets { get; private set; } = new List<Bullet>();
        public GameWorld World { get; private set; }

        private InputManager _input;
        private Action<string> _onGameOver;

        private Random _rnd = new Random();

        private string _saveFilePath = "highscore.txt";

        public GameEngine(InputManager input, Action<string> onGameOver)
        {
            _input = input;
            _onGameOver = onGameOver;
            World = new GameWorld();

            LoadHighScore();
        }

        private void LoadHighScore()
        {
            try
            {
                if (File.Exists(_saveFilePath))
                {
                    string content = File.ReadAllText(_saveFilePath);
                    if (int.TryParse(content, out int savedScore))
                    {
                        HighScore = savedScore;
                    }
                }
            }
            catch (Exception)
            {
                HighScore = 0;
            }
        }

        private void SaveHighScore()
        {
            try
            {
                File.WriteAllText(_saveFilePath, HighScore.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        public void StartGame()
        {
            State = GameState.Playing;
            Score = 0;
            Lives = 3;
            Bullets.Clear();
            EnemyTanks.Clear();

            World.LoadLevel(1);

            PlayerTank = new PlayerTank
            {
                X = World.Width / 2 - 18,
                Y = World.Height / 2 - 18,
                Direction = Direction.Up
            };

            foreach (var point in World.SpawnPoints)
            {
                EnemyTanks.Add(new EnemyTank
                {
                    X = point.X,
                    Y = point.Y,
                    Direction = Direction.Down
                });
            }
        }

        public void UpdateGame()
        {
            if (State != GameState.Playing) return;

            if (PlayerTank.IsAlive)
            {
                PlayerTank.UpdateInvincibility();
                PlayerTank.UpdateCooldown();

                if (_input.IsUpPressed) PlayerTank.Move(Direction.Up, World.Walls, World.Width);
                else if (_input.IsDownPressed) PlayerTank.Move(Direction.Down, World.Walls, World.Width);
                else if (_input.IsLeftPressed) PlayerTank.Move(Direction.Left, World.Walls, World.Width);
                else if (_input.IsRightPressed) PlayerTank.Move(Direction.Right, World.Walls, World.Width);

                if (_input.IsShootPressed)
                {
                    var bullet = PlayerTank.Shoot(TankType.Player);
                    if (bullet != null) Bullets.Add(bullet);
                }
            }

            foreach (var enemy in EnemyTanks)
            {
                if (enemy.IsAlive)
                {
                    enemy.UpdateAI(World.Walls, World.Width, PlayerTank);
                    var bullet = enemy.TryShoot();
                    if (bullet != null) Bullets.Add(bullet);
                }
            }

            UpdateBullets();

            if (Lives <= 0)
            {
                EndGame();
            }

            if (EnemyTanks.All(e => !e.IsAlive))
            {
                RespawnEnemies();
            }
        }

        private void UpdateBullets()
        {
            for (int i = Bullets.Count - 1; i >= 0; i--)
            {
                var b = Bullets[i];
                b.Move();

                if (b.X < 0 || b.X > World.Width || b.Y < 0 || b.Y > World.Height)
                {
                    b.IsActive = false;
                }

                if (CollisionDetector.CheckWallCollision(b.Bounds, World.Walls))
                {
                    b.IsActive = false;
                }

                if (b.IsActive)
                {
                    if (b.Owner == TankType.Enemy && PlayerTank.IsAlive && PlayerTank.InvincibleFrames <= 0)
                    {
                        if (CollisionDetector.IsColliding(b.Bounds, PlayerTank.Bounds))
                        {
                            b.IsActive = false;
                            PlayerTank.TakeDamage();
                            Lives--;
                        }
                    }

                    if (b.Owner == TankType.Player)
                    {
                        foreach (var enemy in EnemyTanks)
                        {
                            if (enemy.IsAlive && CollisionDetector.IsColliding(b.Bounds, enemy.Bounds))
                            {
                                b.IsActive = false;
                                enemy.IsAlive = false;
                                Score += 100;
                                break;
                            }
                        }
                    }
                }

                if (!b.IsActive)
                {
                    Bullets.RemoveAt(i);
                }
            }
        }

        private void RespawnEnemies()
        {
            EnemyTanks.Clear();
            foreach (var point in World.SpawnPoints)
            {
                EnemyTanks.Add(new EnemyTank
                {
                    X = point.X,
                    Y = point.Y,
                    Direction = Direction.Down
                });
            }
        }

        private void EndGame()
        {
            State = GameState.GameOver;

            if (Score > HighScore)
            {
                HighScore = Score;
                SaveHighScore();
            }

            _onGameOver?.Invoke($"Game Over! Score: {Score}");
        }
    }
}