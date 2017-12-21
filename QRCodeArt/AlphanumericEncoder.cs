using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public sealed class AlphanumericEncoder : DataEncoder {
		public static readonly int[] AlphanumericTable = {
			   36,   -1,   -1,   -1,   37,   38,   -1,   -1,   -1,   -1,   39,   40,   -1,   41,   42,   43,
				0,    1,    2,    3,    4,    5,    6,    7,    8,    9,   44,   -1,   -1,   -1,   -1,   -1,
			   -1,   10,   11,   12,   13,   14,   15,   16,   17,   18,   19,   20,   21,   22,   23,   24,
			   25,   26,   27,   28,   29,   30,   31,   32,   33,   34,   35
		};

		public override DataMode DataMode => DataMode.Alphanumeric;

		protected override int BitsOfDataLength {
			get {
				switch (Version) {
					case var v when v >= 1 && v <= 9: return 9;
					case var v when v >= 10 && v <= 26: return 11;
					case var v when v >= 27 && v <= 40: return 13;
					default: throw new InvalidOperationException($"为什么{nameof(QRCode.Version)}会超过范围？");
				}
			}
		}

		public AlphanumericEncoder(int version, ECCLevel level) : base(version, level) { }

		protected override int InternaGetDataBitCount(int dataLength) {
			return dataLength / 2 * 11 + dataLength % 2 * 6;
		}

		protected override BitSet InternalEncode(byte[] data, int start, int length) {
			int Get(int i) {
				var b = data[start + i];
				if (b < 0x20 || b > 'Z' || AlphanumericTable[b - 0x20] < 0) throw new NotSupportedException();
				return AlphanumericTable[b - 0x20];
			}

			var doubleLength = length / 2;
			var result = new BitSet(InternaGetDataBitCount(length));
			for (int i = 0; i < doubleLength; i++) {
				int value = Get(2 * i) * 45 + Get(2 * i + 1);
				result.Write(i * 11, value, 11);
			}
			if (length % 2 != 0) {
				result.Write(doubleLength * 11, Get(2 * doubleLength), 6);
			}
			return result;
		}
	}
}
