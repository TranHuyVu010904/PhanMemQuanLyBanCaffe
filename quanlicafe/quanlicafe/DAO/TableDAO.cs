using QuanLyQuanCafe.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace QuanLyQuanCafe.DAO
{
    public class TableDAO
    {
        private static TableDAO instance;

        public static TableDAO Instance
        {
            get { if (instance == null) instance = new TableDAO(); return TableDAO.instance; }
            private set { TableDAO.instance = value; }
        }

        public static int TableWidth = 90;
        public static int TableHeight = 90;

        private TableDAO() { }

        public void SwitchTable(int id1, int id2)
        {
            DataProvider.Instance.ExecuteQuery("USP_SwitchTable @idTable1 , @idTable2", new object[] { id1, id2 });
        }
        public List<Table> GetActiveTables()
        {
            List<Table> list = new List<Table>();
            string query = "SELECT ID, Name, status \r\nFROM dbo.TableFood \r\nWHERE isDeleted = 0 and id != 105 \r\nORDER BY CAST(SUBSTRING(Name, 5, LEN(Name) - 4) AS INT);";
            DataTable data = DataProvider.Instance.ExecuteQuery(query);
            foreach (DataRow item in data.Rows)
            {
                Table table = new Table(item);
                list.Add(table);
            }
            return list;
        }

        public bool DeleteTable(int idTable)
        {
            // Truy vấn để lấy tên gốc của bàn theo id
            string getTableNameQuery = "SELECT Name FROM dbo.TableFood WHERE id = @idTable";
            string originalName = (string)DataProvider.Instance.ExecuteScalar(getTableNameQuery, new object[] { idTable });

            // Nếu không tìm thấy bàn, trả về false
            if (string.IsNullOrEmpty(originalName))
            {
                return false;
            }

            // Chuẩn bị mẫu tên để kiểm tra các bàn đã bị xóa trước đó với tên gốc
            string deleteNamePattern = $"Delete - {originalName} -%";

            // Truy vấn để lấy số lần bàn này đã bị xóa trước đó
            string getDeleteCountQuery = "SELECT COUNT(*) FROM dbo.TableFood WHERE Name LIKE @deleteNamePattern";
            int deleteCount = (int)DataProvider.Instance.ExecuteScalar(getDeleteCountQuery, new object[] { deleteNamePattern });

            // Tạo tên mới cho bàn khi xóa, sử dụng số đếm để tránh trùng lặp
            string newName = $"Delete - {originalName} - {deleteCount + 1}";

            // Truy vấn cập nhật tên và trạng thái của bàn
            string query = "UPDATE dbo.TableFood SET Name = @newName, isDeleted = 1 WHERE id = @idTable AND isDeleted = 0;";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { newName, idTable });

            return result > 0;
        }



        public bool AddTable(string name)
        {
            try
            {
                string query = string.Format("INSERT INTO TableFood (Name,status,isDeleted) VALUES (N'{0}',N'Trống',0)", name);
                int result = DataProvider.Instance.ExecuteNonQuery(query);
                return result > 0;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) 
                {
                    MessageBox.Show("Tên bàn đã tồn tại. Vui lòng chọn tên khác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return false;
            }

        }
        public bool UpdateTable(int idTable, string name)
        {
            try
            {
                string query = "UPDATE TableFood SET Name = @name WHERE id = @id";

                // Thực thi câu lệnh với tham số truyền vào
                int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { name, idTable });

                return result > 0;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    MessageBox.Show("Tên bàn đã tồn tại. Vui lòng chọn tên khác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return false;
            }

        }
        public Table GetTableByName(string tableName)
        {
            string query = "SELECT * FROM TableFood WHERE Name = @Name AND isDeleted = 0";
            Table table = null;

            // Truy vấn dữ liệu từ cơ sở dữ liệu
            DataTable data = DataProvider.Instance.ExecuteQuery(query, new object[] { tableName });

            // Kiểm tra nếu có kết quả trả về
            if (data.Rows.Count > 0)
            {
                // Chỉ lấy bản ghi đầu tiên (vì table name là duy nhất)
                table = new Table(data.Rows[0]);
            }

            return table; // Trả về đối tượng Table hoặc null nếu không tìm thấy
        }

        public List<Table> LoadTableList()
        {
            List<Table> tableList = new List<Table>();

            DataTable data = DataProvider.Instance.ExecuteQuery("USP_GetTableList");

            foreach (DataRow item in data.Rows)
            {
                Table table = new Table(item);
                tableList.Add(table);
            }

            return tableList;
        }
    }
}
