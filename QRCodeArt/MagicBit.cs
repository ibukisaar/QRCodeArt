using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public struct MagicBit {
		private byte valueBits;

		public bool Value {
			get => (valueBits & 1) != 0;
			//set { if (value) valueBits |= 1; else valueBits &= 0b1111_1110; }
		}

		public MagicBitType Type {
			get => (MagicBitType)(valueBits >> 1);
			//set => valueBits = (byte)((valueBits & 1) | ((byte)value << 1));
		}

		public MagicBit(MagicBitType type, bool value) {
			valueBits = (byte)(((byte)type << 1) | (value ? 1 : 0));
		}

		public override string ToString()
			=> $"{Type}, {(Value ? 1 : 0)}";
	}
}
