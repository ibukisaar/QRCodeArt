using System;

namespace QRCodeArt {
	[Flags]
	public enum ImagePixel : byte {
		White = 0,
		Black = 1,
		Any = 1 << 1,
		Stable = 1 << 2
	}
}
