using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public static class QRCodeMagician {
		/// <summary>
		/// 创建混淆QR Code，第二个QR Code将藏在第一个里面。
		/// </summary>
		/// <param name="data1"></param>
		/// <param name="data2"></param>
		/// <returns></returns>
		public static (QRCode QRCode1, QRCode QRCode2, bool Swap) CreateObfuscatedQRCode(byte[] data1, byte[] data2) {
			for (int level = 0; level < 4; level++) {
				var eccLevel = (ECCLevel) level;
				var mode1 = QRDataEncoder.GuessMode(data1);
				while (true) {
					var mode2 = QRDataEncoder.GuessMode(data2);
					while (true) {
						var ver1 = QRDataEncoder.GuessVersion(data1.Length, eccLevel, mode1);
						var ver2 = QRDataEncoder.GuessVersion(data2.Length, eccLevel, mode2);
						var ver = Math.Max(ver1, ver2);
						for (; ver <= 40; ver++) {
							for (int mask1 = 0; mask1 < 8; mask1++) {
								var qr1 = QRCode.Analysis(ver, mode1, data1, eccLevel, (MaskVersion) mask1);
								for (int mask2 = 0; mask2 < 8; mask2++) {
									var qr2 = QRCode.Analysis(ver, mode2, data2, eccLevel, (MaskVersion) mask2);
									var foverlay = OverlayAnalyzer.IsOverlay(qr1.FormatBinary, qr2.FormatBinary);
									if (foverlay) {
										var overlay = OverlayAnalyzer.IsOverlay(qr1.AnalysisGroup, qr2.AnalysisGroup, qr1.MaxErrorCorrectionBytes + qr2.MaxErrorCorrectionBytes);
										if (overlay) {
											var (f1, f2) = OverlayAnalyzer.FormatOverlay(qr1.FormatBinary, qr2.FormatBinary);
											OverlayAnalyzer.Overlay(qr1.AnalysisGroup, qr2.AnalysisGroup, qr1.MaxErrorCorrectionBytes, qr2.MaxErrorCorrectionBytes);
											qr1.WriteFormatInformation(f1);
											qr2.WriteFormatInformation(f2);
											// if (!OverlayAnalyzer.IsOverlay(qr1.AnalysisGroup, qr2.AnalysisGroup, 0)) throw new InvalidOperationException();
											qr1.Flush();
											qr2.Flush();
											return (qr1, qr2, false);
										}
									}

									foverlay = OverlayAnalyzer.IsOverlay(qr2.FormatBinary, qr1.FormatBinary);
									if (foverlay) {
										var overlay = OverlayAnalyzer.IsOverlay(qr2.AnalysisGroup, qr1.AnalysisGroup, qr1.MaxErrorCorrectionBytes + qr2.MaxErrorCorrectionBytes);
										if (overlay) {
											var (f2, f1) = OverlayAnalyzer.FormatOverlay(qr2.FormatBinary, qr1.FormatBinary);
											OverlayAnalyzer.Overlay(qr2.AnalysisGroup, qr1.AnalysisGroup, qr2.MaxErrorCorrectionBytes, qr1.MaxErrorCorrectionBytes);
											qr1.WriteFormatInformation(f1);
											qr2.WriteFormatInformation(f2);
											// if (!OverlayAnalyzer.IsOverlay(qr2.AnalysisGroup, qr1.AnalysisGroup, 0)) throw new InvalidOperationException();
											qr1.Flush();
											qr2.Flush();
											return (qr2, qr1, true);
										}
									}
								}
							}
						}
						if (!NextMode(mode2, out mode2)) break;
					}
					if (!NextMode(mode1, out mode1)) break;
				}
			}
			return default;
		}

		static readonly int[] modeIndexes = { -1, 0, 1, 2, -1, -1, -1, -1 };
		static readonly QRDataMode[] sortedModeTable = { QRDataMode.Numeric, QRDataMode.Alphanumeric, QRDataMode.Byte };

		private static bool NextMode(QRDataMode mode, out QRDataMode result) {
			var i = modeIndexes[(int) mode];
			if (i < 0 || i + 1 >= sortedModeTable.Length) {
				result = 0;
				return false;
			}
			result = sortedModeTable[i];
			return true;
		}
	}
}
