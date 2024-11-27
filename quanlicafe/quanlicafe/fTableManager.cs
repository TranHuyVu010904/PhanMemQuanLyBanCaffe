using QuanLyQuanCafe.DAO;
using QuanLyQuanCafe.DTO;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace QuanLyQuanCafe
{
    public partial class fTableManager : Form
    {
        private string displayName;
        private string userName;
        public fTableManager(string userName)
        {
            
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.FormClosing += new FormClosingEventHandler(this.fTableManager_FormClosing);

            nmDisCount.Enabled = false;
            txbTotalPrice.Enabled = false;
            this.userName = userName; // Lưu giá trị userName
            this.displayName = AccountDAO.Instance.GetDisplayName(userName);
            LoadTable();
            LoadCategory();
            LoadComboboxTable(cbSwitchTable);
            LoadAccountInfo();
            nmDisCount.ValueChanged += nmDisCount_ValueChanged;
            nmDisCount.KeyUp += nmDisCount_KeyUp;
         
            fAdmin adminForm = new fAdmin(userName);

            // Đăng ký các sự kiện từ fAdmin
            adminForm.InsertFood += HandleInsertFood;
            adminForm.UpdateFood += HandleUpdateFood;
            adminForm.DeleteFood += HandleDeleteFood;


        }

        private bool isExiting = false;  // Cờ để tránh gọi Application.Exit nhiều lần

        private void fTableManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Kiểm tra nếu ứng dụng đang thoát để tránh thông báo xác nhận hiển thị lại
            if (isExiting)
            {
                return; // Không hiển thị thông báo xác nhận
            }

            // Hiển thị thông báo xác nhận khi người dùng muốn đóng form
            var result = MessageBox.Show("Bạn có chắc chắn muốn thoát?", "Xác nhận", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                // Nếu người dùng chọn "Yes", đặt cờ đăng xuất và thoát ứng dụng
                fLogin.isLoggingOut = true; // Đánh dấu đăng xuất
                isExiting = true;  // Đặt cờ để tránh thông báo xác nhận hiển thị lại
                Application.Exit(); // Thoát ứng dụng
            }
            else
            {
                e.Cancel = true;

                fLogin.isLoggingOut = false;
            }
        }



        #region Method
        void LoadAccountInfo()
        {
            // Giả sử bạn có một Label tên là lblAccountInfo để hiển thị thông tin tài khoản
            lblAccountInfo.Text = "Xin chào, " + displayName;
        }
        void LoadTable()
        {
            flpTable.Controls.Clear();  // Xóa các button cũ trong FlowLayoutPanel
            List<Table> tableList = TableDAO.Instance.LoadTableList();

            bool showMuaMangVe = !ckbMangve.Checked; // Nếu checkbox không được chọn thì mới hiển thị "Mang Về"

            foreach (Table item in tableList)
            {
                // Nếu bàn có ID là 105, hoặc tên bàn là "Mang Về" và checkbox đã được chọn, bỏ qua bàn đó hoàn toàn
                if ((item.ID == 105) || (item.Name == "Mang Về" && !showMuaMangVe))
                {
                    continue; // Bỏ qua bàn có ID = 105 hoặc "Mang Về" nếu checkbox đã được chọn
                }

                // Tạo button cho mỗi bàn
                Button btn = new Button()
                {
                    Width = TableDAO.TableWidth,
                    Height = TableDAO.TableHeight,
                    Text = item.Name + Environment.NewLine + item.Status,
                    Tag = item  // Lưu thông tin bàn vào thuộc tính Tag
                };

                // Gắn sự kiện Click cho button
                btn.Click += btn_Click;

                // Cập nhật màu sắc theo trạng thái bàn
                switch (item.Status)
                {
                    case "Trống":
                        btn.BackColor = Color.LightGray;
                        break;
                    case "Đang sử dụng":
                        btn.BackColor = Color.Brown;
                        break;
                    default:
                        btn.BackColor = Color.Gray;
                        break;
                }

                // Thêm button vào FlowLayoutPanel
                flpTable.Controls.Add(btn);
            }
        }

        private void HandleMuaMangVeClick()
        {
            // Kiểm tra trạng thái của checkbox
            if (ckbMangve.Checked)
            {
                // Lấy trạng thái của bàn "Mang Về" (giả sử bạn có một phương thức lấy bàn theo tên)
                Table muaMangVeTable = TableDAO.Instance.GetTableByName("Mang Về");

                // Kiểm tra trạng thái của bàn "Mang Về"
                if (muaMangVeTable.Status == "Đang sử dụng")
                {
                    // Nếu bàn đang có người, vô hiệu hóa checkbox và thông báo
                    ckbMangve.Enabled = false;
                    MessageBox.Show("Bàn 'Mang Về' hiện đang có người sử dụng. Không thể chọn bàn này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Nếu bàn trống, tiếp tục xử lý như bình thường
                muaMangVeTable.Status = "Đang sử dụng";  // Thay đổi trạng thái bàn

                // Tạo một Button cho bàn "Mang Về"
                Button btn = new Button()
                {
                    Width = TableDAO.TableWidth,
                    Height = TableDAO.TableHeight,
                    Text = muaMangVeTable.Name + Environment.NewLine + muaMangVeTable.Status,
                    Tag = muaMangVeTable  // Lưu thông tin bàn vào thuộc tính Tag của Button
                };

                // Gắn sự kiện Click cho button
                btn.Click += btn_Click;

                // Cập nhật màu sắc theo trạng thái bàn
                switch (muaMangVeTable.Status)
                {
                    case "Trống":
                        btn.BackColor = Color.LightGray;
                        break;
                    case "Đang sử dụng":
                        btn.BackColor = Color.Brown;
                        break;
                    default:
                        btn.BackColor = Color.Gray;
                        break;
                }

                // Gọi sự kiện Click cho bàn "Mang Về"
                btn_Click(btn, EventArgs.Empty);  // Gọi sự kiện Click với Button giả lập
            }
            else
            {
                // Nếu checkbox không được chọn, gán Tag của Button là null
                Button btn = new Button()
                {
                    Tag = null  // Gán giá trị null cho Tag
                };

                // Xóa hoặc ẩn lsvBill khi checkbox không được chọn
                lsvBill.Items.Clear();  // Xóa danh sách trong lsvBill
                lsvBill.Tag = null;     // Đặt Tag của lsvBill là null

                // Cập nhật lại trạng thái của các điều khiển khác
                nmDisCount.Enabled = false;
                txbTotalPrice.Enabled = false;

                // Có thể xử lý sự kiện Click theo logic cần thiết ở đây
                btn_Click(btn, EventArgs.Empty);  // Có thể làm điều gì đó với Button này nếu cần
            }
        }




        private void nmDisCount_KeyUp(object sender, KeyEventArgs e)
        {
            // Cập nhật giá tiền khi người dùng nhập từ bàn phím
            
                UpdateTotalPrice();
            
        }
        private void UpdateTotalPrice()
        {
            // Kiểm tra nếu chưa chọn bàn
            Table table = lsvBill.Tag as Table; // Giả sử bạn đang lưu thông tin bàn trong Tag
            if (table == null)
            {
                MessageBox.Show("Vui lòng chọn bàn trước khi tính toán tổng tiền.", "Chưa chọn bàn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txbTotalPrice.Text = ""; // Xóa giá trị tổng tiền nếu chưa chọn bàn
                return;
            }

            // Lấy ID hóa đơn từ bảng
            int tableID = table.ID;

            // Truy vấn tổng tiền từ cơ sở dữ liệu
            float totalPrice = GetTotalPriceFromDatabase(tableID);

            // Lấy giá trị giảm giá từ nmDisCount
            int discount = (int)nmDisCount.Value;

            // Tính toán giá trị cuối cùng
            double finalTotalPrice = totalPrice - (totalPrice / 100) * discount;

            // Cập nhật lại txbTotalPrice với giá trị mới
            CultureInfo culture = new CultureInfo("vi-VN");
            txbTotalPrice.Text = finalTotalPrice.ToString("c", culture);
        }

        private float GetTotalPriceFromDatabase(int tableID)
        {
            float totalPrice = 0;

            // Truy vấn tổng tiền từ cơ sở dữ liệu (ví dụ: sử dụng MenuDAO)
            List<QuanLyQuanCafe.DTO.Menu> listBillInfo = MenuDAO.Instance.GetListMenuByTable(tableID);
            foreach (QuanLyQuanCafe.DTO.Menu item in listBillInfo)
            {
                totalPrice += item.TotalPrice;
            }

            return totalPrice;
        }

        void ShowBill(int id)
        {
            // Kiểm tra nếu không có bàn hợp lệ (lsvBill.Tag == null hoặc bàn không hợp lệ)
            if (lsvBill.Tag == null)
            {
                MessageBox.Show("Vui lòng chọn bàn hợp lệ để xem hóa đơn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lsvBill.Items.Clear();
            List<QuanLyQuanCafe.DTO.Menu> listBillInfo = MenuDAO.Instance.GetListMenuByTable(id);
            float totalPrice = 0;

            // Hiển thị thông tin món ăn trong lsvBill
            foreach (QuanLyQuanCafe.DTO.Menu item in listBillInfo)
            {
                ListViewItem lsvItem = new ListViewItem(item.FoodName.ToString());
                lsvItem.SubItems.Add(item.Count.ToString());
                lsvItem.SubItems.Add(item.Price.ToString());
                lsvItem.SubItems.Add(item.TotalPrice.ToString());
                totalPrice += item.TotalPrice;
                lsvBill.Items.Add(lsvItem);
            }

            // Cập nhật tổng tiền vào txbTotalPrice
            CultureInfo culture = new CultureInfo("vi-VN");
            txbTotalPrice.Text = totalPrice.ToString("c", culture);
        }

        private void HandleInsertFood(object sender, EventArgs e)
        {
            LoadFoodListByCategoryID((cbCategory.SelectedItem as Category).ID);
            ShowBill((lsvBill.Tag as Table).ID);
        }

        private void HandleUpdateFood(object sender, EventArgs e)
        {
            LoadFoodListByCategoryID((cbCategory.SelectedItem as Category).ID);
            ShowBill((lsvBill.Tag as Table).ID);
        }

        private void HandleDeleteFood(object sender, EventArgs e)
        {
            LoadFoodListByCategoryID((cbCategory.SelectedItem as Category).ID);
            ShowBill((lsvBill.Tag as Table).ID);
            LoadTable();
        }

        void LoadComboboxTable(ComboBox cb)
        {
            cb.DataSource = TableDAO.Instance.LoadTableList();
            cb.DisplayMember = "Name";
        }

        void btn_Click(object sender, EventArgs e)
        {
            int tableID = ((sender as Button).Tag as Table)?.ID ?? 0;

            if (tableID == 0)
            {
                // Nếu không có bàn hợp lệ, không thực hiện gì
                MessageBox.Show("Vui lòng chọn bàn hợp lệ để xem hóa đơn.", "Không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lsvBill.Tag = (sender as Button).Tag;
            nmDisCount.Enabled = true;
            txbTotalPrice.Enabled = true;
            ShowBill(tableID);
        }


        void LoadCategory()
        {
            List<Category> listCategory = CategoryDAO.Instance.GetListCategory();
            cbCategory.DataSource = listCategory;
            cbCategory.DisplayMember = "Name";
        }

        void LoadFoodListByCategoryID(int id)
        {
            List<Food> listFood = FoodDAO.Instance.GetFoodByCategoryID(id);
            cbFood.DataSource = listFood;
            cbFood.DisplayMember = "Name";
        }

        

        private void lsvBill_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Handle selected index change event if needed
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // Handle paint event if needed
        }

        private void fTableManager_Load(object sender, EventArgs e)
        {
            // Handle form load event if needed
        }

        private void thôngTinTàiKhoảnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Sử dụng userName đã được lưu trữ
            fAccountProfile f = new fAccountProfile(userName);
            f.UpdateAccount += F_UpdateAccount; // Đăng ký sự kiện
            

            f.ShowDialog();
        }
        private void F_UpdateAccount(object sender, AccountEvent e)
        {
            // Cập nhật thông tin tài khoản trên fTableManager
            lblAccountInfo.Text = "Xin chào, " + e.Acc.DisplayName; // Hoặc cập nhật lại các thông tin khác
        }
        private void đăngXuấtToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            // Đặt cờ đăng xuất để tránh hiển thị thông báo xác nhận khi đóng form
            isExiting = true;

            // Đóng form fTableManager
            this.Hide();

            // Mở lại form đăng nhập
            fLogin fLoginForm = new fLogin();
            fLoginForm.Show();

            this.Close();  // Đóng form hiện tại (fTableManager)
        }
        private void cbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            int id = 0;

            ComboBox cb = sender as ComboBox;

            if (cb.SelectedItem == null)
                return;

            Category selected = cb.SelectedItem as Category;
            id = selected.ID;

            LoadFoodListByCategoryID(id);
        }
        private void btnAddFood_Click_1(object sender, EventArgs e)
        {
            // Kiểm tra nếu không có bàn được chọn
            if (lsvBill.Tag == null)
            {
                MessageBox.Show("Vui lòng chọn bàn trước khi thêm món ăn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra nếu không có món ăn được chọn
            if (cbFood.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn món ăn trước khi thêm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Table table = lsvBill.Tag as Table;

            // Kiểm tra nếu bàn là "Mang Về", vô hiệu hóa checkbox
            if (table != null && table.Name == "Mang Về")
            {
                ckbMangve.Enabled = false;  // Vô hiệu hóa checkbox
            }

            int idBill = BillDAO.Instance.GetUncheckBillIDByTableID(table.ID);
            Food selectedFood = cbFood.SelectedItem as Food;
            int foodID = selectedFood.ID;
            int count = (int)nmFoodCount.Value;

            if (count == 0)
            {
                MessageBox.Show("Số lượng món ăn thêm vào không thể bằng 0.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Nếu số lượng món ăn cần thêm là âm
            if (count < 0)
            {
                // Kiểm tra số lượng hiện tại của món ăn trong BillInfo
                int currentFoodCount = BillInfoDAO.Instance.GetFoodCountInBill(idBill, foodID);

                if (currentFoodCount + count < 0) // count ở đây là âm, nên cộng để kiểm tra
                {
                    MessageBox.Show("Số lượng món ăn không thể nhỏ hơn 0.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Thêm hoặc cập nhật món ăn vào hóa đơn
            if (idBill == -1)
            {
                BillDAO.Instance.InsertBill(table.ID);
                BillInfoDAO.Instance.InsertBillInfo(BillDAO.Instance.GetMaxIDBill(), foodID, count);
            }
            else
            {
                BillInfoDAO.Instance.InsertBillInfo(idBill, foodID, count);
            }

            ShowBill(table.ID);
            LoadTable();
        }
        #endregion
        private void btnCheckOut_Click(object sender, EventArgs e)
        {
            // Kiểm tra nếu không có bàn được chọn
            if (lsvBill.Tag == null)
            {
                MessageBox.Show("Vui lòng chọn bàn trước khi thanh toán.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Table table = lsvBill.Tag as Table;

            int idBill = BillDAO.Instance.GetUncheckBillIDByTableID(table.ID);
            int discount = (int)nmDisCount.Value;

            // Lấy tổng tiền đã tính ở phương thức UpdateTotalPrice
            string sanitizedInput = txbTotalPrice.Text.Replace(".", "").Replace(",", ".").Replace("₫", "").Trim();
            double finalTotalPrice = double.Parse(sanitizedInput, CultureInfo.InvariantCulture);

            if (idBill != -1)
            {
                if (MessageBox.Show(
                    string.Format("Bạn có chắc chắn muốn thanh toán cho bàn '{0}'?", table.Name),
                    "Thông báo",
                    MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                {
                    // Chuyển đổi sang float trước khi lưu
                    BillDAO.Instance.CheckOut(idBill, discount, (float)finalTotalPrice);

                    // Đặt giá trị giảm giá về 0 sau khi thanh toán
                    nmDisCount.Value = 0;

                    // Kiểm tra nếu bàn là "Mang Về", bật lại checkbox
                    if (table.Name == "Mang Về")
                    {
                        ckbMangve.Enabled = true;  // Bật lại checkbox sau khi thanh toán
                    }

                    ShowBill(table.ID);
                    LoadTable();
                }
            }
        }
        private void txbTotalPrice_TextChanged(object sender, EventArgs e)
        {

        }
        private void btnSwitchTable_Click(object sender, EventArgs e)
        {
            // Kiểm tra nếu không có bàn được chọn
            if (lsvBill.Tag == null)
            {
                MessageBox.Show("Vui lòng chọn bàn trước khi chuyển bàn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id1 = (lsvBill.Tag as Table).ID;

            int id2 = (cbSwitchTable.SelectedItem as Table).ID;
            if (MessageBox.Show(string.Format("Bạn có thật sự muốn chuyển bàn {0} qua bàn {1}", (lsvBill.Tag as Table).Name, (cbSwitchTable.SelectedItem as Table).Name), "Thông báo", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                TableDAO.Instance.SwitchTable(id1, id2);
                LoadTable();
            }
        }
        private void nmDisCount_ValueChanged(object sender, EventArgs e)
        {
            if (!nmDisCount.Enabled)
                return;

            UpdateTotalPrice();
        }
        private void khácToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fDailyRevenue fDailyRevenue = new fDailyRevenue();



            fDailyRevenue.ShowDialog();
        }

        private void flpTable_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel11_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void ckbmangve_Click(object sender, EventArgs e)
        {
           

            
            flpTable.Enabled = !ckbMangve.Checked;
            flpTable.BackColor = flpTable.Enabled ? Color.White : Color.LightGray;
        }

        private void cbSwitchTable_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ckbMangve_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbMangve.Checked)
            {
                // Gọi xử lý khi checkbox được chọn, hiển thị bàn "Mua Mang Về"
                HandleMuaMangVeClick();
            }
            else
            { 
                LoadTable(); 
            }
        }
    }
}
