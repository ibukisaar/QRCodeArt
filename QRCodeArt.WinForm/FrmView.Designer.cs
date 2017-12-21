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
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.picView2 = new System.Windows.Forms.PictureBox();
			this.picView3 = new System.Windows.Forms.PictureBox();
			this.picView4 = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.picView)).BeginInit();
			this.flowLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.picView2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.picView3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.picView4)).BeginInit();
			this.SuspendLayout();
			// 
			// picView
			// 
			this.picView.Location = new System.Drawing.Point(3, 3);
			this.picView.Name = "picView";
			this.picView.Size = new System.Drawing.Size(100, 100);
			this.picView.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.picView.TabIndex = 0;
			this.picView.TabStop = false;
			this.picView.Click += new System.EventHandler(this.picView_Click);
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Controls.Add(this.picView);
			this.flowLayoutPanel1.Controls.Add(this.picView2);
			this.flowLayoutPanel1.Controls.Add(this.picView3);
			this.flowLayoutPanel1.Controls.Add(this.picView4);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(619, 361);
			this.flowLayoutPanel1.TabIndex = 1;
			// 
			// picView2
			// 
			this.picView2.Location = new System.Drawing.Point(109, 3);
			this.picView2.Name = "picView2";
			this.picView2.Size = new System.Drawing.Size(100, 100);
			this.picView2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.picView2.TabIndex = 1;
			this.picView2.TabStop = false;
			this.picView2.Click += new System.EventHandler(this.picView_Click);
			// 
			// picView3
			// 
			this.picView3.Location = new System.Drawing.Point(215, 3);
			this.picView3.Name = "picView3";
			this.picView3.Size = new System.Drawing.Size(100, 100);
			this.picView3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.picView3.TabIndex = 2;
			this.picView3.TabStop = false;
			this.picView3.Click += new System.EventHandler(this.picView_Click);
			// 
			// picView4
			// 
			this.picView4.Location = new System.Drawing.Point(321, 3);
			this.picView4.Name = "picView4";
			this.picView4.Size = new System.Drawing.Size(100, 100);
			this.picView4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.picView4.TabIndex = 3;
			this.picView4.TabStop = false;
			this.picView4.Click += new System.EventHandler(this.picView_Click);
			// 
			// FrmView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(619, 361);
			this.Controls.Add(this.flowLayoutPanel1);
			this.Name = "FrmView";
			this.Text = "FrmView";
			((System.ComponentModel.ISupportInitialize)(this.picView)).EndInit();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.picView2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.picView3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.picView4)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.PictureBox picView;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		public System.Windows.Forms.PictureBox picView2;
		public System.Windows.Forms.PictureBox picView3;
		public System.Windows.Forms.PictureBox picView4;
	}
}