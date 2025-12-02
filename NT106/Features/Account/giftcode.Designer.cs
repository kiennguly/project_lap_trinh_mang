namespace plan_fighting_super_start
{
    partial class giftcode
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Panel panelTextBorder;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label labelTitle;

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
            panelMain = new Panel();
            labelTitle = new Label();
            labelHint = new Label();
            panelTextBorder = new Panel();
            textBox1 = new TextBox();
            button1 = new Button();
            panelMain.SuspendLayout();
            panelTextBorder.SuspendLayout();
            SuspendLayout();
            // 
            // panelMain
            // 
            panelMain.BackColor = Color.FromArgb(20, 30, 50);
            panelMain.Controls.Add(labelTitle);
            panelMain.Controls.Add(labelHint);
            panelMain.Controls.Add(panelTextBorder);
            panelMain.Controls.Add(button1);
            panelMain.Location = new Point(20, 20);
            panelMain.Name = "panelMain";
            panelMain.Size = new Size(455, 200);
            panelMain.TabIndex = 0;
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            labelTitle.ForeColor = Color.FromArgb(0, 255, 200);
            labelTitle.Location = new Point(20, 15);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(236, 32);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "Enter your giftcode";
            // 
            // labelHint
            // 
            labelHint.AutoSize = true;
            labelHint.Font = new Font("Segoe UI", 9F);
            labelHint.ForeColor = Color.Silver;
            labelHint.Location = new Point(22, 50);
            labelHint.Name = "labelHint";
            labelHint.Size = new Size(280, 20);
            labelHint.TabIndex = 1;
            labelHint.Text = "Paste the code you received into the box.";
            // 
            // panelTextBorder
            // 
            panelTextBorder.BackColor = Color.FromArgb(0, 200, 200);
            panelTextBorder.Controls.Add(textBox1);
            panelTextBorder.Location = new Point(20, 75);
            panelTextBorder.Name = "panelTextBorder";
            panelTextBorder.Padding = new Padding(2);
            panelTextBorder.Size = new Size(415, 34);
            panelTextBorder.TabIndex = 2;
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.FromArgb(10, 20, 40);
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Dock = DockStyle.Fill;
            textBox1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            textBox1.ForeColor = Color.WhiteSmoke;
            textBox1.HideSelection = false;
            textBox1.Location = new Point(2, 2);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(411, 23);
            textBox1.TabIndex = 0;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(15, 25, 45);
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            button1.ForeColor = Color.FromArgb(0, 255, 200);
            button1.Location = new Point(152, 130);
            button1.Name = "button1";
            button1.Size = new Size(150, 40);
            button1.TabIndex = 3;
            button1.Text = "Redeem";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // giftcode
            // 
            AcceptButton = button1;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.CadetBlue;
            ClientSize = new Size(495, 245);
            Controls.Add(panelMain);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "giftcode";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "giftcode";
            panelMain.ResumeLayout(false);
            panelMain.PerformLayout();
            panelTextBorder.ResumeLayout(false);
            panelTextBorder.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private Label labelHint;
    }
}
