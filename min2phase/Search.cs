﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs.min2phase
{
    public class Search
    {
        private int[] move = new int[31];

        private int[] corn = new int[20];
        private int[] mid4 = new int[20];
        private int[] ud8e = new int[20];

        private int[] twist = new int[6];
        private int[] flip = new int[6];
        private int[] slice = new int[6];

        private int[] corn0 = new int[6];
        private int[] ud8e0 = new int[6];
        private int[] prun = new int[6];

        private sbyte[] f = new sbyte[54];

        private int urfIdx;
        private int depth1;
        private int maxDep2;
        private int sol;
        private int valid1;
        private int valid2;
        private string _solution;
        private long timeOut;
        private long timeMin;
        private int verbose;
        private int firstAxisRestriction;
        private int lastAxisRestriction;
        private CubieCube cc = new CubieCube();

        /**
         *     Verbose_Mask determines if a " . " separates the phase1 and phase2 parts of the solver string like in F' R B R L2 F .
         *     U2 U D for example.<br>
         */
        public static readonly int USE_SEPARATOR = 0x1;

        /**
         *     Verbose_Mask determines if the solution will be inversed to a scramble/state generator.
         */
        public static readonly int INVERSE_SOLUTION = 0x2;

        /**
         *     Verbose_Mask determines if a tag such as "(21f)" will be appended to the solution.
         */
        public static readonly int APPEND_LENGTH = 0x4;

        /**
         * Computes the solver string for a given cube.
         *
         * @param facelets
         * 		is the cube definition string format.<br>
         * The names of the facelet positions of the cube:
         * <pre>
         *	     |************|
         *	     |*U1**U2**U3*|
         *	     |************|
         *	     |*U4**U5**U6*|
         *	     |************|
         *	     |*U7**U8**U9*|
         *	     |************|
         * ************|************|************|************|
         * *L1**L2**L3*|*F1**F2**F3*|*R1**R2**F3*|*B1**B2**B3*|
         * ************|************|************|************|
         * *L4**L5**L6*|*F4**F5**F6*|*R4**R5**R6*|*B4**B5**B6*|
         * ************|************|************|************|
         * *L7**L8**L9*|*F7**F8**F9*|*R7**R8**R9*|*B7**B8**B9*|
         * ************|************|************|************|
         *	     |************|
         *	     |*D1**D2**D3*|
         *	     |************|
         *	     |*D4**D5**D6*|
         *	     |************|
         *	     |*D7**D8**D9*|
         *	     |************|
         * </pre>
         * A cube definition string "UBL..." means for example: In position U1 we have the U-color, in position U2 we have the
         * B-color, in position U3 we have the L color etc. according to the order U1, U2, U3, U4, U5, U6, U7, U8, U9, R1, R2,
         * R3, R4, R5, R6, R7, R8, R9, F1, F2, F3, F4, F5, F6, F7, F8, F9, D1, D2, D3, D4, D5, D6, D7, D8, D9, L1, L2, L3, L4,
         * L5, L6, L7, L8, L9, B1, B2, B3, B4, B5, B6, B7, B8, B9 of the enum constants.
         *
         * @param maxDepth
         * 		defines the maximal allowed maneuver length. For random cubes, a maxDepth of 21 usually will return a
         * 		solution in less than 0.02 seconds on average. With a maxDepth of 20 it takes about 0.1 seconds on average to find a
         * 		solution, but it may take much longer for specific cubes.
         *
         * @param timeOut
         * 		defines the maximum computing time of the method in milliseconds. If it does not return with a solution, it returns with
         * 		an error code.
         *
         * @param timeMin
         * 		defines the minimum computing time of the method in milliseconds. So, if a solution is found within given time, the
         * 		computing will continue to find shorter solution(s). Btw, if timeMin > timeOut, timeMin will be set to timeOut.
         *
         * @param verbose
         * 		determines the format of the solution(s). see USE_SEPARATOR, INVERSE_SOLUTION, APPEND_LENGTH
         *
         * @param firstAxisRestrictionStr
         *	      The solution generated will not start by turning
         *	      any face on the axis of firstAxisRestrictionStr.
         *
         * @param lastAxisRestrictionStr
         *	      The solution generated will not end by turning
         *	      any face on the axis of lastAxisRestrictionStr.
         *
         * @return The solution string or an error code:<br>
         * 		Error 1: There is not exactly one facelet of each colour<br>
         * 		Error 2: Not all 12 edges exist exactly once<br>
         * 		Error 3: Flip error: One edge has to be flipped<br>
         * 		Error 4: Not all corners exist exactly once<br>
         * 		Error 5: Twist error: One corner has to be twisted<br>
         * 		Error 6: Parity error: Two corners or two edges have to be exchanged<br>
         * 		Error 7: No solution exists for the given maxDepth<br>
         * 		Error 8: Timeout, no solution within given time<br>
         * 		Error 9: Invalid firstAxisRestrictionStr or lastAxisRestrictionStr
         */
        public string solution(string facelets, int maxDepth, long timeOut, long timeMin, int verbose, string firstAxisRestrictionStr, string lastAxisRestrictionStr)
        {
            int check = verify(facelets);
            if (check != 0)
            {
                return "Error " + Math.Abs(check);
            }
            this.sol = maxDepth + 1;
            this.timeOut = currentTimeMillis() + timeOut;
            this.timeMin = this.timeOut + Math.Min(timeMin - timeOut, 0);
            this.verbose = verbose;
            this._solution = null;
            this.firstAxisRestriction = -1;
            this.lastAxisRestriction = -1;
            if (firstAxisRestrictionStr != null)
            {
                if (!Util.str2move.ContainsKey(firstAxisRestrictionStr))
                {
                    return "Error 9";
                }
                firstAxisRestriction = Util.str2move[firstAxisRestrictionStr];
                if (firstAxisRestriction % 3 != 0)
                {
                    return "Error 9";
                }
                if (firstAxisRestriction - 9 < 0)
                {
                    // firstAxisRestriction defines an axis of turns that
                    // aren't permitted. Make sure we restrict the entire
                    // axis, and not just one of the faces. See the axis
                    // filtering in phase1() for more details.
                    firstAxisRestriction += 9;
                }
            }
            if (lastAxisRestrictionStr != null)
            {
                if (!Util.str2move.ContainsKey(lastAxisRestrictionStr))
                {
                    return "Error 9";
                }
                lastAxisRestriction = Util.str2move[lastAxisRestrictionStr];
                if (lastAxisRestriction % 3 != 0)
                {
                    return "Error 9";
                }
                if (lastAxisRestriction - 9 < 0)
                {
                    // lastAxisRestriction defines an axis of turns that
                    // aren't permitted. Make sure we restrict the entire
                    // axis, and not just one of the faces. See the axis
                    // filtering in phase2() for more details.
                    lastAxisRestriction += 9;
                }
            }
            return solve(cc);
        }

        public string solution(string facelets, int maxDepth, long timeOut, long timeMin, int verbose)
        {
            return solution(facelets, maxDepth, timeOut, timeMin, verbose, null, null);
        }

        internal int verify(string facelets)
        {
            int count = 0x000000;
            try
            {
                string center = new string(new char[] {
                facelets[4],
                facelets[13],
                facelets[22],
                facelets[31],
                facelets[40],
                facelets[49]
            });
                for (int i = 0; i < 54; i++)
                {
                    f[i] = (sbyte)center.IndexOf(facelets[i]);
                    if (f[i] == -1)
                    {
                        return -1;
                    }
                    count += 1 << (f[i] << 2);
                }
            }
            catch
            {
                return -1;
            }
            if (count != 0x999999)
            {
                return -1;
            }
            Util.toCubieCube(f, cc);
            return cc.verify();
        }

        private string solve(CubieCube c)
        {
            Tools.init();
            int conjMask = 0;
            for (int i = 0; i < 6; i++)
            {
                twist[i] = c.getTwistSym();
                flip[i] = c.getFlipSym();
                slice[i] = c.getUDSlice();
                corn0[i] = c.getCPermSym();
                ud8e0[i] = c.getU4Comb() << 16 | c.getD4Comb();

                for (int j = 0; j < i; j++)
                {   //If S_i^-1 * C * S_i == C, It's unnecessary to compute it again.
                    if (twist[i] == twist[j] && flip[i] == flip[j] && slice[i] == slice[j]
                            && corn0[i] == corn0[j] && ud8e0[i] == ud8e0[j])
                    {
                        conjMask |= 1 << i;
                        break;
                    }
                }
                if ((conjMask & (1 << i)) == 0)
                {
                    prun[i] = Math.Max(Math.Max(
                        CoordCube.getPruning(CoordCube.UDSliceTwistPrun,
                            ((int)((uint)twist[i] >> 3)) * 495 + CoordCube.UDSliceConj[slice[i] & 0x1ff, twist[i] & 7]),
                        CoordCube.getPruning(CoordCube.UDSliceFlipPrun,
                            ((int)((uint)flip[i] >> 3)) * 495 + CoordCube.UDSliceConj[slice[i] & 0x1ff, flip[i] & 7])),
                        Tools.USE_TWIST_FLIP_PRUN ? CoordCube.getPruning(CoordCube.TwistFlipPrun,
                                ((int)((uint)twist[i] >> 3)) * 2688 + (flip[i] & 0xfff8 | CubieCube.Sym8MultInv[flip[i] & 7, twist[i] & 7])) : 0);
                }
                c.URFConjugate();
                if (i == 2)
                {
                    c.invCubieCube();
                }
            }
            for (depth1 = 0; depth1 < sol; depth1++)
            {
                maxDep2 = Math.Min(12, sol - depth1);
                for (urfIdx = 0; urfIdx < 6; urfIdx++)
                {
                    if ((firstAxisRestriction != -1 || lastAxisRestriction != -1) && urfIdx >= 3)
                    {
                        // When urfIdx >= 3, we're solving the
                        // inverse cube. This doesn't work
                        // when we're also restricting the 
                        // first turn, so we just skip inverse
                        // solutions when firstAxisRestriction has
                        // been set.
                        continue;
                    }
                    if ((conjMask & (1 << urfIdx)) != 0)
                    {
                        continue;
                    }
                    corn[0] = corn0[urfIdx];
                    mid4[0] = slice[urfIdx];
                    ud8e[0] = ud8e0[urfIdx];
                    valid1 = 0;
                    int lm = firstAxisRestriction == -1 ? -1 : CubieCube.urfMoveInv[urfIdx][firstAxisRestriction] / 3 * 3;
                    if ((prun[urfIdx] <= depth1)
                            && phase1((int)((uint)twist[urfIdx] >> 3), twist[urfIdx] & 7, (int)((uint)flip[urfIdx] >> 3), flip[urfIdx] & 7,
                                slice[urfIdx] & 0x1ff, depth1, lm) == 0)
                    {
                        return _solution == null ? "Error 8" : _solution;
                    }
                }
            }
            return _solution == null ? "Error 7" : _solution;
        }

        /**
         * @return
         * 		0: Found or Timeout
         * 		1: Try Next Power
         * 		2: Try Next Axis
         */
        private int phase1(int twist, int tsym, int flip, int fsym, int slice, int maxl, int lastAxis)
        {
            if (twist == 0 && flip == 0 && slice == 0 && maxl < 5)
            {
                return maxl == 0 ? initPhase2() : 1;
            }
            for (int axis = 0; axis < 18; axis += 3)
            {
                if (axis == lastAxis || axis == lastAxis - 9)
                {
                    continue;
                }
                for (int power = 0; power < 3; power++)
                {
                    int m = axis + power;

                    int slicex = CoordCube.UDSliceMove[slice, m] & 0x1ff;
                    int twistx = CoordCube.TwistMove[twist, CubieCube.Sym8Move[tsym, m]];
                    int tsymx = CubieCube.Sym8Mult[twistx & 7, tsym];
                    twistx = (int)((uint)twistx >> 3);
                    int prun = CoordCube.getPruning(CoordCube.UDSliceTwistPrun,
                        twistx * 495 + CoordCube.UDSliceConj[slicex, tsymx]);
                    if (prun > maxl)
                    {
                        break;
                    }
                    else if (prun == maxl)
                    {
                        continue;
                    }
                    int flipx = CoordCube.FlipMove[flip, CubieCube.Sym8Move[fsym, m]];
                    int fsymx = CubieCube.Sym8Mult[flipx & 7, fsym];
                    flipx = (int)((uint)flipx >> 3);
                    if (Tools.USE_TWIST_FLIP_PRUN)
                    {
                        prun = CoordCube.getPruning(CoordCube.TwistFlipPrun,
                            (twistx * 336 + flipx) << 3 | CubieCube.Sym8MultInv[fsymx, tsymx]);
                        if (prun > maxl)
                        {
                            break;
                        }
                        else if (prun == maxl)
                        {
                            continue;
                        }
                    }
                    prun = CoordCube.getPruning(CoordCube.UDSliceFlipPrun,
                        flipx * 495 + CoordCube.UDSliceConj[slicex, fsymx]);
                    if (prun > maxl)
                    {
                        break;
                    }
                    else if (prun == maxl)
                    {
                        continue;
                    }
                    move[depth1 - maxl] = m;
                    valid1 = Math.Min(valid1, depth1 - maxl);
                    int ret = phase1(twistx, tsymx, flipx, fsymx, slicex, maxl - 1, axis);
                    if (ret != 1)
                    {
                        return ret >> 1;
                    }
                }
            }
            return 1;
        }

        /**
         * @return
         * 		0: Found or Timeout
         * 		1: Try Next Power
         * 		2: Try Next Axis
         */
        private int initPhase2()
        {
            if (currentTimeMillis() >= (_solution == null ? timeOut : timeMin))
            {
                return 0;
            }
            valid2 = Math.Min(valid2, valid1);
            int cidx = (int)((uint)corn[valid1] >> 4);
            int csym = corn[valid1] & 0xf;
            for (int i = valid1; i < depth1; i++)
            {
                int m = move[i];
                cidx = CoordCube.CPermMove[cidx, CubieCube.SymMove[csym, m]];
                csym = CubieCube.SymMult[cidx & 0xf, csym];
                cidx = (int)((uint)cidx >> 4);
                corn[i + 1] = cidx << 4 | csym;

                int cx = CoordCube.UDSliceMove[mid4[i] & 0x1ff, m];
                mid4[i + 1] = Util.permMult[(uint)mid4[i] >> 9, (uint)cx >> 9] << 9 | cx & 0x1ff;
            }
            valid1 = depth1;
            int mid = (int)((uint)mid4[depth1] >> 9);
            int prun = CoordCube.getPruning(CoordCube.MCPermPrun, cidx * 24 + CoordCube.MPermConj[mid, csym]);
            if (prun >= maxDep2)
            {
                return prun > maxDep2 ? 2 : 1;
            }

            int u4e = (int)((uint)ud8e[valid2] >> 16);
            int d4e = ud8e[valid2] & 0xffff;
            for (int i = valid2; i < depth1; i++)
            {
                int m = move[i];

                int cx = CoordCube.UDSliceMove[u4e & 0x1ff, m];
                u4e = Util.permMult[(uint)u4e >> 9, (uint)cx >> 9] << 9 | cx & 0x1ff;

                cx = CoordCube.UDSliceMove[d4e & 0x1ff, m];
                d4e = Util.permMult[(uint)d4e >> 9, (uint)cx >> 9] << 9 | cx & 0x1ff;

                ud8e[i + 1] = u4e << 16 | d4e;
            }
            valid2 = depth1;

            int edge = CubieCube.MtoEPerm[494 - (u4e & 0x1ff) + ((int)((uint)u4e >> 9)) * 70 + ((int)((uint)d4e >> 9)) * 1680];
            int esym = edge & 15;
            edge = (int)((uint)edge >> 4);

            prun = Math.Max(CoordCube.getPruning(CoordCube.MEPermPrun, edge * 24 + CoordCube.MPermConj[mid, esym]), prun);
            if (prun >= maxDep2)
            {
                return prun > maxDep2 ? 2 : 1;
            }

            int firstAxisRestrictionUd = firstAxisRestriction == -1 ? 10 : Util.std2ud[CubieCube.urfMoveInv[urfIdx][firstAxisRestriction] / 3 * 3 + 1];
            int lm = depth1 == 0 ? firstAxisRestrictionUd : Util.std2ud[move[depth1 - 1] / 3 * 3 + 1];
            for (int depth2 = prun; depth2 < maxDep2; depth2++)
            {
                if (phase2(edge, esym, cidx, csym, mid, depth2, depth1, lm))
                {
                    sol = depth1 + depth2;
                    maxDep2 = Math.Min(12, sol - depth1);
                    _solution = solutionToString();
                    return currentTimeMillis() >= timeMin ? 0 : 1;
                }
            }
            return 1;
        }

        private bool phase2(int eidx, int esym, int cidx, int csym, int mid, int maxl, int depth, int lm)
        {
            if (maxl == 0)
            {
                // We've done the last move we're allowed to do, make sure it's permitted
                // by lastAxisRestriction.
                if (lastAxisRestriction != -1)
                {
                    int stdLm = CubieCube.urfMove[urfIdx][Util.ud2std[lm]];
                    int lastAxis = (stdLm / 3) * 3;
                    if (lastAxisRestriction == lastAxis || lastAxisRestriction == lastAxis + 9)
                    {
                        return false;
                    }
                }
                return eidx == 0 && cidx == 0 && mid == 0;
            }
            for (int m = 0; m < 10; m++)
            {
                if (Util.ckmv2[lm, m])
                {
                    continue;
                }
                int midx = CoordCube.MPermMove[mid, m];
                int cidxx = CoordCube.CPermMove[cidx, CubieCube.SymMove[csym, Util.ud2std[m]]];
                int csymx = CubieCube.SymMult[cidxx & 15, csym];
                cidxx = (int)((uint)cidxx >> 4);
                if (CoordCube.getPruning(CoordCube.MCPermPrun,
                        cidxx * 24 + CoordCube.MPermConj[midx, csymx]) >= maxl)
                {
                    continue;
                }
                int eidxx = CoordCube.EPermMove[eidx, CubieCube.SymMoveUD[esym, m]];
                int esymx = CubieCube.SymMult[eidxx & 15, esym];
                eidxx = (int)((uint)eidxx >> 4);
                if (CoordCube.getPruning(CoordCube.MEPermPrun,
                        eidxx * 24 + CoordCube.MPermConj[midx, esymx]) >= maxl)
                {
                    continue;
                }
                if (phase2(eidxx, esymx, cidxx, csymx, midx, maxl - 1, depth + 1, m))
                {
                    move[depth] = Util.ud2std[m];
                    return true;
                }
            }
            return false;
        }

        private string solutionToString()
        {
            StringBuilder sb = new StringBuilder();
            int urf = (verbose & INVERSE_SOLUTION) != 0 ? (urfIdx + 3) % 6 : urfIdx;
            if (urf < 3)
            {
                for (int s = 0; s < depth1; s++)
                {
                    sb.Append(Util.move2str[CubieCube.urfMove[urf][move[s]]]).Append(' ');
                }
                if ((verbose & USE_SEPARATOR) != 0)
                {
                    sb.Append(".  ");
                }
                for (int s = depth1; s < sol; s++)
                {
                    sb.Append(Util.move2str[CubieCube.urfMove[urf][move[s]]]).Append(' ');
                }
            }
            else
            {
                for (int s = sol - 1; s >= depth1; s--)
                {
                    sb.Append(Util.move2str[CubieCube.urfMove[urf][move[s]]]).Append(' ');
                }
                if ((verbose & USE_SEPARATOR) != 0)
                {
                    sb.Append(".  ");
                }
                for (int s = depth1 - 1; s >= 0; s--)
                {
                    sb.Append(Util.move2str[CubieCube.urfMove[urf][move[s]]]).Append(' ');
                }
            }
            if ((verbose & APPEND_LENGTH) != 0)
            {
                sb.Append("(").Append(sol).Append("f)");
            }
            return sb.ToString();
        }

        private static DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static long currentTimeMillis()
        {
            return (long)((DateTime.UtcNow - Jan1st1970).TotalMilliseconds);
        }
    }
}
