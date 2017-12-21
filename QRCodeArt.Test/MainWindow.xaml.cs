using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QRCodeArt.Test {
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();

			byte[] data1 = Encoding.UTF8.GetBytes("BAKA");
			byte[] data2 = Encoding.UTF8.GetBytes("9");
			//var (qr1, qr2, swap) = QRCodeMagician.CreateObfuscatedQRCode(data1, data2);
			//qrCanvas1.QRSurface = new QRSurface(qr1, 5);
			//qrCanvas2.QRSurface = new QRSurface(qr2, 5);

			//var qr = QRCode.Analysis(data1, ECCLevel.M, MaskVersion.Version000);
			//qr.Flush();
			//qrCanvas1.QRSurface = new QRSurface(qr, 5);

			//var qr = new QRCode(15, QRDataMode.Alphanumeric, data1, ECCLevel.L, MaskVersion.Version000);
			//qrCanvas1.QRSurface = new QRSurface(qr, 5);

			
			var pixels = new ImagePixel[QRInfo.GetN(7), QRInfo.GetN(7)];
			var qr = QRCodeMagician.ImageArt(DataMode.Byte, 7, ECCLevel.L, MaskVersion.Version000, data1, pixels);
			qrCanvas1.QRSurface = new QRSurface(qr, 5);
		}
	}
}
