using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using MVCapplication.Data;
using MVCapplication.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Threading.Tasks;

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
       
        public ActionResult<List<String>> UpdateUser()
        {
            var result = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

            List<string> result1 = new List<string>();
            if (_signinManager.IsSignedIn(User))
            {

                Username = User.Identity.Name;

            }
            //Console.WriteLine("Tha " + Username);
            var connstr = _config.GetConnectionString("UserIdentityDBConnection"); //"Server=DESKTOP-JUH932N\\SQLEXPRESS; Database=user; Trusted_Connection=true;";
            var query = "select FullName,PhoneNumber from AspNetUsers where UserName=@Username;";
            using (var conn = new SqlConnection(connstr))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add(
                   "@Username",
                   SqlDbType.NVarChar).SqlValue = Username;

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
            ViewBag.answer = result1;
            //Console.WriteLine(result1[1]+ ViewBag.answer[0]);
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
        [HttpGet]
        public ActionResult<List<string>> HomeView()
        {
            var result = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            //Console.WriteLine(rows);
            List<string> result1 = new List<string>();
            if (_signinManager.IsSignedIn(User))
            {

                Username = User.Identity.Name;

            }
            //Console.WriteLine("Tha " + Username);

            var connstr = _config.GetConnectionString("UserIdentityDBConnection"); //"Server=DESKTOP-JUH932N\\SQLEXPRESS; Database=user; Trusted_Connection=true;";//"Server=PRAKASH; Database=user; Trusted_Connection=true;";
            var query = "select * from FileUploads where UserName=@Username;";
            using (var conn = new SqlConnection(connstr))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add( "@Username",SqlDbType.NVarChar).SqlValue = Username;

                conn.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                                     
                    result1.Add(Convert.ToString(reader["FileName"]));
                }

                conn.Close();


            }
   
            ViewBag.answer = result1;
           // Console.WriteLine(result1.ToString());
            return View();
        }
       
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        public IActionResult UpdateRecord(IFormCollection frm, string action)
        {
            // Console.WriteLine("Controller class"+action);
           
            if (action == "Update")
            {
               
                UpdateUser model = new UpdateUser();
                string previousname = frm["txtuser"];
                string name = frm["txtFName"] + " "+ frm["txtLName"];
                string email = Convert.ToString(frm["txtEmail"]);
                string password = frm["txtpswd1"];
                string phno = frm["txtPhno"];
               // Console.WriteLine(frm["txtuser"]+"PreviousName:"+previousname+"\n"+name + "\n" + email + "\n" + password + "\n" + phno);
                int t= Update(previousname, name, email, password, phno);
                if (t == 1) ViewBag.status = "Record Updated Successfully!!";
                else ViewBag.error = "Record Not Updated. Try Again!";
                return View("UpdateUser");
            }
            if(action=="Change")
            {
               // Console.WriteLine(frm["newpswd"]);
                //Console.WriteLine(frm["newpswd1"]);
                if (frm["newpswd"]!=frm["newpswd1"])
                {
                    ViewBag.error = "Passwords does not match";
                   // Console.WriteLine(ViewBag.error);
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

        public int Update(string previousname, string name, string email, string password, string phno)
        {
          
            
            string query = "Update AspNetUsers SET NormalizedEmail=@nemail, NormalizedUserName=@nemail, FullName=@name, Email=@email ," +
                     " UserName=@email,Phonenumber=@phno where UserName=@previousname;";
            
            if (email == "") email = previousname;
            var connstr = _config.GetConnectionString("UserIdentityDBConnection");

            using (var conn = new SqlConnection(connstr))
            {
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

        public static bool EmailSend()
        {
            bool status = false;
            try
            {
                Console.WriteLine("Sending mail");
                string HostAddress = "smtp.gmail.com";
                string FormEmailId = "bbsvaishnavi123@gmail.com";
                string Password = "AURORA123*";
                string Port = "587";
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(FormEmailId);
                mailMessage.Subject = "Sample Email";
                mailMessage.Body = "To check email sending.";
                mailMessage.IsBodyHtml = false;
                mailMessage.To.Add(new MailAddress("bbsvsweety77@gmail.com"));
                SmtpClient smtp = new SmtpClient();
                smtp.Host = HostAddress;
                smtp.EnableSsl = true;
                NetworkCredential networkCredential = new NetworkCredential();
                networkCredential.UserName = mailMessage.From.Address;
                networkCredential.Password = Password;
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = networkCredential;
                smtp.Port = Convert.ToInt32(Port);
                smtp.Send(mailMessage);
                status = true;
                return status;
            }
            catch (Exception e)
            {
                Console.WriteLine(e+"Mail not sent");
                return status;
            }
        }
    }
}










//public ActionResult<List<FileUpload>> HomeView1()
//{
//    if (_signinManager.IsSignedIn(User))
//    {

//        Username = User.Identity.Name;

//    }
//    var model = _dbcontext.FileUploads.Where(x => x.UserName == Username).ToList();
//    //  var Movies = (from movie in _dbcontext.FileUploads select movie);
//    ViewBag.answer = model;
//    Console.WriteLine(ViewBag.answer);
//    return View();
//}

//Task<IdentityUser> user = _userManager.GetUserAsync(User);