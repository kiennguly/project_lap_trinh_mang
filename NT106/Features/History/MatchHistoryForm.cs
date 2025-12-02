using System.Text.Json;

namespace plan_fighting_super_start
{
    public partial class MatchHistoryForm : Form
    {
        private readonly HttpClient _httpClient = new HttpClient();

        private readonly string apiBaseUrl = "https://840blg9a68.execute-api.ap-southeast-1.amazonaws.com"; // URL API Gateway 

        public MatchHistoryForm()
        {
            InitializeComponent();
        }

        private async void MatchHistoryForm_Load(object sender, EventArgs e)
        {
            await LoadHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            try
            {
                string username = AccountData.Username; // Username hiện tại

                if (string.IsNullOrEmpty(username))
                {
                    lblStatus.Text = "⚠️ Lỗi: Không có người dùng nào đăng nhập.";
                    MessageBox.Show("Vui lòng đăng nhập trước.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                dgvHistory.Rows.Clear();
                lblStatus.Text = "Đang tải lịch sử...";

                // Gọi API GET /matchhistory/{username}
                string url = $"{apiBaseUrl}/matchhistory/{username}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                string json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    lblStatus.Text = $"Lỗi tải lịch sử: {response.StatusCode}";
                    MessageBox.Show(json, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var historyList = JsonSerializer.Deserialize<List<MatchItem>>(json, options);

                if (historyList == null || historyList.Count == 0)
                {
                    lblStatus.Text = "⚠️ Không có trận đấu nào trong lịch sử.";
                    return;
                }

                foreach (var match in historyList)
                {
                    string winner = match.WinnerUsername ?? "N/A";
                    string loser = match.LoserUsername ?? "N/A";
                    string result = (winner == username) ? "Chiến thắng" : "Thất bại";

                    DateTime matchDateTime;
                    if (string.IsNullOrEmpty(match.MatchDate) || !DateTime.TryParse(match.MatchDate, out matchDateTime))
                    {
                        matchDateTime = DateTime.MinValue;
                    }

                    dgvHistory.Rows.Add(
                        winner,
                        loser,
                        matchDateTime == DateTime.MinValue
                            ? "Không rõ"
                            : matchDateTime.ToString("dd/MM/yyyy HH:mm"),
                        result
                    );
                }

                lblStatus.Text = $"Tổng số trận: {historyList.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải lịch sử đấu: " + ex.Message);
                lblStatus.Text = "Lỗi tải lịch sử.";
            }
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await LoadHistoryAsync();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    // Class để deserialize JSON từ Lambda
    public class MatchItem
    {
        public string Id { get; set; }
        public string WinnerUsername { get; set; }
        public string LoserUsername { get; set; }
        public string MatchDate { get; set; }
    }
}