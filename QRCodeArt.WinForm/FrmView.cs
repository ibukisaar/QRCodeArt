using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QRCodeArt.WinForm {
	public partial class FrmView : Form {
		public FrmView() {
			InitializeComponent();
		}

		private void picView_Click(object sender, EventArgs e) {
			var result = saveFileDialog1.ShowDialog();
			if (result == DialogResult.OK) {
				((sender as PictureBox).Image as Bitmap).Save(saveFileDialog1.FileName);
			}
		}
	}
}
