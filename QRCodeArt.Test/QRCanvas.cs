using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using QRCodeArt;

namespace QRCodeArt.Test {
	public class QRCanvas : Control {
		static QRCanvas() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(QRCanvas), new FrameworkPropertyMetadata(typeof(QRCanvas)));
		}

		public QRSurface QRSurface { get; set; }

		protected override void OnRender(DrawingContext dc) {
			if (QRSurface == null) return;

			var black = Brushes.Black;
			var white = Brushes.White;

			for (int x = 0; x < QRSurface.QRCode.N; x++) {
				for (int y = 0; y < QRSurface.QRCode.N; y++) {
					Brush brush = QRSurface.QRCode[x, y] ? black : white;
					double canvasX = x * QRSurface.CellWidth;
					double canvasY = y * QRSurface.CellWidth;
					dc.DrawRectangle(brush, null, new Rect(canvasX, canvasY, QRSurface.CellWidth, QRSurface.CellWidth));
				}
			}
		}

		protected override Size MeasureOverride(Size constraint) {
			if (QRSurface == null) return Size.Empty;
			return new Size(QRSurface.Width, QRSurface.Width);
		}
	}
}
