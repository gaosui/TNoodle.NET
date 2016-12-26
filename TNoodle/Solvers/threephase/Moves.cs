using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNoodle.Solvers.threephase
{
    internal static class Moves
    {
        internal const sbyte U1 = 0;
        internal const sbyte U2 = 1;
        internal const sbyte U3 = 2;
        internal const sbyte U4 = 3;
        internal const sbyte U5 = 4;
        internal const sbyte U6 = 5;
        internal const sbyte U7 = 6;
        internal const sbyte U8 = 7;
        internal const sbyte U9 = 8;
        internal const sbyte R1 = 9;
        internal const sbyte R2 = 10;
        internal const sbyte R3 = 11;
        internal const sbyte R4 = 12;
        internal const sbyte R5 = 13;
        internal const sbyte R6 = 14;
        internal const sbyte R7 = 15;
        internal const sbyte R8 = 16;
        internal const sbyte R9 = 17;
        internal const sbyte F1 = 18;
        internal const sbyte F2 = 19;
        internal const sbyte F3 = 20;
        internal const sbyte F4 = 21;
        internal const sbyte F5 = 22;
        internal const sbyte F6 = 23;
        internal const sbyte F7 = 24;
        internal const sbyte F8 = 25;
        internal const sbyte F9 = 26;
        internal const sbyte D1 = 27;
        internal const sbyte D2 = 28;
        internal const sbyte D3 = 29;
        internal const sbyte D4 = 30;
        internal const sbyte D5 = 31;
        internal const sbyte D6 = 32;
        internal const sbyte D7 = 33;
        internal const sbyte D8 = 34;
        internal const sbyte D9 = 35;
        internal const sbyte L1 = 36;
        internal const sbyte L2 = 37;
        internal const sbyte L3 = 38;
        internal const sbyte L4 = 39;
        internal const sbyte L5 = 40;
        internal const sbyte L6 = 41;
        internal const sbyte L7 = 42;
        internal const sbyte L8 = 43;
        internal const sbyte L9 = 44;
        internal const sbyte B1 = 45;
        internal const sbyte B2 = 46;
        internal const sbyte B3 = 47;
        internal const sbyte B4 = 48;
        internal const sbyte B5 = 49;
        internal const sbyte B6 = 50;
        internal const sbyte B7 = 51;
        internal const sbyte B8 = 52;
        internal const sbyte B9 = 53;

        internal const sbyte U = 0;
        internal const sbyte D = 1;
        internal const sbyte F = 2;
        internal const sbyte B = 3;
        internal const sbyte R = 4;
        internal const sbyte L = 5;

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

        public static readonly string[] move2str = {"U  ", "U2 ", "U' ", "R  ", "R2 ", "R' ", "F  ", "F2 ", "F' ",
                                             "D  ", "D2 ", "D' ", "L  ", "L2 ", "L' ", "B  ", "B2 ", "B' ",
                                             "Uw ", "Uw2", "Uw'", "Rw ", "Rw2", "Rw'", "Fw ", "Fw2", "Fw'",
                                             "Dw ", "Dw2", "Dw'", "Lw ", "Lw2", "Lw'", "Bw ", "Bw2", "Bw'"};

        public static readonly string[] moveIstr = {"U' ", "U2 ", "U  ", "R' ", "R2 ", "R  ", "F' ", "F2 ", "F  ",
                                             "D' ", "D2 ", "D  ", "L' ", "L2 ", "L  ", "B' ", "B2 ", "B  ",
                                             "Uw'", "Uw2", "Uw ", "Rw'", "Rw2", "Rw ", "Fw'", "Fw2", "Fw ",
                                             "Dw'", "Dw2", "Dw ", "Lw'", "Lw2", "Lw ", "Bw'", "Bw2", "Bw "};

        internal static int[] move2std = {Ux1, Ux2, Ux3, Rx1, Rx2, Rx3, Fx1, Fx2, Fx3,
                             Dx1, Dx2, Dx3, Lx1, Lx2, Lx3, Bx1, Bx2, Bx3,
                             ux2, rx1, rx2, rx3, fx2, dx2, lx1, lx2, lx3, bx2, eom};

        internal static int[] move3std = {Ux1, Ux2, Ux3, Rx2, Fx1, Fx2, Fx3, Dx1, Dx2, Dx3, Lx2, Bx1, Bx2, Bx3,
                             ux2, rx2, fx2, dx2, lx2, bx2, eom};

        internal static int[] std2move = new int[37];
        internal static int[] std3move = new int[37];

        internal static bool[,] ckmv = new bool[37, 36];
        internal static bool[,] ckmv2 = new bool[29, 28];
        internal static bool[,] ckmv3 = new bool[21, 20];

        internal static int[] skipAxis = new int[36];
        internal static int[] skipAxis2 = new int[28];
        internal static int[] skipAxis3 = new int[20];

        static Moves()
        {
            for (int i = 0; i < 29; i++)
            {
                std2move[move2std[i]] = i;
            }
            for (int i = 0; i < 21; i++)
            {
                std3move[move3std[i]] = i;
            }
            for (int i = 0; i < 36; i++)
            {
                for (int j = 0; j < 36; j++)
                {
                    ckmv[i, j] = (i / 3 == j / 3) || ((i / 3 % 3 == j / 3 % 3) && (i > j));
                }
                ckmv[36, i] = false;
            }
            for (int i = 0; i < 29; i++)
            {
                for (int j = 0; j < 28; j++)
                {
                    ckmv2[i, j] = ckmv[move2std[i], move2std[j]];
                }
            }
            for (int i = 0; i < 21; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    ckmv3[i, j] = ckmv[move3std[i], move3std[j]];
                }
            }
            for (int i = 0; i < 36; i++)
            {
                skipAxis[i] = 36;
                for (int j = i; j < 36; j++)
                {
                    if (!ckmv[i, j])
                    {
                        skipAxis[i] = j - 1;
                        break;
                    }
                }
            }
            for (int i = 0; i < 28; i++)
            {
                skipAxis2[i] = 28;
                for (int j = i; j < 28; j++)
                {
                    if (!ckmv2[i, j])
                    {
                        skipAxis2[i] = j - 1;
                        break;
                    }
                }
            }
            for (int i = 0; i < 20; i++)
            {
                skipAxis3[i] = 20;
                for (int j = i; j < 20; j++)
                {
                    if (!ckmv3[i, j])
                    {
                        skipAxis3[i] = j - 1;
                        break;
                    }
                }
            }
        }
    }
}
