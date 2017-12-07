using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public sealed class NumericEncoder : QRDataEncoder {
		static readonly int[] RemBits = { 0, 4, 7 };

		public override QRDataMode DataMode => QRDataMode.Numeric;

		protected override int BitsOfDataLength {
			get {
				switch (QRVersion) {
					case var v when v >= 1 && v <= 9: return 10;
					case var v when v >= 10 && v <= 26: return 12;
					case var v when v >= 27 && v <= 40: return 14;
					default: throw new InvalidOperationException($"为什么{nameof(QRCode.Version)}会超过范围？");
				}
			}
		}

		public NumericEncoder(int version, ECCLevel level) : base(version, level) { }

		protected override BitList InternalEncode(byte[] data, int start, int length) {
			int Get(int index) => data[start + index] >= '0' && data[start + index] <= '9' ? data[start + index] - '0' : throw new FormatException();

			var numberOf10Bits = length / 3;
			var result = new BitList(10 * numberOf10Bits + RemBits[length % 3]);
			for (int i = 0; i < numberOf10Bits; i++) {
				int value = Get(i * 3) * 100 + Get(i * 3 + 1) * 10 + Get(i * 3 + 2);
				result.Write(i * 10, value, 10);
			}
			if (length % 3 == 1) {
				result.Write(numberOf10Bits * 10, Get(numberOf10Bits * 3), 4);
			} else if (length % 3 == 2) {
				result.Write(numberOf10Bits * 10, Get(numberOf10Bits * 3) * 10 + Get(numberOf10Bits * 3 + 1), 7);
			}
			return result;
		}
	}
}
