using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public abstract class DataEncoder {
		protected readonly int Version;
		protected readonly ECCLevel EccLevel;
		public readonly (int NumberOfDataBytes, int Numeric, int Alphanumeric, int Byte, int Kanji) CapacityInfo;
		public readonly (int ECCPerBytes, int BlocksInGroup1, int CodewordsInGroup1, int BlocksInGroup2, int CodewordsInGroup2) ECCInfo;

		public abstract DataMode DataMode { get; }
		protected abstract int BitsOfDataLength { get; }

		public DataEncoder(int version, ECCLevel level) {
			Version = version;
			EccLevel = level;
			CapacityInfo = QRInfo.GetDataCapacityInfo(version, level);
			ECCInfo = QRInfo.GetEccInfo(version, level);
		}

		protected abstract BitSet InternalEncode(byte[] data, int start, int length);

		protected abstract int InternaGetDataBitCount(int dataLength);

		public int GetDataBitCount(int dataLength) {
			var needBits = CapacityInfo.NumberOfDataBytes * 8;
			int validBits = 4 + BitsOfDataLength + InternaGetDataBitCount(dataLength);
			validBits += Math.Min(4, needBits - validBits);
			return validBits;
		}

		public BitSet DataEncode(byte[] data, int start, int length, bool fillPadding = true) {
			var binary = InternalEncode(data, start, length);
			var needBits = CapacityInfo.NumberOfDataBytes * 8;
			int validBits = GetDataBitCount(length);

			var bitResult = new BitSet(needBits);
			bitResult.Write(0, (int) DataMode, 4);
			bitResult.Write(4, length, BitsOfDataLength);
			bitResult.Write(4 + BitsOfDataLength, binary, 0, binary.Count);

			if (fillPadding) {
				var padStart = (validBits + 7) & ~7;
				while (padStart < bitResult.Count) {
					bitResult.Write(padStart, 0b11101100, 8);
					padStart += 8;
					if (padStart < bitResult.Count) {
						bitResult.Write(padStart, 0b00010001, 8);
						padStart += 8;
					}
				}
			}
			return bitResult;
		}

		/// <summary>
		/// 计算纠错码。r(x) = f(x) * x^n (mod gp(x))
		/// <para>r(x) = (ecc[0]*x^(n-1)+ecc[1]*x^(n-2)+...+ecc[n-1]*x^0)</para>
		/// <para>f(x) = (msg[0]*x^(m-1)+msg[1]*x^(m-2)+...+msg[m-1]*x^0)</para>
		/// <para>gp(x) = (x+a^0)(x+a^1)...(x+a^(n-1))</para>
		/// <para>m为消息长度，n为纠错码长度。</para>
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		public static byte[] CalculateECCWords(ArraySegment<byte> array, int eccBytes) {
			return GF.XPolynom.RSEncode(array.Array, array.Offset, array.Count, eccBytes);
		}

		public (byte[] Data, byte[] Ecc)[] Encode(byte[] data, int start, int length, bool fillPadding = true, bool withEcc = true) {
			var encodedData = DataEncode(data, start, length, fillPadding);
			var wordsList = new List<(byte[] Data, byte[] Ecc)>();
			for (int i = 0; i < ECCInfo.BlocksInGroup1; i++) {
				var subData = new ArraySegment<byte>(encodedData.ByteArray, i * ECCInfo.CodewordsInGroup1, ECCInfo.CodewordsInGroup1);
				var eccWords = withEcc ? CalculateECCWords(subData, ECCInfo.ECCPerBytes) : null;
				wordsList.Add((subData.ToArray(), eccWords));
			}
			for (int i = 0; i < ECCInfo.BlocksInGroup2; i++) {
				var subData = new ArraySegment<byte>(encodedData.ByteArray, i * ECCInfo.CodewordsInGroup2, ECCInfo.CodewordsInGroup2);
				var eccWords = withEcc ? CalculateECCWords(subData, ECCInfo.ECCPerBytes) : null;
				wordsList.Add((subData.ToArray(), eccWords));
			}
			return wordsList.ToArray();
		}

		public static DataMode GuessMode(byte[] data) {
			var curr = DataMode.Numeric;
			foreach (var b in data) {
				if (b < 0x20 || b > 'Z' || AlphanumericEncoder.AlphanumericTable[b - 0x20] < 0) {
					return DataMode.Byte;
				} else if (b < '0' || b > '9') {
					curr = DataMode.Alphanumeric;
				}
			}
			return curr;
		}

		public static int GuessVersion(byte[] data, ECCLevel level) => GuessVersion(data.Length, level, GuessMode(data));

		public static int GuessVersion(int dataBytes, ECCLevel level, DataMode mode) {
			int version = 1;
			switch (mode) {
				case DataMode.Numeric:
					for (; version <= 40; version++) {
						if (dataBytes <= QRInfo.GetDataCapacityInfo(version, level).Numeric) return version;
					}
					goto Fail;
				case DataMode.Alphanumeric:
					for (; version <= 40; version++) {
						if (dataBytes <= QRInfo.GetDataCapacityInfo(version, level).Alphanumeric) return version;
					}
					goto Fail;
				case DataMode.Byte:
					for (; version <= 40; version++) {
						if (dataBytes <= QRInfo.GetDataCapacityInfo(version, level).Byte) return version;
					}
					goto Fail;
				case DataMode.Kanji:
					for (; version <= 40; version++) {
						if (dataBytes <= QRInfo.GetDataCapacityInfo(version, level).Kanji) return version;
					}
					goto Fail;
			}
			Fail:
			throw new NotSupportedException("数据过大");
		}

		public static DataEncoder CreateEncoder(DataMode mode, int version, ECCLevel eccLevel) {
			switch (mode) {
				case DataMode.Numeric: return new NumericEncoder(version, eccLevel);
				case DataMode.Alphanumeric: return new AlphanumericEncoder(version, eccLevel);
				case DataMode.Byte: return new ByteEncoder(version, eccLevel);
				default: throw new NotSupportedException($"不支持的模式：{mode}");
			}
		}

		public static DataEncoder CreateEncoder(byte[] data, ECCLevel eccLevel) {
			var mode = GuessMode(data);
			var version = GuessVersion(data.Length, eccLevel, mode);
			return CreateEncoder(mode, version, eccLevel);
		}
	}
}
