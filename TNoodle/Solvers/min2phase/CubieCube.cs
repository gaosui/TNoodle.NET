using System;
using TNoodle.Utils;

namespace TNoodle.Solvers.Min2phase
{
    internal class CubieCube
    {
        internal static CubieCube[] CubeSym { get; } = new CubieCube[16];

        internal static CubieCube[] MoveCube { get; } = new CubieCube[18];

        internal static int[] SymInv { get; } = new int[16];
		internal static int[][] SymMult { get; } = ArrayExtension.New<int>(16, 16);
		internal static int[][] SymMove { get; } = ArrayExtension.New<int>(16, 18);
		internal static int[][] Sym8Mult { get; } = ArrayExtension.New<int>(8, 8);
		internal static int[][] Sym8Move { get; } = ArrayExtension.New<int>(8, 18);
		internal static int[][] Sym8MultInv { get; } = ArrayExtension.New<int>(8, 8);
		internal static int[][] SymMoveUD { get; } = ArrayExtension.New<int>(16, 10);

        internal static char[] FlipS2R { get; } = new char[336];
        internal static char[] TwistS2R { get; } = new char[324];
        internal static char[] EPermS2R { get; } = new char[2768];

        internal static sbyte[] E2C { get; } = { 0, 0, 0, 0, 1, 3, 1, 3, 1, 3, 1, 3, 0, 0, 0, 0 };

        internal static char[] MtoEPerm { get; } = new char[40320];

        internal static char[] FlipR2S { get; private set; }
        internal static char[] TwistR2S { get; private set; }
        internal static char[] EPermR2S { get; private set; }

        internal static char[] SymStateTwist { get; } = new char[324];
        internal static char[] SymStateFlip { get; } = new char[336];
        internal static char[] SymStatePerm { get; } = new char[2768];

        internal static CubieCube URF1 { get; } = new CubieCube(2531, 1373, 67026819, 1367);
        internal static CubieCube URF2 { get; } = new CubieCube(2089, 1906, 322752913, 2040);
        internal static sbyte[][] URFMove { get; } =
        {
            new sbyte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9,10,11,12,13,14,15,16,17},
            new sbyte[] {6, 7, 8, 0, 1, 2, 3, 4, 5,15,16,17, 9,10,11,12,13,14},
            new sbyte[] {3, 4, 5, 6, 7, 8, 0, 1, 2,12,13,14,15,16,17, 9,10,11},
            new sbyte[] {2, 1, 0, 5, 4, 3, 8, 7, 6,11,10, 9,14,13,12,17,16,15},
            new sbyte[] {8, 7, 6, 2, 1, 0, 5, 4, 3,17,16,15,11,10, 9,14,13,12},
            new sbyte[] {5, 4, 3, 8, 7, 6, 2, 1, 0,14,13,12,17,16,15,11,10, 9}
        };
        internal static sbyte[][] URFMoveInv { get; } = new sbyte[URFMove.Length][];

        internal sbyte[] CP { get; } = { 0, 1, 2, 3, 4, 5, 6, 7 };
        internal sbyte[] CO { get; private set; } = { 0, 0, 0, 0, 0, 0, 0, 0 };
        internal sbyte[] EP { get; } = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        internal sbyte[] EO { get; } = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private CubieCube temps;

        static CubieCube()
        {
            for (int urfIdx = 0; urfIdx < URFMove.Length; urfIdx++)
            {
                sbyte[] urfMoveArr = URFMove[urfIdx];
                sbyte[] urfMoveArrInv = new sbyte[urfMoveArr.Length];
                URFMoveInv[urfIdx] = urfMoveArrInv;
                for (sbyte m = 0; m < urfMoveArr.Length; m++)
                {
                    urfMoveArrInv[urfMoveArr[m]] = m;
                }
            }
        }

        internal CubieCube()
        {
        }

        internal CubieCube(int cperm, int twist, int eperm, int flip)
        {
            SetCPerm(cperm);
            SetTwist(twist);
            Util.SetNPerm(EP, eperm, 12);
            SetFlip(flip);
        }

        internal CubieCube(CubieCube c)
        {
            Copy(c);
        }

        private void Copy(CubieCube c)
        {
            for (int i = 0; i < 8; i++)
            {
                CP[i] = c.CP[i];
                CO[i] = c.CO[i];
            }
            for (int i = 0; i < 12; i++)
            {
                EP[i] = c.EP[i];
                EO[i] = c.EO[i];
            }
        }

        internal void InvCubieCube()
        {
            for (sbyte edge = 0; edge < 12; edge++)
                temps.EP[EP[edge]] = edge;
            for (sbyte edge = 0; edge < 12; edge++)
                temps.EO[edge] = EO[temps.EP[edge]];
            for (sbyte corn = 0; corn < 8; corn++)
                temps.CP[CP[corn]] = corn;
            for (sbyte corn = 0; corn < 8; corn++)
            {
                sbyte ori = CO[temps.CP[corn]];
                temps.CO[corn] = (sbyte)-ori;
                if (temps.CO[corn] < 0)
                    temps.CO[corn] += 3;
            }
            Copy(temps);
        }

        internal static void CornMult(CubieCube a, CubieCube b, CubieCube prod)
        {
            for (int corn = 0; corn < 8; corn++)
            {
                prod.CP[corn] = a.CP[b.CP[corn]];
                sbyte oriA = a.CO[b.CP[corn]];
                sbyte oriB = b.CO[corn];
                sbyte ori = oriA;
                ori += (oriA < 3) ? oriB : (sbyte)(6 - oriB);
                ori %= 3;
                if ((oriA >= 3) ^ (oriB >= 3))
                {
                    ori += 3;
                }
                prod.CO[corn] = ori;
            }
        }

        internal static void EdgeMult(CubieCube a, CubieCube b, CubieCube prod)
        {
            for (int ed = 0; ed < 12; ed++)
            {
                prod.EP[ed] = a.EP[b.EP[ed]];
                prod.EO[ed] = (sbyte)(b.EO[ed] ^ a.EO[b.EP[ed]]);
            }
        }

        internal static void CornConjugate(CubieCube a, int idx, CubieCube b)
        {
            CubieCube sinv = CubeSym[SymInv[idx]];
            CubieCube s = CubeSym[idx];
            for (int corn = 0; corn < 8; corn++)
            {
                b.CP[corn] = sinv.CP[a.CP[s.CP[corn]]];
                sbyte oriA = sinv.CO[a.CP[s.CP[corn]]];
                sbyte oriB = a.CO[s.CP[corn]];
                b.CO[corn] = (oriA < 3) ? oriB : (sbyte)((3 - oriB) % 3);
            }
        }

        internal static void EdgeConjugate(CubieCube a, int idx, CubieCube b)
        {
            CubieCube sinv = CubeSym[SymInv[idx]];
            CubieCube s = CubeSym[idx];
            for (int ed = 0; ed < 12; ed++)
            {
                b.EP[ed] = sinv.EP[a.EP[s.EP[ed]]];
                b.EO[ed] = (sbyte)(s.EO[ed] ^ a.EO[s.EP[ed]] ^ sinv.EO[a.EP[s.EP[ed]]]);
            }
        }

        internal void URFConjugate()
        {
            if (temps == null)
            {
                temps = new CubieCube();
            }
            CornMult(URF2, this, temps);
            CornMult(temps, URF1, this);
            EdgeMult(URF2, this, temps);
            EdgeMult(temps, URF1, this);
        }

        // ********************************************* Get and set coordinates *********************************************
        // XSym : Symmetry Coordnate of X. MUST be called after initialization of ClassIndexToRepresentantArrays.

        // ++++++++++++++++++++ Phase 1 Coordnates ++++++++++++++++++++
        // Flip : Orientation of 12 Edges. Raw[0, 2048) Sym[0, 336 * 8)
        // Twist : Orientation of 8 Corners. Raw[0, 2187) Sym[0, 324 * 8)
        // UDSlice : Positions of the 4 UDSlice edges, the order is ignored. [0, 495)

        internal int GetFlip()
        {
            int idx = 0;
            for (int i = 0; i < 11; i++)
            {
                idx <<= 1;
                idx |= EO[i];
            }
            return idx;
        }

        internal void SetFlip(int idx)
        {
            int parity = 0;
            for (int i = 10; i >= 0; i--)
            {
                parity ^= EO[i] = (sbyte)(idx & 1);
                idx >>= 1;
            }
            EO[11] = (sbyte)parity;
        }

        internal int GetFlipSym()
        {
            if (FlipR2S != null)
            {
                return FlipR2S[GetFlip()];
            }
            if (temps == null)
            {
                temps = new CubieCube();
            }
            for (int k = 0; k < 16; k += 2)
            {
                EdgeConjugate(this, SymInv[k], temps);
                int idx = Util.BinarySearch(FlipS2R, temps.GetFlip());
                if (idx != 0xffff)
                {
                    return (idx << 3) | (k >> 1);
                }
            }
            return 0;
        }

        internal int GetTwist()
        {
            int idx = 0;
            for (int i = 0; i < 7; i++)
            {
                idx *= 3;
                idx += CO[i];
            }
            return idx;
        }

        internal void SetTwist(int idx)
        {
            int twst = 0;
            for (int i = 6; i >= 0; i--)
            {
                twst += CO[i] = (sbyte)(idx % 3);
                idx /= 3;
            }
            CO[7] = (sbyte)((15 - twst) % 3);
        }

        internal int GetTwistSym()
        {
            if (TwistR2S != null)
            {
                return TwistR2S[GetTwist()];
            }
            if (temps == null)
            {
                temps = new CubieCube();
            }
            for (int k = 0; k < 16; k += 2)
            {
                CornConjugate(this, SymInv[k], temps);
                int idx = Util.BinarySearch(TwistS2R, temps.GetTwist());
                if (idx != 0xffff)
                {
                    return (idx << 3) | (k >> 1);
                }
            }
            return 0;
        }

        internal int GetUDSlice()
        {
            return Util.GetComb(EP, 8);
        }

        internal void SetUDSlice(int idx)
        {
            Util.SetComb(EP, idx, 8);
        }

        internal int GetU4Comb()
        {
            return Util.GetComb(EP, 0);
        }

        internal int GetD4Comb()
        {
            return Util.GetComb(EP, 4);
        }

        // ++++++++++++++++++++ Phase 2 Coordnates ++++++++++++++++++++
        // EPerm : Permutations of 8 UD Edges. Raw[0, 40320) Sym[0, 2187 * 16)
        // Cperm : Permutations of 8 Corners. Raw[0, 40320) Sym[0, 2187 * 16)
        // MPerm : Permutations of 4 UDSlice Edges. [0, 24)

        internal int GetCPerm()
        {
            return Util.Get8Perm(CP);
        }

        internal void SetCPerm(int idx)
        {
            Util.Set8Perm(CP, idx);
        }

        internal int GetCPermSym()
        {
            if (EPermR2S != null)
            {
                int idx = EPermR2S[GetCPerm()];
                idx ^= E2C[idx & 0x0f];
                return idx;
            }
            if (temps == null)
            {
                temps = new CubieCube();
            }
            for (int k = 0; k < 16; k++)
            {
                CornConjugate(this, SymInv[k], temps);
                int idx = Util.BinarySearch(EPermS2R, temps.GetCPerm());
                if (idx != 0xffff)
                {
                    return (idx << 4) | k;
                }
            }
            return 0;
        }

        internal int GetEPerm()
        {
            return Util.Get8Perm(EP);
        }

        internal void SetEPerm(int idx)
        {
            Util.Set8Perm(EP, idx);
        }

        internal int GetEPermSym()
        {
            if (EPermR2S != null)
            {
                return EPermR2S[GetEPerm()];
            }
            if (temps == null)
            {
                temps = new CubieCube();
            }
            for (int k = 0; k < 16; k++)
            {
                EdgeConjugate(this, SymInv[k], temps);
                int idx = Util.BinarySearch(EPermS2R, temps.GetEPerm());
                if (idx != 0xffff)
                {
                    return (idx << 4) | k;
                }
            }
            return 0;
        }

        internal int GetMPerm()
        {
            return Util.GetComb(EP, 8) >> 9;
        }

        internal void SetMPerm(int idx)
        {
            Util.SetComb(EP, idx << 9, 8);
        }

        /**
         * Check a cubiecube for solvability. Return the error code.
         * 0: Cube is solvable
         * -2: Not all 12 edges exist exactly once
         * -3: Flip error: One edge has to be flipped
         * -4: Not all corners exist exactly once
         * -5: Twist error: One corner has to be twisted
         * -6: Parity error: Two corners or two edges have to be exchanged
         */
        internal int Verify()
        {
            int sum = 0;
            int edgeMask = 0;
            for (int e = 0; e < 12; e++)
                edgeMask |= (1 << EP[e]);
            if (edgeMask != 0x0fff)
                return -2;// missing edges
            for (int i = 0; i < 12; i++)
                sum ^= EO[i];
            if (sum % 2 != 0)
                return -3;
            int cornMask = 0;
            for (int c = 0; c < 8; c++)
                cornMask |= (1 << CP[c]);
            if (cornMask != 0x00ff)
                return -4;// missing corners
            sum = 0;
            for (int i = 0; i < 8; i++)
                sum += CO[i];
            if (sum % 3 != 0)
                return -5;// twisted corner
            if ((Util.GetNParity(Util.GetNPerm(EP, 12), 12) ^ Util.GetNParity(GetCPerm(), 8)) != 0)
                return -6;// parity error
            return 0;// cube ok
        }

        // ********************************************* Initialization functions *********************************************

        internal static void InitMove()
        {
            MoveCube[0] = new CubieCube(15120, 0, 119750400, 0);
            MoveCube[3] = new CubieCube(21021, 1494, 323403417, 0);
            MoveCube[6] = new CubieCube(8064, 1236, 29441808, 550);
            MoveCube[9] = new CubieCube(9, 0, 5880, 0);
            MoveCube[12] = new CubieCube(1230, 412, 2949660, 0);
            MoveCube[15] = new CubieCube(224, 137, 328552, 137);
            for (int a = 0; a < 18; a += 3)
            {
                for (int p = 0; p < 2; p++)
                {
                    MoveCube[a + p + 1] = new CubieCube();
                    EdgeMult(MoveCube[a + p], MoveCube[a], MoveCube[a + p + 1]);
                    CornMult(MoveCube[a + p], MoveCube[a], MoveCube[a + p + 1]);
                }
            }
        }

        internal static void InitSym()
        {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();
            CubieCube t;

            CubieCube f2 = new CubieCube(28783, 0, 259268407, 0);
            CubieCube u4 = new CubieCube(15138, 0, 119765538, 7);
            CubieCube lr2 = new CubieCube(5167, 0, 83473207, 0);
            lr2.CO = new sbyte[] { 3, 3, 3, 3, 3, 3, 3, 3 };

            for (int i = 0; i < 16; i++)
            {
                CubeSym[i] = new CubieCube(c);
                CornMult(c, u4, d);
                EdgeMult(c, u4, d);
                t = d; d = c; c = t;
                if (i % 4 == 3)
                {
                    CornMult(c, lr2, d);
                    EdgeMult(c, lr2, d);
                    t = d; d = c; c = t;
                }
                if (i % 8 == 7)
                {
                    CornMult(c, f2, d);
                    EdgeMult(c, f2, d);
                    t = d; d = c; c = t;
                }
            }
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    CornMult(CubeSym[i], CubeSym[j], c);
                    for (int k = 0; k < 16; k++)
                    {
                        if (CubeSym[k].CP[0] == c.CP[0] && CubeSym[k].CP[1] == c.CP[1] && CubeSym[k].CP[2] == c.CP[2])
                        {
                            SymMult[i][j] = k;
                            if (k == 0)
                            {
                                SymInv[i] = j;
                            }
                            break;
                        }
                    }
                }
            }
            for (int j = 0; j < 18; j++)
            {
                for (int s = 0; s < 16; s++)
                {
                    CornConjugate(MoveCube[j], SymInv[s], c);
                    for (int m = 0; m < 18; m++)
                    {
                        for (int i = 0; i < 8; i += 2)
                        {
                            if (c.CP[i] != MoveCube[m].CP[i])
                            {
                                goto CONTINUE;
                            }
                        }
                        SymMove[s][j] = m;
                        break;
                    CONTINUE: { }
                    }
                }
            }
            for (int j = 0; j < 10; j++)
            {
                for (int s = 0; s < 16; s++)
                {
                    SymMoveUD[s][j] = Util.Std2ud[SymMove[s][Util.Ud2std[j]]];
                }
            }
            for (int j = 0; j < 8; j++)
            {
                for (int s = 0; s < 8; s++)
                {
                    Sym8Mult[j][s] = SymMult[j << 1][s << 1] >> 1;
                    Sym8MultInv[j][s] = SymMult[j << 1][SymInv[s << 1]] >> 1;
                }
            }
            for (int j = 0; j < 18; j++)
            {
                for (int s = 0; s < 8; s++)
                {
                    Sym8Move[s][j] = SymMove[s << 1][j];
                }
            }
        }

        internal static void InitFlipSym2Raw()
        {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();
            int[] occ = new int[2048 >> 5];
            int count = 0;
            for (int i = 0; i < 2048 >> 5; occ[i++] = 0) ;
            FlipR2S = new char[2048];
            for (int i = 0; i < 2048; i++)
            {
                if ((occ[i >> 5] & (1 << (i & 0x1f))) == 0)
                {
                    c.SetFlip(i);
                    for (int s = 0; s < 16; s += 2)
                    {
                        EdgeConjugate(c, s, d);
                        int idx = d.GetFlip();
                        if (idx == i)
                        {
                            SymStateFlip[count] |= (char)(1 << (s >> 1));
                        }
                        occ[idx >> 5] |= 1 << (idx & 0x1f);
                        FlipR2S[idx] = (char)((count << 3) | (s >> 1));
                    }
                    FlipS2R[count++] = (char)i;
                }
            }
        }

        internal static void InitTwistSym2Raw()
        {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();
            int[] occ = new int[2187 / 32 + 1];
            int count = 0;
            for (int i = 0; i < 2187 / 32 + 1; occ[i++] = 0) ;
            TwistR2S = new char[2187];
            for (int i = 0; i < 2187; i++)
            {
                if ((occ[i >> 5] & (1 << (i & 0x1f))) == 0)
                {
                    c.SetTwist(i);
                    for (int s = 0; s < 16; s += 2)
                    {
                        CornConjugate(c, s, d);
                        int idx = d.GetTwist();
                        if (idx == i)
                        {
                            SymStateTwist[count] |= (char)(1 << (s >> 1));
                        }
                        occ[idx >> 5] |= 1 << (idx & 0x1f);
                        TwistR2S[idx] = (char)((count << 3) | (s >> 1));
                    }
                    TwistS2R[count++] = (char)i;
                }
            }
        }

        internal static void InitPermSym2Raw()
        {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();
            int[] occ = new int[40320 / 32];
            int count = 0;
            for (int i = 0; i < 40320 / 32; occ[i++] = 0) ;
            EPermR2S = new char[40320];
            for (int i = 0; i < 40320; i++)
            {
                if ((occ[i >> 5] & (1 << (i & 0x1f))) == 0)
                {
                    c.SetEPerm(i);
                    for (int s = 0; s < 16; s++)
                    {
                        EdgeConjugate(c, s, d);
                        int idx = d.GetEPerm();
                        if (idx == i)
                        {
                            SymStatePerm[count] |= (char)(1 << s);
                        }
                        occ[idx >> 5] |= 1 << (idx & 0x1f);
                        int a = d.GetU4Comb();
                        int b = d.GetD4Comb() >> 9;
                        int m = 494 - (a & 0x1ff) + (a >> 9) * 70 + b * 1680;
                        MtoEPerm[m] = EPermR2S[idx] = (char)(count << 4 | s);
                    }
                    EPermS2R[count++] = (char)i;
                }
            }
        }
    }
}
