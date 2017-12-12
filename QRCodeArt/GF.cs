using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	/// <summary>
	/// 代表有限域GF(2^8)里的元素
	/// </summary>
	public struct GF {
		/// <summary>
		/// 本原多项式: x^8+x^4+x^3+x^2+1
		/// </summary>
		private const int PrimitivePolynom = 0b100011101;
		/// <summary>
		/// 有限域GF(2^8)乘法群的阶
		/// </summary>
		private const int N = 255;

		//private const int PrimitivePolynom = 0b1011;
		//private const int N = 7;

		//private const int PrimitivePolynom = 0b10011;
		//private const int N = 15;

		private static int[] AlphaTable = new int[N + 1];
		private static int[] ExponentTable = new int[N + 1];

		static GF() {
			AlphaTable[1] = 1;
			for (int i = 2; i < N + 1; i++) {
				AlphaTable[i] = AlphaTable[i - 1] << 1;
				if (AlphaTable[i] > N) {
					AlphaTable[i] ^= PrimitivePolynom;
				}
			}

			ExponentTable[0] = -1;
			for (int i = 1; i < N + 1; i++) {
				ExponentTable[AlphaTable[i]] = i - 1;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static GF FromExponent(int exponent) => new GF { Polynom = AlphaTable[exponent + 1] };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static GF FromPolynom(int polynom) => new GF { Polynom = polynom };

		public readonly static GF Zero = new GF();

		public int Polynom;

		public int Exponent => ExponentTable[Polynom];

		public bool IsZero => Polynom == 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static GF operator +(GF left, GF right)
			=> FromPolynom(left.Polynom ^ right.Polynom);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static GF operator *(GF left, GF right) {
			if (left.IsZero || right.IsZero) return Zero;
			var add = left.Exponent + right.Exponent;
			return FromExponent(add % N);
		}

		public static GF operator /(GF left, GF right) {
			if (right.IsZero) throw new DivideByZeroException();
			if (left.IsZero) return Zero;
			var sub = N + left.Exponent - right.Exponent;
			return FromExponent(sub % N);
		}

		public GF Pow(int n) {
			if (IsZero) return Zero;
			return FromExponent(Exponent * n % N);
		}

		public static int PolynomMod(int left, int right) {
			int BitLength(int val) {
				int len = 32;
				while (len > 0 && (val & (1 << (len - 1))) == 0) len--;
				return len;
			}

			var rlen = BitLength(right);
			if (rlen == 0) throw new DivideByZeroException();
			for (int i = BitLength(left) - rlen; i >= 0; i--) {
				if ((left & (1 << (i + rlen - 1))) != 0) {
					left ^= right << i;
				}
			}
			return left;
		}

		public override string ToString() {
			var sb = new StringBuilder(32);
			sb.Append("a^").Append(Exponent).Append(" = ");
			for (int i = 7; i >= 0; i--) {
				if ((Polynom & (1 << i)) != 0) {
					if (sb.Length > 0) {
						sb.Append('+');
					}
					sb.Append("a^").Append(i);
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// 系数为有限域GF(2^8)元素的多项式
		/// </summary>
		public struct XPolynom {
			private GF[] polynoms;

			public int PolynomsCount => polynoms.Length;

			public XPolynom(int polynomsCount) {
				polynoms = new GF[polynomsCount];
			}

			public XPolynom(params GF[] polynoms) {
				this.polynoms = polynoms;
			}

			public static XPolynom FromXPow(int xExponent) {
				var ret = new XPolynom(xExponent + 1);
				ret[xExponent] = FromExponent(0);
				return ret;
			}

			private void ReLength() {
				int len = PolynomsCount;
				while (len > 0 && this[len - 1].Polynom == 0) len--;
				if (len < PolynomsCount) {
					Array.Resize(ref polynoms, len);
				}
			}

			public XPolynom Clone() {
				var ret = new XPolynom(PolynomsCount);
				Array.Copy(polynoms, ret.polynoms, PolynomsCount);
				return ret;
			}

			public ref GF this[int index] => ref polynoms[index];

			public override string ToString() {
				var sb = new StringBuilder(256);
				for (int i = PolynomsCount - 1; i >= 0; i--) {
					if (this[i].Polynom != 0) {
						if (sb.Length > 0) {
							sb.Append(" + ");
						}
						sb.Append("a^").Append(this[i].Exponent).Append("*x^").Append(i);
					}
				}
				return sb.ToString();
			}

			public XPolynom MulXPow(int xExponent) {
				if (xExponent == 0) return this;
				var ret = new XPolynom(PolynomsCount + xExponent);
				polynoms.CopyTo(ret.polynoms, xExponent);
				return ret;
			}

			public static XPolynom operator +(XPolynom left, XPolynom right) {
				XPolynom @short, @long;
				if (left.PolynomsCount > right.PolynomsCount) {
					@long = left; @short = right;
				} else {
					@short = left; @long = right;
				}

				var ret = new XPolynom(@long.PolynomsCount);
				int i = 0;
				for (; i < @short.PolynomsCount; i++) {
					ret[i] = @long[i] + @short[i];
				}
				for (; i < @long.PolynomsCount; i++) {
					ret[i] = @long[i];
				}
				ret.ReLength();
				return ret;
			}

			public static XPolynom operator *(XPolynom left, GF right) {
				var ret = new XPolynom(left.PolynomsCount);
				for (int i = 0; i < left.PolynomsCount; i++) {
					ret[i] = left[i] * right;
				}
				ret.ReLength();
				return ret;
			}

			public static XPolynom operator *(XPolynom left, XPolynom right) {
				var ret = new XPolynom(left.PolynomsCount + right.PolynomsCount - 1);
				for (int i = 0; i < left.PolynomsCount; i++) {
					for (int j = 0; j < right.PolynomsCount; j++) {
						ret[i + j] += left[i] * right[j];
					}
				}
				return ret;
			}

			public XPolynom DivMod(XPolynom right, out XPolynom rem) {
				var divVal = new XPolynom(0);
				var remVal = this;
				var rightHead = right[right.PolynomsCount - 1];
				while (true) {
					var xExponent = remVal.PolynomsCount - right.PolynomsCount;
					if (xExponent < 0) break;
					var alphaDiv = remVal[remVal.PolynomsCount - 1] / rightHead;
					divVal += new XPolynom(alphaDiv).MulXPow(xExponent);
					remVal += right.MulXPow(xExponent) * alphaDiv;
				}
				rem = remVal;
				return divVal;
			}

			public static XPolynom operator %(XPolynom left, XPolynom right) {
				left.DivMod(right, out XPolynom r);
				return r;
			}

			public static XPolynom operator /(XPolynom left, XPolynom right) {
				return left.DivMod(right, out _);
			}
			
			/// <summary>
			/// 快速RS编码
			/// </summary>
			/// <param name="msg"></param>
			/// <param name="msgIndex"></param>
			/// <param name="msgLength"></param>
			/// <param name="eccCount"></param>
			/// <returns></returns>
			public static byte[] RSEncode(byte[] msg, int msgIndex, int msgLength, int eccCount) {
				var gp = Cache<int, XPolynom>.Get(eccCount, n => {
					var g = new XPolynom(FromExponent(0), FromExponent(0));
					for (int i = 1; i < n; i++) {
						g *= new XPolynom(FromExponent(i), FromExponent(0));
					}
					return g;
				});

				byte[] rem = new byte[eccCount];
				Array.Copy(msg, msgIndex, rem, 0, Math.Min(msgLength, eccCount));
				for (int i = 0; i < msgLength; i++) {
					var div = FromPolynom(rem[0]);
					for (int j = 1; j < eccCount; j++) {
						rem[j - 1] = (byte) (FromPolynom(rem[j]) + gp[eccCount - j] * div).Polynom;
					}
					if (i + eccCount < msgLength) {
						rem[eccCount - 1] = (byte) (FromPolynom(msg[msgIndex + i + eccCount]) + gp[0] * div).Polynom;
					} else {
						rem[eccCount - 1] = (byte) (gp[0] * div).Polynom;
					}
				}

				return rem;
			}
		}
	}
}
