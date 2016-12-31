using TNoodle.Utils;

namespace TNoodle.Solvers.Threephase
{
    internal static class Moves
    {
        public const sbyte U1 = 0;
        public const sbyte U2 = 1;
        public const sbyte U3 = 2;
        public const sbyte U4 = 3;
        public const sbyte U5 = 4;
        public const sbyte U6 = 5;
        public const sbyte U7 = 6;
        public const sbyte U8 = 7;
        public const sbyte U9 = 8;
        public const sbyte R1 = 9;
        public const sbyte R2 = 10;
        public const sbyte R3 = 11;
        public const sbyte R4 = 12;
        public const sbyte R5 = 13;
        public const sbyte R6 = 14;
        public const sbyte R7 = 15;
        public const sbyte R8 = 16;
        public const sbyte R9 = 17;
        public const sbyte F1 = 18;
        public const sbyte F2 = 19;
        public const sbyte F3 = 20;
        public const sbyte F4 = 21;
        public const sbyte F5 = 22;
        public const sbyte F6 = 23;
        public const sbyte F7 = 24;
        public const sbyte F8 = 25;
        public const sbyte F9 = 26;
        public const sbyte D1 = 27;
        public const sbyte D2 = 28;
        public const sbyte D3 = 29;
        public const sbyte D4 = 30;
        public const sbyte D5 = 31;
        public const sbyte D6 = 32;
        public const sbyte D7 = 33;
        public const sbyte D8 = 34;
        public const sbyte D9 = 35;
        public const sbyte L1 = 36;
        public const sbyte L2 = 37;
        public const sbyte L3 = 38;
        public const sbyte L4 = 39;
        public const sbyte L5 = 40;
        public const sbyte L6 = 41;
        public const sbyte L7 = 42;
        public const sbyte L8 = 43;
        public const sbyte L9 = 44;
        public const sbyte B1 = 45;
        public const sbyte B2 = 46;
        public const sbyte B3 = 47;
        public const sbyte B4 = 48;
        public const sbyte B5 = 49;
        public const sbyte B6 = 50;
        public const sbyte B7 = 51;
        public const sbyte B8 = 52;
        public const sbyte B9 = 53;

        public const sbyte U = 0;
        public const sbyte D = 1;
        public const sbyte F = 2;
        public const sbyte B = 3;
        public const sbyte R = 4;
        public const sbyte L = 5;

        public const int Ux1 = 0;
        public const int Ux2 = 1;
        public const int Ux3 = 2;
        public const int Rx1 = 3;
        public const int Rx2 = 4;
        public const int Rx3 = 5;
        public const int Fx1 = 6;
        public const int Fx2 = 7;
        public const int Fx3 = 8;
        public const int Dx1 = 9;
        public const int Dx2 = 10;
        public const int Dx3 = 11;
        public const int Lx1 = 12;
        public const int Lx2 = 13;
        public const int Lx3 = 14;
        public const int Bx1 = 15;
        public const int Bx2 = 16;
        public const int Bx3 = 17;
        public const int ux1 = 18;
        public const int ux2 = 19;
        public const int ux3 = 20;
        public const int rx1 = 21;
        public const int rx2 = 22;
        public const int rx3 = 23;
        public const int fx1 = 24;
        public const int fx2 = 25;
        public const int fx3 = 26;
        public const int dx1 = 27;
        public const int dx2 = 28;
        public const int dx3 = 29;
        public const int lx1 = 30;
        public const int lx2 = 31;
        public const int lx3 = 32;
        public const int bx1 = 33;
        public const int bx2 = 34;
        public const int bx3 = 35;
        public const int eom = 36;//End Of Moves

        public static string[] Move2str { get; } = {"U  ", "U2 ", "U' ", "R  ", "R2 ", "R' ", "F  ", "F2 ", "F' ",
                                             "D  ", "D2 ", "D' ", "L  ", "L2 ", "L' ", "B  ", "B2 ", "B' ",
                                             "Uw ", "Uw2", "Uw'", "Rw ", "Rw2", "Rw'", "Fw ", "Fw2", "Fw'",
                                             "Dw ", "Dw2", "Dw'", "Lw ", "Lw2", "Lw'", "Bw ", "Bw2", "Bw'"};

        public static int[] Move2std { get; } = {Ux1, Ux2, Ux3, Rx1, Rx2, Rx3, Fx1, Fx2, Fx3,
                             Dx1, Dx2, Dx3, Lx1, Lx2, Lx3, Bx1, Bx2, Bx3,
                             ux2, rx1, rx2, rx3, fx2, dx2, lx1, lx2, lx3, bx2, eom};

        public static int[] Move3std { get; } = {Ux1, Ux2, Ux3, Rx2, Fx1, Fx2, Fx3, Dx1, Dx2, Dx3, Lx2, Bx1, Bx2, Bx3,
                             ux2, rx2, fx2, dx2, lx2, bx2, eom};

        private static readonly int[] std2move = new int[37];
        private static readonly int[] std3move = new int[37];

		public static bool[][] Ckmv { get; } = ArrayExtension.New<bool>(37, 36);
		public static bool[][] Ckmv2 { get; } = ArrayExtension.New<bool>(29, 28);
		public static bool[][] Ckmv3 { get; } = ArrayExtension.New<bool>(21, 20);

        private static int[] skipAxis = new int[36];
        public static int[] SkipAxis2 { get; } = new int[28];
        public static int[] SkipAxis3 { get; } = new int[20];

        static Moves()
        {
            for (int i = 0; i < 29; i++)
            {
                std2move[Move2std[i]] = i;
            }
            for (int i = 0; i < 21; i++)
            {
                std3move[Move3std[i]] = i;
            }
            for (int i = 0; i < 36; i++)
            {
                for (int j = 0; j < 36; j++)
                {
                    Ckmv[i][j] = (i / 3 == j / 3) || ((i / 3 % 3 == j / 3 % 3) && (i > j));
                }
                Ckmv[36][i] = false;
            }
            for (int i = 0; i < 29; i++)
            {
                for (int j = 0; j < 28; j++)
                {
                    Ckmv2[i][j] = Ckmv[Move2std[i]][Move2std[j]];
                }
            }
            for (int i = 0; i < 21; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    Ckmv3[i][j] = Ckmv[Move3std[i]][Move3std[j]];
                }
            }
            for (int i = 0; i < 36; i++)
            {
                skipAxis[i] = 36;
                for (int j = i; j < 36; j++)
                {
                    if (!Ckmv[i][j])
                    {
                        skipAxis[i] = j - 1;
                        break;
                    }
                }
            }
            for (int i = 0; i < 28; i++)
            {
                SkipAxis2[i] = 28;
                for (int j = i; j < 28; j++)
                {
                    if (!Ckmv2[i][j])
                    {
                        SkipAxis2[i] = j - 1;
                        break;
                    }
                }
            }
            for (int i = 0; i < 20; i++)
            {
                SkipAxis3[i] = 20;
                for (int j = i; j < 20; j++)
                {
                    if (!Ckmv3[i][j])
                    {
                        SkipAxis3[i] = j - 1;
                        break;
                    }
                }
            }
        }
    }
}
