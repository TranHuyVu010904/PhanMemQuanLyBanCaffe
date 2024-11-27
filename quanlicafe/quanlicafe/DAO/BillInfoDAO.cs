using QuanLyQuanCafe.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace QuanLyQuanCafe.DAO
{
    public class BillInfoDAO
    {
        private static BillInfoDAO instance;

        public static BillInfoDAO Instance
        {
            get { if (instance == null) instance = new BillInfoDAO(); return BillInfoDAO.instance; }
            private set { BillInfoDAO.instance = value; }
        }

        private BillInfoDAO() { }

        public List<BillInfo> GetListBillInfo(int id)
        {
            List<BillInfo> listBillInfo = new List<BillInfo>();

            DataTable data = DataProvider.Instance.ExecuteQuery("SELECT * FROM dbo.BillInfo WHERE idBill = " + id);

            foreach (DataRow item in data.Rows)
            {
                BillInfo info = new BillInfo(item);
                listBillInfo.Add(info);
            }

            return listBillInfo;
        }
        public int GetFoodCountInBill(int idBill, int foodID)
        {
            string query = "SELECT count FROM BillInfo WHERE idBill = @idBill AND idFood = @idFood";
            object result = DataProvider.Instance.ExecuteScalar(query, new object[] { idBill, foodID });

            if (result != null)
                return Convert.ToInt32(result);

            return 0; // Nếu không tìm thấy món ăn, trả về 0
        }
        public void InsertBillInfo(int idBill, int idFood, int count)
        {
            DataProvider.Instance.ExecuteNonQuery("USP_InsertBillInfo @idBill , @idFood , @count", new object[] { idBill, idFood, count });
        }
        
        public DataTable GetTop8MostOrderedDrinks(DateTime startDate, DateTime endDate)
        {
            string query = "EXEC USP_GetTop8MostOrderedDrinks @StartDate, @EndDate";
            object[] parameters = { startDate, endDate };
            return DataProvider.Instance.ExecuteQuery(query, parameters);
        }
        public DataTable GetTop8MostOrderedFoods(DateTime startDate, DateTime endDate)
        {
            string query = "EXEC USP_GetTop8MostOrderedFoods @StartDate, @EndDate";
            object[] parameters = { startDate, endDate };
            return DataProvider.Instance.ExecuteQuery(query, parameters);
        }
        public DataTable GetBillInfoByBillId(int billId)
        {
            string query = "EXEC USP_GetBillInfoByID @billId";
            object[] parameters = new object[] { billId };
            DataTable data = DataProvider.Instance.ExecuteQuery(query, parameters);
            return data;
        }

    }
}
