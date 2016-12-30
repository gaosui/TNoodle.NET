using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNoodle.Solvers.Threephase
{
    internal class CenterCube
    {
        public sbyte[] Ct { get; } = new sbyte[24];

        private static readonly int[] center333Map = { 0, 4, 2, 1, 5, 3 };

        public CenterCube()
        {
            for (int i = 0; i < 24; i++)
            {
                Ct[i] = (sbyte)(i / 4);
            }
        }

        public CenterCube(CenterCube c)
        {
            Copy(c);
        }

        public CenterCube(Random r) : this()
        {
            for (int i = 0; i < 23; i++)
            {
                int t = i + r.Next(24 - i);
                if (Ct[t] != Ct[i])
                {
                    sbyte m = Ct[i];
                    Ct[i] = Ct[t];
                    Ct[t] = m;
                }
            }
        }

        public CenterCube(int[] moveseq) : this()
        {
            for (int m = 0; m < moveseq.Length; m++)
            {
                Move(m);
            }
        }

        public void Copy(CenterCube c)
        {
            for (int i = 0; i < 24; i++)
            {
                Ct[i] = c.Ct[i];
            }
        }

        public void Fill333Facelet(char[] facelet)
        {
            int firstIdx = 4, inc = 9;
            for (int i = 0; i < 6; i++)
            {
                int idx = center333Map[i] << 2;
                if (Ct[idx] != Ct[idx + 1] || Ct[idx + 1] != Ct[idx + 2] || Ct[idx + 2] != Ct[idx + 3])
                {
                    throw new Exception("Unsolved Center");
                }
                facelet[firstIdx + i * inc] = Util.ColorMap4to3[Ct[idx]];
            }
        }

        public void Move(int m)
        {
            int key = m % 3;
            m /= 3;
            switch (m)
            {
                case 0: //U
                    Util.Swap(Ct, 0, 1, 2, 3, key);
                    break;
                case 1: //R
                    Util.Swap(Ct, 16, 17, 18, 19, key);
                    break;
                case 2: //F
                    Util.Swap(Ct, 8, 9, 10, 11, key);
                    break;
                case 3: //D
                    Util.Swap(Ct, 4, 5, 6, 7, key);
                    break;
                case 4: //L
                    Util.Swap(Ct, 20, 21, 22, 23, key);
                    break;
                case 5: //B
                    Util.Swap(Ct, 12, 13, 14, 15, key);
                    break;
                case 6: //u
                    Util.Swap(Ct, 0, 1, 2, 3, key);
                    Util.Swap(Ct, 8, 20, 12, 16, key);
                    Util.Swap(Ct, 9, 21, 13, 17, key);
                    break;
                case 7: //r
                    Util.Swap(Ct, 16, 17, 18, 19, key);
                    Util.Swap(Ct, 1, 15, 5, 9, key);
                    Util.Swap(Ct, 2, 12, 6, 10, key);
                    break;
                case 8: //f
                    Util.Swap(Ct, 8, 9, 10, 11, key);
                    Util.Swap(Ct, 2, 19, 4, 21, key);
                    Util.Swap(Ct, 3, 16, 5, 22, key);
                    break;
                case 9: //d
                    Util.Swap(Ct, 4, 5, 6, 7, key);
                    Util.Swap(Ct, 10, 18, 14, 22, key);
                    Util.Swap(Ct, 11, 19, 15, 23, key);
                    break;
                case 10://l
                    Util.Swap(Ct, 20, 21, 22, 23, key);
                    Util.Swap(Ct, 0, 8, 4, 14, key);
                    Util.Swap(Ct, 3, 11, 7, 13, key);
                    break;
                case 11://b
                    Util.Swap(Ct, 12, 13, 14, 15, key);
                    Util.Swap(Ct, 1, 20, 7, 18, key);
                    Util.Swap(Ct, 0, 23, 6, 17, key);
                    break;
            }
        }
    }
}
