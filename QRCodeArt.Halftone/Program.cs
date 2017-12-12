using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace QRCodeArt.Halftone {
	class Program {
		unsafe static void Main(string[] args) {
			int version = 5;
			int cellSize = 6;
			byte[] data1 = Encoding.UTF8.GetBytes("BAKA");
			var N = QRCode.GetN(version);
			var maxError = QRDataEncoder.GetMaxErrorAllowBytes(version, ECCLevel.L);
			var template = new Bitmap(@"Z:\2.png");
			var pixels = QRCodeMagician.GetImagePixel(version, template, cellSize, 140, 220, cellSize / 3);
			var qr = QRCodeMagician.ImageArt(QRDataMode.Alphanumeric, version, ECCLevel.L, MaskVersion.Version100, data1, pixels, 4);
			var bitmap = new Bitmap((N + 2) * cellSize, (N + 2) * cellSize);
			// var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
			var fixedBlackBrush = Brushes.Black;
			var fixedWhiteBrush = Brushes.White;
			var blackBrush = Brushes.Black;
			var whiteBrush = Brushes.White;
			var halftone = cellSize / 3;
			using (var g = Graphics.FromImage(bitmap)) {
				g.Clear(Color.White);
				g.DrawImage(new Bitmap(@"Z:\1.png"), new Rectangle(cellSize, cellSize, N * cellSize, N * cellSize));
				for (int x = 0; x < N; x++) {
					for (int y = 0; y < N; y++) {
						var drawX = x + 1;
						var drawY = y + 1;

						if (qr[x, y].Type == QRValueType.TimingPatterns) continue;


						//if (qr[x, y].Type == QRValueType.Fixed || qr[x, y].Type == QRValueType.TimingPatterns || qr[x, y].Type == QRValueType.Format || qr[x, y].Type == QRValueType.Version) {
						if (qr[x, y].Type == QRValueType.Fixed || qr[x, y].Type == QRValueType.TimingPatterns) {
							if (qr[x, y].Value) {
								g.FillRectangle(fixedBlackBrush, new Rectangle(drawX * cellSize, drawY * cellSize, cellSize, cellSize));
							} else {
								// g.FillRectangle(fixedWhiteBrush, new Rectangle(drawX * cellSize, drawY * cellSize, cellSize, cellSize));
							}
							//if (qr[x, y].Value) {
							//	g.FillRectangle(blackBrush, new Rectangle(drawX * cellSize + halftone, drawY * cellSize + halftone, halftone, halftone));
							//} else {
							//	g.FillRectangle(whiteBrush, new Rectangle(drawX * cellSize + halftone, drawY * cellSize + halftone, halftone, halftone));
							//}
						} else if (!pixels[x, y].HasFlag(ImagePixel.Stable) || qr[x, y].Value != pixels[x, y].HasFlag(ImagePixel.Black)) {
							if (qr[x, y].Value) {
								g.FillRectangle(blackBrush, new Rectangle(drawX * cellSize + halftone, drawY * cellSize + halftone, halftone, halftone));
							} else {
								g.FillRectangle(whiteBrush, new Rectangle(drawX * cellSize + halftone, drawY * cellSize + halftone, halftone, halftone));
							}
							//if (qr[x, y].Value) {
							//	g.FillRectangle(fixedBlackBrush, new Rectangle(drawX * cellSize, drawY * cellSize, cellSize, cellSize));
							//} else {
							//	g.FillRectangle(fixedWhiteBrush, new Rectangle(drawX * cellSize, drawY * cellSize, cellSize, cellSize));
							//}
						}
					}
				}
			}
			bitmap.Save(@"Z:\out.png");
		}
	}
}
