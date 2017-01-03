using System;

namespace TNoodle.Puzzles
{
    public class InvalidMoveException : Exception
    {
        public InvalidMoveException(string move) : base("Invalid move: " + move)
        {
        }
    }
}