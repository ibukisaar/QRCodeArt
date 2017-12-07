namespace QRCodeArt {
	/// <summary>
	/// <image url="imgs/mask.png" />
	/// </summary>
	public enum MaskVersion {
		/// <summary>
		/// (x+y)%2=0
		/// </summary>
		Version000 = 0b000,
		/// <summary>
		/// y%2=0
		/// </summary>
		Version001 = 0b001,
		/// <summary>
		/// x%3=0
		/// </summary>
		Version010 = 0b010,
		/// <summary>
		/// (x+y)%3=0
		/// </summary>
		Version011 = 0b011,
		/// <summary>
		/// (x/3+y/2)%2=0
		/// </summary>
		Version100 = 0b100,
		/// <summary>
		/// xy%2+xy%3=0
		/// </summary>
		Version101 = 0b101,
		/// <summary>
		/// (xy%2+xy%3)%2=0
		/// </summary>
		Version110 = 0b110,
		/// <summary>
		/// ((x+y)%2+xy%3)%2=0
		/// </summary>
		Version111 = 0b111,
	}
}