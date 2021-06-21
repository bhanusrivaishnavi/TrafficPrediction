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
    public class UpdateUser : IdentityUser
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public UpdateUser()
        { }

        public UpdateUser(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public string Username { get; set; }
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
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

        }
        [HttpPost]
        public string Update(string previousname, string name, string email, string password, string phno)
        {
            Console.WriteLine("Model class");
            string query = "Update AspNetUsers SET FullName=@name, Email=@email ," +
                     " UserName=@email,Phonenumber=@phno" +
                     " where UserName=@previousname";
            using (SqlConnection con = new SqlConnection("data source=DESKTOP-JUH932N\\SQLEXPRESS;Database=user;integrated security=SSPI"))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                  
                    con.Open();

                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@email", email);
                    //cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@phno", phno);
                    cmd.Parameters.AddWithValue("@previousname", previousname);
                    string status = (cmd.ExecuteNonQuery()).ToString();
                    //string status = (cmd.ExecuteNonQuery() >= 1) ? "Record is saved Successfully!" : "Record is not saved";
                    //     Console.WriteLine(cmd.ExecuteNonQuery());
                    return status;
                }
            }
        }
        [HttpGet]
        public void Update()
        {

        }
    }
}
//cmd.Parameters.AddWithValue("@name", name);
//cmd.Parameters.AddWithValue("@email", email);
//cmd.Parameters.AddWithValue("@password", password);
//cmd.Parameters.AddWithValue("@phno", phno);
//cmd.Parameters.AddWithValue("@previousname", previousname);
