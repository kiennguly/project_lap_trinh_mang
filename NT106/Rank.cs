using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    public partial class Rank : Form
    {
        // Đặt đúng root của API Gateway (stage $default)
        private const string API_BASE = "https://f1oj97uhee.execute-api.ap-southeast-1.amazonaws.com";

        private static readonly HttpClient http = new HttpClient();
        private readonly JsonSerializerOptions jsonOpt = new() { PropertyNameCaseInsensitive = true };

        private class RankItem
        {
            public string Username { get; set; } = "";
            public int Level { get; set; }
            public int Rank { get; set; }
        }

        private class GetResp
        {
            public bool ok { get; set; }
            public List<RankItem> ranking { get; set; } = new();
        }

        public Rank()
        {
            InitializeComponent();

            // Cấu hình lưới và map DataPropertyName khớp JSON (logic giữ nguyên)
            dgvRank.AutoGenerateColumns = false;
            dgvRank.Columns.Clear();
            dgvRank.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Hạng",
                DataPropertyName = "Rank",
                Width = 70,
                ReadOnly = true
            });
            dgvRank.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Tên",
                DataPropertyName = "Username",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });
            dgvRank.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Level",
                DataPropertyName = "Level",
                Width = 80,
                ReadOnly = true
            });

            statusLabel.Text = "Sẵn sàng";

            // Tải bảng xếp hạng khi form hiện lần đầu
            this.Shown += async (_, __) => await TaiBangXepHangAsync((int)numericUpDown1.Value);
        }

        private async Task TaiBangXepHangAsync(int topN)
        {
            try
            {
                statusLabel.Text = $"Đang tải TOP {topN}...";
                var url = $"{API_BASE}/get?limit={topN}";
                var resp = await http.GetFromJsonAsync<GetResp>(url, jsonOpt);

                var list = resp?.ranking ?? new List<RankItem>();
                dgvRank.DataSource = list;

                // Tô sáng user hiện tại (nếu có)
                if (!string.IsNullOrWhiteSpace(AccountData.Username))
                {
                    foreach (DataGridViewRow row in dgvRank.Rows)
                    {
                        if (row.DataBoundItem is RankItem it &&
                            string.Equals(it.Username, AccountData.Username, StringComparison.OrdinalIgnoreCase))
                        {
                            row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(40, 50, 90);
                            row.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(0, 255, 255);
                            row.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
                        }
                    }
                }

                statusLabel.Text = $"Đã tải TOP {topN}.";
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Lỗi khi tải bảng xếp hạng: " + ex.Message;
            }
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await TaiBangXepHangAsync((int)numericUpDown1.Value);
        }

        private void Rank_Load(object sender, EventArgs e)
        {

        }
    }
}