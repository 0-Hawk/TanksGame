using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace TanksGame
{
    public partial class MainWindow : Window
    {
        private GameEngine _engine;
        private InputManager _input;
        private Renderer _renderer;
        private DispatcherTimer _gameTimer;

        public MainWindow()
        {
            InitializeComponent();

            _input = new InputManager();
            _renderer = new Renderer(GameCanvas);

            // Инициализация движка
            _engine = new GameEngine(_input, ShowGameOver);

            // Настройка таймера (60 FPS)
            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromMilliseconds(16);
            _gameTimer.Tick += GameLoop;

            // Загрузка рекорда
            TxtHighScore.Text = _engine.HighScore.ToString();

            StartNewGame();
        }

        private void StartNewGame()
        {
            GameOverOverlay.Visibility = Visibility.Collapsed;
            _engine.StartGame();
            _gameTimer.Start();

            // ВАЖНО: Принудительно ставим фокус на Canvas.
            // Теперь все нажатия клавиш будут идти сюда.
            Keyboard.Focus(GameCanvas);
        }

        private void GameLoop(object sender, EventArgs e)
        {
            _engine.UpdateGame();
            _renderer.Draw(_engine);
            UpdateUI();
        }

        private void UpdateUI()
        {
            TxtScore.Text = _engine.Score.ToString();
            TxtLives.Text = _engine.Lives.ToString();
            TxtHighScore.Text = _engine.HighScore.ToString();
        }

        // Обработка ввода
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Обрабатываем только если игра идет
            if (_engine.State == GameState.Playing)
            {
                _input.UpdateKey(e.Key, true);

                // Помечаем игровые клавиши как обработанные, 
                // чтобы они точно не всплывали ни к каким другим элементам
                if (IsGameKey(e.Key))
                {
                    e.Handled = true;
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (_engine.State == GameState.Playing)
            {
                _input.UpdateKey(e.Key, false);

                if (IsGameKey(e.Key))
                {
                    e.Handled = true;
                }
            }
        }

        // Вспомогательный метод для проверки игровых клавиш
        private bool IsGameKey(Key key)
        {
            return key == Key.Space ||
                   key == Key.Up || key == Key.Down || key == Key.Left || key == Key.Right ||
                   key == Key.W || key == Key.S || key == Key.A || key == Key.D;
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }

        private void ShowGameOver(string message)
        {
            _gameTimer.Stop();
            TxtGameOverMsg.Text = message;
            GameOverOverlay.Visibility = Visibility.Visible;
            TxtHighScore.Text = _engine.HighScore.ToString();

            // При Game Over фокус не так важен, так как игра остановлена,
            // но можно оставить его на Canvas или переключить на кнопку "Играть снова",
            // если хотим чтобы Enter нажимал рестарт. 
            // Но с Focusable="False" у кнопок, Enter тоже не сработает на них, 
            // что даже лучше для единообразия управления (только мышь для UI).
        }
    }
}