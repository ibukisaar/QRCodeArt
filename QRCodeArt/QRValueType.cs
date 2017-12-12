using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public enum QRValueType {
		Unused,
		Fixed,
		FixedPoint,
		TimingPatterns,
		Format,
		Version,
		Data,
		DataPadding,
		Ecc,
		Padding
	}
}
