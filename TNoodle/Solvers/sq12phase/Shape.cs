using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Utils;

namespace TNoodle.Solvers.sq12phase
{
    internal class Shape
    {

        //1 = corner, 0 = edge.
        internal static int[] halflayer = {0x00, 0x03, 0x06, 0x0c, 0x0f, 0x18, 0x1b, 0x1e,
        0x30, 0x33, 0x36, 0x3c, 0x3f};

        internal static int[] ShapeIdx = new int[3678];
        internal static int[] ShapePrun = new int[3768 * 2];
        internal static int[] ShapePrunOpt = new int[3768 * 2];

        internal static int[] TopMove = new int[3678 * 2];
        internal static int[] BottomMove = new int[3678 * 2];
        internal static int[] TwistMove = new int[3678 * 2];

        private Shape() { }

        internal int top;
        internal int bottom;
        internal int parity;

        internal static int getShape2Idx(int shp)
        {
            int ret = Array.BinarySearch(ShapeIdx, shp & 0xffffff) << 1 | shp >> 24;
            return ret;
        }

        internal int getIdx()
        {
            int ret = Array.BinarySearch(ShapeIdx, top << 12 | bottom) << 1 | parity;
            return ret;
        }

        internal void setIdx(int idx)
        {
            parity = idx & 1;
            top = ShapeIdx[idx >> 1];
            bottom = top & 0xfff;
            top >>= 12;
        }

        internal int topMove()
        {
            int move = 0;
            int moveParity = 0;
            do
            {
                if ((top & 0x800) == 0)
                {
                    move += 1;
                    top = top << 1;
                }
                else
                {
                    move += 2;
                    top = (top << 2) ^ 0x3003;
                }
                moveParity = 1 - moveParity;
            } while ((Functions.BitCount(top & 0x3f) & 1) != 0);
            if ((Functions.BitCount(top) & 2) == 0)
            {
                parity ^= moveParity;

            }
            return move;
        }

        internal int bottomMove()
        {
            int move = 0;
            int moveParity = 0;
            do
            {
                if ((bottom & 0x800) == 0)
                {
                    move += 1;
                    bottom = bottom << 1;
                }
                else
                {
                    move += 2;
                    bottom = (bottom << 2) ^ 0x3003;
                }
                moveParity = 1 - moveParity;
            } while ((Functions.BitCount(bottom & 0x3f) & 1) != 0);
            if ((Functions.BitCount(bottom) & 2) == 0)
            {
                parity ^= moveParity;
            }
            return move;
        }

        internal void twistMove()
        {
            int temp = top & 0x3f;
            int p1 = Functions.BitCount(temp);
            int p3 = Functions.BitCount(bottom & 0xfc0);
            parity ^= 1 & ((p1 & p3) >> 1);

            top = (top & 0xfc0) | ((bottom >> 6) & 0x3f);
            bottom = (bottom & 0x3f) | temp << 6;
        }

        internal static bool inited = false;

        internal static void init()
        {
            if (inited)
            {
                return;
            }
            int count = 0;
            for (int i = 0; i < 13 * 13 * 13 * 13; i++)
            {
                int dr = halflayer[i % 13];
                int dl = halflayer[i / 13 % 13];
                int ur = halflayer[i / 13 / 13 % 13];
                int ul = halflayer[i / 13 / 13 / 13];
                int value = ul << 18 | ur << 12 | dl << 6 | dr;
                if (Functions.BitCount(value) == 16)
                {
                    ShapeIdx[count++] = value;
                }
            }
            //System.out.println(count);
            Shape s = new Shape();
            for (int i = 0; i < 3678 * 2; i++)
            {
                s.setIdx(i);
                TopMove[i] = s.topMove();
                TopMove[i] |= s.getIdx() << 4;
                s.setIdx(i);
                BottomMove[i] = s.bottomMove();
                BottomMove[i] |= s.getIdx() << 4;
                s.setIdx(i);
                s.twistMove();
                TwistMove[i] = s.getIdx();
            }
            for (int i = 0; i < 3768 * 2; i++)
            {
                ShapePrun[i] = -1;
                ShapePrunOpt[i] = -1;
            }

            //0 110110110110 011011011011
            //1 110110110110 110110110110
            //1 011011011011 011011011011
            //0 011011011011 110110110110
            ShapePrun[getShape2Idx(0x0db66db)] = 0;
            ShapePrun[getShape2Idx(0x1db6db6)] = 0;
            ShapePrun[getShape2Idx(0x16db6db)] = 0;
            ShapePrun[getShape2Idx(0x06dbdb6)] = 0;
            ShapePrunOpt[new FullCube().getShapeIdx()] = 0;
            int done = 4;
            int done0 = 0;
            int depth = -1;
            while (done != done0)
            {
                done0 = done;
                ++depth;
                //System.out.println(done);
                for (int i = 0; i < 3768 * 2; i++)
                {
                    if (ShapePrun[i] == depth)
                    {
                        // try top
                        int m = 0;
                        int idx = i;
                        do
                        {
                            idx = TopMove[idx];
                            m += idx & 0xf;
                            idx >>= 4;
                            if (ShapePrun[idx] == -1)
                            {
                                ++done;
                                ShapePrun[idx] = depth + 1;
                            }
                        } while (m != 12);

                        // try bottom
                        m = 0;
                        idx = i;
                        do
                        {
                            idx = BottomMove[idx];
                            m += idx & 0xf;
                            idx >>= 4;
                            if (ShapePrun[idx] == -1)
                            {
                                ++done;
                                ShapePrun[idx] = depth + 1;
                            }
                        } while (m != 12);

                        // try twist
                        idx = TwistMove[i];
                        if (ShapePrun[idx] == -1)
                        {
                            ++done;
                            ShapePrun[idx] = depth + 1;
                        }
                    }
                }
            }
            done = 1;
            done0 = 0;
            depth = -1;
            while (done != done0)
            {
                done0 = done;
                ++depth;
                //System.out.println(done);
                for (int i = 0; i < 3768 * 2; i++)
                {
                    if (ShapePrunOpt[i] == depth)
                    {
                        // try top
                        int m = 0;
                        int idx = i;
                        do
                        {
                            idx = TopMove[idx];
                            m += idx & 0xf;
                            idx >>= 4;
                            if (ShapePrunOpt[idx] == -1)
                            {
                                ++done;
                                ShapePrunOpt[idx] = depth + 1;
                            }
                        } while (m != 12);

                        // try bottom
                        m = 0;
                        idx = i;
                        do
                        {
                            idx = BottomMove[idx];
                            m += idx & 0xf;
                            idx >>= 4;
                            if (ShapePrunOpt[idx] == -1)
                            {
                                ++done;
                                ShapePrunOpt[idx] = depth + 1;
                            }
                        } while (m != 12);

                        // try twist
                        idx = TwistMove[i];
                        if (ShapePrunOpt[idx] == -1)
                        {
                            ++done;
                            ShapePrunOpt[idx] = depth + 1;
                        }
                    }
                }
            }
            inited = true;
        }

    }

}
