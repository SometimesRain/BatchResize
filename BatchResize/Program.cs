using System;
using System.Drawing;
using System.Threading.Tasks;

namespace BatchResize
{
    class Program
    {
		static void Main(string[] args)
		{
			//Resize files using custom resizing behavior
			Resize.Batch(ResizeProc, args);

			//Wait for enter (autoclose after 3.5s)
			Task.WaitAny(Task.Run(() => Console.ReadLine()), Task.Delay(3500));
		}

		//Outputs the factor by which the width and the height are to be multiplied
		static float ResizeProc(SizeF size)
		{
			//Limit width and height to a maximum of 1600px (maintain aspect ratio)
			return Math.Min(Math.Min(1600 / size.Width, 1600 / size.Height), 1);
		}
	}
	public static class Format
    {
		public static string Bytes(long value)
		{
			//Gauge the magnitude (1 iter = divide by 1024)
			int i = 0;
			while ((value >> i) > 1024)
				i += 10;

			//Set the suffix and format (2 decimal places)
			string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
			return $"{(float)value / (1 << i):0.##} {suffixes[i / 10]}";
		}
	}
}
