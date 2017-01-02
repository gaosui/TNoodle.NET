using static TNoodle.Puzzles.Puzzle;

namespace TNoodle.Puzzles
{
    public class PuzzleStateAndGenerator
    {
        public PuzzleState State { get; }
        public string Generator { get; }

        public PuzzleStateAndGenerator(PuzzleState state, string generator)
        {
            State = state;
            Generator = generator;
        }
    }
}
