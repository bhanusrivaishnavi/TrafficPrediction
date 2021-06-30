using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Mail;
using System.Net;

namespace MVCapplication.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                try
                {
                    Console.WriteLine("Sending mail");
                   // var gmail = await _userManager.FindByEmailAsync(email);
                    string HostAddress = "smtp.gmail.com";
                    string FormEmailId = "vassrini962@gmail.com";
                    string Password = "Prakash@7899";
                    string Port = "587";
                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(FormEmailId);
                    mailMessage.Subject = "Confirmation Email";
                    string body = "<a href= " + callbackUrl+">Click Here</a>";
                    Console.WriteLine(body);
                    mailMessage.Body = "Click the link below to reset your password.\n\n" + body;
                    mailMessage.IsBodyHtml = true;
                    mailMessage.To.Add(new MailAddress(Input.Email));
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = HostAddress;
                    smtp.EnableSsl = true;
                    NetworkCredential networkCredential = new NetworkCredential();
                    networkCredential.UserName = mailMessage.From.Address;
                    networkCredential.Password = Password;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = networkCredential;
                    smtp.Port = Convert.ToInt32(Port);
                    smtp.Send(mailMessage);
                    Console.WriteLine("Mail sent");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e + "Mail not sent");

                }
                //await _emailSender.SendEmailAsync(
                //    Input.Email,
                //    "Reset Password",
                //    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
