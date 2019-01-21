using QRCodeArt;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt.Console {
	class Program {
		static void Save(int padding, int weight, Bitmap bitmap, QRCode qr, Color color, bool writeWhite = true) {
			var white = Brushes.White;
			var black = new SolidBrush(color);
			using (Graphics g = Graphics.FromImage(bitmap)) {
				if (writeWhite) g.Clear(Color.White);
				for (int x = 0; x < qr.N; x++) {
					for (int y = 0; y < qr.N; y++) {
						if (!qr[x, y].Value && !writeWhite) continue;
						var brush = qr[x, y].Value ? black : white;
						g.FillRectangle(brush, new Rectangle(padding + x * weight, padding + y * weight, weight, weight));
					}
				}
			}
		}

		static void Save(string file, QRCode qr, QRCode qr2, int weight) {
			const int padding = 20;
			const int C = 128;
			var bitmap = new Bitmap(padding * 2 + qr.N * weight, padding * 2 + qr.N * weight, PixelFormat.Format32bppRgb);
			if (qr2 != null) {
				Save(padding, weight, bitmap, qr2, qr2 != null ? Color.FromArgb(C, C, C) : Color.Black);
				Save(padding, weight, bitmap, qr, Color.Black, false);
			} else {
				Save(padding, weight, bitmap, qr, Color.Black);
			}
			bitmap.Save(file);
		}

		unsafe static void DrawRect(uint* p, int w, int x, int y, int weight, uint color) {
			for (int i = 0; i < weight; i++) {
				for (int j = 0; j < weight; j++) {
					p[w * (y + j) + x + i] = color;
				}
			}
		}

		unsafe static void SaveObfuscated(string file, QRCode qr, QRCode qr2, int weight) {
			const int padding = 30;
			var bitmap = new Bitmap(padding * 2 + qr.N * weight, padding * 2 + qr.N * weight, PixelFormat.Format32bppArgb);
			using (var g = Graphics.FromImage(bitmap)) g.Clear(Color.White);
			var bitmapData = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			for (int x = 0; x < qr.N; x++) {
				for (int y = 0; y < qr.N; y++) {
					uint color;
					if (qr[x, y].Value && qr2[x, y].Value) color = 0xFF000000;
					else if (!qr[x, y].Value && !qr2[x, y].Value) color = 0xFFFFFFFF;
					else color = 0;
					DrawRect((uint*)bitmapData.Scan0, bitmap.Width, padding + x * weight, padding + y * weight, weight, color);
				}
			}

			bitmap.UnlockBits(bitmapData);
			bitmap.Save(file);
		}

		static void Main(string[] args) {
			var data1 = Encoding.UTF8.GetBytes("0123456789038912381290");
			var data2 = Encoding.UTF8.GetBytes("这是一条测试文本");
			var (qr1, qr2) = QRCodeMagician.CreateObfuscatedQRCode(data1, data2);

			Save(@"Z:\1.png", qr1, null, 6);
			Save(@"Z:\2.png", qr2, null, 6);
			Save(@"Z:\1+2.png", qr1, qr2, 6);
		}

		private static void NewMethod() {
			//var g = new GF.XPolynom(GF.FromExponent(0), GF.FromExponent(0));
			//var eccNum = 2;
			//for (int i = 1; i < eccNum; i++) {
			//	g *= new GF.XPolynom(GF.FromExponent(i), GF.FromExponent(0));
			//}

			//var r = new GF.XPolynom[8 * eccNum];
			//for (int i = 0; i < 8 * eccNum; i++) {
			//	var msg = new GF.XPolynom(eccNum * 2 + 1);
			//	// msg[eccNum * 2] = GF.FromPolynom(0x55);
			//	msg[eccNum * 2 - 1 - i / 8] = GF.FromPolynom(1 << (7 - i % 8));

			//	r[i] = msg % g;
			//	System.Console.WriteLine(Convert.ToString(r[i][0].Polynom, 2).PadLeft(8, '0') + "," + Convert.ToString(r[i][1].Polynom, 2).PadLeft(8, '0'));
			//}
			//System.Console.WriteLine();
			//{
			//	var msg = new GF.XPolynom(eccNum * 2 + 1);
			//	// msg[eccNum * 2] = GF.FromPolynom(0x55);
			//	// msg[eccNum * 2 - 1] = GF.FromPolynom(0b11000000);
			//	//var r0 = msg % g;
			//	//System.Console.WriteLine(Convert.ToString(r0[0].Polynom, 2).PadLeft(8, '0') + "," + Convert.ToString(r0[1].Polynom, 2).PadLeft(8, '0'));

			//	msg[eccNum * 2 - 1] = GF.FromPolynom(0b10000000);
			//	msg[eccNum * 2 - 2] = GF.FromPolynom(0b00000001);
			//	var r0 = msg % g;
			//	System.Console.WriteLine(Convert.ToString(r0[0].Polynom, 2).PadLeft(8, '0') + "," + Convert.ToString(r0[1].Polynom, 2).PadLeft(8, '0'));
			//}




			//byte[] data1 = Encoding.UTF8.GetBytes("17689480023");
			//byte[] data2 = Encoding.UTF8.GetBytes("15059723108");
			//var (qr1, qr2, swap) = QRCodeMagician.CreateObfuscatedQRCode(data1, data2);
			//if (qr1 == null) { System.Console.WriteLine("失败"); return; }
			//Save(@"Z:\qr1.png", qr1, null, 5);
			//Save(@"Z:\qr2.png", qr2, null, 5);
			//Save(@"Z:\qr12.png", qr1, qr2, 5);
			//Save(@"Z:\qr21.png", qr2, qr1, 5);
			//SaveObfuscated(@"Z:\qr.png", qr1, qr2, 5);
			//System.Console.WriteLine("成功");
			//var qr = new QRCode(data1);
			//Save(@"Z:\qr.png", qr, null, 5);

			//var r1 = QRDataEncoder.CalculateECCWords(new byte[] { 1,3,2 }, 2);
			//var r2 = QRDataEncoder.CalculateECCWords(new byte[] { 2 }, 2);
			//var r3 = QRDataEncoder.CalculateECCWords(new byte[] { 1, 2 }, 4);

			// System.IO.File.WriteAllBytes(@"D:\MyDocuments\Desktop\1.bin", new byte[] { 1 });

			//const int N = 5;
			//const int M = 5;
			//var left = new byte[N][];
			//var right = new byte[N][];
			//for (int i = 0; i < N; i++) {
			//	left[i] = new byte[N];
			//	left[i][i] = 1;
			//	right[i] = new byte[M];
			//}
			//Random random = new Random();
			//for (int i = 0; i < N; i++) {
			//	for (int j = 0; j < M; j++) {
			//		right[i][j] = (byte) (random.Next(10) < 2 ? 1 : 0);
			//	}
			//}

			//Print(left, right);
			//System.Console.WriteLine();
			//List<int> indexes;
			//(indexes, right) = BitBlock.GaussianElimination(left, right);
			//Print(left, right);

			//for (int i = 0; i < indexes.Count; i++) {
			//	System.Console.Write(indexes[i] + " ");
			//}
			//System.Console.WriteLine();

			int version = 5;
			byte[] data1 = Encoding.UTF8.GetBytes("0.0");
			var N = QRInfo.GetN(version);
			int padding = 6;
			// byte[] data1 = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
			var pixels = new ImagePixel[N, N];
			for (int x = 0; x < pixels.GetLength(0); x++) {
				for (int y = 0; y < pixels.GetLength(1); y++) {
					//if (x > 15) {
					//	pixels[x, y] = ImagePixel.White;
					//} else if (x <= 4) {
					//	pixels[x, y] = ImagePixel.White;
					//} else {
					//	pixels[x, y] = ImagePixel.Any;
					//}
					//if (x >= 11 || y <= 8) {
					//	pixels[x, y] = ImagePixel.White;
					//} else {
					//	pixels[x, y] = ImagePixel.Any;
					//}

					//if (y >= padding && y < N - padding && x >= padding && x < N - padding) {
					//	pixels[x, y] = y % 2 == 0 ? ImagePixel.White : ImagePixel.Black;
					//} else {
					//	pixels[x, y] = ImagePixel.Any | ImagePixel.White;
					//}
				}
			}
			pixels[0, 9] = ImagePixel.Black;
			pixels[0, 10] = ImagePixel.Black;
			pixels[0, 11] = ImagePixel.Black;
			pixels[0, 12] = ImagePixel.Black;
			pixels[0, 13] = ImagePixel.Black;
			pixels[0, 14] = ImagePixel.Black;
			pixels[0, 15] = ImagePixel.Black;
			pixels[0, 16] = ImagePixel.Black;
			pixels[1, 9] = ImagePixel.Black;
			pixels[1, 10] = ImagePixel.Black;
			pixels[1, 11] = ImagePixel.Black;
			pixels[1, 12] = ImagePixel.Black;
			pixels[1, 13] = ImagePixel.Black;
			pixels[1, 14] = ImagePixel.Black;
			pixels[1, 15] = ImagePixel.Black;
			pixels[1, 16] = ImagePixel.Black;
			pixels[2, 9] = ImagePixel.Black;
			pixels[2, 10] = ImagePixel.Black;
			pixels[2, 11] = ImagePixel.Black;
			pixels[2, 12] = ImagePixel.Black;
			pixels[2, 13] = ImagePixel.Black;
			pixels[2, 14] = ImagePixel.Black;
			pixels[2, 15] = ImagePixel.Black;
			pixels[2, 16] = ImagePixel.Black;
			pixels[3, 9] = ImagePixel.Black;
			pixels[3, 10] = ImagePixel.Black;
			pixels[3, 11] = ImagePixel.Black;
			pixels[3, 12] = ImagePixel.Black;
			pixels[3, 13] = ImagePixel.Black;
			pixels[3, 14] = ImagePixel.Black;
			pixels[3, 15] = ImagePixel.Black;
			pixels[3, 16] = ImagePixel.Black;

			pixels[9, 0] = ImagePixel.Any;
			pixels[10, 0] = ImagePixel.Any;
			pixels[11, 0] = ImagePixel.Any;
			pixels[12, 0] = ImagePixel.Any;
			pixels[9, 1] = ImagePixel.Any;
			pixels[10, 1] = ImagePixel.Any;
			pixels[11, 1] = ImagePixel.Any;
			pixels[12, 1] = ImagePixel.Any;
			pixels[9, 2] = ImagePixel.Any;
			pixels[10, 2] = ImagePixel.Any;
			pixels[11, 2] = ImagePixel.Any;
			pixels[12, 2] = ImagePixel.Any;
			pixels[9, 3] = ImagePixel.Any;
			pixels[10, 3] = ImagePixel.Any;
			pixels[11, 3] = ImagePixel.Any;
			pixels[12, 3] = ImagePixel.Any;
			pixels[13, 3] = ImagePixel.Any;
			pixels[14, 3] = ImagePixel.Any;
			pixels[15, 3] = ImagePixel.Any;
			//pixels[16, 3] = ImagePixel.Any;
			//pixels[17, 3] = ImagePixel.Any;
			//pixels[18, 3] = ImagePixel.Any;
			//pixels[19, 3] = ImagePixel.Any;
			//pixels[20, 3] = ImagePixel.Any;
			//pixels[21, 3] = ImagePixel.Any;

			int cellSize = 5;
			//pixels = QRCodeMagician.GetImagePixel(version, new Bitmap(@"Z:\1.png"), cellSize);
			//var qr = QRCodeMagician.ImageArt(DataMode.Byte, version, ECCLevel.L, MaskVersion.Version100, data1, pixels, QRInfo.GetMaxErrorAllowBytes(version, ECCLevel.L) / 2);
			//Save(@"Z:\art.png", qr, null, cellSize);

			var qr2 = QRCodeMagician.ImageArt(DataMode.Byte, version, ECCLevel.L, MaskVersion.Version100, data1, pixels);
			Save(@"D:\MyDocuments\Desktop\art2.png", qr2, null, cellSize);

			//var qr = new QRCode(version, DataMode.Byte, data1, ECCLevel.Q);
			//Save(@"D:\MyDocuments\Desktop\art2.png", qr, null, cellSize);

			//var bits = new BitSet(1000);
			//bits[3] = true;
			//var r1 = GF.XPolynom.RSEncode(bits.ByteArray, 0, bits.ByteArray.Length, 100);
			//bits[3] = false;
			//bits[300] = true;
			//var r2 = GF.XPolynom.RSEncode(bits.ByteArray, 0, bits.ByteArray.Length, 100);
			//bits[3] = true;
			//var r3 = GF.XPolynom.RSEncode(bits.ByteArray, 0, bits.ByteArray.Length, 100);
		}

		static void Print(byte[][] left, byte[][] right, List<int> indexes = null) {
			for (int i = 0; i < left.Length; i++) {
				for (int j = 0; j < left[0].Length; j++) {
					System.Console.Write((left[i][j] == 1 ? "1 " : "  "));
				}
				System.Console.Write(" | ");
				if (indexes == null) {
					for (int j = 0; j < right[0].Length; j++) {
						System.Console.Write((right[i][j] == 1 ? "1 " : "  "));
					}
				} else {
					for (int j = 0; j < indexes.Count; j++) {
						System.Console.Write((right[i][indexes[j]] == 1 ? "1 " : "  "));
					}
				}
				System.Console.WriteLine();
			}
		}
	}
}
