using QuanLyQuanCafe.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLyQuanCafe.DAO
{
    public class FoodDAO
    {
        private static FoodDAO instance;

        public static FoodDAO Instance
        {
            get { if (instance == null) instance = new FoodDAO(); return FoodDAO.instance; }
            private set { FoodDAO.instance = value; }
        }

        private FoodDAO() { }

        public List<Food> GetFoodByCategoryID(int id)
        {
            List<Food> list = new List<Food>();

            string query = "select * from Food where IsDelete = '0' and idCategory = " + id;

            DataTable data = DataProvider.Instance.ExecuteQuery(query);

            foreach (DataRow item in data.Rows)
            {
                Food food = new Food(item);
                list.Add(food);
            }

            return list;
        }
        public bool InsertFood(string name, int id, float price)
        {
            try
            {
                string query = string.Format("INSERT INTO Food (name, idCategory, price) VALUES (N'{0}', {1}, {2})", name, id, price);


                int result = DataProvider.Instance.ExecuteNonQuery(query);
                return result > 0;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // Lỗi trùng khóa (unique constraint violation)
                {
                    MessageBox.Show("Tên món đã tồn tại. Vui lòng chọn tên khác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return false;
            }
        }
        public DataTable GetListFood()
        {
            return DataProvider.Instance.ExecuteQuery("SELECT f.id, f.name, c.name AS TenDanhMuc, f.price FROM dbo.Food f JOIN dbo.FoodCategory c ON f.idCategory = c.id where IsDelete = '0' ORDER BY c.name");
        }
        public DataTable SearFoodByName(string name)
        {
            string query = string.Format(
                "SELECT f.id, f.name, c.name AS TenDanhMuc, f.price " +
                "FROM dbo.Food f " +
                "JOIN dbo.FoodCategory c ON f.idCategory = c.id " +
                "WHERE f.name LIKE N'%{0}%' OR SOUNDEX(f.name) = SOUNDEX(N'{0}') " + 
                "ORDER BY c.name",
                name
            );

            return DataProvider.Instance.ExecuteQuery(query);
        }
        public bool UpdateFood(int idFood, string name, int id, float price)
        {
            try
            {
                string query = string.Format("UPDATE dbo.Food SET name = N'{0}', idCategory = {1}, price = {2} WHERE id = {3}", name, id, price, idFood);
                int result = DataProvider.Instance.ExecuteNonQuery(query);

                return result > 0;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // Lỗi trùng khóa (unique constraint violation)
                {
                    MessageBox.Show("Tên món đã tồn tại. Vui lòng chọn tên khác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return false;
            }
        }
        public bool DeleteFood(int idFood)
        {
            // Lấy tên món ăn để sử dụng trong thông tin cập nhật
            string foodName = GetFoodNameById(idFood);

            // Lấy số lần đã xóa món ăn
            int deleteCount = GetDeleteCountByFoodId(idFood);

            // Cập nhật thông tin món ăn
            string updatedFoodInfo = $"Deleted - {foodName} - {deleteCount + 1}";
            string queryUpdateFood = "UPDATE dbo.Food SET Name = @UpdatedInfo, IsDelete = 1 WHERE id = @IdFood";

            // Tạo mảng tham số để truyền vào ExecuteNonQuery
            object[] parameters = new object[]
            {
                updatedFoodInfo,
                idFood
            };

            // Thực hiện cập nhật thông tin
            int result = DataProvider.Instance.ExecuteNonQuery(queryUpdateFood, parameters);

            return result > 0;
        }

        private string GetFoodNameById(int idFood)
        {
            // Thực hiện truy vấn để lấy tên món ăn dựa trên ID
            string query = "SELECT Name FROM dbo.Food WHERE id = @IdFood";
            object[] parameters = new object[] { idFood };
            return DataProvider.Instance.ExecuteScalar(query, parameters)?.ToString();
        }

        private int GetDeleteCountByFoodId(int idFood)
        {
            // Thực hiện truy vấn để lấy số lần đã xóa món ăn
            string query = "SELECT COUNT(*) FROM dbo.Food WHERE IsDelete = 1 AND id = @IdFood";
            object[] parameters = new object[] { idFood };
            return (int)DataProvider.Instance.ExecuteScalar(query, parameters);
        }


    }
}
