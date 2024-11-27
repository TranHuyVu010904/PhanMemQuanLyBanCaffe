using QuanLyQuanCafe.DAO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLyQuanCafe
{
    public partial class fDailyRevenue : Form
    {
        public fDailyRevenue()
        {
            InitializeComponent();
            LoadDailyRevenue();
        }
        void LoadDailyRevenue()
        {
            DataTable data = BillDAO.Instance.GetDailyRevenue();

            dgDailyRevenue.DataSource = data;
            GetTotalPriceInTxB();


        }
        void GetTotalPriceInTxB()
        {
            // Kiểm tra nếu có dữ liệu trong DataGridView
            if (dgDailyRevenue.DataSource is DataTable data && data.Rows.Count > 0)
            {
                decimal totalRevenue = 0;

                // Lặp qua từng dòng để tính tổng
                foreach (DataRow row in data.Rows)
                {
                    if (row["Tổng Tiền"] != DBNull.Value)
                    {
                        totalRevenue += Convert.ToDecimal(row["Tổng Tiền"]);
                    }
                }

                // Hiển thị tổng doanh thu vào TextBox
                CultureInfo vietnamCulture = new CultureInfo("vi-VN");
                txbTotalRevenue.Text = totalRevenue.ToString("C", vietnamCulture);
            }
            else
            {
                // Nếu không có dữ liệu, đặt TextBox về 0
                txbTotalRevenue.Text = "0";
            }
        }
        private int selectedBillId;
        private void btnBill_Click(object sender, EventArgs e)
        {
            LoadDailyRevenue();
        }
        private void dgDailyRevenue_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Kiểm tra nếu người dùng click vào một dòng dữ liệu hợp lệ
            if (e.RowIndex >= 0)
            {
                // Lấy dòng đã chọn và các giá trị cần thiết
                DataGridViewRow selectedRow = dgDailyRevenue.Rows[e.RowIndex];
                selectedBillId = (int)selectedRow.Cells["ID"].Value;
                decimal totalAmount = Convert.ToDecimal(selectedRow.Cells["Tổng Tiền"].Value);

                // Định dạng tổng tiền theo văn hóa Việt Nam và hiển thị vào txbTongTienBill
                CultureInfo vietnamCulture = new CultureInfo("vi-VN");
                txbTongTienBill.Text = totalAmount.ToString("C", vietnamCulture);

                // Lấy và hiển thị thông tin chi tiết BillInfo
                DataTable billInfoData = BillInfoDAO.Instance.GetBillInfoByBillId(selectedBillId);
                dtgBillInfo.DataSource = billInfoData;
            }
        }

        private void dtgBillInfo_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }

}
