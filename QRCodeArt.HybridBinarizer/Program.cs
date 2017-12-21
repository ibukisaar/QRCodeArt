using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QRCodeArt;

namespace QRCodeArt.HybridBinarizerTest {
	class Program {
		const int RWeight = 19595;
		const int GWeight = 38470;
		const int BWeight = 7471;

		unsafe static void Main(string[] args) {
			var bitmap = new Bitmap(@"Z:\3.png");
			int w = bitmap.Width, h = bitmap.Height;
			var thresholds = HybridBinarizer.GetThreshold(bitmap);
			var output = new Bitmap(bitmap.Width, bitmap.Height);
			var data = bitmap.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
			var bitmapData = output.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
			var p0 = (uint*) data.Scan0;
			var p = (uint*) bitmapData.Scan0;
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					byte* bgr = (byte*) (p0 + y * w + x);
					int gray = (RWeight * bgr[2] + GWeight * bgr[1] + BWeight * bgr[0]) >> 16;
					uint g = thresholds[x, y];
					//p[y * w + x] = (g << 16) | (g << 8) | g;
					 p[y * w + x] = gray <= g ? 0 : 0xffffffu;
				}
			}
			output.UnlockBits(bitmapData);
			bitmap.UnlockBits(data);
			output.Save(@"Z:\out5.png");
		}
	}
}
