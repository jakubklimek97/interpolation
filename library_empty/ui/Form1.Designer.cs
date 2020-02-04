namespace ui
{
    partial class Skalowanie
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.sourceSelectBtn = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.heightBox = new System.Windows.Forms.NumericUpDown();
            this.widthBox = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.asmRadioBtn = new System.Windows.Forms.RadioButton();
            this.cRadioBtn = new System.Windows.Forms.RadioButton();
            this.infoLabel = new System.Windows.Forms.Label();
            this.convertBtn = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.destBtn = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.folderRadioButton = new System.Windows.Forms.RadioButton();
            this.filesRadioButton = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.fileSelectDIalog = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.heightBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.widthBox)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(102, 21);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 17);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // sourceSelectBtn
            // 
            this.sourceSelectBtn.Location = new System.Drawing.Point(3, 140);
            this.sourceSelectBtn.Name = "sourceSelectBtn";
            this.sourceSelectBtn.Size = new System.Drawing.Size(127, 23);
            this.sourceSelectBtn.TabIndex = 1;
            this.sourceSelectBtn.Text = "Wybierz pliki/folder";
            this.sourceSelectBtn.UseVisualStyleBackColor = true;
            this.sourceSelectBtn.Click += new System.EventHandler(this.selectSource);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.heightBox);
            this.groupBox1.Controls.Add(this.widthBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Location = new System.Drawing.Point(92, 56);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 78);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Rozdzielczość";
            // 
            // heightBox
            // 
            this.heightBox.Location = new System.Drawing.Point(80, 44);
            this.heightBox.Maximum = new decimal(new int[] {
            65534,
            0,
            0,
            0});
            this.heightBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.heightBox.Name = "heightBox";
            this.heightBox.Size = new System.Drawing.Size(114, 20);
            this.heightBox.TabIndex = 4;
            this.heightBox.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // widthBox
            // 
            this.widthBox.Location = new System.Drawing.Point(80, 18);
            this.widthBox.Maximum = new decimal(new int[] {
            65534,
            0,
            0,
            0});
            this.widthBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.widthBox.Name = "widthBox";
            this.widthBox.Size = new System.Drawing.Size(114, 20);
            this.widthBox.TabIndex = 3;
            this.widthBox.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Wysokość";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Szerokość";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.asmRadioBtn);
            this.groupBox2.Controls.Add(this.cRadioBtn);
            this.groupBox2.Location = new System.Drawing.Point(298, 56);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(64, 78);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Metoda";
            // 
            // asmRadioBtn
            // 
            this.asmRadioBtn.AutoSize = true;
            this.asmRadioBtn.Checked = true;
            this.asmRadioBtn.Location = new System.Drawing.Point(9, 45);
            this.asmRadioBtn.Name = "asmRadioBtn";
            this.asmRadioBtn.Size = new System.Drawing.Size(48, 17);
            this.asmRadioBtn.TabIndex = 1;
            this.asmRadioBtn.TabStop = true;
            this.asmRadioBtn.Text = "ASM";
            this.asmRadioBtn.UseVisualStyleBackColor = true;
            // 
            // cRadioBtn
            // 
            this.cRadioBtn.AutoSize = true;
            this.cRadioBtn.Location = new System.Drawing.Point(9, 21);
            this.cRadioBtn.Name = "cRadioBtn";
            this.cRadioBtn.Size = new System.Drawing.Size(44, 17);
            this.cRadioBtn.TabIndex = 0;
            this.cRadioBtn.Text = "C++";
            this.cRadioBtn.UseVisualStyleBackColor = true;
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.Location = new System.Drawing.Point(12, 21);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(257, 13);
            this.infoLabel.TabIndex = 4;
            this.infoLabel.Text = "Wybierz folder zawierający obrazy do przeskalowania";
            // 
            // convertBtn
            // 
            this.convertBtn.Enabled = false;
            this.convertBtn.Location = new System.Drawing.Point(266, 140);
            this.convertBtn.Name = "convertBtn";
            this.convertBtn.Size = new System.Drawing.Size(110, 23);
            this.convertBtn.TabIndex = 5;
            this.convertBtn.Text = "Skaluj";
            this.convertBtn.UseVisualStyleBackColor = true;
            this.convertBtn.Click += new System.EventHandler(this.convertBtn_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(3, 169);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(373, 23);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 6;
            // 
            // destBtn
            // 
            this.destBtn.Location = new System.Drawing.Point(136, 140);
            this.destBtn.Name = "destBtn";
            this.destBtn.Size = new System.Drawing.Size(124, 23);
            this.destBtn.TabIndex = 7;
            this.destBtn.Text = "Folder docelowy";
            this.destBtn.UseVisualStyleBackColor = true;
            this.destBtn.Click += new System.EventHandler(this.destBtn_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.folderRadioButton);
            this.groupBox3.Controls.Add(this.filesRadioButton);
            this.groupBox3.Location = new System.Drawing.Point(15, 56);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(71, 78);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Pliki/folder";
            // 
            // folderRadioButton
            // 
            this.folderRadioButton.AutoSize = true;
            this.folderRadioButton.Location = new System.Drawing.Point(7, 44);
            this.folderRadioButton.Name = "folderRadioButton";
            this.folderRadioButton.Size = new System.Drawing.Size(54, 17);
            this.folderRadioButton.TabIndex = 1;
            this.folderRadioButton.Text = "Folder";
            this.folderRadioButton.UseVisualStyleBackColor = true;
            // 
            // filesRadioButton
            // 
            this.filesRadioButton.AutoSize = true;
            this.filesRadioButton.Checked = true;
            this.filesRadioButton.Location = new System.Drawing.Point(7, 20);
            this.filesRadioButton.Name = "filesRadioButton";
            this.filesRadioButton.Size = new System.Drawing.Size(44, 17);
            this.filesRadioButton.TabIndex = 0;
            this.filesRadioButton.TabStop = true;
            this.filesRadioButton.Text = "Pliki";
            this.filesRadioButton.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(286, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Ilość rdzeni: 2";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(286, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(81, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Ilość wątków: 4";
            // 
            // fileSelectDIalog
            // 
            this.fileSelectDIalog.FileName = "fileSelectDIalog";
            this.fileSelectDIalog.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG";
            this.fileSelectDIalog.Multiselect = true;
            // 
            // Skalowanie
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 197);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.destBtn);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.convertBtn);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.sourceSelectBtn);
            this.Name = "Skalowanie";
            this.Text = "Skalowanie";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.heightBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.widthBox)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button sourceSelectBtn;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown heightBox;
        private System.Windows.Forms.NumericUpDown widthBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton asmRadioBtn;
        private System.Windows.Forms.RadioButton cRadioBtn;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.Button convertBtn;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button destBtn;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton folderRadioButton;
        private System.Windows.Forms.RadioButton filesRadioButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.OpenFileDialog fileSelectDIalog;
    }
}

