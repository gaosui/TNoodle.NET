using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs.min2phase
{
    internal class CoordCube
    {
        internal static readonly int N_MOVES = 18;
        internal static readonly int N_MOVES2 = 10;

        internal static readonly int N_SLICE = 495;
        internal static readonly int N_TWIST_SYM = 324;
        internal static readonly int N_FLIP_SYM = 336;
        internal static readonly int N_PERM_SYM = 2768;
        internal static readonly int N_MPERM = 24;

        //phase1
        internal static char[,] UDSliceMove = new char[N_SLICE, N_MOVES];
        internal static char[,] TwistMove = new char[N_TWIST_SYM, N_MOVES];
        internal static char[,] FlipMove = new char[N_FLIP_SYM, N_MOVES];
        internal static char[,] UDSliceConj = new char[N_SLICE, 8];
        internal static int[] UDSliceTwistPrun = new int[N_SLICE * N_TWIST_SYM / 8 + 1];
        internal static int[] UDSliceFlipPrun = new int[N_SLICE * N_FLIP_SYM / 8];
        internal static int[] TwistFlipPrun = Tools.USE_TWIST_FLIP_PRUN ? new int[N_FLIP_SYM * N_TWIST_SYM * 8 / 8] : null;

        //phase2
        internal static char[,] CPermMove = new char[N_PERM_SYM, N_MOVES];
        internal static char[,] EPermMove = new char[N_PERM_SYM, N_MOVES2];
        internal static char[,] MPermMove = new char[N_MPERM, N_MOVES2];
        internal static char[,] MPermConj = new char[N_MPERM, 16];
        internal static int[] MCPermPrun = new int[N_MPERM * N_PERM_SYM / 8];
        internal static int[] MEPermPrun = new int[N_MPERM * N_PERM_SYM / 8];

        internal static void setPruning(int[] table, int index, int value)
        {
            table[index >> 3] ^= (0x0f ^ value) << ((index & 7) << 2);
        }

        internal static int getPruning(int[] table, int index)
        {
            return (table[index >> 3] >> ((index & 7) << 2)) & 0x0f;
        }

        internal static void initUDSliceMoveConj()
        {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();
            for (int i = 0; i < N_SLICE; i++)
            {
                c.setUDSlice(i);
                for (int j = 0; j < N_MOVES; j += 3)
                {
                    CubieCube.EdgeMult(c, CubieCube.moveCube[j], d);
                    UDSliceMove[i, j] = (char)d.getUDSlice();
                }
                for (int j = 0; j < 16; j += 2)
                {
                    CubieCube.EdgeConjugate(c, CubieCube.SymInv[j], d);
                    UDSliceConj[i, (uint)j >> 1] = (char)(d.getUDSlice() & 0x1ff);
                }
            }
            for (int i = 0; i < N_SLICE; i++)
            {
                for (int j = 0; j < N_MOVES; j += 3)
                {
                    int udslice = UDSliceMove[i, j];
                    for (int k = 1; k < 3; k++)
                    {
                        int cx = UDSliceMove[udslice & 0x1ff, j];
                        udslice = Util.permMult[(uint)udslice >> 9, ((uint)cx >> 9)] << 9 | cx & 0x1ff;
                        UDSliceMove[i, j + k] = (char)(udslice);
                    }
                }
            }
        }

        internal static void initFlipMove()
        {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();
            for (int i = 0; i < N_FLIP_SYM; i++)
            {
                c.setFlip(CubieCube.FlipS2R[i]);
                for (int j = 0; j < N_MOVES; j++)
                {
                    CubieCube.EdgeMult(c, CubieCube.moveCube[j], d);
                    FlipMove[i, j] = (char)d.getFlipSym();
                }
            }
        }

        internal static void initTwistMove()
        {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();
            for (int i = 0; i < N_TWIST_SYM; i++)
            {
                c.setTwist(CubieCube.TwistS2R[i]);
                for (int j = 0; j < N_MOVES; j++)
                {
                    CubieCube.CornMult(c, CubieCube.moveCube[j], d);
                    TwistMove[i, j] = (char)d.getTwistSym();
                }
            }
        }

        internal static void initCPermMove()
        {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();
            for (int i = 0; i < N_PERM_SYM; i++)
            {
                c.setCPerm(CubieCube.EPermS2R[i]);
                for (int j = 0; j < N_MOVES; j++)
                {
                    CubieCube.CornMult(c, CubieCube.moveCube[j], d);
                    CPermMove[i, j] = (char)d.getCPermSym();
                }
            }
        }

        internal static void initEPermMove()
        {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();
            for (int i = 0; i < N_PERM_SYM; i++)
            {
                c.setEPerm(CubieCube.EPermS2R[i]);
                for (int j = 0; j < N_MOVES2; j++)
                {
                    CubieCube.EdgeMult(c, CubieCube.moveCube[Util.ud2std[j]], d);
                    EPermMove[i, j] = (char)d.getEPermSym();
                }
            }
        }

        internal static void initMPermMoveConj()
        {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();
            for (int i = 0; i < N_MPERM; i++)
            {
                c.setMPerm(i);
                for (int j = 0; j < N_MOVES2; j++)
                {
                    CubieCube.EdgeMult(c, CubieCube.moveCube[Util.ud2std[j]], d);
                    MPermMove[i, j] = (char)d.getMPerm();
                }
                for (int j = 0; j < 16; j++)
                {
                    CubieCube.EdgeConjugate(c, CubieCube.SymInv[j], d);
                    MPermConj[i, j] = (char)d.getMPerm();
                }
            }
        }

        internal static void initTwistFlipPrun()
        {
            int depth = 0;
            int done = 8;
            bool inv;
            int select;
            int check;
            //		TwistFlipPrun = new int[N_FLIP_SYM * N_TWIST_SYM * 8 / 8];
            for (int i = 0; i < N_FLIP_SYM * N_TWIST_SYM * 8 / 8; i++)
            {
                TwistFlipPrun[i] = -1;
            }
            for (int i = 0; i < 8; i++)
            {
                setPruning(TwistFlipPrun, i, 0);
            }
            while (done < N_FLIP_SYM * N_TWIST_SYM * 8)
            {
                inv = depth > 6;
                select = inv ? 0x0f : depth;
                check = inv ? depth : 0x0f;
                depth++;
                for (int i = 0; i < N_FLIP_SYM * N_TWIST_SYM * 8; i++)
                {
                    if (getPruning(TwistFlipPrun, i) == select)
                    {
                        int twist = i / 2688;
                        int flip = i % 2688;
                        int fsym = i & 7;
                        //flip >>>= 3;
                        flip = (int)((uint)flip >> 3);
                        for (int m = 0; m < N_MOVES; m++)
                        {
                            int twistx = TwistMove[twist, m];
                            int tsymx = twistx & 7;
                            //twistx >>>= 3;
                            twistx = (int)((uint)twistx >> 3);
                            int flipx = FlipMove[flip, CubieCube.Sym8Move[fsym, m]];
                            int fsymx = CubieCube.Sym8MultInv[CubieCube.Sym8Mult[flipx & 7, fsym], tsymx];
                            //flipx >>>= 3;
                            flipx = (int)((uint)flipx >> 3);
                            int idx = ((twistx * 336 + flipx) << 3 | fsymx);
                            if (getPruning(TwistFlipPrun, idx) == check)
                            {
                                done++;
                                if (inv)
                                {
                                    setPruning(TwistFlipPrun, i, depth);
                                    break;
                                }
                                else
                                {
                                    setPruning(TwistFlipPrun, idx, depth);
                                    char sym = CubieCube.SymStateTwist[twistx];
                                    char symF = CubieCube.SymStateFlip[flipx];
                                    if (sym != 1 || symF != 1)
                                    {
                                        for (int j = 0; j < 8; j++, symF >>= 1)
                                        {
                                            if ((symF & 1) == 1)
                                            {
                                                int fsymxx = CubieCube.Sym8MultInv[fsymx, j];
                                                for (int k = 0; k < 8; k++)
                                                {
                                                    if ((sym & (1 << k)) != 0)
                                                    {
                                                        int idxx = twistx * 2688 + (flipx << 3 | CubieCube.Sym8MultInv[fsymxx, k]);
                                                        if (getPruning(TwistFlipPrun, idxx) == 0x0f)
                                                        {
                                                            setPruning(TwistFlipPrun, idxx, depth);
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
                //			System.out.println(String.format("%2d%10d", depth, done));
            }


        }

        internal static void initRawSymPrun(int[] PrunTable, int INV_DEPTH,
          char[,] RawMove, char[,] RawConj,
          char[,] SymMove, char[] SymState,
          sbyte[] SymSwitch, int[] moveMap, int SYM_SHIFT)
        {

            int SYM_MASK = (1 << SYM_SHIFT) - 1;
            int N_RAW = RawMove.Length;
            int N_SYM = SymMove.Length;
            int N_SIZE = N_RAW * N_SYM;
            int N_MOVES = RawMove.GetLength(1);

            for (int i = 0; i < (N_RAW * N_SYM + 7) / 8; i++)
            {
                PrunTable[i] = -1;
            }
            setPruning(PrunTable, 0, 0);

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
                                int symx = SymMove[sym, moveMap == null ? m : moveMap[m]];
                                int rawx = RawConj[RawMove[raw, m] & 0x1ff, symx & SYM_MASK];
                                //symx >>>= SYM_SHIFT;
                                symx = (int)((uint)symx >> SYM_SHIFT);
                                int idx = symx * N_RAW + rawx;
                                if (getPruning(PrunTable, idx) == check)
                                {
                                    done++;
                                    if (inv)
                                    {
                                        setPruning(PrunTable, i, depth);
                                        break;
                                    }
                                    else
                                    {
                                        setPruning(PrunTable, idx, depth);
                                        for (int j = 1, symState = SymState[symx]; (symState >>= 1) != 0; j++)
                                        {
                                            if ((symState & 1) == 1)
                                            {
                                                int idxx = symx * N_RAW + RawConj[rawx, j ^ (SymSwitch == null ? 0 : SymSwitch[j])];
                                                if (getPruning(PrunTable, idxx) == 0x0f)
                                                {
                                                    setPruning(PrunTable, idxx, depth);
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
                //			System.out.println(String.format("%2d%10d", depth, done));
            }

        }
        internal static void initSliceTwistPrun()
        {
            initRawSymPrun(UDSliceTwistPrun, 6,
                UDSliceMove, UDSliceConj,
                TwistMove, CubieCube.SymStateTwist,
                null, null, 3
            );
        }

        internal static void initSliceFlipPrun()
        {
            initRawSymPrun(UDSliceFlipPrun, 6,
                UDSliceMove, UDSliceConj,
                FlipMove, CubieCube.SymStateFlip,
                null, null, 3
            );
        }

        internal static void initMEPermPrun()
        {
            initRawSymPrun(MEPermPrun, 7,
                MPermMove, MPermConj,
                EPermMove, CubieCube.SymStatePerm,
                null, null, 4
            );
        }

        internal static void initMCPermPrun()
        {
            initRawSymPrun(MCPermPrun, 10,
                MPermMove, MPermConj,
                CPermMove, CubieCube.SymStatePerm,
                CubieCube.e2c, Util.ud2std, 4
            );
        }
    }
}
