using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCapplication.Models
{
    public class UpdateUser
    {
        public int Update(string previousname,string name, string email, string password,string phno)
        {
            Console.WriteLine("Model class");
            string strConString = @"DESKTOP-JUH932N\SQLEXPRESS;Initial Catalog=user;Integrated Security=True";

            using (SqlConnection con = new SqlConnection(strConString))
            {
                con.Open();
                string query = "Update AspNetUsers SET FullName=@name, Email=@email ," +
                    " UserName=@email ,PasswordHash=@password ,Phonenumber=@phno" +
                    " where UserName=@previousname";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@phno", phno);
                cmd.Parameters.AddWithValue("@previousname", previousname);
                Console.WriteLine(cmd.ExecuteNonQuery());
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
