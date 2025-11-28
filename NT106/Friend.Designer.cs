using System.Drawing;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    partial class Friend
    {
        private System.ComponentModel.IContainer components = null;

        private ListView lvFriends;
        private ColumnHeader colAvatar;
        private ColumnHeader colUsername;
        private ColumnHeader colStatus;
        private TextBox txtFriendUsername;
        private Label lblFriendUsername;
        private Button btnSendRequest;
        private Button btnAccept;
        private Button btnDecline;
        private Button btnRefresh;
        private Label lblTitle;
        private Label lblLoading;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lvFriends = new ListView();
            colAvatar = new ColumnHeader();
            colUsername = new ColumnHeader("(none)");
            colStatus = new ColumnHeader();
            txtFriendUsername = new TextBox();
            lblFriendUsername = new Label();
            btnSendRequest = new Button();
            btnAccept = new Button();
            btnDecline = new Button();
            btnRefresh = new Button();
            lblTitle = new Label();
            lblLoading = new Label();
            button1 = new Button();
            SuspendLayout();
            // 
            // lvFriends
            // 
            lvFriends.BackColor = Color.FromArgb(15, 20, 40);
            lvFriends.BorderStyle = BorderStyle.FixedSingle;
            lvFriends.Columns.AddRange(new ColumnHeader[] { colAvatar, colUsername, colStatus });
            lvFriends.Font = new Font("Arial", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            lvFriends.ForeColor = Color.FromArgb(0, 192, 192);
            lvFriends.FullRowSelect = true;
            lvFriends.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvFriends.Location = new Point(20, 50);
            lvFriends.MultiSelect = false;
            lvFriends.Name = "lvFriends";
            lvFriends.Size = new Size(520, 350);
            lvFriends.TabIndex = 0;
            lvFriends.UseCompatibleStateImageBehavior = false;
            lvFriends.View = View.Details;
            lvFriends.ColumnWidthChanging += lvFriends_ColumnWidthChanging;
            lvFriends.SelectedIndexChanged += lvFriends_SelectedIndexChanged;
            // 
            // colAvatar
            // 
            colAvatar.Text = "";
            // 
            // colUsername
            // 
            colUsername.Text = "User";
            colUsername.Width = 180;
            // 
            // colStatus
            // 
            colStatus.Text = "Status";
            colStatus.Width = 260;
            // 
            // txtFriendUsername
            // 
            txtFriendUsername.BackColor = Color.FromArgb(15, 25, 45);
            txtFriendUsername.BorderStyle = BorderStyle.FixedSingle;
            txtFriendUsername.ForeColor = Color.White;
            txtFriendUsername.Location = new Point(172, 418);
            txtFriendUsername.Name = "txtFriendUsername";
            txtFriendUsername.Size = new Size(210, 27);
            txtFriendUsername.TabIndex = 1;
            // 
            // lblFriendUsername
            // 
            lblFriendUsername.AutoSize = true;
            lblFriendUsername.Font = new Font("Arial", 10.2F, FontStyle.Bold);
            lblFriendUsername.ForeColor = Color.FromArgb(0, 192, 192);
            lblFriendUsername.Location = new Point(20, 423);
            lblFriendUsername.Name = "lblFriendUsername";
            lblFriendUsername.Size = new Size(146, 19);
            lblFriendUsername.TabIndex = 2;
            lblFriendUsername.Text = "Username bạn bè";
            // 
            // btnSendRequest
            // 
            btnSendRequest.Font = new Font("Arial", 10.2F, FontStyle.Bold);
            btnSendRequest.ForeColor = Color.FromArgb(0, 192, 192);
            btnSendRequest.Location = new Point(388, 418);
            btnSendRequest.Name = "btnSendRequest";
            btnSendRequest.Size = new Size(110, 27);
            btnSendRequest.TabIndex = 3;
            btnSendRequest.Text = "Gửi lời mời";
            btnSendRequest.UseVisualStyleBackColor = false;
            btnSendRequest.Click += btnSendRequest_Click;
            // 
            // btnAccept
            // 
            btnAccept.Font = new Font("Arial", 10.2F, FontStyle.Bold);
            btnAccept.ForeColor = Color.FromArgb(0, 192, 192);
            btnAccept.Location = new Point(560, 95);
            btnAccept.Name = "btnAccept";
            btnAccept.Size = new Size(140, 32);
            btnAccept.TabIndex = 4;
            btnAccept.Text = "✔ Chấp nhận";
            btnAccept.UseVisualStyleBackColor = false;
            btnAccept.Click += btnAccept_Click;
            // 
            // btnDecline
            // 
            btnDecline.Font = new Font("Arial", 10.2F, FontStyle.Bold);
            btnDecline.ForeColor = Color.FromArgb(0, 192, 192);
            btnDecline.Location = new Point(560, 137);
            btnDecline.Name = "btnDecline";
            btnDecline.Size = new Size(140, 32);
            btnDecline.TabIndex = 5;
            btnDecline.Text = "✖ Từ chối";
            btnDecline.UseVisualStyleBackColor = false;
            btnDecline.Click += btnDecline_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Font = new Font("Arial", 10.2F, FontStyle.Bold);
            btnRefresh.ForeColor = Color.FromArgb(0, 192, 192);
            btnRefresh.Location = new Point(560, 53);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(140, 32);
            btnRefresh.TabIndex = 6;
            btnRefresh.Text = "⟳ Làm mới";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Consolas", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Cyan;
            lblTitle.Location = new Point(12, 9);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(224, 32);
            lblTitle.TabIndex = 10;
            lblTitle.Text = "CYBER  FRIENDS";
            // 
            // lblLoading
            // 
            lblLoading.AutoSize = true;
            lblLoading.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
            lblLoading.ForeColor = Color.Cyan;
            lblLoading.Location = new Point(560, 30);
            lblLoading.Name = "lblLoading";
            lblLoading.Size = new Size(99, 20);
            lblLoading.TabIndex = 11;
            lblLoading.Text = "LOADING...";
            lblLoading.Visible = false;
            // 
            // button1
            // 
            button1.Font = new Font("Arial", 10.2F, FontStyle.Bold);
            button1.ForeColor = Color.FromArgb(0, 192, 192);
            button1.Location = new Point(560, 410);
            button1.Name = "button1";
            button1.Size = new Size(140, 32);
            button1.TabIndex = 12;
            button1.Text = "Đóng";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // Friend
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(10, 15, 30);
            ClientSize = new Size(720, 460);
            Controls.Add(button1);
            Controls.Add(lblLoading);
            Controls.Add(lblTitle);
            Controls.Add(btnRefresh);
            Controls.Add(btnDecline);
            Controls.Add(btnAccept);
            Controls.Add(btnSendRequest);
            Controls.Add(lblFriendUsername);
            Controls.Add(txtFriendUsername);
            Controls.Add(lvFriends);
            Font = new Font("Consolas", 9.75F);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Friend";
            Text = "Cyber Friend List";
            Load += Friend_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
    }
}
