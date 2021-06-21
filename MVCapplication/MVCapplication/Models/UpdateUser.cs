using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MVCapplication.Data.Migrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal.ExternalLoginModel;

namespace MVCapplication.Models
{
    public class UpdateUser :IdentityUser
    {
        [BindProperty]
        public InputModel Input { get; set; }
  
        public class InputModel
        {
        
            [Required]
            [Display(Name = "Name")]
            public string FullName { get; set; }
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
    
        }
        
        public int Update(string previousname, string name, string email, string password, string phno)
        {
            Console.WriteLine("Model class");
            // //string strConString = "data source=DESKTOP-JUH932N/SQLEXPRESS;Initial Catalog=user;Integrated Security=True";
            // SqlConnection sqlConnection = new SqlConnection("data source=DESKTOP-JUH923N/SQLEXPRESS; database=user; integrated security=True");
            // //using SqlConnection con = sqlConnection;

            // //string query = "INSERT INTO UserRegister(Name,Email,Password) VALUES(Name, Email, Password)";
            // //using (SqlCommand cmd = new SqlCommand(query))
            // //{
            // //    cmd.Connection = con;

            // //    con.Open();
            // using SqlConnection con = sqlConnection;
            // string query = "Update AspNetUsers SET FullName=@name, Email=@email ," +
            //         " UserName=@email ,PasswordHash=@password ,Phonenumber=@phno" +
            //         " where UserName=@previousname";
            //// SqlCommand cmd = new SqlCommand(query);
            // using (SqlCommand cmd = new SqlCommand(query))
            // { 

            //     cmd.Connection = con;
            //     con.Open();
            //     cmd.Parameters.AddWithValue("@name", name);
            //     cmd.Parameters.AddWithValue("@email", email);
            //     cmd.Parameters.AddWithValue("@password", password);
            //     cmd.Parameters.AddWithValue("@phno", phno);
            //     cmd.Parameters.AddWithValue("@previousname", previousname);
            //     Console.WriteLine(cmd.ExecuteNonQuery());
            //     return cmd.ExecuteNonQuery();
            // }
            //string status = "";
            SqlConnection sqlConnection = new SqlConnection("data source=DESKTOP-JUH932N/SQLEXPRESS;database=user; integrated security=True");
            using SqlConnection con = sqlConnection;
            string query = "Update AspNetUsers SET FullName=@name, Email=@email ," +
                     " UserName=@email ,PasswordHash=@password ,Phonenumber=@phno" +
                     " where UserName=@previousname";
            //string query = "INSERT INTO UserRegister(Name,Email,Password) VALUES(Name, Email, Password)";
            using (SqlCommand cmd = new SqlCommand(query))
            {
                cmd.Connection = con;

                con.Open();

                //cmd.Parameters.AddWithValue("@Name", user.Name);
                //cmd.Parameters.AddWithValue("@Email", user.Email);
                //cmd.Parameters.AddWithValue("@Password", user.Password);
                Console.WriteLine(cmd.ExecuteNonQuery());

                return cmd.ExecuteNonQuery();
            }
            
        }
    }
}
