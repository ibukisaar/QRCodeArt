using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QRCodeArt;

namespace QRCodeArt.WinForm {
	public partial class Form1 : Form {
		private ECCLevel eccLevel = ECCLevel.L;
		private DataMode? dataMode = null;

		public Form1() {
			InitializeComponent();
		}

		private void btnOpen_Click(object sender, EventArgs e) {
			openFileDialog1.Filter = "图片格式|*.jpg;*.png;*.gif;*.jpeg;*.bmp|所有文件|*";
			var result = openFileDialog1.ShowDialog();
			if (result == DialogResult.OK) {
				try {
					picTemplate.Image = new Bitmap(openFileDialog1.FileName);
				} catch {
					MessageBox.Show("错误的图像文件");
				}
			}
		}

		private void Form1_Load(object sender, EventArgs e) {
			cmbEccLevel.SelectedIndex = 0;
			cmbMask.SelectedIndex = 0;
			cmbMode.SelectedIndex = 0;
		}

		private void numVersion_ValueChanged(object sender, EventArgs e) {

		}

		private void cmbMask_SelectedIndexChanged(object sender, EventArgs e) {

		}

		private void cmbEccLevel_SelectedIndexChanged(object sender, EventArgs e) {
			switch (cmbEccLevel.SelectedIndex) {
				case 0: eccLevel = ECCLevel.L; break;
				case 1: eccLevel = ECCLevel.M; break;
				case 2: eccLevel = ECCLevel.Q; break;
				case 3: eccLevel = ECCLevel.H; break;
			}
		}

		private void cmbMode_SelectedIndexChanged(object sender, EventArgs e) {
			switch (cmbMode.SelectedIndex) {
				case 0: dataMode = null; break;
				case 1: dataMode = DataMode.Numeric; break;
				case 2: dataMode = DataMode.Alphanumeric; break;
				case 3: dataMode = DataMode.Byte; break;
			}
		}

		private void btnCreateQR_Click(object sender, EventArgs e) {
			byte[] data = Encoding.UTF8.GetBytes(txtData.Text);
			int version = (int) numVersion.Value;
			MaskVersion maskVersion = (MaskVersion) cmbMask.SelectedIndex;
			int cellSize = (int) numCellSize.Value;
			int halftone = (int) numHalftone.Value;
			int maxError = (int) numMaxError.Value;
			int blackThreshold = (int) numBlackThreshold.Value;
			int whiteThreshold = (int) numWhiteThreshold.Value;
			int deviation = (int) numDeviation.Value;
			dataMode = dataMode ?? DataEncoder.GuessMode(data);
			if (version == 0) version = DataEncoder.GuessVersion(data.Length, eccLevel, dataMode.Value);
			var template = picTemplate.Image as Bitmap;

			var pixels = QRCodeMagician.GetImagePixel(version, template, cellSize, halftone, deviation);
			var qr = QRCodeMagician.ImageArt(dataMode.Value, version, eccLevel, maskVersion, data, pixels, maxError);
			int N = qr.N;

			var fixedBlackBrush = Brushes.Black;
			var fixedWhiteBrush = Brushes.White;
			var blackBrush = Brushes.Black;
			var whiteBrush = Brushes.White;
			int margin = halftone;
			if (halftone <= 0 || halftone >= cellSize) {
				halftone = cellSize;
				margin = 0;
			}

			var bitmap = new Bitmap((N + 2) * cellSize, (N + 2) * cellSize);
			using (var g = Graphics.FromImage(bitmap)) {
				g.Clear(Color.White);
				g.DrawImage(template, new Rectangle(cellSize, cellSize, N * cellSize, N * cellSize));
				for (int x = 0; x < N; x++) {
					for (int y = 0; y < N; y++) {
						var drawX = x + 1;
						var drawY = y + 1;

						if (margin == 0) {
							if (qr[x, y].Value) {
								g.FillRectangle(blackBrush, new Rectangle(drawX * cellSize, drawY * cellSize, halftone, halftone));
							} else {
								g.FillRectangle(whiteBrush, new Rectangle(drawX * cellSize, drawY * cellSize, halftone, halftone));
							}
							continue;
						}

						if (qr[x, y].Type == QRValueType.TimingPatterns) continue;

						//if (qr[x, y].Type == QRValueType.Fixed || qr[x, y].Type == QRValueType.TimingPatterns || qr[x, y].Type == QRValueType.Format || qr[x, y].Type == QRValueType.Version) {
						if (qr[x, y].Type == QRValueType.Fixed || qr[x, y].Type == QRValueType.TimingPatterns) {
							if (qr[x, y].Value) {
								g.FillRectangle(fixedBlackBrush, new Rectangle(drawX * cellSize, drawY * cellSize, cellSize, cellSize));
							} else {
								g.FillRectangle(fixedWhiteBrush, new Rectangle(drawX * cellSize, drawY * cellSize, cellSize, cellSize));
							}
						} else if (!pixels[x, y].HasFlag(ImagePixel.Stable) || qr[x, y].Value != pixels[x, y].HasFlag(ImagePixel.Black)) {
							var brush = qr[x, y].Value ? blackBrush : whiteBrush;
							g.FillRectangle(brush, new Rectangle(drawX * cellSize + halftone, drawY * cellSize + halftone, halftone, halftone));
						}
					}
				}
			}

			var bitmap2 = new Bitmap((N + 2) * cellSize, (N + 2) * cellSize);
			using (var g = Graphics.FromImage(bitmap2)) {
				g.Clear(Color.White);
				g.DrawImage(template, new Rectangle(cellSize, cellSize, N * cellSize, N * cellSize));
				for (int x = 0; x < N; x++) {
					for (int y = 0; y < N; y++) {
						var drawX = x + 1;
						var drawY = y + 1;

						if (margin == 0) {
							if (qr[x, y].Value) {
								g.FillRectangle(blackBrush, new Rectangle(drawX * cellSize, drawY * cellSize, halftone, halftone));
							} else {
								g.FillRectangle(whiteBrush, new Rectangle(drawX * cellSize, drawY * cellSize, halftone, halftone));
							}
							continue;
						}

						if (qr[x, y].Type == QRValueType.TimingPatterns) continue;

						//if (qr[x, y].Type == QRValueType.Fixed || qr[x, y].Type == QRValueType.TimingPatterns || qr[x, y].Type == QRValueType.Format || qr[x, y].Type == QRValueType.Version) {
						if (qr[x, y].Type == QRValueType.Fixed || qr[x, y].Type == QRValueType.TimingPatterns) {
							if (qr[x, y].Value) {
								g.FillRectangle(fixedBlackBrush, new Rectangle(drawX * cellSize, drawY * cellSize, cellSize, cellSize));
							} else {
								g.FillRectangle(fixedWhiteBrush, new Rectangle(drawX * cellSize, drawY * cellSize, cellSize, cellSize));
							}
						} else {
							if (qr[x, y].Value) {
								g.FillRectangle(blackBrush, new Rectangle(drawX * cellSize + halftone, drawY * cellSize + halftone, halftone, halftone));
							} else {
								g.FillRectangle(whiteBrush, new Rectangle(drawX * cellSize + halftone, drawY * cellSize + halftone, halftone, halftone));
							}
						}
					}
				}
			}


			var (binarizerBtimap, pixels2) = QRCodeMagician.GetBinarizer(version, template, cellSize, halftone, deviation);
			qr = QRCodeMagician.ImageArt(dataMode.Value, version, eccLevel, maskVersion, data, pixels2, maxError);
			var bitmap3 = new Bitmap((N + 2) * cellSize, (N + 2) * cellSize);
			using (var g = Graphics.FromImage(bitmap3)) {
				g.Clear(Color.White);
				g.DrawImage((binarizerBtimap), new Rectangle(cellSize, cellSize, N * cellSize, N * cellSize));
				for (int x = 0; x < N; x++) {
					for (int y = 0; y < N; y++) {
						var drawX = x + 1;
						var drawY = y + 1;

						if (margin == 0) {
							if (qr[x, y].Value) {
								g.FillRectangle(blackBrush, new Rectangle(drawX * cellSize, drawY * cellSize, halftone, halftone));
							} else {
								g.FillRectangle(whiteBrush, new Rectangle(drawX * cellSize, drawY * cellSize, halftone, halftone));
							}
							continue;
						}

						// if (qr[x, y].Type == QRValueType.TimingPatterns) continue;

						if (qr[x, y].Type == QRValueType.Fixed || qr[x, y].Type == QRValueType.TimingPatterns) {
							if (qr[x, y].Value) {
								g.FillRectangle(fixedBlackBrush, new Rectangle(drawX * cellSize, drawY * cellSize, cellSize, cellSize));
							} else {
								g.FillRectangle(fixedWhiteBrush, new Rectangle(drawX * cellSize, drawY * cellSize, cellSize, cellSize));
							}
						} else if (!pixels2[x, y].HasFlag(ImagePixel.Stable) || qr[x, y].Value != pixels2[x, y].HasFlag(ImagePixel.Black)) {
							if (qr[x, y].Value) {
								g.FillRectangle(blackBrush, new Rectangle(drawX * cellSize + halftone, drawY * cellSize + halftone, halftone, halftone));
							} else {
								g.FillRectangle(whiteBrush, new Rectangle(drawX * cellSize + halftone, drawY * cellSize + halftone, halftone, halftone));
							}
						}
					}
				}
			}

			WindowManager<FrmView>.Get().Text = $"最大允许错误：{qr.MaxErrorAllowBytes}×{QRInfo.GetEccInfo(version, eccLevel).BlocksInGroup1 + QRInfo.GetEccInfo(version, eccLevel).BlocksInGroup2}";
			WindowManager<FrmView>.Get().picView.Image = bitmap;
			WindowManager<FrmView>.Get().picView2.Image = bitmap2;
			WindowManager<FrmView>.Get().picView3.Image = HybridBinarizer.BinarizerBitmap(bitmap);
			WindowManager<FrmView>.Get().picView4.Image = bitmap3;
			WindowManager<FrmView>.Get().Show();
		}
	}
}
