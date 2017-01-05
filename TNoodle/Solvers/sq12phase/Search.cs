using System;
using System.Text;

namespace TNoodle.Solvers.sq12phase
{
    public class Search
    {
        private readonly FullCube _d = new FullCube();
        private readonly int[] _move = new int[100];

        private readonly Square _sq = new Square();
        private FullCube _c;
        private int _length1;
        private int _maxlen2;
        private string _solString;

        static Search()
        {
            Shape.Init();
            Square.Init();
        }

        public string Solution(FullCube c)
        {
            _c = c;
            _solString = null;
            var shape = c.GetShapeIdx();
            for (_length1 = Shape.ShapePrun[shape]; _length1 < 100; _length1++)
            {
                _maxlen2 = Math.Min(31 - _length1, 17);
                if (Phase1(shape, Shape.ShapePrun[shape], _length1, 0, -1))
                    break;
            }
            return _solString;
        }

        public string SolutionOpt(FullCube c, int maxl)
        {
            _c = c;
            _solString = null;
            var shape = c.GetShapeIdx();
            for (_length1 = Shape.ShapePrunOpt[shape]; _length1 <= maxl; _length1++)
                if (Phase1Opt(shape, _length1, 0, -1))
                    break;
            return _solString;
        }


        private bool Phase1Opt(int shape, int maxl, int depth, int lm)
        {
            if (maxl == 0)
                return IsSolvedInPhase1();
            //try each possible move. First twist;
            if (lm != 0)
            {
                var shapexx = Shape.TwistMove[shape];
                var prunx = Shape.ShapePrunOpt[shapexx];
                if (prunx < maxl)
                {
                    _move[depth] = 0;
                    if (Phase1(shapexx, prunx, maxl - 1, depth + 1, 0))
                        return true;
                }
            }

            //Try top layer
            var shapex = shape;
            if (lm <= 0)
            {
                var m = 0;
                while (true)
                {
                    m += Shape.TopMove[shapex];
                    shapex = m >> 4;
                    m &= 0x0f;
                    if (m >= 12)
                        break;
                    var prunx = Shape.ShapePrunOpt[shapex];
                    if (prunx > maxl)
                        break;
                    if (prunx >= maxl) continue;
                    _move[depth] = m;
                    if (Phase1(shapex, prunx, maxl - 1, depth + 1, 1))
                        return true;
                }
            }

            shapex = shape;
            //Try bottom layer
            if (lm > 1) return false;
            {
                var m = 0;
                while (true)
                {
                    m += Shape.BottomMove[shapex];
                    shapex = m >> 4;
                    m &= 0x0f;
                    if (m >= 6)
                        break;
                    var prunx = Shape.ShapePrunOpt[shapex];
                    if (prunx > maxl)
                        break;
                    if (prunx >= maxl) continue;
                    _move[depth] = -m;
                    if (Phase1(shapex, prunx, maxl - 1, depth + 1, 2))
                        return true;
                }
            }

            return false;
        }

        private bool Phase1(int shape, int prunvalue, int maxl, int depth, int lm)
        {
            if (prunvalue == 0 && maxl < 4)
                return maxl == 0 && Init2();

            //try each possible move. First twist;
            if (lm != 0)
            {
                var shapexx = Shape.TwistMove[shape];
                var prunx = Shape.ShapePrun[shapexx];
                if (prunx < maxl)
                {
                    _move[depth] = 0;
                    if (Phase1(shapexx, prunx, maxl - 1, depth + 1, 0))
                        return true;
                }
            }

            //Try top layer
            var shapex = shape;
            if (lm <= 0)
            {
                var m = 0;
                while (true)
                {
                    m += Shape.TopMove[shapex];
                    shapex = m >> 4;
                    m &= 0x0f;
                    if (m >= 12)
                        break;
                    var prunx = Shape.ShapePrun[shapex];
                    if (prunx > maxl)
                        break;
                    if (prunx >= maxl) continue;
                    _move[depth] = m;
                    if (Phase1(shapex, prunx, maxl - 1, depth + 1, 1))
                        return true;
                }
            }

            shapex = shape;
            //Try bottom layer
            if (lm > 1) return false;
            {
                var m = 0;
                while (true)
                {
                    m += Shape.BottomMove[shapex];
                    shapex = m >> 4;
                    m &= 0x0f;
                    if (m >= 6)
                        break;
                    var prunx = Shape.ShapePrun[shapex];
                    if (prunx > maxl)
                        break;
                    if (prunx >= maxl) continue;
                    _move[depth] = -m;
                    if (Phase1(shapex, prunx, maxl - 1, depth + 1, 2))
                        return true;
                }
            }

            return false;
        }

        private bool IsSolvedInPhase1()
        {
            _d.Copy(_c);
            for (var i = 0; i < _length1; i++)
                _d.DoMove(_move[i]);
            var isSolved = _d.Ul == 0x011233 && _d.Ur == 455677 && _d.Dl == 0x998bba && _d.Dr == 0xddcffe && _d.Ml == 0;
            if (isSolved)
                _solString = Move2String(_length1);
            return isSolved;
        }

        private bool Init2()
        {
            _d.Copy(_c);
            for (var i = 0; i < _length1; i++)
                _d.DoMove(_move[i]);
            //assert Shape.ShapePrun[d.getShapeIdx()] == 0;
            _d.GetSquare(_sq);

            var edge = _sq.Edgeperm;
            var corner = _sq.Cornperm;
            var ml = _sq.Ml;

            int prun = Math.Max(Square.SquarePrun[(_sq.Edgeperm << 1) | ml], Square.SquarePrun[(_sq.Cornperm << 1) | ml]);

            for (var i = prun; i < _maxlen2; i++)
            {
                if (!Phase2(edge, corner, _sq.TopEdgeFirst, _sq.BotEdgeFirst, ml, i, _length1, 0)) continue;
                _solString = Move2String(i + _length1);
                return true;
            }

            return false;
        }

        private string Move2String(int len)
        {
            //TODO whether to invert the solution or not should be set by params.
            var s = new StringBuilder();
            int top = 0, bottom = 0;
            for (var i = len - 1; i >= 0; i--)
            {
                var val = _move[i];
                if (val > 0)
                {
                    val = 12 - val;
                    top = val > 6 ? val - 12 : val;
                }
                else if (val < 0)
                {
                    val = 12 + val;
                    bottom = val > 6 ? val - 12 : val;
                }
                else
                {
                    if (top == 0 && bottom == 0)
                        s.Append(" / ");
                    else
                        s.Append('(').Append(top).Append(",").Append(bottom).Append(") / ");
                    top = 0;
                    bottom = 0;
                }
            }
            if (top == 0 && bottom == 0)
            {
            }
            else
            {
                s.Append('(').Append(top).Append(",").Append(bottom).Append(")");
            }
            return s.ToString();
        }

        private bool Phase2(int edge, int corner, bool topEdgeFirst, bool botEdgeFirst, int ml, int maxl, int depth,
            int lm)
        {
            if (maxl == 0 && !topEdgeFirst && botEdgeFirst)
                return true;

            //try each possible move. First twist;
            if (lm != 0 && topEdgeFirst == botEdgeFirst)
            {
                int edgex = Square.TwistMove[edge];
                int cornerx = Square.TwistMove[corner];

                if (Square.SquarePrun[(edgex << 1) | (1 - ml)] < maxl &&
                    Square.SquarePrun[(cornerx << 1) | (1 - ml)] < maxl)
                {
                    _move[depth] = 0;
                    if (Phase2(edgex, cornerx, topEdgeFirst, botEdgeFirst, 1 - ml, maxl - 1, depth + 1, 0))
                        return true;
                }
            }

            //Try top layer
            if (lm <= 0)
            {
                var topEdgeFirstx = !topEdgeFirst;
                var edgex = topEdgeFirstx ? Square.TopMove[edge] : edge;
                var cornerx = topEdgeFirstx ? corner : Square.TopMove[corner];
                var m = topEdgeFirstx ? 1 : 2;
                int prun1 = Square.SquarePrun[(edgex << 1) | ml];
                int prun2 = Square.SquarePrun[(cornerx << 1) | ml];
                while (m < 12 && prun1 <= maxl && prun1 <= maxl)
                {
                    if (prun1 < maxl && prun2 < maxl)
                    {
                        _move[depth] = m;
                        if (Phase2(edgex, cornerx, topEdgeFirstx, botEdgeFirst, ml, maxl - 1, depth + 1, 1))
                            return true;
                    }
                    topEdgeFirstx = !topEdgeFirstx;
                    if (topEdgeFirstx)
                    {
                        edgex = Square.TopMove[edgex];
                        prun1 = Square.SquarePrun[(edgex << 1) | ml];
                        m += 1;
                    }
                    else
                    {
                        cornerx = Square.TopMove[cornerx];
                        prun2 = Square.SquarePrun[(cornerx << 1) | ml];
                        m += 2;
                    }
                }
            }

            if (lm > 1) return false;
            {
                var botEdgeFirstx = !botEdgeFirst;
                var edgex = botEdgeFirstx ? Square.BottomMove[edge] : edge;
                var cornerx = botEdgeFirstx ? corner : Square.BottomMove[corner];
                var m = botEdgeFirstx ? 1 : 2;
                int prun1 = Square.SquarePrun[(edgex << 1) | ml];
                int prun2 = Square.SquarePrun[(cornerx << 1) | ml];
                while (m < (maxl > 6 ? 6 : 12) && prun1 <= maxl && prun1 <= maxl)
                {
                    if (prun1 < maxl && prun2 < maxl)
                    {
                        _move[depth] = -m;
                        if (Phase2(edgex, cornerx, topEdgeFirst, botEdgeFirstx, ml, maxl - 1, depth + 1, 2))
                            return true;
                    }
                    botEdgeFirstx = !botEdgeFirstx;
                    if (botEdgeFirstx)
                    {
                        edgex = Square.BottomMove[edgex];
                        prun1 = Square.SquarePrun[(edgex << 1) | ml];
                        m += 1;
                    }
                    else
                    {
                        cornerx = Square.BottomMove[cornerx];
                        prun2 = Square.SquarePrun[(cornerx << 1) | ml];
                        m += 2;
                    }
                }
            }
            return false;
        }
    }
}