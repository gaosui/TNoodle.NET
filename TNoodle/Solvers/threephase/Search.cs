using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TNoodle.Solvers.Threephase.Moves;

namespace TNoodle.Solvers.Threephase
{
    public class Search
    {
        private const int PHASE1_SOLUTIONS = 10000;
        private const int PHASE2_ATTEMPTS = 500;
        private const int PHASE2_SOLUTIONS = 100;
        private const int PHASE3_ATTEMPTS = 100;

        private static bool inited = false;

        private SortedSet<FullCube> p1sols = new SortedSet<FullCube>(new FullCube.ValueComparator());

        private static readonly int[] count = new int[1];

        private readonly int[] move1 = new int[15];
        private readonly int[] move2 = new int[20];
        private readonly int[] move3 = new int[20];
        private int length1 = 0;
        private int length2 = 0;
        private bool add1 = false;
        private FullCube cubeToSolve;
        private readonly FullCube c1 = new FullCube();
        private readonly FullCube c2 = new FullCube();
        private readonly Center2 ct2 = new Center2();
        private readonly Center3 ct3 = new Center3();
        private readonly Edge3 e12 = new Edge3();
        private readonly Edge3[] tempe = new Edge3[20];

        private readonly Min2phase.Search search333 = new Min2phase.Search();

        //private int valid1 = 0;
        private string solution = "";

        private int p1SolsCnt = 0;
        private readonly FullCube[] arr2 = new FullCube[PHASE2_SOLUTIONS];
        private int arr2idx = 0;

        public bool InverseSolution { get; set; } = true;
        public bool WithRotation { get; set; } = false;

        public Search()
        {
            for (int i = 0; i < 20; i++)
            {
                tempe[i] = new Edge3();
            }
        }

        private static void Init()
        {
            if (inited)
            {
                return;
            }
            Min2phase.Tools.Init();

            Center1.InitSym();
            Center1.Raw2sym = new int[735471];
            Center1.InitSym2Raw();
            Center1.CreateMoveTable();
            Center1.Raw2sym = null;
            Center1.CreatePrun();

            Center2.Init();

            Center3.Init();

            Edge3.InitMvrot();
            Edge3.InitRaw2Sym();
            Edge3.CreatePrun();

            inited = true;
        }

        public string RandomMove(Random r)
        {
            int[] moveseq = new int[40];
            int lm = 36;
            for (int i = 0; i < moveseq.Length;)
            {
                int m = r.Next(27);
                if (!Ckmv[lm][m])
                {
                    moveseq[i++] = m;
                    lm = m;
                }
            }
            return Solve(moveseq);
        }

        public string RandomState(Random r)
        {
            cubeToSolve = new FullCube(r);
            DoSearch();
            return solution;
        }

        public string Solve(string scramble)
        {
            int[] moveseq = Util.Tomove(scramble);
            return Solve(moveseq);
        }

        public string Solve(int[] moveseq)
        {
            cubeToSolve = new FullCube(moveseq);
            DoSearch();
            return solution;
        }

        public string Solve(FullCube c)
        {
            cubeToSolve = c;
            DoSearch();
            return solution;
        }

        private int totlen = 0;

        private void DoSearch()
        {
            Init();
            solution = "";
            int ud = new Center1(cubeToSolve.GetCenter(), 0).Getsym();
            int fb = new Center1(cubeToSolve.GetCenter(), 1).Getsym();
            int rl = new Center1(cubeToSolve.GetCenter(), 2).Getsym();
            int udprun = Center1.Csprun[ud >> 6];
            int fbprun = Center1.Csprun[fb >> 6];
            int rlprun = Center1.Csprun[rl >> 6];

            p1SolsCnt = 0;
            arr2idx = 0;
            p1sols.Clear();

            for (length1 = Math.Min(Math.Min(udprun, fbprun), rlprun); length1 < 100; length1++)
            {
                if (rlprun <= length1 && Search1((int)((uint)rl >> 6), rl & 0x3f, length1, -1, 0)
                        || udprun <= length1 && Search1((int)((uint)ud >> 6), ud & 0x3f, length1, -1, 0)
                        || fbprun <= length1 && Search1((int)((uint)fb >> 6), fb & 0x3f, length1, -1, 0))
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
                for (length12 = p1SolsArr[0].Value; length12 < 100; length12++)
                {
                    for (int i = 0; i < p1SolsArr.Length; i++)
                    {
                        if (p1SolsArr[i].Value > length12)
                        {
                            break;
                        }
                        if (length12 - p1SolsArr[i].Length1 > MAX_LENGTH2)
                        {
                            continue;
                        }
                        c1.Copy(p1SolsArr[i]);
                        ct2.Set(c1.GetCenter(), c1.GetEdge().GetParity());
                        int s2ct = ct2.Getct();
                        int s2rl = ct2.Getrl();
                        length1 = p1SolsArr[i].Length1;
                        length2 = length12 - p1SolsArr[i].Length1;

                        if (Search2(s2ct, s2rl, length2, 28, 0))
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
                for (length123 = arr2[0].Value; length123 < 100; length123++)
                {
                    for (int i = 0; i < Math.Min(arr2idx, PHASE3_ATTEMPTS); i++)
                    {
                        if (arr2[i].Value > length123)
                        {
                            break;
                        }
                        if (length123 - arr2[i].Length1 - arr2[i].Length2 > MAX_LENGTH3)
                        {
                            continue;
                        }
                        int eparity = e12.Set(arr2[i].GetEdge());
                        ct3.Set(arr2[i].GetCenter(), eparity ^ arr2[i].GetCorner().GetParity());
                        int ct = ct3.Getct();
                        int edge = e12.Get(10);
                        int prun = Edge3.Getprun(e12.Getsym());
                        int lm = 20;

                        if (prun <= length123 - arr2[i].Length1 - arr2[i].Length2
                                && Search3(edge, ct, prun, length123 - arr2[i].Length1 - arr2[i].Length2, lm, 0))
                        {
                            solcnt++;
                            index = i;
                            goto OUT2;
                        }
                    }
                }
            OUT2:
                MAX_LENGTH3++;
            } while (length123 == 100);

            FullCube solcube = new FullCube(arr2[index]);
            length1 = solcube.Length1;
            length2 = solcube.Length2;
            int length = length123 - length1 - length2;

            for (int i = 0; i < length; i++)
            {
                solcube.Move(Move3std[move3[i]]);
            }

            string facelet = solcube.To333Facelet();
            string sol = search333.Solution(facelet, 20, 100, 50, 0);
            if (sol.StartsWith("Error 8"))
            {
                sol = search333.Solution(facelet, 21, 1000000, 30, 0);
            }
            int len333 = sol.Length / 3;
            if (sol.StartsWith("Error"))
            {
                throw new Exception();
            }
            int[] sol333 = Util.Tomove(sol);
            for (int i = 0; i < sol333.Length; i++)
            {
                solcube.Move(sol333[i]);
            }

            StringBuilder str = new StringBuilder();
            str.Append(solcube.GetMoveString(InverseSolution, WithRotation));

            solution = str.ToString();

            totlen = length1 + length2 + length + len333;
        }

        public void Calc(FullCube s)
        {
            cubeToSolve = s;
            DoSearch();
        }

        private bool Search1(int ct, int sym, int maxl, int lm, int depth)
        {
            if (ct == 0)
            {
                return maxl == 0 && Init2(sym, lm);
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
                    int ctx = Center1.Ctsmv[ct][Center1.Symmove[sym][m]];
                    int prun = Center1.Csprun[(uint)ctx >> 6];
                    if (prun >= maxl)
                    {
                        if (prun > maxl)
                        {
                            break;
                        }
                        continue;
                    }
                    int symx = Center1.Symmult[sym][ctx & 0x3f];
                    //ctx >>>= 6;
                    ctx = (int)((uint)ctx >> 6);
                    move1[depth] = m;
                    if (Search1(ctx, symx, maxl - 1, axis, depth + 1))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool Init2(int sym, int lm)
        {
            c1.Copy(cubeToSolve);
            for (int i = 0; i < length1; i++)
            {
                c1.Move(move1[i]);
            }

            switch (Center1.Finish[sym])
            {
                case 0:
                    c1.Move(fx1);
                    c1.Move(bx3);
                    move1[length1] = fx1;
                    move1[length1 + 1] = bx3;
                    add1 = true;
                    sym = 19;
                    break;
                case 12869:
                    c1.Move(ux1);
                    c1.Move(dx3);
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
            ct2.Set(c1.GetCenter(), c1.GetEdge().GetParity());
            int s2ct = ct2.Getct();
            int s2rl = ct2.Getrl();
            int ctp = Center2.Ctprun[s2ct * 70 + s2rl];

            c1.Value = ctp + length1;
            c1.Length1 = length1;
            c1.Add1 = add1;
            c1.Sym = sym;
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
                if (next.Value > c1.Value)
                {
                    next.Copy(c1);
                }
            }
            p1sols.Add(next);

            return p1SolsCnt == PHASE1_SOLUTIONS;
        }

        private bool Search2(int ct, int rl, int maxl, int lm, int depth)
        {
            if (ct == 0 && Center2.Ctprun[rl] == 0)
            {
                return maxl == 0 && Init3();
            }
            for (int m = 0; m < 23; m++)
            {
                if (Ckmv2[lm][m])
                {
                    m = SkipAxis2[m];
                    continue;
                }
                int ctx = Center2.Ctmv[ct][m];
                int rlx = Center2.Rlmv[rl][m];

                int prun = Center2.Ctprun[ctx * 70 + rlx];
                if (prun >= maxl)
                {
                    if (prun > maxl)
                    {
                        m = SkipAxis2[m];
                    }
                    continue;
                }

                move2[depth] = Move2std[m];
                if (Search2(ctx, rlx, maxl - 1, m, depth + 1))
                {
                    return true;
                }
            }
            return false;
        }

        private bool Init3()
        {
            c2.Copy(c1);
            for (int i = 0; i < length2; i++)
            {
                c2.Move(move2[i]);
            }
            if (!c2.CheckEdge())
            {
                return false;
            }
            int eparity = e12.Set(c2.GetEdge());
            ct3.Set(c2.GetCenter(), eparity ^ c2.GetCorner().GetParity());
            int ct = ct3.Getct();
            int edge = e12.Get(10);
            int prun = Edge3.Getprun(e12.Getsym());

            if (arr2[arr2idx] == null)
            {
                arr2[arr2idx] = new FullCube(c2);
            }
            else
            {
                arr2[arr2idx].Copy(c2);
            }
            arr2[arr2idx].Value = length1 + length2 + Math.Max(prun, Center3.Prun[ct]);
            arr2[arr2idx].Length2 = length2;
            arr2idx++;

            return arr2idx == arr2.Length;
        }

        private bool Search3(int edge, int ct, int prun, int maxl, int lm, int depth)
        {
            if (maxl == 0)
            {
                return edge == 0 && ct == 0;
            }
            tempe[depth].Set(edge);
            for (int m = 0; m < 17; m++)
            {
                if (Ckmv3[lm][m])
                {
                    m = SkipAxis3[m];
                    continue;
                }
                int ctx = Center3.Ctmove[ct][m];
                int prun1 = Center3.Prun[ctx];
                if (prun1 >= maxl)
                {
                    if (prun1 > maxl && m < 14)
                    {
                        m = SkipAxis3[m];
                    }
                    continue;
                }
                int edgex = Edge3.Getmvrot(tempe[depth].Edge, m << 3, 10);

                int cord1x = edgex / Edge3.N_RAW;
                int symcord1x = Edge3.Raw2sym[cord1x];
                int symx = symcord1x & 0x7;
                symcord1x >>= 3;
                int cord2x = Edge3.Getmvrot(tempe[depth].Edge, m << 3 | symx, 10) % Edge3.N_RAW;

                int prunx = Edge3.Getprun(symcord1x * Edge3.N_RAW + cord2x, prun);
                if (prunx >= maxl)
                {
                    if (prunx > maxl && m < 14)
                    {
                        m = SkipAxis3[m];
                    }
                    continue;
                }

                if (Search3(edgex, ctx, prunx, maxl - 1, m, depth + 1))
                {
                    move3[depth] = m;
                    return true;
                }
            }
            return false;
        }
    }
}
