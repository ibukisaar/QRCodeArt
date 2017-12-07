using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public sealed class QRPaintEventArgs : EventArgs {
		public double X { get; internal set; }
		public double Y { get; internal set; }
		public double Width { get; internal set; }
		public bool Value { get; internal set; }
	}
}
