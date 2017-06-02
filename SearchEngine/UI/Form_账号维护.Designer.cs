namespace 搜索引擎自动营销助手V1._0.UI
{
    partial class Form_账号维护
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
            this.textBox1_百度账号导入文件 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.textBox1_百度账号名模糊查询 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.comboBox1_账号分类 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox1_百度账号导入文件
            // 
            this.textBox1_百度账号导入文件.Location = new System.Drawing.Point(12, 12);
            this.textBox1_百度账号导入文件.Name = "textBox1_百度账号导入文件";
            this.textBox1_百度账号导入文件.ReadOnly = true;
            this.textBox1_百度账号导入文件.Size = new System.Drawing.Size(542, 21);
            this.textBox1_百度账号导入文件.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(560, 11);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(88, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "导入账号";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 105);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(634, 477);
            this.dataGridView1.TabIndex = 2;
            // 
            // textBox1_百度账号名模糊查询
            // 
            this.textBox1_百度账号名模糊查询.Location = new System.Drawing.Point(120, 73);
            this.textBox1_百度账号名模糊查询.Name = "textBox1_百度账号名模糊查询";
            this.textBox1_百度账号名模糊查询.Size = new System.Drawing.Size(231, 21);
            this.textBox1_百度账号名模糊查询.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 77);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "账号名称模糊查找：";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(585, 72);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(61, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "查询";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // comboBox1_账号分类
            // 
            this.comboBox1_账号分类.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1_账号分类.FormattingEnabled = true;
            this.comboBox1_账号分类.Items.AddRange(new object[] {
            "百度",
            "搜狗",
            "360问答",
            "所有"});
            this.comboBox1_账号分类.Location = new System.Drawing.Point(458, 73);
            this.comboBox1_账号分类.Name = "comboBox1_账号分类";
            this.comboBox1_账号分类.Size = new System.Drawing.Size(121, 20);
            this.comboBox1_账号分类.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(392, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "账号分类：";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // Form_账号维护
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(658, 594);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox1_账号分类);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.textBox1_百度账号名模糊查询);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1_百度账号导入文件);
            this.MaximizeBox = false;
            this.Name = "Form_账号维护";
            this.Text = "账号维护";
            this.Load += new System.EventHandler(this.Form_百度账号维护_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1_百度账号导入文件;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TextBox textBox1_百度账号名模糊查询;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ComboBox comboBox1_账号分类;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}