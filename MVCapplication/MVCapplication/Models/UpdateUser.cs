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