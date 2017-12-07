using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public class QRSurface {
		public double CellWidth { get; set; }
		public QRCode QRCode { get; }
		public double Width => QRCode.N * CellWidth;

		public QRSurface(QRCode qrCode, int cellWidth) {
			QRCode = qrCode;
			CellWidth = cellWidth;
		}

		public void Update() {
			for (int x = 0; x < QRCode.N; x++) {
				for (int y = 0; y < QRCode.N; y++) {
					var e = new QRPaintEventArgs {
						X = x * CellWidth,
						Y = y * CellWidth,
						Value = QRCode[x, y],
						Width = CellWidth
					};
				}
			}
		}
	}
}
