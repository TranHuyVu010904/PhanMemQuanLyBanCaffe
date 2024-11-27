using QuanLyQuanCafe.DAO;
using QuanLyQuanCafe.DTO;
using System;
using System.Data;
using System.Windows.Forms;

namespace QuanLyQuanCafe
{
    public partial class fCategory : Form
    {
        // Thêm thuộc tính CurrentCategory để lưu thông tin danh mục hiện tại
        private Category currentCategory;
        
        private fAdmin adminForm;

        public fCategory(fAdmin admin)
        {
            InitializeComponent();
            adminForm = admin;
            LoadCategoryList();
        }

        private void LoadCategoryList()
        {
            DataTable DTCategory = CategoryDAO.Instance.GetDataTableCategory();
            dgvCategory.DataSource = DTCategory;

            if (DTCategory.Rows.Count > 0)
            {
                dgvCategory.Columns["id"].HeaderText = "ID";
                dgvCategory.Columns["name"].HeaderText = "Tên Danh Mục";

                AddCategoryBinding(); // Đảm bảo rằng dữ liệu có sẵn trước khi binding
            }
            else
            {
                MessageBox.Show("Danh sách danh mục trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AddCategoryBinding()
        {
            txbCategoryID.DataBindings.Clear();
            txbCategoryName.DataBindings.Clear();

            if (dgvCategory.Rows.Count > 0)
            {
                // Sử dụng DataTable làm nguồn dữ liệu
                txbCategoryID.DataBindings.Add(new Binding("Text", dgvCategory.DataSource, "id", true, DataSourceUpdateMode.Never));
                txbCategoryName.DataBindings.Add(new Binding("Text", dgvCategory.DataSource, "name", true, DataSourceUpdateMode.Never));
            }
        }

        private void btnAddCategory_Click(object sender, EventArgs e)
        {
            string categoryName = txbCategoryName.Text; // Lấy tên danh mục từ TextBox

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                MessageBox.Show("Tên danh mục không được để trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (CategoryDAO.Instance.InsertCategory(categoryName)) // Gọi phương thức thêm
            {
                MessageBox.Show("Thêm danh mục thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadCategoryList(); // Tải lại danh sách để cập nhật
                adminForm.ReloadFoodAndCategoryData();
            }
            else
            {
                MessageBox.Show("Thêm danh mục thất bại. Vui lòng kiểm tra lại thông tin.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteCategory_Click(object sender, EventArgs e)
        {
            if (dgvCategory.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một danh mục để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int categoryId = Convert.ToInt32(dgvCategory.SelectedRows[0].Cells["id"].Value); // Lấy ID từ dòng đã chọn

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa danh mục này?", "Xác nhận", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                if (CategoryDAO.Instance.DeleteCategoryAddCategory(categoryId)) // Gọi phương thức xóa
                {
                    MessageBox.Show("Xóa danh mục thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadCategoryList(); // Tải lại danh sách để cập nhật
                    adminForm.ReloadFoodAndCategoryData();
                }
                else
                {
                    MessageBox.Show("Xóa danh mục thất bại. Vui lòng kiểm tra lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnUpdateCategory_Click(object sender, EventArgs e)
        {
            if (dgvCategory.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một danh mục để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string categoryName = txbCategoryName.Text; // Nhập từ TextBox
            

            // Chuyển đổi từ string sang int với xử lý lỗi
            if (!int.TryParse(txbCategoryID.Text, out int categoryId))
            {
                MessageBox.Show("ID danh mục không hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                MessageBox.Show("Tên danh mục không được để trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Gọi phương thức cập nhật
            if (CategoryDAO.Instance.UpdateCategory(categoryId, categoryName))
            {
                MessageBox.Show("Cập nhật danh mục thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadCategoryList(); // Tải lại danh sách để cập nhật
                adminForm.ReloadFoodAndCategoryData();
            }
            else
            {
                MessageBox.Show("Cập nhật danh mục thất bại. Vui lòng kiểm tra lại thông tin.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvCategory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
