using QuanLyQuanCafe.DAO;
using QuanLyQuanCafe.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;

namespace QuanLyQuanCafe
{
    public partial class fAdmin : Form
    {
        public string CurrentUserName { get; set; }
        private bool enableFormattedPriceChange = true;
        BindingSource accountList = new BindingSource();

        public fAdmin(string currentUserName)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            cFoods.MouseClick += Chart_MouseClick;
            cDrinks.MouseClick += Chart_MouseClick;

            // Gán account được truyền vào constructor cho biến LoginAccount
            CurrentUserName = currentUserName;

            Loading();

            // Đăng ký sự kiện cho nmPrice
            this.nmPrice.ValueChanged += new EventHandler(this.nmPrice_ValueChanged);
            this.nmPrice.KeyUp += new KeyEventHandler(this.nmPrice_KeyUp); // Sử dụng sự kiện KeyUp

            // Đăng ký sự kiện cho txbFormattedPrice
            this.txbFormattedPrice.TextChanged += new EventHandler(this.txbFormattedPrice_TextChanged);
        }
        public void ReloadFoodAndCategoryData()
        {
            LoadListFood(); // Phương thức để load lại dữ liệu cho dtgvFood
            LoatCategoryIntoCombobox(); // Phương thức để load lại dữ liệu cho ComboBox
        }
        void AddAccountBinding()
        {
            txbAccount.DataBindings.Clear();
            txbDPN.DataBindings.Clear();
            txbEmail.DataBindings.Clear();

            if (dtgvAccount.Rows.Count > 0) // Kiểm tra nếu dtgvAccount có dữ liệu
            {
                // Sử dụng DataTable làm nguồn dữ liệu
                txbAccount.DataBindings.Add(new Binding("Text", dtgvAccount.DataSource, "UserName", true, DataSourceUpdateMode.Never));
                txbDPN.DataBindings.Add(new Binding("Text", dtgvAccount.DataSource, "DisplayName", true, DataSourceUpdateMode.Never));
                txbEmail.DataBindings.Add(new Binding("Text", dtgvAccount.DataSource, "Email", true, DataSourceUpdateMode.Never));
            }
            else
            {
                MessageBox.Show("Danh sách tài khoản trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        void LoadAccount()
        {
            DataTable DTAccount = AccountDAO.Instance.GetListAccount();
            dtgvAccount.DataSource = DTAccount;

            if (DTAccount.Rows.Count > 0)
            {
                dtgvAccount.Columns["UserName"].HeaderText = "Tên Tài Khoản";
                dtgvAccount.Columns["DisplayName"].HeaderText = "Tên hiển thị";
                dtgvAccount.Columns["NameAccountByType"].HeaderText = "Loại Tài Khoản";

                AddAccountBinding(); // Đảm bảo rằng dữ liệu có sẵn trước khi binding
            }
            else
            {
                MessageBox.Show("Danh sách tài khoản trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        void Loading()
        {
            LoadDateTimePickerBill();
            LoadListBillByDate(dptTuNgay.Value, dptDenNgay.Value);
            LoadListFood();
            AddFoodBinding();
            LoadAccount();
            AddAccountBinding();
            LoadActiveTables();
            LoatCategoryIntoCombobox();
            LoadTpyeIntoCombobox();
            LoadChartDrinks(dptTuNgay.Value, dptDenNgay.Value);
            LoadChartFood(dptTuNgay.Value, dptDenNgay.Value);
        }
        void LoadDateTimePickerBill()
        {
            DateTime today = DateTime.Now;
            dptTuNgay.Value = new DateTime(today.Year, today.Month, 1);
            dptDenNgay.Value = dptTuNgay.Value.AddMonths(1).AddDays(-1);
        }

        void LoadListBillByDate(DateTime checkIn, DateTime checkOut)
        {
            // Kiểm tra nếu ngày kết thúc trước ngày bắt đầu
            if (checkOut < checkIn)
            {
                dgThongKe.Rows.Clear();
                MessageBox.Show("Ngày kết thúc không thể trước ngày bắt đầu. Vui lòng chọn lại khoảng thời gian.", "Lỗi ngày tháng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataTable billData = BillDAO.Instance.GetBillListByDate(checkIn, checkOut);

            // Xóa dữ liệu cũ
            dgThongKe.Rows.Clear();

            // Thêm dữ liệu mới vào DataGridView
            foreach (DataRow row in billData.Rows)
            {
                int index = dgThongKe.Rows.Add(); // Tạo một hàng mới
                dgThongKe.Rows[index].Cells["id"].Value = row["id"];
                dgThongKe.Rows[index].Cells["name"].Value = row["name"];
                dgThongKe.Rows[index].Cells["DateCheckIn"].Value = row["DateCheckIn"];
                dgThongKe.Rows[index].Cells["DateCheckOut"].Value = row["DateCheckOut"];
                dgThongKe.Rows[index].Cells["giamgia"].Value = row["discount"];
                dgThongKe.Rows[index].Cells["tongtien"].Value = row["totalPrice"];
                // Thêm các cột khác nếu cần
            }

            dgThongKe.Refresh(); // Cập nhật giao diện người dùng
        }
        private int selectedBillId;
        private void dgThongKe_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Kiểm tra nếu người dùng click vào một dòng dữ liệu hợp lệ
                if (e.RowIndex >= 0)
                {
                    // Lấy dòng đã chọn và các giá trị cần thiết
                    DataGridViewRow selectedRow = dgThongKe.Rows[e.RowIndex];
                    selectedBillId = (int)selectedRow.Cells["ID"].Value;
                    decimal totalAmount = Convert.ToDecimal(selectedRow.Cells["TongTien"].Value);

                    // Định dạng tổng tiền theo văn hóa Việt Nam và hiển thị vào txbTongTienBill
                    CultureInfo vietnamCulture = new CultureInfo("vi-VN");
                    txbTongTienBill.Text = totalAmount.ToString("C", vietnamCulture);

                    // Lấy và hiển thị thông tin chi tiết BillInfo
                    DataTable billInfoData = BillInfoDAO.Instance.GetBillInfoByBillId(selectedBillId);
                    dtgChiTiet.DataSource = billInfoData;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lấy thông tin chi tiết hóa đơn: " + ex.Message);
            }
        }
        private void dgThongKe_RowEnter(object sender, DataGridViewCellEventArgs e)
        {

        }

        void LoadListFood()
        {
            DataTable DTFood = FoodDAO.Instance.GetListFood();
            dtgvFood.DataSource = DTFood;

            dtgvFood.Columns["id"].HeaderText = "ID";
            dtgvFood.Columns["name"].HeaderText = "Tên món";
            dtgvFood.Columns["TenDanhMuc"].HeaderText = "Danh mục";
            dtgvFood.Columns["price"].HeaderText = "Giá";

            AddFoodBinding();

            if (DTFood.Rows.Count > 0)
            {
                nmPrice.Value = Convert.ToDecimal(DTFood.Rows[0]["price"]);
            }
        }






        void AddFoodBinding()
        {
            // Xóa ràng buộc cũ nếu có trước khi thêm ràng buộc mới
            txbFoodName.DataBindings.Clear();
            txbFoodID.DataBindings.Clear();
            nmPrice.DataBindings.Clear();

            // Kiểm tra nếu DataSource của DataGridView không null
            if (dtgvFood.DataSource != null)
            {
                // Thiết lập ràng buộc cho textbox từ DataGridView
                txbFoodName.DataBindings.Add(new Binding("Text", dtgvFood.DataSource, "name", true, DataSourceUpdateMode.Never));
                txbFoodID.DataBindings.Add(new Binding("Text", dtgvFood.DataSource, "id", true, DataSourceUpdateMode.Never));
                nmPrice.DataBindings.Add(new Binding("Text", dtgvFood.DataSource, "price", true, DataSourceUpdateMode.Never));
            }
        }

        private void LoadChartFood(DateTime checkIn, DateTime checkOut)
        {
            DataTable data = BillInfoDAO.Instance.GetTop8MostOrderedFoods(checkIn, checkOut);
            if (checkOut < checkIn)
            {
                return;
            }
            if (data == null || data.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để hiển thị.");
                cFoods.Series.Clear();
                return;
            }


            int totalSold = 0;
            foreach (DataRow row in data.Rows)
            {
                totalSold += (int)row["TotalOrdered"];
            }

            if (cFoods.Series.IsUniqueName("MostOrderedFoods"))
            {
                var mostOrderedFoodsSeries = new Series("MostOrderedFoods")
                {
                    ChartType = SeriesChartType.Pie
                };
                cFoods.Series.Add(mostOrderedFoodsSeries);
            }

            var series = cFoods.Series["MostOrderedFoods"];
            series.Points.Clear();

            foreach (DataRow row in data.Rows)
            {
                string foodName = row["FoodName"].ToString();
                int totalOrdered = (int)row["TotalOrdered"];
                double percentage = (double)totalOrdered / totalSold * 100;

                int pointIndex = series.Points.AddXY(foodName, percentage);
                series.Points[pointIndex].Tag = totalOrdered;
                series.Points[pointIndex].Label = $"{percentage:F1}%";
                series.Points[pointIndex].LegendText = foodName;
            }
        }


        private void LoadChartDrinks(DateTime checkIn, DateTime checkOut)
        {
            DataTable data = BillInfoDAO.Instance.GetTop8MostOrderedDrinks(checkIn, checkOut);
            if (checkOut < checkIn)
            {
                return;
            }
            if (data == null || data.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để hiển thị.");
                cDrinks.Series.Clear();
                return;
            }
           

            int totalSold = 0;
            foreach (DataRow row in data.Rows)
            {
                totalSold += (int)row["TotalOrdered"];
            }

            if (cDrinks.Series.IsUniqueName("MostOrderedFoods"))
            {
                var mostOrderedFoodsSeries = new Series("MostOrderedFoods")
                {
                    ChartType = SeriesChartType.Pie
                };
                cDrinks.Series.Add(mostOrderedFoodsSeries);
            }

            var series = cDrinks.Series["MostOrderedFoods"];
            series.Points.Clear();

            foreach (DataRow row in data.Rows)
            {
                string foodName = row["FoodName"].ToString();
                int totalOrdered = (int)row["TotalOrdered"];
                double percentage = (double)totalOrdered / totalSold * 100;

                int pointIndex = series.Points.AddXY(foodName, percentage);
                series.Points[pointIndex].Tag = totalOrdered;
                series.Points[pointIndex].Label = $"{percentage:F1}%";
                series.Points[pointIndex].LegendText = foodName;
            }
        }



        private decimal CalculateTotalRevenue()
        {
            decimal totalRevenue = 0;

            // Duyệt qua từng hàng trong DataGridView
            foreach (DataGridViewRow row in dgThongKe.Rows)
            {
                // Kiểm tra nếu hàng không phải hàng trống
                if (row.Cells["tongtien"].Value != null)
                {
                    // Chuyển đổi giá trị thành decimal và cộng vào tổng
                    totalRevenue += Convert.ToDecimal(row.Cells["tongtien"].Value);
                }
            }

            return totalRevenue;
        }


        private void dtpkToDate_ValueChanged(object sender, EventArgs e)
            {

            }

            private void tcAdmin_SelectedIndexChanged(object sender, EventArgs e)
            {

            }



            private void tpBill_Click(object sender, EventArgs e)
            {

            }



            private void fAdmin_Load(object sender, EventArgs e)
            {

            }

            private void ThongKe_Click(object sender, EventArgs e)
            {
                DateTime checkIn = dptTuNgay.Value;
                DateTime checkOut = dptDenNgay.Value;

                // Xác nhận giá trị ngày
                Console.WriteLine($"CheckIn: {checkIn.ToString("yyyy-MM-dd")}, CheckOut: {checkOut.ToString("yyyy-MM-dd")}");


                LoadListBillByDate(checkIn, checkOut);
                LoadChartDrinks(checkIn, checkOut);
                LoadChartFood(checkIn, checkOut);
                decimal totalRevenue = CalculateTotalRevenue();
                txbTongDoanhThu.Text = totalRevenue.ToString("C", new CultureInfo("vi-VN")); // Định dạng tiền tệ và gán vào TextBox
             }

            private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
            {

            }

            private void panel30_Paint(object sender, PaintEventArgs e)
            {

            }

            private void btnShowFood_Click(object sender, EventArgs e)
            {
                LoadListFood();
            }

        private void nmPrice_ValueChanged(object sender, EventArgs e)
        {
            // Cập nhật giá trị hiển thị trong txbFormattedPrice khi giá trị thay đổi
            UpdateFormattedPrice(nmPrice.Value);
        }
        private void nmPrice_KeyUp(object sender, KeyEventArgs e)
        {
            // Cập nhật giá trị hiển thị trong txbFormattedPrice khi người dùng nhập vào
            UpdateFormattedPrice(nmPrice.Value);
        }

        private void UpdateFormattedPrice(decimal priceValue)
        {
            // Định dạng giá trị theo định dạng số với dấu phân cách hàng nghìn
            string formattedPrice = String.Format(CultureInfo.InvariantCulture, "{0:N0}", priceValue);

            // Cập nhật giá trị hiển thị trong txbFormattedPrice
            txbFormattedPrice.Text = formattedPrice;
            txbFormattedPrice.Select(txbFormattedPrice.Text.Length, 0); // Đặt con trỏ ở cuối
        }
        private void txbFormattedPrice_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txbFormattedPrice.Text.Replace('.', ','), NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out decimal newValue))
            {
                nmPrice.Value = newValue; // Cập nhật giá trị của nmPrice
            }
        }
        void LoatCategoryIntoCombobox()
        {
            cbFoodCategory.DataSource = CategoryDAO.Instance.GetListCategory();
            cbFoodCategory.DisplayMember = "Name";
        }
        void LoadTpyeIntoCombobox()
        {
            cbType.DataSource = AccountTypeDAO.Instance.GetListType();
            cbType.DisplayMember = "Name";
        }
        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void cbFoodCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
    
        }

        private void txbFoodID_TextChanged(object sender, EventArgs e)
        {
            // Kiểm tra xem có ô nào được chọn trong DataGridView không
            if (dtgvFood.SelectedCells.Count > 0)
            {
                // Lấy hàng đang được chọn
                var selectedRow = dtgvFood.SelectedCells[0].OwningRow;

                // Kiểm tra nếu hàng đang chọn không phải là null
                if (selectedRow != null)
                {
                    // Lấy tên danh mục từ ô đã chọn
                    string name = selectedRow.Cells["TenDanhMuc"].Value?.ToString(); // Sử dụng toán tử ?. để tránh NullReferenceException

                    // Kiểm tra xem name có null hay không
                    if (string.IsNullOrEmpty(name))
                    {
                        cbFoodCategory.SelectedItem = null; // Nếu name là null hoặc rỗng, đặt cbFoodCategory về null
                    }
                    else
                    {
                        // Lấy category dựa trên tên
                        Category category = CategoryDAO.Instance.GetCategoryByName(name);

                        // Kiểm tra category có hợp lệ không
                        if (category != null)
                        {
                            // Tìm chỉ số của danh mục trong ComboBox
                            int index = -1;
                            for (int i = 0; i < cbFoodCategory.Items.Count; i++)
                            {
                                if (((Category)cbFoodCategory.Items[i]).Name == category.Name) // So sánh tên danh mục
                                {
                                    index = i;
                                    break;
                                }
                            }

                            // Nếu tìm thấy, chọn danh mục trong ComboBox
                            if (index != -1)
                            {
                                cbFoodCategory.SelectedIndex = index; // Cập nhật chỉ số đã chọn
                            }
                        }
                        else
                        {
                            cbFoodCategory.SelectedItem = null; // Nếu không tìm thấy danh mục, đặt cbFoodCategory về null
                        }
                    }
                }
            }
            else
            {
                cbFoodCategory.SelectedItem = null; // Nếu không có ô nào được chọn, đặt cbFoodCategory về null
            }
        }


        private void dtgvFood_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnAddFood_Click(object sender, EventArgs e)
        {
            string name = txbFoodName.Text;
            int categoryID = CategoryDAO.Instance.GetCategoryIdByName(cbFoodCategory.Text.ToString()); // Lấy ID từ tên danh mục

            // Kiểm tra xem categoryID có hợp lệ không
            if (categoryID == -1)
            {
                MessageBox.Show("Danh mục không hợp lệ. Vui lòng chọn một danh mục hợp lệ.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Giả sử giá là float
            float price = (float)nmPrice.Value;

            // Thực hiện thêm món ăn
            if (FoodDAO.Instance.InsertFood(name, categoryID, price))
            {
                MessageBox.Show("Thêm món ăn thành công!");
                LoadListFood();
                if (insertFood != null)
                    insertFood(this, new EventArgs());
            }
            else
            {
                MessageBox.Show("Thêm món ăn không thành công. Vui lòng kiểm tra lại.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEditFood_Click(object sender, EventArgs e)
        {
            string name = txbFoodName.Text;
            int categoryID = CategoryDAO.Instance.GetCategoryIdByName(cbFoodCategory.Text.ToString()); // Lấy ID từ tên danh mục

            // Kiểm tra xem categoryID có hợp lệ không
            if (categoryID == -1)
            {
                MessageBox.Show("Danh mục không hợp lệ. Vui lòng chọn một danh mục hợp lệ.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int id = Convert.ToInt32(txbFoodID.Text);
            // Giả sử giá là float
            float price = (float)nmPrice.Value;

            // Thực hiện thêm món ăn
            if (FoodDAO.Instance.UpdateFood(id, name, categoryID, price))
            {
                MessageBox.Show("Sửa món ăn thành công!");
                LoadListFood();
                if (updateFood != null)
                    updateFood(this, new EventArgs());
            }
            else
            {
                MessageBox.Show("Sửa món ăn không thành công. Vui lòng kiểm tra lại.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteFood_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(txbFoodID.Text);
            if (FoodDAO.Instance.DeleteFood(id))
            {
                MessageBox.Show("Xóa món ăn thành công!");
                LoadListFood();
                if(deleteFood != null)
                    deleteFood (this, new EventArgs());
            }
            else
            {
                MessageBox.Show("Xóa món ăn không thành công. Vui lòng kiểm tra lại.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private event EventHandler insertFood;
        public event EventHandler InsertFood
        {
            add { insertFood += value; }
            remove { insertFood -= value; }
        }

        private event EventHandler deleteFood;
        public event EventHandler DeleteFood
        {
            add { deleteFood += value; }
            remove { deleteFood -= value; }
        }

        private event EventHandler updateFood;
        public event EventHandler UpdateFood
        {
            add { updateFood += value; }
            remove { updateFood -= value; }
        }
        public DataTable SearFoodByName(string name)
        {
            return FoodDAO.Instance.SearFoodByName(name);
        }
        private void btnSearchFood_Click(object sender, EventArgs e)
        {
            string searchText = txbSearchFoodName.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                // Set the DataSource of the DataGridView to the result of the search
                dtgvFood.DataSource = SearFoodByName(searchText);
            }
            else
            {
                // Show a message if the search box is empty
                MessageBox.Show("Vui lòng nhập tên món ăn để tìm kiếm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnShowFood_Click_1(object sender, EventArgs e)
        {
            LoadListFood();
        }

        private void btnShowCategory(object sender, EventArgs e)
        {
            fCategory categoryForm = new fCategory(this);
            categoryForm.ShowDialog();
        }

        private void btnShowAccount_Click(object sender, EventArgs e)
        {
            LoadAccount();
        }

        private void txbAccount_TextChanged(object sender, EventArgs e)
        {
            if (dtgvAccount.SelectedCells.Count > 0)
            {
                var selectedRow = dtgvAccount.SelectedCells[0].OwningRow;

                if (selectedRow != null)
                {
                    string name = selectedRow.Cells["NameAccountByType"].Value?.ToString();

                    

                    if (string.IsNullOrEmpty(name))
                    {
                        cbType.SelectedItem = null;
                    }
                    else
                    {
                        AccountType accountType = AccountTypeDAO.Instance.GetAccountTypeByNameType(name);

                       

                        if (accountType != null)
                        {
                            int index = -1;
                            for (int i = 0; i < cbType.Items.Count; i++)
                            {
                                // Lấy ra DataRowView từ ComboBox
                                DataRowView rowView = (DataRowView)cbType.Items[i];
                                if (rowView["Name"].ToString() == accountType.Name)
                                {
                                    index = i;
                                    break;
                                }
                            }

                            

                            if (index != -1)
                            {
                                cbType.SelectedIndex = index;
                            }
                        }
                        else
                        {
                            cbType.SelectedItem = null;
                        }
                    }
                }
            }
            else
            {
                cbType.SelectedItem = null;
            }
        }
        



        private void dtgvAccount_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
           
        }
        private bool IsEmailValid(string email)
        {
            // Sử dụng biểu thức chính quy để kiểm tra định dạng email
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }
        private void btnAddAccount_Click(object sender, EventArgs e)
        {
            string userName = txbAccount.Text;
            string displayName = txbDPN.Text;
            int IDType = AccountTypeDAO.Instance.GetAccountTypeIdByName(cbType.Text.ToString()); // Lấy ID từ tên danh mục
            string email = txbEmail.Text;

            // Kiểm tra tính hợp lệ của email
            if (!IsEmailValid(email))
            {
                MessageBox.Show("Email không hợp lệ. Vui lòng kiểm tra lại.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Thoát khỏi phương thức nếu email không hợp lệ
            }

            // Kiểm tra xem categoryID có hợp lệ không
            if (IDType == -1)
            {
                MessageBox.Show("Loại tài khoản không hợp lệ. Vui lòng chọn một loại tài khoản hợp lệ.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Hiển thị thông báo xác nhận
            DialogResult result = MessageBox.Show($"Bạn có chắc muốn thêm tài khoản: {userName}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Thực hiện thêm tài khoản với email
                bool success = AccountDAO.Instance.InsertAccount(userName, displayName, IDType, email);

                // Thông báo kết quả
                if (success)
                {
                    MessageBox.Show("Thêm tài khoản thành công!");
                    // Có thể làm mới danh sách tài khoản hoặc thực hiện các hành động khác sau khi thêm tài khoản
                }
                else
                {
                    MessageBox.Show("Thêm tài khoản thất bại. Vui lòng kiểm tra lại thông tin.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Nếu người dùng chọn No, không làm gì thêm
                return;
            }

            LoadAccount(); // Tải lại danh sách tài khoản sau khi thêm thành công
        }



        private void btnEditAccount_Click(object sender, EventArgs e)
        {
            string userName = txbAccount.Text;
            string displayName = txbDPN.Text;
            int IDType = AccountTypeDAO.Instance.GetAccountTypeIdByName(cbType.Text.ToString());
            string email = txbEmail.Text; // Lấy email từ TextBox

            // Kiểm tra tính hợp lệ của email
            if (!IsEmailValid(email))
            {
                MessageBox.Show("Email không hợp lệ. Vui lòng kiểm tra lại.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Dừng thực hiện nếu email không hợp lệ
            }

            DialogResult result = MessageBox.Show($"Bạn có chắc muốn cập nhật tài khoản: {userName}?", "Xác Nhận", MessageBoxButtons.OKCancel);

            // Nếu người dùng nhấn OK, tiếp tục cập nhật tài khoản
            if (result == DialogResult.OK)
            {
                bool success = AccountDAO.Instance.EditAccount(userName, displayName, IDType, email); // Thêm email vào tham số

                if (success)
                {
                    MessageBox.Show("Cập nhật tài khoản thành công!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Có thể thêm mã để làm mới danh sách tài khoản hoặc thực hiện các hành động khác sau khi cập nhật
                    LoadAccount();
                }
                else
                {
                    MessageBox.Show("Cập nhật tài khoản thất bại. Vui lòng kiểm tra lại thông tin.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }




        private void btnDeleteAccount_Click(object sender, EventArgs e)
        {
            string userName = txbAccount.Text;
            DialogResult result = MessageBox.Show($"Bạn Có Chắc Muốn Xóa Tài Khoản: {userName} ?", "Xác Nhận", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                bool success = AccountDAO.Instance.DeleteAccount(userName);

                if (success)
                {
                    MessageBox.Show("Xóa tài khoản thành công!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Có thể thêm mã để làm mới danh sách tài khoản hoặc thực hiện các hành động khác sau khi cập nhật
                }
                else
                {
                    MessageBox.Show("Xóa tài khoản thất bại. Vui lòng kiểm tra lại.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            LoadAccount();
        }

        private void btnResetPassword_Click(object sender, EventArgs e)
        {
            string userName = txbAccount.Text; 

            // Kiểm tra xem người dùng đã nhập tên người dùng chưa
            if (string.IsNullOrWhiteSpace(userName))
            {
                MessageBox.Show("Vui lòng nhập tên người dùng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Hiển thị thông báo xác nhận
            DialogResult dialogResult = MessageBox.Show($"Bạn có chắc muốn reset mật khẩu cho tài khoản: {userName}?", "Xác nhận", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

            // Nếu người dùng chọn OK, thực hiện reset mật khẩu
            if (dialogResult == DialogResult.OK)
            {
                bool resetResult = AccountDAO.Instance.ResetPassword(userName); // Gọi hàm ResetPassword

                if (resetResult)
                {
                    MessageBox.Show("Đặt lại mật khẩu thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Đặt lại mật khẩu thất bại. Vui lòng kiểm tra lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void LoadActiveTables()
        {
            // Lấy danh sách bàn đang hoạt động
            List<Table> activeTables = TableDAO.Instance.GetActiveTables();

            // Chuyển đổi danh sách thành DataTable để dễ dàng gán vào DataGridView
            DataTable tableData = new DataTable();
            tableData.Columns.Add("ID", typeof(int));
            tableData.Columns.Add("Name", typeof(string));
            tableData.Columns.Add("Status", typeof(string));

            foreach (Table table in activeTables)
            {
                tableData.Rows.Add(table.ID, table.Name, table.Status); 
            }

            // Gán DataTable vào DataGridView
            dtgvTable.DataSource = tableData;
            AddTableBinding();
        }
        void AddTableBinding()
        {
            // Xóa ràng buộc cũ nếu có trước khi thêm ràng buộc mới
            txbIdTable.DataBindings.Clear();
            txbTableName.DataBindings.Clear();
           

            // Kiểm tra nếu DataSource của DataGridView không null
            if (dtgvTable.DataSource != null)
            {
                // Thiết lập ràng buộc cho textbox từ DataGridView
                txbIdTable.DataBindings.Add(new Binding("Text", dtgvTable.DataSource, "ID", true, DataSourceUpdateMode.Never));
                txbTableName.DataBindings.Add(new Binding("Text", dtgvTable.DataSource, "Name", true, DataSourceUpdateMode.Never));
                
            }
        }
        private void btnAddTable_Click(object sender, EventArgs e)
        {
            string name = txbTableName.Text;
          
            if (TableDAO.Instance.AddTable(name))
            {
                MessageBox.Show("Thêm bàn thành công!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadActiveTables();
                
            }
            else
            {
                MessageBox.Show("Thêm bàn không thành công. Vui lòng kiểm tra lại.","Thông Báo",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteTable_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem có bàn nào được chọn không
            if (string.IsNullOrEmpty(txbIdTable.Text))
            {
                MessageBox.Show("Vui lòng chọn bàn cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idTable = int.Parse(txbIdTable.Text); // Lấy ID bàn từ TextBox

            // Gọi phương thức xóa bàn
            if (TableDAO.Instance.DeleteTable(idTable))
            {
                MessageBox.Show("Xóa bàn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadActiveTables(); // Làm mới danh sách bàn
            }
            else
            {
                MessageBox.Show("Xóa bàn không thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnEditTable_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem có bàn nào được chọn không
            if (string.IsNullOrEmpty(txbIdTable.Text))
            {
                MessageBox.Show("Vui lòng chọn bàn cần sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idTable = int.Parse(txbIdTable.Text); // Lấy ID bàn từ TextBox
            string tableName = txbTableName.Text; // Lấy tên bàn từ TextBox

            if (string.IsNullOrWhiteSpace(tableName))
            {
                MessageBox.Show("Vui lòng nhập tên bàn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Gọi phương thức sửa bàn
            if (TableDAO.Instance.UpdateTable(idTable, tableName))
            {
                MessageBox.Show("Cập nhật bàn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadActiveTables(); // Làm mới danh sách bàn
            }
            else
            {
                MessageBox.Show("Cập nhật bàn không thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Chart_MouseClick(object sender, EventArgs e)
        {
            // Chuyển đổi EventArgs sang MouseEventArgs
            MouseEventArgs mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs == null) return;

            // Xác định biểu đồ nào được nhấp vào
            var chart = sender as Chart;
            if (chart == null) return;

            // Xóa highlight khỏi biểu đồ không được nhấp vào
            Chart otherChart = (chart == cFoods) ? cDrinks : cFoods;
            if (otherChart.Series.IndexOf("MostOrderedFoods") >= 0)
            {
                foreach (var point in otherChart.Series["MostOrderedFoods"].Points)
                {
                    point.BorderWidth = 1; // Đặt lại BorderWidth về mặc định
                    point.BorderColor = Color.Transparent; // Đặt lại màu viền về trong suốt
                }
            }

            // Kiểm tra tọa độ nhấp chuột trên biểu đồ được chọn
            HitTestResult result = chart.HitTest(mouseEventArgs.X, mouseEventArgs.Y);

            // Kiểm tra nếu nhấp vào một điểm dữ liệu
            if (result.ChartElementType == ChartElementType.DataPoint)
            {
                // Lấy chỉ số của điểm dữ liệu
                int pointIndex = result.PointIndex;
                Series series = chart.Series["MostOrderedFoods"];

                // Lấy giá trị totalOrdered từ dữ liệu tương ứng
                int totalOrdered = (int)series.Points[pointIndex].Tag;

                // Gán số lượng bán được (totalOrdered) vào TextBox
                txbSoLuong.Text = totalOrdered.ToString();

                // Bỏ highlight khỏi tất cả các điểm trước đó trong series của biểu đồ được chọn
                foreach (var point in series.Points)
                {
                    point.BorderWidth = 1; // Đặt lại BorderWidth về mặc định
                    point.BorderColor = Color.Transparent; // Đặt lại màu viền về trong suốt
                }

                // Làm nổi bật điểm được chọn
                var selectedPoint = series.Points[pointIndex];
                selectedPoint.BorderWidth = 3; // Tăng độ rộng viền để làm nổi bật
                selectedPoint.BorderColor = Color.Red; // Đặt màu viền nổi bật là đỏ
            }
        }



        private void dptTuNgay_ValueChanged(object sender, EventArgs e)
        {

        }

        private void panel20_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label18_Click(object sender, EventArgs e)
        {

        }
    }

}

