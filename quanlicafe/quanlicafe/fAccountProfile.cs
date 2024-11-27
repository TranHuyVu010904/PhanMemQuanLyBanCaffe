using QuanLyQuanCafe.DAO;
using QuanLyQuanCafe.DTO;
using System;
using System.Windows.Forms;

namespace QuanLyQuanCafe
{
    public partial class fAccountProfile : Form
    {
        private Account loginAccount;

        public Account LoginAccount
        {
            get { return loginAccount; }
            set
            {
                loginAccount = value;
                ChangeAccount(loginAccount);
            }
        }

        // Constructor nhận vào username
        public fAccountProfile(string userName)
        {
            InitializeComponent();

            // Lấy thông tin tài khoản từ database dựa trên username
            Account acc = AccountDAO.Instance.GetAccountByUserName(userName);
            if (acc != null)
            {
                LoginAccount = acc; // Nếu tài khoản không null, lưu thông tin tài khoản
            }
            else
            {
                MessageBox.Show("Không tìm thấy tài khoản."); // Thông báo nếu không tìm thấy tài khoản
            }
        }

        void ChangeAccount(Account acc)
        {
            txbUserName.Text = acc.UserName; // Đổ username vào textbox
            txbDisplayName.Text = acc.DisplayName; // Đổ displayName vào textbox
        }

        void UpdateAccountInfo()
        {
            string displayName = txbDisplayName.Text;
            string password = txbPassWordOld.Text;
            string newpass = txbNewPass.Text;
            string reenterPass = txbReEnterPass.Text;
            string userName = txbUserName.Text;

            if (!newpass.Equals(reenterPass))
            {
                MessageBox.Show("Vui lòng nhập lại mật khẩu đúng với mật khẩu mới!");
            }
            else if (!AccountDAO.Instance.CheckOldPassword(userName, password))
            {
                MessageBox.Show("Mật khẩu cũ không đúng, vui lòng kiểm tra lại!");
            }
            else
            {
                if (AccountDAO.Instance.UpdateAccount(userName, displayName, password, newpass))
                {
                    MessageBox.Show("Cập nhật thành công");
                    // Cập nhật tài khoản mới
                    LoginAccount = AccountDAO.Instance.GetAccountByUserName(userName);
                    if (updateAccount != null)
                        updateAccount(this, new AccountEvent(LoginAccount));
                }
                else
                {
                    MessageBox.Show("Cập nhật thất bại");
                }
            }
        }


        private event EventHandler<AccountEvent> updateAccount;
        public event EventHandler<AccountEvent> UpdateAccount
        {
            add { updateAccount += value; }
            remove { updateAccount -= value; }
        }

      

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            UpdateAccountInfo();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    public class AccountEvent : EventArgs
    {
        public Account Acc { get; set; }

        public AccountEvent(Account acc)
        {
            this.Acc = acc;
        }
    }
}
