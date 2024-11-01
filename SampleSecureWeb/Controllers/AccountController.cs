using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SampleSecuredWeb.ViewModel;
using SampleSecureWeb.Data;
using SampleSecureWeb.Models;
using SampleSecureWeb.ViewModels;
using System.Text.RegularExpressions;

namespace SampleSecureWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUser _userData;
        
        public AccountController(IUser user)
        {
            _userData = user;
        }

        // GET: AccountController
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegistrationViewModel registrationViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = new Models.User
                    {
                        Username = registrationViewModel.Username,
                        Password = NormalizePassword(registrationViewModel.Password),
                        RoleName = "contributor"
                    };
                    _userData.Registration(user);
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View(registrationViewModel);
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel loginViewModel)
        {
            try
            {
                loginViewModel.ReturnUrl = loginViewModel.ReturnUrl ?? Url.Content("~/");

                var user = new User
                {
                    Username = loginViewModel.Username,
                    Password = loginViewModel.Password
                };

                var loginUser = _userData.Login(user);
                if (loginUser == null)
                {
                    ViewBag.Message = "Invalid login attempt.";
                    return View(loginViewModel);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username)
                };
                var identity = new ClaimsIdentity(claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = loginViewModel.RememberLogin
                    });
                return RedirectToAction("Index", "Home");
            }
            catch (System.Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return View(loginViewModel);
        }

        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(changePasswordViewModel);
            }

            try
            {
                var newPassword = NormalizePassword(changePasswordViewModel.NewPassword) ?? string.Empty;
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    ModelState.AddModelError(string.Empty, "User tidak ditemukan. Coba login kembali.");
                    return View(changePasswordViewModel);
                }

                var currentUser = _userData.GetCurrentUser(username);
                if (currentUser == null || !BCrypt.Net.BCrypt.Verify(changePasswordViewModel.CurrentPassword, currentUser.Password))
                {
                    ModelState.AddModelError("CurrentPassword", "Password saat ini tidak benar.");
                    return View(changePasswordViewModel);
                }

                currentUser.Password = newPassword; // Pastikan untuk mengenkripsi password ini sebelum menyimpan
                _userData.UpdatePassword(currentUser);
                
                ViewBag.Message = "Password berhasil diubah!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View(changePasswordViewModel);
        }

        // Utility function to normalize password
        private string NormalizePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return password;
            }
            // Replace consecutive spaces with a single space
            return Regex.Replace(password.Trim(), @"\s+", " ");
        }
    }
}
