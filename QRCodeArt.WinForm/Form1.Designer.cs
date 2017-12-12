namespace QRCodeArt.WinForm {
	partial class Form1 {
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows 窗体设计器生成的代码

		/// <summary>
		/// 设计器支持所需的方法 - 不要修改
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent() {
			this.picTemplate = new System.Windows.Forms.PictureBox();
			this.btnOpen = new System.Windows.Forms.Button();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.btnCreateQR = new System.Windows.Forms.Button();
			this.numVersion = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.cmbEccLevel = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.cmbMask = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.txtData = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.cmbMode = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.numCellSize = new System.Windows.Forms.NumericUpDown();
			this.numHalftone = new System.Windows.Forms.NumericUpDown();
			this.label7 = new System.Windows.Forms.Label();
			this.numMaxError = new System.Windows.Forms.NumericUpDown();
			this.label8 = new System.Windows.Forms.Label();
			this.numWhiteThreshold = new System.Windows.Forms.NumericUpDown();
			this.label9 = new System.Windows.Forms.Label();
			this.numBlackThreshold = new System.Windows.Forms.NumericUpDown();
			this.label10 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.picTemplate)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numVersion)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numCellSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numHalftone)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numMaxError)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numWhiteThreshold)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numBlackThreshold)).BeginInit();
			this.SuspendLayout();
			// 
			// picTemplate
			// 
			this.picTemplate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.picTemplate.Location = new System.Drawing.Point(151, 104);
			this.picTemplate.Name = "picTemplate";
			this.picTemplate.Size = new System.Drawing.Size(150, 150);
			this.picTemplate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.picTemplate.TabIndex = 0;
			this.picTemplate.TabStop = false;
			// 
			// btnOpen
			// 
			this.btnOpen.Location = new System.Drawing.Point(309, 104);
			this.btnOpen.Name = "btnOpen";
			this.btnOpen.Size = new System.Drawing.Size(75, 23);
			this.btnOpen.TabIndex = 1;
			this.btnOpen.Text = "打开图片";
			this.btnOpen.UseVisualStyleBackColor = true;
			this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			// 
			// btnCreateQR
			// 
			this.btnCreateQR.Location = new System.Drawing.Point(517, 234);
			this.btnCreateQR.Name = "btnCreateQR";
			this.btnCreateQR.Size = new System.Drawing.Size(75, 23);
			this.btnCreateQR.TabIndex = 2;
			this.btnCreateQR.Text = "生成二维码";
			this.btnCreateQR.UseVisualStyleBackColor = true;
			this.btnCreateQR.Click += new System.EventHandler(this.btnCreateQR_Click);
			// 
			// numVersion
			// 
			this.numVersion.Location = new System.Drawing.Point(14, 119);
			this.numVersion.Maximum = new decimal(new int[] {
            40,
            0,
            0,
            0});
			this.numVersion.Name = "numVersion";
			this.numVersion.Size = new System.Drawing.Size(121, 21);
			this.numVersion.TabIndex = 3;
			this.numVersion.ValueChanged += new System.EventHandler(this.numVersion_ValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Location = new System.Drawing.Point(12, 104);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(41, 12);
			this.label1.TabIndex = 4;
			this.label1.Text = "版本：";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 143);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(65, 12);
			this.label2.TabIndex = 5;
			this.label2.Text = "纠错等级：";
			// 
			// cmbEccLevel
			// 
			this.cmbEccLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbEccLevel.FormattingEnabled = true;
			this.cmbEccLevel.Items.AddRange(new object[] {
            "L(7%)",
            "M(15%)",
            "Q(25%)",
            "H(30%)"});
			this.cmbEccLevel.Location = new System.Drawing.Point(14, 158);
			this.cmbEccLevel.Name = "cmbEccLevel";
			this.cmbEccLevel.Size = new System.Drawing.Size(121, 20);
			this.cmbEccLevel.TabIndex = 6;
			this.cmbEccLevel.SelectedIndexChanged += new System.EventHandler(this.cmbEccLevel_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 181);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(41, 12);
			this.label3.TabIndex = 7;
			this.label3.Text = "掩码：";
			// 
			// cmbMask
			// 
			this.cmbMask.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbMask.FormattingEnabled = true;
			this.cmbMask.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7"});
			this.cmbMask.Location = new System.Drawing.Point(14, 196);
			this.cmbMask.Name = "cmbMask";
			this.cmbMask.Size = new System.Drawing.Size(121, 20);
			this.cmbMask.TabIndex = 8;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 9);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(41, 12);
			this.label4.TabIndex = 9;
			this.label4.Text = "数据：";
			// 
			// txtData
			// 
			this.txtData.Location = new System.Drawing.Point(59, 6);
			this.txtData.Multiline = true;
			this.txtData.Name = "txtData";
			this.txtData.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtData.Size = new System.Drawing.Size(533, 89);
			this.txtData.TabIndex = 10;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(12, 219);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(65, 12);
			this.label5.TabIndex = 11;
			this.label5.Text = "编码模式：";
			// 
			// cmbMode
			// 
			this.cmbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbMode.FormattingEnabled = true;
			this.cmbMode.Items.AddRange(new object[] {
            "自动",
            "数字",
            "Alphanumerice",
            "字节数组"});
			this.cmbMode.Location = new System.Drawing.Point(14, 234);
			this.cmbMode.Name = "cmbMode";
			this.cmbMode.Size = new System.Drawing.Size(121, 20);
			this.cmbMode.TabIndex = 12;
			this.cmbMode.SelectedIndexChanged += new System.EventHandler(this.cmbMode_SelectedIndexChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(307, 140);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(65, 12);
			this.label6.TabIndex = 13;
			this.label6.Text = "单元大小：";
			// 
			// numCellSize
			// 
			this.numCellSize.Location = new System.Drawing.Point(309, 155);
			this.numCellSize.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
			this.numCellSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numCellSize.Name = "numCellSize";
			this.numCellSize.Size = new System.Drawing.Size(121, 21);
			this.numCellSize.TabIndex = 14;
			this.numCellSize.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
			// 
			// numHalftone
			// 
			this.numHalftone.Location = new System.Drawing.Point(309, 194);
			this.numHalftone.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
			this.numHalftone.Name = "numHalftone";
			this.numHalftone.Size = new System.Drawing.Size(121, 21);
			this.numHalftone.TabIndex = 16;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(307, 179);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(65, 12);
			this.label7.TabIndex = 15;
			this.label7.Text = "halftone：";
			// 
			// numMaxError
			// 
			this.numMaxError.Location = new System.Drawing.Point(309, 233);
			this.numMaxError.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
			this.numMaxError.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
			this.numMaxError.Name = "numMaxError";
			this.numMaxError.Size = new System.Drawing.Size(121, 21);
			this.numMaxError.TabIndex = 18;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(307, 218);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(149, 12);
			this.label8.TabIndex = 17;
			this.label8.Text = "错误码模拟图像最大数量：";
			// 
			// numWhiteThreshold
			// 
			this.numWhiteThreshold.Location = new System.Drawing.Point(471, 158);
			this.numWhiteThreshold.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.numWhiteThreshold.Name = "numWhiteThreshold";
			this.numWhiteThreshold.Size = new System.Drawing.Size(121, 21);
			this.numWhiteThreshold.TabIndex = 22;
			this.numWhiteThreshold.Value = new decimal(new int[] {
            225,
            0,
            0,
            0});
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(469, 143);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(53, 12);
			this.label9.TabIndex = 21;
			this.label9.Text = "白阈值：";
			// 
			// numBlackThreshold
			// 
			this.numBlackThreshold.Location = new System.Drawing.Point(471, 119);
			this.numBlackThreshold.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.numBlackThreshold.Name = "numBlackThreshold";
			this.numBlackThreshold.Size = new System.Drawing.Size(121, 21);
			this.numBlackThreshold.TabIndex = 20;
			this.numBlackThreshold.Value = new decimal(new int[] {
            90,
            0,
            0,
            0});
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(469, 104);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(53, 12);
			this.label10.TabIndex = 19;
			this.label10.Text = "黑阈值：";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(607, 266);
			this.Controls.Add(this.numWhiteThreshold);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.numBlackThreshold);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.numMaxError);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.numHalftone);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.numCellSize);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.cmbMode);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.txtData);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.cmbMask);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.cmbEccLevel);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.numVersion);
			this.Controls.Add(this.btnCreateQR);
			this.Controls.Add(this.btnOpen);
			this.Controls.Add(this.picTemplate);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "Form1";
			this.Text = "QRArt，带透明通道的图像效果最好";
			this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.picTemplate)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numVersion)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numCellSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numHalftone)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numMaxError)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numWhiteThreshold)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numBlackThreshold)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox picTemplate;
		private System.Windows.Forms.Button btnOpen;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button btnCreateQR;
		private System.Windows.Forms.NumericUpDown numVersion;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cmbEccLevel;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox cmbMask;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txtData;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox cmbMode;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown numCellSize;
		private System.Windows.Forms.NumericUpDown numHalftone;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.NumericUpDown numMaxError;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.NumericUpDown numWhiteThreshold;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.NumericUpDown numBlackThreshold;
		private System.Windows.Forms.Label label10;
	}
}

