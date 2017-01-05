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

    20	21		8	9		16	17		12	13
    23	22		11	10		19	18		15	14

                4	5
                7	6
    */

    internal sealed class Center1
    {
		public static int[][] Ctsmv { get; } = ArrayExtension.New<int>(15582, 36);
        private static readonly int[] sym2raw = new int[15582];
        public static sbyte[] Csprun { get; } = new sbyte[15582];

		public static int[][] Symmult { get; } = ArrayExtension.New<int>(48, 48);
		public static int[][] Symmove { get; } = ArrayExtension.New<int>(48, 36);
        public static int[] Syminv { get; } = new int[48];
        public static int[] Finish { get; } = new int[48];

        public static int[] Raw2sym { get; set; }

        public static void InitSym2Raw()
        {
            Center1 c = new Center1();
            int[] occ = new int[735471 / 32 + 1];
            int count = 0;
            for (int i = 0; i < 735471; i++)
            {
                if ((occ[(uint)i >> 5] & (1 << (i & 0x1f))) == 0)
                {
                    c.Set(i);
                    for (int j = 0; j < 48; j++)
                    {
                        int idx = c.Get();
                        occ[(uint)idx >> 5] |= (1 << (idx & 0x1f));
                        if (Raw2sym != null)
                        {
                            Raw2sym[idx] = count << 6 | Syminv[j];
                        }
                        c.Rot(0);
                        if (j % 2 == 1) c.Rot(1);
                        if (j % 8 == 7) c.Rot(2);
                        if (j % 16 == 15) c.Rot(3);
                    }
                    sym2raw[count++] = i;
                }
            }
        }

        public static void CreatePrun()
        {
            ArrayExtension.Fill(Csprun, (sbyte)-1);
            Csprun[0] = 0;
            int depth = 0;
            int done = 1;

            while (done != 15582)
            {
                bool inv = depth > 4;
                int select = inv ? -1 : depth;
                int check = inv ? depth : -1;
                depth++;
                for (int i = 0; i < 15582; i++)
                {
                    if (Csprun[i] != select)
                    {
                        continue;
                    }
                    for (int m = 0; m < 27; m++)
                    {
                        int idx = (int)((uint)Ctsmv[i][m] >> 6);
                        if (Csprun[idx] != check)
                        {
                            continue;
                        }
                        ++done;
                        if (inv)
                        {
                            Csprun[i] = (sbyte)depth;
                            break;
                        }
                        else
                        {
                            Csprun[idx] = (sbyte)depth;
                        }
                    }
                }
            }
        }

        public static void CreateMoveTable()
        {
            Center1 c = new Center1();
            Center1 d = new Center1();
            for (int i = 0; i < 15582; i++)
            {
                d.Set(sym2raw[i]);
                for (int m = 0; m < 36; m++)
                {
                    c.Set(d);
                    c.Move(m);
                    Ctsmv[i][m] = c.Getsym();
                }
            }
        }

        public sbyte[] Ct { get; } = new sbyte[24];

        public Center1()
        {
            for (int i = 0; i < 8; i++)
            {
                Ct[i] = 1;
            }
            for (int i = 8; i < 24; i++)
            {
                Ct[i] = 0;
            }
        }

        public Center1(sbyte[] ct)
        {
            for (int i = 0; i < 24; i++)
            {
                Ct[i] = ct[i];
            }
        }

        public Center1(CenterCube c, int urf)
        {
            for (int i = 0; i < 24; i++)
            {
                Ct[i] = (sbyte)((c.Ct[i] / 2 == urf) ? 1 : 0);
            }
        }

        public void Move(int m)
        {
            int key = m % 3;
            m /= 3;
            switch (m)
            {
                case 0: //U
                    Util.Swap(Ct, 0, 1, 2, 3, key);
                    break;
                case 1: //R
                    Util.Swap(Ct, 16, 17, 18, 19, key);
                    break;
                case 2: //F
                    Util.Swap(Ct, 8, 9, 10, 11, key);
                    break;
                case 3: //D
                    Util.Swap(Ct, 4, 5, 6, 7, key);
                    break;
                case 4: //L
                    Util.Swap(Ct, 20, 21, 22, 23, key);
                    break;
                case 5: //B
                    Util.Swap(Ct, 12, 13, 14, 15, key);
                    break;
                case 6: //u
                    Util.Swap(Ct, 0, 1, 2, 3, key);
                    Util.Swap(Ct, 8, 20, 12, 16, key);
                    Util.Swap(Ct, 9, 21, 13, 17, key);
                    break;
                case 7: //r
                    Util.Swap(Ct, 16, 17, 18, 19, key);
                    Util.Swap(Ct, 1, 15, 5, 9, key);
                    Util.Swap(Ct, 2, 12, 6, 10, key);
                    break;
                case 8: //f
                    Util.Swap(Ct, 8, 9, 10, 11, key);
                    Util.Swap(Ct, 2, 19, 4, 21, key);
                    Util.Swap(Ct, 3, 16, 5, 22, key);
                    break;
                case 9: //d
                    Util.Swap(Ct, 4, 5, 6, 7, key);
                    Util.Swap(Ct, 10, 18, 14, 22, key);
                    Util.Swap(Ct, 11, 19, 15, 23, key);
                    break;
                case 10://l
                    Util.Swap(Ct, 20, 21, 22, 23, key);
                    Util.Swap(Ct, 0, 8, 4, 14, key);
                    Util.Swap(Ct, 3, 11, 7, 13, key);
                    break;
                case 11://b
                    Util.Swap(Ct, 12, 13, 14, 15, key);
                    Util.Swap(Ct, 1, 20, 7, 18, key);
                    Util.Swap(Ct, 0, 23, 6, 17, key);
                    break;
            }
        }

        private void Set(int idx)
        {
            int r = 8;
            for (int i = 23; i >= 0; i--)
            {
                Ct[i] = 0;
                if (idx >= Util.Cnk[i][r])
                {
                    idx -= Util.Cnk[i][r--];
                    Ct[i] = 1;
                }
            }
        }

        private int Get()
        {
            int idx = 0;
            int r = 8;
            for (int i = 23; i >= 0; i--)
            {
                if (Ct[i] == 1)
                {
                    idx += Util.Cnk[i][r--];
                }
            }
            return idx;
        }

        public int Getsym()
        {
            if (Raw2sym != null)
            {
                return Raw2sym[Get()];
            }
            for (int j = 0; j < 48; j++)
            {
                int cord = Raw2symMth(Get());
                if (cord != -1)
                    return cord * 64 + j;
                Rot(0);
                if (j % 2 == 1) Rot(1);
                if (j % 8 == 7) Rot(2);
                if (j % 16 == 15) Rot(3);
            }
            return -1;
        }

        private static int Raw2symMth(int n)
        {
            int m = Array.BinarySearch(sym2raw, n);
            return (m >= 0 ? m : -1);
        }

        private void Set(Center1 c)
        {
            for (int i = 0; i < 24; i++)
            {
                Ct[i] = c.Ct[i];
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
                    Util.Swap(Ct, 0, 3, 1, 2, 1);
                    Util.Swap(Ct, 8, 11, 9, 10, 1);
                    Util.Swap(Ct, 4, 7, 5, 6, 1);
                    Util.Swap(Ct, 12, 15, 13, 14, 1);
                    Util.Swap(Ct, 16, 19, 21, 22, 1);
                    Util.Swap(Ct, 17, 18, 20, 23, 1);
                    break;
                case 3:
                    Move(Moves.ux1);
                    Move(Moves.dx3);
                    Move(Moves.fx1);
                    Move(Moves.bx3);
                    break;
            }
        }
        /*
        0	I
        1	y2
        2	x
        3	xy2
        4	x2
        5	z2
        6	x'
        7	x'y2
        16	yz
        17	y'z'
        18	y2z
        19	z'
        20	y'z
        21	yz'
        22	z
        23	zy2
        32	y'x'
        33	yx
        34	y'
        35	y
        36	y'x
        37	yx'
        38	yz2
        39	y'z2
         */
        public static string[] Rot2str { get; } = {"", "y2", "x", "x y2", "x2", "z2", "x'", "x' y2", "", "", "", "", "", "", "", "",
        "y z", "y' z'", "y2 z", "z'", "y' z", "y z'", "z", "z y2", "", "", "", "", "", "", "", "",
        "y' x'", "y x", "y'", "y", "y' x", "y x'", "y z2", "y' z2",  "", "", "", "", "", "", "", ""};


        private void Rotate(int r)
        {
            for (int j = 0; j < r; j++)
            {
                Rot(0);
                if (j % 2 == 1) Rot(1);
                if (j % 8 == 7) Rot(2);
                if (j % 16 == 15) Rot(3);
            }
        }

        public static int GetSolvedSym(CenterCube cube)
        {
            Center1 c = new Center1(cube.Ct);
            for (int j = 0; j < 48; j++)
            {
                bool check = true;
                for (int i = 0; i < 24; i++)
                {
                    if (c.Ct[i] != i / 4)
                    {
                        check = false;
                        break;
                    }
                }
                if (check)
                {
                    return j;
                }
                c.Rot(0);
                if (j % 2 == 1) c.Rot(1);
                if (j % 8 == 7) c.Rot(2);
                if (j % 16 == 15) c.Rot(3);
            }
            return -1;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (obj is Center1)
            {
                Center1 c = (Center1)obj;
                for (int i = 0; i < 24; i++)
                {
                    if (Ct[i] != c.Ct[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public static void InitSym()
        {
            Center1 c = new Center1();
            for (sbyte i = 0; i < 24; i++)
            {
                c.Ct[i] = i;
            }
            Center1 d = new Center1(c.Ct);
            Center1 e = new Center1(c.Ct);
            Center1 f = new Center1(c.Ct);

            for (int i = 0; i < 48; i++)
            {
                for (int j = 0; j < 48; j++)
                {
                    for (int k = 0; k < 48; k++)
                    {
                        if (c.Equals(d))
                        {
                            Symmult[i][j] = k;
                            if (k == 0)
                            {
                                Syminv[i] = j;
                            }
                        }
                        d.Rot(0);
                        if (k % 2 == 1) d.Rot(1);
                        if (k % 8 == 7) d.Rot(2);
                        if (k % 16 == 15) d.Rot(3);
                    }
                    c.Rot(0);
                    if (j % 2 == 1) c.Rot(1);
                    if (j % 8 == 7) c.Rot(2);
                    if (j % 16 == 15) c.Rot(3);
                }
                c.Rot(0);
                if (i % 2 == 1) c.Rot(1);
                if (i % 8 == 7) c.Rot(2);
                if (i % 16 == 15) c.Rot(3);
            }

            for (int i = 0; i < 48; i++)
            {
                c.Set(e);
                c.Rotate(Syminv[i]);
                for (int j = 0; j < 36; j++)
                {
                    d.Set(c);
                    d.Move(j);
                    d.Rotate(i);
                    for (int k = 0; k < 36; k++)
                    {
                        f.Set(e);
                        f.Move(k);
                        if (f.Equals(d))
                        {
                            Symmove[i][j] = k;
                            break;
                        }
                    }
                }
            }

            c.Set(0);
            for (int i = 0; i < 48; i++)
            {
                Finish[Syminv[i]] = c.Get();
                c.Rot(0);
                if (i % 2 == 1) c.Rot(1);
                if (i % 8 == 7) c.Rot(2);
                if (i % 16 == 15) c.Rot(3);
            }
        }
    }
}
