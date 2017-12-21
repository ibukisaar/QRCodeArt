using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public static class HybridBinarizer {
		const int BlockSize = 8;
		const int RWeight = 19595;
		const int GWeight = 38470;
		const int BWeight = 7471;
		const int MinDynamicRange = 24;

		unsafe public static byte[,] GetThreshold(Bitmap bitmap, int BlockSize = 8) {
			int width = bitmap.Width;
			int height = bitmap.Height;
			var bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
			var bgr = (uint*) bitmapData.Scan0;

			int subWidth = (width + (BlockSize - 1)) / BlockSize;
			int subHeight = (height + (BlockSize - 1)) / BlockSize;
			byte[,] grayPoints = new byte[subWidth, subHeight];
			for (int y = 0; y < subHeight; y++) {
				int yOffset = Math.Min(y * BlockSize, height - BlockSize);
				for (int x = 0; x < subWidth; x++) {
					int xOffset = Math.Min(x * BlockSize, width - BlockSize);
					int sum = 0;
					int min = 255, max = 0;
					for (int y0 = 0, offset = yOffset * width + xOffset; y0 < BlockSize; y0++, offset += width) {
						for (int x0 = 0; x0 < BlockSize; x0++) {
							byte B = ((byte*) (bgr + offset + x0))[0];
							byte G = ((byte*) (bgr + offset + x0))[1];
							byte R = ((byte*) (bgr + offset + x0))[2];
							int gray = (R * RWeight + G * GWeight + B * BWeight) >> 16;
							sum += gray;
							if (gray < min) min = gray;
							if (gray > max) max = gray;
						}
					}

					int avg = sum / (BlockSize * BlockSize);
					if (max - min <= MinDynamicRange) {
						avg = min >> 1;
						if (y > 0 && x > 0) {
							int neighbor = ((grayPoints[x - 1, y] << 1) + grayPoints[x - 1, y - 1] + grayPoints[x, y - 1]) >> 2;
							if (min < neighbor) avg = neighbor;
						}
					}
					grayPoints[x, y] = (byte) avg;
				}
			}
			bitmap.UnlockBits(bitmapData);

			var thresholds = new byte[width, height];
			for (int y = 0; y < subHeight; y++) {
				int yOffset = Math.Min(y * BlockSize, height - BlockSize);
				for (int x = 0; x < subWidth; x++) {
					int xOffset = Math.Min(x * BlockSize, width - BlockSize);
					int xCenter = Clamp(x, 2, subWidth - 3);
					int yCenter = Clamp(y, 2, subHeight - 3);
					int sum = 0;
					for (int y0 = -2; y0 <= 2; y0++) {
						for (int x0 = -2; x0 <= 2; x0++) {
							sum += grayPoints[xCenter + x0, yCenter + y0];
						}
					}
					int avg = sum / 25;
					for (int y0 = 0; y0 < BlockSize; y0++) {
						for (int x0 = 0; x0 < BlockSize; x0++) {
							thresholds[xOffset + x0, yOffset + y0] = (byte) avg;
						}
					}
				}
			}
			return thresholds;
		}

		static int Clamp(int value, int min, int max) {
			return value < min ? min : value > max ? max : value;
		}

		unsafe public static Bitmap BinarizerBitmap(Bitmap bitmap, int BlockSize = 8) {
			int w = bitmap.Width, h = bitmap.Height;
			var thresholds = GetThreshold(bitmap, BlockSize);
			var output = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
			var data = bitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			var outData = output.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			var p0 = (uint*) data.Scan0;
			var p = (uint*) outData.Scan0;
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					byte* bgr = (byte*) (p0 + y * w + x);
					int gray = (RWeight * bgr[2] + GWeight * bgr[1] + BWeight * bgr[0]) >> 16;
					uint g = thresholds[x, y];
					p[y * w + x] = (gray <= g ? 0 : 0xffffffu) | ((uint) bgr[3] << 24);
				}
			}
			output.UnlockBits(outData);
			bitmap.UnlockBits(data);
			return output;
		}

		unsafe public static Bitmap GetRGB(Bitmap bitmap) {
			int w = bitmap.Width, h = bitmap.Height;
			var output = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppRgb);
			var data = bitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			var outData = output.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
			var p0 = (uint*) data.Scan0;
			var p = (uint*) outData.Scan0;
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					uint alpha = p0[y * w + x] >> 24;
					p[y * w + x] = p0[y * w + x];
				}
			}
			output.UnlockBits(outData);
			bitmap.UnlockBits(data);
			return output;
		}
	}
}
