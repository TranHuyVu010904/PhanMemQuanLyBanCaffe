using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms; // Đảm bảo bạn đã thêm không gian tên này
using QuanLyQuanCafe.DAO;

namespace QuanLyQuanCafe
{
    public partial class fVerifyCode : Form
    {
        private string email; // Thuộc tính để lưu email

        public fVerifyCode(string email)
        {
            InitializeComponent();
            this.email = email; // Gán email
            InitializeTextBoxEvents();
            txbNumber1.Focus(); // Đặt focus vào ô đầu tiên
            DisableAllTextBoxesExceptFirst();
        }

        private void InitializeTextBoxEvents()
        {
            // Thêm sự kiện cho tất cả các Guna2TextBox
            txbNumber1.KeyPress += new KeyPressEventHandler(txbNumber_KeyPress);
            txbNumber2.KeyPress += new KeyPressEventHandler(txbNumber_KeyPress);
            txbNumber3.KeyPress += new KeyPressEventHandler(txbNumber_KeyPress);
            txbNumber4.KeyPress += new KeyPressEventHandler(txbNumber_KeyPress);
            txbNumber5.KeyPress += new KeyPressEventHandler(txbNumber_KeyPress);
            txbNumber6.KeyPress += new KeyPressEventHandler(txbNumber_KeyPress);

            txbNumber1.TextChanged += new EventHandler(txbNumber_TextChanged);
            txbNumber2.TextChanged += new EventHandler(txbNumber_TextChanged);
            txbNumber3.TextChanged += new EventHandler(txbNumber_TextChanged);
            txbNumber4.TextChanged += new EventHandler(txbNumber_TextChanged);
            txbNumber5.TextChanged += new EventHandler(txbNumber_TextChanged);
            txbNumber6.TextChanged += new EventHandler(txbNumber6_TextChanged); // Chỉ định sự kiện riêng cho txbNumber6
        }

        private void DisableAllTextBoxesExceptFirst()
        {
            txbNumber2.Enabled = false;
            txbNumber3.Enabled = false;
            txbNumber4.Enabled = false;
            txbNumber5.Enabled = false;
            txbNumber6.Enabled = false;
        }

        private void EnableTextBox(Guna2TextBox textBox)
        {
            textBox.Enabled = true;
            textBox.Focus();
        }

        private void txbNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Kiểm tra xem ký tự nhập vào có phải là số hay không
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Không cho phép nhập ký tự không phải số
            }
        }

        private void txbNumber_TextChanged(object sender, EventArgs e)
        {
            Guna2TextBox currentTextBox = sender as Guna2TextBox;

            // Chuyển sang ô tiếp theo khi có ký tự
            if (currentTextBox.Text.Length == 1)
            {
                currentTextBox.Enabled = false; // Khóa ô hiện tại
                if (currentTextBox == txbNumber1) EnableTextBox(txbNumber2);
                else if (currentTextBox == txbNumber2) EnableTextBox(txbNumber3);
                else if (currentTextBox == txbNumber3) EnableTextBox(txbNumber4);
                else if (currentTextBox == txbNumber4) EnableTextBox(txbNumber5);
                else if (currentTextBox == txbNumber5) EnableTextBox(txbNumber6);
            }

        }

        private void txbNumber6_TextChanged(object sender, EventArgs e)
        {
            // Kiểm tra số lượng ký tự nhập vào txbNumber6
            if (txbNumber6.Text.Length == 1)
            {
                // Tạo chuỗi mã xác thực từ các ô nhập
                string inputCode = txbNumber1.Text + txbNumber2.Text + txbNumber3.Text +
                                   txbNumber4.Text + txbNumber5.Text + txbNumber6.Text;

                // Kiểm tra mã xác thực với email
                if (AccountDAO.Instance.CheckCode(inputCode, email)) // Sử dụng _email
                {
                    // Cập nhật mã đã sử dụng và chuyển sang fForgotPassword
                    AccountDAO.Instance.UpdateCodeUsed(inputCode);
                    fForgotPassword forgotPasswordForm = new fForgotPassword(email); // Sử dụng _email
                    forgotPasswordForm.Show();
                    this.Close(); // Đóng form hiện tại
                }
                else
                {
                    MessageBox.Show("Mã xác thực không hợp lệ.");
                    // Có thể quyết định có cần gọi lại mã xác thực hay không
                    AccountDAO.Instance.UpdateCodeUsed(inputCode); // Cập nhật mã đã sử dụng (nếu cần)
                    fEmailSender fEmailSender = new fEmailSender(); // Tạo lại form gửi email
                    fEmailSender.Show();
                    this.Close(); // Đóng form hiện tại
                }
            }
        }
    }
}
