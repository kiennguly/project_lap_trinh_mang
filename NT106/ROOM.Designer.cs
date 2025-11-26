using System.Drawing;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    partial class Room
    {
        private System.ComponentModel.IContainer components = null;
        private Label labelTitle;
        private Label labelRoom;
        private TextBox txtRoomID;
        private Button btnCreateRoom;
        private Button btnJoinRoom;
        private Button btnStartGame;
        private Label lblStatus;

        private Label label1;
        private DataGridView IdRoom;
        private DataGridViewTextBoxColumn Player1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn colPlayersStatus;

        private Button button1;
        private Button button2;

        // 🔹 Chat controls
        private RichTextBox chatBox;
        private TextBox txtChat;
        private Button btnSendChat;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            labelTitle = new Label();
            labelRoom = new Label();
            txtRoomID = new TextBox();
            btnCreateRoom = new Button();
            btnJoinRoom = new Button();
            btnStartGame = new Button();
            lblStatus = new Label();
            label1 = new Label();
            IdRoom = new DataGridView();
            Player1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            colPlayersStatus = new DataGridViewTextBoxColumn();
            button1 = new Button();
            button2 = new Button();
            chatBox = new RichTextBox();
            txtChat = new TextBox();
            btnSendChat = new Button();
            ((System.ComponentModel.ISupportInitialize)IdRoom).BeginInit();
            SuspendLayout();
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.BackColor = Color.FromArgb(90, 0, 0, 0);
            labelTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            labelTitle.ForeColor = Color.FromArgb(0, 255, 255);
            labelTitle.Location = new Point(30, 25);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(364, 37);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "SOLO LAN – 2 NGƯỜI CHƠI";
            // 
            // labelRoom
            // 
            labelRoom.AutoSize = true;
            labelRoom.BackColor = Color.Transparent;
            labelRoom.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            labelRoom.ForeColor = Color.FromArgb(0, 192, 192);
            labelRoom.Location = new Point(32, 100);
            labelRoom.Name = "labelRoom";
            labelRoom.Size = new Size(134, 23);
            labelRoom.TabIndex = 1;
            labelRoom.Text = "Room ID (6 số):";
            // 
            // txtRoomID
            // 
            txtRoomID.BackColor = Color.FromArgb(15, 22, 45);
            txtRoomID.BorderStyle = BorderStyle.FixedSingle;
            txtRoomID.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            txtRoomID.ForeColor = Color.FromArgb(0, 255, 255);
            txtRoomID.Location = new Point(180, 96);
            txtRoomID.Margin = new Padding(3, 4, 3, 4);
            txtRoomID.MaxLength = 6;
            txtRoomID.Name = "txtRoomID";
            txtRoomID.Size = new Size(146, 30);
            txtRoomID.TabIndex = 2;
            txtRoomID.TextAlign = HorizontalAlignment.Center;
            // 
            // btnCreateRoom
            // 
            btnCreateRoom.BackColor = Color.FromArgb(10, 20, 40);
            btnCreateRoom.FlatAppearance.BorderColor = Color.FromArgb(0, 192, 192);
            btnCreateRoom.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 120, 140);
            btnCreateRoom.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 80, 100);
            btnCreateRoom.FlatStyle = FlatStyle.Flat;
            btnCreateRoom.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            btnCreateRoom.ForeColor = Color.FromArgb(0, 192, 192);
            btnCreateRoom.Location = new Point(36, 155);
            btnCreateRoom.Margin = new Padding(3, 4, 3, 4);
            btnCreateRoom.Name = "btnCreateRoom";
            btnCreateRoom.Size = new Size(290, 45);
            btnCreateRoom.TabIndex = 3;
            btnCreateRoom.Text = "Tạo phòng (Host)";
            btnCreateRoom.UseVisualStyleBackColor = false;
            btnCreateRoom.Click += btnCreateRoom_Click;
            // 
            // btnJoinRoom
            // 
            btnJoinRoom.BackColor = Color.FromArgb(10, 20, 40);
            btnJoinRoom.FlatAppearance.BorderColor = Color.FromArgb(0, 192, 192);
            btnJoinRoom.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 120, 140);
            btnJoinRoom.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 80, 100);
            btnJoinRoom.FlatStyle = FlatStyle.Flat;
            btnJoinRoom.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            btnJoinRoom.ForeColor = Color.FromArgb(0, 192, 192);
            btnJoinRoom.Location = new Point(36, 215);
            btnJoinRoom.Margin = new Padding(3, 4, 3, 4);
            btnJoinRoom.Name = "btnJoinRoom";
            btnJoinRoom.Size = new Size(290, 45);
            btnJoinRoom.TabIndex = 4;
            btnJoinRoom.Text = "Tham gia phòng (Client)";
            btnJoinRoom.UseVisualStyleBackColor = false;
            btnJoinRoom.Click += btnJoinRoom_Click;
            // 
            // btnStartGame
            // 
            btnStartGame.BackColor = Color.FromArgb(10, 20, 40);
            btnStartGame.Enabled = false;
            btnStartGame.FlatAppearance.BorderColor = Color.FromArgb(0, 192, 192);
            btnStartGame.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 120, 140);
            btnStartGame.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 80, 100);
            btnStartGame.FlatStyle = FlatStyle.Flat;
            btnStartGame.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnStartGame.ForeColor = Color.FromArgb(0, 255, 255);
            btnStartGame.Location = new Point(36, 275);
            btnStartGame.Margin = new Padding(3, 4, 3, 4);
            btnStartGame.Name = "btnStartGame";
            btnStartGame.Size = new Size(290, 50);
            btnStartGame.TabIndex = 5;
            btnStartGame.Text = "BẮT ĐẦU";
            btnStartGame.UseVisualStyleBackColor = false;
            btnStartGame.Click += btnStartGame_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoEllipsis = true;
            lblStatus.BackColor = Color.FromArgb(110, 0, 0, 0);
            lblStatus.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            lblStatus.ForeColor = Color.FromArgb(0, 192, 192);
            lblStatus.Location = new Point(36, 400);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(424, 70);
            lblStatus.TabIndex = 6;
            lblStatus.Text = "Trạng thái: Chưa tạo/tham gia phòng.";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.FromArgb(90, 0, 0, 0);
            label1.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(0, 255, 255);
            label1.Location = new Point(514, 86);
            label1.Name = "label1";
            label1.Size = new Size(289, 37);
            label1.TabIndex = 8;
            label1.Text = "DANH SÁCH PHÒNG ";
            // 
            // IdRoom
            // 
            IdRoom.AllowUserToAddRows = false;
            IdRoom.AllowUserToDeleteRows = false;
            IdRoom.AllowUserToResizeRows = false;
            dataGridViewCellStyle4.BackColor = Color.FromArgb(14, 20, 40);
            dataGridViewCellStyle4.ForeColor = Color.White;
            dataGridViewCellStyle4.SelectionBackColor = Color.FromArgb(0, 192, 192);
            dataGridViewCellStyle4.SelectionForeColor = Color.Black;
            IdRoom.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle4;
            IdRoom.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            IdRoom.BackgroundColor = Color.FromArgb(10, 15, 35);
            IdRoom.BorderStyle = BorderStyle.None;
            IdRoom.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            IdRoom.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.BackColor = Color.FromArgb(0, 120, 140);
            dataGridViewCellStyle5.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            dataGridViewCellStyle5.ForeColor = Color.White;
            dataGridViewCellStyle5.SelectionBackColor = Color.FromArgb(0, 150, 160);
            dataGridViewCellStyle5.SelectionForeColor = Color.White;
            dataGridViewCellStyle5.WrapMode = DataGridViewTriState.True;
            IdRoom.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            IdRoom.ColumnHeadersHeight = 32;
            IdRoom.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            IdRoom.Columns.AddRange(new DataGridViewColumn[] { Player1, dataGridViewTextBoxColumn2, colPlayersStatus });
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = Color.FromArgb(18, 24, 48);
            dataGridViewCellStyle6.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle6.ForeColor = Color.WhiteSmoke;
            dataGridViewCellStyle6.SelectionBackColor = Color.FromArgb(0, 192, 192);
            dataGridViewCellStyle6.SelectionForeColor = Color.Black;
            dataGridViewCellStyle6.WrapMode = DataGridViewTriState.False;
            IdRoom.DefaultCellStyle = dataGridViewCellStyle6;
            IdRoom.GridColor = Color.FromArgb(0, 192, 192);
            IdRoom.Location = new Point(514, 147);
            IdRoom.Name = "IdRoom";
            IdRoom.ReadOnly = true;
            IdRoom.RowHeadersVisible = false;
            IdRoom.RowHeadersWidth = 51;
            IdRoom.RowTemplate.Height = 28;
            IdRoom.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            IdRoom.Size = new Size(478, 323);
            IdRoom.TabIndex = 9;
            IdRoom.CellContentClick += IdRoom_CellContentClick;
            IdRoom.CellDoubleClick += IdRoom_CellDoubleClick;
            // 
            // Player1
            // 
            Player1.HeaderText = "ROOM ID";
            Player1.MinimumWidth = 6;
            Player1.Name = "Player1";
            Player1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.HeaderText = "Host";
            dataGridViewTextBoxColumn2.MinimumWidth = 6;
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // colPlayersStatus
            // 
            colPlayersStatus.HeaderText = "Players / Status";
            colPlayersStatus.MinimumWidth = 6;
            colPlayersStatus.Name = "colPlayersStatus";
            colPlayersStatus.ReadOnly = true;
            // 
            // button1
            // 
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
            button1.ForeColor = Color.FromArgb(0, 192, 192);
            button1.Location = new Point(36, 348);
            button1.Name = "button1";
            button1.Size = new Size(290, 49);
            button1.TabIndex = 10;
            button1.Text = "Lịch sử đấu";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.FlatStyle = FlatStyle.Flat;
            button2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 163);
            button2.ForeColor = Color.FromArgb(0, 192, 192);
            button2.Location = new Point(908, 527);
            button2.Name = "button2";
            button2.Size = new Size(149, 54);
            button2.TabIndex = 11;
            button2.Text = "Load Data Room";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // chatBox
            // 
            chatBox.BackColor = Color.FromArgb(10, 15, 35);
            chatBox.BorderStyle = BorderStyle.FixedSingle;
            chatBox.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 163);
            chatBox.ForeColor = Color.FromArgb(0, 255, 255);
            chatBox.Location = new Point(36, 470);
            chatBox.Name = "chatBox";
            chatBox.ReadOnly = true;
            chatBox.Size = new Size(424, 70);
            chatBox.TabIndex = 12;
            chatBox.Text = "";
            chatBox.TextChanged += chatBox_TextChanged;
            // 
            // txtChat
            // 
            txtChat.BackColor = Color.FromArgb(15, 22, 45);
            txtChat.BorderStyle = BorderStyle.FixedSingle;
            txtChat.Font = new Font("Segoe UI", 9F);
            txtChat.ForeColor = Color.White;
            txtChat.Location = new Point(36, 545);
            txtChat.Name = "txtChat";
            txtChat.Size = new Size(340, 27);
            txtChat.TabIndex = 13;
            txtChat.TextChanged += txtChat_TextChanged;
            // 
            // btnSendChat
            // 
            btnSendChat.FlatStyle = FlatStyle.Flat;
            btnSendChat.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnSendChat.ForeColor = Color.FromArgb(0, 192, 192);
            btnSendChat.Location = new Point(385, 543);
            btnSendChat.Name = "btnSendChat";
            btnSendChat.Size = new Size(75, 30);
            btnSendChat.TabIndex = 14;
            btnSendChat.Text = "Gửi";
            btnSendChat.UseVisualStyleBackColor = true;
            btnSendChat.Click += btnSendChat_Click;
            // 
            // Room
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(10, 15, 30);
            BackgroundImage = Properties.Resource.Gemini_Generated_Image_5ka7of5ka7of5ka7;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1084, 593);
            Controls.Add(btnSendChat);
            Controls.Add(txtChat);
            Controls.Add(chatBox);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(IdRoom);
            Controls.Add(label1);
            Controls.Add(lblStatus);
            Controls.Add(btnStartGame);
            Controls.Add(btnJoinRoom);
            Controls.Add(btnCreateRoom);
            Controls.Add(txtRoomID);
            Controls.Add(labelRoom);
            Controls.Add(labelTitle);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Room";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Lobby – Solo LAN";
            FormClosing += Form5_FormClosing;
            Load += Room_Load;
            ((System.ComponentModel.ISupportInitialize)IdRoom).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion
    }
}
