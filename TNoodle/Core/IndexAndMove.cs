using System;
namespace TNoodle.Core
{
    public class IndexAndMove
    {
        public int index;
        public string move;
        public IndexAndMove(int index, string move)
        {
            this.index = index;
            this.move = move;
        }
        public string toString()
        {
            return "{ index: " + index + " move: " + move + " }";
        }
    }
}