namespace plan_fighting_super_start
{
    partial class Rank
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            label1 = new Label();
            numericUpDown1 = new NumericUpDown();
            btnRefresh = new Button();
            dgvRank = new DataGridView();
            colHang = new DataGridViewTextBoxColumn();
            colTen = new DataGridViewTextBoxColumn();
            colLevel = new DataGridViewTextBoxColumn();
            statusStrip1 = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            label2 = new Label();
            Hạng = new DataGridViewTextBoxColumn();
            Tên = new DataGridViewTextBoxColumn();
            Level = new DataGridViewTextBoxColumn();
            statusStrip2 = new StatusStrip();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvRank).BeginInit();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 163);
            label1.ForeColor = Color.FromArgb(0, 255, 255);
            label1.Location = new Point(310, 9);
            label1.Name = "label1";
            label1.Size = new Size(262, 41);
            label1.TabIndex = 9;
            label1.Text = "BẢNG XẾP HẠNG";
            // 
            // numericUpDown1
            // 
            numericUpDown1.BackColor = Color.FromArgb(15, 20, 40);
            numericUpDown1.BorderStyle = BorderStyle.FixedSingle;
            numericUpDown1.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            numericUpDown1.ForeColor = Color.FromArgb(0, 192, 192);
            numericUpDown1.Location = new Point(206, 72);
            numericUpDown1.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(230, 30);
            numericUpDown1.TabIndex = 10;
            numericUpDown1.TextAlign = HorizontalAlignment.Center;
            numericUpDown1.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // btnRefresh
            // 
            btnRefresh.BackColor = Color.FromArgb(10, 20, 40);
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(0, 192, 192);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnRefresh.ForeColor = Color.FromArgb(0, 192, 192);
            btnRefresh.Location = new Point(524, 67);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(180, 35);
            btnRefresh.TabIndex = 11;
            btnRefresh.Text = "Tải bảng xếp hạng";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // dgvRank
            // 
            dgvRank.AllowUserToAddRows = false;
            dgvRank.AllowUserToDeleteRows = false;
            dgvRank.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(18, 24, 48);
            dataGridViewCellStyle1.ForeColor = Color.White;
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(0, 192, 192);
            dataGridViewCellStyle1.SelectionForeColor = Color.Black;
            dgvRank.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvRank.BackgroundColor = Color.FromArgb(10, 15, 35);
            dgvRank.BorderStyle = BorderStyle.None;
            dgvRank.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvRank.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(0, 120, 140);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(0, 150, 160);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvRank.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvRank.ColumnHeadersHeight = 32;
            dgvRank.Columns.AddRange(new DataGridViewColumn[] { colHang, colTen, colLevel });
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = Color.FromArgb(10, 15, 35);
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.WhiteSmoke;
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(0, 192, 192);
            dataGridViewCellStyle3.SelectionForeColor = Color.Black;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvRank.DefaultCellStyle = dataGridViewCellStyle3;
            dgvRank.EnableHeadersVisualStyles = false;
            dgvRank.GridColor = Color.FromArgb(0, 192, 192);
            dgvRank.Location = new Point(195, 130);
            dgvRank.MultiSelect = false;
            dgvRank.Name = "dgvRank";
            dgvRank.ReadOnly = true;
            dgvRank.RowHeadersVisible = false;
            dgvRank.RowHeadersWidth = 51;
            dgvRank.RowTemplate.Height = 28;
            dgvRank.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRank.Size = new Size(424, 320);
            dgvRank.TabIndex = 12;
            // 
            // colHang
            // 
            colHang.HeaderText = "Hạng";
            colHang.MinimumWidth = 6;
            colHang.Name = "colHang";
            colHang.ReadOnly = true;
            colHang.Width = 80;
            // 
            // colTen
            // 
            colTen.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colTen.HeaderText = "Tên";
            colTen.MinimumWidth = 6;
            colTen.Name = "colTen";
            colTen.ReadOnly = true;
            // 
            // colLevel
            // 
            colLevel.HeaderText = "Level";
            colLevel.MinimumWidth = 6;
            colLevel.Name = "colLevel";
            colLevel.ReadOnly = true;
            colLevel.Width = 90;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.FromArgb(10, 15, 30);
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip1.Location = new Point(0, 524);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(882, 29);
            statusStrip1.TabIndex = 13;
            statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            statusLabel.BackColor = Color.Transparent;
            statusLabel.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            statusLabel.ForeColor = Color.FromArgb(0, 192, 192);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(81, 23);
            statusLabel.Text = "Sẵn sàng";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            label2.ForeColor = Color.FromArgb(0, 192, 192);
            label2.Location = new Point(147, 76);
            label2.Name = "label2";
            label2.Size = new Size(47, 23);
            label2.TabIndex = 14;
            label2.Text = "TOP:";
            // 
            // Hạng
            // 
            Hạng.MinimumWidth = 6;
            Hạng.Name = "Hạng";
            Hạng.Width = 125;
            // 
            // Tên
            // 
            Tên.MinimumWidth = 6;
            Tên.Name = "Tên";
            Tên.Width = 125;
            // 
            // Level
            // 
            Level.MinimumWidth = 6;
            Level.Name = "Level";
            Level.Width = 125;
            // 
            // statusStrip2
            // 
            statusStrip2.ImageScalingSize = new Size(20, 20);
            statusStrip2.Location = new Point(0, 0);
            statusStrip2.Name = "statusStrip2";
            statusStrip2.Size = new Size(200, 22);
            statusStrip2.TabIndex = 0;
            // 
            // Rank
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resource.anhnenrank;
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(882, 553);
            Controls.Add(label2);
            Controls.Add(statusStrip1);
            Controls.Add(dgvRank);
            Controls.Add(btnRefresh);
            Controls.Add(numericUpDown1);
            Controls.Add(label1);
            DoubleBuffered = true;
            MinimumSize = new Size(900, 600);
            Name = "Rank";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Bảng xếp hạng";
            Load += Rank_Load;
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvRank).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.DataGridView dgvRank;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn Hạng;
        private System.Windows.Forms.DataGridViewTextBoxColumn Tên;
        private System.Windows.Forms.DataGridViewTextBoxColumn Level;
        private System.Windows.Forms.StatusStrip statusStrip2;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHang;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTen;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLevel;
        private System.Windows.Forms.Label label2;
    }
}