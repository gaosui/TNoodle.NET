using System;
using TNoodle.Utils;

namespace TNoodle.Solvers.Min2phase
{
    internal static class CoordCube
    {
        internal const int N_MOVES = 18;
        internal const int N_MOVES2 = 10;

        internal const int N_SLICE = 495;
        internal const int N_TWIST_SYM = 324;
        internal const int N_FLIP_SYM = 336;
        internal const int N_PERM_SYM = 2768;
        internal const int N_MPERM = 24;

        //phase1
		internal static char[][] UDSliceMove { get; } = ArrayExtension.New<char>(N_SLICE, N_MOVES);
		internal static char[][] TwistMove { get; } = ArrayExtension.New<char>(N_TWIST_SYM, N_MOVES);
		internal static char[][] FlipMove { get; } = ArrayExtension.New<char>(N_FLIP_SYM, N_MOVES);
		internal static char[][] UDSliceConj { get; } = ArrayExtension.New<char>(N_SLICE, 8);
        internal static int[] UDSliceTwistPrun { get; } = new int[N_SLICE * N_TWIST_SYM / 8 + 1];
        internal static int[] UDSliceFlipPrun { get; } = new int[N_SLICE * N_FLIP_SYM / 8];
        internal static int[] TwistFlipPrun { get; } = Tools.USE_TWIST_FLIP_PRUN ? new int[N_FLIP_SYM * N_TWIST_SYM * 8 / 8] : null;

        //phase2
		internal static char[][] CPermMove { get; } = ArrayExtension.New<char>(N_PERM_SYM, N_MOVES);
		internal static char[][] EPermMove { get; } = ArrayExtension.New<char>(N_PERM_SYM, N_MOVES2);
		internal static char[][] MPermMove { get; } = ArrayExtension.New<char>(N_MPERM, N_MOVES2);
		internal static char[][] MPermConj { get; } = ArrayExtension.New<char>(N_MPERM, 16);
        internal static int[] MCPermPrun { get; } = new int[N_MPERM * N_PERM_SYM / 8];
        internal static int[] MEPermPrun { get; } = new int[N_MPERM * N_PERM_SYM / 8];

        internal static void SetPruning(int[] table, int index, int value)
        {
            table[index >> 3] ^= (0x0f ^ value) << ((index & 7) << 2);
        }

        internal static int GetPruning(int[] table, int index)
        {
            return (table[index >> 3] >> ((index & 7) << 2)) & 0x0f;
        }

        internal static void InitUDSliceMoveConj()
        {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();
            for (int i = 0; i < N_SLICE; i++)
            {
                c.SetUDSlice(i);
                for (int j = 0; j < N_MOVES; j += 3)
                {
                    CubieCube.EdgeMult(c, CubieCube.MoveCube[j], d);
                    UDSliceMove[i][j] = (char)d.GetUDSlice();
                }
                for (uint j = 0; j < 16; j += 2)
                {
                    CubieCube.EdgeConjugate(c, CubieCube.SymInv[j], d);
                    UDSliceConj[i][j >> 1] = (char)(d.GetUDSlice() & 0x1ff);
                }
            }
            for (int i = 0; i < N_SLICE; i++)
            {
                for (int j = 0; j < N_MOVES; j += 3)
                {
                    int udslice = UDSliceMove[i][j];
                    for (int k = 1; k < 3; k++)
                    {
                        int cx = UDSliceMove[udslice & 0x1ff][j];
                        udslice = Util.PermMult[(uint)udslice >> 9][((uint)cx >> 9)] << 9 | cx & 0x1ff;
                        UDSliceMove[i][j + k] = (char)(udslice);
                    }
                }
            }
        }

        internal static void InitFlipMove()
        {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();
            for (int i = 0; i < N_FLIP_SYM; i++)
            {
                c.SetFlip(CubieCube.FlipS2R[i]);
                for (int j = 0; j < N_MOVES; j++)
                {
                    CubieCube.EdgeMult(c, CubieCube.MoveCube[j], d);
                    FlipMove[i][j] = (char)d.GetFlipSym();
                }
            }
        }

        internal static void InitTwistMove()
        {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();
            for (int i = 0; i < N_TWIST_SYM; i++)
            {
                c.SetTwist(CubieCube.TwistS2R[i]);
                for (int j = 0; j < N_MOVES; j++)
                {
                    CubieCube.CornMult(c, CubieCube.MoveCube[j], d);
                    TwistMove[i][j] = (char)d.GetTwistSym();
                }
            }
        }

        internal static void InitCPermMove()
        {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();
            for (int i = 0; i < N_PERM_SYM; i++)
            {
                c.SetCPerm(CubieCube.EPermS2R[i]);
                for (int j = 0; j < N_MOVES; j++)
                {
                    CubieCube.CornMult(c, CubieCube.MoveCube[j], d);
                    CPermMove[i][j] = (char)d.GetCPermSym();
                }
            }
        }

        internal static void InitEPermMove()
        {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();
            for (int i = 0; i < N_PERM_SYM; i++)
            {
                c.SetEPerm(CubieCube.EPermS2R[i]);
                for (int j = 0; j < N_MOVES2; j++)
                {
                    CubieCube.EdgeMult(c, CubieCube.MoveCube[Util.Ud2std[j]], d);
                    EPermMove[i][j] = (char)d.GetEPermSym();
                }
            }
        }

        internal static void InitMPermMoveConj()
        {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();
            for (int i = 0; i < N_MPERM; i++)
            {
                c.SetMPerm(i);
                for (int j = 0; j < N_MOVES2; j++)
                {
                    CubieCube.EdgeMult(c, CubieCube.MoveCube[Util.Ud2std[j]], d);
                    MPermMove[i][j] = (char)d.GetMPerm();
                }
                for (int j = 0; j < 16; j++)
                {
                    CubieCube.EdgeConjugate(c, CubieCube.SymInv[j], d);
                    MPermConj[i][j] = (char)d.GetMPerm();
                }
            }
        }

        internal static void InitTwistFlipPrun()
        {
            int depth = 0;
            int done = 8;
            bool inv;
            int select;
            int check;
            for (int i = 0; i < N_FLIP_SYM * N_TWIST_SYM * 8 / 8; i++)
            {
                TwistFlipPrun[i] = -1;
            }
            for (int i = 0; i < 8; i++)
            {
                SetPruning(TwistFlipPrun, i, 0);
            }
            while (done < N_FLIP_SYM * N_TWIST_SYM * 8)
            {
                inv = depth > 6;
                select = inv ? 0x0f : depth;
                check = inv ? depth : 0x0f;
                depth++;
                for (int i = 0; i < N_FLIP_SYM * N_TWIST_SYM * 8; i++)
                {
                    if (GetPruning(TwistFlipPrun, i) == select)
                    {
                        int twist = i / 2688;
                        int flip = i % 2688;
                        int fsym = i & 7;
                        flip = (int)((uint)flip >> 3);
                        for (int m = 0; m < N_MOVES; m++)
                        {
                            int twistx = TwistMove[twist][m];
                            int tsymx = twistx & 7;
                            twistx = (int)((uint)twistx >> 3);
                            int flipx = FlipMove[flip][CubieCube.Sym8Move[fsym][m]];
                            int fsymx = CubieCube.Sym8MultInv[CubieCube.Sym8Mult[flipx & 7][fsym]][tsymx];
                            flipx = (int)((uint)flipx >> 3);
                            int idx = ((twistx * 336 + flipx) << 3 | fsymx);
                            if (GetPruning(TwistFlipPrun, idx) == check)
                            {
                                done++;
                                if (inv)
                                {
                                    SetPruning(TwistFlipPrun, i, depth);
                                    break;
                                }
                                else
                                {
                                    SetPruning(TwistFlipPrun, idx, depth);
                                    char sym = CubieCube.SymStateTwist[twistx];
                                    char symF = CubieCube.SymStateFlip[flipx];
                                    if (sym != 1 || symF != 1)
                                    {
                                        for (int j = 0; j < 8; j++, symF >>= 1)
                                        {
                                            if ((symF & 1) == 1)
                                            {
                                                int fsymxx = CubieCube.Sym8MultInv[fsymx][j];
                                                for (int k = 0; k < 8; k++)
                                                {
                                                    if ((sym & (1 << k)) != 0)
                                                    {
                                                        int idxx = twistx * 2688 + (flipx << 3 | CubieCube.Sym8MultInv[fsymxx][k]);
                                                        if (GetPruning(TwistFlipPrun, idxx) == 0x0f)
                                                        {
                                                            SetPruning(TwistFlipPrun, idxx, depth);
                                                            done++;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static void InitRawSymPrun(int[] PrunTable, int INV_DEPTH,
            char[][] RawMove, char[][] RawConj,
            char[][] SymMove, char[] SymState,
            sbyte[] SymSwitch, int[] moveMap, int SYM_SHIFT)
        {

            int SYM_MASK = (1 << SYM_SHIFT) - 1;
            int N_RAW = RawMove.Length;
            int N_SYM = SymMove.Length;
            int N_SIZE = N_RAW * N_SYM;
            int N_MOVES = RawMove[0].Length;

            for (int i = 0; i < (N_RAW * N_SYM + 7) / 8; i++)
            {
                PrunTable[i] = -1;
            }
            SetPruning(PrunTable, 0, 0);

            int depth = 0;
            int done = 1;

            while (done < N_SIZE)
            {
                bool inv = depth > INV_DEPTH;
                int select = inv ? 0x0f : depth;
                int check = inv ? depth : 0x0f;
                depth++;
                for (int i = 0; i < N_SIZE;)
                {
                    int val = PrunTable[i >> 3];
                    if (!inv && val == -1)
                    {
                        i += 8;
                        continue;
                    }
                    for (int end = Math.Min(i + 8, N_SIZE); i < end; i++, val >>= 4)
                    {
                        if ((val & 0x0f)/*getPruning(PrunTable, i)*/ == select)
                        {
                            int raw = i % N_RAW;
                            int sym = i / N_RAW;
                            for (int m = 0; m < N_MOVES; m++)
                            {
                                int symx = SymMove[sym][moveMap == null ? m : moveMap[m]];
                                int rawx = RawConj[RawMove[raw][m] & 0x1ff][symx & SYM_MASK];
                                symx = (int)((uint)symx >> SYM_SHIFT);
                                int idx = symx * N_RAW + rawx;
                                if (GetPruning(PrunTable, idx) == check)
                                {
                                    done++;
                                    if (inv)
                                    {
                                        SetPruning(PrunTable, i, depth);
                                        break;
                                    }
                                    else
                                    {
                                        SetPruning(PrunTable, idx, depth);
                                        for (int j = 1, symState = SymState[symx]; (symState >>= 1) != 0; j++)
                                        {
                                            if ((symState & 1) == 1)
                                            {
                                                int idxx = symx * N_RAW + RawConj[rawx][j ^ (SymSwitch == null ? 0 : SymSwitch[j])];
                                                if (GetPruning(PrunTable, idxx) == 0x0f)
                                                {
                                                    SetPruning(PrunTable, idxx, depth);
                                                    done++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static void InitSliceTwistPrun()
        {
            InitRawSymPrun(UDSliceTwistPrun, 6,
                UDSliceMove, UDSliceConj,
                TwistMove, CubieCube.SymStateTwist,
                null, null, 3
            );
        }

        internal static void InitSliceFlipPrun()
        {
            InitRawSymPrun(UDSliceFlipPrun, 6,
                UDSliceMove, UDSliceConj,
                FlipMove, CubieCube.SymStateFlip,
                null, null, 3
            );
        }

        internal static void InitMEPermPrun()
        {
            InitRawSymPrun(MEPermPrun, 7,
                MPermMove, MPermConj,
                EPermMove, CubieCube.SymStatePerm,
                null, null, 4
            );
        }

        internal static void InitMCPermPrun()
        {
            InitRawSymPrun(MCPermPrun, 10,
                MPermMove, MPermConj,
                CPermMove, CubieCube.SymStatePerm,
                CubieCube.E2C, Util.Ud2std, 4
            );
        }
    }
}
