namespace QRCodeArt.WinForm {
	partial class FrmView {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.picView = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.picView)).BeginInit();
			this.SuspendLayout();
			// 
			// picView
			// 
			this.picView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.picView.Location = new System.Drawing.Point(0, 0);
			this.picView.Name = "picView";
			this.picView.Size = new System.Drawing.Size(284, 261);
			this.picView.TabIndex = 0;
			this.picView.TabStop = false;
			// 
			// FrmView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.Controls.Add(this.picView);
			this.Name = "FrmView";
			this.Text = "FrmView";
			((System.ComponentModel.ISupportInitialize)(this.picView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.PictureBox picView;
	}
}