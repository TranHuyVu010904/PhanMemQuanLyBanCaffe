using QuanLyQuanCafe.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace QuanLyQuanCafe.DAO
{
    public class AccountDAO
    {
        private static AccountDAO instance;

        public static AccountDAO Instance
        {
            get { if (instance == null) instance = new AccountDAO(); return instance; }
            private set { instance = value; }
        }

        private AccountDAO() { }

        public bool Login(string userName, string passWord)
        {
            string query = "USP_Login @userName , @passWord";

            DataTable result = DataProvider.Instance.ExecuteQuery(query, new object[] { userName, passWord });

            return result.Rows.Count > 0;
        }
        public bool CheckOldPassword(string userName, string password)
        {
            string query = "SELECT COUNT(*) FROM Account WHERE UserName = @userName AND PassWord = @password";
            int result = (int)DataProvider.Instance.ExecuteScalar(query, new object[] { userName, password });
            return result > 0;
        }
        public bool UpdateAccount(string userName, string displayName, string pass, string newPass)
        {
            int result = DataProvider.Instance.ExecuteNonQuery("exec USP_UpdateAccount @userName , @displayName , @password , @newPassword", new object[] { userName, displayName, pass, newPass });

            return result > 0;
        }
        public string GetDisplayName(string userName)
        {
            string query = "SELECT displayName FROM Account WHERE userName = @userName";

            DataTable result = DataProvider.Instance.ExecuteQuery(query, new object[] { userName });

            if (result.Rows.Count > 0)
            {
                return result.Rows[0]["displayName"].ToString();
            }

            // Trả về chuỗi rỗng hoặc giá trị mặc định nếu không tìm thấy
            return string.Empty;
        }
        public Account GetAccountByUserName(string userName)
        {
            // Tạo câu lệnh SQL để lấy thông tin tài khoản
            string query = "SELECT * FROM Account WHERE userName = @userName";
            object[] parameters = new object[] { userName };

            DataTable data = DataProvider.Instance.ExecuteQuery(query, parameters);

            if (data.Rows.Count > 0)
            {
                return new Account(data.Rows[0]);
            }
            return null; // Trả về null nếu không tìm thấy tài khoản
        }
        
        public DataTable GetListAccount()
        {
            return DataProvider.Instance.ExecuteQuery("SELECT a.UserName, a.DisplayName,a.Email , t.name AS NameAccountByType\r\nFROM Account a\r\nJOIN dbo.AccountType t ON a.type = t.ID");
        }
       
        public int GetAccountType(string userName)
        {
            string query = "SELECT type FROM Account WHERE userName = @userName";

            DataTable result = DataProvider.Instance.ExecuteQuery(query, new object[] { userName });

            if (result.Rows.Count > 0)
            {
                return Convert.ToInt32(result.Rows[0]["type"]);
            }

            // Trả về -1 nếu không tìm thấy tài khoản (hoặc có thể tùy chỉnh theo yêu cầu của bạn)
            return -1;
        }
        public bool InsertAccount(string name, string displayName, int type, string email)
        {
            try
            {
                // Câu truy vấn SQL để chèn tài khoản vào cơ sở dữ liệu
                string query = string.Format("INSERT INTO Account (UserName, DisplayName, Type, Email) VALUES (N'{0}', N'{1}', {2}, N'{3}')", name, displayName, type, email);

                // Thực hiện truy vấn
                int result = DataProvider.Instance.ExecuteNonQuery(query);
                return result > 0;
            }
            catch (SqlException ex)
            {
                // Kiểm tra mã lỗi để xác định lỗi tài khoản đã tồn tại
                if (ex.Number == 2627)
                {
                    MessageBox.Show("Tài khoản đã tồn tại. Vui lòng kiểm tra lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return false;
            }
        }
        public bool EditAccount(string userName, string displayName, int type, string email)
        {
            // Câu truy vấn SQL để cập nhật thông tin tài khoản
            string query = "UPDATE Account SET DisplayName = @displayName, Type = @type, Email = @Email WHERE UserName = @userName";

            // Thực hiện truy vấn và trả về kết quả
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { displayName, type, email, userName });

            // Kết quả trả về > 0 nếu có thay đổi trong cơ sở dữ liệu, ngược lại là false.
            return result > 0;
        }

        public bool DeleteAccount(string name)
        {
           
         
            string query = string.Format("delete dbo.Account where UserName = N'{0}' ", name);
            int result = DataProvider.Instance.ExecuteNonQuery(query);

            return result > 0;
        }
        public bool ResetPassword(string name)
        {
            string query = string.Format("update dbo.Account set password = N'0' where UserName = N'{0}' ", name);
            int result = DataProvider.Instance.ExecuteNonQuery(query);

            return result > 0;
        }
        public bool CheckEmailExists(string email)
        {
            string query = "SELECT COUNT(*) FROM Account WHERE Email = @Email";
            object result = DataProvider.Instance.ExecuteScalar(query, new object[] { email });
            return Convert.ToInt32(result) > 0;
        }
        public string GetUsernameByEmail(string email)
        {
        string query = "SELECT UserName FROM Account WHERE Email = @Email";
        DataTable result = DataProvider.Instance.ExecuteQuery(query, new object[] { email });

        if (result.Rows.Count > 0)
        {
            return result.Rows[0]["UserName"].ToString();
        }
        return null;
        }

        // Cập nhật mã xác thực trong bảng Account
        public void InsertVerificationCode(string userName, string email, string verificationCode)
        {
            // Đặt thời gian hết hạn cho mã xác thực (5 phút sau khi tạo)
            DateTime expirationTime = DateTime.Now.AddMinutes(5);

            string query = "INSERT INTO VerificationCodes (UserName, Email, Code, ExpirationTime, IsUsed) " +
                           "VALUES (@UserName, @Email, @VerificationCode, @ExpirationTime, 0)";

            DataProvider.Instance.ExecuteNonQuery(query, new object[] { userName, email, verificationCode, expirationTime });
        }
        public bool CheckCode(string code, string email)
        {
            // Chuỗi truy vấn SQL để kiểm tra mã xác thực
            string query = "SELECT COUNT(*) FROM VerificationCodes " +
                           "WHERE Code = @Code AND Email = @Email AND ExpirationTime > GETDATE() AND IsUsed = 0";

            // Thực thi truy vấn và lấy số lượng kết quả
            int count = (int)DataProvider.Instance.ExecuteScalar(query, new object[] { code, email });

            // Nếu số lượng lớn hơn 0, có nghĩa là mã xác thực hợp lệ
            return count > 0;
        }
        public void UpdateCodeUsed(string code)
        {
            string query = "UPDATE VerificationCodes SET IsUsed = 1 WHERE Code = @Code";
            DataProvider.Instance.ExecuteNonQuery(query, new object[] { code });
        }
        public bool UpdatePassword(string userName, string newPassword)
        {   
            // Chuỗi truy vấn SQL để cập nhật mật khẩu
            string query = "UPDATE Account SET PassWord = @Password WHERE UserName = @UserName";

            // Thực hiện truy vấn và trả về kết quả
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { newPassword, userName });

            // Trả về true nếu có bản ghi được cập nhật, ngược lại false
            return result > 0;
        }
    }
}
