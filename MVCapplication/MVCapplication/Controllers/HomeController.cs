using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MVCapplication.Data;
using MVCapplication.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;

namespace MVCapplication.Controllers
{
    public class HomeController : Controller
    {
       
        private readonly ILogger<HomeController> _logger;
        public ApplicationDbContext _dbcontext;
        private readonly SignInManager<IdentityUser> _signinManager;
        public string Username;
        private readonly IConfiguration _config;

        public HomeController(IConfiguration config,ILogger<HomeController> logger,ApplicationDbContext db, SignInManager<IdentityUser> signinManager)
        {
            _dbcontext = db;
            _logger = logger;
            _signinManager = signinManager;
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }
         
        //To show the data in User management
        public ActionResult<List<String>> UpdateUser()
        {
            var result = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            List<string> result1 = new List<string>();
            if (_signinManager.IsSignedIn(User))
            {
                Username = User.Identity.Name;
            }
            var connstr = _config.GetConnectionString("UserIdentityDBConnection"); //"Server=DESKTOP-JUH932N\\SQLEXPRESS; Database=user; Trusted_Connection=true;";
            var query = "select FullName,PhoneNumber from AspNetUsers where UserName=@Username;";
            using (var conn = new SqlConnection(connstr))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@Username",SqlDbType.NVarChar).SqlValue = Username;
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result1.Add(reader["FullName"].ToString());
                    result1.Add(reader["PhoneNumber"].ToString());                   
                }
                reader.Close();
                conn.Close();
            }
            ViewBag.nameandphone = result1;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult UploadView()
        {
            return View();
        }

        public IActionResult UserView()
        {
            return View();
        }

        public IActionResult UpdatePassword()
        {
            return View();
        }

        //To show the list of processed files in home page
        [HttpGet]
        public ActionResult<List<string>> HomeView()
        {
            var result = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            List<string> result1 = new List<string>();
            if (_signinManager.IsSignedIn(User))
            {
                Username = User.Identity.Name;
            }
            var flag = "true";
            var connstr = _config.GetConnectionString("UserIdentityDBConnection"); //"Server=DESKTOP-JUH932N\\SQLEXPRESS; Database=user; Trusted_Connection=true;";//"Server=PRAKASH; Database=user; Trusted_Connection=true;";
            var query = "select * from FileUploads where UserName=@Username and IsProcessed=@flag;";
            using (var conn = new SqlConnection(connstr))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add( "@Username",SqlDbType.NVarChar).SqlValue = Username;
                cmd.Parameters.Add("@flag",SqlDbType.NVarChar).SqlValue = flag;
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {                                     
                    result1.Add(Convert.ToString(reader["FileName"]));
                }
                conn.Close();
            }  
            ViewBag.files = result1;         
            return View();
        }
       
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //Input from the Update user form and Update password form
        [HttpPost]
        public IActionResult UpdateRecord(IFormCollection frm, string action)
        {
            if (action == "Update")
            {
               
                UpdateUser model = new UpdateUser();
                string previousname = frm["txtuser"];
                string name = frm["txtFName"];
                string email = Convert.ToString(frm["txtEmail"]);
                string password = frm["txtpswd1"];
                string phno = frm["txtPhno"];
                try
                {
                    int t = Update(previousname, name, email, password, phno);
                    if (t == 1) ViewBag.status = "Record Updated Successfully!!";
                    else ViewBag.error = "Record Not Updated. Try Again!";
                }
                catch(Exception )
                {
                    ViewBag.error="Error: Username already exists.";
                }
                return View("UpdateUser");
            }
            if(action=="Change")
            {
                if (frm["newpswd"]!=frm["newpswd1"])
                {
                    ViewBag.error = "Passwords does not match";
                }
                else
                {
                    string password = frm["newpswd"];
                    int t= UpdatePswd(password);
                    if (t == 1) ViewBag.status = "Password Updated Successfully";
                    else ViewBag.error = "Try Again Later";
                }
                return View("UpdatePassword");
            }
            
            return View();
        }

        //Function from the Update record to update the password 
        public int UpdatePswd(string newpswd)
        {
            if (_signinManager.IsSignedIn(User))  Username = User.Identity.Name;
            string query = "Update AspNetUsers SET PasswordHash=@newpswd where UserName=@Username;";
            var connstr = _config.GetConnectionString("UserIdentityDBConnection");
            using (var conn = new SqlConnection(connstr))
            {
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    newpswd = HashPassword(newpswd);
                    cmd.Parameters.Add("@newpswd", SqlDbType.NVarChar).Value = newpswd;
                    cmd.Parameters.Add("@Username", SqlDbType.NVarChar).Value = Username;
                    int status = (cmd.ExecuteNonQuery());
                    conn.Close();
                    return status;
                }
            }                   
        }

        //Password hash to update the password
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

        //Function from update record to update the details
        public int Update(string previousname, string name, string email, string password, string phno)
        {                   
            string query = "Update AspNetUsers SET NormalizedEmail=@nemail, NormalizedUserName=@nemail, FullName=@name, Email=@email ," +
                     " UserName=@email,Phonenumber=@phno where UserName=@previousname;";
            string query1 = "Update FileUploads SET UserName=@email, FullName=@name where UserName=@previousname;";
            if (email == "") email = previousname;
            var connstr = _config.GetConnectionString("UserIdentityDBConnection");
            using (var conn = new SqlConnection(connstr))
            {
                using (var cmd = new SqlCommand(query1, conn))
                {
                    conn.Open();
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = name;
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.Add("@previousname", SqlDbType.NVarChar).Value = previousname;
                    int status = (cmd.ExecuteNonQuery());
                    conn.Close();
                }
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    var nemail = email.ToUpper();                  
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = name;
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@nemail", nemail);
                    cmd.Parameters.AddWithValue("@phno", phno);
                    cmd.Parameters.Add("@previousname", SqlDbType.NVarChar).Value = previousname;
                    int status = (cmd.ExecuteNonQuery());
                    conn.Close();
                    return status;
                }
            }           
        }
    }
}








