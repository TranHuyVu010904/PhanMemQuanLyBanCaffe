using System;
using System.Data;

namespace QuanLyQuanCafe.DTO
{
    internal class AccountType
    {
        // Constructor với tham số id và name
        public AccountType(int id, string name)
        {
            this.ID = id;   // Sử dụng ID thay vì iD
            this.Name = name;
        }

        // Constructor nhận một DataRow
        public AccountType(DataRow row)
        {
            this.ID = row["id"] != DBNull.Value ? (int)row["id"] : 0; // Kiểm tra DBNull
            this.Name = row["name"].ToString();
        }

        private int iD;    // Biến riêng
        private string name;

        public int ID { get => iD; set => iD = value; }
        public string Name { get => name; set => name = value; }
    }
}
