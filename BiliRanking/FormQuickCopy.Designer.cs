namespace BiliRanking
{
    partial class FormQuickCopy
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
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.buttonCopy = new System.Windows.Forms.Button();
            this.numericUpDownFpaiming = new System.Windows.Forms.NumericUpDown();
            this.labelNow = new System.Windows.Forms.Label();
            this.labelResult = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFpaiming)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "{时间} {av号} {作者}",
            "{播放}{}{硬币}{}{收藏}{}{弹幕}{}{评论}",
            "{总分}",
            "{tag}",
            "{标题}"});
            this.comboBox1.Location = new System.Drawing.Point(9, 34);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(309, 20);
            this.comboBox1.TabIndex = 0;
            this.comboBox1.Text = "{时间} {av号} {作者}";
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // buttonCopy
            // 
            this.buttonCopy.Location = new System.Drawing.Point(11, 70);
            this.buttonCopy.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(308, 18);
            this.buttonCopy.TabIndex = 1;
            this.buttonCopy.Text = "复制";
            this.buttonCopy.UseVisualStyleBackColor = true;
            this.buttonCopy.Click += new System.EventHandler(this.buttonCopy_Click);
            // 
            // numericUpDownFpaiming
            // 
            this.numericUpDownFpaiming.Location = new System.Drawing.Point(9, 10);
            this.numericUpDownFpaiming.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.numericUpDownFpaiming.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numericUpDownFpaiming.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownFpaiming.Name = "numericUpDownFpaiming";
            this.numericUpDownFpaiming.Size = new System.Drawing.Size(38, 21);
            this.numericUpDownFpaiming.TabIndex = 2;
            this.numericUpDownFpaiming.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownFpaiming.ValueChanged += new System.EventHandler(this.numericUpDownFpaiming_ValueChanged);
            // 
            // labelNow
            // 
            this.labelNow.AutoSize = true;
            this.labelNow.Location = new System.Drawing.Point(51, 14);
            this.labelNow.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelNow.Name = "labelNow";
            this.labelNow.Size = new System.Drawing.Size(53, 12);
            this.labelNow.TabIndex = 3;
            this.labelNow.Text = "当前标题";
            // 
            // labelResult
            // 
            this.labelResult.AutoSize = true;
            this.labelResult.Location = new System.Drawing.Point(9, 55);
            this.labelResult.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelResult.Name = "labelResult";
            this.labelResult.Size = new System.Drawing.Size(29, 12);
            this.labelResult.TabIndex = 4;
            this.labelResult.Text = "预览";
            // 
            // FormQuickCopy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(326, 95);
            this.Controls.Add(this.labelResult);
            this.Controls.Add(this.labelNow);
            this.Controls.Add(this.numericUpDownFpaiming);
            this.Controls.Add(this.buttonCopy);
            this.Controls.Add(this.comboBox1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "FormQuickCopy";
            this.Text = "快速复制ヽ(✿ﾟ▽ﾟ)ノ";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormQuickCopy_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFpaiming)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button buttonCopy;
        private System.Windows.Forms.NumericUpDown numericUpDownFpaiming;
        private System.Windows.Forms.Label labelNow;
        private System.Windows.Forms.Label labelResult;
    }
}