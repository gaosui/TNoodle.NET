using System;
using TNoodle.Utils;

namespace TNoodle.Solvers.sq12phase
{
    internal class Square
    {
        private static readonly int[] Fact = {1, 1, 2, 6, 24, 120, 720, 5040};

        private static bool _inited;
        internal int Edgeperm { get; set; }
        internal int Cornperm { get; set; }
        internal bool TopEdgeFirst { get; set; }
        internal bool BotEdgeFirst { get; set; }
        internal int Ml { get; set; }

        internal static sbyte[] SquarePrun { get; } = new sbyte[40320 * 2];

        internal static char[] TwistMove { get; } = new char[40320];
        internal static char[] TopMove { get; } = new char[40320];
        internal static char[] BottomMove { get; } = new char[40320];

        internal static int[][] Cnk { get; } = ArrayExtension.New<int>(12, 12);
        public static event Action<string> Log;

        internal static void Set8Perm(sbyte[] arr, int idx)
        {
            var val = 0x76543210;
            for (var i = 0; i < 7; i++)
            {
                var p = Fact[7 - i];
                var v = idx / p;
                idx -= v * p;
                v <<= 2;
                arr[i] = (sbyte) ((val >> v) & 07);
                var m = (1 << v) - 1;
                val = (val & m) + ((val >> 4) & ~m);
            }
            arr[7] = (sbyte) val;
        }

        internal static char Get8Perm(sbyte[] arr)
        {
            var idx = 0;
            var val = 0x76543210;
            for (var i = 0; i < 7; i++)
            {
                var v = arr[i] << 2;
                idx = (8 - i) * idx + ((val >> v) & 07);
                val -= 0x11111110 << v;
            }
            return (char) idx;
        }

        internal static int Get8Comb(byte[] arr)
        {
            int idx = 0, r = 4;
            for (var i = 0; i < 8; i++)
                if (arr[i] >= 4)
                    idx += Cnk[7 - i][r--];
            return idx;
        }

        internal static void Init()
        {
            if (_inited)
                return;
            for (var i = 0; i < 12; i++)
            {
                Cnk[i][0] = 1;
                Cnk[i][i] = 1;
                for (var j = 1; j < i; j++)
                    Cnk[i][j] = Cnk[i - 1][j - 1] + Cnk[i - 1][j];
            }
            var pos = new sbyte[8];

            for (var i = 0; i < 40320; i++)
            {
                //twist
                Set8Perm(pos, i);

                var temp = pos[2];
                pos[2] = pos[4];
                pos[4] = temp;
                temp = pos[3];
                pos[3] = pos[5];
                pos[5] = temp;
                TwistMove[i] = Get8Perm(pos);

                //top layer turn
                Set8Perm(pos, i);
                temp = pos[0];
                pos[0] = pos[1];
                pos[1] = pos[2];
                pos[2] = pos[3];
                pos[3] = temp;
                TopMove[i] = Get8Perm(pos);

                //bottom layer turn
                Set8Perm(pos, i);
                temp = pos[4];
                pos[4] = pos[5];
                pos[5] = pos[6];
                pos[6] = pos[7];
                pos[7] = temp;
                BottomMove[i] = Get8Perm(pos);
            }

            for (var i = 0; i < 40320 * 2; i++)
                SquarePrun[i] = -1;
            SquarePrun[0] = 0;
            var depth = 0;
            var done = 1;
            while (done < 40320 * 2)
            {
                var inv = depth >= 11;
                var find = inv ? -1 : depth;
                var check = inv ? depth : -1;
                ++depth;
                //OUT:
                for (var i = 0; i < 40320 * 2; i++)
                {
                    if (SquarePrun[i] == find)
                    {
                        var idx = i >> 1;
                        var ml = i & 1;

                        //try twist
                        var idxx = (TwistMove[idx] << 1) | (1 - ml);
                        if (SquarePrun[idxx] == check)
                        {
                            ++done;
                            SquarePrun[inv ? i : idxx] = (sbyte) depth;
                            if (inv)
                                goto OUT;
                        }

                        //try turning top layer
                        idxx = idx;
                        for (var m = 0; m < 4; m++)
                        {
                            idxx = TopMove[idxx];
                            if (SquarePrun[(idxx << 1) | ml] != check) continue;
                            ++done;
                            SquarePrun[inv ? i : (idxx << 1) | ml] = (sbyte) depth;
                            if (inv)
                                goto OUT;
                        }
                        //assert idxx == idx;
                        //try turning bottom layer
                        for (var m = 0; m < 4; m++)
                        {
                            idxx = BottomMove[idxx];
                            if (SquarePrun[(idxx << 1) | ml] != check) continue;
                            ++done;
                            SquarePrun[inv ? i : (idxx << 1) | ml] = (sbyte) depth;
                            if (inv)
                                goto OUT;
                        }
                    }
                    OUT:
                    ;
                }
                //System.out.print(depth);
                //System.out.print('\t');
                //System.out.println(done);
                Log?.Invoke(depth.ToString());
                Log?.Invoke('\t'.ToString());
                Log?.Invoke(done.ToString());
            }
            _inited = true;
        }
    }
}