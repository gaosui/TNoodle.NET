using System;
using System.Text;
using TNoodle.Utils;
using static TNoodle.Utils.Assertion;

namespace TNoodle.Solvers
{
    public class PyraminxSolver
    {
        /** There are 4 corners on the pyraminx that are in a fixed position.
          * There are 3 different orientations for each corner.
          *
          *                         U
          *     ____  ____  ____          ____  ____  ____
          *    \    /\    /\    /   /\   \    /\    /\    /
          *     \  /3 \  /0 \  /   /  \   \  /0 \  /3 \  /
          *      \/____\/____\/   /____\   \/____\/____\/
          *       \    /\    /   /\    /\   \    /\    /
          *        \  /1 \  /   /  \0 /  \   \  /2 \  /
          *         \/____\/   /____\/____\   \/____\/
          *          \    /   /\    /\    /\   \    /
          *           \  /   /  \1 /  \2 /  \   \  /
          *            \/   /____\/____\/____\   \/
          *               L                    R
          *                  ____  ____  ____
          *                 \    /\    /\    /
          *                  \  /1 \  /2 \  /
          *                   \/____\/____\/
          *                    \    /\    /
          *                     \  /3 \  /
          *                      \/____\/
          *                       \    /
          *                        \  /
          *                         \/
          *
          *                         B
          *
          * There are 6 edges, each one having 2 different orientations.
          * Dollars mark the primary facelet position.
          *                         U
          *     ____  ____  ____          ____  ____  ____
          *    \    /\    /\    /   /\   \    /\    /\    /
          *     \  /  \5$/  \  /   /  \   \  /  \5 /  \  /
          *      \/____\/____\/   /____\   \/____\/____\/
          *       \    /\    /   /\    /\   \    /\    /
          *        \2 /  \1 /   /1$\  /3$\   \3 /  \4 /
          *         \/____\/   /____\/____\   \/____\/
          *          \    /   /\    /\    /\   \    /
          *           \  /   /  \  /0$\  /  \   \  /
          *            \/   /____\/____\/____\   \/
          *               L                    R
          *                  ____  ____  ____
          *                 \    /\    /\    /
          *                  \  /  \0 /  \  /
          *                   \/____\/____\/
          *                    \    /\    /
          *                     \2$/  \4$/
          *                      \/____\/
          *                       \    /
          *                        \  /
          *                         \/
          *
          *                         B
          */
        private const int NEdgePerm = 720; // Number of permutations of edges
        private const int NEdgeOrient = 32; // Number of orientations of edges
        private const int NCornerOrient = 81; // Number of orientations of corners
        private const int NOrient = NEdgeOrient * NCornerOrient;
        private const int NTips = 81; // Number of tips positions
        private const int NMoves = 8; // Number of moves
        private const int MaxLength = 20;
        private static readonly string[] MoveToString = {"U", "U'", "L", "L'", "R", "R'", "B", "B'"};
        private static readonly string[] InverseMoveToString = {"U'", "U", "L'", "L", "R'", "R", "B'", "B"};
        private static readonly string[] TipToString = {"u", "u'", "l", "l'", "r", "r'", "b", "b'"};
        private static readonly string[] InverseTipToString = {"u'", "u", "l'", "l", "r'", "r", "b'", "b"};

        private static readonly int[] Fact = {1, 1, 2, 6, 24, 120, 720}; // fact[x] = x!

        /**
         * Converts the list of edges into a number representing the permutation of the edges.
         * @param edges   edges representation (ori << 3 + perm)
         * @return        an integer between 0 and 719 representing the permutation of 6 elements
         */

        public static int PackEdgePerm(int[] edges)
        {
            var idx = 0;
            var val = 0x543210;
            for (var i = 0; i < 5; i++)
            {
                var v = (edges[i] & 0x7) << 2;
                idx = (6 - i) * idx + ((val >> v) & 0x7);
                val -= 0x111110 << v;
            }
            return idx;
        }

        /**
         * Converts an integer representing a permutation of 6 elements into a list of edges.
         * @param perm     an integer between 0 and 719 representing the permutation of 6 elements
         * @param edges    edges representation (ori << 3 + perm)
         */

        private static void UnpackEdgePerm(int perm, int[] edges)
        {
            var val = 0x543210;
            for (var i = 0; i < 5; i++)
            {
                var p = Fact[5 - i];
                var v = perm / p;
                perm -= v * p;
                v <<= 2;
                edges[i] = (val >> v) & 0x7;
                var m = (1 << v) - 1;
                val = (val & m) + ((val >> 4) & ~m);
            }
            edges[5] = val;
        }

        /**
         * Converts the list of edges into a number representing the orientation of the edges.
         * @param edges    edges representation (ori << 3 + perm)
         * @return         an integer between 0 and 31 representing the orientation of 5 elements (the 6th is fixed)
         */

        public static int PackEdgeOrient(int[] edges)
        {
            var ori = 0;
            for (var i = 0; i < 5; i++)
            {
                ori = 2 * ori + (edges[i] >> 3);
            }
            return ori;
        }

        /**
         * Converts an integer representing the orientation of 5 elements into a list of cubies.
         * @param ori      an integer between 0 and 31 representing the orientation of 5 elements (the 6th is fixed)
         * @param edges    edges representation (ori << 3 + perm)
         */

        private static void UnpackEdgeOrient(int ori, int[] edges)
        {
            var sumOri = 0;
            for (var i = 4; i >= 0; i--)
            {
                edges[i] = (ori & 1) << 3;
                sumOri ^= ori & 1;
                ori >>= 1;
            }
            edges[5] = sumOri << 3;
        }

        /**
         * Converts the list of corners into a number representing the orientation of the corners.
         * @param corners   corner representation
         * @return          an integer between 0 and 80 representing the orientation of 4 elements
         */

        public static int PackCornerOrient(int[] corners)
        {
            var ori = 0;
            for (var i = 0; i < 4; i++)
            {
                ori = 3 * ori + corners[i];
            }
            return ori;
        }

        /**
         * Converts an integer representing the orientation of 4 elements into a list of corners.
         * @param ori       an integer between 0 and 80 representing the orientation of 4 elements
         * @param corners   corners representation
         */

        private static void UnpackCornerOrient(int ori, int[] corners)
        {
            for (var i = 3; i >= 0; i--)
            {
                corners[i] = ori % 3;
                ori /= 3;
            }
        }

        /**
         * Cycle three elements of an array. Also orient first and second elements.
         * @param edges    edges representation (ori << 3 + perm)
         * @param a        first element to cycle
         * @param b        second element to cycle
         * @param c        third element to cycle
         * @param times    number of times to cycle
         */

        private static void CycleAndOrient(int[] edges, int a, int b, int c, int times)
        {
            var temp = edges[c];
            edges[c] = (edges[b] + 8) % 16;
            edges[b] = (edges[a] + 8) % 16;
            edges[a] = temp;
            if (times > 1)
            {
                CycleAndOrient(edges, a, b, c, times - 1);
            }
        }

        /**
         * Apply a move on the edges representation.
         * @param edges    edges representation (ori << 3 + perm)
         * @param move     move to apply to the edges
         */

        private static void MoveEdges(int[] edges, int move)
        {
            var face = move / 2;
            var times = (move % 2) + 1;
            switch (face)
            {
                case 0: // U face
                    CycleAndOrient(edges, 5, 3, 1, times);
                    break;
                case 1: // L face
                    CycleAndOrient(edges, 2, 1, 0, times);
                    break;
                case 2: // R face
                    CycleAndOrient(edges, 0, 3, 4, times);
                    break;
                case 3: // B face
                    CycleAndOrient(edges, 2, 4, 5, times);
                    break;
                default:
                    Assert(false);
                    break;
            }
        }

        /**
         * Apply a move on the corners representation.
         * @param corners  corners representation
         * @param move     move to apply to the corners
         */

        private static void MoveCorners(int[] corners, int move)
        {
            var face = move / 2;
            var times = (move % 2) + 1;
            corners[face] = (corners[face] + times) % 3;
        }

        /**
         * Fill the arrays to move permutation and orientation coordinates.
         */
        public static int[][] MoveEdgePerm { get; } = ArrayExtension.New<int>(NEdgePerm, NMoves);
        public static int[][] MoveEdgeOrient { get; } = ArrayExtension.New<int>(NEdgeOrient, NMoves);
        public static int[][] MoveCornerOrient { get; } = ArrayExtension.New<int>(NCornerOrient, NMoves);

        private static void InitMoves()
        {
            var edges1 = new int[6];
            var edges2 = new int[6];
            for (var perm = 0; perm < NEdgePerm; perm++)
            {
                UnpackEdgePerm(perm, edges1);
                for (var move = 0; move < NMoves; move++)
                {
                    Array.Copy(edges1, 0, edges2, 0, 6);
                    MoveEdges(edges2, move);
                    var newPerm = PackEdgePerm(edges2);
                    MoveEdgePerm[perm][move] = newPerm;
                }
            }

            for (var orient = 0; orient < NEdgeOrient; orient++)
            {
                UnpackEdgeOrient(orient, edges1);
                for (var move = 0; move < NMoves; move++)
                {
                    Array.Copy(edges1, 0, edges2, 0, 6);
                    MoveEdges(edges2, move);
                    var newOrient = PackEdgeOrient(edges2);
                    MoveEdgeOrient[orient][move] = newOrient;
                }
            }

            var corners1 = new int[4];
            var corners2 = new int[4];
            for (var orient = 0; orient < NCornerOrient; orient++)
            {
                UnpackCornerOrient(orient, corners1);
                for (var move = 0; move < NMoves; move++)
                {
                    Array.Copy(corners1, 0, corners2, 0, 4);
                    MoveCorners(corners2, move);
                    var newOrient = PackCornerOrient(corners2);
                    MoveCornerOrient[orient][move] = newOrient;
                }
            }
        }

        /**
         * Fill the pruning tables for the permutation and orientation coordinates.
         */
        private static readonly int[] PrunPerm = new int[NEdgePerm];
        private static readonly int[] PrunOrient = new int[NOrient];

        private static void InitPrun()
        {
            for (var perm = 0; perm < NEdgePerm; perm++)
            {
                PrunPerm[perm] = -1;
            }
            PrunPerm[0] = 0;

            var done = 1;
            for (var length = 0; done < NEdgePerm / 2; length++)
            {
                // Only half of the permutations are accessible due to parity
                for (var perm = 0; perm < NEdgePerm; perm++)
                {
                    if (PrunPerm[perm] == length)
                    {
                        for (var move = 0; move < NMoves; move++)
                        {
                            var newPerm = MoveEdgePerm[perm][move];
                            if (PrunPerm[newPerm] == -1)
                            {
                                PrunPerm[newPerm] = length + 1;
                                done++;
                            }
                        }
                    }
                }
            }

            for (var orient = 0; orient < NOrient; orient++)
            {
                PrunOrient[orient] = -1;
            }
            PrunOrient[0] = 0;

            done = 1;
            for (var length = 0; done < NOrient; length++)
            {
                for (var orient = 0; orient < NOrient; orient++)
                {
                    if (PrunOrient[orient] == length)
                    {
                        for (var move = 0; move < NMoves; move++)
                        {
                            var newEdgeOrient = MoveEdgeOrient[orient % NEdgeOrient][move];
                            var newCornerOrient = MoveCornerOrient[orient / NEdgeOrient][move];
                            var newOrient = newCornerOrient * NEdgeOrient + newEdgeOrient;
                            if (PrunOrient[newOrient] == -1)
                            {
                                PrunOrient[newOrient] = length + 1;
                                done++;
                            }
                        }
                    }
                }
            }
        }

        static PyraminxSolver()
        {
            InitMoves();
            InitPrun();
        }

        /**
         * Search a solution from a position given by permutation and orientation coordinates
         * @param edgePerm       edge permutation coordinate to solve
         * @param edgeOrient     edge orientation coordinate to solve
         * @param cornerOrient   corner orientation coordinate to solve
         * @param depth          current depth of the search (first called with 0)
         * @param length         the remaining number of moves we can apply
         * @param last_move      what was the last move done (first called with an int >= 9)
         * @param solution       the array containing the current moves done.
         * @return               true if a solution was found (stored in the solution array)
         *                       false if no solution was found
         */

        private static bool Search(int edgePerm, int edgeOrient, int cornerOrient, int depth, int length, int lastMove,
            int[] solution, Random randomiseMoves)
        {
            /* If there are no moves left to try (length=0), returns if the current position is solved */
            if (length == 0)
            {
                return (edgePerm == 0) && (edgeOrient == 0) && (cornerOrient == 0);
            }

            /* Check if we might be able to solve the permutation or the orientation of the position
             * given the remaining number of moves ('length' parameter), using the pruning tables.
             * If not, there is no point keeping searching for a solution, just stop.
             */
            if ((PrunPerm[edgePerm] > length) || (PrunOrient[cornerOrient * NEdgeOrient + edgeOrient] > length))
            {
                return false;
            }

            /* The recursive part of the search function.
             * Try every move from the current position, and call the search function with the new position
             * and the updated parameters (depth -> depth+1; length -> length-1; last_move -> move)
             * We don't need to try a move of the same face as the last move.
             * We randomise the move order by generating a random offset.
             */
            var randomOffset = randomiseMoves.Next(NMoves);
            for (var move = 0; move < NMoves; move++)
            {
                var randomMove = (move + randomOffset) % NMoves;
                // Check if the tested move is of the same face as the previous move (last_move).
                if ((randomMove / 2) == (lastMove / 2))
                {
                    continue;
                }
                // Apply the move
                var newEdgePerm = MoveEdgePerm[edgePerm][randomMove];
                var newEdgeOrient = MoveEdgeOrient[edgeOrient][randomMove];
                var newCornerOrient = MoveCornerOrient[cornerOrient][randomMove];
                // Call the recursive function
                if (Search(newEdgePerm, newEdgeOrient, newCornerOrient, depth + 1, length - 1, randomMove, solution,
                    randomiseMoves))
                {
                    // Store the move
                    solution[depth] = randomMove;
                    return true;
                }
            }
            return false;
        }


        /**
                 * Generate a random pyraminx position.
                 * @param r         random int generator
                 */

        public PyraminxSolverState RandomState(Random r)
        {
            var state = new PyraminxSolverState();
            do
            {
                state.EdgePerm = r.Next(NEdgePerm);
            } while (PrunPerm[state.EdgePerm] == -1); // incorrect permutation (bad parity)
            state.EdgeOrient = r.Next(NEdgeOrient);
            state.CornerOrient = r.Next(NCornerOrient);
            state.Tips = r.Next(NTips);
            return state;
        }

        /**
         * Solve a given position in less than or equal to length number of turns.
         * Returns either the solution or the generator (inverse solution)
         * @param state         state
         * @param length        length of the desired solution
         * @param includingTips do we want to include tips in the solution lenght ?
         * @return              a string representing the solution or the scramble of a random position
         */

        public string SolveIn(PyraminxSolverState state, int length, bool includingTips)
        {
            return Solve(state, length, false, false, includingTips);
        }

        /**
         * Return a generator of a given position in exactly length number of turns or not at all.
         * Returns either the solution or the generator (inverse solution)
         * @param state         state
         * @param length        length of the desired solution
         * @param includingTips do we want to include tips in the solution lenght ?
         * @return              a string representing the solution or the scramble of a random position
         */

        public string GenerateExactly(PyraminxSolverState state, int length, bool includingTips)
        {
            return Solve(state, length, true, true, includingTips);
        }

        private static string Solve(PyraminxSolverState state, int desiredLength, bool exactLength, bool inverse,
            bool includingTips)
        {
            var r = new Random();
            var solution = new int[MaxLength];
            var foundSolution = false;

            // If we count the tips in the desired length, we have to subtract the number of unsolved tips from the length of the main puzzle search.
            if (includingTips)
            {
                desiredLength -= state.UnsolvedTips();
            }
            var length = exactLength ? desiredLength : 0;

            while (length <= desiredLength)
            {
                if (Search(state.EdgePerm, state.EdgeOrient, state.CornerOrient, 0, length, 42, solution, r))
                {
                    foundSolution = true;
                    break;
                }
                length++;
            }

            if (!foundSolution)
            {
                return null;
            }

            var scramble = new StringBuilder((MaxLength + 4) * 3);
            if (inverse)
            {
                for (var i = length - 1; i >= 0; i--)
                {
                    scramble.Append(" ").Append(InverseMoveToString[solution[i]]);
                }
            }
            else
            {
                for (var i = 0; i < length; i++)
                {
                    scramble.Append(" ").Append(MoveToString[solution[i]]);
                }
            }

            // Scramble the tips
            var arrayTips = new int[4];
            UnpackCornerOrient(state.Tips, arrayTips);
            for (var tip = 0; tip < 4; tip++)
            {
                var dir = arrayTips[tip];
                if (dir > 0)
                {
                    scramble.Append(" ")
                        .Append(inverse ? TipToString[tip * 2 + dir - 1] : InverseTipToString[tip * 2 + dir - 1]);
                }
            }

            return scramble.ToString().Trim();
        }
    }

    public class PyraminxSolverState
    {
        public int EdgePerm { get; set; }
        public int EdgeOrient { get; set; }
        public int CornerOrient { get; set; }
        public int Tips { get; set; }

        public int UnsolvedTips()
        {
            var numberUnsolved = 0;
            var tempTips = Tips;
            while (tempTips != 0)
            {
                if ((tempTips % 3) > 0)
                {
                    numberUnsolved++;
                }
                tempTips /= 3;
            }
            Assert(numberUnsolved <= 4);
            return numberUnsolved;
        }
    }
}