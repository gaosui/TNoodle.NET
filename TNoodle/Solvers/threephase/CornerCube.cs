using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TNoodle.Solvers.Threephase.Moves;

namespace TNoodle.Solvers.Threephase
{
    internal class CornerCube
    {
        /**
         * 18 move cubes
         */
        private static readonly CornerCube[] moveCube = new CornerCube[18];

        private static readonly int[] cpmv = {1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1,
                                        1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1};

        private readonly sbyte[] cp = { 0, 1, 2, 3, 4, 5, 6, 7 };
        private readonly sbyte[] co = { 0, 0, 0, 0, 0, 0, 0, 0 };

        private CornerCube temps;//new CornerCube();

        public CornerCube()
        {
        }

        public CornerCube(Random r) : this(r.Next(40320), r.Next(2187))
        {
        }

        public CornerCube(int cperm, int twist)
        {
            SetCPerm(cperm);
            SetTwist(twist);
        }

        public CornerCube(CornerCube c)
        {
            Copy(c);
        }

        public void Copy(CornerCube c)
        {
            for (int i = 0; i < 8; i++)
            {
                cp[i] = c.cp[i];
                co[i] = c.co[i];
            }
        }

        public int GetParity()
        {
            return Util.Parity(cp);
        }

        private static readonly sbyte[][] cornerFacelet = 
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

        public void Fill333Facelet(char[] facelet)
        {
            for (int corn = 0; corn < 8; corn++)
            {
                int j = cp[corn];
                int ori = co[corn];
                for (int n = 0; n < 3; n++)
                {
                    facelet[cornerFacelet[corn][(n + ori) % 3]] = "URFDLB"[cornerFacelet[j][n] / 9];
                }
            }
        }

        /**
         * prod = a * b, Corner Only.
         */
        private static void CornMult(CornerCube a, CornerCube b, CornerCube prod)
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

        private void SetTwist(int idx)
        {
            int twst = 0;
            for (int i = 6; i >= 0; i--)
            {
                twst += co[i] = (sbyte)(idx % 3);
                idx /= 3;
            }
            co[7] = (sbyte)((15 - twst) % 3);
        }

        private void SetCPerm(int idx)
        {
            Util.Set8Perm(cp, idx);
        }

        public void Move(int idx)
        {
            if (temps == null)
            {
                temps = new CornerCube();
            }
            CornMult(this, moveCube[idx], temps);
            Copy(temps);
        }

        static CornerCube()
        {
            InitMove();
        }

        private static void InitMove()
        {
            moveCube[0] = new CornerCube(15120, 0);
            moveCube[3] = new CornerCube(21021, 1494);
            moveCube[6] = new CornerCube(8064, 1236);
            moveCube[9] = new CornerCube(9, 0);
            moveCube[12] = new CornerCube(1230, 412);
            moveCube[15] = new CornerCube(224, 137);
            for (int a = 0; a < 18; a += 3)
            {
                for (int p = 0; p < 2; p++)
                {
                    moveCube[a + p + 1] = new CornerCube();
                    CornMult(moveCube[a + p], moveCube[a], moveCube[a + p + 1]);
                }
            }
        }
    }
}
