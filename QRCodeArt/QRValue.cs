using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public struct QRValue {
		private byte bits;

		public bool Value {
			get => (bits & 1) != 0;
			set {
				if (value) {
					bits |= 1;
				} else {
					bits &= 0xfe;
				}
			}
		}

		public QRValueType Type {
			get => (QRValueType)(bits >> 1);
			set => bits = (byte)(((int)value << 1) | (bits & 1));
		}
	}
}
