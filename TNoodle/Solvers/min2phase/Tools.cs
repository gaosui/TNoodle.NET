using System;

namespace TNoodle.Solvers.Min2phase
{
    public static class Tools
    {
        internal const bool USE_TWIST_FLIP_PRUN = true;

        public static bool Inited { get; private set; } = false;

        private static int[] initState = new int[2];
        private static int[] require = { 0x0, 0x1, 0x2, 0x2, 0x2, 0x7, 0xa, 0x3, 0x13, 0x13, 0x3, 0x6e, 0xca, 0xa6, 0x612, 0x512 };

        private static readonly Random r = new Random();

        public static sbyte[] STATE_RANDOM { get; } = null;
        public static sbyte[] STATE_SOLVED { get; } = new sbyte[0];

        private static void InitIdx(int idx)
        {
            switch (idx)
            {
                case 0: CubieCube.InitMove(); break;//-
                case 1: CubieCube.InitSym(); break;//0
                case 2: CubieCube.InitFlipSym2Raw(); break;//1
                case 3: CubieCube.InitTwistSym2Raw(); break;//1

                case 4: CubieCube.InitPermSym2Raw(); break;//1
                case 5: CoordCube.InitFlipMove(); break;//0, 1, 2
                case 6: CoordCube.InitTwistMove(); break;//0, 1, 3
                case 7: CoordCube.InitUDSliceMoveConj(); break;//0, 1

                case 8: CoordCube.InitCPermMove(); break;//0, 1, 4
                case 9: CoordCube.InitEPermMove(); break;//0, 1, 4
                case 10: CoordCube.InitMPermMoveConj(); break;//0, 1
                case 11: if (USE_TWIST_FLIP_PRUN) { CoordCube.InitTwistFlipPrun(); } break;//1, 2, 3, 5, 6

                case 12: CoordCube.InitSliceTwistPrun(); break;//1, 3, 6, 7
                case 13: CoordCube.InitSliceFlipPrun(); break;//1, 2, 5, 7
                case 14: CoordCube.InitMEPermPrun(); break;//1, 4, 9, 10
                case 15: CoordCube.InitMCPermPrun(); break;//1, 4, 8, 10
            }
        }

        public static void Init()
        {
            if (Inited)
            {
                return;
            }

            for (int i = 0; i <= 15; i++)
            {
                InitIdx(i);
            }

            Inited = true;
        }

        public static string RandomCube()
        {
            return RandomCube(r);
        }

        /**
         * Generates a random cube.<br>
         *
         * The random source can be set by {@link cs.min2phase.Tools#setRandomSource(java.util.Random)}
         *
         * @return A random cube in the string representation. Each cube of the cube space has almost (depends on randomSource) the same probability.
         *
         * @see cs.min2phase.Tools#setRandomSource(java.util.Random)
         * @see cs.min2phase.Search#solution(java.lang.String facelets, int maxDepth, long timeOut, long timeMin, int verbose)
         */
        public static string RandomCube(Random gen)
        {
            return RandomState(STATE_RANDOM, STATE_RANDOM, STATE_RANDOM, STATE_RANDOM, gen);
        }

        private static int ResolveOri(sbyte[] arr, int super, Random gen)
        {
            int sum = 0, idx = 0, lastUnknown = -1;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == -1)
                {
                    arr[i] = (sbyte)gen.Next(super);
                    lastUnknown = i;
                }
                sum += arr[i];
            }
            if (sum % super != 0 && lastUnknown != -1)
            {
                arr[lastUnknown] = (sbyte)((30 + arr[lastUnknown] - sum) % super);
            }
            for (int i = 0; i < arr.Length - 1; i++)
            {
                idx *= super;
                idx += arr[i];
            }
            return idx;
        }

        private static int CountUnknown(sbyte[] arr)
        {
            if (arr == STATE_SOLVED)
            {
                return 0;
            }
            int cnt = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == -1)
                {
                    cnt++;
                }
            }
            return cnt;
        }

        private static int ResolvePerm(sbyte[] arr, int cntU, int parity, Random gen)
        {
            if (arr == STATE_SOLVED)
            {
                return 0;
            }
            else if (arr == STATE_RANDOM)
            {
                return parity == -1 ? gen.Next(2) : parity;
            }
            sbyte[] val = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] != -1)
                {
                    val[arr[i]] = -1;
                }
            }
            int idx = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (val[i] != -1)
                {
                    int j = gen.Next(idx + 1);
                    sbyte temp = val[i];
                    val[idx++] = val[j];
                    val[j] = temp;
                }
            }
            int last = -1;
            for (idx = 0; idx < arr.Length && cntU > 0; idx++)
            {
                if (arr[idx] == -1)
                {
                    if (cntU == 2)
                    {
                        last = idx;
                    }
                    arr[idx] = val[--cntU];
                }
            }
            int p = Util.GetNParity(Util.GetNPerm(arr, arr.Length), arr.Length);
            if (p == 1 - parity && last != -1)
            {
                sbyte temp = arr[idx - 1];
                arr[idx - 1] = arr[last];
                arr[last] = temp;
            }
            return p;
        }

        private static string RandomState(sbyte[] cp, sbyte[] co, sbyte[] ep, sbyte[] eo, Random gen)
        {
            int parity;
            int cntUE = ep == STATE_RANDOM ? 12 : CountUnknown(ep);
            int cntUC = cp == STATE_RANDOM ? 8 : CountUnknown(cp);
            int cpVal, epVal;
            if (cntUE < 2)
            {   //ep != STATE_RANDOM
                if (ep == STATE_SOLVED)
                {
                    epVal = parity = 0;
                }
                else
                {
                    parity = ResolvePerm(ep, cntUE, -1, gen);
                    epVal = Util.GetNPerm(ep, 12);
                }
                if (cp == STATE_SOLVED)
                {
                    cpVal = 0;
                }
                else if (cp == STATE_RANDOM)
                {
                    do
                    {
                        cpVal = gen.Next(40320);
                    } while (Util.GetNParity(cpVal, 8) != parity);
                }
                else
                {
                    ResolvePerm(cp, cntUC, parity, gen);
                    cpVal = Util.GetNPerm(cp, 8);
                }
            }
            else
            {   //ep != STATE_SOLVED
                if (cp == STATE_SOLVED)
                {
                    cpVal = parity = 0;
                }
                else if (cp == STATE_RANDOM)
                {
                    cpVal = gen.Next(40320);
                    parity = Util.GetNParity(cpVal, 8);
                }
                else
                {
                    parity = ResolvePerm(cp, cntUC, -1, gen);
                    cpVal = Util.GetNPerm(cp, 8);
                }
                if (ep == STATE_RANDOM)
                {
                    do
                    {
                        epVal = gen.Next(479001600);
                    } while (Util.GetNParity(epVal, 12) != parity);
                }
                else
                {
                    ResolvePerm(ep, cntUE, parity, gen);
                    epVal = Util.GetNPerm(ep, 12);
                }
            }
            return Util.ToFaceCube(new CubieCube(
                cpVal,
                co == STATE_RANDOM ? gen.Next(2187) : (co == STATE_SOLVED ? 0 : ResolveOri(co, 3, gen)),
                epVal,
                eo == STATE_RANDOM ? gen.Next(2048) : (eo == STATE_SOLVED ? 0 : ResolveOri(eo, 2, gen))));
        }

        public static string RandomLastLayer()
        {
            return RandomLastLayer(r);
        }

        public static string RandomLastLayer(Random gen)
        {
            return RandomState(
                new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7 },
                new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0 },
                new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7, 8, 9, 10, 11 },
                new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0 }, gen);
        }

        public static string RandomLastSlot()
        {
            return RandomLastSlot(r);
        }

        public static string RandomLastSlot(Random gen)
        {
            return RandomState(
                new sbyte[] { -1, -1, -1, -1, -1, 5, 6, 7 },
                new sbyte[] { -1, -1, -1, -1, -1, 0, 0, 0 },
                new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7, -1, 9, 10, 11 },
                new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0, -1, 0, 0, 0 }, gen);
        }

        public static string RandomZBLastLayer()
        {
            return RandomZBLastLayer(r);
        }

        public static string RandomZBLastLayer(Random gen)
        {
            return RandomState(
                new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7 },
                new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0 },
                new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7, 8, 9, 10, 11 },
                STATE_SOLVED, gen);
        }

        public static string RandomCornerOfLastLayer()
        {
            return RandomCornerOfLastLayer(r);
        }

        public static string RandomCornerOfLastLayer(Random gen)
        {
            return RandomState(
                new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7 },
                new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0 },
                STATE_SOLVED,
                STATE_SOLVED, gen);
        }

        public static string RandomEdgeOfLastLayer()
        {
            return RandomEdgeOfLastLayer(r);
        }

        public static string RandomEdgeOfLastLayer(Random gen)
        {
            return RandomState(
                STATE_SOLVED,
                STATE_SOLVED,
                new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7, 8, 9, 10, 11 },
                new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0 }, gen);
        }

        public static string RandomCrossSolved()
        {
            return RandomCrossSolved(r);
        }
        public static string RandomCrossSolved(Random gen)
        {
            return RandomState(
                STATE_RANDOM,
                STATE_RANDOM,
                new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7, -1, -1, -1, -1 },
                new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0, -1, -1, -1, -1 }, gen);
        }

        public static string RandomEdgeSolved()
        {
            return RandomEdgeSolved(r);
        }
        public static string RandomEdgeSolved(Random gen)
        {
            return RandomState(
                STATE_RANDOM,
                STATE_RANDOM,
                STATE_SOLVED,
                STATE_SOLVED, gen);
        }

        public static string RandomCornerSolved()
        {
            return RandomCornerSolved(r);
        }
        public static string RandomCornerSolved(Random gen)
        {
            return RandomState(
                STATE_SOLVED,
                STATE_SOLVED,
                STATE_RANDOM,
                STATE_RANDOM, gen);
        }

        public static string SuperFlip()
        {
            return Util.ToFaceCube(new CubieCube(0, 0, 0, 2047));
        }

        /**
         * Check whether the cube definition string s represents a solvable cube.
         *
         * @param facelets is the cube definition string , see {@link cs.min2phase.Search#solution(java.lang.String facelets, int maxDepth, long timeOut, long timeMin, int verbose)}
         * @return 0: Cube is solvable<br>
         *         -1: There is not exactly one facelet of each colour<br>
         *         -2: Not all 12 edges exist exactly once<br>
         *         -3: Flip error: One edge has to be flipped<br>
         *         -4: Not all 8 corners exist exactly once<br>
         *         -5: Twist error: One corner has to be twisted<br>
         *         -6: Parity error: Two corners or two edges have to be exchanged
         */
        public static int Verify(string facelets)
        {
            return new Search().Verify(facelets);
        }
    }
}
