﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TNoodle.Solvers.threephase.Moves;
using static TNoodle.Solvers.threephase.Center1;
using static TNoodle.Solvers.threephase.Center2;
using static TNoodle.Solvers.threephase.Util;

namespace TNoodle.Solvers.threephase
{


    public class Search
    {
        internal const int PHASE1_SOLUTIONS = 10000;
        internal const int PHASE2_ATTEMPTS = 500;
        internal const int PHASE2_SOLUTIONS = 100;
        internal const int PHASE3_ATTEMPTS = 100;

        internal static bool inited = false;

        internal SortedSet<FullCube> p1sols = new SortedSet<FullCube>();

        internal static int[] count = new int[1];

        internal int[] move1 = new int[15];
        internal int[] move2 = new int[20];
        internal int[] move3 = new int[20];
        internal int length1 = 0;
        internal int length2 = 0;
        internal int maxlength2;
        internal bool add1 = false;
        public FullCube c;
        internal FullCube c1 = new FullCube();
        internal FullCube c2 = new FullCube();
        internal Center2 ct2 = new Center2();
        internal Center3 ct3 = new Center3();
        internal Edge3 e12 = new Edge3();
        internal Edge3[] tempe = new Edge3[20];

        Min2phase.Search search333 = new Min2phase.Search();

        internal int valid1 = 0;
        internal string solution = "";

        internal int p1SolsCnt = 0;
        internal FullCube[] arr2 = new FullCube[PHASE2_SOLUTIONS];
        internal int arr2idx = 0;

        public bool inverse_solution = true;
        public bool with_rotation = false;

        public Search()
        {
            for (int i = 0; i < 20; i++)
            {
                tempe[i] = new Edge3();
            }
        }

        public static void init()
        {
            if (inited)
            {
                return;
            }
            Min2phase.Tools.init();

            //System.out.println("Initialize Center1 Solver...");

            Center1.initSym();
            Center1.raw2sym = new int[735471];
            Center1.initSym2Raw();
            Center1.createMoveTable();
            Center1.raw2sym = null;
            Center1.createPrun();

            //System.out.println("Initialize Center2 Solver...");

            Center2.init();

            //System.out.println("Initialize Center3 Solver...");

            Center3.init();

            //System.out.println("Initialize Edge3 Solver...");

            Edge3.initMvrot();
            Edge3.initRaw2Sym();
            Edge3.createPrun();

            //System.out.println("OK");

            inited = true;
        }

        public String randomMove(Random r)
        {
            int[] moveseq = new int[40];
            int lm = 36;
            for (int i = 0; i < moveseq.Length;)
            {
                int m = r.Next(27);
                if (!ckmv[lm, m])
                {
                    moveseq[i++] = m;
                    lm = m;
                }
            }
            //System.out.println(tostr(moveseq));
            return solve(moveseq);
        }

        public String randomState(Random r)
        {
            c = new FullCube(r);
            doSearch();
            return solution;
        }

        public String solve(String scramble)
        {
            int[] moveseq = tomove(scramble);
            return solve(moveseq);
        }

        public String solve(int[] moveseq)
        {
            c = new FullCube(moveseq);
            doSearch();
            return solution;
        }

        internal int totlen = 0;

        internal void doSearch()
        {
            init();
            solution = "";
            int ud = new Center1(c.getCenter(), 0).getsym();
            int fb = new Center1(c.getCenter(), 1).getsym();
            int rl = new Center1(c.getCenter(), 2).getsym();
            int udprun = csprun[ud >> 6];
            int fbprun = csprun[fb >> 6];
            int rlprun = csprun[rl >> 6];

            p1SolsCnt = 0;
            arr2idx = 0;
            p1sols.Clear();

            for (length1 = Math.Min(Math.Min(udprun, fbprun), rlprun); length1 < 100; length1++)
            {
                if (rlprun <= length1 && search1((int)((uint)rl >> 6), rl & 0x3f, length1, -1, 0)
                        || udprun <= length1 && search1((int)((uint)ud >> 6), ud & 0x3f, length1, -1, 0)
                        || fbprun <= length1 && search1((int)((uint)fb >> 6), fb & 0x3f, length1, -1, 0))
                {
                    break;
                }
            }

            //FullCube[] p1SolsArr = p1sols.ToArray(new FullCube[0]);
            FullCube[] p1SolsArr = new FullCube[p1sols.Count];
            p1sols.CopyTo(p1SolsArr, 0);
            Array.Sort(p1SolsArr, 0, p1SolsArr.Length);

            int MAX_LENGTH2 = 9;
            int length12;
            do
            {
                for (length12 = p1SolsArr[0].value; length12 < 100; length12++)
                {
                    for (int i = 0; i < p1SolsArr.Length; i++)
                    {
                        if (p1SolsArr[i].value > length12)
                        {
                            break;
                        }
                        if (length12 - p1SolsArr[i].length1 > MAX_LENGTH2)
                        {
                            continue;
                        }
                        c1.copy(p1SolsArr[i]);
                        ct2.set(c1.getCenter(), c1.getEdge().getParity());
                        int s2ct = ct2.getct();
                        int s2rl = ct2.getrl();
                        length1 = p1SolsArr[i].length1;
                        length2 = length12 - p1SolsArr[i].length1;

                        if (search2(s2ct, s2rl, length2, 28, 0))
                        {
                            goto OUT;
                        }
                    }
                }
            OUT:
                MAX_LENGTH2++;
            } while (length12 == 100);

            Array.Sort(arr2, 0, arr2idx);
            int length123, index = 0;
            int solcnt = 0;

            int MAX_LENGTH3 = 13;
            do
            {
                for (length123 = arr2[0].value; length123 < 100; length123++)
                {
                    for (int i = 0; i < Math.Min(arr2idx, PHASE3_ATTEMPTS); i++)
                    {
                        if (arr2[i].value > length123)
                        {
                            break;
                        }
                        if (length123 - arr2[i].length1 - arr2[i].length2 > MAX_LENGTH3)
                        {
                            continue;
                        }
                        int eparity = e12.set(arr2[i].getEdge());
                        ct3.set(arr2[i].getCenter(), eparity ^ arr2[i].getCorner().getParity());
                        int ct = ct3.getct();
                        int edge = e12.get(10);
                        int prun = Edge3.getprun(e12.getsym());
                        int lm = 20;

                        if (prun <= length123 - arr2[i].length1 - arr2[i].length2
                                && search3(edge, ct, prun, length123 - arr2[i].length1 - arr2[i].length2, lm, 0))
                        {
                            solcnt++;
                            //					System.out.println(length123 + " " + solcnt);
                            //					if (solcnt == 5) {
                            index = i;
                            goto OUT2;
                            //					}
                        }
                    }
                }
            OUT2:
                MAX_LENGTH3++;
            } while (length123 == 100);

            FullCube solcube = new FullCube(arr2[index]);
            length1 = solcube.length1;
            length2 = solcube.length2;
            int length = length123 - length1 - length2;

            for (int i = 0; i < length; i++)
            {
                solcube.move(move3std[move3[i]]);
            }

            String facelet = solcube.to333Facelet();
            String sol = search333.Solution(facelet, 20, 100, 50, 0);
            if (sol.StartsWith("Error 8"))
            {
                sol = search333.Solution(facelet, 21, 1000000, 30, 0);
            }
            int len333 = sol.Length / 3;
            if (sol.StartsWith("Error"))
            {
                //System.out.println(sol);
                throw new Exception();
            }
            int[] sol333 = tomove(sol);
            for (int i = 0; i < sol333.Length; i++)
            {
                solcube.move(sol333[i]);
            }

            StringBuilder str = new StringBuilder();
            str.Append(solcube.getMoveString(inverse_solution, with_rotation));

            solution = str.ToString();

            totlen = length1 + length2 + length + len333;
        }

        public void calc(FullCube s)
        {
            c = s;
            doSearch();
        }

        internal bool search1(int ct, int sym, int maxl, int lm, int depth)
        {
            if (ct == 0)
            {
                return maxl == 0 && init2(sym, lm);
            }
            for (int axis = 0; axis < 27; axis += 3)
            {
                if (axis == lm || axis == lm - 9 || axis == lm - 18)
                {
                    continue;
                }
                for (int power = 0; power < 3; power++)
                {
                    int m = axis + power;
                    int ctx = ctsmv[ct, symmove[sym, m]];
                    int prun = csprun[(uint)ctx >> 6];
                    if (prun >= maxl)
                    {
                        if (prun > maxl)
                        {
                            break;
                        }
                        continue;
                    }
                    int symx = symmult[sym, ctx & 0x3f];
                    //ctx >>>= 6;
                    ctx = (int)((uint)ctx >> 6);
                    move1[depth] = m;
                    if (search1(ctx, symx, maxl - 1, axis, depth + 1))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool init2(int sym, int lm)
        {
            c1.copy(c);
            for (int i = 0; i < length1; i++)
            {
                c1.move(move1[i]);
            }

            switch (Center1.finish[sym])
            {
                case 0:
                    c1.move(fx1);
                    c1.move(bx3);
                    move1[length1] = fx1;
                    move1[length1 + 1] = bx3;
                    add1 = true;
                    sym = 19;
                    break;
                case 12869:
                    c1.move(ux1);
                    c1.move(dx3);
                    move1[length1] = ux1;
                    move1[length1 + 1] = dx3;
                    add1 = true;
                    sym = 34;
                    break;
                case 735470:
                    add1 = false;
                    sym = 0;
                    break;
            }
            ct2.set(c1.getCenter(), c1.getEdge().getParity());
            int s2ct = ct2.getct();
            int s2rl = ct2.getrl();
            int ctp = ctprun[s2ct * 70 + s2rl];

            c1.value = ctp + length1;
            c1.length1 = length1;
            c1.add1 = add1;
            c1.sym = sym;
            p1SolsCnt++;

            FullCube next;
            if (p1sols.Count < PHASE2_ATTEMPTS)
            {
                next = new FullCube(c1);
            }
            else
            {
                next = p1sols.FirstOrDefault();
                p1sols.Remove(next);
                if (next.value > c1.value)
                {
                    next.copy(c1);
                }
            }
            p1sols.Add(next);

            return p1SolsCnt == PHASE1_SOLUTIONS;
        }

        internal bool search2(int ct, int rl, int maxl, int lm, int depth)
        {
            if (ct == 0 && ctprun[rl] == 0)
            {
                return maxl == 0 && init3();
            }
            for (int m = 0; m < 23; m++)
            {
                if (ckmv2[lm, m])
                {
                    m = skipAxis2[m];
                    continue;
                }
                int ctx = ctmv[ct, m];
                int rlx = rlmv[rl, m];

                int prun = ctprun[ctx * 70 + rlx];
                if (prun >= maxl)
                {
                    if (prun > maxl)
                    {
                        m = skipAxis2[m];
                    }
                    continue;
                }

                move2[depth] = move2std[m];
                if (search2(ctx, rlx, maxl - 1, m, depth + 1))
                {
                    return true;
                }
            }
            return false;
        }

        internal bool init3()
        {
            c2.copy(c1);
            for (int i = 0; i < length2; i++)
            {
                c2.move(move2[i]);
            }
            if (!c2.checkEdge())
            {
                return false;
            }
            int eparity = e12.set(c2.getEdge());
            ct3.set(c2.getCenter(), eparity ^ c2.getCorner().getParity());
            int ct = ct3.getct();
            int edge = e12.get(10);
            int prun = Edge3.getprun(e12.getsym());

            if (arr2[arr2idx] == null)
            {
                arr2[arr2idx] = new FullCube(c2);
            }
            else
            {
                arr2[arr2idx].copy(c2);
            }
            arr2[arr2idx].value = length1 + length2 + Math.Max(prun, Center3.prun[ct]);
            arr2[arr2idx].length2 = length2;
            arr2idx++;

            return arr2idx == arr2.Length;
        }

        public bool search3(int edge, int ct, int prun, int maxl, int lm, int depth)
        {
            if (maxl == 0)
            {
                return edge == 0 && ct == 0;
            }
            tempe[depth].set(edge);
            for (int m = 0; m < 17; m++)
            {
                if (ckmv3[lm, m])
                {
                    m = skipAxis3[m];
                    continue;
                }
                int ctx = Center3.ctmove[ct, m];
                int prun1 = Center3.prun[ctx];
                if (prun1 >= maxl)
                {
                    if (prun1 > maxl && m < 14)
                    {
                        m = skipAxis3[m];
                    }
                    continue;
                }
                int edgex = Edge3.getmvrot(tempe[depth].edge, m << 3, 10);

                int cord1x = edgex / Edge3.N_RAW;
                int symcord1x = Edge3.raw2sym[cord1x];
                int symx = symcord1x & 0x7;
                symcord1x >>= 3;
                int cord2x = Edge3.getmvrot(tempe[depth].edge, m << 3 | symx, 10) % Edge3.N_RAW;

                int prunx = Edge3.getprun(symcord1x * Edge3.N_RAW + cord2x, prun);
                if (prunx >= maxl)
                {
                    if (prunx > maxl && m < 14)
                    {
                        m = skipAxis3[m];
                    }
                    continue;
                }

                if (search3(edgex, ctx, prunx, maxl - 1, m, depth + 1))
                {
                    move3[depth] = m;
                    return true;
                }
            }
            return false;
        }
    }

}
