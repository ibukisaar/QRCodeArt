using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public enum MagicBitType {
		/// <summary>
		/// 自由值。 Data：可以随意赋值（待求解）。ECC：无视，不关心是0还是1。
		/// </summary>
		Freedom,
		/// <summary>
		/// 期望值。 Data：直接设置成Template值。ECC：通过求解Freedom的Data达到期望值。
		/// </summary>
		Expect
	}
}
