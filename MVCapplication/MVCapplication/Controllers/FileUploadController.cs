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

                        List<string> extensions = new List<string>() {
                            ".csv",".xls",".xlsx"
                        };


                        if (extensions.Contains(ext) == false)
                        {
                            StatusMessage = "Only Dataset Files are Accepted";
                            ViewBag.status = StatusMessage;
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
                    StatusMessage = "File Uploaded Successfully.";

                }
                else
                {
                    StatusMessage = "Select Files.";
                }

            }
            catch (Exception e)
            {
                StatusMessage = e.Message;
            }
            ViewBag.status = StatusMessage;

            return View("UploadFileAdo");

        }



        //posting meta data 
        [HttpGet]
        public ActionResult<List<string>> GetFileMetaData(string fname)
        {
           // Console.WriteLine("Action:" + action);
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
                //change the ViewBag Names

                ViewBag.answer = GetData();
                ViewBag.answer1 = result2;
                //List<String> first = GetProcessedDataID(fname);
                //ViewBag.answer2 = first;
                //ViewBag.answer3 = GetProcessedDataVolume(fname);
                //ViewBag.answer4 = first.Count();
                conn.Close();
                //Console.WriteLine("Rows "+frm["rows"]);
               // try { ViewBag.rows = int.Parse(frm["rows"]); }
                //catch { }
                ViewBag.usingcsvhelper = GetProcessedData(fname);
                // return filename;
                return View("GetFileMetaData");

                // ViewBag.keys = GetProcessedColumnNames(fname);


            }


        }

     
        public List<dynamic> GetProcessedData(string fname)
        {
            //this is sending accidentally to Final folder
            var path = Path.Combine("", _hostingEnvironment.ContentRootPath + "\\Temp\\" + fname);
            string pathOnly = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            using (var reader = new StreamReader(path))
            {
                var line = reader.ReadLine();

                var values = line.Split(',');
                ViewBag.ColumnHeaders = values;
                ViewBag.ColumnCount = values.Length;
            }
            using (var reader = new StreamReader(path))
            {
                /* var line = reader.ReadLine();

                 var values = line.Split(',');
                 ViewBag.columns = values.Length;*/
                using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
                {

                    var records = csvReader.GetRecords<dynamic>().ToList();

                    return records;
                }

            }



        }




        public List<string> GetProcessedDataID(string fname)
        {
            //this is sending accidentally to Final folder
            var path = Path.Combine("", _hostingEnvironment.ContentRootPath + "\\Temp\\" + fname);
            string pathOnly = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            using (var reader = new StreamReader(path))
            {

                List<string> listA = new List<string>();
                List<string> listB = new List<string>();
                int c = 0;
                while (!reader.EndOfStream)
                {
                    if (c == 10) break;
                    c = c + 1;
                    var line = reader.ReadLine();

                    var values = line.Split(',');

                    listA.Add(values[0]);


                }

                return listA;
            }
        }
        public List<string> GetProcessedDataVolume(string fname)
        {
            //this is sending accidentally to Final folder
            var path = Path.Combine("", _hostingEnvironment.ContentRootPath + "\\Temp\\" + fname);
            string pathOnly = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            using (var reader = new StreamReader(path))
            {


                List<string> listB = new List<string>();
                int c = 0;
                while (!reader.EndOfStream)
                {
                    if (c == 10) break;
                    c = c + 1;
                    var line = reader.ReadLine();

                    var values = line.Split(',');


                    listB.Add(values[1]);

                }

                return listB;
            }



        }
        public List<string> GetData()
        {
            if (_signinManager.IsSignedIn(User))
            {

                Username = User.Identity.Name;

            }
            //            Console.WriteLine("The " + Username);
            var flag = "True";
            var connstr = _config.GetConnectionString("UserIdentityDBConnection"); //"Server=DESKTOP-JUH932N\\SQLEXPRESS; Database=user; Trusted_Connection=true;";//"Server=PRAKASH; Database=user; Trusted_Connection=true;";
            var query = "select * from FileUploads where UserName=@Username and IsProcessed=@flag;";
            using (var conn = new SqlConnection(connstr))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add(
                   "@Username",
                   SqlDbType.NVarChar).SqlValue = Username;
                cmd.Parameters.Add("@flag",
                   SqlDbType.NVarChar).SqlValue = flag;

                conn.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {

                    result1.Add(Convert.ToString(reader["FileName"]));
                }
                reader.Close();
                conn.Close();


            }
           // Console.WriteLine(result1);
            return result1;
        }
        [HttpGet]
        public ActionResult<List<string>> HomeView()
        {
            var result = new HttpResponseMessage(System.Net.HttpStatusCode.OK);


            result1 = GetData();
           // Console.WriteLine(result1);
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


//private async Task LoadAsync(IdentityUser user)
//{
//    var userName = await _userManager.GetUserNameAsync(user);
//    var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
//    Console.WriteLine(Username);
//    Username = userName;
//}


//Trying using ado.net

//[HttpGet("{id}")]
//public ActionResult<FileUpload> Download3(int id)
//{
//    var result = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
//    var filename = "";
//    var filepath = "";
//    var connstr = "Server=DESKTOP-JUH932N\\SQLEXPRESS; Database=user; Trusted_Connection=true;";
//    var query = "select FilePath from FileUploads  where F_ID=@id;";
//    using (var conn = new SqlConnection(connstr))
//    using (var cmd = new SqlCommand(query, conn))
//    {
//        cmd.Parameters.Add(
//           "@id",
//           SqlDbType.Int).SqlValue = id;

//        conn.Open();
//        var reader = cmd.ExecuteReader();

//        while (reader.Read())
//        {
//            filepath = reader.GetString(0);
//        }
//        /*if (string.IsNullOrWhiteSpace(filepath))
//        {
//            result.StatusCode = System.Net.HttpStatusCode.NotFound;
//            return 
//        }*/
//        filename = Path.GetFileName(filepath);
//        var filebytes = System.IO.File.ReadAllBytes(filepath);

//        var filestream = new MemoryStream(filebytes);
//        result.Content = new StreamContent(filestream);
//        var headers = result.Content.Headers;
//        headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
//        headers.ContentDisposition.Name = filename;
//        headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
//        headers.ContentLength = filestream.Length;
//        FileContentResult result1 = new FileContentResult(System.IO.File.ReadAllBytes(filepath), "text/csv")
//        {
//            FileDownloadName = "myFile.docx"
//        };
//        conn.Close();
//        return result1;


//}

// }

/*if (string.IsNullOrWhiteSpace(filepath))
               {
                   result.StatusCode = System.Net.HttpStatusCode.NotFound;
                   return 
               }*/



//[HttpGet]
//public ActionResult<List<FileUpload>> GetFileUpload()
//{
//    var result = _dbcontext.FileUploads.ToList();

//    return result;

//}

//[HttpGet("{id}")]
//public ActionResult<FileUpload> Get(int id)
//{
//    var file = _dbcontext.FileUploads.FirstOrDefault(c => c.F_ID == id);
//    if (file == null)
//    {
//        return NotFound(new { Message = "no file has not been found." });
//    }

//    return Ok(file.FilePath);
//}

////downloadinnggggg
//[HttpGet("{id}")]
//public ActionResult<FileUpload> Download(int id)
//{
//    var file = _dbcontext.FileUploads.FirstOrDefault(c => c.F_ID == id);
//    if (file == null)
//    {
//        return NotFound(new { Message = "no file has not been found." });
//    }

//    string path = file.FilePath;
//    FileContentResult result = new FileContentResult(System.IO.File.ReadAllBytes(path), "text/csv")
//    {
//        FileDownloadName = "myFile.docx"
//    };
//    return result;
//}
////downloadinngg   by using file Name
//[HttpGet("{FileName}")]
//public ActionResult<FileUpload> Download2(string FileName)
//{
//    var file = _dbcontext.FileUploads.FirstOrDefault(c => c.FileName == FileName);
//    if (file == null)
//    {
//        return NotFound(new { Message = "no file has not been found." });
//    }

//    string path = file.FilePath;
//    FileContentResult result = new FileContentResult(System.IO.File.ReadAllBytes(path), "text/csv")
//    {
//        FileDownloadName = "myFile.docx"
//    };
//    return result;
//}

////update option if file name has to be changed
//[HttpPost("{id}/{name}")]
//public ActionResult<string> UpdateName(int id, string name)
//{
//    try
//    {
//        var file = _dbcontext.FileUploads.FirstOrDefault(c => c.F_ID == id);
//        if (file == null)
//        {
//            return NotFound(new { Message = "no file has not been found." });
//        }
//        file.FileName = name;
//        /*var imageupload = new ImageUpload();
//        imageupload.FileName = name;*/
//        _dbcontext.SaveChanges();
//        return file.FileName;


//    }
//    catch (Exception e)
//    {
//        return e.Message;
//    }
//}


//[HttpPost]
//public ActionResult<string> UploadFiles()
//{
//    var t = (_signinManager.IsSignedIn(User));
//    try
//    {
//        var files = HttpContext.Request.Form.Files;
//        if (files != null && files.Count > 0)
//        {
//            foreach (var file in files)
//            {
//                FileInfo fi = new FileInfo(file.FileName);
//                //var filename = file.FileName;
//                //guid
//               var  uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
//                var path = Path.Combine("", _hostingEnvironment.ContentRootPath + "\\Temp\\" + uniqueFileName);

//                using (var stream = new FileStream(path, FileMode.Create))
//                {
//                    file.CopyTo(stream);
//                }
//                if (_signinManager.IsSignedIn(User))
//                {

//                    Username = User.Identity.Name;

//                }


//                var fileupload = new FileUpload();
//                fileupload.FilePath = path;
//                fileupload.IsProcessed = "false";
//                 fileupload.UserName = Username;




//              //  ApplicationUser currentUser = db.Users.FirstOrDefault(x => x.Id == currentUserId);

//                //for debugging
//                // fileupload.UserName = "prakashgvs789";


//                //should update foreign keyy

//                fileupload.InsertedOn = DateTime.Today;
//                fileupload.FileName = uniqueFileName;
//                _dbcontext.FileUploads.Add(fileupload);
//                _dbcontext.SaveChanges();

//            }
//            return "Saved succesfully"+t+"Saved successfully";

//        }
//        else
//        {
//            return "Select Files";
//        }

//    }
//    catch (Exception e)
//    {
//        return e.Message;
//    }



//}


//public ActionResult<string> UploadFileAdo()
//{
//    try
//    {
//        var files = HttpContext.Request.Form.Files;
//        if (files != null && files.Count > 0)
//        {
//            foreach (var file in files)
//            {
//                FileInfo fi = new FileInfo(file.FileName);
//                //var filename = file.FileName;
//                //guid
//                var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
//                var ext = Path.GetExtension(uniqueFileName);
//                if (ext != ".csv")
//                {
//                    StatusMessage = "Only csv Files are Accepted";
//                }
//                var path = Path.Combine("", _hostingEnvironment.ContentRootPath + "\\Temp\\" + uniqueFileName);

//                // var path = Path.Combine("", _hostingEnvironment.ContentRootPath + "\\Temp\\" + uniqueFileName);

//                using (var stream = new FileStream(path, FileMode.Create))
//                {
//                    file.CopyTo(stream);
//                }
//                var time = DateTime.Today;
//                if (_signinManager.IsSignedIn(User))
//                {

//                    Username = User.Identity.Name;

//                }

//                var connstr = "Server=DESKTOP-JUH932N\\SQLEXPRESS; Database=user; Trusted_Connection=true;";

//                var query = "INSERT INTO dbo.FileUploads(" + " UserName, FileName, FilePath, InsertedOn,IsProcessed " + ") VALUES(" + " @user, @file, @filpath,@insert,@process);";
//                using (var conn = new SqlConnection(connstr))
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    conn.Open();
//                    cmd.Parameters.AddWithValue("@user", Username);
//                    cmd.Parameters.AddWithValue("@file", uniqueFileName);
//                    cmd.Parameters.AddWithValue("@filpath", path);
//                    cmd.Parameters.AddWithValue("@insert", time);
//                    cmd.Parameters.AddWithValue("@process", "false");


//                    cmd.ExecuteNonQuery();
//                    conn.Close();
//                }

//            }
//            StatusMessage = "File Uploaded Successfully.";

//        }
//        else
//        {
//            StatusMessage = "Select Files.";
//        }

//    }
//    catch (Exception e)
//    {
//        StatusMessage = e.Message;
//    }

//    return StatusMessage;

//}



/* string csvData = System.IO.File.ReadAllText(path);
 int count = 0;
 foreach (string row in csvData.Split('\n'))
 {
     if (!string.IsNullOrEmpty(row))
     {
         count += 1;
     }
 }*/
// return count.ToString();
/*StringBuilder sb = new StringBuilder();
using (StreamReader sr = new StreamReader(path, Encoding.Default, true))
{
    String line;
    // Read and display lines from the file until the end of 
    // the file is reached.
    while ((line = sr.ReadLine()) != null)
    {
        sb.AppendLine(line);
    }
}
string allines = sb.ToString();


UTF8Encoding utf8 = new UTF8Encoding();


var preamble = utf8.GetPreamble();

var data = utf8.GetBytes(allines);

string hexString = BitConverter.ToString(data).Replace("-", string.Empty).ToLower();

return hexString+"helll";
*/
/* var records = new List<string>();

 string test = "hello";
 int count = 0;
 using (var stream = new FileStream(path, FileMode.Create))
 using (var sreader = new StreamReader(stream))
     while (!sreader.EndOfStream)
     {
         count += 1;
         string[] rows = sreader.ReadLine().Split(',');
         records.Add(rows[1]);
         test = count.ToString();

     }*/

// return count.ToString()+path+csvData;


//[HttpGet]
//public ActionResult<FileUpload> Download3(int? id, string fname)
//{
//    var result = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
//    var filename = "";
//    var filepath = "";
//    var connstr = _config.GetConnectionString("UserIdentityDBConnection"); //"Server=DESKTOP-JUH932N\\SQLEXPRESS; Database=user; Trusted_Connection=true;";
//    var query = "select FilePath from FileUploads  where FileName=@fname;";
//    using (var conn = new SqlConnection(connstr))
//    using (var cmd = new SqlCommand(query, conn))
//    {
//        cmd.Parameters.Add(
//           "@fname",
//           SqlDbType.NVarChar).SqlValue = fname;

//        conn.Open();
//        var reader = cmd.ExecuteReader();

//        while (reader.Read())
//        {
//            filepath = reader.GetString(0);
//        }

//        filename = Path.GetFileName(filepath);
//        var filebytes = System.IO.File.ReadAllBytes(filepath);

//        var filestream = new MemoryStream(filebytes);
//        result.Content = new StreamContent(filestream);
//        var headers = result.Content.Headers;
//        headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
//        headers.ContentDisposition.Name = filename;
//        headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
//        headers.ContentLength = filestream.Length;
//        FileContentResult result1 = new FileContentResult(System.IO.File.ReadAllBytes(filepath), "text/csv")
//        {
//            FileDownloadName = filename.Substring(37)
//        };
//        conn.Close();
//        return result1;


//    }

//}