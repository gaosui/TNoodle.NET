namespace TNoodle.Core
{
    public class PuzzleStateAndGenerator
    {
        public Puzzle.PuzzleState state;
        public string generator;
        public PuzzleStateAndGenerator(Puzzle.PuzzleState state, string generator)
        {
            this.state = state;
            this.generator = generator;
        }
    }
}
