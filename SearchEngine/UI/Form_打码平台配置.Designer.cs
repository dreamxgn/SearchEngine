namespace 搜索引擎自动营销助手V1._0.UI
{
    partial class Form_打码平台配置
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1_接口地址 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox2_账号 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox3_密码 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox4_超时时间 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "接口地址：";
            // 
            // textBox1_接口地址
            // 
            this.textBox1_接口地址.Location = new System.Drawing.Point(83, 26);
            this.textBox1_接口地址.Name = "textBox1_接口地址";
            this.textBox1_接口地址.Size = new System.Drawing.Size(351, 21);
            this.textBox1_接口地址.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(47, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "账号：";
            // 
            // textBox2_账号
            // 
            this.textBox2_账号.Location = new System.Drawing.Point(83, 67);
            this.textBox2_账号.Name = "textBox2_账号";
            this.textBox2_账号.Size = new System.Drawing.Size(128, 21);
            this.textBox2_账号.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(47, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "密码：";
            // 
            // textBox3_密码
            // 
            this.textBox3_密码.Location = new System.Drawing.Point(83, 104);
            this.textBox3_密码.Name = "textBox3_密码";
            this.textBox3_密码.Size = new System.Drawing.Size(128, 21);
            this.textBox3_密码.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 146);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "响应超时：";
            // 
            // textBox4_超时时间
            // 
            this.textBox4_超时时间.Location = new System.Drawing.Point(83, 143);
            this.textBox4_超时时间.Name = "textBox4_超时时间";
            this.textBox4_超时时间.Size = new System.Drawing.Size(49, 21);
            this.textBox4_超时时间.TabIndex = 7;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(83, 176);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "保存配置";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form_打码平台配置
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(473, 211);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox4_超时时间);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox3_密码);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox2_账号);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox1_接口地址);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.Name = "Form_打码平台配置";
            this.Text = "打码平台配置";
            this.Load += new System.EventHandler(this.Form_打码平台配置_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1_接口地址;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox2_账号;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox3_密码;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox4_超时时间;
        private System.Windows.Forms.Button button1;
    }
}