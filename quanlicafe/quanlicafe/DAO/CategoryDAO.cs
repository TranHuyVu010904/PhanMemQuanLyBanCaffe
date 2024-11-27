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
    public class CategoryDAO
    {
        private static CategoryDAO instance;

        public static CategoryDAO Instance
        {
            get { if (instance == null) instance = new CategoryDAO(); return CategoryDAO.instance; }
            private set { CategoryDAO.instance = value; }
        }

        private CategoryDAO() { }

        public List<Category> GetListCategory()
        {
            List<Category> list = new List<Category>();

            string query = "SELECT * FROM FoodCategory";
            DataTable data = DataProvider.Instance.ExecuteQuery(query);

            // Kiểm tra xem có danh mục None đã tồn tại hay chưa
            bool hasNoneCategory = false;

            foreach (DataRow item in data.Rows)
            {
                Category category = new Category(item);
                list.Add(category);

                // Kiểm tra nếu tên danh mục là None
                if (category.Name == "<<None>>")
                {
                    hasNoneCategory = true;
                }
            }

            // Nếu danh mục None chưa có, thêm vào danh sách
            if (!hasNoneCategory)
            {
                list.Insert(0, new Category(-1, "<<None>>")); // Thêm danh mục None với ID giả
            }

            return list;
        }



        public int GetCategoryIdByName(string name)
        {
            int id = -1; // Giá trị mặc định nếu không tìm thấy
            string query = "SELECT id FROM FoodCategory WHERE name = @name"; // Sử dụng tham số

            // Thực hiện truy vấn
            DataTable data = DataProvider.Instance.ExecuteQuery(query, new object[] { name });

            // Kiểm tra nếu có dữ liệu trả về
            if (data.Rows.Count > 0)
            {
                id = Convert.ToInt32(data.Rows[0]["id"]); // Lấy id từ hàng đầu tiên
            }

            return id; // Trả về id hoặc -1 nếu không tìm thấy
        }
        public DataTable GetDataTableCategory()
        {
            // Thực hiện truy vấn để lấy dữ liệu từ bảng FoodCategory
            string query = "SELECT * FROM FoodCategory";

            // Thực hiện truy vấn và nhận về DataTable
            DataTable data = DataProvider.Instance.ExecuteQuery(query);

            return data; // Trả về DataTable chứa tất cả danh mục
        }

        public Category GetCategoryByName(string name)
        {
            Category category = null;
            string query = "SELECT * FROM FoodCategory WHERE name = @name"; // Sử dụng tham số

            // Sử dụng ExecuteQuery với tham số
            DataTable data = DataProvider.Instance.ExecuteQuery(query, new object[] { name });

            // Kiểm tra nếu có dữ liệu trả về
            if (data.Rows.Count > 0)
            {
                // Tạo danh mục từ hàng đầu tiên
                category = new Category(data.Rows[0]);
            }

            return category; // Trả về category hoặc null nếu không tìm thấy
        }
        public bool InsertCategory(string name)
        {
            try
            {
                string query = string.Format("INSERT INTO FoodCategory  (Name) VALUES (N'{0}')", name);


            int result = DataProvider.Instance.ExecuteNonQuery(query);
            return result > 0;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // Lỗi trùng khóa (unique constraint violation)
                {
                    MessageBox.Show("Tên danh mục đã tồn tại. Vui lòng chọn tên khác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return false;
            }
        }
        public bool UpdateCategory(int ID, string Name)
        {
            try
            {

                // Lấy tên danh mục trước khi xóa
                string categoryName = GetCategoryNameById(ID);

                if (categoryName == "<<None>>")
                {
                    MessageBox.Show("Không thể sửa danh mục này!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false; // Không cho phép xóa
                }
                // Sử dụng câu truy vấn với tham số thay vì dùng string.Format
                string query = "UPDATE dbo.FoodCategory SET name = @name WHERE id = @id";

                // Thực thi câu lệnh với tham số truyền vào
                int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { Name, ID });

                return result > 0;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // Lỗi trùng khóa (unique constraint violation)
                {
                    MessageBox.Show("Tên danh mục đã tồn tại. Vui lòng chọn tên khác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return false;
            }
        }

        public bool DeleteCategoryAddCategory(int ID)
        {
            // Lấy tên danh mục trước khi xóa
            string categoryName = GetCategoryNameById(ID);

            if (categoryName == "<<None>>")
            {
                MessageBox.Show("Không thể xóa danh mục có tên là '<<None>>'.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false; // Không cho phép xóa
            }

            // Xóa danh mục nếu tên không phải là 'None'
            string query = "DELETE FROM dbo.FoodCategory WHERE id = @ID";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { ID });

            return result > 0;
        }

        private string GetCategoryNameById(int id)
        {
            string query = "SELECT name FROM dbo.FoodCategory WHERE id = @ID";
            DataTable result = DataProvider.Instance.ExecuteQuery(query, new object[] { id });

            if (result.Rows.Count > 0)
            {
                return result.Rows[0]["name"].ToString();
            }

            return string.Empty; // Trả về chuỗi rỗng nếu không tìm thấy
        }

    }
}
