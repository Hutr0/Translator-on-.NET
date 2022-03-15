
namespace MyProgram
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.textOut = new System.Windows.Forms.TextBox();
            this.textIn = new System.Windows.Forms.RichTextBox();
            this.numeric_textBox = new System.Windows.Forms.RichTextBox();
            this.BNF__textBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1, 414);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(162, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Результат программы";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1, -2);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "Программа";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(834, 404);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(125, 30);
            this.button1.TabIndex = 2;
            this.button1.Text = "Выполнить";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textOut
            // 
            this.textOut.Location = new System.Drawing.Point(1, 437);
            this.textOut.Multiline = true;
            this.textOut.Name = "textOut";
            this.textOut.ReadOnly = true;
            this.textOut.Size = new System.Drawing.Size(971, 143);
            this.textOut.TabIndex = 4;
            // 
            // textIn
            // 
            this.textIn.Location = new System.Drawing.Point(47, 21);
            this.textIn.Name = "textIn";
            this.textIn.Size = new System.Drawing.Size(447, 377);
            this.textIn.TabIndex = 5;
            this.textIn.Text = "";
            this.textIn.ContentsResized += new System.Windows.Forms.ContentsResizedEventHandler(this.textIn_ContentsResized);
            this.textIn.VScroll += new System.EventHandler(this.textIn_VScroll);
            this.textIn.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textIn_MouseDown);
            // 
            // numeric_textBox
            // 
            this.numeric_textBox.Location = new System.Drawing.Point(12, 21);
            this.numeric_textBox.Name = "numeric_textBox";
            this.numeric_textBox.ReadOnly = true;
            this.numeric_textBox.Size = new System.Drawing.Size(29, 377);
            this.numeric_textBox.TabIndex = 6;
            this.numeric_textBox.Text = "";
            // 
            // BNF__textBox
            // 
            this.BNF__textBox.Location = new System.Drawing.Point(511, 21);
            this.BNF__textBox.Multiline = true;
            this.BNF__textBox.Name = "BNF__textBox";
            this.BNF__textBox.ReadOnly = true;
            this.BNF__textBox.Size = new System.Drawing.Size(448, 377);
            this.BNF__textBox.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(971, 581);
            this.Controls.Add(this.BNF__textBox);
            this.Controls.Add(this.numeric_textBox);
            this.Controls.Add(this.textIn);
            this.Controls.Add(this.textOut);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textOut;
        private System.Windows.Forms.RichTextBox textIn;
        private System.Windows.Forms.RichTextBox numeric_textBox;
        private System.Windows.Forms.TextBox BNF__textBox;
    }
}

