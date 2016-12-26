using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNoodle.Solvers.sq12phase
{
    internal class Square
    {
        internal int edgeperm;       //number encoding the edge permutation 0-40319
        internal int cornperm;       //number encoding the corner permutation 0-40319
        internal bool topEdgeFirst;   //true if top layer starts with edge left of seam
        internal bool botEdgeFirst;   //true if bottom layer starts with edge right of seam
        internal int ml;         //shape of middle layer (+/-1, or 0 if ignored)

        internal static sbyte[] SquarePrun = new sbyte[40320 * 2];         //pruning table; #twists to solve corner|edge permutation

        internal static char[] TwistMove = new char[40320];          //transition table for twists
        internal static char[] TopMove = new char[40320];            //transition table for top layer turns
        internal static char[] BottomMove = new char[40320];         //transition table for bottom layer turns

        private static int[] fact = { 1, 1, 2, 6, 24, 120, 720, 5040 };

        internal static void set8Perm(sbyte[] arr, int idx)
        {
            int val = 0x76543210;
            for (int i = 0; i < 7; i++)
            {
                int p = fact[7 - i];
                int v = idx / p;
                idx -= v * p;
                v <<= 2;
                arr[i] = (sbyte)((val >> v) & 07);
                int m = (1 << v) - 1;
                val = (val & m) + ((val >> 4) & ~m);
            }
            arr[7] = (sbyte)val;
        }

        internal static char get8Perm(sbyte[] arr)
        {
            int idx = 0;
            int val = 0x76543210;
            for (int i = 0; i < 7; i++)
            {
                int v = arr[i] << 2;
                idx = (8 - i) * idx + ((val >> v) & 07);
                val -= 0x11111110 << v;
            }
            return (char)idx;
        }

        internal static int[,] Cnk = new int[12, 12];

        internal static int get8Comb(byte[] arr)
        {
            int idx = 0, r = 4;
            for (int i = 0; i < 8; i++)
            {
                if (arr[i] >= 4)
                {
                    idx += Cnk[7 - i, r--];
                }
            }
            return idx;
        }

        internal static bool inited = false;

        internal static void init()
        {
            if (inited)
            {
                return;
            }
            for (int i = 0; i < 12; i++)
            {
                Cnk[i, 0] = 1;
                Cnk[i, i] = 1;
                for (int j = 1; j < i; j++)
                {
                    Cnk[i, j] = Cnk[i - 1, j - 1] + Cnk[i - 1, j];
                }
            }
            sbyte[] pos = new sbyte[8];
            sbyte temp;

            for (int i = 0; i < 40320; i++)
            {
                //twist
                set8Perm(pos, i);

                temp = pos[2]; pos[2] = pos[4]; pos[4] = temp;
                temp = pos[3]; pos[3] = pos[5]; pos[5] = temp;
                TwistMove[i] = get8Perm(pos);

                //top layer turn
                set8Perm(pos, i);
                temp = pos[0]; pos[0] = pos[1]; pos[1] = pos[2]; pos[2] = pos[3]; pos[3] = temp;
                TopMove[i] = get8Perm(pos);

                //bottom layer turn
                set8Perm(pos, i);
                temp = pos[4]; pos[4] = pos[5]; pos[5] = pos[6]; pos[6] = pos[7]; pos[7] = temp;
                BottomMove[i] = get8Perm(pos);
            }

            for (int i = 0; i < 40320 * 2; i++)
            {
                SquarePrun[i] = -1;
            }
            SquarePrun[0] = 0;
            int depth = 0;
            int done = 1;
            while (done < 40320 * 2)
            {
                bool inv = depth >= 11;
                int find = inv ? -1 : depth;
                int check = inv ? depth : -1;
                ++depth;
                //OUT:
                for (int i = 0; i < 40320 * 2; i++)
                {
                    if (SquarePrun[i] == find)
                    {
                        int idx = i >> 1;
                        int ml = i & 1;

                        //try twist
                        int idxx = TwistMove[idx] << 1 | (1 - ml);
                        if (SquarePrun[idxx] == check)
                        {
                            ++done;
                            SquarePrun[inv ? i : idxx] = (sbyte)(depth);
                            if (inv)
                            {
                                goto OUT;
                            }
                        }

                        //try turning top layer
                        idxx = idx;
                        for (int m = 0; m < 4; m++)
                        {
                            idxx = TopMove[idxx];
                            if (SquarePrun[idxx << 1 | ml] == check)
                            {
                                ++done;
                                SquarePrun[inv ? i : (idxx << 1 | ml)] = (sbyte)(depth);
                                if (inv)
                                {
                                    goto OUT;
                                }
                            }
                        }
                        //assert idxx == idx;
                        //try turning bottom layer
                        for (int m = 0; m < 4; m++)
                        {
                            idxx = BottomMove[idxx];
                            if (SquarePrun[idxx << 1 | ml] == check)
                            {
                                ++done;
                                SquarePrun[inv ? i : (idxx << 1 | ml)] = (sbyte)(depth);
                                if (inv)
                                {
                                    goto OUT;
                                }
                            }
                        }

                    }
                OUT: { }
                }
                //System.out.print(depth);
                //System.out.print('\t');
                //System.out.println(done);
            }
            inited = true;
        }

    }

}
