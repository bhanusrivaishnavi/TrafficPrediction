using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal.ExternalLoginModel;

namespace MVCapplication.Models
{
    public class UpdateUser
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
