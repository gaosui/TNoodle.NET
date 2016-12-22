using System;

namespace TNoodle.Core
{
    public class InvalidScrambleException : Exception
    {
        public InvalidScrambleException(string scramble) : base(scramble, null)
        {
        }
        public InvalidScrambleException(string scramble, Exception t) : base("Invalid scramble: " + scramble, t)
        {
        }
    }
}
