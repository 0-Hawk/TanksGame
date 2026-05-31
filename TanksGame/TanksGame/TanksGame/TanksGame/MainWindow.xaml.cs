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

            _engine = new GameEngine(_input, ShowGameOver);

            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromMilliseconds(16);
            _gameTimer.Tick += GameLoop;

            TxtHighScore.Text = _engine.HighScore.ToString();

            StartNewGame();
        }

        private void StartNewGame()
        {
            GameOverOverlay.Visibility = Visibility.Collapsed;
            _engine.StartGame();
            _gameTimer.Start();

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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_engine.State == GameState.Playing)
            {
                _input.UpdateKey(e.Key, true);

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
        }
    }
}