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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

       
    }

    public class UpdateController 
    {
        //public IActionResult UpdateRecord(FormCollection frm, string action)
        //{
        //    Console.WriteLine("Controller class");
        //    if (action == "Submit")
        //    {
        //        UpdateUser model = new UpdateUser();
        //        string previousname = frm["txtuser"];
        //        string name = frm["txtFName"] + frm["txtLName"];
        //        string email = Convert.ToString(frm["txtEmail"]);
        //        string password = frm["password"];
        //        string phno = frm["txtPhno"];
        //        int status = model.Update(previousname, name, email, password, phno);
        //        Console.WriteLine(status);
        //    }

        //    return View();
            
        //}

      
    }
}
