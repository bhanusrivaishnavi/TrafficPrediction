using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVCapplication.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MVCapplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UpdateUser()
        {
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        public IActionResult UpdateRecord(IFormCollection frm, string action)
        {
            Console.WriteLine("Controller class"+action);
            if (action == "Update")
            {
                UpdateUser model = new UpdateUser();
                string previousname = frm["txtuser"];
                string name = frm["txtFName"] + frm["txtLName"];
                string email = Convert.ToString(frm["txtEmail"]);
                string password = frm["txtpswd1"];
                string phno = frm["txtPhno"];
                Console.WriteLine(frm["txtuser"]+"PreviousName:"+previousname+"\n"+name + "\n" + email + "\n" + password + "\n" + phno);
                string status=model.Update(previousname, name, email, password, phno);
               Console.WriteLine(status);
            }
            return View();
        }

    }
}
