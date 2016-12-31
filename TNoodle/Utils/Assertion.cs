using System;
namespace TNoodle.Utils
{
	public static class Assertion
	{
		public static void Assert(bool expr)
		{
			if (!expr)
			{
				throw new Exception();
			}
		}

		public static void Assert(bool expr, string message)
		{
			if (!expr)
			{
				throw new Exception(message);
			}
		}

		public static void Assert(bool expr,string message, Exception t)
		{
			if (!expr)
			{
				throw new Exception(message, t);
			}
		}
	}
}
