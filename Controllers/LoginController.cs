using MediLinkCB.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Net.Mail;
using System.Net;
using BCrypt.Net; // Use BCrypt.Net package
using System.Text;
using System.Security.Cryptography;
using MediLinkCB.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MediLinkCB.Controllers
{
    
    public class LoginController : Controller
    {
        private MediLinkDBEntities7 db = new MediLinkDBEntities7();
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserProfile objUser)
        {
            if (ModelState.IsValid)
            {
                using (MediLinkDBEntities7 db = new MediLinkDBEntities7())
                {
                    var obj = db.UserProfiles.FirstOrDefault(a => a.UserName.Equals(objUser.UserName));
                    if (obj != null)
                    {
                        if (BCrypt.Net.BCrypt.Verify(objUser.PasswordHash, obj.PasswordHash))
                        {
                            Session["UserId"] = obj.UserID.ToString();
                            Session["UserName"] = obj.UserName.ToString();
                            Session["UserType"] = obj.UserType.ToString(); // Save user type in session

                            // Redirect based on UserType
                            if (obj.UserName == "admin")
                            {
                                Session.Timeout = 5;
                                return RedirectToAction("AdminDashboard");
                            }
                            else if (obj.UserName == "clerk")
                            {
                                Session.Timeout = 5;
                                return RedirectToAction("Dashboard", "Login");
                            }
                            else
                            {
                                return RedirectToAction("MainDashboard");
                            }
                        }
                        else
                        {
                            ViewBag.Message = "Incorrect password.";
                        }
                    }
                    else
                    {
                        ViewBag.Message = "User not found.";
                    }
                }
            }
            return View(objUser);
        }

        public ActionResult Dashboard()
        {
            Session.Timeout = 5;
            var patients = db.Patients.ToList(); 
            return View(patients);
        }

        public ActionResult MainDashboard()
        {
            if (Session["UserID"] != null && Session["UserName"].ToString() != "admin")
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        public ActionResult ClerkDashboard()
        {
            if (Session["UserID"] != null && Session["UserName"].ToString() != "admin")
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        // A method to hash passwords before saving them to the database
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // GET: Admin Dashboard - only accessible by Admin
        //[Authorize(Roles = "admin")]
        public ActionResult AdminDashboard()
        {
            if (Session["UserID"] != null && Session["UserName"].ToString() == "admin")
            {
                //return View();
                return View("AdminDashboard");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        // GET: Add User
        //[Authorize(Roles = "admin", Users = "admin")]
        public ActionResult AddUser()
        {
            return View();
        }

        // POST: Add User
        [HttpPost]
        //[Authorize(Roles = "admin", Users = "admin")]
        [ValidateAntiForgeryToken]
        public ActionResult AddUser(UserProfile newUser)
        {
            if (ModelState.IsValid)
            {
                using (MediLinkDBEntities7 db = new MediLinkDBEntities7())
                {
                    // Check if the user already exists
                    if (db.UserProfiles.Any(u => u.UserName == newUser.UserName))
                    {
                        ViewBag.Message = "Username already exists.";
                        return View(newUser);
                    }

                    // Hash the password before storing
                    newUser.PasswordHash = HashPassword(newUser.PasswordHash);

                    db.UserProfiles.Add(newUser);

                    // Generate OTP for account recovery
                    string otp = GenerateOTP();
                    newUser.OTP = otp;
                    newUser.OTPExpiry = DateTime.Now.AddMinutes(15); // OTP is valid for 15 minutes

                    db.SaveChanges();

                    // Send OTP email for account recovery
                    SendEmailOTP(newUser.Email, otp);

                    ViewBag.Message = "User successfully added and OTP sent.";
                }
            }
            return RedirectToAction("Index", "UserProfiles");
        }

        // OTP and password hashing utilities
        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public void SendEmailOTP(string email, string otp)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(email);
                mail.From = new MailAddress("medilinksystem@outlook.com");
                mail.Subject = "Your OTP Code";
                mail.Body = $"Your OTP code is {otp}. It is valid for 10 minutes.";

                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.office365.com",
                    Port = 587,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("medilinksystem@outlook.com", "MediLink2024"),
                    EnableSsl = true
                };

                smtp.Send(mail);
            }
            catch (Exception)
            {
                // Log the error (e.g., to a file or monitoring system)
                // Provide feedback to the user
                ViewBag.Message = "There was an error sending the email. Please try again.";
            }
        }




        // Forgot Password
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Forgot Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            using (MediLinkDBEntities7 db = new MediLinkDBEntities7())
            {
                var user = db.UserProfiles.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    string otp = GenerateOTP();
                    user.OTP = otp;
                    user.OTPExpiry = DateTime.Now.AddMinutes(15);
                    db.SaveChanges();

                    SendEmailOTP(user.Email, otp);
                    ViewBag.Message = "OTP sent to your email address.";

                    // Redirect to VerifyOTP view
                    return RedirectToAction("VerifyOTP", new { email = user.Email });
                }
                else
                {
                    ViewBag.Message = "Email address not found.";
                }
            }
            return View();
        }

        // GET: OTP Verification
        public ActionResult VerifyOTP(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        // POST: OTP Verification
        // POST: OTP Verification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyOTP(string email, string otp)
        {
            try
            {
                using (MediLinkDBEntities7 db = new MediLinkDBEntities7())
                {
                    // Find the user by email and OTP, and ensure the OTP hasn't expired
                    var user = db.UserProfiles.FirstOrDefault(u => u.Email == email && u.OTP == otp && u.OTPExpiry > DateTime.Now);
                    if (user != null)
                    {
                        // Clear the OTP after successful verification
                        user.OTP = null;
                        user.OTPExpiry = DateTime.Now;
                        db.SaveChanges();

                        // Redirect to the Reset Password page
                        return RedirectToAction("ResetPassword", new { email = user.Email });
                    }
                    else
                    {
                        // OTP is either incorrect or expired
                        ViewBag.Message = "Invalid OTP or OTP has expired.";
                    }
                }
            }
            catch (Exception)
            {
                // only added a catch to this code
                ViewBag.Message = "An error occurred while verifying the OTP. Please try again.";


            }
            return View();
        }


        // GET: Reset Password
        public ActionResult ResetPassword(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        // POST: Reset Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string email, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Message = "Passwords do not match.";
                return View();
            }

            using (MediLinkDBEntities7 db = new MediLinkDBEntities7())
            {
                var user = db.UserProfiles.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    // Hash the new password before storing it
                    user.PasswordHash = HashPassword(newPassword);
                    db.SaveChanges();

                    ViewBag.Message = "Password successfully updated.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.Message = "User not found.";
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult Logout()
        {
            // End the user session
            Session.Clear();
            Session.Abandon();

            // Redirect to login page or any other page after logout
            return RedirectToAction("Login", "Login");
        }


    }
}
