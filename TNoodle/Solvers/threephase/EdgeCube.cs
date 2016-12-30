using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TNoodle.Solvers.Threephase.Moves;

namespace TNoodle.Solvers.Threephase
{
    internal class EdgeCube
    {
        private static readonly int[] epmv = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                        1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1};

        private static readonly int[][] edgeColor =
        {
            new int[] { F, U },
            new int[] { L, U },
            new int[] { B, U },
            new int[] { R, U },
            new int[] { B, D },
            new int[] { L, D },
            new int[] { F, D },
            new int[] { R, D },
            new int[] { F, L },
            new int[] { B, L },
            new int[] { B, R },
            new int[] { F, R }
        };

        private static readonly int[] EdgeMap = { F2, L2, B2, R2, B8, L8, F8, R8, F4, B6, B4, F6, U8, U4, U2, U6, D8, D4, D2, D6, L6, L4, R6, R4 };

        public sbyte[] Ep { get; } = new sbyte[24];

        public EdgeCube()
        {
            for (sbyte i = 0; i < 24; i++)
            {
                Ep[i] = i;
            }
        }

        public EdgeCube(EdgeCube c)
        {
            Copy(c);
        }

        public EdgeCube(Random r) : this()
        {
            for (int i = 0; i < 23; i++)
            {
                int t = i + r.Next(24 - i);
                if (t != i)
                {
                    sbyte m = Ep[i];
                    Ep[i] = Ep[t];
                    Ep[t] = m;
                }
            }
        }

        public EdgeCube(int[] moveseq) : this()
        {
            for (int m = 0; m < moveseq.Length; m++)
            {
                Move(m);
            }
        }

        public int GetParity()
        {
            return Util.Parity(Ep);
        }

        public void Copy(EdgeCube c)
        {
            for (int i = 0; i < 24; i++)
            {
                Ep[i] = c.Ep[i];
            }
        }

        public void Fill333Facelet(char[] facelet)
        {
            for (int i = 0; i < 24; i++)
            {
                facelet[EdgeMap[i]] = Util.ColorMap4to3[edgeColor[Ep[i] % 12][Ep[i] / 12]];
            }
        }

        public bool CheckEdge()
        {
            int ck = 0;
            bool parity = false;
            for (int i = 0; i < 12; i++)
            {
                ck |= 1 << Ep[i];
                parity = parity != Ep[i] >= 12;
            }
            ck &= ck >> 12;
            return ck == 0 && !parity;
        }

        /*
        Edge Cubies: 
                            14	2	
                        1			15
                        13			3
                            0	12	
            1	13			0	12			3	15			2	14	
        9			20	20			11	11			22	22			9
        21			8	8			23	23			10	10			21
            17	5			18	6			19	7			16	4	
                            18	6	
                        5			19
                        17			7
                            4	16	

        Center Cubies: 
                    0	1
                    3	2

        20	21		8	9		16	17		12	13
        23	22		11	10		19	18		15	14

                    4	5
                    7	6

             *             |************|
             *             |*U1**U2**U3*|
             *             |************|
             *             |*U4**U5**U6*|
             *             |************|
             *             |*U7**U8**U9*|
             *             |************|
             * ************|************|************|************|
             * *L1**L2**L3*|*F1**F2**F3*|*R1**R2**F3*|*B1**B2**B3*|
             * ************|************|************|************|
             * *L4**L5**L6*|*F4**F5**F6*|*R4**R5**R6*|*B4**B5**B6*|
             * ************|************|************|************|
             * *L7**L8**L9*|*F7**F8**F9*|*R7**R8**R9*|*B7**B8**B9*|
             * ************|************|************|************|
             *             |************|
             *             |*D1**D2**D3*|
             *             |************|
             *             |*D4**D5**D6*|
             *             |************|
             *             |*D7**D8**D9*|
             *             |************|
             */

        public void Move(int m)
        {
            int key = m % 3;
            m /= 3;
            switch (m)
            {
                case 0: //U
                    Util.Swap(Ep, 0, 1, 2, 3, key);
                    Util.Swap(Ep, 12, 13, 14, 15, key);
                    break;
                case 1: //R
                    Util.Swap(Ep, 11, 15, 10, 19, key);
                    Util.Swap(Ep, 23, 3, 22, 7, key);
                    break;
                case 2: //F
                    Util.Swap(Ep, 0, 11, 6, 8, key);
                    Util.Swap(Ep, 12, 23, 18, 20, key);
                    break;
                case 3: //D
                    Util.Swap(Ep, 4, 5, 6, 7, key);
                    Util.Swap(Ep, 16, 17, 18, 19, key);
                    break;
                case 4: //L
                    Util.Swap(Ep, 1, 20, 5, 21, key);
                    Util.Swap(Ep, 13, 8, 17, 9, key);
                    break;
                case 5: //B
                    Util.Swap(Ep, 2, 9, 4, 10, key);
                    Util.Swap(Ep, 14, 21, 16, 22, key);
                    break;
                case 6: //u
                    Util.Swap(Ep, 0, 1, 2, 3, key);
                    Util.Swap(Ep, 12, 13, 14, 15, key);
                    Util.Swap(Ep, 9, 22, 11, 20, key);
                    break;
                case 7: //r
                    Util.Swap(Ep, 11, 15, 10, 19, key);
                    Util.Swap(Ep, 23, 3, 22, 7, key);
                    Util.Swap(Ep, 2, 16, 6, 12, key);
                    break;
                case 8: //f
                    Util.Swap(Ep, 0, 11, 6, 8, key);
                    Util.Swap(Ep, 12, 23, 18, 20, key);
                    Util.Swap(Ep, 3, 19, 5, 13, key);
                    break;
                case 9: //d
                    Util.Swap(Ep, 4, 5, 6, 7, key);
                    Util.Swap(Ep, 16, 17, 18, 19, key);
                    Util.Swap(Ep, 8, 23, 10, 21, key);
                    break;
                case 10://l
                    Util.Swap(Ep, 1, 20, 5, 21, key);
                    Util.Swap(Ep, 13, 8, 17, 9, key);
                    Util.Swap(Ep, 14, 0, 18, 4, key);
                    break;
                case 11://b
                    Util.Swap(Ep, 2, 9, 4, 10, key);
                    Util.Swap(Ep, 14, 21, 16, 22, key);
                    Util.Swap(Ep, 7, 15, 1, 17, key);
                    break;
            }
        }
    }
}
