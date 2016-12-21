using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs.min2phase
{
    public class Tools
    {
        internal static readonly bool USE_TWIST_FLIP_PRUN = true;

        private static bool inited = false;

        private static int[] initState = new int[2];
        private static int[] require = { 0x0, 0x1, 0x2, 0x2, 0x2, 0x7, 0xa, 0x3, 0x13, 0x13, 0x3, 0x6e, 0xca, 0xa6, 0x612, 0x512 };

        private static void initIdx(int idx)
        {
            switch (idx)
            {
                case 0: CubieCube.initMove(); break;//-
                case 1: CubieCube.initSym(); break;//0
                case 2: CubieCube.initFlipSym2Raw(); break;//1
                case 3: CubieCube.initTwistSym2Raw(); break;//1

                case 4: CubieCube.initPermSym2Raw(); break;//1
                case 5: CoordCube.initFlipMove(); break;//0, 1, 2
                case 6: CoordCube.initTwistMove(); break;//0, 1, 3
                case 7: CoordCube.initUDSliceMoveConj(); break;//0, 1

                case 8: CoordCube.initCPermMove(); break;//0, 1, 4
                case 9: CoordCube.initEPermMove(); break;//0, 1, 4
                case 10: CoordCube.initMPermMoveConj(); break;//0, 1
                case 11: if (USE_TWIST_FLIP_PRUN) { CoordCube.initTwistFlipPrun(); } break;//1, 2, 3, 5, 6

                case 12: CoordCube.initSliceTwistPrun(); break;//1, 3, 6, 7
                case 13: CoordCube.initSliceFlipPrun(); break;//1, 2, 5, 7
                case 14: CoordCube.initMEPermPrun(); break;//1, 4, 9, 10
                case 15: CoordCube.initMCPermPrun(); break;//1, 4, 8, 10
            }
        }

        protected internal Tools() { }

        public static void init()
        {
            if (inited)
            {
                return;
            }
            /**
             * Can be replaced by:
             *     new Tools().run();
             */
            //initParallel(Runtime.getRuntime().availableProcessors());
            //initParallel(1);

            // This linear init is something gwt can deal with,
            // unlike the threading madness above.
            for (int i = 0; i <= 15; i++)
            {
                initIdx(i);
            }


            inited = true;
        }

        public static bool isInited()
        {
            return inited;
        }

        private static readonly Random r = new Random();
        public static string randomCube()
        {
            return randomCube(r);
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
        public static string randomCube(Random gen)
        {
            return randomState(STATE_RANDOM, STATE_RANDOM, STATE_RANDOM, STATE_RANDOM, gen);
        }

        private static int resolveOri(sbyte[] arr, int super, Random gen)
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

        private static int countUnknown(sbyte[] arr)
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

        private static int resolvePerm(sbyte[] arr, int cntU, int parity, Random gen)
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
            int p = Util.getNParity(Util.getNPerm(arr, arr.Length), arr.Length);
            if (p == 1 - parity && last != -1)
            {
                sbyte temp = arr[idx - 1];
                arr[idx - 1] = arr[last];
                arr[last] = temp;
            }
            return p;
        }

        public static readonly sbyte[] STATE_RANDOM = null;
        public static readonly sbyte[] STATE_SOLVED = new sbyte[0];

        protected internal static string randomState(sbyte[] cp, sbyte[] co, sbyte[] ep, sbyte[] eo, Random gen)
        {
            int parity;
            int cntUE = ep == STATE_RANDOM ? 12 : countUnknown(ep);
            int cntUC = cp == STATE_RANDOM ? 8 : countUnknown(cp);
            int cpVal, epVal;
            if (cntUE < 2)
            {   //ep != STATE_RANDOM
                if (ep == STATE_SOLVED)
                {
                    epVal = parity = 0;
                }
                else
                {
                    parity = resolvePerm(ep, cntUE, -1, gen);
                    epVal = Util.getNPerm(ep, 12);
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
                    } while (Util.getNParity(cpVal, 8) != parity);
                }
                else
                {
                    resolvePerm(cp, cntUC, parity, gen);
                    cpVal = Util.getNPerm(cp, 8);
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
                    parity = Util.getNParity(cpVal, 8);
                }
                else
                {
                    parity = resolvePerm(cp, cntUC, -1, gen);
                    cpVal = Util.getNPerm(cp, 8);
                }
                if (ep == STATE_RANDOM)
                {
                    do
                    {
                        epVal = gen.Next(479001600);
                    } while (Util.getNParity(epVal, 12) != parity);
                }
                else
                {
                    resolvePerm(ep, cntUE, parity, gen);
                    epVal = Util.getNPerm(ep, 12);
                }
            }
            return Util.toFaceCube(new CubieCube(
                cpVal,
                co == STATE_RANDOM ? gen.Next(2187) : (co == STATE_SOLVED ? 0 : resolveOri(co, 3, gen)),
                epVal,
                eo == STATE_RANDOM ? gen.Next(2048) : (eo == STATE_SOLVED ? 0 : resolveOri(eo, 2, gen))));
        }

        public static string randomLastLayer()
        {
            return randomLastLayer(r);
        }

        public static string randomLastLayer(Random gen)
        {
            return randomState(
                new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7 },
                new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0 },
                new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7, 8, 9, 10, 11 },
                new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0 }, gen);
        }

        public static string randomLastSlot()
        {
            return randomLastSlot(r);
        }

        public static string randomLastSlot(Random gen)
        {
            return randomState(
                new sbyte[] { -1, -1, -1, -1, -1, 5, 6, 7 },
                new sbyte[] { -1, -1, -1, -1, -1, 0, 0, 0 },
                new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7, -1, 9, 10, 11 },
                new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0, -1, 0, 0, 0 }, gen);
        }

        public static string randomZBLastLayer()
        {
            return randomZBLastLayer(r);
        }

        public static string randomZBLastLayer(Random gen)
        {
            return randomState(
                new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7 },
                new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0 },
                new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7, 8, 9, 10, 11 },
                STATE_SOLVED, gen);
        }

        public static string randomCornerOfLastLayer()
        {
            return randomCornerOfLastLayer(r);
        }

        public static string randomCornerOfLastLayer(Random gen)
        {
            return randomState(
                new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7 },
                new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0 },
                STATE_SOLVED,
                STATE_SOLVED, gen);
        }

        public static string randomEdgeOfLastLayer()
        {
            return randomEdgeOfLastLayer(r);
        }

        public static string randomEdgeOfLastLayer(Random gen)
        {
            return randomState(
                STATE_SOLVED,
                STATE_SOLVED,
                new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7, 8, 9, 10, 11 },
                new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0 }, gen);
        }

        public static string randomCrossSolved()
        {
            return randomCrossSolved(r);
        }
        public static string randomCrossSolved(Random gen)
        {
            return randomState(
                STATE_RANDOM,
                STATE_RANDOM,
                new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7, -1, -1, -1, -1 },
                new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0, -1, -1, -1, -1 }, gen);
        }

        public static string randomEdgeSolved()
        {
            return randomEdgeSolved(r);
        }
        public static string randomEdgeSolved(Random gen)
        {
            return randomState(
                STATE_RANDOM,
                STATE_RANDOM,
                STATE_SOLVED,
                STATE_SOLVED, gen);
        }

        public static string randomCornerSolved()
        {
            return randomCornerSolved(r);
        }
        public static string randomCornerSolved(Random gen)
        {
            return randomState(
                STATE_SOLVED,
                STATE_SOLVED,
                STATE_RANDOM,
                STATE_RANDOM, gen);
        }

        public static string superFlip()
        {
            return Util.toFaceCube(new CubieCube(0, 0, 0, 2047));
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
        public static int verify(string facelets)
        {
            return new Search().verify(facelets);
        }
    }
}
