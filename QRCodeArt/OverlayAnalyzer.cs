using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Diagnostics;

namespace QRCodeArt {
	public static class OverlayAnalyzer {
		/*
		x y (x&~y) 
		0 0 0
		0 1 0
		1 0 1
		1 1 0

		v x y x' y'
		0 0 0 0  0
		0 0 1 0  0
		0 1 0 0  1
		0 1 1 0  1
		1 0 0 0  1
		1 0 1 1  1
		1 1 0 0  1
		1 1 1 1  1
		*/

		static readonly Random random = new Random();

		static readonly int[] Int5Table = { 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5 };

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int BitCount(int x) => Int5Table[x & 0b11111] + Int5Table[(x >> 5) & 0b11111] + Int5Table[(x >> 10)];

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Overlay(int x, int y) => x & ~y;

		[DebuggerStepThrough]
		private static byte[] Merge((byte[] Data, byte[] Ecc) arr) {
			var result = new byte[arr.Data.Length + arr.Ecc.Length];
			arr.Data.CopyTo(result, 0);
			arr.Ecc.CopyTo(result, arr.Data.Length);
			return result;
		}

		public static bool IsOverlay(int format1, int format2) {
			return BitCount(Overlay(format1, format2)) <= 6;
		}

		public static bool IsOverlay(byte[] data1, byte[] data2, int totalErrorCorrectionBytes) {
			if (data1.Length != data2.Length) throw new InvalidOperationException();

			int missCount = 0;
			for (int i = 0; i < data1.Length; i++) {
				if (Overlay(data1[i], data2[i]) != 0) missCount++;
				if (missCount > totalErrorCorrectionBytes) return false;
			}
			return true;
		}

		public static bool IsOverlay((byte[] Data, byte[] Ecc)[] data1, (byte[] Data, byte[] Ecc)[] data2, int totalErrorCorrectionBytes) {
			for (int i = 0; i < data1.Length; i++) {
				if (!IsOverlay(Merge(data1[i]), Merge(data2[i]), totalErrorCorrectionBytes)) return false;
			}
			return true;
		}

		/// <summary>
		/// 假定可以覆盖
		/// </summary>
		/// <param name="arr1"></param>
		/// <param name="arr2"></param>
		/// <param name="errorCorrectionBytes1"></param>
		public static void Overlay((byte[] Data, byte[] Ecc)[] arr1, (byte[] Data, byte[] Ecc)[] arr2, int errorCorrectionBytes1, int errorCorrectionBytes2) {
			if (arr1.Length != arr2.Length) throw new InvalidOperationException();
			for (int i = 0; i < arr1.Length; i++) {
				var (data1, ecc1) = arr1[i];
				var (data2, ecc2) = arr2[i];
				int miss1 = 0, miss2 = 0;

				if (data1.Length != data2.Length) throw new InvalidOperationException();
				for (int j = 0; j < data1.Length; j++) {
					if (Overlay(data1[j], data2[j]) != 0) {
						byte value = (byte) random.Next(0, 256);
						if (miss1 < errorCorrectionBytes1) {
							data1[j] = (byte) (value & data2[j]);
							miss1++;
						} else if (miss2 < errorCorrectionBytes2) {
							data2[i] = (byte) (value |data1[j]);
							miss2++;
						} else {
							throw new InvalidOperationException();
						}
					}
				}

				if (ecc1.Length != ecc2.Length) throw new InvalidOperationException();
				for (int j = 0; j < ecc1.Length; j++) {
					if (Overlay(ecc1[j], ecc2[j]) != 0) {
						byte value = (byte) random.Next(0, 256);
						if (miss1 < errorCorrectionBytes1) {
							ecc1[j] = (byte) (value & ecc2[j]);
							miss1++;
						} else if (miss2 < errorCorrectionBytes2) {
							ecc2[j] = (byte) (value | ecc1[j]);
							miss2++;
						} else {
							throw new InvalidOperationException();
						}
					}
				}
			}
		}

		public static (int Format1, int Format2) FormatOverlay(int format1, int format2) {
			var bits1 = new BitArray(new[] { format1 });
			var bits2 = new BitArray(new[] { format2 });
			int miss1 = 0, miss2 = 0;
			for (int i = 0; i < 15; i++) {
				if (bits1[i] && !bits2[i]) {
					if (miss1 < 3) {
						bits1[i] = false;
						miss1++;
					} else {
						bits2[i] = true;
						miss2++;
					}
				}
			}
			int[] ret1 = new int[1], ret2 = new int[1];
			bits1.CopyTo(ret1, 0);
			bits2.CopyTo(ret2, 0);
			return (ret1[0], ret2[0]);
		}
	}
}
