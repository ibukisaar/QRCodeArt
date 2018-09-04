using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public enum QRValueType : byte {
		/// <summary>
		/// 未使用区域
		/// </summary>
		Unused,
		/// <summary>
		/// 固定区域，用来定位
		/// </summary>
		Fixed,
		/// <summary>
		/// 格式区域的固定点
		/// </summary>
		FixedPoint,
		/// <summary>
		/// 定位线
		/// </summary>
		TimingPatterns,
		/// <summary>
		/// 格式区域，
		/// </summary>
		Format,
		/// <summary>
		/// 版本区域
		/// </summary>
		Version,
		/// <summary>
		/// 数据区域
		/// </summary>
		Data,
		/// <summary>
		/// 数据填充
		/// </summary>
		DataPadding,
		/// <summary>
		/// 纠错码
		/// </summary>
		Ecc,
		/// <summary>
		/// 末填充区域
		/// </summary>
		Padding
	}
}
