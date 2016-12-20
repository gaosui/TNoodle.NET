using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs.min2phase
{
    internal class Util
    {
        //Moves
        internal static readonly sbyte Ux1 = 0;
        internal static readonly sbyte Ux2 = 1;
        internal static readonly sbyte Ux3 = 2;
        internal static readonly sbyte Rx1 = 3;
        internal static readonly sbyte Rx2 = 4;
        internal static readonly sbyte Rx3 = 5;
        internal static readonly sbyte Fx1 = 6;
        internal static readonly sbyte Fx2 = 7;
        internal static readonly sbyte Fx3 = 8;
        internal static readonly sbyte Dx1 = 9;
        internal static readonly sbyte Dx2 = 10;
        internal static readonly sbyte Dx3 = 11;
        internal static readonly sbyte Lx1 = 12;
        internal static readonly sbyte Lx2 = 13;
        internal static readonly sbyte Lx3 = 14;
        internal static readonly sbyte Bx1 = 15;
        internal static readonly sbyte Bx2 = 16;
        internal static readonly sbyte Bx3 = 17;

        //Facelets
        internal static readonly sbyte U1 = 0;
        internal static readonly sbyte U2 = 1;
        internal static readonly sbyte U3 = 2;
        internal static readonly sbyte U4 = 3;
        internal static readonly sbyte U5 = 4;
        internal static readonly sbyte U6 = 5;
        internal static readonly sbyte U7 = 6;
        internal static readonly sbyte U8 = 7;
        internal static readonly sbyte U9 = 8;
        internal static readonly sbyte R1 = 9;
        internal static readonly sbyte R2 = 10;
        internal static readonly sbyte R3 = 11;
        internal static readonly sbyte R4 = 12;
        internal static readonly sbyte R5 = 13;
        internal static readonly sbyte R6 = 14;
        internal static readonly sbyte R7 = 15;
        internal static readonly sbyte R8 = 16;
        internal static readonly sbyte R9 = 17;
        internal static readonly sbyte F1 = 18;
        internal static readonly sbyte F2 = 19;
        internal static readonly sbyte F3 = 20;
        internal static readonly sbyte F4 = 21;
        internal static readonly sbyte F5 = 22;
        internal static readonly sbyte F6 = 23;
        internal static readonly sbyte F7 = 24;
        internal static readonly sbyte F8 = 25;
        internal static readonly sbyte F9 = 26;
        internal static readonly sbyte D1 = 27;
        internal static readonly sbyte D2 = 28;
        internal static readonly sbyte D3 = 29;
        internal static readonly sbyte D4 = 30;
        internal static readonly sbyte D5 = 31;
        internal static readonly sbyte D6 = 32;
        internal static readonly sbyte D7 = 33;
        internal static readonly sbyte D8 = 34;
        internal static readonly sbyte D9 = 35;
        internal static readonly sbyte L1 = 36;
        internal static readonly sbyte L2 = 37;
        internal static readonly sbyte L3 = 38;
        internal static readonly sbyte L4 = 39;
        internal static readonly sbyte L5 = 40;
        internal static readonly sbyte L6 = 41;
        internal static readonly sbyte L7 = 42;
        internal static readonly sbyte L8 = 43;
        internal static readonly sbyte L9 = 44;
        internal static readonly sbyte B1 = 45;
        internal static readonly sbyte B2 = 46;
        internal static readonly sbyte B3 = 47;
        internal static readonly sbyte B4 = 48;
        internal static readonly sbyte B5 = 49;
        internal static readonly sbyte B6 = 50;
        internal static readonly sbyte B7 = 51;
        internal static readonly sbyte B8 = 52;
        internal static readonly sbyte B9 = 53;

        //Colors
        internal static readonly sbyte U = 0;
        internal static readonly sbyte R = 1;
        internal static readonly sbyte F = 2;
        internal static readonly sbyte D = 3;
        internal static readonly sbyte L = 4;
        internal static readonly sbyte B = 5;

        internal static readonly sbyte[,] cornerFacelet = { { U9, R1, F3 }, { U7, F1, L3 }, { U1, L1, B3 }, { U3, B1, R3 },
            { D3, F9, R7 }, { D1, L9, F7 }, { D7, B9, L7 }, { D9, R9, B7 } };

        internal static readonly sbyte[,] edgeFacelet = { { U6, R2 }, { U8, F2 }, { U4, L2 }, { U2, B2 }, { D6, R8 }, { D2, F8 },
            { D4, L8 }, { D8, B8 }, { F6, R4 }, { F4, L6 }, { B6, L4 }, { B4, R6 } };

        internal static int[,] Cnk = new int[12, 12];
        internal static int[] fact = new int[13];
        internal static int[,] permMult = new int[24, 24];
        internal static string[] move2str = {"U", "U2", "U'", "R", "R2", "R'", "F", "F2", "F'",
                                "D", "D2", "D'", "L", "L2", "L'", "B", "B2", "B'"};
        internal static Dictionary<string, int> str2move = new Dictionary<string, int>();
        static Util()
        {
            for (int i = 0; i < move2str.Length; i++)
            {
                str2move.Add(move2str[i], i);
            }

            for (int i = 0; i < 10; i++)
            {
                std2ud[ud2std[i]] = i;
            }
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    int ix = ud2std[i];
                    int jx = ud2std[j];
                    ckmv2[i, j] = (ix / 3 == jx / 3) || ((ix / 3 % 3 == jx / 3 % 3) && (ix >= jx));
                }
                ckmv2[10, i] = false;
            }
            fact[0] = 1;
            for (int i = 0; i < 12; i++)
            {
                Cnk[i, 0] = Cnk[i, i] = 1;
                fact[i + 1] = fact[i] * (i + 1);
                for (int j = 1; j < i; j++)
                {
                    Cnk[i, j] = Cnk[i - 1, j - 1] + Cnk[i - 1, j];
                }
            }
            sbyte[] arr1 = new sbyte[4];
            sbyte[] arr2 = new sbyte[4];
            sbyte[] arr3 = new sbyte[4];
            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < 24; j++)
                {
                    setNPerm(arr1, i, 4);
                    setNPerm(arr2, j, 4);
                    for (int k = 0; k < 4; k++)
                    {
                        arr3[k] = arr1[arr2[k]];
                    }
                    permMult[i, j] = getNPerm(arr3, 4);
                }
            }
        }
        internal static int[] ud2std = { Ux1, Ux2, Ux3, Rx2, Fx2, Dx1, Dx2, Dx3, Lx2, Bx2 };
        internal static int[] std2ud = new int[18];

        internal static bool[,] ckmv2 = new bool[11, 10];

        //TODO:
        internal static void toCubieCube(sbyte[] f, CubieCube ccRet)
        {
            sbyte ori;
            for (int i = 0; i < 8; i++)
                ccRet.cp[i] = 0;// invalidate corners
            for (int i = 0; i < 12; i++)
                ccRet.ep[i] = 0;// and edges
            sbyte col1, col2;
            for (sbyte i = 0; i < 8; i++)
            {
                // get the colors of the cubie at corner i, starting with U/D
                for (ori = 0; ori < 3; ori++)
                    if (f[cornerFacelet[i, ori]] == U || f[cornerFacelet[i, ori]] == D)
                        break;
                col1 = f[cornerFacelet[i, (ori + 1) % 3]];
                col2 = f[cornerFacelet[i, (ori + 2) % 3]];

                for (sbyte j = 0; j < 8; j++)
                {
                    if (col1 == cornerFacelet[j, 1] / 9 && col2 == cornerFacelet[j, 2] / 9)
                    {
                        // in cornerposition i we have cornercubie j
                        ccRet.cp[i] = j;
                        ccRet.co[i] = (sbyte)(ori % 3);
                        break;
                    }
                }
            }
            for (sbyte i = 0; i < 12; i++)
            {
                for (sbyte j = 0; j < 12; j++)
                {
                    if (f[edgeFacelet[i, 0]] == edgeFacelet[j, 0] / 9
                            && f[edgeFacelet[i, 1]] == edgeFacelet[j, 1] / 9)
                    {
                        ccRet.ep[i] = j;
                        ccRet.eo[i] = 0;
                        break;
                    }
                    if (f[edgeFacelet[i, 0]] == edgeFacelet[j, 1] / 9
                            && f[edgeFacelet[i, 1]] == edgeFacelet[j, 0] / 9)
                    {
                        ccRet.ep[i] = j;
                        ccRet.eo[i] = 1;
                        break;
                    }
                }
            }
        }

        internal static string toFaceCube(CubieCube cc)
        {
            char[] f = new char[54];
            char[] ts = { 'U', 'R', 'F', 'D', 'L', 'B' };
            for (int i = 0; i < 54; i++)
            {
                f[i] = ts[i / 9];
            }
            for (sbyte c = 0; c < 8; c++)
            {
                sbyte j = cc.cp[c];// cornercubie with index j is at
                                   // cornerposition with index c
                sbyte ori = cc.co[c];// Orientation of this cubie
                for (sbyte n = 0; n < 3; n++)
                    f[cornerFacelet[c, (n + ori) % 3]] = ts[cornerFacelet[j, n] / 9];
            }
            for (sbyte e = 0; e < 12; e++)
            {
                sbyte j = cc.ep[e];// edgecubie with index j is at edgeposition
                                   // with index e
                sbyte ori = cc.eo[e];// Orientation of this cubie
                for (sbyte n = 0; n < 2; n++)
                    f[edgeFacelet[e, (n + ori) % 2]] = ts[edgeFacelet[j, n] / 9];
            }
            return new string(f);
        }

        internal static int binarySearch(char[] arr, int key)
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

        internal static int getNParity(int idx, int n)
        {
            int p = 0;
            for (int i = n - 2; i >= 0; i--)
            {
                p ^= idx % (n - i);
                idx /= (n - i);
            }
            return p & 1;
        }

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

        internal static int get8Perm(sbyte[] arr)
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

        internal static void setNPerm(sbyte[] arr, int idx, int n)
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

        internal static int getNPerm(sbyte[] arr, int n)
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

        internal static int getComb(sbyte[] arr, int mask)
        {
            int idxC = 0, idxP = 0, r = 4, val = 0x123;
            for (int i = 11; i >= 0; i--)
            {
                if ((arr[i] & 0xc) == mask)
                {
                    int v = (arr[i] & 3) << 2;
                    idxP = r * idxP + ((val >> v) & 0x0f);
                    val -= 0x0111 >> (12 - v);
                    idxC += Cnk[i, r--];
                }
            }
            return idxP << 9 | (494 - idxC);
        }

        internal static void setComb(sbyte[] arr, int idx, int mask)
        {
            int r = 4, fill = 11, val = 0x123;
            int idxC = 494 - (idx & 0x1ff);
            int idxP = (int)((uint)idx >> 9);
            for (int i = 11; i >= 0; i--)
            {
                if (idxC >= Cnk[i, r])
                {
                    idxC -= Cnk[i, r--];
                    int p = fact[r & 3];
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
