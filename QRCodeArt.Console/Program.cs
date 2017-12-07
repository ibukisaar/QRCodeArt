using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QRCodeArt;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;

namespace QRCodeArt.Console {
	class Program {
		static void Save(int weight, Bitmap bitmap, QRCode qr, Color color, bool writeWhite = true) {
			var white = Brushes.White;
			var black = new SolidBrush(color);
			using (Graphics g = Graphics.FromImage(bitmap)) {
				for (int x = 0; x < qr.N; x++) {
					for (int y = 0; y < qr.N; y++) {
						if (!qr[x, y] && !writeWhite) continue;
						var brush = qr[x, y] ? black : white;
						g.FillRectangle(brush, new Rectangle(x * weight, y * weight, weight, weight));
					}
				}
			}
		}

		static void Save(string file, QRCode qr, QRCode qr2, int weight) {
			const int C = 150;
			var bitmap = new Bitmap(qr.N * weight, qr.N * weight, PixelFormat.Format32bppRgb);
			Save(weight, bitmap, qr, qr2 != null ? Color.FromArgb(C, C, C) : Color.Black);
			if (qr2 != null) {
				Save(weight, bitmap, qr2, Color.Black, false);
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
			var bitmap = new Bitmap(qr.N * weight, qr.N * weight, PixelFormat.Format32bppArgb);
			var bitmapData = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			for (int x = 0; x < qr.N; x++) {
				for (int y = 0; y < qr.N; y++) {
					uint color;
					if (qr[x, y] && qr2[x, y]) color = 0xFF000000;
					else if (!qr[x, y] && !qr2[x, y]) color = 0xFFFFFFFF;
					else color = 0;
					DrawRect((uint*) bitmapData.Scan0, bitmap.Width, x * weight, y * weight, weight, color);
				}
			}

			bitmap.UnlockBits(bitmapData);
			bitmap.Save(file);
		}

		static void Main(string[] args) {
			//var g = new GF.XPolynom(GF.FromExponent(0), GF.FromExponent(0));
			//var eccNum = 4;
			//for (int i = 1; i < eccNum; i++) {
			//	g *= new GF.XPolynom(GF.FromExponent(i), GF.FromExponent(0));
			//}

			//var msg = new GF.XPolynom(GF.FromPolynom(0x56), GF.FromPolynom(0x34), GF.FromPolynom(0x12));
			//msg = msg.MulXPow(eccNum);
			//var r = msg % g;

			//var r2 = GF.PolynomMod(0b111101011001, 0b10100110111);

			//BitList bits = new BitList(10);
			//bits.Write(0, 0b11100001, 8);
			byte[] data1 = Encoding.UTF8.GetBytes("SAAR");
			byte[] data2 = Encoding.UTF8.GetBytes("KAREN");
			var (qr1, qr2, swap) = QRCodeMagician.CreateObfuscatedQRCode(data1, data2);
			if (qr1 == null) { System.Console.WriteLine("失败"); return; }
			Save(@"Z:\qr1.png", qr1, null, 5);
			Save(@"Z:\qr2.png", qr2, null, 5);
			Save(@"Z:\qr12.png", qr1, qr2, 5);
			Save(@"Z:\qr21.png", qr2, qr1, 5);
			SaveObfuscated(@"Z:\qr.png", qr1, qr2, 5);
			System.Console.WriteLine("成功");
		}
	}
}
