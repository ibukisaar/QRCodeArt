using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	unsafe public static class QRCodeMagician {
		public enum BitType : byte {
			/// <summary>
			/// 固定值。 Data：无法被修改了。ECC：未使用。
			/// </summary>
			Fixed,
			/// <summary>
			/// 自由值。 Data：可以随意赋值（待求解）。ECC：无视，不关心是0还是1。
			/// </summary>
			Freedom,
			/// <summary>
			/// 期望值。 Data：直接设置成Template值。ECC：通过求解Freedom的Data达到期望值。
			/// </summary>
			Expect
		}

		static readonly int[] Int4Table = { 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4 };

		private static int BitCount(byte x) {
			return Int4Table[x >> 4] + Int4Table[x & 15];
		}

		///// <summary>
		///// 创建混淆QR Code，第二个QR Code将藏在第一个里面。
		///// </summary>
		///// <param name="data1"></param>
		///// <param name="data2"></param>
		///// <returns></returns>
		//[Obsolete("这是个失败品，请不要使用", true)]
		//public static (QRCode QRCode1, QRCode QRCode2, bool Swap) CreateObfuscatedQRCode(byte[] data1, byte[] data2) {
		//	for (int level = 0; level < 4; level++) {
		//		var eccLevel = (ECCLevel)level;
		//		var mode1 = DataEncoder.GuessMode(data1);
		//		while (true) {
		//			var mode2 = DataEncoder.GuessMode(data2);
		//			while (true) {
		//				var ver1 = DataEncoder.GuessVersion(data1.Length, eccLevel, mode1);
		//				var ver2 = DataEncoder.GuessVersion(data2.Length, eccLevel, mode2);
		//				var ver = Math.Max(ver1, ver2);
		//				for (; ver <= 40; ver++) {
		//					for (int mask1 = 0; mask1 < 8; mask1++) {
		//						var qr1 = QRCode.AnalysisOverlay(ver, mode1, data1, eccLevel, (MaskVersion)mask1);
		//						for (int mask2 = 0; mask2 < 8; mask2++) {
		//							var qr2 = QRCode.AnalysisOverlay(ver, mode2, data2, eccLevel, (MaskVersion)mask2);
		//							var foverlay = OverlayAnalyzer.IsOverlay(qr1.FormatBinary, qr2.FormatBinary);
		//							if (foverlay) {
		//								var overlay = OverlayAnalyzer.IsOverlay(qr1.AnalysisGroup, qr2.AnalysisGroup, qr1.MaxErrorAllowBytes + qr2.MaxErrorAllowBytes);
		//								if (overlay) {
		//									var (f1, f2) = OverlayAnalyzer.FormatOverlay(qr1.FormatBinary, qr2.FormatBinary);
		//									OverlayAnalyzer.Overlay(qr1.AnalysisGroup, qr2.AnalysisGroup, qr1.MaxErrorAllowBytes, qr2.MaxErrorAllowBytes);
		//									qr1.WriteFormatInformation(f1);
		//									qr2.WriteFormatInformation(f2);
		//									// if (!OverlayAnalyzer.IsOverlay(qr1.AnalysisGroup, qr2.AnalysisGroup, 0)) throw new InvalidOperationException();
		//									qr1.Flush();
		//									qr2.Flush();
		//									return (qr1, qr2, false);
		//								}
		//							}

		//							foverlay = OverlayAnalyzer.IsOverlay(qr2.FormatBinary, qr1.FormatBinary);
		//							if (foverlay) {
		//								var overlay = OverlayAnalyzer.IsOverlay(qr2.AnalysisGroup, qr1.AnalysisGroup, qr1.MaxErrorAllowBytes + qr2.MaxErrorAllowBytes);
		//								if (overlay) {
		//									var (f2, f1) = OverlayAnalyzer.FormatOverlay(qr2.FormatBinary, qr1.FormatBinary);
		//									OverlayAnalyzer.Overlay(qr2.AnalysisGroup, qr1.AnalysisGroup, qr2.MaxErrorAllowBytes, qr1.MaxErrorAllowBytes);
		//									qr1.WriteFormatInformation(f1);
		//									qr2.WriteFormatInformation(f2);
		//									// if (!OverlayAnalyzer.IsOverlay(qr2.AnalysisGroup, qr1.AnalysisGroup, 0)) throw new InvalidOperationException();
		//									qr1.Flush();
		//									qr2.Flush();
		//									return (qr2, qr1, true);
		//								}
		//							}
		//						}
		//					}
		//				}
		//				if (!NextMode(mode2, out mode2)) break;
		//			}
		//			if (!NextMode(mode1, out mode1)) break;
		//		}
		//	}
		//	return default;
		//}

		static readonly int[] modeIndexes = { -1, 0, 1, 2, -1, -1, -1, -1 };
		static readonly DataMode[] sortedModeTable = { DataMode.Numeric, DataMode.Alphanumeric, DataMode.Byte };

		private static bool NextMode(DataMode mode, out DataMode result) {
			var i = modeIndexes[(int)mode];
			if (i < 0 || i + 1 >= sortedModeTable.Length) {
				result = 0;
				return false;
			}
			result = sortedModeTable[i + 1];
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

		public static QRCode ImageArt(DataMode mode, int version, ECCLevel level, MaskVersion mask, byte[] data, ImagePixel[,] pixels, int maxErrorNumber = 0, bool cracking = true) {
			var qr = QRCode.AnalysisImageArt(version, mode, data, level, mask);
			var encodedBytes = qr.Encoder.Encode(data, 0, data.Length, false, false);
			var validBits = qr.Encoder.GetDataBitCount(data.Length);

			var flags = new BitType[QRInfo.GetTotalBytes(version, level) * 8];
			var pixelArray = qr.BitmapDataToArray(pixels);
			for (int i = 0; i < flags.Length; i++) {
				if (pixelArray[i].HasFlag(ImagePixel.Any)) {
					flags[i] = BitType.Freedom;
				} else {
					flags[i] = BitType.Expect;
				}
			}
			var templateArray = Array.ConvertAll(pixelArray, p => (p & ImagePixel.PixelMask) == ImagePixel.Black); // template = image
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
				encodedBytes[i].Ecc = RS.Encode(encodedBytes[i].Data, qr.Encoder.ECCInfo.ECCPerBytes);
			}
			Xor(templateBlocks, encodedBytes);

			var tempResult = new (byte[] Data, byte[] Ecc)[dataBlocks.Length];
			for (int i = 0; i < dataBlocks.Length; i++) {
				dataBlocks[i].Data = Arranger.ToBitArray(encodedBytes[i].Data);

				// RS(padding) = RS(data) ^ image ^ mask
				var templateData = templateBlocks[i].Data.Zip(flagBlocks[i].Data, (val, flag) => new MagicBit(flag == BitType.Freedom ? MagicBitType.Freedom : MagicBitType.Expect, val)).ToArray();
				var templateEcc = templateBlocks[i].Ecc.Zip(flagBlocks[i].Ecc, (val, flag) => new MagicBit(flag == BitType.Freedom ? MagicBitType.Freedom : MagicBitType.Expect, val)).ToArray();
				Match(templateData, templateEcc, dataBlocks[i].Data, cracking);

				// RS(data+padding) = RS(data) ^ RS(padding) = RS(data) ^ RS(data) ^ image ^ mask = image ^ mask
				var dataBytes = Arranger.ToByteArray(dataBlocks[i].Data);
				var ecc = RS.Encode(dataBytes, qr.Encoder.ECCInfo.ECCPerBytes);
				tempResult[i] = (dataBytes, ecc);

#if DEBUG
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
#endif
			}

			// output.ecc = RS(padding) ^ mask = image ^ mask ^ mask = image
			if (maxErrorNumber == 0) {
				qr.WriteData(Arranger.Interlock(tempResult));
			} else { // 如果允许错误，那么将调整某些字节和图像一致
				if (maxErrorNumber < 0 || maxErrorNumber > qr.MaxErrorAllowBytes)
					maxErrorNumber = qr.MaxErrorAllowBytes;

				Xor(tempResult, qr.AnalysisGroup); // outBlocks = outBlocks ^ mask
				var imageBlocks = Arranger.GetBlocks(version, level, Arranger.ToByteArray(templateArray));
				var anyFlagsBlocks = Arranger.GetBlocks(version, level, Arranger.ToByteArray(Array.ConvertAll(pixelArray, p => !p.HasFlag(ImagePixel.Any))));
				var unstableFlagsBlocks = Arranger.GetBlocks(version, level, Arranger.ToByteArray(Array.ConvertAll(pixelArray, p => (p & (ImagePixel.Stable | ImagePixel.Any)) == 0)));
				var points = qr.GetDataRegionPoints().Select(args => (args.X, args.Y));
				var pointBlocks = Arranger.GetBitBlocks(version, level, points);
				const int NormalWeight = 1, AnyFlagWeight = 4, UnstableWeight = 16;

				for (int i = 0; i < tempResult.Length; i++) {
					int dataLength = tempResult[i].Data.Length;
					int eccLength = tempResult[i].Ecc.Length;
					var errorTable = new (int Index, bool Unstable, int Score)[dataLength + eccLength];
					for (int j = 0; j < errorTable.Length; j++) errorTable[j].Index = j;

					for (int j = 0; j < dataLength; j++) {
						byte diff = (byte)(tempResult[i].Data[j] ^ imageBlocks[i].Data[j]);
						errorTable[j].Score = NormalWeight * BitCount(diff);
						var unstableCount = BitCount(unstableFlagsBlocks[i].Data[j]);
						if (unstableCount == 0) {
							diff &= anyFlagsBlocks[i].Data[j];
							errorTable[j].Score += AnyFlagWeight * BitCount(diff);
						} else {
							errorTable[j].Score += UnstableWeight * unstableCount;
							errorTable[j].Unstable = true;
						}
					}
					for (int j = 0; j < eccLength; j++) {
						byte diff = (byte)(tempResult[i].Ecc[j] ^ imageBlocks[i].Ecc[j]);
						errorTable[dataLength + j].Score = BitCount(diff);
						var unstableCount = NormalWeight * BitCount(unstableFlagsBlocks[i].Ecc[j]);
						if (unstableCount == 0) {
							diff &= anyFlagsBlocks[i].Ecc[j];
							errorTable[dataLength + j].Score += AnyFlagWeight * BitCount(diff);
						} else {
							errorTable[j].Score += UnstableWeight * unstableCount;
							errorTable[j].Unstable = true;
						}
					}

					foreach (var (index, unstable, _) in errorTable.OrderByDescending(p => p.Score).Take(maxErrorNumber)) {
						int subIndex;
						byte[] subResult, subImage;
						(int X, int Y)[] subPoints;
						if (index < dataLength) {
							subIndex = index;
							subResult = tempResult[i].Data;
							subImage = imageBlocks[i].Data;
							subPoints = pointBlocks[i].Data;
						} else {
							subIndex = index - dataLength;
							subResult = tempResult[i].Ecc;
							subImage = imageBlocks[i].Ecc;
							subPoints = pointBlocks[i].Ecc;
						}

						if (unstable) {
							for (int k = 0; k < 8; k++) {
								var (x, y) = subPoints[subIndex * 8 + k];
								pixels[x, y] |= ImagePixel.Stable;
							}
						} else {
							subResult[subIndex] = subImage[subIndex];
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
					rightBits[i][j] = rBits[expects[j]] ? (byte)1 : (byte)0;
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

		static readonly byte[] Log2Table = {
			0,1,10,2,11,14,24,3,30,12,28,15,21,25,17,4,31,9,13,23,29,27,20,16,8,22,26,19,7,18,6,5
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int IntLog2(int x) => Log2Table[((uint)x * 0x7C4BB35u) >> 27];

		static byte[] Graycode(int bits) {
			if (bits < 0) throw new ArgumentException($"{nameof(bits)}必须>=0");
			var result = new byte[1 << bits];
			for (int n = 1; n <= bits; n++) {
				int offset = 1 << (n - 1);
				result[offset] = (byte)(n - 1);
				for (int i = 1; i < offset; i++) {
					result[offset + i] = result[offset - i];
				}
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int BitCount(ulong x) {
			x = x - ((x >> 1) & 0x5555555555555555u);
			x = (x & 0x3333333333333333u) + ((x >> 2) & 0x3333333333333333u);
			x = x + (x >> 4) & 0x0f0f0f0f0f0f0f0fu;
			return (int)(x % 255);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void SetBit(byte* bits, int index, bool value) {
			if (value) {
				bits[index >> 3] |= (byte)(1 << (7 - (index & 7)));
			} else {
				bits[index >> 3] &= (byte)~(1 << (7 - (index & 7)));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool GetBit(ReadOnlySpan<byte> bits, int index) {
			return (bits[index >> 3] & (1 << (7 - (index & 7)))) != 0;
		}

		static int Match1(byte[] graycode, ulong** eccTable, ulong* target) {
			int ans = 0;
			int minError = BitCount(target[0]);
			int currGraycode = 0;
			for (int i = 1; i < graycode.Length; i++) {
				if (minError == 0) return ans;
				currGraycode ^= 1 << graycode[i];
				var vector = eccTable[graycode[i]];
				target[0] ^= vector[0];
				int error = BitCount(target[0]);
				if (error < minError) {
					minError = error;
					ans = currGraycode;
				}
			}
			return ans;
		}

		static int Match2(byte[] graycode, ulong** eccTable, ulong* target) {
			int ans = 0;
			int minError = BitCount(target[0]) + BitCount(target[1]);
			int currGraycode = 0;
			for (int i = 1; i < graycode.Length; i++) {
				if (minError == 0) return ans;
				currGraycode ^= 1 << graycode[i];
				var vector = eccTable[graycode[i]];
				target[0] ^= vector[0];
				target[1] ^= vector[1];
				int error = BitCount(target[0]) + BitCount(target[1]);
				if (error < minError) {
					minError = error;
					ans = currGraycode;
				}
			}
			return ans;
		}

		static int Match3(byte[] graycode, ulong** eccTable, ulong* target) {
			int ans = 0;
			int minError = BitCount(target[0]) + BitCount(target[1]) + BitCount(target[2]);
			int currGraycode = 0;
			for (int i = 1; i < graycode.Length; i++) {
				if (minError == 0) return ans;
				currGraycode ^= 1 << graycode[i];
				var vector = eccTable[graycode[i]];
				target[0] ^= vector[0];
				target[1] ^= vector[1];
				target[2] ^= vector[2];
				int error = BitCount(target[0]) + BitCount(target[1]) + BitCount(target[2]);
				if (error < minError) {
					minError = error;
					ans = currGraycode;
				}
			}
			return ans;
		}

		static int Match4(byte[] graycode, ulong** eccTable, ulong* target) {
			int ans = 0;
			int minError = BitCount(target[0]) + BitCount(target[1]) + BitCount(target[2]) + BitCount(target[3]);
			int currGraycode = 0;
			for (int i = 1; i < graycode.Length; i++) {
				if (minError == 0) return ans;
				currGraycode ^= 1 << graycode[i];
				var vector = eccTable[graycode[i]];
				target[0] ^= vector[0];
				target[1] ^= vector[1];
				target[2] ^= vector[2];
				target[3] ^= vector[3];
				int error = BitCount(target[0]) + BitCount(target[1]) + BitCount(target[2]) + BitCount(target[3]);
				if (error < minError) {
					minError = error;
					ans = currGraycode;
				}
			}
			return ans;
		}

		static int Match(ReadOnlySpan<IntPtr> eccTable, IntPtr target, int longCount) {
			var graycode = Graycode(eccTable.Length); // 用格雷码加速遍历过程
			fixed (IntPtr* pTable = eccTable) {
				switch (longCount) {
					case 1: return Match1(graycode, (ulong**)pTable, (ulong*)target);
					case 2: return Match2(graycode, (ulong**)pTable, (ulong*)target);
					case 3: return Match3(graycode, (ulong**)pTable, (ulong*)target);
					case 4: return Match4(graycode, (ulong**)pTable, (ulong*)target);
				}
			}
			throw new NotSupportedException();
		}

		/// <summary>
		/// 尽量让<paramref name="outData"/>的RS编码结果匹配<paramref name="templateEcc"/>
		/// </summary>
		/// <param name="templateData"></param>
		/// <param name="templateEcc"></param>
		/// <param name="outData"></param>
		/// <param name="cracking">如果为true，则尝试暴力破解方程组</param>
		public static void Match(ReadOnlySpan<MagicBit> templateData, ReadOnlySpan<MagicBit> templateEcc, Span<bool> outData, bool cracking = true) {
			if (templateData.Length != outData.Length) throw new ArgumentException($"{nameof(templateData)}和{nameof(outData)}长度不一致");
			if (templateData.Length % 8 != 0) throw new ArgumentException($"{nameof(templateData)}的长度必须是8的倍数");
			if (templateEcc.Length % 8 != 0) throw new ArgumentException($"{nameof(templateEcc)}的长度必须是8的倍数");
			int dataLength = templateData.Length / 8;
			int eccLength = templateEcc.Length / 8;

			var unknowns = new List<int>(); // 未知数列表
			for (int i = 0; i < templateData.Length; i++) {
				if (templateData[i].Type == MagicBitType.Freedom) {
					unknowns.Add(i);
				}
			}
			if (unknowns.Count == 0) return;

			var expects = new List<int>(); // 期望值列表
			for (int i = 0; i < templateEcc.Length; i++) {
				if (templateEcc[i].Type == MagicBitType.Expect) {
					expects.Add(i);
				}
			}
			if (expects.Count == 0) {
				for (int i = 0; i < unknowns.Count; i++) {
					outData[unknowns[i]] = templateData[unknowns[i]].Value;
				}
				return;
			}
			// unknowns和expects将组成方程组

			Span<byte> tempEcc = stackalloc byte[eccLength];
			var gaussianElimination = new GaussianEliminationTarget(Math.Min(unknowns.Count, expects.Count));
			var actualUnknowns = new List<int>();
			var overflow = new List<int>();

			RandomOrder(unknowns);
			for (int i = 0; i < unknowns.Count; i++) {
				RS.Encode((byte)(1 << (7 - (unknowns[i] & 7))), dataLength - 1 - (unknowns[i] >> 3), tempEcc);
				var eccVector = new byte[expects.Count];
				for (int j = 0; j < expects.Count; j++) {
					eccVector[j] = (tempEcc[expects[j] >> 3] & (1 << (7 - (expects[j] & 7)))) != 0 ? (byte)1 : (byte)0;
				}

				bool success = gaussianElimination.AddVectorDamage(eccVector);
				if (success) {
					actualUnknowns.Add(unknowns[i]);
				} else {
					overflow.Add(unknowns[i]);
				}
			}

			byte[] targetEcc = new byte[expects.Count];
			for (int i = 0; i < targetEcc.Length; i++) {
				targetEcc[i] = templateEcc[expects[i]].Value ? (byte)1 : (byte)0;
			}

			if (overflow.Count > 0) {
				var overflowData = new BitSet(new byte[dataLength]);
				foreach (var i in overflow) { // 溢出的未知数不需要参与运算，直接设置成templateData
					overflowData[i] = outData[i] = templateData[i].Value;
				}
				RS.Encode(overflowData.ByteArray, tempEcc);
				for (int i = 0; i < expects.Count; i++) {
					if (GetBit(tempEcc, expects[i])) {
						targetEcc[i] ^= 1;
					}
				}
			}

#if DEBUG
			#region DEBUG
			Console.WriteLine($"自由：{unknowns.Count}, 实际自由：{actualUnknowns.Count}, 溢出自由：{overflow.Count}, 期望：{expects.Count}");
			#endregion
#endif

			if (cracking && actualUnknowns.Count <= 24 && expects.Count >= 28) { // 如果未知数太少了，可以采用暴力搜索求解方程组
				int expectsLongCount = (expects.Count + 63) / 64;
				var eccVectorBuffer = stackalloc ulong[actualUnknowns.Count * expectsLongCount];
				var targetEccBuffer = stackalloc ulong[expectsLongCount];
				Span<IntPtr> eccTable = stackalloc IntPtr[actualUnknowns.Count];
				for (int i = 0; i < actualUnknowns.Count; i++) {
					RS.Encode((byte)(1 << (7 - (actualUnknowns[i] & 7))), dataLength - 1 - (actualUnknowns[i] >> 3), tempEcc);
					var eccVector = (byte*)(eccVectorBuffer + i * expectsLongCount);
					for (int j = 0; j < expects.Count; j++) {
						SetBit(eccVector, j, (tempEcc[expects[j] >> 3] & (1 << (7 - (expects[j] & 7)))) != 0);
					}
					eccTable[i] = (IntPtr)eccVector;
				}

				for (int j = 0; j < expects.Count; j++) {
					SetBit((byte*)targetEccBuffer, j, targetEcc[j] != 0);
				}

				int ans = Match(eccTable, (IntPtr)targetEccBuffer, expectsLongCount);
				for (int i = 0; i < actualUnknowns.Count; i++) {
					outData[actualUnknowns[i]] = (ans & (1 << i)) != 0;
				}

			} else { // 解方程
				var left = new byte[actualUnknowns.Count];
				var linearlyIndependent = gaussianElimination.LinearlyIndependent;
				for (int i = 0; i < linearlyIndependent.Count; i++) {
					if (targetEcc[linearlyIndependent[i]] != 0) {
						for (int j = 0; j < left.Length; j++) {
							left[j] ^= gaussianElimination.Left[i][j];
						}
					}
				}

				var tempData = new BitSet(new byte[dataLength]);
				for (int i = 0; i < left.Length; i++) {
					tempData[actualUnknowns[i]] = left[i] != 0;
				}
				RS.Encode(tempData.ByteArray, tempEcc);
				int error = 0;
				for (int i = 0; i < targetEcc.Length; i++) {
					if ((targetEcc[i] != 0) != GetBit(tempEcc, expects[i])) {
						error++;
					}
				}

				if (cracking && error >= 2) { // 局部暴力搜索，尝试降低错误，提高匹配率
					var loopLevel = Math.Min((int)Math.Log(1000_0000, actualUnknowns.Count), error - 1); // 大约尝试1千万次
					var indexes = stackalloc int[loopLevel];
					int expectsLongCount = (expects.Count + 63) / 64;
					var eccVectorBuffer = stackalloc ulong[actualUnknowns.Count * expectsLongCount];
					var targetEccBuffer = stackalloc ulong[expectsLongCount];
					var targetBuffer = stackalloc ulong[expectsLongCount];
					Span<IntPtr> eccTable = stackalloc IntPtr[actualUnknowns.Count];

					for (int j = 0; j < expects.Count; j++) {
						SetBit((byte*)targetBuffer, j, GetBit(tempEcc, expects[j]));
					}

					for (int i = 0; i < actualUnknowns.Count; i++) {
						RS.Encode((byte)(1 << (7 - (actualUnknowns[i] & 7))), dataLength - 1 - (actualUnknowns[i] >> 3), tempEcc);
						var eccVector = (byte*)(eccVectorBuffer + i * expectsLongCount);
						for (int j = 0; j < expects.Count; j++) {
							SetBit(eccVector, j, (tempEcc[expects[j] >> 3] & (1 << (7 - (expects[j] & 7)))) != 0);
						}
						eccTable[i] = (IntPtr)eccVector;
					}

					for (int j = 0; j < expects.Count; j++) {
						SetBit((byte*)targetEccBuffer, j, targetEcc[j] != 0);
					}
					for (int i = 0; i < expectsLongCount; i++) {
						targetBuffer[i] ^= targetEccBuffer[i];
					}

					int maxIndex = actualUnknowns.Count;
					int currLoopLevel = 1;
					int minError = error;
					int[] minXor = Array.Empty<int>();
					var tempTarget = stackalloc ulong[expectsLongCount];
					int error0 = error;

					while (minError > currLoopLevel) {
					Restart:
						indexes[0]++;
						int i = 0;
						for (; i < currLoopLevel - 1; i++) {
							if (indexes[i] >= maxIndex) {
								indexes[i] = 0;
								indexes[i + 1]++;
							}
						}
						if (indexes[i] >= maxIndex) {
							indexes[i] = 0;
							indexes[i + 1]++;
							currLoopLevel++;
							if (currLoopLevel > loopLevel) break;
						}

						for (i = 0; i < currLoopLevel; i++) {
							for (int j = i + 1; j < currLoopLevel; j++) {
								if (indexes[i] == indexes[j]) goto Restart;
							}
						}

						for (i = 0; i < expectsLongCount; i++) {
							tempTarget[i] = targetBuffer[i];
						}
						for (i = 0; i < currLoopLevel; i++) {
							var xorVector = (ulong*)eccTable[indexes[i]];
							for (int j = 0; j < expectsLongCount; j++) {
								tempTarget[j] ^= xorVector[j];
							}
						}
						error = 0;
						for (i = 0; i < expectsLongCount; i++) {
							error += BitCount(tempTarget[i]);
						}

						if (error < minError) {
							minError = error;
							minXor = new ReadOnlySpan<int>(indexes, currLoopLevel).ToArray();
						}
					}

					foreach (var xorIndex in minXor) {
						left[xorIndex] ^= 1;
					}

#if DEBUG
					#region DEBUG
					Console.WriteLine($"调整比特数：{minXor.Length}, 原错误：{error0}, 调整后错误：{minError}");
					#endregion
#endif
				}

				for (int i = 0; i < left.Length; i++) {
					outData[actualUnknowns[i]] = left[i] != 0;
				}
			}
		}

		static readonly Random random = new Random();

		static void RandomOrder<T>(IList<T> list) {
			for (int i = 0; i < list.Count; i++) {
				int j = random.Next(i, list.Count);
				var temp = list[i];
				list[i] = list[j];
				list[j] = temp;
			}
		}

		public static void DebugPrint(byte[][] left, byte[][] right) {
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
		public static List<int> GaussianElimination(byte[][] left, byte[][] right) {
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
			//DebugPrint(left, right);
			//Console.WriteLine();
			ReverseMat(left);
			ReverseMat(right);
			//DebugPrint(left, right);
			//Console.WriteLine();
			Elimination();
			//DebugPrint(left, right);
			//Console.WriteLine();
			ReverseMat(left);
			ReverseMat(right);
			//DebugPrint(left, right);
			//Console.WriteLine();

			int baseIndex = 0;
			for (int i = 0; right[i][0] == 0; i++) baseIndex++;
			if (baseIndex > 0) {
				for (int i = 0; i < colIndexes.Count; i++) {
					Swap(ref left[i], ref left[baseIndex + i]);
				}
			}

			return colIndexes;
		}

		unsafe public static (int[] LeftLinearlyIndependent, int[] RightLinearlyIndependent, int[][] EntangledIndexes, int[][] GroupIndexes) GaussianElimination(ref byte[][] left, ref byte[][] right) {
			void Swap<T>(ref T x, ref T y) {
				var t = x; x = y; y = t;
			}

			void Xor(byte[][] mat, int dstRow, int srcRow) {
				var dst = mat[dstRow];
				var src = mat[srcRow];
				for (int i = 0; i < dst.Length; i++) {
					dst[i] ^= src[i];
				}
			}

			int* rowIndexes = stackalloc int[right.Length];
			for (int i = 0; i < right.Length; i++) {
				rowIndexes[i] = i;
			}
			List<int> colIndexes = new List<int>(right[0].Length);
			int firstRow = 0, firstCol = 0;
			for (; firstRow < right.Length && firstCol < right[0].Length; firstCol++) {
				bool find = false;
				for (int row = firstRow; row < right.Length; row++) {
					if (right[row][firstCol] != 0) {
						Swap(ref rowIndexes[firstRow], ref rowIndexes[row]);
						Swap(ref left[firstRow], ref left[row]);
						Swap(ref right[firstRow], ref right[row]);
						colIndexes.Add(firstCol);
						find = true;
						break;
					}
				}
				if (find) {
					for (int row = firstRow + 1; row < right.Length; row++) {
						if (right[row][firstCol] != 0) {
							Xor(left, row, firstRow);
							Xor(right, row, firstRow);
						}
					}
					firstRow++;
				}
			}

			int[] leftLinearlyIndependent = new int[firstRow];
			for (int i = 0; i < leftLinearlyIndependent.Length; i++) {
				leftLinearlyIndependent[i] = rowIndexes[i];
			}
			int[] rightLinearlyIndependent = colIndexes.ToArray();

			Array.Resize(ref right, firstRow);
			byte[][] newLeft = new byte[firstRow][];
			for (int row = 0; row < firstRow; row++) {
				newLeft[row] = new byte[firstRow];
				for (int col = 0; col < firstRow; col++) {
					newLeft[row][col] = left[row][leftLinearlyIndependent[col]];
				}
			}
			left = newLeft;

			for (int row = firstRow - 1; row >= 0; row--) {
				int col = rightLinearlyIndependent[row];
				for (int r = row - 1; r >= 0; r--) {
					if (right[r][col] != 0) {
						Xor(left, r, row);
						Xor(right, r, row);
					}
				}
			}

			var indexLists = new List<int>[firstRow];
			for (int i = 0; i < indexLists.Length; i++) {
				indexLists[i] = new List<int>();
			}

			for (int i = 1; i < rightLinearlyIndependent.Length; i++) {
				for (int col = rightLinearlyIndependent[i - 1] + 1; col < rightLinearlyIndependent[i]; col++) {
					for (int row = 0; row < i; row++) {
						if (right[row][col] != 0) {
							indexLists[row].Add(col);
						}
					}
				}
			}
			for (int col = rightLinearlyIndependent[firstRow - 1] + 1; col < right[0].Length; col++) {
				for (int row = 0; row < right.Length; row++) {
					if (right[row][col] != 0) {
						indexLists[row].Add(col);
					}
				}
			}

			var entangledIndexes = Array.ConvertAll(indexLists, list => list.ToArray());

			var indexesState = stackalloc bool[firstRow];
			var rowQueue = new Queue<int>();
			var groupIndexList = new List<int[]>();

			for (int rootRow = 0; rootRow < firstRow; rootRow++) {
				if (indexesState[rootRow]) continue;
				indexesState[rootRow] = true;
				var groupList = new List<int>();
				rowQueue.Enqueue(rootRow);
				while (rowQueue.Count > 0) {
					var row0 = rowQueue.Dequeue();
					foreach (var col in entangledIndexes[row0]) {
						var maxRow = Math.Min(firstRow, col);
						for (int row = 0; row < maxRow; row++) {
							if (indexesState[row]) continue;
							if (right[row][col] != 0) {
								indexesState[row] = true;
								rowQueue.Enqueue(row);
							}
						}
					}
					groupList.Add(row0);
				}
				groupIndexList.Add(groupList.ToArray());
			}

			var groupIndexes = groupIndexList.ToArray();

			return (leftLinearlyIndependent, rightLinearlyIndependent, entangledIndexes, groupIndexes);
		}

		unsafe public static ImagePixel[,] GetImagePixel(int version, Bitmap bitmap, int cellSize, int halftoneSize = 0, int deviation = 16) {
			const int RWeight = 19595;
			const int GWeight = 38470;
			const int BWeight = 7471;

			int N = QRInfo.GetN(version);
			bitmap = new Bitmap(bitmap, N * cellSize, N * cellSize);
			var thresholds = HybridBinarizer.GetThreshold(bitmap);
			var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			var p = (uint*)bitmapData.Scan0;
			var W = N * cellSize;
			var result = new ImagePixel[N, N];
			if (halftoneSize <= 0 || halftoneSize > cellSize) halftoneSize = cellSize;
			var margin = (cellSize - halftoneSize) / 2;
			int baseCount = halftoneSize * halftoneSize;

			for (int y = 0; y < N; y++) {
				for (int x = 0; x < N; x++) {
					bool whiteStable = true, blackStable = true;
					int whiteStableMiss = 0, blackStableMiss = 0;
					int graySum = 0;
					int alpha = 0;
					int thresholdSum = 0;
					var px = x * cellSize;
					var py = y * cellSize;
					for (int y0 = margin; y0 < margin + halftoneSize; y0++) {
						for (int x0 = margin; x0 < margin + halftoneSize; x0++) {
							var pix = (byte*)&p[(py + y0) * W + (px + x0)];
							int g = (pix[2] * RWeight + pix[1] * GWeight + pix[0] * BWeight) >> 16;
							int t = thresholds[px + x0, py + y0];

							graySum += g;
							alpha += pix[3];
							thresholdSum += t;

							if (g < t + deviation) whiteStable = false;
							if (g > t - deviation) blackStable = false;
							//if (g < t + deviation) whiteStableMiss++;
							//if (g > t - deviation) blackStableMiss++;
						}
					}

					//if (whiteStableMiss >= baseCount / 2) whiteStable = false;
					//if (blackStableMiss >= baseCount / 2) blackStable = false;

					//int thresholdAvg = thresholdSum / (halftoneSize * halftoneSize);
					//if (thresholdAvg < 100) result[x, y] = ImagePixel.Black;
					//else if (thresholdAvg > 155) result[x, y] = ImagePixel.White;
					//else result[x, y] = graySum > thresholdSum ? ImagePixel.White : ImagePixel.Black;
					//if (graySum < thresholdSum + deviation * halftoneSize * halftoneSize) whiteStable = false;
					//if (graySum > thresholdSum - deviation * halftoneSize * halftoneSize) blackStable = false;

					result[x, y] = graySum > thresholdSum ? ImagePixel.White : ImagePixel.Black;
					if (alpha < 128 * baseCount) result[x, y] = ImagePixel.Any | ImagePixel.White;
					else if (whiteStable || blackStable) result[x, y] |= ImagePixel.Stable;
				}
			}

			bitmap.UnlockBits(bitmapData);
			return result;
		}

		unsafe public static (Bitmap BinarizerBitmap, ImagePixel[,] Pixels) GetBinarizer(int version, Bitmap bitmap, int cellSize, int halftoneSize = 0, int deviation = 5) {
			int N = QRInfo.GetN(version);
			int W = N * cellSize;
			if (halftoneSize <= 0 || halftoneSize > cellSize) halftoneSize = cellSize;
			var margin = (cellSize - halftoneSize) / 2;
			int baseCount = halftoneSize * halftoneSize;
			bitmap = new Bitmap(bitmap, W, W);
			var binarizer = HybridBinarizer.BinarizerBitmap(bitmap);
			var data = bitmap.LockBits(new Rectangle(0, 0, W, W), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
			var binarizerData = binarizer.LockBits(new Rectangle(0, 0, W, W), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
			uint* alpha = (uint*)data.Scan0;
			uint* p = (uint*)binarizerData.Scan0;
			var result = new ImagePixel[N, N];

			for (int y = 0; y < N; y++) {
				for (int x = 0; x < N; x++) {
					int blackSum = 0;
					int alphaSum = 0;
					var px = x * cellSize;
					var py = y * cellSize;
					for (int y0 = margin; y0 < margin + halftoneSize; y0++) {
						for (int x0 = margin; x0 < margin + halftoneSize; x0++) {
							var pix = (byte*)&p[(py + y0) * W + (px + x0)];
							if (pix[0] <= 10) blackSum++;
							alphaSum += ((byte*)&alpha[(py + y0) * W + (px + x0)])[3];
						}
					}

					result[x, y] = blackSum * 2 < baseCount ? ImagePixel.Black : ImagePixel.White;
					if (alphaSum < 128 * baseCount) result[x, y] = ImagePixel.Any | ImagePixel.White;
					else if (Math.Abs(blackSum * 2 - baseCount) >= deviation) result[x, y] |= ImagePixel.Stable;
				}
			}

			bitmap.UnlockBits(data);
			binarizer.UnlockBits(binarizerData);
			return (binarizer, result);
		}

		public static QRCode CreateObfuscatedQRCode(QRCode qrCode, byte[] newData, DataMode mode, ECCLevel eccLevel, MaskVersion maskVersion) {
			var version = qrCode.Version;
			var pixels = new ImagePixel[qrCode.N, qrCode.N];
			for (int y = 0; y < qrCode.N; y++) {
				for (int x = 0; x < qrCode.N; x++) {
					if (qrCode[x, y].IsWhite) {
						pixels[x, y] = ImagePixel.White | ImagePixel.Any;
					} else {
						pixels[x, y] = ImagePixel.Black;
					}
				}
			}

			QRCode newQR;
			try {
				newQR = ImageArt(mode, version, eccLevel, maskVersion, newData, pixels, 0, false);
			} catch (ArgumentOutOfRangeException) {
				return null;
			}
			if (!OverlayAnalyzer.IsOverlay(qrCode.FormatBinary, newQR.FormatBinary)) return null;
			var (format1, format2) = OverlayAnalyzer.FormatOverlay(qrCode.FormatBinary, newQR.FormatBinary);
			qrCode.WriteFormatInformation(format1);
			newQR.WriteFormatInformation(format2);

			bool[,] diff = new bool[qrCode.N, qrCode.N];

			for (int y = 0; y < qrCode.N; y++) {
				for (int x = 0; x < qrCode.N; x++) {
					if (qrCode[x, y].IsBlack && newQR[x, y].IsWhite) {
						diff[x, y] = true;
					}
				}
			}

			var diffArray = Arranger.ToByteArray(newQR.BitmapDataToArray(diff));
			var diffBlocks = Arranger.GetBlocks(version, eccLevel, diffArray);
			var maxError = QRInfo.GetMaxErrorAllowBytes(version, eccLevel);
			foreach (var (data, ecc) in diffBlocks) {
				int error = data.Count(b => b != 0) + ecc.Count(b => b != 0);
				if (error > maxError) goto Fail;
			}

			for (int y = 0; y < diff.GetLength(1); y++) {
				for (int x = 0; x < diff.GetLength(0); x++) {
					if (diff[x, y]) {
						newQR[x, y].Value = true;
					}
				}
			}
			return newQR;

		// 基本上都会失败，这时候要在容错的情况下修改2个QRCode
		Fail:
			var indexBlocks1 = Arranger.GetBlocks<byte>(version, qrCode.ECCLevel);
			var indexBlocks2 = Arranger.GetBlocks<byte>(version, eccLevel);
			var maxError1 = QRInfo.GetMaxErrorAllowBytes(version, qrCode.ECCLevel);
			var maxError2 = maxError;
			var errorCounter1 = stackalloc byte[indexBlocks1.Length];
			var errorCounter2 = stackalloc byte[indexBlocks2.Length];
			var spaceCounter1 = stackalloc byte[indexBlocks1.Length];
			var spaceCounter2 = stackalloc byte[indexBlocks2.Length];

			for (int i = 0; i < indexBlocks1.Length; i++) {
				indexBlocks1[i].Data.AsSpan().Fill((byte)i);
				indexBlocks1[i].Ecc.AsSpan().Fill((byte)i);
				spaceCounter1[i] = (byte)(indexBlocks1[i].Data.Length + indexBlocks1[i].Ecc.Length);
			}
			for (int i = 0; i < indexBlocks2.Length; i++) {
				indexBlocks2[i].Data.AsSpan().Fill((byte)i);
				indexBlocks2[i].Ecc.AsSpan().Fill((byte)i);
				spaceCounter2[i] = (byte)(indexBlocks2[i].Data.Length + indexBlocks2[i].Ecc.Length);
			}

			var random = new Random();
			var indexes1 = Arranger.Interlock(indexBlocks1);
			var indexes2 = Arranger.Interlock(indexBlocks2);
			var data1 = qrCode.DataToArray();
			var data2 = newQR.DataToArray();
			for (int i = 0; i < diffArray.Length; i++) {
				if (diffArray[i] != 0) {
					if ((spaceCounter1[indexes1[i]] < spaceCounter2[indexes2[i]] || errorCounter2[indexes2[i]] >= maxError2) && errorCounter1[indexes1[i]] < maxError1) {
						data1[i] = (byte)(data2[i] & random.Next());
						errorCounter1[indexes1[i]]++;
						spaceCounter1[indexes1[i]]--;
						spaceCounter2[indexes2[i]]--;
					} else if (errorCounter2[indexes2[i]] < maxError2) {
						data2[i] = (byte)(data1[i] | random.Next());
						errorCounter2[indexes2[i]]++;
						spaceCounter1[indexes1[i]]--;
						spaceCounter2[indexes2[i]]--;
					} else {
						return null;
					}
				}
			}

			qrCode.WriteData(data1, false);
			newQR.WriteData(data2, false);

			// 设置padding区域，虽然不重要
			foreach (var (i, x, y) in qrCode.GetPoints(QRValueType.Padding)) {
				if (qrCode[x, y].IsBlack || random.Next(2) == 0) {
					newQR[x, y].Value = true;
				}
			}

			return newQR;
		}

		public static QRCode CreateObfuscatedQRCode(QRCode qrCode, byte[] newData) {
			var mode = DataEncoder.GuessMode(newData);
			while (true) {
				for (int eccLevel = 0; eccLevel < 4; eccLevel++) {
					for (int mask = 0; mask < 8; mask++) {
						var newQR = CreateObfuscatedQRCode(qrCode, newData, mode, (ECCLevel)eccLevel, (MaskVersion)mask);
						if (newQR != null) return newQR;
					}
				}
				if (!NextMode(mode, out mode)) break;
			}
			return null;
		}

		public static (QRCode QR1, QRCode QR2) CreateObfuscatedQRCode(byte[] data1, byte[] data2) {
			for (int version = 1; version <= 40; version++) {
				var mode = DataEncoder.GuessMode(data1);
				while (true) {
					for (int eccLevel = 0; eccLevel < 4; eccLevel++) {
						for (int mask = 0; mask < 8; mask++) {
							try {
								var qr1 = new QRCode(version, mode, data1, (ECCLevel)eccLevel, (MaskVersion)mask);
								var qr2 = CreateObfuscatedQRCode(qr1, data2);
								if (qr2 != null) return (qr1, qr2);
							} catch { }
						}
					}
					if (!NextMode(mode, out mode)) break;
				}
			}
			return default;
		}
	}
}
