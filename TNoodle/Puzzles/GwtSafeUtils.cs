using System;
using System.Collections.Generic;
using System.Text;

namespace TNoodle.Puzzles
{
    public static class GwtSafeUtils
    {
        public static H choose<H>(Random r, IEnumerable<H> keySet)
        {
            H chosen = default(H);
            int count = 0;
            foreach (H element in keySet)
            {
                if (r.Next(++count) == 0)
                {
                    chosen = element;
                }
            }
            //azzert(count > 0);
            return chosen;
        }

        public static LinkedHashMap<B, A> reverseHashMap<A, B>(LinkedHashMap<A, B> map)
        {
            LinkedHashMap<B, A> reverseMap = new LinkedHashMap<B, A>();
            foreach (A a in map.Keys)
            {
                B b = map[a];
                reverseMap.Add(b, a);
            }
            return reverseMap;
        }

        public static string join<H>(List<H> arr, string separator)
        {
            if (separator == null)
            {
                separator = ",";
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < arr.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(separator);
                }
                sb.Append(arr[i].ToString());
            }
            return sb.ToString();
        }

        public static void deepCopy(int[][] src, int[][] dest)
        {
            for (int i = 0; i < src.Length; i++)
            {
                Array.Copy(src[i], 0, dest[i], 0, src[i].Length);
            }
        }

        public static void deepCopy(int[][][] src, int[][][] dest)
        {
            for (int i = 0; i < src.Length; i++)
            {
                deepCopy(src[i], dest[i]);
            }
        }

    }


}