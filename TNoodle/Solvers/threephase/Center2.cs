using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Utils;

namespace TNoodle.Solvers.Threephase
{
    /*
                0	1
                3	2

    4	5		8	9		0	1		12	13
    7	6		11	10		3	2		15	14

                4	5
                7	6
    */

    internal class Center2
    {
        private readonly int[] rl = new int[8];
        private readonly int[] ct = new int[16];
        private int parity = 0;

		public static int[][] Rlmv { get; } = ArrayExtension.New<int>(70, 28);
		public static char[][] Ctmv { get; } = ArrayExtension.New<char>(6435, 28);
		private static readonly int[][] rlrot = ArrayExtension.New<int>(70, 16);
		private static readonly char[][] ctrot = ArrayExtension.New<char>(6435, 16);
        public static sbyte[] Ctprun { get; } = new sbyte[6435 * 35 * 2];

        private static readonly int[] pmv = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1,
                        0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0};

        public static void Init()
        {
            Center2 c = new Center2();

            for (int i = 0; i < 35 * 2; i++)
            {
                for (int m = 0; m < 28; m++)
                {
                    c.Setrl(i);
                    c.Move(Moves.Move2std[m]);
                    Rlmv[i][m] = c.Getrl();
                }
            }

            for (int i = 0; i < 70; i++)
            {
                c.Setrl(i);
                for (int j = 0; j < 16; j++)
                {
                    rlrot[i][j] = c.Getrl();
                    c.Rot(0);
                    if (j % 2 == 1) c.Rot(1);
                    if (j % 8 == 7) c.Rot(2);
                }
            }

            for (int i = 0; i < 6435; i++)
            {
                c.Setct(i);
                for (int j = 0; j < 16; j++)
                {
                    ctrot[i][j] = (char)c.Getct();
                    c.Rot(0);
                    if (j % 2 == 1) c.Rot(1);
                    if (j % 8 == 7) c.Rot(2);
                }
            }

            for (int i = 0; i < 6435; i++)
            {
                for (int m = 0; m < 28; m++)
                {
                    c.Setct(i);
                    c.Move(Moves.Move2std[m]);
                    Ctmv[i][m] = (char)c.Getct();
                }
            }

            ArrayExtension.Fill(Ctprun, (sbyte)-1);

            Ctprun[0] = Ctprun[18] = Ctprun[28] = Ctprun[46] = Ctprun[54] = Ctprun[56] = 0;
            int depth = 0;
            int done = 6;
            while (done != 6435 * 35 * 2)
            {
                for (int i = 0; i < 6435 * 35 * 2; i++)
                {
                    if (Ctprun[i] != depth)
                    {
                        continue;
                    }
                    int ct = i / 70;
                    int rl = i % 70;
                    for (int m = 0; m < 23; m++)
                    {
                        int ctx = Ctmv[ct][m];
                        int rlx = Rlmv[rl][m];
                        int idx = ctx * 70 + rlx;
                        if (Ctprun[idx] == -1)
                        {
                            Ctprun[idx] = (sbyte)(depth + 1);
                            done++;
                        }
                    }
                }
                depth++;
            }
        }

        public Center2()
        {
        }

        public Center2(CenterCube c) : this()
        {
            for (int i = 0; i < 16; i++)
            {
                ct[i] = c.Ct[i] / 2;
            }
            for (int i = 0; i < 8; i++)
            {
                rl[i] = c.Ct[i + 16];
            }
        }

        public void Set(CenterCube c, int edgeParity)
        {
            for (int i = 0; i < 16; i++)
            {
                ct[i] = c.Ct[i] / 2;
            }
            for (int i = 0; i < 8; i++)
            {
                rl[i] = c.Ct[i + 16];
            }
            parity = edgeParity;
        }

        public int Getrl()
        {
            int idx = 0;
            int r = 4;
            for (int i = 6; i >= 0; i--)
            {
                if (rl[i] != rl[7])
                {
                    idx += Util.Cnk[i][r--];
                }
            }
            return idx * 2 + parity;
        }

        private void Setrl(int idx)
        {
            parity = idx & 1;
            //idx >>>= 1;
            idx = (int)((uint)idx >> 1);
            int r = 4;
            rl[7] = 0;
            for (int i = 6; i >= 0; i--)
            {
                if (idx >= Util.Cnk[i][r])
                {
                    idx -= Util.Cnk[i][r--];
                    rl[i] = 1;
                }
                else
                {
                    rl[i] = 0;
                }
            }
        }

        public int Getct()
        {
            int idx = 0;
            int r = 8;
            for (int i = 14; i >= 0; i--)
            {
                if (ct[i] != ct[15])
                {
                    idx += Util.Cnk[i][r--];
                }
            }
            return idx;
        }

        private void Setct(int idx)
        {
            int r = 8;
            ct[15] = 0;
            for (int i = 14; i >= 0; i--)
            {
                if (idx >= Util.Cnk[i][r])
                {
                    idx -= Util.Cnk[i][r--];
                    ct[i] = 1;
                }
                else
                {
                    ct[i] = 0;
                }
            }
        }

        private void Rot(int r)
        {
            switch (r)
            {
                case 0:
                    Move(Moves.ux2);
                    Move(Moves.dx2);
                    break;
                case 1:
                    Move(Moves.rx1);
                    Move(Moves.lx3);
                    break;
                case 2:
                    Util.Swap(ct, 0, 3, 1, 2, 1);
                    Util.Swap(ct, 8, 11, 9, 10, 1);
                    Util.Swap(ct, 4, 7, 5, 6, 1);
                    Util.Swap(ct, 12, 15, 13, 14, 1);
                    Util.Swap(rl, 0, 3, 5, 6, 1);
                    Util.Swap(rl, 1, 2, 4, 7, 1);
                    break;
            }
        }


        private void Move(int m)
        {
            parity ^= pmv[m];
            int key = m % 3;
            m /= 3;
            switch (m)
            {
                case 0:     //U
                    Util.Swap(ct, 0, 1, 2, 3, key);
                    break;
                case 1:     //R
                    Util.Swap(rl, 0, 1, 2, 3, key);
                    break;
                case 2:     //F
                    Util.Swap(ct, 8, 9, 10, 11, key);
                    break;
                case 3:     //D
                    Util.Swap(ct, 4, 5, 6, 7, key);
                    break;
                case 4:     //L
                    Util.Swap(rl, 4, 5, 6, 7, key);
                    break;
                case 5:     //B
                    Util.Swap(ct, 12, 13, 14, 15, key);
                    break;
                case 6:     //u
                    Util.Swap(ct, 0, 1, 2, 3, key);
                    Util.Swap(rl, 0, 5, 4, 1, key);
                    Util.Swap(ct, 8, 9, 12, 13, key);
                    break;
                case 7:     //r
                    Util.Swap(rl, 0, 1, 2, 3, key);
                    Util.Swap(ct, 1, 15, 5, 9, key);
                    Util.Swap(ct, 2, 12, 6, 10, key);
                    break;
                case 8:     //f
                    Util.Swap(ct, 8, 9, 10, 11, key);
                    Util.Swap(rl, 0, 3, 6, 5, key);
                    Util.Swap(ct, 3, 2, 5, 4, key);
                    break;
                case 9:     //d
                    Util.Swap(ct, 4, 5, 6, 7, key);
                    Util.Swap(rl, 3, 2, 7, 6, key);
                    Util.Swap(ct, 11, 10, 15, 14, key);
                    break;
                case 10:    //l
                    Util.Swap(rl, 4, 5, 6, 7, key);
                    Util.Swap(ct, 0, 8, 4, 14, key);
                    Util.Swap(ct, 3, 11, 7, 13, key);
                    break;
                case 11:    //b		
                    Util.Swap(ct, 12, 13, 14, 15, key);
                    Util.Swap(rl, 1, 4, 7, 2, key);
                    Util.Swap(ct, 1, 0, 7, 6, key);
                    break;
            }
        }
    }
}
