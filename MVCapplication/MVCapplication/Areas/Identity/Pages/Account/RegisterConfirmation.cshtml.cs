using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Net.Mail;
using System.Net;

namespace MVCapplication.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _sender;

        public RegisterConfirmationModel(UserManager<IdentityUser> userManager, IEmailSender sender)
        {
            _userManager = userManager;
            _sender = sender;
        }

        public string Email { get; set; }

        public bool DisplayConfirmAccountLink { get; set; }

        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;
            // Once you add a real email sender, you should remove this code that lets you confirm the account
            DisplayConfirmAccountLink = true;
            if (DisplayConfirmAccountLink)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                Console.WriteLine(user);
                EmailConfirmationUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);
                try
                {
                    Console.WriteLine("Sending mail");
                    var gmail = await _userManager.FindByEmailAsync(email);
                    string HostAddress = "smtp.gmail.com";
                    string FormEmailId = "trafficprediction789@gmail.com";
                    string Password = "Prakash@7899";
                    string Port = "587";
                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(FormEmailId);
                    mailMessage.Subject = "Confirmation Email"; 
                    string body = "<a href= " + EmailConfirmationUrl + ">Click Here</a>";
                    mailMessage.Body = "Click the link below to confirm your account and login\n\n"+body ;
                    mailMessage.IsBodyHtml = true;
                    mailMessage.To.Add(new MailAddress(gmail.ToString()));
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
            }
            
            return Page();
        }
    }
}
