using QuanLyQuanCafe.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyQuanCafe.DAO
{
    internal class AccountTypeDAO
    {
        private static AccountTypeDAO instance;

        public static AccountTypeDAO Instance
        {
            get { if (instance == null) instance = new AccountTypeDAO(); return AccountTypeDAO.instance; }
            private set { AccountTypeDAO.instance = value; }
        }

        private AccountTypeDAO() { }

        public AccountType GetAccountTypeByNameType(string name)
        {
            AccountType accountType = null; // Sử dụng AccountType thay vì Account
            string query = "SELECT * FROM AccountType WHERE name = @name"; // Sử dụng tham số

            // Sử dụng ExecuteQuery với tham số
            DataTable data = DataProvider.Instance.ExecuteQuery(query, new object[] { name });

            if (data.Rows.Count > 0)
            {
                // Tạo đối tượng AccountType từ dòng dữ liệu đầu tiên
                accountType = new AccountType(data.Rows[0]);
            }

            return accountType;
        }
        public DataTable GetListType()
        {
            return DataProvider.Instance.ExecuteQuery("SELECT * From AccountType");
        }
        public int GetAccountTypeIdByName(string name)
        {
            int id = -1; // Giá trị mặc định nếu không tìm thấy
            string query = "SELECT ID FROM AccountType WHERE name = @name"; // Sử dụng tham số

            // Thực hiện truy vấn
            DataTable data = DataProvider.Instance.ExecuteQuery(query, new object[] { name });

            // Kiểm tra nếu có dữ liệu trả về
            if (data.Rows.Count > 0)
            {
                id = Convert.ToInt32(data.Rows[0]["ID"]); // Lấy ID từ hàng đầu tiên
            }

            return id; // Trả về ID hoặc -1 nếu không tìm thấy
        }


    }
}
