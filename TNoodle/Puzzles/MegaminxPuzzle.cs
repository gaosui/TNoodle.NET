using System;
using System.Collections.Generic;
using System.Text;
using TNoodle.Utils;
using static TNoodle.Utils.Assertion;

namespace TNoodle.Puzzles
{
    public class MegaminxPuzzle : Puzzle
    {
        public enum Face
        {
            U,
            Bl,
            Br,
            R,
            F,
            L,
            D,
            Dr,
            Dbr,
            B,
            Dbl,
            Dl
        }

        public override string GetLongName()
        {
            return "Megaminx";
        }

        public override string GetShortName()
        {
            return "minx";
        }


        private static void Turn(int[][] image, Face side, int dir)
        {
            dir = Functions.Modulo(dir, 5);
            for (var i = 0; i < dir; i++)
            {
                Turn(image, side);
            }
        }

        private static void Turn(int[][] image, Face face)
        {
            var s = (int) face;
            var b = (s >= 6 ? 6 : 0);
            switch (s % 6)
            {
                case 0:
                    SwapOnSide(image, b, 1, 6, 5, 4, 4, 2, 3, 0, 2, 8);
                    break;
                case 1:
                    SwapOnSide(image, b, 0, 0, 2, 0, 9, 6, 10, 6, 5, 2);
                    break;
                case 2:
                    SwapOnSide(image, b, 0, 2, 3, 2, 8, 4, 9, 4, 1, 4);
                    break;
                case 3:
                    SwapOnSide(image, b, 0, 4, 4, 4, 7, 2, 8, 2, 2, 6);
                    break;
                case 4:
                    SwapOnSide(image, b, 0, 6, 5, 6, 11, 0, 7, 0, 3, 8);
                    break;
                case 5:
                    SwapOnSide(image, b, 0, 8, 1, 8, 10, 8, 11, 8, 4, 0);
                    break;
                default:
                    Assert(false);
                    break;
            }

            RotateFace(image, face);
        }

        private static void SwapOnSide(int[][] image, int b, int f1, int s1, int f2, int s2, int f3, int s3, int f4,
            int s4, int f5, int s5)
        {
            for (var i = 0; i < 3; i++)
            {
                var temp = image[(f1 + b) % 12][(s1 + i) % 10];
                image[(f1 + b) % 12][(s1 + i) % 10] = image[(f2 + b) % 12][(s2 + i) % 10];
                image[(f2 + b) % 12][(s2 + i) % 10] = image[(f3 + b) % 12][(s3 + i) % 10];
                image[(f3 + b) % 12][(s3 + i) % 10] = image[(f4 + b) % 12][(s4 + i) % 10];
                image[(f4 + b) % 12][(s4 + i) % 10] = image[(f5 + b) % 12][(s5 + i) % 10];
                image[(f5 + b) % 12][(s5 + i) % 10] = temp;
            }
        }

        private static void SwapOnFace(int[][] image, Face face, int s1, int s2, int s3, int s4, int s5)
        {
            var f = (int) face;
            var temp = image[f][s1];
            image[f][s1] = image[f][s2];
            image[f][s2] = image[f][s3];
            image[f][s3] = image[f][s4];
            image[f][s4] = image[f][s5];
            image[f][s5] = temp;
        }

        private static void RotateFace(int[][] image, Face f)
        {
            SwapOnFace(image, f, 0, 8, 6, 4, 2);
            SwapOnFace(image, f, 1, 9, 7, 5, 3);
        }

        private static void BigTurn(int[][] image, Face side, int dir)
        {
            dir = Functions.Modulo(dir, 5);
            for (var i = 0; i < dir; i++)
            {
                BigTurn(image, side);
            }
        }

        private static void BigTurn(int[][] image, Face f)
        {
            if (f == Face.Dbr)
            {
                for (var i = 0; i < 7; i++)
                {
                    Swap(image, 0, (1 + i) % 10, 4, (3 + i) % 10, 11, (1 + i) % 10, 10, (1 + i) % 10, 1, (1 + i) % 10);
                }
                SwapCenters(image, 0, 4, 11, 10, 1);

                SwapWholeFace(image, 2, 0, 3, 0, 7, 0, 6, 8, 9, 8);

                RotateFace(image, Face.Dbr);
            }
            else
            {
                //azzert(f == Face.D);
                for (var i = 0; i < 7; i++)
                {
                    Swap(image, 1, (9 + i) % 10, 2, (1 + i) % 10, 3, (3 + i) % 10, 4, (5 + i) % 10, 5, (7 + i) % 10);
                }
                SwapCenters(image, 1, 2, 3, 4, 5);

                SwapWholeFace(image, 11, 0, 10, 8, 9, 6, 8, 4, 7, 2);

                RotateFace(image, Face.D);
            }
        }

        private static void Swap(int[][] image, int f1, int s1, int f2, int s2, int f3, int s3, int f4, int s4, int f5,
            int s5)
        {
            var temp = image[f1][s1];
            image[f1][s1] = image[f2][s2];
            image[f2][s2] = image[f3][s3];
            image[f3][s3] = image[f4][s4];
            image[f4][s4] = image[f5][s5];
            image[f5][s5] = temp;
        }

        private static void SwapCenters(int[][] image, int f1, int f2, int f3, int f4, int f5)
        {
            Swap(image, f1, 10, f2, 10, f3, 10, f4, 10, f5, 10);
        }

        private static void SwapWholeFace(int[][] image, int f1, int s1, int f2, int s2, int f3, int s3, int f4, int s4,
            int f5, int s5)
        {
            for (var i = 0; i < 10; i++)
            {
                var temp = image[(f1) % 12][(s1 + i) % 10];
                image[(f1) % 12][(s1 + i) % 10] = image[(f2) % 12][(s2 + i) % 10];
                image[(f2) % 12][(s2 + i) % 10] = image[(f3) % 12][(s3 + i) % 10];
                image[(f3) % 12][(s3 + i) % 10] = image[(f4) % 12][(s4 + i) % 10];
                image[(f4) % 12][(s4 + i) % 10] = image[(f5) % 12][(s5 + i) % 10];
                image[(f5) % 12][(s5 + i) % 10] = temp;
            }
            SwapCenters(image, f1, f2, f3, f4, f5);
        }


        public override PuzzleState GetSolvedState()
        {
            return new MegaminxState(this);
        }

        protected override int GetRandomMoveCount()
        {
            return 11 * 7;
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            var scramble = new StringBuilder();

            const int width = 10;
            const int height = 7;
            for (var i = 0; i < height; i++)
            {
                if (i > 0)
                {
                    scramble.Append("\n");
                }
                var dir = 0;
                for (var j = 0; j < width; j++)
                {
                    if (j > 0)
                    {
                        scramble.Append(" ");
                    }
                    var side = (j % 2 == 0) ? 'R' : 'D';
                    dir = r.Next(2);
                    scramble.Append(side + ((dir == 0) ? "++" : "--"));
                }
                scramble.Append(" U");
                if (dir != 0)
                {
                    scramble.Append("'");
                }
            }

            var scrambleStr = scramble.ToString();

            var state = GetSolvedState();
            try
            {
                state = state.ApplyAlgorithm(scrambleStr);
            }
            catch (InvalidScrambleException e)
            {
                Assert(false, e.Message, e);
                return null;
            }
            return new PuzzleStateAndGenerator(state, scrambleStr);
        }

        private const int CenterIndex = 10;

        private static bool IsNormalized(int[][] image)
        {
            return image[(int) Face.U][CenterIndex] == (int) Face.U && image[(int) Face.F][CenterIndex] == (int) Face.F;
        }

        private static int[][] CloneImage(int[][] image)
        {
            var imageCopy = ArrayExtension.New<int>(image.Length, image[0].Length);
            image.DeepCopyTo(imageCopy);
            //GwtSafeUtils.deepCopy(image, imageCopy);
            return imageCopy;
        }

        private static void SpinMinx(int[][] image, Face face, int dir)
        {
            Turn(image, face, dir);
            BigTurn(image, (Face) face.OppositeFace(), 5 - dir);
        }

        private static void SpinToTop(int[][] image, Face face)
        {
            switch (face)
            {
                case Face.U:
                    break;
                case Face.Bl:
                    SpinMinx(image, Face.L, 1);
                    break;
                case Face.Br:
                    SpinMinx(image, Face.U, 1);
                    SpinToTop(image, Face.R);
                    break;
                case Face.R:
                    SpinMinx(image, Face.U, 1);
                    SpinToTop(image, Face.F);
                    break;
                case Face.F:
                    SpinMinx(image, Face.L, -1);
                    break;
                case Face.L:
                    SpinMinx(image, Face.U, 1);
                    SpinToTop(image, Face.Bl);
                    break;
                case Face.D:
                    SpinMinx(image, Face.L, -2);
                    SpinToTop(image, Face.R);
                    break;
                case Face.Dr:
                    SpinMinx(image, Face.L, -1);
                    SpinToTop(image, Face.R);
                    break;
                case Face.Dbr:
                    SpinMinx(image, Face.U, 1);
                    SpinMinx(image, Face.L, -1);
                    SpinToTop(image, Face.R);
                    break;
                case Face.B:
                    SpinMinx(image, Face.L, -3);
                    SpinToTop(image, Face.R);
                    break;
                case Face.Dbl:
                    SpinMinx(image, Face.L, 2);
                    break;
                case Face.Dl:
                    SpinMinx(image, Face.L, -2);
                    break;
                default:
                    Assert(false);
                    break;
            }
        }

        private static int[][] Normalize(int[][] image)
        {
            if (IsNormalized(image))
            {
                return image;
            }

            image = CloneImage(image);
            foreach (Face face in Enum.GetValues(typeof(Face)))
            {
                if (image[(int) face][CenterIndex] == (int) Face.U)
                {
                    SpinToTop(image, face);
                    Assert(image[(int) Face.U][CenterIndex] == (int) Face.U);
                    for (var chooseF = 0; chooseF < 5; chooseF++)
                    {
                        SpinMinx(image, Face.U, 1);
                        if (IsNormalized(image))
                        {
                            return image;
                        }
                    }
                    Assert(false);
                }
            }
            Assert(false);
            return null;
        }

        internal class MegaminxState : PuzzleState
        {
            private readonly MegaminxPuzzle _puzzle;
            private readonly int[][] _image;
            private MegaminxState _normalizedState;

            public MegaminxState(MegaminxPuzzle p) : base(p)
            {
                _puzzle = p;
                _image = ArrayExtension.New<int>(12, 11);
                for (var i = 0; i < _image.Length; i++)
                {
                    for (var j = 0; j < _image[0].Length; j++)
                    {
                        _image[i][j] = i;
                    }
                }
                _normalizedState = this;
            }

            public MegaminxState(int[][] image, MegaminxPuzzle p) : base(p)
            {
                _puzzle = p;
                _image = image;
            }

            public override PuzzleState GetNormalized()
            {
                return _normalizedState ?? (_normalizedState = new MegaminxState(Normalize(_image), _puzzle));
            }

            public override bool IsNormalized()
            {
                return MegaminxPuzzle.IsNormalized(_image);
            }

            public override LinkedHashMap<string, PuzzleState> GetSuccessorsByName()
            {
                var successors = new LinkedHashMap<string, PuzzleState>();

                string[] prettyDir = {null, "", "2", "2'", "'"};
                foreach (Face face in Enum.GetValues(typeof(Face)))
                {
                    for (var dir = 1; dir <= 4; dir++)
                    {
                        var move = face.ToString();
                        move += prettyDir[dir];

                        var imageCopy = CloneImage(_image);
                        Turn(imageCopy, face, dir);

                        successors[move] = new MegaminxState(imageCopy, _puzzle);
                    }
                }

                var pochmannFaceNames = new Dictionary<string, Face>
                {
                    ["R"] = Face.Dbr,
                    ["D"] = Face.D
                };
                string[] prettyPochmannDir = {null, "+", "++", "--", "-"};
                foreach (var pochmannFaceName in pochmannFaceNames.Keys)
                {
                    for (var dir = 1; dir < 5; dir++)
                    {
                        var move = pochmannFaceName + prettyPochmannDir[dir];

                        var imageCopy = CloneImage(_image);
                        BigTurn(imageCopy, pochmannFaceNames[pochmannFaceName], dir);

                        successors[move] = new MegaminxState(imageCopy, _puzzle);
                    }
                }
                return successors;
            }

            public override LinkedHashMap<string, PuzzleState> GetScrambleSuccessors()
            {
                var successors = GetSuccessorsByName();
                var scrambleSuccessors = new LinkedHashMap<string, PuzzleState>();
                foreach (var turn in new[] {"R++", "R--", "D++", "D--", "U", "U2", "U2'", "U'"})
                {
                    scrambleSuccessors[turn] = successors[turn];
                }
                return scrambleSuccessors;
            }

            public override bool Equals(object other)
            {
                var o = ((MegaminxState) other);
                return _image.DeepEquals(o._image);
            }

            public override int GetHashCode()
            {
                return _image.DeepHashCode();
            }
        }
    }
}