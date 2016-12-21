using System;

namespace TNoodle.min2phase
{
    internal class CubieCube
    {
        internal static CubieCube[] CubeSym = new CubieCube[16];

        internal static CubieCube[] moveCube = new CubieCube[18];

        internal static int[] SymInv = new int[16];
        internal static int[,] SymMult = new int[16, 16];
        internal static int[,] SymMove = new int[16, 18];
        internal static int[,] Sym8Mult = new int[8, 8];
        internal static int[,] Sym8Move = new int[8, 18];
        internal static int[,] Sym8MultInv = new int[8, 8];
        internal static int[,] SymMoveUD = new int[16, 10];

        internal static char[] FlipS2R = new char[336];
        internal static char[] TwistS2R = new char[324];
        internal static char[] EPermS2R = new char[2768];

        internal static sbyte[] e2c = { 0, 0, 0, 0, 1, 3, 1, 3, 1, 3, 1, 3, 0, 0, 0, 0 };

        internal static char[] MtoEPerm = new char[40320];

        internal static char[] FlipR2S;
        internal static char[] TwistR2S;
        internal static char[] EPermR2S;

        internal static char[] SymStateTwist = new char[324];
        internal static char[] SymStateFlip = new char[336];
        internal static char[] SymStatePerm = new char[2768];

        internal static CubieCube urf1 = new CubieCube(2531, 1373, 67026819, 1367);
        internal static CubieCube urf2 = new CubieCube(2089, 1906, 322752913, 2040);
        internal static sbyte[][] urfMove =
        {
            new sbyte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9,10,11,12,13,14,15,16,17},
            new sbyte[] {6, 7, 8, 0, 1, 2, 3, 4, 5,15,16,17, 9,10,11,12,13,14},
            new sbyte[] {3, 4, 5, 6, 7, 8, 0, 1, 2,12,13,14,15,16,17, 9,10,11},
            new sbyte[] {2, 1, 0, 5, 4, 3, 8, 7, 6,11,10, 9,14,13,12,17,16,15},
            new sbyte[] {8, 7, 6, 2, 1, 0, 5, 4, 3,17,16,15,11,10, 9,14,13,12},
            new sbyte[] {5, 4, 3, 8, 7, 6, 2, 1, 0,14,13,12,17,16,15,11,10, 9}
        };
        internal static sbyte[][] urfMoveInv = new sbyte[urfMove.Length][];

        internal sbyte[] cp = { 0, 1, 2, 3, 4, 5, 6, 7 };
        internal sbyte[] co = { 0, 0, 0, 0, 0, 0, 0, 0 };
        internal sbyte[] ep = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        internal sbyte[] eo = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        internal CubieCube temps = null;

        static CubieCube()
        {
            for (int urfIdx = 0; urfIdx < urfMove.GetLength(0); urfIdx++)
            {
                sbyte[] urfMoveArr = urfMove[urfIdx];
                sbyte[] urfMoveArrInv = new sbyte[urfMoveArr.Length];
                urfMoveInv[urfIdx] = urfMoveArrInv;
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
            setCPerm(cperm);
            setTwist(twist);
            Util.setNPerm(ep, eperm, 12);
            setFlip(flip);
        }

        internal CubieCube(CubieCube c)
        {
            copy(c);
        }

        internal void copy(CubieCube c)
        {
            for (int i = 0; i < 8; i++)
            {
                cp[i] = c.cp[i];
                co[i] = c.co[i];
            }
            for (int i = 0; i < 12; i++)
            {
                ep[i] = c.ep[i];
                eo[i] = c.eo[i];
            }
        }

        internal void invCubieCube()
        {
            for (sbyte edge = 0; edge < 12; edge++)
                temps.ep[ep[edge]] = edge;
            for (sbyte edge = 0; edge < 12; edge++)
                temps.eo[edge] = eo[temps.ep[edge]];
            for (sbyte corn = 0; corn < 8; corn++)
                temps.cp[cp[corn]] = corn;
            for (sbyte corn = 0; corn < 8; corn++)
            {
                sbyte ori = co[temps.cp[corn]];
                temps.co[corn] = (sbyte)-ori;
                if (temps.co[corn] < 0)
                    temps.co[corn] += 3;
            }
            copy(temps);
        }

        internal static void CornMult(CubieCube a, CubieCube b, CubieCube prod)
        {
            for (int corn = 0; corn < 8; corn++)
            {
                prod.cp[corn] = a.cp[b.cp[corn]];
                sbyte oriA = a.co[b.cp[corn]];
                sbyte oriB = b.co[corn];
                sbyte ori = oriA;
                ori += (oriA < 3) ? oriB : (sbyte)(6 - oriB);
                ori %= 3;
                if ((oriA >= 3) ^ (oriB >= 3))
                {
                    ori += 3;
                }
                prod.co[corn] = ori;
            }
        }

        internal static void EdgeMult(CubieCube a, CubieCube b, CubieCube prod)
        {
            for (int ed = 0; ed < 12; ed++)
            {
                prod.ep[ed] = a.ep[b.ep[ed]];
                prod.eo[ed] = (sbyte)(b.eo[ed] ^ a.eo[b.ep[ed]]);
            }
        }

        internal static void CornConjugate(CubieCube a, int idx, CubieCube b)
        {
            CubieCube sinv = CubeSym[SymInv[idx]];
            CubieCube s = CubeSym[idx];
            for (int corn = 0; corn < 8; corn++)
            {
                b.cp[corn] = sinv.cp[a.cp[s.cp[corn]]];
                sbyte oriA = sinv.co[a.cp[s.cp[corn]]];
                sbyte oriB = a.co[s.cp[corn]];
                b.co[corn] = (sbyte)((oriA < 3) ? oriB : (3 - oriB) % 3);
            }
        }

        internal static void EdgeConjugate(CubieCube a, int idx, CubieCube b)
        {
            CubieCube sinv = CubeSym[SymInv[idx]];
            CubieCube s = CubeSym[idx];
            for (int ed = 0; ed < 12; ed++)
            {
                b.ep[ed] = sinv.ep[a.ep[s.ep[ed]]];
                b.eo[ed] = (sbyte)(s.eo[ed] ^ a.eo[s.ep[ed]] ^ sinv.eo[a.ep[s.ep[ed]]]);
            }
        }

        internal void URFConjugate()
        {
            if (temps == null)
            {
                temps = new CubieCube();
            }
            CornMult(urf2, this, temps);
            CornMult(temps, urf1, this);
            EdgeMult(urf2, this, temps);
            EdgeMult(temps, urf1, this);
        }

        // ********************************************* Get and set coordinates *********************************************
        // XSym : Symmetry Coordnate of X. MUST be called after initialization of ClassIndexToRepresentantArrays.

        // ++++++++++++++++++++ Phase 1 Coordnates ++++++++++++++++++++
        // Flip : Orientation of 12 Edges. Raw[0, 2048) Sym[0, 336 * 8)
        // Twist : Orientation of 8 Corners. Raw[0, 2187) Sym[0, 324 * 8)
        // UDSlice : Positions of the 4 UDSlice edges, the order is ignored. [0, 495)

        internal int getFlip()
        {
            int idx = 0;
            for (int i = 0; i < 11; i++)
            {
                idx <<= 1;
                idx |= eo[i];
            }
            return idx;
        }

        internal void setFlip(int idx)
        {
            int parity = 0;
            for (int i = 10; i >= 0; i--)
            {
                parity ^= eo[i] = (sbyte)(idx & 1);
                idx >>= 1;
            }
            eo[11] = (sbyte)parity;
        }

        internal int getFlipSym()
        {
            if (FlipR2S != null)
            {
                return FlipR2S[getFlip()];
            }
            if (temps == null)
            {
                temps = new CubieCube();
            }
            for (int k = 0; k < 16; k += 2)
            {
                EdgeConjugate(this, SymInv[k], temps);
                int idx = Util.binarySearch(FlipS2R, temps.getFlip());
                if (idx != 0xffff)
                {
                    return (idx << 3) | (k >> 1);
                }
            }
            return 0;
        }

        internal int getTwist()
        {
            int idx = 0;
            for (int i = 0; i < 7; i++)
            {
                idx *= 3;
                idx += co[i];
            }
            return idx;
        }

        internal void setTwist(int idx)
        {
            int twst = 0;
            for (int i = 6; i >= 0; i--)
            {
                twst += co[i] = (sbyte)(idx % 3);
                idx /= 3;
            }
            co[7] = (sbyte)((15 - twst) % 3);
        }

        internal int getTwistSym()
        {
            if (TwistR2S != null)
            {
                return TwistR2S[getTwist()];
            }
            if (temps == null)
            {
                temps = new CubieCube();
            }
            for (int k = 0; k < 16; k += 2)
            {
                CornConjugate(this, SymInv[k], temps);
                int idx = Util.binarySearch(TwistS2R, temps.getTwist());
                if (idx != 0xffff)
                {
                    return (idx << 3) | (k >> 1);
                }
            }
            return 0;
        }

        internal int getUDSlice()
        {
            return Util.getComb(ep, 8);
        }

        internal void setUDSlice(int idx)
        {
            Util.setComb(ep, idx, 8);
        }

        internal int getU4Comb()
        {
            return Util.getComb(ep, 0);
        }

        internal int getD4Comb()
        {
            return Util.getComb(ep, 4);
        }

        // ++++++++++++++++++++ Phase 2 Coordnates ++++++++++++++++++++
        // EPerm : Permutations of 8 UD Edges. Raw[0, 40320) Sym[0, 2187 * 16)
        // Cperm : Permutations of 8 Corners. Raw[0, 40320) Sym[0, 2187 * 16)
        // MPerm : Permutations of 4 UDSlice Edges. [0, 24)

        internal int getCPerm()
        {
            return Util.get8Perm(cp);
        }

        internal void setCPerm(int idx)
        {
            Util.set8Perm(cp, idx);
        }

        internal int getCPermSym()
        {
            if (EPermR2S != null)
            {
                int idx = EPermR2S[getCPerm()];
                idx ^= e2c[idx & 0x0f];
                return idx;
            }
            if (temps == null)
            {
                temps = new CubieCube();
            }
            for (int k = 0; k < 16; k++)
            {
                CornConjugate(this, SymInv[k], temps);
                int idx = Util.binarySearch(EPermS2R, temps.getCPerm());
                if (idx != 0xffff)
                {
                    return (idx << 4) | k;
                }
            }
            return 0;
        }

        internal int getEPerm()
        {
            return Util.get8Perm(ep);
        }

        internal void setEPerm(int idx)
        {
            Util.set8Perm(ep, idx);
        }

        internal int getEPermSym()
        {
            if (EPermR2S != null)
            {
                return EPermR2S[getEPerm()];
            }
            if (temps == null)
            {
                temps = new CubieCube();
            }
            for (int k = 0; k < 16; k++)
            {
                EdgeConjugate(this, SymInv[k], temps);
                int idx = Util.binarySearch(EPermS2R, temps.getEPerm());
                if (idx != 0xffff)
                {
                    return (idx << 4) | k;
                }
            }
            return 0;
        }

        internal int getMPerm()
        {
            return Util.getComb(ep, 8) >> 9;
        }

        internal void setMPerm(int idx)
        {
            Util.setComb(ep, idx << 9, 8);
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
        internal int verify()
        {
            int sum = 0;
            int edgeMask = 0;
            for (int e = 0; e < 12; e++)
                edgeMask |= (1 << ep[e]);
            if (edgeMask != 0x0fff)
                return -2;// missing edges
            for (int i = 0; i < 12; i++)
                sum ^= eo[i];
            if (sum % 2 != 0)
                return -3;
            int cornMask = 0;
            for (int c = 0; c < 8; c++)
                cornMask |= (1 << cp[c]);
            if (cornMask != 0x00ff)
                return -4;// missing corners
            sum = 0;
            for (int i = 0; i < 8; i++)
                sum += co[i];
            if (sum % 3 != 0)
                return -5;// twisted corner
            if ((Util.getNParity(Util.getNPerm(ep, 12), 12) ^ Util.getNParity(getCPerm(), 8)) != 0)
                return -6;// parity error
            return 0;// cube ok
        }

        internal void resolve(Random gen)
        {

        }

        // ********************************************* Initialization functions *********************************************

        internal static void initMove()
        {
            moveCube[0] = new CubieCube(15120, 0, 119750400, 0);
            moveCube[3] = new CubieCube(21021, 1494, 323403417, 0);
            moveCube[6] = new CubieCube(8064, 1236, 29441808, 550);
            moveCube[9] = new CubieCube(9, 0, 5880, 0);
            moveCube[12] = new CubieCube(1230, 412, 2949660, 0);
            moveCube[15] = new CubieCube(224, 137, 328552, 137);
            for (int a = 0; a < 18; a += 3)
            {
                for (int p = 0; p < 2; p++)
                {
                    moveCube[a + p + 1] = new CubieCube();
                    EdgeMult(moveCube[a + p], moveCube[a], moveCube[a + p + 1]);
                    CornMult(moveCube[a + p], moveCube[a], moveCube[a + p + 1]);
                }
            }
        }

        internal static void initSym()
        {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();
            CubieCube t;

            CubieCube f2 = new CubieCube(28783, 0, 259268407, 0);
            CubieCube u4 = new CubieCube(15138, 0, 119765538, 7);
            CubieCube lr2 = new CubieCube(5167, 0, 83473207, 0);
            lr2.co = new sbyte[] { 3, 3, 3, 3, 3, 3, 3, 3 };

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
                        if (CubeSym[k].cp[0] == c.cp[0] && CubeSym[k].cp[1] == c.cp[1] && CubeSym[k].cp[2] == c.cp[2])
                        {
                            SymMult[i, j] = k;
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
                    CornConjugate(moveCube[j], SymInv[s], c);
                    for (int m = 0; m < 18; m++)
                    {
                        for (int i = 0; i < 8; i += 2)
                        {
                            if (c.cp[i] != moveCube[m].cp[i])
                            {

                                goto CONTINUE;
                            }
                        }
                        SymMove[s, j] = m;
                        break;
                    CONTINUE: { }
                    }
                }
            }
            for (int j = 0; j < 10; j++)
            {
                for (int s = 0; s < 16; s++)
                {
                    SymMoveUD[s, j] = Util.std2ud[SymMove[s, Util.ud2std[j]]];
                }
            }
            for (int j = 0; j < 8; j++)
            {
                for (int s = 0; s < 8; s++)
                {
                    Sym8Mult[j, s] = SymMult[j << 1, s << 1] >> 1;
                    Sym8MultInv[j, s] = SymMult[j << 1, SymInv[s << 1]] >> 1;
                }
            }
            for (int j = 0; j < 18; j++)
            {
                for (int s = 0; s < 8; s++)
                {
                    Sym8Move[s, j] = SymMove[s << 1, j];
                }
            }
        }

        internal static void initFlipSym2Raw()
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
                    c.setFlip(i);
                    for (int s = 0; s < 16; s += 2)
                    {
                        EdgeConjugate(c, s, d);
                        int idx = d.getFlip();
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

        internal static void initTwistSym2Raw()
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
                    c.setTwist(i);
                    for (int s = 0; s < 16; s += 2)
                    {
                        CornConjugate(c, s, d);
                        int idx = d.getTwist();
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

        internal static void initPermSym2Raw()
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
                    c.setEPerm(i);
                    for (int s = 0; s < 16; s++)
                    {
                        EdgeConjugate(c, s, d);
                        int idx = d.getEPerm();
                        if (idx == i)
                        {
                            SymStatePerm[count] |= (char)(1 << s);
                        }
                        occ[idx >> 5] |= 1 << (idx & 0x1f);
                        int a = d.getU4Comb();
                        int b = d.getD4Comb() >> 9;
                        int m = 494 - (a & 0x1ff) + (a >> 9) * 70 + b * 1680;
                        MtoEPerm[m] = EPermR2S[idx] = (char)(count << 4 | s);
                    }
                    EPermS2R[count++] = (char)i;
                }
            }
        }
    }
}
