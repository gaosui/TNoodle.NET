using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNoodle.Core
{
    public enum MergingMode
    {
        // There are several degrees of manipulation we can choose to do
        // while building an algorithm. Here they are, ranging from least to
        // most aggressive. Examples are on a 3x3x3.

        // Straightforward, blindly append moves.
        // For example:
        //  - "R R" stays unmodified.
        NO_MERGING,

        // Merge together redundant moves, but preserve the exact state
        // of the puzzle (unlike CANONICALIZE_MOVES).
        // In other words, the resulting state will be the
        // same as if we had used NO_MERGING.
        // For example:
        //  - "R R" becomes "R2"
        //  - "L Rw" stays unmodified.
        //  - "F x U" will become something like "F2 x".
        //  TODO - add actual support for this! feel free to rename it
        //MERGE_REDUNDANT_MOVES_PRESERVE_STATE,

        // Most aggressive merging.
        // See PuzzleState.getCanonicalMovesByState() for the
        // definition of "canonical" moves.
        // Canonical moves will not necessarily let us preserve the
        // exact state we would have achieved with NO_MERGING. This is
        // because canonical moves may not let us rotate the puzzle.
        // However, the resulting state when normalized will be the
        // same as the normalization of the state we would have
        // achieved if we had used NO_MERGING.
        // For example:
        //  - "R R" becomes "R2"
        //  - "L Rw" becomes "L2"
        //  - "F x U" becomes "F2"
        CANONICALIZE_MOVES
    }
}
