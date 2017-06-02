namespace 搜索引擎自动营销助手V1._0.UI
{
    partial class Form_注册
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
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1_硬件码 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox2_注册码 = new System.Windows.Forms.TextBox();
            this.button1_注册 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(251, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "产品未注册";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 118);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "硬件码";
            // 
            // textBox1_硬件码
            // 
            this.textBox1_硬件码.Location = new System.Drawing.Point(12, 133);
            this.textBox1_硬件码.Name = "textBox1_硬件码";
            this.textBox1_硬件码.ReadOnly = true;
            this.textBox1_硬件码.Size = new System.Drawing.Size(582, 21);
            this.textBox1_硬件码.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 168);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "注册码";
            // 
            // textBox2_注册码
            // 
            this.textBox2_注册码.Location = new System.Drawing.Point(12, 183);
            this.textBox2_注册码.Name = "textBox2_注册码";
            this.textBox2_注册码.Size = new System.Drawing.Size(582, 21);
            this.textBox2_注册码.TabIndex = 4;
            // 
            // button1_注册
            // 
            this.button1_注册.Location = new System.Drawing.Point(12, 210);
            this.button1_注册.Name = "button1_注册";
            this.button1_注册.Size = new System.Drawing.Size(75, 23);
            this.button1_注册.TabIndex = 5;
            this.button1_注册.Text = "注册";
            this.button1_注册.UseVisualStyleBackColor = true;
            this.button1_注册.Click += new System.EventHandler(this.button1_注册_Click);
            // 
            // Form_注册
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(620, 268);
            this.Controls.Add(this.button1_注册);
            this.Controls.Add(this.textBox2_注册码);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox1_硬件码);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.Name = "Form_注册";
            this.Text = "注册产品";
            this.Load += new System.EventHandler(this.Form_注册_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1_硬件码;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox2_注册码;
        private System.Windows.Forms.Button button1_注册;
    }
}