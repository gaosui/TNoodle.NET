using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TNoodle.Solvers.Threephase.Moves;

namespace TNoodle.Solvers.Threephase
{
    /*
    Edge Cubies: 
                        14	2	
                    1			15
                    13			3
                        0	12	
        1	13			0	12			3	15			2	14	
    9			20	20			11	11			22	22			9
    21			8	8			23	23			10	10			21
        17	5			18	6			19	7			16	4	
                        18	6	
                    5			19
                    17			7
                        4	16	

    Center Cubies: 
                0	1
                3	2

    20	21		8	9		16	17		12	13
    23	22		11	10		19	18		15	14

                4	5
                7	6

         *             |************|
         *             |*U1**U2**U3*|
         *             |************|
         *             |*U4**U5**U6*|
         *             |************|
         *             |*U7**U8**U9*|
         *             |************|
         * ************|************|************|************|
         * *L1**L2**L3*|*F1**F2**F3*|*R1**R2**F3*|*B1**B2**B3*|
         * ************|************|************|************|
         * *L4**L5**L6*|*F4**F5**F6*|*R4**R5**R6*|*B4**B5**B6*|
         * ************|************|************|************|
         * *L7**L8**L9*|*F7**F8**F9*|*R7**R8**R9*|*B7**B8**B9*|
         * ************|************|************|************|
         *             |************|
         *             |*D1**D2**D3*|
         *             |************|
         *             |*D4**D5**D6*|
         *             |************|
         *             |*D7**D8**D9*|
         *             |************|
         */


    public class FullCube : IComparable<FullCube>
    {
        public class ValueComparator : IComparer<FullCube>
        {
            public int Compare(FullCube c1, FullCube c2)
            {
                return c2.Value - c1.Value;
            }
        }

        private EdgeCube edge;
        private CenterCube center;
        private CornerCube corner;

        internal int Value { get; set; } = 0;
        internal bool Add1 { get; set; } = false;
        internal int Length1 { get; set; } = 0;
        internal int Length2 { get; set; } = 0;
        private int length3 = 0;

        public int CompareTo(FullCube c)
        {
            return Value - c.Value;
        }

        public FullCube()
        {
            edge = new EdgeCube();
            center = new CenterCube();
            corner = new CornerCube();
        }

        public FullCube(FullCube c) : this()
        {
            Copy(c);
        }

        public FullCube(Random r)
        {
            edge = new EdgeCube(r);
            center = new CenterCube(r);
            corner = new CornerCube(r);
        }

        public FullCube(int[] moveseq) : this()
        {
            foreach (int m in moveseq)
            {
                DoMove(m);
            }
        }

        public void Copy(FullCube c)
        {
            edge.Copy(c.edge);
            center.Copy(c.center);
            corner.Copy(c.corner);

            Value = c.Value;
            Add1 = c.Add1;
            Length1 = c.Length1;
            Length2 = c.Length2;
            length3 = c.length3;

            Sym = c.Sym;

            for (int i = 0; i < 60; i++)
            {
                moveBuffer[i] = c.moveBuffer[i];
            }
            moveLength = c.moveLength;
            edgeAvail = c.edgeAvail;
            centerAvail = c.centerAvail;
            cornerAvail = c.cornerAvail;
        }

        public bool CheckEdge()
        {
            return GetEdge().CheckEdge();
        }

        public string GetMoveString(bool inverse, bool rotation)
        {
            int[] fixedMoves = new int[moveLength - (Add1 ? 2 : 0)];
            int idx = 0;
            for (int i = 0; i < Length1; i++)
            {
                fixedMoves[idx++] = moveBuffer[i];
            }
            int sym = this.Sym;
            for (int i = Length1 + (Add1 ? 2 : 0); i < moveLength; i++)
            {
                if (Center1.Symmove[sym][moveBuffer[i]] >= dx1)
                {
                    fixedMoves[idx++] = Center1.Symmove[sym][moveBuffer[i]] - 9;
                    int rot = move2rot[Center1.Symmove[sym][moveBuffer[i]] - dx1];
                    sym = Center1.Symmult[sym][rot];
                }
                else
                {
                    fixedMoves[idx++] = Center1.Symmove[sym][moveBuffer[i]];
                }
            }
            int finishSym = Center1.Symmult[Center1.Syminv[sym]][Center1.GetSolvedSym(GetCenter())];

            StringBuilder sb = new StringBuilder();
            sym = finishSym;
            if (inverse)
            {
                for (int i = idx - 1; i >= 0; i--)
                {
                    int move = fixedMoves[i];
                    move = move / 3 * 3 + (2 - move % 3);
                    if (Center1.Symmove[sym][move] >= dx1)
                    {
                        sb.Append(Move2str[Center1.Symmove[sym][move] - 9]).Append(' ');
                        int rot = move2rot[Center1.Symmove[sym][move] - dx1];
                        sym = Center1.Symmult[sym][rot];
                    }
                    else
                    {
                        sb.Append(Move2str[Center1.Symmove[sym][move]]).Append(' ');
                    }
                }
                if (rotation)
                {
                    sb.Append(Center1.Rot2str[Center1.Syminv[sym]] + " ");//cube rotation after solution. for wca scramble, it should be omitted.
                }
            }
            else
            {
                for (int i = 0; i < idx; i++)
                {
                    sb.Append(Move2str[fixedMoves[i]]).Append(' ');
                }
                if (rotation)
                {
                    sb.Append(Center1.Rot2str[finishSym]);//cube rotation after solution.
                }
            }
            return sb.ToString();
        }

        private static readonly int[] move2rot = { 35, 1, 34, 2, 4, 6, 22, 5, 19 };

        internal string To333Facelet()
        {
            char[] ret = new char[54];
            GetEdge().Fill333Facelet(ret);
            GetCenter().Fill333Facelet(ret);
            GetCorner().Fill333Facelet(ret);
            return new string(ret);
        }

        private sbyte[] moveBuffer = new sbyte[60];
        private int moveLength = 0;
        private int edgeAvail = 0;
        private int centerAvail = 0;
        private int cornerAvail = 0;

        internal int Sym { get; set; } = 0;

        internal void Move(int m)
        {
            moveBuffer[moveLength++] = (sbyte)m;
            return;
        }

        private void DoMove(int m)
        {
            GetEdge().Move(m);
            GetCenter().Move(m);
            GetCorner().Move(m % 18);
        }

        internal EdgeCube GetEdge()
        {
            while (edgeAvail < moveLength)
            {
                edge.Move(moveBuffer[edgeAvail++]);
            }
            return edge;
        }

        internal CenterCube GetCenter()
        {
            while (centerAvail < moveLength)
            {
                center.Move(moveBuffer[centerAvail++]);
            }
            return center;
        }

        internal CornerCube GetCorner()
        {
            while (cornerAvail < moveLength)
            {
                corner.Move(moveBuffer[cornerAvail++] % 18);
            }
            return corner;
        }
    }
}
