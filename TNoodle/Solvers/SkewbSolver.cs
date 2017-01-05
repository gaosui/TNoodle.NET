using System;
using System.Text;
using TNoodle.Utils;

namespace TNoodle.Solvers
{
    public class SkewbSolver
    {
        private const int NMoves = 4;

        private static readonly int[] Fact = {1, 1, 1, 3, 12, 60, 360}; //fact[x] = x!/2
        private static readonly char[][] Permmv = ArrayExtension.New<char>(4320, 4);
        private static readonly char[][] Twstmv = ArrayExtension.New<char>(2187, 4);
        private static readonly sbyte[] Permprun = new sbyte[4320];
        private static readonly sbyte[] Twstprun = new sbyte[2187];

        private const int MaxSolutionLength = 12;

        private static readonly sbyte[][] Cornerpermmv =
        {
            new sbyte[] {6, 5, 10, 1},
            new sbyte[] {9, 7, 4, 2},
            new sbyte[] {3, 11, 8, 0},
            new sbyte[] {10, 1, 6, 5},
            new sbyte[] {0, 8, 11, 3},
            new sbyte[] {7, 9, 2, 4},
            new sbyte[] {4, 2, 9, 7},
            new sbyte[] {11, 3, 0, 8},
            new sbyte[] {1, 10, 5, 6},
            new sbyte[] {8, 0, 3, 11},
            new sbyte[] {2, 4, 7, 9},
            new sbyte[] {5, 6, 1, 10}
        };

        private static readonly sbyte[] Ori =
        {
            0, 1, 2, 0, 2, 1, 1, 2, 0,
            2, 1, 0
        };

        private static int Getpermmv(int idx, int move)
        {
            var centerindex = idx / 12;
            var cornerindex = idx % 12;
            var val = 0x543210;
            var parity = 0;
            var centerperm = new int[6];
            for (var i = 0; i < 5; i++)
            {
                var p = Fact[5 - i];
                var v = centerindex / p;
                centerindex -= v * p;
                parity ^= v;
                v <<= 2;
                centerperm[i] = (val >> v) & 0xf;
                var m = (1 << v) - 1;
                val = (val & m) + ((val >> 4) & ~m);
            }
            if ((parity & 1) == 0)
            {
                centerperm[5] = val;
            }
            else
            {
                centerperm[5] = centerperm[4];
                centerperm[4] = val;
            }
            int t;
            if (move == 0)
            {
                t = centerperm[0];
                centerperm[0] = centerperm[1];
                centerperm[1] = centerperm[3];
                centerperm[3] = t;
            }
            else if (move == 1)
            {
                t = centerperm[0];
                centerperm[0] = centerperm[4];
                centerperm[4] = centerperm[2];
                centerperm[2] = t;
            }
            else if (move == 2)
            {
                t = centerperm[1];
                centerperm[1] = centerperm[2];
                centerperm[2] = centerperm[5];
                centerperm[5] = t;
            }
            else if (move == 3)
            {
                t = centerperm[3];
                centerperm[3] = centerperm[5];
                centerperm[5] = centerperm[4];
                centerperm[4] = t;
            }
            val = 0x543210;
            for (var i = 0; i < 4; i++)
            {
                var v = centerperm[i] << 2;
                centerindex *= 6 - i;
                centerindex += (val >> v) & 0xf;
                val -= (int) (0x111110L << v);
            }
            return centerindex * 12 + Cornerpermmv[cornerindex][move];
        }

        private static int Gettwstmv(int idx, int move)
        {
            var fixedtwst = new int[4];
            var twst = new int[4];
            for (var i = 0; i < 4; i++)
            {
                fixedtwst[i] = idx % 3;
                idx /= 3;
            }
            for (var i = 0; i < 3; i++)
            {
                twst[i] = idx % 3;
                idx /= 3;
            }
            twst[3] = (6 - twst[0] - twst[1] - twst[2]) % 3;
            fixedtwst[move] = (fixedtwst[move] + 1) % 3;
            int t;
            switch (move)
            {
                case 0:
                    t = twst[0];
                    twst[0] = twst[2] + 2;
                    twst[2] = twst[1] + 2;
                    twst[1] = t + 2;
                    break;
                case 1:
                    t = twst[0];
                    twst[0] = twst[1] + 2;
                    twst[1] = twst[3] + 2;
                    twst[3] = t + 2;
                    break;
                case 2:
                    t = twst[0];
                    twst[0] = twst[3] + 2;
                    twst[3] = twst[2] + 2;
                    twst[2] = t + 2;
                    break;
                case 3:
                    t = twst[1];
                    twst[1] = twst[2] + 2;
                    twst[2] = twst[3] + 2;
                    twst[3] = t + 2;
                    break;
            }
            for (var i = 2; i >= 0; i--)
            {
                idx = idx * 3 + twst[i] % 3;
            }
            for (var i = 3; i >= 0; i--)
            {
                idx = idx * 3 + fixedtwst[i];
            }
            return idx;
        }

        static SkewbSolver()
        {
            Init();
        }

        private static void Init()
        {
            for (var i = 0; i < 4320; i++)
            {
                Permprun[i] = -1;
                for (var j = 0; j < 4; j++)
                {
                    Permmv[i][j] = (char) Getpermmv(i, j);
                }
            }
            for (var i = 0; i < 2187; i++)
            {
                Twstprun[i] = -1;
                for (var j = 0; j < 4; j++)
                {
                    Twstmv[i][j] = (char) Gettwstmv(i, j);
                }
            }
            Permprun[0] = 0;
            for (var l = 0; l < 6; l++)
            {
                for (var p = 0; p < 4320; p++)
                {
                    if (Permprun[p] == l)
                    {
                        for (var m = 0; m < 4; m++)
                        {
                            var q = p;
                            for (var c = 0; c < 2; c++)
                            {
                                q = Permmv[q][m];
                                if (Permprun[q] == -1)
                                {
                                    Permprun[q] = (sbyte) (l + 1);
                                }
                            }
                        }
                    }
                }
            }
            Twstprun[0] = 0;
            for (var l = 0; l < 6; l++)
            {
                for (var p = 0; p < 2187; p++)
                {
                    if (Twstprun[p] == l)
                    {
                        for (var m = 0; m < 4; m++)
                        {
                            var q = p;
                            for (var c = 0; c < 2; c++)
                            {
                                q = Twstmv[q][m];
                                if (Twstprun[q] == -1)
                                {
                                    Twstprun[q] = (sbyte) (l + 1);
                                }
                            }
                        }
                    }
                }
            }
        }

        protected bool Search(int depth, int perm, int twst, int maxl, int lm, int[] sol, Random randomizeMoves)
        {
            if (maxl == 0)
            {
                _solutionLength = depth;
                return (perm == 0 && twst == 0);
            }
            _solutionLength = -1;
            if (Permprun[perm] > maxl || Twstprun[twst] > maxl)
            {
                return false;
            }
            var randomOffset = randomizeMoves.Next(NMoves);
            for (var m = 0; m < NMoves; m++)
            {
                var randomMove = (m + randomOffset) % NMoves;
                if (randomMove != lm)
                {
                    var p = perm;
                    var s = twst;
                    for (var a = 0; a < 2; a++)
                    {
                        p = Permmv[p][randomMove];
                        s = Twstmv[s][randomMove];
                        if (Search(depth + 1, p, s, maxl - 1, randomMove, sol, randomizeMoves))
                        {
                            sol[depth] = randomMove * 2 + a;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public class SkewbSolverState
        {
            public int Perm { get; set; }
            public int Twst { get; set; }

            public bool IsSolvable()
            {
                return Ori[Perm % 12] == (Twst + Twst / 3 + Twst / 9 + Twst / 27) % 3;
            }
        }

        public SkewbSolverState RandomState(Random r)
        {
            var state = new SkewbSolverState {Perm = r.Next(4320)};
            do
            {
                state.Twst = r.Next(2187);
            } while (!state.IsSolvable());
            return state;
        }

        public string SolveIn(SkewbSolverState state, int length, Random randomizeMoves)
        {
            var sol = new int[MaxSolutionLength];
            Search(0, state.Perm, state.Twst, length, -1, sol, randomizeMoves);
            if (_solutionLength != -1)
            {
                return GetSolution(sol);
            }
            return null;
        }

        public string GenerateExactly(SkewbSolverState state, int length, Random randomizeMoves)
        {
            var sol = new int[MaxSolutionLength];
            Search(0, state.Perm, state.Twst, length, -1, sol, randomizeMoves);
            return GetSolution(sol);
        }

        private int _solutionLength = -1;


        /**
                 * The solver is written in jaap's notation. Now we're going to convert the result to FCN(fixed corner notation):
                 * Step one, the puzzle is rotated by z2, which will bring "R L D B" (in jaap's notation) to "L R F U" (in FCN, F has not
                 *     been defined, now we define it as the opposite corner of B)
                 * Step two, convert F to B by rotation [F' B]. When an F found in the move sequence, it is replaced immediately by B and other 3 moves
                 *     should be swapped. For example, if the next move is R, we should turn U instead. Because the R corner is at U after rotation.
                 *     In another word, "F R" is converted to "B U". The correctness can be easily verified and the procedure is recursable.
                 */

        private string GetSolution(int[] sol)
        {
            var sb = new StringBuilder();
            string[] move2Str = {"L", "R", "B", "U"}; //RLDB (in jaap's notation) rotated by z2
            for (var i = 0; i < _solutionLength; i++)
            {
                var axis = sol[i] >> 1;
                var pow = sol[i] & 1;
                if (axis == 2)
                {
                    //step two.
                    for (var p = 0; p <= pow; p++)
                    {
                        var temp = move2Str[0];
                        move2Str[0] = move2Str[1];
                        move2Str[1] = move2Str[3];
                        move2Str[3] = temp;
                    }
                }
                sb.Append(move2Str[axis] + ((pow == 1) ? "'" : ""));
                sb.Append(" ");
            }
            var scrambleSequence = sb.ToString().Trim();
            return scrambleSequence;
        }
    }
}