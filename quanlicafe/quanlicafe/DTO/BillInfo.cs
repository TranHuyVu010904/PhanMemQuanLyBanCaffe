using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyQuanCafe.DTO
{
    public class BillInfo
    {
        public BillInfo(int id, int billID, int foodID,int count) 
        {
            this.ID = id;
            this.billID=billID;
            this.foodID=foodID;
            this.Count = count;
        }
        public BillInfo(DataRow row)
        {
            this.ID = (int)row["id"];
            this.billID = (int)row["billID"]; ;
            this.foodID = (int)row["foodID"];
            this.Count = (int)row["count"]; 
        }

        private int Count;
        private int foodID;
        private int billID;
        private int iD;

        public int ID { get => iD; set => iD = value; }
        public int BillID { get => billID; set => billID = value; }
        public int FoodID { get => foodID; set => foodID = value; }
        public int Count1 { get => Count; set => Count = value; }
    }
}
