using System;
using TNoodle.Puzzles;

namespace TNoodle.Utils
{
	public static class ArrayExtension
	{
		public static T[][] New<T>(int l1, int l2)
		{
			T[][] res = new T[l1][];
			for (int i = 0; i < l1; i++)
			{
				res[i] = new T[l2];
			}
			return res;
		}

		public static T[][][] New<T>(int l1, int l2, int l3)
		{
			T[][][] res = new T[l1][][];
			for (int i = 0; i < l1; i++)
			{
				res[i] = new T[l2][];
				for (int j = 0; j < l2; j++)
				{
					res[i][j] = new T[l3];
				}
			}
			return res;
		}

		public static void DeepCopyTo<T>(this T[][][] src, T[][][] dest)
		{
			for (int i = 0; i < src.Length; i++)
			{
				src[i].DeepCopyTo(dest[i]);
			}
		}

		public static void DeepCopyTo<T>(this T[][] src, T[][] dest)
		{
			for (int i = 0; i < src.Length; i++)
			{
				Array.Copy(src[i], 0, dest[i], 0, src[i].Length);
			}
		}

		public static bool DeepEquals(this int[][][] src, int[][][] dest)
		{
			for (int i = 0; i < src.Length; i++)
			{
				for (int j = 0; j < src[i].Length; j++)
				{
					for (int k = 0; k < src[i][j].Length; k++)
					{
						if (src[i][j][k] != dest[i][j][k]) return false;
					}
				}
			}
			return true;
		}

		public static int DeepHashCode(this int[][][] src)
		{
			int result = 1;
			for (int i = 0; i < src.Length; i++)
			{
				for (int j = 0; j < src[i].Length; j++)
				{
					for (int k = 0; k < src[i][j].Length; k++)
					{
						result = 31 * result + src[i][j][k];
					}
				}
			}
			return result;
		}

		public static LinkedHashMap<B, A> ReverseHashMap<A, B>(this LinkedHashMap<A, B> map)
		{
			var reverseMap = new LinkedHashMap<B, A>();
			foreach (var pair in map)
			{
				reverseMap.Add(pair.Value, pair.Key);
			}
			return reverseMap;
		}
	}
}

