using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public static class Arranger {
		//public class Info<T> {
		//	public int X;
		//	public int Y;
		//	public int BlockIndex;
		//	public int DataByteIndex;
		//	public int EccByteIndex;
		//	public int BitIndex;
		//	public T UserData;
		//}

		public static bool[] ToBitArray(byte[] byteArray) {
			var result = new bool[byteArray.Length * 8];
			var bits = new BitSet(byteArray);
			for (int i = 0; i < result.Length; i++) {
				result[i] = bits[i];
			}
			return result;
		}

		public static (bool[] Data, bool[] Ecc)[] ToBitBlocks(IReadOnlyList<(byte[] Data, byte[] Ecc)> input) {
			var result = new (bool[] Data, bool[] Ecc)[input.Count];
			for (int i = 0; i < result.Length; i++) {
				result[i] = (ToBitArray(input[i].Data), ToBitArray(input[i].Ecc));
			}
			return result;
		}

		public static byte[] ToByteArray(bool[] bitArray) {
			return new BitSet(bitArray).ByteArray;
		}

		public static (byte[] Data, byte[] Ecc)[] ToByteBlocks(IReadOnlyList<(bool[] Data, bool[] Ecc)> input) {
			var result = new (byte[] Data, byte[] Ecc)[input.Count];
			for (int i = 0; i < result.Length; i++) {
				result[i] = (ToByteArray(input[i].Data), ToByteArray(input[i].Ecc));
			}
			return result;
		}

		public static IEnumerable<int> GetBlockGroupEnumerable(int blocks1, int words1, int blocks2, int words2, int eccNum) {
			var sumBlocks = blocks1 + blocks2;
			var maxWords = Math.Max(words1, words2);
			for (int wordIndex = 0; wordIndex < maxWords; wordIndex++) {
				if (wordIndex < words1) {
					for (int blockIndex = 0; blockIndex < blocks1; blockIndex++) {
						yield return blockIndex;
					}
				}
				if (wordIndex < words2) {
					for (int blockIndex = blocks1; blockIndex < sumBlocks; blockIndex++) {
						yield return blockIndex;
					}
				}
			}
			for (int eccIndex = 0; eccIndex < eccNum; eccIndex++) {
				for (int blockIndex = 0; blockIndex < sumBlocks; blockIndex++) {
					yield return blockIndex;
				}
			}
		}

		public static (T[] Data, T[] Ecc)[] GetBlocks<T>(int blocks1, int words1, int blocks2, int words2, int eccNum, int step, IEnumerable<T> dataIterator = null, bool withEcc = true) {
			var sumBlocks = blocks1 + blocks2;
			var maxWords = Math.Max(words1, words2);
			var result = new (T[] Data, T[] Ecc)[sumBlocks];
			for (int i = 0; i < blocks1; i++)
				result[i] = (new T[words1], withEcc ? new T[eccNum] : null);
			for (int i = blocks1; i < sumBlocks; i++)
				result[i] = (new T[words2], withEcc ? new T[eccNum] : null);

			if (dataIterator != null) {
				using (var enumer = dataIterator.GetEnumerator()) {
					for (int j = 0; j < maxWords; j += step) {
						for (int i = 0; i < sumBlocks; i++) {
							if (j >= result[i].Data.Length) continue;
							for (int k = 0; k < step; k++) {
								if (!enumer.MoveNext()) return result;
								result[i].Data[j + k] = enumer.Current;
							}
						}
					}
					if (withEcc) {
						for (int j = 0; j < eccNum; j += step) {
							for (int i = 0; i < sumBlocks; i++) {
								for (int k = 0; k < step; k++) {
									if (!enumer.MoveNext()) return result;
									result[i].Ecc[j + k] = enumer.Current;
								}
							}
						}
					}
				}
			}

			return result;
		}

		public static (T[] Data, T[] Ecc)[] GetBlocks<T>(int version, ECCLevel level, IEnumerable<T> dataIterator = null, bool withEcc = true) {
			var info = QRInfo.GetEccInfo(version, level);
			return GetBlocks(info.BlocksInGroup1, info.CodewordsInGroup1, info.BlocksInGroup2, info.CodewordsInGroup2, info.ECCPerBytes, 1, dataIterator, withEcc);
		}

		public static (T[] Data, T[] Ecc)[] GetBitBlocks<T>(int version, ECCLevel level, IEnumerable<T> dataIterator = null, bool withEcc = true) {
			var info = QRInfo.GetEccInfo(version, level);
			return GetBlocks(info.BlocksInGroup1, info.CodewordsInGroup1 * 8, info.BlocksInGroup2, info.CodewordsInGroup2 * 8, info.ECCPerBytes * 8, 8, dataIterator, withEcc);
		}

		public static T[] Interlock<T>(IEnumerable<(T[] Data, T[] Ecc)> blocks) {
			var totalCount = blocks.Sum(p => p.Data.Length + p.Ecc.Length);
			var result = new T[totalCount];
			int index = 0;
			for (int i = 0; ; i++) {
				int temp = index;
				foreach (var (data, ecc) in blocks) {
					if (i < data.Length) result[index++] = data[i];
				}
				if (temp == index) break;
			}
			for (int i = 0, len = blocks.First().Ecc.Length; i < len; i++) {
				foreach (var (data, ecc) in blocks) {
					result[index++] = ecc[i];
				}
			}
			return result;
		}
	}
}
