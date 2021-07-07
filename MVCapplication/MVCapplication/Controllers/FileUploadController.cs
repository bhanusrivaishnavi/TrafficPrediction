using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MVCapplication.Data;
using MVCapplication.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

        public FileUploadController(IWebHostEnvironment hostingEnvironment, ApplicationDbContext dbcontext, SignInManager<IdentityUser> signinManager, IConfiguration config)
        {
            _hostingEnvironment = hostingEnvironment;
            _dbcontext = dbcontext;
            _signinManager = signinManager;
            _config = config;
        }

        //Uploading files using ado.net
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
                        List<string> extensions = new List<string>() { ".csv", ".xls", ".xlsx" };
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
                        var connstr = _config.GetConnectionString("UserIdentityDBConnection");
                        var fullname = "";
                        using (var conn = new SqlConnection(connstr))
                        using (var cmd = new SqlCommand(query1, conn))
                        {
                            conn.Open();
                            cmd.Parameters.Add("@Username", SqlDbType.NVarChar).SqlValue = Username;
                            string name = cmd.ExecuteScalar().ToString();
                            fullname = name;
                            conn.Close();
                        }

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


        [HttpGet]
        public ActionResult<List<string>> GetFileMetaData(string fname)
        {
            List<string> meta_data = new List<string>();
            if (_signinManager.IsSignedIn(User))
            {
                Username = User.Identity.Name;
            }
            var connstr = _config.GetConnectionString("UserIdentityDBConnection");
            var query = "select * from FileUploads where FileName=@fname;";
            using (var conn = new SqlConnection(connstr))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@fname", SqlDbType.NVarChar).SqlValue = fname;
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    meta_data.Add(reader["FullName"].ToString());

                    meta_data.Add(reader["InsertedOn"].ToString());

                    meta_data.Add(reader["FileName"].ToString());



                }
                ViewBag.filenames = GetData();
                ViewBag.filedetails = meta_data;
                conn.Close();

                ViewBag.usingcsvhelper = GetProcessedData(fname);
                return View("GetFileMetaData");
            }
        }

        //Get the list of names of processed files from database
        public List<string> GetData()
        {
            if (_signinManager.IsSignedIn(User))
            {
                Username = User.Identity.Name;
            }
            var flag = "true";
            var connstr = _config.GetConnectionString("UserIdentityDBConnection");
            var query = "select * from FileUploads where UserName=@Username and IsProcessed=@flag;";
            var Processed_files = new List<string>();
            using (var conn = new SqlConnection(connstr))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@Username", SqlDbType.NVarChar).SqlValue = Username;
                cmd.Parameters.Add("@flag", SqlDbType.NVarChar).SqlValue = flag;
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Processed_files.Add(Convert.ToString(reader["FileName"]));
                }
                reader.Close();
                conn.Close();
            }
            return Processed_files;
        }


        //get the records from the csv file
        public List<dynamic> GetProcessedData(string fname)
        {

            var path = Path.Combine("", _hostingEnvironment.ContentRootPath + "\\Final\\" + fname);



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




        //Function to download
        [HttpGet]
        public ActionResult<FileUpload> Download(string fname)
        {
            var path = Path.Combine("", _hostingEnvironment.ContentRootPath + "\\Final\\" + fname);
            FileContentResult downloaded = new FileContentResult(System.IO.File.ReadAllBytes(path), "text/csv")
            {
                FileDownloadName = fname.Substring(37)
            };
            return downloaded;
        }

    }
}