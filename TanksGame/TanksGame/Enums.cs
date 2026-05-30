using System;

namespace TanksGame
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum GameState
    {
        Playing,
        GameOver
    }

    public enum TankType
    {
        Player,
        Enemy
    }
}