using System;

namespace TNoodle.Core
{
    public class InvalidMoveException : Exception
    {
        public InvalidMoveException(string move) : base("Invalid move: " + move) { }
    }
}
