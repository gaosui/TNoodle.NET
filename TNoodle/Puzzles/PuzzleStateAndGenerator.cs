namespace TNoodle.Puzzles
{
    public class PuzzleStateAndGenerator
    {
        public PuzzleState state;
        public string generator;
        public PuzzleStateAndGenerator(PuzzleState state, string generator)
        {
            this.state = state;
            this.generator = generator;
        }
    }
}
