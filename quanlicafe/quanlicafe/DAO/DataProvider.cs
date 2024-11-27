using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyQuanCafe.DAO
{
    public class DataProvider
    {
        private static DataProvider instance; // Ctrl + R + E

        public static DataProvider Instance
        {
            get { if (instance == null) instance = new DataProvider(); return DataProvider.instance; }
            private set { DataProvider.instance = value; }
        }

        private DataProvider() { }

        private string connectionSTR = "Data Source=LAPTOP-6K3UA9B7\\HUYVU;Initial Catalog=QuanLyQuanCafe;Integrated Security=True";

        public DataTable ExecuteQuery(string query, object[] parameter = null)
        {
            DataTable data = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionSTR))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);

                if (parameter != null)
                {
                    // Tách các tham số bằng cách dùng ký tự '@'
                    string[] listPara = query.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    int i = 0;
                    foreach (string item in listPara)
                    {
                        if (item.StartsWith("@"))
                        {
                            // Kiểm tra để thêm tham số
                            command.Parameters.AddWithValue(item, parameter[i]);
                            i++;
                        }
                    }
                }

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(data);
            }

            return data;
        }


        public int ExecuteNonQuery(string query, object[] parameters = null)
        {
            int data = 0;

            using (SqlConnection connection = new SqlConnection(connectionSTR))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        // Tìm các tham số trong câu lệnh SQL
                        var parameterNames = new List<string>();
                        foreach (var word in query.Split(new char[] { ' ', ',', '(', ')' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (word.StartsWith("@"))
                            {
                                parameterNames.Add(word);
                            }
                        }

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            // Thêm tham số vào command
                            command.Parameters.AddWithValue(parameterNames[i], parameters[i]);
                        }
                    }

                    data = command.ExecuteNonQuery();
                }
            }

            return data;
        }


        public object ExecuteScalar(string query, object[] parameter = null)
        {
            object data = 0;

            using (SqlConnection connection = new SqlConnection(connectionSTR))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(query, connection);

                if (parameter != null)
                {
                    string[] listPara = query.Split(' ');
                    int i = 0;
                    foreach (string item in listPara)
                    {
                        if (item.Contains('@'))
                        {
                            command.Parameters.AddWithValue(item, parameter[i]);
                            i++;
                        }
                    }
                }

                data = command.ExecuteScalar();

                connection.Close();
            }

            return data;
        }
    }
}
