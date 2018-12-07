using System;

namespace TextParser
{
	class Vector
	{
		

		public float X { get; set; }
		public float Y { get; set; }

		public Vector(float x, float y)
		{
			X = x;
			Y = y;
		}
		
		public string Get()
		{
			return string.Format("({0};{1})",X.ToString(),Y.ToString());
		}
		public static Vector operator -(Vector a, Vector b)
		{
			return new Vector(a.X - b.X, a.Y - b.Y);
		}
		public static Vector operator +(Vector a, Vector b)
		{
			return new Vector(a.X + b.X, a.Y + b.Y);
		}
		public double Length()
		{
			return Math.Sqrt(X*X + Y * Y);
		}
	}
}
