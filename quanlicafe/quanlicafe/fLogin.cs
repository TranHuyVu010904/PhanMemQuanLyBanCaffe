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

namespace QuanLyQuanCafe

{
    public partial class fLogin : Form
    {
        public fLogin()
        {
            InitializeComponent();
            txbUserName.KeyDown += new KeyEventHandler(txbUserName_KeyDown);
            txbPassWord.KeyDown += new KeyEventHandler(txbPassWord_KeyDown);    
        }

        

        bool Login(string userName, string passWord)
        {
            return AccountDAO.Instance.Login(userName, passWord);
        }

        private void fLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Kiểm tra nếu ứng dụng đang thoát để tránh thông báo xác nhận hiển thị lại
            if (fLogin.isLoggingOut)
            {
                return; // Không hiển thị thông báo xác nhận khi đóng form fLogin sau khi đăng xuất
            }

            // Nếu người dùng không chọn đăng xuất, hiển thị thông báo xác nhận
            var result = MessageBox.Show("Bạn có chắc chắn muốn thoát chương trình?", "Xác nhận", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                // Nếu người dùng chọn "Yes", thoát ứng dụng
                Application.Exit();
            }
            else
            {
                // Nếu người dùng chọn "No", hủy hành động đóng form
                e.Cancel = true;
            }
        }

        private void fLogin_Load(object sender, EventArgs e)
        {

        }

       
  

        private void btnExit_Click_1(object sender, EventArgs e)
        {
            
        }

        private void txbUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Gọi hàm đăng nhập
                btnLogin_Click_2(sender, e);
                e.Handled = true; // Ngăn chặn âm thanh beep khi nhấn Enter
            }

        }

        private void txbPassWord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Gọi hàm đăng nhập
                btnLogin_Click_2(sender, e);
            }
        }
        public static bool isLoggingOut = false;
        private void btnLogin_Click_2(object sender, EventArgs e)
        {
            string userName = txbUserName.Text.Trim();
            string passWord = txbPassWord.Text.Trim();

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passWord))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập và mật khẩu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra đăng nhập
            if (Login(userName, passWord))
            {
                int accountType = AccountDAO.Instance.GetAccountType(userName);

                if (accountType == 1)
                {
                    fAdmin admin = new fAdmin(userName); // Truyền tên tài khoản hiện tại
                    this.Hide();
                    admin.ShowDialog();
                    this.Show();
                    txbUserName.Clear();
                    txbPassWord.Clear();
                    
                }
                else if (accountType == 2)
                {
                    fTableManager f = new fTableManager(userName);
                    this.Hide();

                    // Đăng ký sự kiện FormClosed chỉ một lần
                    f.FormClosed += (s, args) =>
                    {
                        // Kiểm tra nếu form đăng nhập chưa được mở
                        var loginForm = Application.OpenForms.OfType<fLogin>().FirstOrDefault();
                        if (loginForm == null || loginForm.IsDisposed) // Nếu form đăng nhập chưa tồn tại hoặc đã bị đóng
                        {
                            // Mở lại form đăng nhập
                            fLogin fLoginForm = new fLogin();
                            fLoginForm.Show();
                        }
                    };

                    f.ShowDialog();  // Mở fTableManager, sau khi đóng sẽ quay lại form đăng nhập.
                    txbUserName.Clear();
                    txbPassWord.Clear();
                }

                else
                {
                    MessageBox.Show("Loại tài khoản không hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            fEmailSender forgotPasswordForm = new fEmailSender();
            forgotPasswordForm.Show();
        }

        private void txbPassWord_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
            isLoggingOut = true;
        }
    }
}
