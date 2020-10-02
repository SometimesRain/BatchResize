using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace BatchResize
{
	public static class Resize
	{
		public static void File(Func<SizeF, float> proc, string path)
		{
			//Open image and calculate factor
			Bitmap source = new Bitmap(path);
			float factor = proc(source.Size);

			//This image is already of the target size
			if (factor == 1)
				return;

			//Use resize constructor to create a copy
			using (Bitmap output = new Bitmap(source, (new SizeF(source.Size) * factor).ToSize()))
			{
				//Copy orientation metadata
				const int id = 0x112;
				if (source.PropertyIdList.Contains(id))
					output.SetPropertyItem(source.GetPropertyItem(id));
				source.Dispose();

				//Store size and overwrite file
				long size = new FileInfo(path).Length;
				output.Save(path, ImageFormat.Jpeg);

				//Compare sizes
				FileInfo info = new FileInfo(path);
				Console.WriteLine($"{info.Name}: {Format.Bytes(size)} -> {Format.Bytes(info.Length)}");
			}
		}

		public static void Batch(Func<SizeF, float> proc, string[] paths)
		{
			//========================== Shared state ==========================
			int state = 0;

			//========================= Create threads =========================
			Thread[] threads = new Thread[Environment.ProcessorCount];
			for (int i = 0; i < threads.Length; i++)
			{
				threads[i] = new Thread(() =>
				{
					int local = state;
					while (local < paths.Length)
					{
						//Store current state in value
						int value = Interlocked.CompareExchange(ref state, local + 1, local);

						//Local state is the current state => process item
						if (local == value)
							File(proc, paths[local]);

						//Update local state
						local = value;
					}
				});
				threads[i].IsBackground = true;
				threads[i].Start();
			}

			//======================= Wait for completion ======================
			for (int i = 0; i < threads.Length; i++)
				threads[i].Join();
		}
	}
}
