using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MVCapplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCapplication.Controllers
{
    //public class UpdateController
    //{
    //    public ActionResult UpdateRecord(FormCollection frm, string action)
    //    {
    //        Console.WriteLine("Controller class");
    //        if (action == "Submit")
    //        {
    //            UpdateUser model = new UpdateUser();
    //            string previousname = frm["txtuser"];
    //            string name = frm["txtFName"]+frm["txtLName"];
    //            string email = Convert.ToString(frm["txtEmail"]);
    //            string password = frm["password"];
    //            string phno = frm["txtPhno"];
    //            int status = model.Update(previousname,name,email,password,phno);
    //            Console.WriteLine(status);
    //        }
    //        return (ActionResult)View(new UpdateUser());
    //    }

    //    private IActionResult View(UpdateUser updateUser)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
