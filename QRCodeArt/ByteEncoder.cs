using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public sealed class ByteEncoder : DataEncoder {
		public override DataMode DataMode => DataMode.Byte;

		protected override int BitsOfDataLength {
			get {
				switch (Version) {
					case var v when v >= 1 && v <= 9: return 8;
					case var v when v >= 10 && v <= 40: return 16;
					default: throw new InvalidOperationException($"为什么{nameof(QRCode.Version)}会超过范围？");
				}
			}
		}

		public ByteEncoder(int version, ECCLevel level) : base(version, level) { }

		protected override int InternaGetDataBitCount(int dataLength) {
			return dataLength * 8;
		}

		protected override BitSet InternalEncode(byte[] data, int start, int length) {
			var newData = new byte[length];
			Array.Copy(data, start, newData, 0, length);
			return new BitSet(newData);
		}
	}
}
