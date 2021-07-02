using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MVCapplication.Data;
using MVCapplication.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using System.Security.Claims;
using System.Net.Http;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using MVCapplication.Data.Migrations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using CsvHelper;

namespace MVCapplication.Controllers
{
    [Authorize]

    [Route("api/[controller]/[action]")]
    [ApiController]

    public class FileUploadController : Controller
    {
        ApplicationUser au = new ApplicationUser();
        List<string> result1 = new List<string>();
        public IWebHostEnvironment _hostingEnvironment;
        public ApplicationDbContext _dbcontext;
        private readonly SignInManager<IdentityUser> _signinManager;
        private readonly IConfiguration _config;
        public string Username;

        [TempData]
        public string StatusMessage { get; set; }

        public FileUploadController(IWebHostEnvironment hostingEnvironment, ApplicationDbContext dbcontext, /*UserManager<IdentityUser> userManager,*/ SignInManager<IdentityUser> signinManager, IConfiguration config)
        {
            _hostingEnvironment = hostingEnvironment;
            _dbcontext = dbcontext;
            _signinManager = signinManager;
            _config = config;
        }

        public ActionResult<string> UploadFileAdo()
        {
            try
            {
                var files = HttpContext.Request.Form.Files;
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        FileInfo fi = new FileInfo(file.FileName);
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        var ext = Path.GetExtension(uniqueFileName);
                        List<string> extensions = new List<string>() { ".csv",".xls",".xlsx"  };
                        if (extensions.Contains(ext) == false)
                        {
                            ViewBag.error = "Only Dataset Files are Accepted";
                            return View("UploadFileAdo");
                        }
                        var path = Path.Combine("", _hostingEnvironment.ContentRootPath + "\\Temp\\" + uniqueFileName);
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        var time = DateTime.Today;
                        if (_signinManager.IsSignedIn(User))
                        {
                            Username = User.Identity.Name;
                        }
                        var query1 = "Select FullName from dbo.AspNetUsers where UserName=@Username;";
                        var connstr = _config.GetConnectionString("UserIdentityDBConnection");//"Server=DESKTOP-JUH932N\\SQLEXPRESS; Database=user; Trusted_Connection=true;";// "Server=PRAKASH; Database=user; Trusted_Connection=true;";
                        var fullname = "hi";
                        using (var conn = new SqlConnection(connstr))
                        using (var cmd = new SqlCommand(query1, conn))
                        {
                            conn.Open();
                            cmd.Parameters.Add("@Username", SqlDbType.NVarChar).SqlValue = Username;
                            string name = cmd.ExecuteScalar().ToString();
                            fullname = name;
                            conn.Close();
                        }
                        //@ iss use to prevent SQL Injection
                        var query = "INSERT INTO dbo.FileUploads(" + " UserName, FileName,FullName, FilePath, InsertedOn,IsProcessed " + ") VALUES(" + " @user, @file, @fullname,@filpath,@insert,@process);";
                        using (var conn = new SqlConnection(connstr))
                        using (var cmd = new SqlCommand(query, conn))
                        {
                            conn.Open();
                            cmd.Parameters.AddWithValue("@user", Username);
                            cmd.Parameters.AddWithValue("@file", uniqueFileName);
                            cmd.Parameters.AddWithValue("@filpath", path);
                            cmd.Parameters.AddWithValue("@insert", time);
                            cmd.Parameters.AddWithValue("@process", "false");
                            cmd.Parameters.AddWithValue("@fullname", fullname);
                            cmd.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                    ViewBag.status = "File Uploaded Successfully.";
                }
                else
                {
                    ViewBag.error = "Select Files.";
                }
            }
            catch (Exception e)
            {
                ViewBag.error = e.Message;
            }
            return View("UploadFileAdo");
        }

        //posting meta data 
        [HttpGet]
        public ActionResult<List<string>> GetFileMetaData(string fname)
        {
            List<string> result2 = new List<string>();
            if (_signinManager.IsSignedIn(User))
            {
                Username = User.Identity.Name;
            }
            var connstr = _config.GetConnectionString("UserIdentityDBConnection"); //"Server=DESKTOP-JUH932N\\SQLEXPRESS; Database=user; Trusted_Connection=true;";
            var query = "select * from FileUploads where FileName=@fname;";
            using (var conn = new SqlConnection(connstr))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@fname", SqlDbType.NVarChar).SqlValue = fname;
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result2.Add(reader["FilePath"].ToString());
                    result2.Add(reader["FileName"].ToString());
                    result2.Add(reader["UserName"].ToString());
                    result2.Add(reader["InsertedOn"].ToString());
                    result2.Add(reader["FullName"].ToString());
                }
                ViewBag.filenames = GetData();
                ViewBag.filedetails = result2;
                conn.Close();
               
                ViewBag.usingcsvhelper = GetProcessedData(fname);
                return View("GetFileMetaData");
            }
        }

        public List<dynamic> GetProcessedData(string fname)
        {
            var connstr = _config.GetConnectionString("UserIdentityDBConnection"); //"Server=DESKTOP-JUH932N\\SQLEXPRESS; Database=user; Trusted_Connection=true;";//"Server=PRAKASH; Database=user; Trusted_Connection=true;";
            var query = "select IsProcessed from FileUploads where UserName=@Username and FileName=@fname;";
            var processed = "False";
            using (var conn = new SqlConnection(connstr))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add( "@Username", SqlDbType.NVarChar).SqlValue = Username;
                cmd.Parameters.Add("@fname", SqlDbType.NVarChar).SqlValue = fname;
                conn.Open();
                processed = cmd.ExecuteScalar().ToString();
            }
            var path = "";
            if (processed == "True")
            {
                path = Path.Combine("", _hostingEnvironment.ContentRootPath + "\\Final\\" + fname);
                string pathOnly = Path.GetDirectoryName(path);
                string fileName = Path.GetFileName(path);
            }
            else
            {
                path = Path.Combine("", _hostingEnvironment.ContentRootPath + "\\Temp\\" + fname);
            }
            using (var reader = new StreamReader(path))
            {
                var line = reader.ReadLine();

                var values = line.Split(',');
                ViewBag.ColumnHeaders = values;
                ViewBag.ColumnCount = values.Length;
            }
            using (var reader = new StreamReader(path))
            {
                 using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csvReader.GetRecords<dynamic>().ToList();
                    return records;
                }
            }
        }

        public List<string> GetData()
        {
            if (_signinManager.IsSignedIn(User))
            {
                Username = User.Identity.Name;
            }
            var flag = "True";
            var connstr = _config.GetConnectionString("UserIdentityDBConnection"); //"Server=DESKTOP-JUH932N\\SQLEXPRESS; Database=user; Trusted_Connection=true;";//"Server=PRAKASH; Database=user; Trusted_Connection=true;";
            var query = "select * from FileUploads where UserName=@Username and IsProcessed=@flag;";
            using (var conn = new SqlConnection(connstr))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add( "@Username", SqlDbType.NVarChar).SqlValue = Username;
                cmd.Parameters.Add("@flag", SqlDbType.NVarChar).SqlValue = flag;
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result1.Add(Convert.ToString(reader["FileName"]));
                }
                reader.Close();
                conn.Close();
            }
            return result1;
        }

        [HttpGet]
        public ActionResult<List<string>> HomeView()
        {
            var result = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            result1 = GetData();
            return result1;
        }

        [HttpGet]
        public ActionResult<FileUpload> Download3(int? id, string fname)
        {
            var path = Path.Combine("", _hostingEnvironment.ContentRootPath + "\\Final\\" + fname);
            FileContentResult result1 = new FileContentResult(System.IO.File.ReadAllBytes(path), "text/csv")
            {
                FileDownloadName = fname.Substring(37)
            };
            return result1;
        }

    }
}