using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	/// <summary>
	/// 针对QR Code优化的超快速RS编码
	/// </summary>
	unsafe public static class RS {
		/// <summary>
		/// 表头
		/// </summary>
		struct Header {
			/// <summary>
			/// 纠错码长度
			/// </summary>
			public int EccLength;
			/// <summary>
			/// 消息的最大长度
			/// </summary>
			public int MaxMessageLength;
			/// <summary>
			/// 纠错码表，Cache[x的次数][系数]
			/// </summary>
			public IntPtr[][] Cache;
		}

		/// <summary>
		/// cacheHeaders[纠错码长度]
		/// </summary>
		readonly static Header[] cacheHeaders = new Header[31];

		static int Align8(int n) => (n + 7) & ~7;

		static void SetHeader(int version, ECCLevel level) {
			var (eccLength, _, msgLength1, _, msgLength2) = QRInfo.GetEccInfo(version, level);
			int msgLength = Math.Max(msgLength1, msgLength2);
			if (cacheHeaders[eccLength].MaxMessageLength < msgLength)
				cacheHeaders[eccLength].MaxMessageLength = msgLength;
		}

		static byte[] CreateGeneratePolynom(int n) {
			var gp = new GF.XPolynom(GF.One, GF.One);
			for (int i = 1; i < n; i++) {
				gp *= new GF.XPolynom(GF.FromExponent(i), GF.One);
			}

			var result = new byte[gp.PolynomsCount - 1];
			for (int i = 0; i < result.Length; i++) {
				result[i] = gp[result.Length - 1 - i].Polynom;
			}
			return result;
		}

		static void IterateEncode(ReadOnlySpan<byte> gp, ReadOnlySpan<byte> msg, Span<byte> ecc) {
			var gfGP = MemoryMarshal.Cast<byte, GF>(gp);
			var gfMsg = MemoryMarshal.Cast<byte, GF>(msg);
			var gfEcc = MemoryMarshal.Cast<byte, GF>(ecc);
			var div = gfMsg[0];
			int i = 0;
			for (; i < gfEcc.Length - 1; i++) {
				gfEcc[i] = gfMsg[i + 1] + div * gfGP[i];
			}
			gfEcc[i] = div * gfGP[i];
		}

		static RS() {
			for (int version = 1; version <= 40; version++) {
				SetHeader(version, ECCLevel.L);
				SetHeader(version, ECCLevel.M);
				SetHeader(version, ECCLevel.Q);
				SetHeader(version, ECCLevel.H);
			}

			for (int i = 0; i < cacheHeaders.Length; i++) {
				cacheHeaders[i].EccLength = i;
				int maxMessageLength = cacheHeaders[i].MaxMessageLength;
				if (maxMessageLength > 0) {
					int eccLength = i;
					int longCount = (eccLength + 7) / 8;
					var buffer = Marshal.AllocHGlobal(maxMessageLength * 256 * longCount * 8);
					ReadOnlySpan<byte> gp = CreateGeneratePolynom(eccLength);
					Span<byte> msg = stackalloc byte[eccLength];
					msg[0] = 1;
					cacheHeaders[i].Cache = new IntPtr[maxMessageLength][];
					for (int msgLength = 1; msgLength <= maxMessageLength; msgLength++) {
						var buffer2 = buffer + (msgLength - 1) * 256 * longCount * 8;
						cacheHeaders[i].Cache[msgLength - 1] = new IntPtr[256];

						new Span<long>((void*)buffer2, longCount).Fill(0); // RS(0)=0
						var pEcc1 = (GF*)(buffer2 + longCount * 8);
						IterateEncode(gp, msg, new Span<byte>(pEcc1, eccLength));

						cacheHeaders[i].Cache[msgLength - 1][0] = buffer2;
						cacheHeaders[i].Cache[msgLength - 1][1] = (IntPtr)pEcc1;

						for (int firstByte = 2; firstByte < 256; firstByte++) {
							GF scale = GF.FromPolynom(firstByte);
							var pEccN = (GF*)(buffer2 + firstByte * longCount * 8);
							for (int j = 0; j < eccLength; j++) {
								pEccN[j] = pEcc1[j] * scale;
							}
							cacheHeaders[i].Cache[msgLength - 1][firstByte] = (IntPtr)pEccN;
						}

						msg = new Span<byte>(pEcc1, eccLength);
					}
				}
			}
		}

		/// <summary>
		/// 纠错码长度 ≤ 8
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="ecc"></param>
		static void Encode1(ReadOnlySpan<byte> msg, Span<byte> ecc) {
			long r = 0;
			var table = cacheHeaders[ecc.Length].Cache;
			for (int i = 0; i < msg.Length; i++) {
				var cache = (long*)table[msg.Length - i - 1][msg[i]];
				r ^= *cache;
			}
			new ReadOnlySpan<byte>(&r, ecc.Length).CopyTo(ecc);
		}

		/// <summary>
		/// 9 ≤ 纠错码长度 ≤ 16
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="ecc"></param>
		static void Encode2(ReadOnlySpan<byte> msg, Span<byte> ecc) {
			long* r = stackalloc long[2];
			var table = cacheHeaders[ecc.Length].Cache;
			for (int i = 0; i < msg.Length; i++) {
				var cache = (long*)table[msg.Length - i - 1][msg[i]];
				r[0] ^= cache[0];
				r[1] ^= cache[1];
			}
			new ReadOnlySpan<byte>(r, ecc.Length).CopyTo(ecc);
		}

		/// <summary>
		/// 17 ≤ 纠错码长度 ≤ 24
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="ecc"></param>
		static void Encode3(ReadOnlySpan<byte> msg, Span<byte> ecc) {
			long* r = stackalloc long[3];
			var table = cacheHeaders[ecc.Length].Cache;
			for (int i = 0; i < msg.Length; i++) {
				var cache = (long*)table[msg.Length - i - 1][msg[i]];
				r[0] ^= cache[0];
				r[1] ^= cache[1];
				r[2] ^= cache[2];
			}
			new ReadOnlySpan<byte>(r, ecc.Length).CopyTo(ecc);
		}

		/// <summary>
		/// 25 ≤ 纠错码长度 ≤ 32
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="ecc"></param>
		static void Encode4(ReadOnlySpan<byte> msg, Span<byte> ecc) {
			long* r = stackalloc long[4];
			var table = cacheHeaders[ecc.Length].Cache;
			for (int i = 0; i < msg.Length; i++) {
				var cache = (long*)table[msg.Length - i - 1][msg[i]];
				r[0] ^= cache[0];
				r[1] ^= cache[1];
				r[2] ^= cache[2];
				r[3] ^= cache[3];
			}
			new ReadOnlySpan<byte>(r, ecc.Length).CopyTo(ecc);
		}

		/// <summary>
		/// 如果<paramref name="msg"/>和<paramref name="ecc"/>的长度范围在QR Code标准中，则使用快速RS算法。
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="ecc"></param>
		public static void Encode(ReadOnlySpan<byte> msg, Span<byte> ecc) {
			if (ecc.Length < cacheHeaders.Length && msg.Length <= cacheHeaders[ecc.Length].MaxMessageLength) {
				switch ((ecc.Length + 7) >> 3) {
					case 1: Encode1(msg, ecc); return;
					case 2: Encode2(msg, ecc); return;
					case 3: Encode3(msg, ecc); return;
					case 4: Encode4(msg, ecc); return;
				}
			}
			// 如果不是QR Code标准中的RS编码，则使用普通的RS编码算法
			GF.XPolynom.RSEncode(msg, ecc);
		}

		public static byte[] Encode(ReadOnlySpan<byte> msg, int eccCount) {
			var ecc = new byte[eccCount];
			Encode(msg, ecc);
			return ecc;
		}

		/// <summary>
		/// 单字节的RS编码
		/// </summary>
		/// <param name="singleByteMsg"></param>
		/// <param name="xExponent"></param>
		/// <param name="ecc"></param>
		public static void Encode(byte singleByteMsg, int xExponent, Span<byte> ecc) {
			if (ecc.Length < cacheHeaders.Length && xExponent + 1 <= cacheHeaders[ecc.Length].MaxMessageLength) {
				var table = cacheHeaders[ecc.Length].Cache;
				var r = (void*)table[xExponent][singleByteMsg];
				new ReadOnlySpan<byte>(r, ecc.Length).CopyTo(ecc);
			} else {
				Span<byte> msg = stackalloc byte[xExponent + 1];
				msg[0] = singleByteMsg;
				GF.XPolynom.RSEncode(msg, ecc);
			}
		}

		/// <summary>
		/// 单字节的RS编码
		/// </summary>
		/// <param name="singleByteMsg"></param>
		/// <param name="xExponent"></param>
		/// <param name="eccCount"></param>
		/// <returns></returns>
		public static byte[] Encode(byte singleByteMsg, int xExponent, int eccCount) {
			var ecc = new byte[eccCount];
			Encode(singleByteMsg, xExponent, ecc);
			return ecc;
		}
	}
}
