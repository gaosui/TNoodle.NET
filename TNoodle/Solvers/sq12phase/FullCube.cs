using System;

namespace TNoodle.Solvers.sq12phase
{
    public class FullCube : IComparable<FullCube>
    {
        private readonly int[] _arr = new int[16];

        private readonly sbyte[] _prm = new sbyte[8];
        internal int Ul { get; set; } = 0x011233;
        internal int Ur { get; set; } = 0x455677;
        internal int Dl { get; set; } = 0x998bba;
        internal int Dr { get; set; } = 0xddcffe;
        internal int Ml { get; set; }

        public int CompareTo(FullCube f)
        {
            if (Ul != f.Ul)
                return Ul - f.Ul;
            if (Ur != f.Ur)
                return Ur - f.Ur;
            if (Dl != f.Dl)
                return Dl - f.Dl;
            if (Dr != f.Dr)
                return Dr - f.Dr;
            return Ml - f.Ml;
        }

        public static FullCube RandomCube(Random r)
        {
            var shape = Shape.ShapeIdx[r.Next(3678)];
            var f = new FullCube();
            var corner = (0x01234567 << 1) | 0x11111111;
            var edge = 0x01234567 << 1;
            int nCorner = 8, nEdge = 8;
            for (var i = 0; i < 24; i++)
            {
                int rnd;
                int m;
                if (((shape >> i) & 1) == 0)
                {
//edge
                    rnd = r.Next(nEdge) << 2;
                    f.SetPiece(23 - i, (edge >> rnd) & 0xf);
                    m = (1 << rnd) - 1;
                    edge = (edge & m) + ((edge >> 4) & ~m);
                    --nEdge;
                }
                else
                {
//corner
                    rnd = r.Next(nCorner) << 2;
                    f.SetPiece(23 - i, (corner >> rnd) & 0xf);
                    f.SetPiece(22 - i, (corner >> rnd) & 0xf);
                    m = (1 << rnd) - 1;
                    corner = (corner & m) + ((corner >> 4) & ~m);
                    --nCorner;
                    ++i;
                }
            }
            f.Ml = r.Next(2);
            return f;
        }

        internal void Copy(FullCube c)
        {
            Ul = c.Ul;
            Ur = c.Ur;
            Dl = c.Dl;
            Dr = c.Dr;
            Ml = c.Ml;
        }

        /**
         * @param move
         * 0 = twist
         * [1, 11] = top move
         * [-1, -11] = bottom move
         * for example, 6 == (6, 0), 9 == (-3, 0), -4 == (0, 4)
         */

        internal void DoMove(int move)
        {
            move <<= 2;
            if (move > 24)
            {
                move = 48 - move;
                var temp = Ul;
                Ul = ((Ul >> move) | (Ur << (24 - move))) & 0xffffff;
                Ur = ((Ur >> move) | (temp << (24 - move))) & 0xffffff;
            }
            else if (move > 0)
            {
                var temp = Ul;
                Ul = ((Ul << move) | (Ur >> (24 - move))) & 0xffffff;
                Ur = ((Ur << move) | (temp >> (24 - move))) & 0xffffff;
            }
            else if (move == 0)
            {
                var temp = Ur;
                Ur = Dl;
                Dl = temp;
                Ml = 1 - Ml;
            }
            else if (move >= -24)
            {
                move = -move;
                var temp = Dl;
                Dl = ((Dl << move) | (Dr >> (24 - move))) & 0xffffff;
                Dr = ((Dr << move) | (temp >> (24 - move))) & 0xffffff;
            }
            else if (move < -24)
            {
                move = 48 + move;
                var temp = Dl;
                Dl = ((Dl >> move) | (Dr << (24 - move))) & 0xffffff;
                Dr = ((Dr >> move) | (temp << (24 - move))) & 0xffffff;
            }
        }

        private sbyte PieceAt(int idx)
        {
            int ret;
            if (idx < 6)
                ret = Ul >> ((5 - idx) << 2);
            else if (idx < 12)
                ret = Ur >> ((11 - idx) << 2);
            else if (idx < 18)
                ret = Dl >> ((17 - idx) << 2);
            else
                ret = Dr >> ((23 - idx) << 2);
            return (sbyte) (ret & 0x0f);
        }

        private void SetPiece(int idx, int value)
        {
            if (idx < 6)
            {
                Ul &= ~(0xf << ((5 - idx) << 2));
                Ul |= value << ((5 - idx) << 2);
            }
            else if (idx < 12)
            {
                Ur &= ~(0xf << ((11 - idx) << 2));
                Ur |= value << ((11 - idx) << 2);
            }
            else if (idx < 18)
            {
                Dl &= ~(0xf << ((17 - idx) << 2));
                Dl |= value << ((17 - idx) << 2);
            }
            else if (idx < 24)
            {
                Dr &= ~(0xf << ((23 - idx) << 2));
                Dr |= value << ((23 - idx) << 2);
            }
            else
            {
                Ml = value;
            }
        }

        private int GetParity()
        {
            var cnt = 0;
            _arr[0] = PieceAt(0);
            for (var i = 1; i < 24; i++)
                if (PieceAt(i) != _arr[cnt])
                    _arr[++cnt] = PieceAt(i);
            var p = 0;
            for (var a = 0; a < 16; a++)
            for (var b = a + 1; b < 16; b++)
                if (_arr[a] > _arr[b])
                    p ^= 1;
            return p;
        }

        internal int GetShapeIdx()
        {
            var urx = Ur & 0x111111;
            urx |= urx >> 3;
            urx |= urx >> 6;
            urx = (urx & 0xf) | ((urx >> 12) & 0x30);
            var ulx = Ul & 0x111111;
            ulx |= ulx >> 3;
            ulx |= ulx >> 6;
            ulx = (ulx & 0xf) | ((ulx >> 12) & 0x30);
            var drx = Dr & 0x111111;
            drx |= drx >> 3;
            drx |= drx >> 6;
            drx = (drx & 0xf) | ((drx >> 12) & 0x30);
            var dlx = Dl & 0x111111;
            dlx |= dlx >> 3;
            dlx |= dlx >> 6;
            dlx = (dlx & 0xf) | ((dlx >> 12) & 0x30);
            return Shape.GetShape2Idx((GetParity() << 24) | (ulx << 18) | (urx << 12) | (dlx << 6) | drx);
        }

        internal void GetSquare(Square sq)
        {
            for (var i = 0; i < 8; i++)
                _prm[i] = (sbyte) (PieceAt(i * 3 + 1) >> 1);
            //convert to number
            sq.Cornperm = Square.Get8Perm(_prm);

            int a, b;
            //Strip top layer edges
            sq.TopEdgeFirst = PieceAt(0) == PieceAt(1);
            a = sq.TopEdgeFirst ? 2 : 0;
            for (b = 0; b < 4; a += 3, b++)
                _prm[b] = (sbyte) (PieceAt(a) >> 1);

            sq.BotEdgeFirst = PieceAt(12) == PieceAt(13);
            a = sq.BotEdgeFirst ? 14 : 12;

            for (; b < 8; a += 3, b++)
                _prm[b] = (sbyte) (PieceAt(a) >> 1);
            sq.Edgeperm = Square.Get8Perm(_prm);
            sq.Ml = Ml;
        }
    }
}