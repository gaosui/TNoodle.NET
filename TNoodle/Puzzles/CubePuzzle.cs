using System;
using System.Collections.Generic;
using TNoodle.Solvers;
using TNoodle.Utils;
using static TNoodle.Utils.Assertion;

namespace TNoodle.Puzzles
{
	public class CubePuzzle : Puzzle
	{
		public enum Face
		{
			R, U, F, L, D, B
		}

		static readonly string[] DIR_TO_STR = { null, "", "2", "'" };
		static readonly Dictionary<Face, string> faceRotationsByName = new Dictionary<Face, string>
		{
			[Face.R] = "x",
			[Face.U] = "y",
			[Face.F] = "z"
		};

		public class CubeMove
		{
			readonly CubePuzzle puzzle;

			public Face Face { get; }
			public int Direction { get; }
			public int InnerSlice { get; }
			public int OuterSlice { get; }

			public CubeMove(Face face, int dir, CubePuzzle p) : this(face, dir, 0, p)
			{
			}
			public CubeMove(Face face, int dir, int innerSlice, CubePuzzle p) : this(face, dir, innerSlice, 0, p)
			{
			}
			public CubeMove(Face face, int dir, int innerSlice, int outerSlice, CubePuzzle p)
			{
				puzzle = p;
				Face = face;
				Direction = dir;
				InnerSlice = innerSlice;
				OuterSlice = outerSlice;
				// We haven't come up with names for moves where outerSlice != 0
				Assert(outerSlice == 0);
			}

			public override string ToString()
			{
				var f = Face.ToString();
				string move;
				if (InnerSlice == 0)
				{
					move = f;
				}
				else if (InnerSlice == 1)
				{
					move = f + "w";
				}
				else if (InnerSlice == puzzle.Size - 1)
				{
					// Turning all the slices is a rotation
					if (!faceRotationsByName.ContainsKey(Face)) return null;
					move = faceRotationsByName[Face];
				}
				else
				{
					move = (InnerSlice + 1) + f + "w";
				}
				move += DIR_TO_STR[Direction];

				return move;
			}
		}

		static readonly int[] DEFAULT_LENGTHS = { 0, 0, 25, 25, 40, 60, 80, 100, 120, 140, 160, 180 };

		protected int Size { get; }

		protected CubeMove[][] GetRandomOrientationMoves(int thickness)
		{
			var randomUFaceMoves = new CubeMove[]
			{
				null,
				new CubeMove(Face.R, 1, thickness, this),
				new CubeMove(Face.R, 2, thickness, this),
				new CubeMove(Face.R, 3, thickness, this),
				new CubeMove(Face.F, 1, thickness, this),
				new CubeMove(Face.F, 3, thickness, this)
			};
			var randomFFaceMoves = new CubeMove[]
			{
				null,
				new CubeMove(Face.U, 1, thickness, this),
				new CubeMove(Face.U, 2, thickness, this),
				new CubeMove(Face.U, 3, thickness, this)
			};
			CubeMove[][] randomOrientationMoves = new CubeMove[randomUFaceMoves.Length * randomFFaceMoves.Length][];
			int i = 0;
			foreach (var randomUFaceMove in randomUFaceMoves)
			{
				foreach (var randomFFaceMove in randomFFaceMoves)
				{
					var moves = new List<CubeMove>();
					if (randomUFaceMove != null)
					{
						moves.Add(randomUFaceMove);
					}
					if (randomFFaceMove != null)
					{
						moves.Add(randomFFaceMove);
					}
					var movesArr = new CubeMove[moves.Count];
					moves.CopyTo(movesArr);
					randomOrientationMoves[i++] = movesArr;
				}
			}
			return randomOrientationMoves;
		}


		public CubePuzzle(int size)
		{
			Assert(size >= 0 && size < DEFAULT_LENGTHS.Length, "Invalid cube size");
			Size = size;
		}

		public override string GetLongName()
		{
			return Size + "x" + Size + "x" + Size;
		}

		public override string GetShortName()
		{
			return Size + "" + Size + "" + Size;
		}

		static void Swap(int[][][] image,
		int f1, int x1, int y1,
		int f2, int x2, int y2,
		int f3, int x3, int y3,
		int f4, int x4, int y4,
		int dir)
		{
			if (dir == 1)
			{
				int temp = image[f1][x1][y1];
				image[f1][x1][y1] = image[f2][x2][y2];
				image[f2][x2][y2] = image[f3][x3][y3];
				image[f3][x3][y3] = image[f4][x4][y4];
				image[f4][x4][y4] = temp;
			}
			else if (dir == 2)
			{
				int temp = image[f1][x1][y1];
				image[f1][x1][y1] = image[f3][x3][y3];
				image[f3][x3][y3] = temp;
				temp = image[f2][x2][y2];
				image[f2][x2][y2] = image[f4][x4][y4];
				image[f4][x4][y4] = temp;
			}
			else if (dir == 3)
			{
				int temp = image[f4][x4][y4];
				image[f4][x4][y4] = image[f3][x3][y3];
				image[f3][x3][y3] = image[f2][x2][y2];
				image[f2][x2][y2] = image[f1][x1][y1];
				image[f1][x1][y1] = temp;
			}
			else
			{
				Assert(false);
			}
		}

		static void Slice(Face face, int slice, int dir, int[][][] image)
		{
			var size = image[0].Length;
			Assert(slice >= 0 && slice < size);

			Face sface = face;
			int sslice = slice;
			int sdir = dir;

			if (face != Face.L && face != Face.D && face != Face.B)
			{
				sface = face.OppositeFace();
				sslice = size - 1 - slice;
				sdir = 4 - dir;
			}
			for (int j = 0; j < size; j++)
			{
				if (sface == Face.L)
				{
					Swap(image,
							(int)Face.U, j, sslice,
							(int)Face.B, size - 1 - j, size - 1 - sslice,
							(int)Face.D, j, sslice,
							(int)Face.F, j, sslice,
							sdir);
				}
				else if (sface == Face.D)
				{
					Swap(image,
							(int)Face.L, size - 1 - sslice, j,
							(int)Face.B, size - 1 - sslice, j,
							(int)Face.R, size - 1 - sslice, j,
							(int)Face.F, size - 1 - sslice, j,
							sdir);
				}
				else if (sface == Face.B)
				{
					Swap(image,
							(int)Face.U, sslice, j,
							(int)Face.R, j, size - 1 - sslice,
							(int)Face.D, size - 1 - sslice, size - 1 - j,
							(int)Face.L, size - 1 - j, sslice,
							sdir);
				}
				else
				{
					Assert(false);
				}
			}
			if (slice == 0 || slice == size - 1)
			{
				int f;
				if (slice == 0)
				{
					f = (int)face;
					sdir = 4 - dir;
				}
				else if (slice == size - 1)
				{
					f = (int)face.OppositeFace();
					sdir = dir;
				}
				else
				{
					Assert(false);
					return;
				}
				for (int j = 0; j < (size + 1) / 2; j++)
				{
					for (int k = 0; k < size / 2; k++)
					{
						Swap(image,
								f, j, k,
								f, k, size - 1 - j,
								f, size - 1 - j, size - 1 - k,
								f, size - 1 - k, j,
								sdir);
					}
				}
			}
		}

		public override PuzzleState GetSolvedState()
		{
			return new CubeState(this);
		}

		protected override int GetRandomMoveCount()
		{
			return DEFAULT_LENGTHS[Size];
		}

		int[][][] CloneImage(int[][][] image)
		{
			var imageCopy = ArrayExtension.New<int>(image.Length, image[0].Length, image[0][0].Length);
			image.DeepCopyTo(imageCopy);
			return imageCopy;
		}

		void SpinCube(int[][][] image, Face face, int dir)
		{
			for (int slice = 0; slice < Size; slice++)
			{
				Slice(face, slice, dir, image);
			}
		}

		int[][][] Normalize(int[][][] image)
		{
			image = CloneImage(image);

			int spins = 0;
			while (!IsNormalized(image))
			{
				Assert(spins < 2);
				var stickersByPiece = GetStickersByPiece(image);

				int goal = 0;
				goal |= 1 << (int)Face.B;
				goal |= 1 << (int)Face.L;
				goal |= 1 << (int)Face.D;
				int idx = -1;
				for (int i = 0; i < stickersByPiece.Length; i++)
				{
					int t = 0;
					for (int j = 0; j < stickersByPiece[i].Length; j++)
					{
						t |= 1 << stickersByPiece[i][j];
					}
					if (t == goal)
					{
						idx = i;
						break;
					}
				}
				Assert(idx >= 0);
				Face? f = null;
				int dir = 1;
				if (stickersByPiece[idx][0] == (int)Face.D)
				{
					if (idx < 4)
					{
						// on U
						f = Face.F;
						dir = 2;
					}
					else
					{
						// on D
						f = Face.U;
						switch (idx)
						{
							case 4:
								dir = 2; break;
							case 5:
								dir = 1; break;
							case 6:
								dir = 3; break;
							default:
								Assert(false);
								break;
						}
					}
				}
				else if (stickersByPiece[idx][1] == (int)Face.D)
				{
					switch (idx)
					{
						case 0:
						case 6:
							f = Face.F; break; // on R
						case 1:
						case 4:
							f = Face.L; break; // on F
						case 2:
						case 7:
							f = Face.R; break; // on B
						case 3:
						case 5:
							f = Face.B; break; // on L
						default:
							Assert(false);
							break;
					}
				}
				else
				{
					switch (idx)
					{
						case 2:
						case 4:
							f = Face.F; break; // on R
						case 0:
						case 5:
							f = Face.L; break; // on F
						case 3:
						case 6:
							f = Face.R; break; // on B
						case 1:
						case 7:
							f = Face.B; break; // on L
						default:
							Assert(false);
							break;
					}
				}
				SpinCube(image, (Face)f, dir);
				spins++;
			}

			return image;
		}

		bool IsNormalized(int[][][] image)
		{
			// A CubeState is normalized if the BLD piece is solved
			return image[(int)Face.B][Size - 1][Size - 1] == (int)Face.B &&
				   image[(int)Face.L][Size - 1][0] == (int)Face.L &&
				   image[(int)Face.D][Size - 1][0] == (int)Face.D;
		}

		protected static int[][] GetStickersByPiece(int[][][] img)
		{
			int s = img[0].Length - 1;
			return new int[][]
			{
				new int[] { img[(int)Face.U][s][s], img[(int)Face.R][0][0], img[(int)Face.F][0][s] },
				new int[] { img[(int)Face.U][s][0], img[(int)Face.F][0][0], img[(int)Face.L][0][s] },
				new int[] { img[(int)Face.U][0][s], img[(int)Face.B][0][0], img[(int)Face.R][0][s] },
				new int[] { img[(int)Face.U][0][0], img[(int)Face.L][0][0], img[(int)Face.B][0][s] },

				new int[] { img[(int)Face.D][0][s], img[(int)Face.F][s][s], img[(int)Face.R][s][0] },
				new int[] { img[(int)Face.D][0][0], img[(int)Face.L][s][s], img[(int)Face.F][s][0] },
				new int[] { img[(int)Face.D][s][s], img[(int)Face.R][s][s], img[(int)Face.B][s][0] },
				new int[] { img[(int)Face.D][s][0], img[(int)Face.B][s][s], img[(int)Face.L][s][0] }
			};
		}

		#region CubeState

		public class CubeState : PuzzleState
		{
			CubePuzzle puzzle;
			readonly int[][][] image;
		 CubeState normalizedState ;

			public CubeState(CubePuzzle cp) : base(cp)
			{
				puzzle = cp;
				image = ArrayExtension.New<int>(6, puzzle.Size, puzzle.Size);

				for (int face = 0; face < image.Length; face++)
				{
					for (int j = 0; j < puzzle.Size; j++)
					{
						for (int k = 0; k < puzzle.Size; k++)
						{
							image[face][ j][ k] = face;
						}
					}
				}
				normalizedState = this;
			}

			public CubeState(int[][][] image, CubePuzzle cp) : base(cp)
			{
				puzzle = cp;
				this.image = image;
			}

			public override bool IsNormalized()
			{
				return puzzle.IsNormalized(image);
			}

			public override PuzzleState GetNormalized()
			{
				if (normalizedState == null)
				{
					var normalizedImage = puzzle.Normalize(image);
					normalizedState = new CubeState(normalizedImage, puzzle);
				}
				return normalizedState;
			}

			public TwoByTwoState ToTwoByTwoState()
			{
				var state = new TwoByTwoState();

				var stickersByPiece = GetStickersByPiece(image);

				// Here's a clever color value assigning system that gives each piece
				// a unique id just by summing up the values of its stickers.
				//
				//            +----------+
				//            |*3*    *2*|
				//            |   U (0)  |
				//            |*1*    *0*|
				// +----------+----------+----------+----------+
				// | 3      1 | 1      0 | 0      2 | 2      3 |
				// |   L (1)  |   F (0)  |   R (0)  |   B (2)  |
				// | 7      5 | 5      4 | 4      6 | 6      7 |
				// +----------+----------+----------+----------+
				//            |*5*    *4*|
				//            |   D (4)  |
				//            |*7*    *6*|
				//            +----------+
				//

				int dColor = stickersByPiece[7][ 0];
				int bColor = stickersByPiece[7][ 1];
				int lColor = stickersByPiece[7][ 2];

				var uColor = (int)((Face)dColor).OppositeFace();
				var fColor = (int)((Face)bColor).OppositeFace();
				var rColor = (int)((Face)lColor).OppositeFace();

				int[] colorToVal = new int[8];
				colorToVal[uColor] = 0;
				colorToVal[fColor] = 0;
				colorToVal[rColor] = 0;
				colorToVal[lColor] = 1;
				colorToVal[bColor] = 2;
				colorToVal[dColor] = 4;

				int[] pieces = new int[7];
				for (int i = 0; i < pieces.Length; i++)
				{
					int[] stickers = stickersByPiece[i];
					int pieceVal = colorToVal[stickers[0]] + colorToVal[stickers[1]] + colorToVal[stickers[2]];

					int clockwiseTurnsToGetToPrimaryColor = 0;
					while (stickers[clockwiseTurnsToGetToPrimaryColor] != uColor && stickers[clockwiseTurnsToGetToPrimaryColor] != dColor)
					{
						clockwiseTurnsToGetToPrimaryColor++;
						Assert(clockwiseTurnsToGetToPrimaryColor < 3);
					}
					int piece = (clockwiseTurnsToGetToPrimaryColor << 3) + pieceVal;
					pieces[i] = piece;
				}

				state.Permutation = TwoByTwoSolver.PackPerm(pieces);
				state.Orientation = TwoByTwoSolver.PackOrient(pieces);
				return state;
			}

			public string ToFaceCube()
			{
				Assert(puzzle.Size == 3);
				string state = "";
				Face[] faces = { Face.U, Face.R, Face.F, Face.D, Face.L, Face.B };
				foreach (var f in faces)
				{
					int[][] faceArr = image[(int)f];
					for (int i = 0; i < faceArr.Length; i++)
					{
						for (int j = 0; j < faceArr[i].Length; j++)
						{
							state += ((Face)faceArr[i][ j]).ToString();
						}
					}
				}
				return state;
			}

			public override LinkedHashMap<string, PuzzleState> GetSuccessorsByName()
			{
				return GetSuccessorsWithinSlice(puzzle.Size - 1, true);
			}

			public override LinkedHashMap<string, PuzzleState> GetScrambleSuccessors()
			{
				return GetSuccessorsWithinSlice(puzzle.Size / 2 - 1, false);
			}

			public override LinkedHashMap<PuzzleState, string> GetCanonicalMovesByState()
			{
				return GetScrambleSuccessors().ReverseHashMap();
			}

			LinkedHashMap<string, PuzzleState> GetSuccessorsWithinSlice(int maxSlice, bool includeRedundant)
			{
				var successors = new LinkedHashMap<string, PuzzleState>();
				for (int innerSlice = 0; innerSlice <= maxSlice; innerSlice++)
				{
					foreach (Face face in Enum.GetValues(typeof(Face)))
					{
						bool halfOfEvenCube = puzzle.Size % 2 == 0 && (innerSlice == (puzzle.Size / 2) - 1);
						if (!includeRedundant && (int)face >= 3 && halfOfEvenCube)
						{
							// Skip turning the other halves of even sized cubes
							continue;
						}
						int outerSlice = 0;
						for (int dir = 1; dir <= 3; dir++)
						{
							var move = new CubeMove(face, dir, innerSlice, outerSlice, puzzle);
							var moveStr = move.ToString();
							if (moveStr == null)
							{
								// Skip unnamed rotations.
								continue;
							}

							var imageCopy =puzzle.CloneImage( image);
							for (int slice = outerSlice; slice <= innerSlice; slice++)
							{
								Slice(face, slice, dir, imageCopy);
							}
							successors[moveStr] = new CubeState(imageCopy, puzzle);
						}
					}
				}

				return successors;
			}

			public override bool Equals(object obj)
			{
				return image.DeepEquals(((CubeState)obj).image);
			}

			public override int GetHashCode()
			{
				return image.DeepHashCode();
			}
		}

		#endregion
	}
}

