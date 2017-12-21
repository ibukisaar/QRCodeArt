using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public enum DataMode : byte {
		ECI = 0b0111,
		Numeric = 0b0001,
		Alphanumeric = 0b0010,
		Byte = 0b0100,
		Kanji = 0b1000,
		StructuredAppend = 0b0011,
		FNC1_First = 0b0101,
		FNC1_Second = 0b1001,
		Terminator = 0b0000,
	}
}
