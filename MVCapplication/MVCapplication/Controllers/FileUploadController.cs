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


namespace MVCapplication.Controllers
{
   // [Authorize]
    
    [Route("api/[controller]/[action]")]
    [ApiController]
    
    public class FileUploadController : ControllerBase
    {
        ApplicationUser au = new ApplicationUser();

        public IWebHostEnvironment _hostingEnvironment;
        public ApplicationDbContext _dbcontext;
        //private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signinManager;


        public string Username;

       

        public FileUploadController(IWebHostEnvironment hostingEnvironment, ApplicationDbContext dbcontext, /*UserManager<IdentityUser> userManager,*/ SignInManager<IdentityUser> signinManager)
        {
            _hostingEnvironment = hostingEnvironment;
            _dbcontext = dbcontext;
            _signinManager = signinManager;
          //  _userManager = userManager;

        }
        //private async Task LoadAsync(IdentityUser user)
        //{
        //    var userName = await _userManager.GetUserNameAsync(user);
        //    var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        //    Console.WriteLine(Username);
        //    Username = userName;
        //}
        [HttpPost]
        public ActionResult<string> UploadFilesAsync()
        {
            var t = (_signinManager.IsSignedIn(User));
            try
            {
                var files = HttpContext.Request.Form.Files;
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        FileInfo fi = new FileInfo(file.FileName);
                        //var filename = file.FileName;
                        //guid
                       var  uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        var path = Path.Combine("", _hostingEnvironment.ContentRootPath + "\\Temp\\" + uniqueFileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        if (_signinManager.IsSignedIn(User))
                        {

                            Username = User.Identity.Name;

                        }


                        var fileupload = new FileUpload();
                        fileupload.FilePath = path;
                        fileupload.IsProcessed = "false";
                         fileupload.UserName = Username;
                      


                      
                      //  ApplicationUser currentUser = db.Users.FirstOrDefault(x => x.Id == currentUserId);

                        //for debugging
                        // fileupload.UserName = "prakashgvs789";


                        //should update foreign keyy

                        fileupload.InsertedOn = DateTime.Today;
                        fileupload.FileName = uniqueFileName;
                        _dbcontext.FileUploads.Add(fileupload);
                        _dbcontext.SaveChanges();

                    }
                    return "Saved succesfully"+t+"Saved successfully";

                }
                else
                {
                    return "Select Files";
                }

            }
            catch (Exception e)
            {
                return e.Message;
            }


            
        }

        [HttpGet]
        public ActionResult<List<FileUpload>> GetFileUpload()
        {
            var result = _dbcontext.FileUploads.ToList();

            return result;

        }

        [HttpGet("{id}")]
        public ActionResult<FileUpload> Get(int id)
        {
            var file = _dbcontext.FileUploads.FirstOrDefault(c => c.F_ID == id);
            if (file == null)
            {
                return NotFound(new { Message = "no file has not been found." });
            }

            return Ok(file.FilePath);
        }

        //downloadinnggggg
        [HttpGet("{id}")]
        public ActionResult<FileUpload> Download(int id)
        {
            var file = _dbcontext.FileUploads.FirstOrDefault(c => c.F_ID == id);
            if (file == null)
            {
                return NotFound(new { Message = "no file has not been found." });
            }

            string path = file.FilePath;
            FileContentResult result = new FileContentResult(System.IO.File.ReadAllBytes(path), "text/csv")
            {
                FileDownloadName = "myFile.docx"
            };
            return result;
        }
        //downloadinngg   by using file Name
        [HttpGet("{FileName}")]
        public ActionResult<FileUpload> Download2(string FileName)
        {
            var file = _dbcontext.FileUploads.FirstOrDefault(c => c.FileName == FileName);
            if (file == null)
            {
                return NotFound(new { Message = "no file has not been found." });
            }

            string path = file.FilePath;
            FileContentResult result = new FileContentResult(System.IO.File.ReadAllBytes(path), "text/csv")
            {
                FileDownloadName = "myFile.docx"
            };
            return result;
        }

        //update option if file name has to be changed
        [HttpPost("{id}/{name}")]
        public ActionResult<string> UpdateName(int id, string name)
        {
            try
            {
                var file = _dbcontext.FileUploads.FirstOrDefault(c => c.F_ID == id);
                if (file == null)
                {
                    return NotFound(new { Message = "no file has not been found." });
                }
                file.FileName = name;
                /*var imageupload = new ImageUpload();
                imageupload.FileName = name;*/
                _dbcontext.SaveChanges();
                return file.FileName;


            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        //Trying using ado.net

        [HttpGet("{id}")]
        public ActionResult<FileUpload> Download3(int id)
        {
            var result = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            var filename = "";
            var filepath = "";
            var connstr = "Server=PRAKASH; Database=user; Trusted_Connection=true;";
            var query = "select FilePath from FileUploads  where F_ID=@id;";
            using (var conn = new SqlConnection(connstr))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add(
                   "@id",
                   SqlDbType.Int).SqlValue = id;

                conn.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    filepath = reader.GetString(0);
                }
                /*if (string.IsNullOrWhiteSpace(filepath))
                {
                    result.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return 
                }*/
                filename = Path.GetFileName(filepath);
                var filebytes = System.IO.File.ReadAllBytes(filepath);

                var filestream = new MemoryStream(filebytes);
                result.Content = new StreamContent(filestream);
                var headers = result.Content.Headers;
                headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                headers.ContentDisposition.Name = filename;
                headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                headers.ContentLength = filestream.Length;
                FileContentResult result1 = new FileContentResult(System.IO.File.ReadAllBytes(filepath), "text/csv")
                {
                    FileDownloadName = "myFile.docx"
                };
                conn.Close();
                return result1;

                
            }

        }

    }
}
