using System.Collections.Generic;
using TNoodle.Utils;

namespace TNoodle.Solvers.Min2phase
{
    internal static class Util
    {
        //Moves
        internal const sbyte Ux1 = 0;
        internal const sbyte Ux2 = 1;
        internal const sbyte Ux3 = 2;
        internal const sbyte Rx1 = 3;
        internal const sbyte Rx2 = 4;
        internal const sbyte Rx3 = 5;
        internal const sbyte Fx1 = 6;
        internal const sbyte Fx2 = 7;
        internal const sbyte Fx3 = 8;
        internal const sbyte Dx1 = 9;
        internal const sbyte Dx2 = 10;
        internal const sbyte Dx3 = 11;
        internal const sbyte Lx1 = 12;
        internal const sbyte Lx2 = 13;
        internal const sbyte Lx3 = 14;
        internal const sbyte Bx1 = 15;
        internal const sbyte Bx2 = 16;
        internal const sbyte Bx3 = 17;

        //Facelets
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

        //Colors
        internal const sbyte U = 0;
        internal const sbyte R = 1;
        internal const sbyte F = 2;
        internal const sbyte D = 3;
        internal const sbyte L = 4;
        internal const sbyte B = 5;

        internal static sbyte[][] CornerFacelet { get; } =
        {
            new sbyte[] { U9, R1, F3 },
            new sbyte[] { U7, F1, L3 },
            new sbyte[] { U1, L1, B3 },
            new sbyte[] { U3, B1, R3 },
            new sbyte[] { D3, F9, R7 },
            new sbyte[] { D1, L9, F7 },
            new sbyte[] { D7, B9, L7 },
            new sbyte[] { D9, R9, B7 }
        };

        internal static sbyte[][] EdgeFacelet { get; } =
        {
            new sbyte[] { U6, R2 },
            new sbyte[] { U8, F2 },
            new sbyte[] { U4, L2 },
            new sbyte[] { U2, B2 },
            new sbyte[] { D6, R8 },
            new sbyte[] { D2, F8 },
            new sbyte[] { D4, L8 },
            new sbyte[] { D8, B8 },
            new sbyte[] { F6, R4 },
            new sbyte[] { F4, L6 },
            new sbyte[] { B6, L4 },
            new sbyte[] { B4, R6 }
        };

		internal static int[][] Cnk { get; } = ArrayExtension.New<int>(12, 12);
        internal static int[] Fact { get; } = new int[13];
		internal static int[][] PermMult { get; } = ArrayExtension.New<int>(24, 24);
        internal static string[] Move2str { get; } =
        {
            "U", "U2", "U'", "R", "R2", "R'", "F", "F2", "F'",
            "D", "D2", "D'", "L", "L2", "L'", "B", "B2", "B'"
        };
        internal static Dictionary<string, int> Str2move { get; } = new Dictionary<string, int>();

        internal static int[] Ud2std { get; } = { Ux1, Ux2, Ux3, Rx2, Fx2, Dx1, Dx2, Dx3, Lx2, Bx2 };
        internal static int[] Std2ud { get; } = new int[18];

		internal static bool[][] Ckmv2 { get; } = ArrayExtension.New<bool>(11, 10);

        static Util()
        {
            for (int i = 0; i < Move2str.Length; i++)
            {
                Str2move[Move2str[i]] = i;
            }

            for (int i = 0; i < 10; i++)
            {
                Std2ud[Ud2std[i]] = i;
            }
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    int ix = Ud2std[i];
                    int jx = Ud2std[j];
                    Ckmv2[i][j] = (ix / 3 == jx / 3) || ((ix / 3 % 3 == jx / 3 % 3) && (ix >= jx));
                }
                Ckmv2[10][i] = false;
            }
            Fact[0] = 1;
            for (int i = 0; i < 12; i++)
            {
                Cnk[i][0] = Cnk[i][i] = 1;
                Fact[i + 1] = Fact[i] * (i + 1);
                for (int j = 1; j < i; j++)
                {
                    Cnk[i][j] = Cnk[i - 1][j - 1] + Cnk[i - 1][j];
                }
            }
            sbyte[] arr1 = new sbyte[4];
            sbyte[] arr2 = new sbyte[4];
            sbyte[] arr3 = new sbyte[4];
            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < 24; j++)
                {
                    SetNPerm(arr1, i, 4);
                    SetNPerm(arr2, j, 4);
                    for (int k = 0; k < 4; k++)
                    {
                        arr3[k] = arr1[arr2[k]];
                    }
                    PermMult[i][j] = GetNPerm(arr3, 4);
                }
            }
        }

        internal static void ToCubieCube(sbyte[] f, CubieCube ccRet)
        {
            sbyte ori;
            for (int i = 0; i < 8; i++)
                ccRet.CP[i] = 0;// invalidate corners
            for (int i = 0; i < 12; i++)
                ccRet.EP[i] = 0;// and edges
            sbyte col1, col2;
            for (sbyte i = 0; i < 8; i++)
            {
                // get the colors of the cubie at corner i, starting with U/D
                for (ori = 0; ori < 3; ori++)
                    if (f[CornerFacelet[i][ori]] == U || f[CornerFacelet[i][ori]] == D)
                        break;
                col1 = f[CornerFacelet[i][(ori + 1) % 3]];
                col2 = f[CornerFacelet[i][(ori + 2) % 3]];

                for (sbyte j = 0; j < 8; j++)
                {
                    if (col1 == CornerFacelet[j][1] / 9 && col2 == CornerFacelet[j][2] / 9)
                    {
                        // in cornerposition i we have cornercubie j
                        ccRet.CP[i] = j;
                        ccRet.CO[i] = (sbyte)(ori % 3);
                        break;
                    }
                }
            }
            for (sbyte i = 0; i < 12; i++)
            {
                for (sbyte j = 0; j < 12; j++)
                {
                    if (f[EdgeFacelet[i][0]] == EdgeFacelet[j][0] / 9
                            && f[EdgeFacelet[i][1]] == EdgeFacelet[j][1] / 9)
                    {
                        ccRet.EP[i] = j;
                        ccRet.EO[i] = 0;
                        break;
                    }
                    if (f[EdgeFacelet[i][0]] == EdgeFacelet[j][1] / 9
                            && f[EdgeFacelet[i][1]] == EdgeFacelet[j][0] / 9)
                    {
                        ccRet.EP[i] = j;
                        ccRet.EO[i] = 1;
                        break;
                    }
                }
            }
        }

        internal static string ToFaceCube(CubieCube cc)
        {
            char[] f = new char[54];
            char[] ts = { 'U', 'R', 'F', 'D', 'L', 'B' };
            for (int i = 0; i < 54; i++)
            {
                f[i] = ts[i / 9];
            }
            for (sbyte c = 0; c < 8; c++)
            {
                sbyte j = cc.CP[c];// cornercubie with index j is at
                                   // cornerposition with index c
                sbyte ori = cc.CO[c];// Orientation of this cubie
                for (sbyte n = 0; n < 3; n++)
                    f[CornerFacelet[c][(n + ori) % 3]] = ts[CornerFacelet[j][n] / 9];
            }
            for (sbyte e = 0; e < 12; e++)
            {
                sbyte j = cc.EP[e];// edgecubie with index j is at edgeposition
                                   // with index e
                sbyte ori = cc.EO[e];// Orientation of this cubie
                for (sbyte n = 0; n < 2; n++)
                    f[EdgeFacelet[e][(n + ori) % 2]] = ts[EdgeFacelet[j][n] / 9];
            }
            return new string(f);
        }

        internal static int BinarySearch(char[] arr, int key)
        {
            int length = arr.Length;
            if (key <= arr[length - 1])
            {
                int l = 0;
                int r = length - 1;
                while (l <= r)
                {
                    int mid = (int)((uint)(l + r) >> 1);
                    char val = arr[mid];
                    if (key > val)
                    {
                        l = mid + 1;
                    }
                    else if (key < val)
                    {
                        r = mid - 1;
                    }
                    else
                    {
                        return mid;
                    }
                }
            }
            return 0xffff;
        }

        internal static int GetNParity(int idx, int n)
        {
            int p = 0;
            for (int i = n - 2; i >= 0; i--)
            {
                p ^= idx % (n - i);
                idx /= (n - i);
            }
            return p & 1;
        }

        internal static void Set8Perm(sbyte[] arr, int idx)
        {
            int val = 0x76543210;
            for (int i = 0; i < 7; i++)
            {
                int p = Fact[7 - i];
                int v = idx / p;
                idx -= v * p;
                v <<= 2;
                arr[i] = (sbyte)((val >> v) & 07);
                int m = (1 << v) - 1;
                val = (val & m) + ((val >> 4) & ~m);
            }
            arr[7] = (sbyte)val;
        }

        internal static int Get8Perm(sbyte[] arr)
        {
            int idx = 0;
            int val = 0x76543210;
            for (int i = 0; i < 7; i++)
            {
                int v = arr[i] << 2;
                idx = (8 - i) * idx + ((val >> v) & 07);
                val -= 0x11111110 << v;
            }
            return idx;
        }

        internal static void SetNPerm(sbyte[] arr, int idx, int n)
        {
            arr[n - 1] = 0;
            for (int i = n - 2; i >= 0; i--)
            {
                arr[i] = (sbyte)(idx % (n - i));
                idx /= (n - i);
                for (int j = i + 1; j < n; j++)
                {
                    if (arr[j] >= arr[i])
                        arr[j]++;
                }
            }
        }

        internal static int GetNPerm(sbyte[] arr, int n)
        {
            int idx = 0;
            for (int i = 0; i < n; i++)
            {
                idx *= (n - i);
                for (int j = i + 1; j < n; j++)
                {
                    if (arr[j] < arr[i])
                    {
                        idx++;
                    }
                }
            }
            return idx;
        }

        internal static int GetComb(sbyte[] arr, int mask)
        {
            int idxC = 0, idxP = 0, r = 4, val = 0x123;
            for (int i = 11; i >= 0; i--)
            {
                if ((arr[i] & 0xc) == mask)
                {
                    int v = (arr[i] & 3) << 2;
                    idxP = r * idxP + ((val >> v) & 0x0f);
                    val -= 0x0111 >> (12 - v);
                    idxC += Cnk[i][r--];
                }
            }
            return idxP << 9 | (494 - idxC);
        }

        internal static void SetComb(sbyte[] arr, int idx, int mask)
        {
            int r = 4, fill = 11, val = 0x123;
            int idxC = 494 - (idx & 0x1ff);
            int idxP = (int)((uint)idx >> 9);
            for (int i = 11; i >= 0; i--)
            {
                if (idxC >= Cnk[i][r])
                {
                    idxC -= Cnk[i][r--];
                    int p = Fact[r & 3];
                    int v = idxP / p << 2;
                    idxP %= p;
                    arr[i] = (sbyte)((val >> v) & 3 | mask);
                    int m = (1 << v) - 1;
                    val = (val & m) + ((val >> 4) & ~m);
                }
                else
                {
                    if ((fill & 0xc) == mask)
                    {
                        fill -= 4;
                    }
                    arr[i] = (sbyte)(fill--);
                }
            }
        }
    }
}
