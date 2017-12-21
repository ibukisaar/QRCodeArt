using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public class QRCode {
		private QRValue[,] valueMap;
		private DataEncoder encoder;
		private (byte[] Data, byte[] Ecc)[] analysisGroup;
		private int formatBinary;
		private int maxErrorAllowBytes;

		public int Version { get; }
		public int N { get; }
		//// <summary>
		/// <code><see cref="ValueMap"/>[x,y]</code>
		/// </summary>
		//public bool[,] ValueMap { get; }
		public DataMode DataMode { get; }
		public ECCLevel ECCLevel { get; }
		public MaskVersion MaskVersion { get; }
		public int FormatBinary => formatBinary;
		public int MaxErrorAllowBytes => maxErrorAllowBytes;
		public (byte[] Data, byte[] Ecc)[] AnalysisGroup => analysisGroup;
		public DataEncoder Encoder => encoder;

		public ref QRValue this[int x, int y] => ref valueMap[x, y];

		public QRCode(int version, DataMode dataMode, byte[] data, ECCLevel eccLevel = ECCLevel.L, MaskVersion maskVersion = MaskVersion.Version000)
			: this(version, dataMode, data, eccLevel, maskVersion, AnalysisType.None) {
		}

		public QRCode(byte[] data, ECCLevel eccLevel = ECCLevel.L, MaskVersion maskVersion = MaskVersion.Version000)
			: this(0, null, data, eccLevel, maskVersion, AnalysisType.None) {
		}

		private QRCode(int version, DataMode? dataMode, byte[] data, ECCLevel eccLevel, MaskVersion maskVersion, AnalysisType analysisType) {
			DataMode = dataMode ?? DataEncoder.GuessMode(data);
			Version = version < 1 || version > 40 ? DataEncoder.GuessVersion(data.Length, eccLevel, DataMode) : version;
			N = QRInfo.GetN(version);
			ECCLevel = eccLevel;
			MaskVersion = maskVersion;

			valueMap = new QRValue[N, N];
			Init(data, analysisType);
		}

		private void Init(byte[] data, AnalysisType analysis) {
			encoder = DataEncoder.CreateEncoder(DataMode, Version, ECCLevel);
			maxErrorAllowBytes = QRInfo.GetMaxErrorAllowBytes(Version, ECCLevel);

			SetPositionDetectionPattern();
			SetAlignmentPatterns();
			SetTimingPatterns();
			SetFormatInformation();
			SetVersionInformation();
			SetFlags(data.Length);

			if (analysis == AnalysisType.ImageArt) {
				analysisGroup = GetMaskGroupBytes(QRInfo.GetTotalBytes(Version, ECCLevel) * 8);
			} else {
				var dataGroups = encoder.Encode(data, 0, data.Length);
				if (analysis == AnalysisType.None) {
					var binary = Arranger.Interlock(dataGroups);
					SetData(binary);
					SetMask();
				} else {
					void Xor(byte[] left, byte[] right) {
						if (left.Length != right.Length) throw new InvalidOperationException();
						for (int i = 0; i < left.Length; i++) {
							left[i] ^= right[i];
						}
					}

					analysisGroup = GetMaskGroupBytes(QRInfo.GetTotalBytes(Version, ECCLevel) * 8);
					for (int i = 0; i < analysisGroup.Length; i++) {
						Xor(analysisGroup[i].Data, dataGroups[i].Data);
						Xor(analysisGroup[i].Ecc, dataGroups[i].Ecc);
					}
				}
			}
		}

		private void SetFlags(int dataLength) {
			var dataTotalBits = encoder.GetDataBitCount(dataLength);
			var dataPaddingTotalBits = QRInfo.GetDataCapacityInfo(Version, ECCLevel).NumberOfDataBytes * 8 - dataTotalBits;
			var eccTotalBits = QRInfo.GetTotalEccBytes(Version, ECCLevel) * 8;

			var dataCounter = 0;
			var dataPaddingCounter = 0;
			var eccCounter = 0;
			foreach (var (i, x, y) in GetPoints(QRValueType.Unused)) {
				if (dataCounter < dataTotalBits) {
					valueMap[x, y].Type = QRValueType.Data;
					dataCounter++;
				} else if (dataPaddingCounter < dataPaddingTotalBits) {
					valueMap[x, y].Type = QRValueType.DataPadding;
					dataPaddingCounter++;
				} else if (eccCounter < eccTotalBits) {
					valueMap[x, y].Type = QRValueType.Ecc;
					eccCounter++;
				} else {
					valueMap[x, y].Type = QRValueType.Padding;
				}
			}
		}

		/// <summary>
		/// 将<see cref="AnalysisGroup"/>写入QR Code中。
		/// </summary>
		public void Flush() {
			if (analysisGroup != null) {
				var data = Arranger.Interlock(analysisGroup);
				SetData(data);
				analysisGroup = null;
			}
		}

		public void WriteData(byte[] data, bool setMask = true) {
			SetData(data);
			if (setMask) SetMask();
		}

		private void SetValue(int x, int y, bool value, QRValueType type) {
			valueMap[x, y].Value = value;
			valueMap[x, y].Type = type;
		}

		private void SetValue(int x, int y, bool value)
			=> valueMap[x, y].Value = value;

		private void CopyRect(byte[,] srcRect, int dstX, int dstY) {
			int n = srcRect.GetLength(0);
			for (int x = dstX; x < dstX + n; x++) {
				if (x < 0 || x >= N) continue;
				for (int y = dstY; y < dstY + n; y++) {
					if (y < 0 || y >= N) continue;
					SetValue(x, y, srcRect[x - dstX, y - dstY] != 0, QRValueType.Fixed);
				}
			}
		}

		private void SetPositionDetectionPattern() {
			var image = new byte[9, 9];
			var template = new byte[5] { 0, 1, 0, 1, 1 };
			for (int x = 0; x < 9; x++) {
				for (int y = 0; y < 9; y++) {
					int i = 4 - Math.Max(Math.Abs(x - 4), Math.Abs(y - 4));
					image[x, y] = template[i];
				}
			}

			CopyRect(image, -1, -1);
			CopyRect(image, -1, N - 8);
			CopyRect(image, N - 8, -1);
		}

		static readonly int[][] AlignmentPatternsPoints = {
			new[]{ 6, 18 },
			new[]{ 6, 22 },
			new[]{ 6, 26 },
			new[]{ 6, 30 },
			new[]{ 6, 34 },
			new[]{ 6, 22, 38 },// 7
			new[]{ 6, 24, 42 },
			new[]{ 6, 26, 46 },
			new[]{ 6, 28, 50 },
			new[]{ 6, 30, 54 },
			new[]{ 6, 32, 58 },
			new[]{ 6, 34, 62 },
			new[]{ 6, 26, 46, 66 }, // 14
			new[]{ 6, 26, 48, 70 },
			new[]{ 6, 26, 50, 74 },
			new[]{ 6, 30, 54, 78 },
			new[]{ 6, 30, 56, 82 },
			new[]{ 6, 30, 58, 86 },
			new[]{ 6, 34, 62, 90 },
			new[]{ 6, 28, 50, 72, 94 }, // 21
			new[]{ 6, 26, 50, 74, 98 },
			new[]{ 6, 30, 54, 78, 102 },
			new[]{ 6, 28, 54, 80, 106 },
			new[]{ 6, 32, 58, 84, 110 },
			new[]{ 6, 30, 58, 86, 114 },
			new[]{ 6, 34, 62, 90, 118 },
			new[]{ 6, 26, 50, 74, 98, 122 }, // 28
			new[]{ 6, 30, 54, 78, 102, 126 },
			new[]{ 6, 26, 52, 78, 104, 130 },
			new[]{ 6, 30, 56, 82, 108, 134 },
			new[]{ 6, 34, 60, 86, 112, 138 },
			new[]{ 6, 30, 58, 86, 114, 142 },
			new[]{ 6, 34, 62, 90, 118, 146 },
			new[]{ 6, 30, 54, 78, 102, 126, 150 }, // 35
			new[]{ 6, 24, 50, 76, 102, 128, 154 },
			new[]{ 6, 28, 54, 80, 106, 132, 158 },
			new[]{ 6, 32, 58, 84, 110, 136, 162 },
			new[]{ 6, 26, 54, 82, 110, 138, 166 },
			new[]{ 6, 30, 58, 86, 114, 142, 170 },
		};

		private void SetAlignmentPatterns() {
			if (Version == 1) return;

			var points = AlignmentPatternsPoints[Version - 2];
			int count = points.Length;
			var image = new byte[5, 5] {
				{ 1,1,1,1,1 },
				{ 1,0,0,0,1 },
				{ 1,0,1,0,1 },
				{ 1,0,0,0,1 },
				{ 1,1,1,1,1 },
			};

			for (int i = 0; i < count; i++) {
				for (int j = 0; j < count; j++) {
					if (i == 0 && j == 0) continue;
					if (i == 0 && j == count - 1) continue;
					if (i == count - 1 && j == 0) continue;
					CopyRect(image, points[i] - 2, points[j] - 2);
				}
			}
		}

		private void SetTimingPatterns() {
			bool value = true;
			for (int x = 8; x < N - 8; x++) {
				if (valueMap[x, 6].Type == QRValueType.Unused) {
					SetValue(x, 6, value, QRValueType.TimingPatterns);
				}
				value = !value;
			}

			value = true;
			for (int y = 8; y < N - 8; y++) {
				if (valueMap[6, y].Type == QRValueType.Unused) {
					SetValue(6, y, value, QRValueType.TimingPatterns);
				}
				value = !value;
			}
		}

		static readonly (int X, int Y)[] FormatInformationPoints1 = { (8, 0), (8, 1), (8, 2), (8, 3), (8, 4), (8, 5), (8, 7), (8, 8), (7, 8), (5, 8), (4, 8), (3, 8), (2, 8), (1, 8), (0, 8) };
		static readonly (int X, int Y)[] FormatInformationPoints2 = { (-1, 8), (-2, 8), (-3, 8), (-4, 8), (-5, 8), (-6, 8), (-7, 8), (-8, 8), (8, -7), (8, -6), (8, -5), (8, -4), (8, -3), (8, -2), (8, -1) };

		private (int X, int Y) GetAbsolutePoint((int X, int Y) point) {
			int x = point.X < 0 ? point.X + N : point.X;
			int y = point.Y < 0 ? point.Y + N : point.Y;
			return (x, y);
		}

		private void SetFormatInformation() {
			const int generator = 0b10100110111;
			const int mask = 0b101010000010010;
			formatBinary = ((int) ECCLevel << 13) | ((int) MaskVersion << 10);
			var bch = GF.PolynomMod(formatBinary, generator);
			formatBinary = (formatBinary | bch) ^ mask;

			WriteFormatInformation(formatBinary);

			var darkModulePoint = GetAbsolutePoint((8, -8));
			SetValue(darkModulePoint.X, darkModulePoint.Y, true, QRValueType.FixedPoint);
		}

		public void WriteFormatInformation(int formatBinary) {
			for (int i = 0; i < 15; i++) {
				bool value = (formatBinary & (1 << i)) != 0;
				var p = GetAbsolutePoint(FormatInformationPoints1[i]);
				SetValue(p.X, p.Y, value, QRValueType.Format);
				p = GetAbsolutePoint(FormatInformationPoints2[i]);
				SetValue(p.X, p.Y, value, QRValueType.Format);
			}
		}

		private void SetVersionInformation() {
			if (Version < 7) return;
			const int generator = 0b1111100100101;
			var bits = Version << 12;
			bits |= GF.PolynomMod(bits, generator);
			for (int j = 0; j < 6; j++) {
				for (int i = 0; i < 3; i++) {
					var value = (bits & (1 << (j * 3 + i))) != 0;
					var rightTopPoint = GetAbsolutePoint((-11 + i, j));
					SetValue(rightTopPoint.X, rightTopPoint.Y, value, QRValueType.Version);
					var leftBottomPoint = GetAbsolutePoint((j, -11 + i));
					SetValue(leftBottomPoint.X, leftBottomPoint.Y, value, QRValueType.Version);
				}
			}
		}

		public IEnumerable<(int I, int X, int Y)> GetPoints(params QRValueType[] types) {
			int flags = 0;
			foreach (var t in types) flags |= 1 << (int) t;

			int i = 0;
			var magicWorld = N * (N - 1);
			int enchantment = (N - 7) / 2;
			for (int magicEnergy = 0; magicEnergy < magicWorld; magicEnergy++) {
				int magic = magicEnergy;
				int pos1 = Math.DivRem(magic, N * 2, out magic);
				int pos2 = Math.DivRem(magic, 2, out magic);
				int x = pos1 < enchantment ? pos1 * 2 + magic : pos1 * 2 + magic + 1;
				int y = pos1 % 2 == 0 ? pos2 : N - 1 - pos2;
				(x, y) = (N - 1 - x, N - 1 - y);
				if (((1 << (int) valueMap[x, y].Type) & flags) != 0) {
					yield return (i, x, y);
					i++;
				}
			}
		}

		public IEnumerable<(int I, int X, int Y)> GetPoints(int dataBits, params QRValueType[] types) {
			foreach (var (i, x, y) in GetPoints(types)) {
				if (i >= dataBits) break;
				yield return (i, x, y);
			}
		}

		public IEnumerable<(int I, int X, int Y)> GetDataRegionPoints()
			=> GetPoints(QRInfo.GetTotalBytes(Version, ECCLevel) * 8, QRValueType.Data, QRValueType.DataPadding, QRValueType.Ecc, QRValueType.Padding, QRValueType.Unused);

		private void SetData(byte[] data) {
			var bits = new BitSet(data);
			foreach (var (i, x, y) in GetPoints(bits.Count, QRValueType.Unused, QRValueType.Data, QRValueType.DataPadding, QRValueType.Ecc)) {
				SetValue(x, y, bits[i]);
			}
		}


		static readonly Func<int, int, bool>[] MaskGenerators = {
			(x, y) => (x + y) % 2 == 0,
			(x, y) => y % 2 == 0,
			(x, y) => x % 3 == 0,
			(x, y) => (x + y) % 3 == 0,
			(x, y) => (x / 3 + y / 2) % 2 == 0,
			(x, y) => (x * y) % 2 + (x * y) % 3 == 0,
			(x, y) => ((x * y) % 2 + (x * y) % 3) % 2 == 0,
			(x, y) => ((x + y) % 2 + (x * y) % 3) % 2 == 0,
		};

		private void SetMask() {
			const int cmp = 1 << (int) QRValueType.Data
				| 1 << (int) QRValueType.DataPadding
				| 1 << (int) QRValueType.Ecc
				| 1 << (int) QRValueType.Padding
				| 1 << (int) QRValueType.Unused
				;

			var generator = MaskGenerators[(int) MaskVersion];
			for (int x = 0; x < N; x++) {
				for (int y = 0; y < N; y++) {
					if (((1 << (int) valueMap[x, y].Type) & cmp) != 0) {
						bool newValue = generator(x, y) != valueMap[x, y].Value;
						SetValue(x, y, newValue);
					}
				}
			}
		}

		public (byte[] Data, byte[] Ecc)[] GetMaskGroupBytes(int dataBitLength) {
			var generator = MaskGenerators[(int) MaskVersion];
			var maskBits = new BitSet(dataBitLength);
			foreach (var (i, x, y) in GetPoints(maskBits.Count, QRValueType.Data, QRValueType.DataPadding, QRValueType.Ecc, QRValueType.Padding, QRValueType.Unused)) {
				maskBits[i] = generator(x, y);
			}
			return Arranger.GetBlocks(Version, ECCLevel, maskBits.ByteArray);
		}

		public T[] BitmapDataToArray<T>(T[,] bitmapData) {
			var result = new T[QRInfo.GetTotalBytes(Version, ECCLevel) * 8];
			foreach (var (i, x, y) in GetDataRegionPoints()) {
				result[i] = bitmapData[x, y];
			}
			return result;
		}

		public static QRCode AnalysisOverlay(int version, DataMode dataMode, byte[] data, ECCLevel eccLevel, MaskVersion maskVersion) {
			return new QRCode(version, dataMode, data, eccLevel, maskVersion, AnalysisType.Overlay);
		}

		public static QRCode AnalysisOverlay(byte[] data, ECCLevel eccLevel, MaskVersion maskVersion) {
			return new QRCode(0, null, data, eccLevel, maskVersion, AnalysisType.Overlay);
		}

		public static QRCode AnalysisImageArt(int version, DataMode dataMode, byte[] data, ECCLevel eccLevel, MaskVersion maskVersion) {
			return new QRCode(version, dataMode, data, eccLevel, maskVersion, AnalysisType.ImageArt);
		}
	}
}
