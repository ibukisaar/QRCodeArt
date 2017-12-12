using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace QRCodeArt {
	public static class QRCodeMagician {
		public enum BitType {
			/// <summary>
			/// 固定值。 Data：无法被修改了。ECC：未使用。
			/// </summary>
			Fixed,
			/// <summary>
			/// 自由值。 Data：待求解。ECC：可以随意赋值。
			/// </summary>
			Freedom,
			/// <summary>
			/// 期望值。 Data：直接设置成Template值。ECC：通过求解Data达到期望值。
			/// </summary>
			Expect
		}

		static readonly int[] Int4Table = { 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4 };

		private static int BitCount(byte x) {
			return Int4Table[x >> 4] + Int4Table[x & 15];
		}

		/// <summary>
		/// 创建混淆QR Code，第二个QR Code将藏在第一个里面。
		/// </summary>
		/// <param name="data1"></param>
		/// <param name="data2"></param>
		/// <returns></returns>
		public static (QRCode QRCode1, QRCode QRCode2, bool Swap) CreateObfuscatedQRCode(byte[] data1, byte[] data2) {
			for (int level = 0; level < 4; level++) {
				var eccLevel = (ECCLevel) level;
				var mode1 = QRDataEncoder.GuessMode(data1);
				while (true) {
					var mode2 = QRDataEncoder.GuessMode(data2);
					while (true) {
						var ver1 = QRDataEncoder.GuessVersion(data1.Length, eccLevel, mode1);
						var ver2 = QRDataEncoder.GuessVersion(data2.Length, eccLevel, mode2);
						var ver = Math.Max(ver1, ver2);
						for (; ver <= 40; ver++) {
							for (int mask1 = 0; mask1 < 8; mask1++) {
								var qr1 = QRCode.AnalysisOverlay(ver, mode1, data1, eccLevel, (MaskVersion) mask1);
								for (int mask2 = 0; mask2 < 8; mask2++) {
									var qr2 = QRCode.AnalysisOverlay(ver, mode2, data2, eccLevel, (MaskVersion) mask2);
									var foverlay = OverlayAnalyzer.IsOverlay(qr1.FormatBinary, qr2.FormatBinary);
									if (foverlay) {
										var overlay = OverlayAnalyzer.IsOverlay(qr1.AnalysisGroup, qr2.AnalysisGroup, qr1.MaxErrorAllowBytes + qr2.MaxErrorAllowBytes);
										if (overlay) {
											var (f1, f2) = OverlayAnalyzer.FormatOverlay(qr1.FormatBinary, qr2.FormatBinary);
											OverlayAnalyzer.Overlay(qr1.AnalysisGroup, qr2.AnalysisGroup, qr1.MaxErrorAllowBytes, qr2.MaxErrorAllowBytes);
											qr1.WriteFormatInformation(f1);
											qr2.WriteFormatInformation(f2);
											// if (!OverlayAnalyzer.IsOverlay(qr1.AnalysisGroup, qr2.AnalysisGroup, 0)) throw new InvalidOperationException();
											qr1.Flush();
											qr2.Flush();
											return (qr1, qr2, false);
										}
									}

									foverlay = OverlayAnalyzer.IsOverlay(qr2.FormatBinary, qr1.FormatBinary);
									if (foverlay) {
										var overlay = OverlayAnalyzer.IsOverlay(qr2.AnalysisGroup, qr1.AnalysisGroup, qr1.MaxErrorAllowBytes + qr2.MaxErrorAllowBytes);
										if (overlay) {
											var (f2, f1) = OverlayAnalyzer.FormatOverlay(qr2.FormatBinary, qr1.FormatBinary);
											OverlayAnalyzer.Overlay(qr2.AnalysisGroup, qr1.AnalysisGroup, qr2.MaxErrorAllowBytes, qr1.MaxErrorAllowBytes);
											qr1.WriteFormatInformation(f1);
											qr2.WriteFormatInformation(f2);
											// if (!OverlayAnalyzer.IsOverlay(qr2.AnalysisGroup, qr1.AnalysisGroup, 0)) throw new InvalidOperationException();
											qr1.Flush();
											qr2.Flush();
											return (qr2, qr1, true);
										}
									}
								}
							}
						}
						if (!NextMode(mode2, out mode2)) break;
					}
					if (!NextMode(mode1, out mode1)) break;
				}
			}
			return default;
		}

		static readonly int[] modeIndexes = { -1, 0, 1, 2, -1, -1, -1, -1 };
		static readonly QRDataMode[] sortedModeTable = { QRDataMode.Numeric, QRDataMode.Alphanumeric, QRDataMode.Byte };

		private static bool NextMode(QRDataMode mode, out QRDataMode result) {
			var i = modeIndexes[(int) mode];
			if (i < 0 || i + 1 >= sortedModeTable.Length) {
				result = 0;
				return false;
			}
			result = sortedModeTable[i];
			return true;
		}

		private static void Xor(bool[] dst, byte[] src) {
			var bits = new BitSet(src);
			for (int i = 0; i < dst.Length; i++) {
				dst[i] = dst[i] != bits[i];
			}
		}

		private static void Xor(byte[] dst, byte[] src) {
			for (int i = 0; i < dst.Length; i++) dst[i] ^= src[i];
		}

		private static void Xor((bool[] Data, bool[] Ecc)[] dst, IReadOnlyList<(byte[] Data, byte[] Ecc)> src) {
			for (int i = 0; i < dst.Length; i++) {
				Xor(dst[i].Data, src[i].Data);
				if (src[i].Ecc == null) continue;
				Xor(dst[i].Ecc, src[i].Ecc);
			}
		}

		private static void Xor((byte[] Data, byte[] Ecc)[] dst, IReadOnlyList<(byte[] Data, byte[] Ecc)> src) {
			for (int i = 0; i < dst.Length; i++) {
				Xor(dst[i].Data, src[i].Data);
				Xor(dst[i].Ecc, src[i].Ecc);
			}
		}

		public static QRCode ImageArt(QRDataMode mode, int version, ECCLevel level, MaskVersion mask, byte[] data, ImagePixel[,] pixels, int maxErrorNumber = 0) {
			var qr = QRCode.AnalysisImageArt(version, mode, data, level, mask);
			var encodedBytes = qr.Encoder.Encode(data, 0, data.Length, false, false);
			var validBits = qr.Encoder.GetDataBitCount(data.Length);

			var flags = new BitType[qr.Encoder.TotalBytes * 8];
			var pixelArray = qr.BitmapDataToArray(pixels);
			for (int i = 0; i < flags.Length; i++) {
				if (pixelArray[i].HasFlag(ImagePixel.Any)) {
					flags[i] = BitType.Freedom;
				} else {
					flags[i] = BitType.Expect;
				}
			}
			var templateArray = Array.ConvertAll(pixelArray, p => p.HasFlag(ImagePixel.Black)); // template = image
			var flagBlocks = Arranger.GetBitBlocks(version, level, flags);
			var templateBlocks = Arranger.GetBitBlocks(version, level, templateArray);
			var dataBlocks = Arranger.GetBitBlocks<bool>(version, level, null, false);

			Xor(templateBlocks, qr.AnalysisGroup); // template = image ^ mask
			int fixedCount = 0;
			for (int i = 0; i < encodedBytes.Length; i++) {
				var dataBits = new BitSet(encodedBytes[i].Data);
				for (int j = 0; j < flagBlocks[i].Data.Length; j++) {
					if (fixedCount < validBits) {
						flagBlocks[i].Data[j] = BitType.Fixed;
						fixedCount++;
					} else if (flagBlocks[i].Data[j] == BitType.Expect) {
						dataBits[j] = templateBlocks[i].Data[j];
					}
				}
				encodedBytes[i].Ecc = GF.XPolynom.RSEncode(encodedBytes[i].Data, 0, encodedBytes[i].Data.Length, qr.Encoder.ECCInfo.ECCPerBytes);
			}
			Xor(templateBlocks, encodedBytes);

			var tempResult = new(byte[] Data, byte[] Ecc)[dataBlocks.Length];
			for (int i = 0; i < dataBlocks.Length; i++) {
				dataBlocks[i].Data = Arranger.ToBitArray(encodedBytes[i].Data);

				// RS(padding) = RS(data) ^ image ^ mask
				Match(templateBlocks[i], (dataBlocks[i].Data, qr.Encoder.ECCInfo.ECCPerBytes * 8), flagBlocks[i]);

				// RS(data+padding) = RS(data) ^ RS(padding) = RS(data) ^ RS(data) ^ image ^ mask = image ^ mask
				var dataBytes = Arranger.ToByteArray(dataBlocks[i].Data);
				var ecc = GF.XPolynom.RSEncode(dataBytes, 0, dataBytes.Length, qr.Encoder.ECCInfo.ECCPerBytes);
				tempResult[i] = (dataBytes, ecc);

				#region DEBUG
				var bits = new BitSet(ecc);
				var cmpBits = new BitSet(encodedBytes[i].Ecc);
				int total = 0, correct = 0;
				for (int j = 0; j < flagBlocks[i].Ecc.Length; j++) {
					if (flagBlocks[i].Ecc[j] == BitType.Expect) {
						total++;
						if ((templateBlocks[i].Ecc[j] != cmpBits[j]) == bits[j]) {
							correct++;
							// Console.Write($"{j} ");
						}
					}
				}
				// Console.WriteLine();
				Console.WriteLine($"总数：{total}, 命中：{correct}");
				#endregion
			}

			// output.ecc = RS(padding) ^ mask = image ^ mask ^ mask = image
			if (maxErrorNumber == 0) {
				qr.WriteData(Arranger.Interlock(tempResult));
			} else { // 如果允许错误，那么将调整某些字节和图像一致
				maxErrorNumber = Math.Min(maxErrorNumber, qr.MaxErrorAllowBytes);

				Xor(tempResult, qr.AnalysisGroup); // outBlocks = outBlocks ^ mask
				var imageBlocks = Arranger.GetBlocks(version, level, Arranger.ToByteArray(templateArray));
				var typeFlagsBlocks = Arranger.GetBlocks(version, level, Arranger.ToByteArray(Array.ConvertAll(pixelArray, p => !p.HasFlag(ImagePixel.Any))));
				for (int i = 0; i < tempResult.Length; i++) {
					int dataLength = tempResult[i].Data.Length;
					int eccLength = tempResult[i].Ecc.Length;
					var errorTable = new(int Index, int Score)[dataLength + eccLength];
					for (int j = 0; j < errorTable.Length; j++) errorTable[j].Index = j;

					for (int j = 0; j < dataLength; j++) {
						byte diff = (byte) (tempResult[i].Data[j] ^ imageBlocks[i].Data[j]);
						errorTable[j].Score = BitCount(diff);
						diff &= typeFlagsBlocks[i].Data[j];
						errorTable[j].Score += 8 * BitCount(diff);
					}
					for (int j = 0; j < eccLength; j++) {
						byte diff = (byte) (tempResult[i].Ecc[j] ^ imageBlocks[i].Ecc[j]);
						errorTable[dataLength + j].Score = BitCount(diff);
						diff &= typeFlagsBlocks[i].Ecc[j];
						errorTable[dataLength + j].Score += 8 * BitCount(diff);
					}

					foreach (var (index, _) in errorTable.OrderByDescending(p => p.Score).Take(maxErrorNumber)) {
						if (index < dataLength) {
							tempResult[i].Data[index] = imageBlocks[i].Data[index];
						} else {
							tempResult[i].Ecc[index - dataLength] = imageBlocks[i].Ecc[index - dataLength];
						}
					}
				}
				qr.WriteData(Arranger.Interlock(tempResult), false);
			}

			foreach (var (_, x, y) in qr.GetPoints(QRValueType.Padding)) {
				qr[x, y].Value = pixels[x, y].HasFlag(ImagePixel.Black);
			}

			return qr;
		}


		/// <summary>
		/// 让<paramref name="dst"/>的RS编码结果尽量匹配<paramref name="template"/>。
		/// </summary>
		/// <param name="template">目标模板</param>
		/// <param name="dst">要计算的数据</param>
		/// <param name="flag">通过此参数设置方程组</param>
		public static void Match((bool[] Data, bool[] Ecc) template, (bool[] Data, int EccBits) dst, (BitType[] Data, BitType[] Ecc) flag) {
			var unknowns = new List<int>(); // 未知数列表
			for (int i = 0; i < dst.Data.Length; i++) {
				if (flag.Data[i] == BitType.Freedom) {
					unknowns.Add(i);
				}
			}
			if (unknowns.Count == 0) return;

			var expects = new List<int>(); // 期望值列表
			for (int i = 0; i < dst.EccBits; i++) {
				if (flag.Ecc[i] == BitType.Expect) {
					expects.Add(i);
				}
			}
			if (expects.Count == 0) return;
			// unknowns和expects将组成方程组

			var msgExpectBits = new BitSet(dst.Data.Length);
			var leftBits = new byte[unknowns.Count][];
			var rightBits = new byte[unknowns.Count][];
			for (int i = 0; i < unknowns.Count; i++) {
				leftBits[i] = new byte[unknowns.Count];
				rightBits[i] = new byte[expects.Count];

				msgExpectBits[unknowns[i]] = true;
				var rBits = new BitSet(GF.XPolynom.RSEncode(msgExpectBits.ByteArray, 0, msgExpectBits.ByteArray.Length, dst.EccBits / 8));
				msgExpectBits[unknowns[i]] = false;

				leftBits[i][i] = 1;
				for (int j = 0; j < expects.Count; j++) {
					rightBits[i][j] = rBits[expects[j]] ? (byte) 1 : (byte) 0;
				}
			}

			var rightIndexes = GaussianElimination(leftBits, rightBits);
			for (int i = 0; i < rightIndexes.Count; i++) {
				var leftRow = leftBits[i];
				if (template.Ecc[expects[rightIndexes[i]]]) {
					for (int j = 0; j < unknowns.Count; j++) {
						if (leftRow[j] != 0) {
							dst.Data[unknowns[j]] = !dst.Data[unknowns[j]];
							msgExpectBits[unknowns[j]] = !msgExpectBits[unknowns[j]];
						}
					}
				}
			}
		}

		private static void DebugPrint(byte[][] left, byte[][] right) {
			for (int i = 0; i < left.Length; i++) {
				for (int j = 0; j < left[0].Length; j++) {
					Console.Write((left[i][j] == 1 ? "1" : "."));
				}
				Console.Write(" | ");
				for (int j = 0; j < right[0].Length; j++) {
					Console.Write((right[i][j] == 1 ? "1" : "."));
				}
				Console.WriteLine();
			}
		}

		/// <summary>
		/// 求极大无关组的列索引
		/// </summary>
		/// <param name="mat">阶梯形矩阵</param>
		/// <returns></returns>
		private static List<int> LinearlyIndependent(byte[][] mat) {
			var result = new List<int>();
			int row = 0;
			for (int col = 0; row < mat.Length && col < mat[0].Length; col++) {
				if (mat[row][col] != 0) {
					result.Add(col);
					row++;
				}
			}
			return result;
		}

		/// <summary>
		/// 使用初等行变换进行高斯消元，返回<paramref name="right"/>标准正交基的列索引。
		/// </summary>
		/// <param name="left">既是输入也是输出</param>
		/// <param name="right"></param>
		/// <returns></returns>
		private static List<int> GaussianElimination(byte[][] left, byte[][] right) {
			void Swap(ref byte[] x, ref byte[] y) {
				var t = x; x = y; y = t;
			}

			bool Find1AndSwap(int row, int col) {
				if (right[row][col] == 0) {
					for (int i = row + 1; i < right.Length; i++) {
						if (right[i][col] != 0) {
							Swap(ref left[row], ref left[i]);
							Swap(ref right[row], ref right[i]);
							return true;
						}
					}
					return false;
				}
				return true;
			}

			void Xor(byte[][] mat, int dstRow, int srcRow) {
				var dst = mat[dstRow];
				var src = mat[srcRow];
				for (int i = 0; i < dst.Length; i++) {
					dst[i] ^= src[i];
				}
			}

			void Elimination() {
				for (int row = 0, col = 0; row < right.Length && col < right[0].Length; col++) {
					if (!Find1AndSwap(row, col)) continue;
					for (int r = row + 1; r < right.Length; r++) {
						if (right[r][col] != 0) {
							Xor(left, r, row);
							Xor(right, r, row);
						}
					}
					row++;
				}
			}

			void ReverseMat(byte[][] mat) {
				Array.Reverse(mat);
				foreach (var row in mat) {
					Array.Reverse(row);
				}
			}

			Elimination();
			var colIndexes = LinearlyIndependent(right);
			if (colIndexes.Count < right[0].Length) {
				Console.WriteLine($"不受控制的RS编码Bit数：{right[0].Length - colIndexes.Count}"); // 但也可能命中
				var newRight = new byte[right.Length][];
				for (int i = 0; i < right.Length; i++) {
					newRight[i] = new byte[colIndexes.Count];
					for (int j = 0; j < colIndexes.Count; j++) {
						newRight[i][j] = right[i][colIndexes[j]];
					}
				}
				right = newRight;
			}
			ReverseMat(left);
			ReverseMat(right);
			Elimination();
			ReverseMat(left);
			ReverseMat(right);

			int baseIndex = 0;
			for (int i = 0; right[i][0] == 0; i++) baseIndex++;
			if (baseIndex > 0) {
				for (int i = 0; i < colIndexes.Count; i++) {
					Swap(ref left[i], ref left[baseIndex + i]);
				}
			}

			return colIndexes;
		}

		unsafe public static ImagePixel[,] GetImagePixel(int version, Bitmap bitmap, int cellSize, int blackThreshold = 64, int whiteThreshold = 192, int halftoneSize = 0) {
			int N = QRCode.GetN(version);
			bitmap = new Bitmap(bitmap, N * cellSize, N * cellSize);
			var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			var p = (uint*) bitmapData.Scan0;
			var W = N * cellSize;
			var result = new ImagePixel[N, N];
			if (halftoneSize <= 0 || halftoneSize > cellSize) halftoneSize = cellSize;
			var margin = (cellSize - halftoneSize) / 2;
			int baseCount = halftoneSize * halftoneSize;
			int centerThreshold = (blackThreshold + whiteThreshold) / 2;

			for (int y = 0; y < N; y++) {
				for (int x = 0; x < N; x++) {
					int gray = 0;
					var alpha = 0;
					var px = x * cellSize;
					var py = y * cellSize;
					for (int y0 = margin; y0 < margin + halftoneSize; y0++) {
						for (int x0 = margin; x0 < margin + halftoneSize; x0++) {
							var pix = (byte*) &p[(py + y0) * W + (px + x0)];
							gray += (pix[2] * 38 + pix[1] * 75 + pix[0] * 15) >> 7;
							// gray += (pix[2] + pix[1] + pix[0]) / 3;
							alpha += pix[3];
						}
					}
					result[x, y] = gray >= centerThreshold * baseCount ? ImagePixel.White : ImagePixel.Black;
					if (alpha < 128 * baseCount) result[x, y] = ImagePixel.Any | ImagePixel.White;
					if (gray <= blackThreshold * baseCount || gray >= whiteThreshold * baseCount) result[x, y] |= ImagePixel.Stable;
				}
			}

			return result;
		}
	}
}
