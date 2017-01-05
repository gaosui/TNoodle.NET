using System;
using TNoodle.Utils;

namespace TNoodle.Solvers.sq12phase
{
    internal class Shape
    {
        public static event Action<string> Log;
        //1 = corner, 0 = edge.
        private static readonly int[] Halflayer =
        {
            0x00, 0x03, 0x06, 0x0c, 0x0f, 0x18, 0x1b, 0x1e,
            0x30, 0x33, 0x36, 0x3c, 0x3f
        };

        private static bool _inited;
        private int _bottom;
        private int _parity;

        private int _top;

        private Shape()
        {
        }

        internal static int[] ShapeIdx { get; } = new int[3678];
        internal static int[] ShapePrun { get; } = new int[3768 * 2];
        internal static int[] ShapePrunOpt { get; } = new int[3768 * 2];

        internal static int[] TopMove { get; } = new int[3678 * 2];
        internal static int[] BottomMove { get; } = new int[3678 * 2];
        internal static int[] TwistMove { get; } = new int[3678 * 2];

        internal static int GetShape2Idx(int shp)
        {
            var ret = (Array.BinarySearch(ShapeIdx, shp & 0xffffff) << 1) | (shp >> 24);
            return ret;
        }

        private int GetIdx()
        {
            var ret = (Array.BinarySearch(ShapeIdx, (_top << 12) | _bottom) << 1) | _parity;
            return ret;
        }

        private void SetIdx(int idx)
        {
            _parity = idx & 1;
            _top = ShapeIdx[idx >> 1];
            _bottom = _top & 0xfff;
            _top >>= 12;
        }

        private int TopMoveMth()
        {
            var move = 0;
            var moveParity = 0;
            do
            {
                if ((_top & 0x800) == 0)
                {
                    move += 1;
                    _top = _top << 1;
                }
                else
                {
                    move += 2;
                    _top = (_top << 2) ^ 0x3003;
                }
                moveParity = 1 - moveParity;
            } while ((Functions.BitCount(_top & 0x3f) & 1) != 0);
            if ((Functions.BitCount(_top) & 2) == 0)
                _parity ^= moveParity;
            return move;
        }

        private int BottomMoveMth()
        {
            var move = 0;
            var moveParity = 0;
            do
            {
                if ((_bottom & 0x800) == 0)
                {
                    move += 1;
                    _bottom = _bottom << 1;
                }
                else
                {
                    move += 2;
                    _bottom = (_bottom << 2) ^ 0x3003;
                }
                moveParity = 1 - moveParity;
            } while ((Functions.BitCount(_bottom & 0x3f) & 1) != 0);
            if ((Functions.BitCount(_bottom) & 2) == 0)
                _parity ^= moveParity;
            return move;
        }

        private void TwistMoveMth()
        {
            var temp = _top & 0x3f;
            var p1 = Functions.BitCount(temp);
            var p3 = Functions.BitCount(_bottom & 0xfc0);
            _parity ^= 1 & ((p1 & p3) >> 1);

            _top = (_top & 0xfc0) | ((_bottom >> 6) & 0x3f);
            _bottom = (_bottom & 0x3f) | (temp << 6);
        }

        internal static void Init()
        {
            if (_inited)
                return;
            var count = 0;
            for (var i = 0; i < 13 * 13 * 13 * 13; i++)
            {
                var dr = Halflayer[i % 13];
                var dl = Halflayer[i / 13 % 13];
                var ur = Halflayer[i / 13 / 13 % 13];
                var ul = Halflayer[i / 13 / 13 / 13];
                var value = (ul << 18) | (ur << 12) | (dl << 6) | dr;
                if (Functions.BitCount(value) == 16)
                    ShapeIdx[count++] = value;
            }
            //System.out.println(count);
            Log?.Invoke(count.ToString());
            var s = new Shape();
            for (var i = 0; i < 3678 * 2; i++)
            {
                s.SetIdx(i);
                TopMove[i] = s.TopMoveMth();
                TopMove[i] |= s.GetIdx() << 4;
                s.SetIdx(i);
                BottomMove[i] = s.BottomMoveMth();
                BottomMove[i] |= s.GetIdx() << 4;
                s.SetIdx(i);
                s.TwistMoveMth();
                TwistMove[i] = s.GetIdx();
            }
            for (var i = 0; i < 3768 * 2; i++)
            {
                ShapePrun[i] = -1;
                ShapePrunOpt[i] = -1;
            }

            //0 110110110110 011011011011
            //1 110110110110 110110110110
            //1 011011011011 011011011011
            //0 011011011011 110110110110
            ShapePrun[GetShape2Idx(0x0db66db)] = 0;
            ShapePrun[GetShape2Idx(0x1db6db6)] = 0;
            ShapePrun[GetShape2Idx(0x16db6db)] = 0;
            ShapePrun[GetShape2Idx(0x06dbdb6)] = 0;
            ShapePrunOpt[new FullCube().GetShapeIdx()] = 0;
            var done = 4;
            var done0 = 0;
            var depth = -1;
            while (done != done0)
            {
                done0 = done;
                ++depth;
                //System.out.println(done);
                Log?.Invoke(done.ToString());
                for (var i = 0; i < 3768 * 2; i++)
                {
                    if (ShapePrun[i] != depth) continue;
                    // try top
                    var m = 0;
                    var idx = i;
                    do
                    {
                        idx = TopMove[idx];
                        m += idx & 0xf;
                        idx >>= 4;
                        if (ShapePrun[idx] != -1) continue;
                        ++done;
                        ShapePrun[idx] = depth + 1;
                    } while (m != 12);

                    // try bottom
                    m = 0;
                    idx = i;
                    do
                    {
                        idx = BottomMove[idx];
                        m += idx & 0xf;
                        idx >>= 4;
                        if (ShapePrun[idx] != -1) continue;
                        ++done;
                        ShapePrun[idx] = depth + 1;
                    } while (m != 12);

                    // try twist
                    idx = TwistMove[i];
                    if (ShapePrun[idx] != -1) continue;
                    ++done;
                    ShapePrun[idx] = depth + 1;
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
                Log?.Invoke(done.ToString());
                for (var i = 0; i < 3768 * 2; i++)
                {
                    if (ShapePrunOpt[i] != depth) continue;
                    // try top
                    var m = 0;
                    var idx = i;
                    do
                    {
                        idx = TopMove[idx];
                        m += idx & 0xf;
                        idx >>= 4;
                        if (ShapePrunOpt[idx] != -1) continue;
                        ++done;
                        ShapePrunOpt[idx] = depth + 1;
                    } while (m != 12);

                    // try bottom
                    m = 0;
                    idx = i;
                    do
                    {
                        idx = BottomMove[idx];
                        m += idx & 0xf;
                        idx >>= 4;
                        if (ShapePrunOpt[idx] != -1) continue;
                        ++done;
                        ShapePrunOpt[idx] = depth + 1;
                    } while (m != 12);

                    // try twist
                    idx = TwistMove[i];
                    if (ShapePrunOpt[idx] != -1) continue;
                    ++done;
                    ShapePrunOpt[idx] = depth + 1;
                }
            }
            _inited = true;
        }
    }
}