using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public class TooManyErrorException : Exception {
		public TooManyErrorException() : base("错误过多无法纠正") { }
	}
}
