using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MVCapplication.Data.Migrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;


namespace MVCapplication.Models
{
    public class UpdateUser : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _config;
        public UpdateUser()
        {
            
        }
        public UpdateUser(IConfiguration config,UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }
        public string Username { get { return "Vaishnavi"; }  set { } }
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
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
            public string StatusMessage { get; set; }

        }
        public static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }

        public string Update(string previousname, string name, string email, string password, string phno)
        {
            Console.WriteLine("Model class UserName:"+previousname);
            //List<String> values = Data(previousname);
            string query = "Update AspNetUsers SET NormalizedEmail=@nemail, NormalizedUserName=@nemail, FullName=@name, Email=@email ," +
                     " UserName=@email,Phonenumber=@phno";
            if (password == "") query=query+"";
            else   query = query + ",PasswordHash=@password";
            query=query+" where UserName=@previousname;";
            Console.WriteLine(query);
            if (email == "") email = previousname;
            var connstr = _config.GetConnectionString("UserIdentityDBConnection");

            using (var conn = new SqlConnection(connstr))
            {
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();

                    var nemail = email.ToUpper();
                    //Task<IdentityUser> user = _userManager.GetUserAsync(User);
                    if (password != null && password != "")
                    {
                        var pswd = HashPassword(password);
                        cmd.Parameters.AddWithValue("@password", pswd);
                    }
                    // change= _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
                    //Console.WriteLine(User);
                    // command.Parameters.Add("@Pinz", SqlDbType.Int).Value = Pinz;
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = name;
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@nemail", nemail);
                    cmd.Parameters.AddWithValue("@phno", phno);
                    cmd.Parameters.Add("@previousname", SqlDbType.NVarChar).Value = previousname;
                    // cmd.Parameters.AddWithValue("@previousname", previousname);
                    string status = (cmd.ExecuteNonQuery()).ToString();
                    //string status = (cmd.ExecuteNonQuery() >= 1) ? "Record is saved Successfully!" : "Record is not saved";
                    //     Console.WriteLine(cmd.ExecuteNonQuery());
                    conn.Close();

                    return status;

                }
            }
            
        }
       
      
       
    }
}
//cmd.Parameters.AddWithValue("@name", name);
//cmd.Parameters.AddWithValue("@email", email);
//cmd.Parameters.AddWithValue("@password", password);
//cmd.Parameters.AddWithValue("@phno", phno);
//cmd.Parameters.AddWithValue("@previousname", previousname);



//public async Task<IActionResult> OnUpdateAsync()
//{

//    var user = await _userManager.GetUserAsync(User);
//    var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
//    if (!changePasswordResult.Succeeded)
//    {
//        Console.WriteLine("Not changed password");
//    }
//    else
//    {
//        Console.WriteLine("Password Changed");
//    }
//    await _signInManager.RefreshSignInAsync(user);

//    return Page();
//}