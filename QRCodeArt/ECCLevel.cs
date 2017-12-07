namespace QRCodeArt {
	public enum ECCLevel {
		/// <summary>
		/// 可以纠正大约7%的错误
		/// </summary>
		L = 0b01,
		/// <summary>
		/// 可以纠正大约15%的错误
		/// </summary>
		M = 0b00,
		/// <summary>
		/// 可以纠正大约25%的错误
		/// </summary>
		Q = 0b11,
		/// <summary>
		/// 可以纠正大约30%的错误
		/// </summary>
		H = 0b10,
	}
}