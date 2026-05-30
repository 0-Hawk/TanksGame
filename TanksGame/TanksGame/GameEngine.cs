using System;
using System.Collections.Generic;
using System.IO; // Нужно для работы с файлами
using System.Linq;
using System.Windows;

namespace TanksGame
{
    public class GameEngine
    {
        // Состояние
        public GameState State { get; private set; } = GameState.Playing;
        public int Score { get; private set; } = 0;
        public int Lives { get; private set; } = 3;

        // Рекорд теперь хранится в обычном поле, но загружается/сохраняется в файл
        public int HighScore { get; private set; } = 0;

        // Объекты
        public PlayerTank PlayerTank { get; private set; }
        public List<EnemyTank> EnemyTanks { get; private set; } = new List<EnemyTank>();
        public List<Bullet> Bullets { get; private set; } = new List<Bullet>();
        public GameWorld World { get; private set; }

        // Зависимости
        private InputManager _input;
        private Action<string> _onGameOver;

        private Random _rnd = new Random();

        // Путь к файлу сохранения (в папке с исполняемым файлом)
        private string _saveFilePath = "highscore.txt";

        public GameEngine(InputManager input, Action<string> onGameOver)
        {
            _input = input;
            _onGameOver = onGameOver;
            World = new GameWorld();

            // Загружаем рекорд из файла при создании движка
            LoadHighScore();
        }

        // Метод загрузки рекорда из файла
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
                // Если ошибка чтения (файл занят или поврежден), просто оставляем 0
                HighScore = 0;
            }
        }

        // Метод сохранения рекорда в файл
        private void SaveHighScore()
        {
            try
            {
                File.WriteAllText(_saveFilePath, HighScore.ToString());
            }
            catch (Exception ex)
            {
                // Можно вывести ошибку, если нужно, но лучше не ломать игру
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

            // Спавн игрока (центр)
            PlayerTank = new PlayerTank
            {
                X = World.Width / 2 - 18,
                Y = World.Height / 2 - 18,
                Direction = Direction.Up
            };

            // Спавн врагов
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

            // 1. Обновление игрока
            if (PlayerTank.IsAlive)
            {
                PlayerTank.UpdateInvincibility();
                PlayerTank.UpdateCooldown();

                // Движение
                if (_input.IsUpPressed) PlayerTank.Move(Direction.Up, World.Walls, World.Width);
                else if (_input.IsDownPressed) PlayerTank.Move(Direction.Down, World.Walls, World.Width);
                else if (_input.IsLeftPressed) PlayerTank.Move(Direction.Left, World.Walls, World.Width);
                else if (_input.IsRightPressed) PlayerTank.Move(Direction.Right, World.Walls, World.Width);

                // Стрельба
                if (_input.IsShootPressed)
                {
                    var bullet = PlayerTank.Shoot(TankType.Player);
                    if (bullet != null) Bullets.Add(bullet);
                }
            }

            // 2. Обновление врагов
            foreach (var enemy in EnemyTanks)
            {
                if (enemy.IsAlive)
                {
                    enemy.UpdateAI(World.Walls, World.Width, PlayerTank);
                    var bullet = enemy.TryShoot();
                    if (bullet != null) Bullets.Add(bullet);
                }
            }

            // 3. Обновление пуль и коллизии
            UpdateBullets();

            // 4. Проверка конца игры
            if (Lives <= 0)
            {
                EndGame();
            }

            // Респавн врагов если всех убили
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

                // Выход за границы
                if (b.X < 0 || b.X > World.Width || b.Y < 0 || b.Y > World.Height)
                {
                    b.IsActive = false;
                }

                // Столкновение со стенами
                if (CollisionDetector.CheckWallCollision(b.Bounds, World.Walls))
                {
                    b.IsActive = false;
                }

                // Столкновение с танками
                if (b.IsActive)
                {
                    // Игрок
                    if (b.Owner == TankType.Enemy && PlayerTank.IsAlive && PlayerTank.InvincibleFrames <= 0)
                    {
                        if (CollisionDetector.IsColliding(b.Bounds, PlayerTank.Bounds))
                        {
                            b.IsActive = false;
                            PlayerTank.TakeDamage();
                            Lives--;
                        }
                    }

                    // Враги
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

            // Обновляем рекорд, если текущий счет выше
            if (Score > HighScore)
            {
                HighScore = Score;
                // Сохраняем в файл сразу же
                SaveHighScore();
            }

            _onGameOver?.Invoke($"Game Over! Score: {Score}");
        }
    }
}