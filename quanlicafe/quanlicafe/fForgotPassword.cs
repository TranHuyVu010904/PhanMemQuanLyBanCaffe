using QuanLyQuanCafe.DAO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace QuanLyQuanCafe
{
    public partial class fForgotPassword : Form
    {
        private string email;
        public fForgotPassword(string email)
        {
            InitializeComponent();
            this.email = email;
            SetUserNameFromEmail();
        }
        private void SetUserNameFromEmail()
        {
            // Lấy UserName dựa trên Email và gán vào txbUserName
            string userName = AccountDAO.Instance.GetUsernameByEmail(email);
            txbUserName.Text = userName; // Gán giá trị vào TextBox
        }

        private void btnXacNhan_Click(object sender, EventArgs e)
        {
            // Lấy giá trị mật khẩu từ các TextBox
            string newPassword = txbConfirmPassword.Text;
            string confirmPassword = txbNewPassword.Text;
            string userName = txbUserName.Text;

            // Kiểm tra xem mật khẩu mới và mật khẩu xác nhận có khớp không
            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp. Vui lòng thử lại.");
                return;
            }

            // Cập nhật mật khẩu trong cơ sở dữ liệu
            if (AccountDAO.Instance.UpdatePassword(userName, newPassword))
            {
                MessageBox.Show("Mật khẩu đã được cập nhật thành công.");
                this.Close(); // Đóng form sau khi cập nhật thành công
            }
            else
            {
                MessageBox.Show("Cập nhật mật khẩu thất bại. Vui lòng thử lại.");
            }
        }

        private void txbNewPassword_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
