using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public sealed class BitSet : IEnumerable {
		static readonly byte[] ReverseTable;

		static BitSet() {
			ReverseTable = new byte[256];
			for (int i = 0; i < 256; i++) {
				ReverseTable[i] = (byte) ((i * 0x0202020202L & 0x010884422010L) % 1023);
			}
		}


		static int GetByteCount(int bitCount) => (bitCount + 7) >> 3;

		private byte[] values;
		private int bitCount;

		public bool this[int i] {
			get => i >= 0 && i < bitCount ? (values[i >> 3] & (1 << (7 - (i & 7)))) != 0 : throw new ArgumentOutOfRangeException();
			set {
				if (!(i >= 0 && i < bitCount)) throw new ArgumentOutOfRangeException();
				if (value) {
					values[i >> 3] |= (byte) (1 << (7 - (i & 7)));
				} else {
					values[i >> 3] &= (byte) ~(1 << (7 - (i & 7)));
				}
			}
		}

		public int Count => bitCount;

		public byte[] ByteArray => values;

		public BitSet(int bitCount) {
			values = new byte[GetByteCount(bitCount)];
			this.bitCount = bitCount;
		}

		public BitSet(byte[] bitBytes) {
			values = bitBytes;
			bitCount = values.Length * 8;
		}

		public BitSet(bool[] allValue) {
			values = new byte[(allValue.Length + 7) >> 3];
			bitCount = allValue.Length;
			for (int i = 0; i < bitCount; i++) this[i] = allValue[i];
		}

		public IEnumerator GetEnumerator() {
			for (int i = 0; i < bitCount; i++) {
				yield return this[i];
			}
		}

		public void Resize(int newBitCount) {
			if (newBitCount != bitCount) {
				bitCount = newBitCount;
				Array.Resize(ref values, GetByteCount(newBitCount));
			}
		}

		public void Write(int dstIndex, BitSet src, int srcIndex, int srcCount) {
			if (dstIndex + srcCount > bitCount || srcIndex + srcCount > src.bitCount) throw new ArgumentOutOfRangeException();

			for (int i = 0; i < srcCount; i++) {
				this[dstIndex + i] = src[srcIndex + i];
			}
		}

		public void Write(int dstIndex, int src, int srcBitCount) {
			if (dstIndex + srcBitCount > bitCount) throw new ArgumentOutOfRangeException();

			for (int i = 0; i < srcBitCount; i++) {
				this[dstIndex + i] = (src & (1 << (srcBitCount - 1 - i))) != 0;
			}
		}

		public void ReverseAllByte() {
			for (int i = 0; i < values.Length; i++) {
				values[i] = ReverseTable[values[i]];
			}
		}
	}
}
