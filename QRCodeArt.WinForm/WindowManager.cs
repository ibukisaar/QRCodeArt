using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QRCodeArt.WinForm {
	public static class WindowManager<T> where T : Form, new() {
		private static T form = null;

		public static T Get() {
			if (form == null || form.IsDisposed) form = new T();
			return form;
		}
	}
}
