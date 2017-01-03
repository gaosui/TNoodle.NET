using System;
using static TNoodle.Puzzles.AlgorithmBuilder;
using static TNoodle.Utils.Assertion;

namespace TNoodle.Puzzles
{
    public class ThreeByThreeCubeFewestMovesPuzzle : ThreeByThreeCubePuzzle
    {
        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            // For fewest moves, we want to minimize the probability that the
            // scramble has useful "stuff" in it. The problem with conventional
            // Kociemba 2 phase solutions is that there's a pretty obvious
            // orientation step, which competitors might intentionally (or even
            // accidentally!) use in their solution. To lower the probability of this happening,
            // we intentionally generate dumbed down solutions.

            // We eventually decided to go with "Tom2", which is described by Tom
            // Rokicki (https://groups.google.com/d/msg/wca-admin/vVnuhk92hqg/P5oaJJQjDQAJ):

            // START TOM MESSAGE
            // If we're going this direction, and the exact length isn't critical, why not combine a bunch of the
            // ideas we've seen so far into something this simple:
            //
            // 1. Fix a set of prefix/suffix moves, say, U F R / U F R.
            // 2. For a given position p, find *any* solution where that solution prefixed and suffixed with
            //    the appropriate moves is canonical.
            //
            // That's it.  So we generate a random position, then find any two-phase solution g such
            // that U F R g U F R is a canonical sequence, and that's our FMC scramble.
            //
            // The prefix/suffix will be easily recognizable and memorable and ideally finger-trick
            // friendly (if it matters).
            //
            // Someone wanting to practice speed FMC (hehe) can make up their own scrambles just
            // by manually doing the prefix/suffix thing on any existing scrambler.
            //
            // It does not perturb the uniformity of the random position selection.
            //
            // It's simple enough that it is less likely to suffer from subtle defects due to the
            // perturbation of the two-phase search (unlikely those these may be).
            //
            // And even if the two-phase solution is short, adding U F R to the front and back makes it
            // no longer be unusually short (with high probability).
            // END TOM MESSAGE

            // Michael Young suggested using R' U' F as our padding (https://groups.google.com/d/msg/wca-admin/vVnuhk92hqg/EzQfG_vPBgAJ):

            // START MICHAEL MESSSAGE
            // I think that something more like R' U' F (some sequence that
            // involves a quarter-turn on all three "axes") is better because it
            // guarantees at least one bad edge in every orientation; with EO-first
            // options becoming more popular, "guaranteeing" that solving EO
            // optimally can't be derived from the scramble is a nice thing to
            // have, I think.  (Someone who was both unscrupulous and lucky could
            // see that R' F R doesn't affect U2/D2 EO, and therefore find out how
            // to solve EO by looking at the solution ignoring the R' F R.  That
            // being said, it still does change things, and I like how
            // finger-tricky/reversible the current prefix is.)  Just my two cents,
            // 'tho.
            // END MICHAEL MESSSAGE
            var scramblePrefix = SplitAlgorithm("R' U' F");
            var scrambleSuffix = SplitAlgorithm("R' U' F");

            // super.generateRandomMoves(...) will pick a random state S and find a solution:
            //  solution = sol_0, sol_1, ..., sol_n-1, sol_n
            // We then invert that solution to create a scramble:
            //  scramble = sol_n' + sol_(n-1)' + ... + sol_1' + sol_0'
            // We then prefix the scramble with scramblePrefix and suffix it with
            // scrambleSuffix to create paddedScramble:
            //  paddedScramble = scramblePrefix + scramble + scrambleSuffix
            //  paddedScramble = scramblePrefix + (sol_n' + sol_(n-1)' + ... + sol_1' + sol_0') + scrambleSuffix
            //
            // We don't want any moves to cancel here, so we need to make sure that
            // sol_n' doesn't cancel with the first move of scramblePrefix:
            var solutionLastAxisRestriction = scramblePrefix[scramblePrefix.Length - 1].Substring(0, 1);
            // and we need to make sure that sol_0' doesn't cancel with the first move of
            // scrambleSuffix:
            var solutionFirstAxisRestriction = scrambleSuffix[0].Substring(0, 1);
            var psag = GenerateRandomMoves(r, solutionFirstAxisRestriction, solutionLastAxisRestriction);
            var ab = new AlgorithmBuilder(MergingMode.NoMerging, GetSolvedState());
            try
            {
                ab.AppendAlgorithms(scramblePrefix);
                ab.AppendAlgorithm(psag.Generator);
                ab.AppendAlgorithms(scrambleSuffix);
            }
            catch (InvalidMoveException e)
            {
                Assert(false, e.Message, e);
                return null;
            }
            return ab.GetStateAndGenerator();
        }

        public override string GetShortName()
        {
            return "333fm";
        }

        public override string GetLongName()
        {
            return "3x3x3 Fewest Moves";
        }
    }
}