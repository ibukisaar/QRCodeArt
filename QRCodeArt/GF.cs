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

		internal readonly static byte[] AlphaTable = new byte[N + 1];
		internal readonly static byte[] ExponentTable = new byte[N + 1];

		static GF() {
			AlphaTable[1] = 1;
			for (int i = 2; i < N + 1; i++) {
				int t = AlphaTable[i - 1] << 1;
				AlphaTable[i] = (byte)(t > N ? t ^ PrimitivePolynom : t);
			}

			ExponentTable[0] = N;
			for (int i = 1; i < N + 1; i++) {
				ExponentTable[AlphaTable[i]] = (byte)(i - 1);
			}
		}

		public static GF FromExponent(int exponent) => new GF { Polynom = AlphaTable[exponent + 1] };

		public static GF FromPolynom(int polynom) => new GF { Polynom = (byte)polynom };

		public readonly static GF Zero = new GF();
		public readonly static GF One = new GF { Polynom = 1 };

		public byte Polynom;

		public byte Exponent => ExponentTable[Polynom];

		public bool IsZero => Polynom == 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static GF operator +(GF left, GF right)
			=> FromPolynom(left.Polynom ^ right.Polynom);

		public static GF operator *(GF left, GF right) {
			if (left.IsZero || right.IsZero) return Zero;
			var add = left.Exponent + right.Exponent;
			return FromExponent(add >= N ? add - N : add);
		}

		public static GF operator /(GF left, GF right) {
			if (right.IsZero) throw new DivideByZeroException();
			if (left.IsZero) return Zero;
			var sub = N + left.Exponent - right.Exponent;
			return FromExponent(sub >= N ? sub - N : sub);
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
			if (IsZero) return "0";

			bool first = true;
			var sb = new StringBuilder(32);
			sb.Append("a^").Append(Exponent).Append(" = ");
			for (int i = 7; i >= 0; i--) {
				if ((Polynom & (1 << i)) != 0) {
					if (!first) {
						sb.Append('+');
					} else {
						first = false;
					}
					sb.Append("a^").Append(i);
				}
			}
			return sb.ToString();
		}

		public GF Reciprocal {
			get {
				if (IsZero) throw new DivideByZeroException();
				if (Polynom == One.Polynom) return One;
				return FromExponent(N - Exponent);
			}
		}

		/// <summary>
		/// 系数为有限域GF(2^8)元素的多项式
		/// </summary>
		public struct XPolynom {
			private GF[] polynoms;

			public int PolynomsCount => polynoms.Length;

			public bool IsZero => polynoms.Length == 0;

			public XPolynom(int polynomsCount) {
				polynoms = new GF[polynomsCount];
			}

			public XPolynom(params GF[] polynoms) {
				this.polynoms = polynoms;
			}

			public static XPolynom FromXPow(int xExponent) {
				var ret = new XPolynom(xExponent + 1);
				ret[xExponent] = One;
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

			public static XPolynom operator *(GF left, XPolynom right)
				=> right * left;

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

			public GF Apply(GF x) {
				GF result = Zero;
				for (int i = polynoms.Length - 1; i >= 0; i--) {
					result = result * x + polynoms[i];
				}
				return result;
			}

			/// <summary>
			/// 形式导数
			/// </summary>
			public XPolynom Derivative {
				get {
					if (PolynomsCount <= 1) return new XPolynom(0);
					var derivative = new XPolynom(PolynomsCount % 2 == 0 ? PolynomsCount : PolynomsCount - 1);
					for (int i = 1; i < derivative.polynoms.Length; i += 2) {
						derivative.polynoms[i] = polynoms[i];
					}
					return derivative;
				}
			}

			/// <summary>
			/// RS编码
			/// </summary>
			/// <param name="msg"></param>
			/// <param name="msgIndex"></param>
			/// <param name="msgLength"></param>
			/// <param name="eccCount"></param>
			/// <returns></returns>
			public static byte[] RSEncode(byte[] msg, int msgIndex, int msgLength, int eccCount) {
				var gp = Cache<int, XPolynom>.Get(eccCount, n => {
					var g = new XPolynom(One, One);
					for (int i = 1; i < n; i++) {
						g *= new XPolynom(FromExponent(i), One);
					}
					return g;
				});

				byte[] rem = new byte[eccCount];
				Array.Copy(msg, msgIndex, rem, 0, Math.Min(msgLength, eccCount));
				for (int i = 0; i < msgLength; i++) {
					var div = FromPolynom(rem[0]);
					for (int j = 1; j < eccCount; j++) {
						rem[j - 1] = (FromPolynom(rem[j]) + gp[eccCount - j] * div).Polynom;
					}
					if (i + eccCount < msgLength) {
						rem[eccCount - 1] = (FromPolynom(msg[msgIndex + i + eccCount]) + gp[0] * div).Polynom;
					} else {
						rem[eccCount - 1] = (gp[0] * div).Polynom;
					}
				}

				return rem;
			}

			public static void RSEncode(ReadOnlySpan<byte> msg, Span<byte> ecc) {
				int eccCount = ecc.Length;
				var gp = Cache<int, XPolynom>.Get(eccCount, n => {
					var g = new XPolynom(One, One);
					for (int i = 1; i < n; i++) {
						g *= new XPolynom(FromExponent(i), One);
					}
					return g;
				});

				if (msg.Length > ecc.Length) {
					msg.Slice(0, ecc.Length).CopyTo(ecc);
				} else {
					msg.CopyTo(ecc);
					ecc.Slice(msg.Length).Fill(0);
				}
				for (int i = 0; i < msg.Length; i++) {
					var div = FromPolynom(ecc[0]);
					for (int j = 1; j < eccCount; j++) {
						ecc[j - 1] = (FromPolynom(ecc[j]) + gp[eccCount - j] * div).Polynom;
					}
					if (i + eccCount < msg.Length) {
						ecc[eccCount - 1] = (FromPolynom(msg[i + eccCount]) + gp[0] * div).Polynom;
					} else {
						ecc[eccCount - 1] = (gp[0] * div).Polynom;
					}
				}
			}

			public static (byte Index, byte Error)[] RSDecode(ReadOnlySpan<byte> msg, ReadOnlySpan<byte> ecc) {
				var h = new XPolynom(msg.Length + ecc.Length);
				{
					int i = 0;
					for (int j = ecc.Length - 1; j >= 0; j--) {
						h[i++] = FromPolynom(ecc[j]);
					}
					for (int j = msg.Length - 1; j >= 0; j--) {
						h[i++] = FromPolynom(msg[j]);
					}
				}

				bool noError = true;
				Span<GF> s = stackalloc GF[ecc.Length];
				for (int i = 0; i < s.Length; i++) {
					s[i] = h.Apply(FromExponent(i));
					if (!s[i].IsZero) noError = false;
				}
				if (noError) return null;

				var sPolynom = new XPolynom(s.Length);
				for (int i = 0; i < s.Length; i++) {
					sPolynom[i] = s[i];
				}

				var errCoefficient = BerlekampMassey(s);
				var errIndexPolynom = new XPolynom(errCoefficient.Length + 1);
				errIndexPolynom[0] = One;
				for (int i = 1; i < errIndexPolynom.PolynomsCount; i++) {
					errIndexPolynom[i] = errCoefficient[i - 1];
				}

				Span<GF> errX = stackalloc GF[errCoefficient.Length];
				var errCount = 0;
				for (int i = Math.Max(N + 1 - h.PolynomsCount, 1); i < N; i++) {
					GF x = FromExponent(i);
					if (errIndexPolynom.Apply(x).IsZero) {
						errX[errCount++] = x;
					}
				}
				if (errIndexPolynom.Apply(One).IsZero) {
					errX[errCount++] = One;
				}

				if (errCount != errCoefficient.Length)
					throw new TooManyErrorException();

				var keyPolynom = (sPolynom * errIndexPolynom) % FromXPow(ecc.Length);
				var denPolynom = errIndexPolynom.Derivative;

				var result = new (byte Index, byte Error)[errX.Length];
				for (int i = 0; i < result.Length; i++) {
					result[i] = ((byte)(h.PolynomsCount - 1 - errX[i].Reciprocal.Exponent), (keyPolynom.Apply(errX[i]) / denPolynom.Apply(errX[i])).Polynom);
				}
				return result;
			}

			public static GF[] BerlekampMassey(ReadOnlySpan<GF> s) {
				var Rs = new GF[s.Length + 1][];
				Rs[0] = Array.Empty<GF>();
				var deltas = new GF[s.Length];
				var fails = new List<int>();

				for (int i = 0; i < s.Length; i++) {
					GF sum = Zero;
					var R = Rs[i];
					for (int j = 0; j < R.Length; j++) {
						sum += R[j] * s[i - 1 - j];
					}
					deltas[i] = s[i] + sum;
					if (deltas[i].IsZero) {
						Rs[i + 1] = R;
					} else {
						if (fails.Count == 0) {
							Rs[i + 1] = new[] { FromPolynom(0) };
						} else {
							var fail = fails[fails.Count - 1];
							var mul = deltas[i] / deltas[fail];
							var lastChangedR = Rs[fails.Count - 1];
							var nextR = Rs[i + 1] = new GF[i - fail + lastChangedR.Length];
							nextR[i - fail - 1] = mul;
							for (int j = 0; j < lastChangedR.Length; j++) {
								nextR[i - fail + j] = mul * lastChangedR[j];
							}
							for (int j = 0; j < Rs[i].Length; j++) {
								nextR[j] += Rs[i][j];
							}
						}
						fails.Add(i);
					}
				}

				return Rs[s.Length];
			}
		}
	}
}
