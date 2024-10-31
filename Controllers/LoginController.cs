using MediLinkCB.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Net.Mail;
using System.Net;
using BCrypt.Net; 
using System.Text;
using System.Security.Cryptography;
using MediLinkCB.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PagedList;
using System.Web.UI;
using System.Collections.Generic;
using System.Diagnostics;

namespace MediLinkCB.Controllers
{
    
    public class LoginController : Controller
    {
        private MediLinkDBEntities13 db = new MediLinkDBEntities13();
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
                using (MediLinkDBEntities13 db = new MediLinkDBEntities13())
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
                            if (obj.UserType == "Admin" || obj.UserName=="Admin")

                            {
                                Session.Timeout = 15;
                                return RedirectToAction("AdminDashboard");
                            }
                            else if (obj.UserType == "Clerk" || obj.UserName=="Clerk")
                            {
                                Session.Timeout = 15;
                                return RedirectToAction("ClerkDashboard", "Login");
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
            Session.Timeout = 15;
            var patients = db.Patients.ToList(); 
            return View(patients);
        }

        public ActionResult MainDashboard(string searchSaID, int? page)
        {
            if (Session["UserId"] != null && Session["UserName"] != null && Session["UserName"].ToString() != "admin")
            {
                int pageSize = 10;
                int pageNumber = (page ?? 1);


                IQueryable<Patient> patientsQuery = db.Patients;


                if (!string.IsNullOrEmpty(searchSaID))
                {

                    patientsQuery = patientsQuery.Where(p => p.PatientID.Contains(searchSaID));


                    if (!patientsQuery.Any())
                    {
                        ViewBag.Message = "No patient found with the provided ID Number.";
                        return View(new List<Patient>().ToPagedList(pageNumber, pageSize));
                    }
                }


                var patientsPaged = patientsQuery.OrderBy(p => p.PatientID).ToPagedList(pageNumber, pageSize);


                return View(patientsPaged);
            }
            else
            {

                return RedirectToAction("Login");
            }
        }
        public ActionResult ClerkDashboard(string searchSaID, int? page)
        {
            if (Session["UserId"] != null && Session["UserName"] != null && Session["UserName"].ToString() != "admin")
            {
                int pageSize = 10; 
                int pageNumber = (page ?? 1);

                
                IQueryable<Patient> patientsQuery = db.Patients;

                
                if (!string.IsNullOrEmpty(searchSaID))
                {
                    
                    patientsQuery = patientsQuery.Where(p => p.PatientID.Contains(searchSaID));

                    
                    if (!patientsQuery.Any())
                    {
                        ViewBag.Message = "No patient found with the provided ID Number.";
                        return View(new List<Patient>().ToPagedList(pageNumber, pageSize));
                    }
                }

                
                var patientsPaged = patientsQuery.OrderBy(p => p.PatientID).ToPagedList(pageNumber, pageSize);

                
                return View(patientsPaged);
            }
            else
            {
               
                return RedirectToAction("Login");
            }
        }


        
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        
        public ActionResult AdminDashboard(int? page)
        {
            if (Session["UserId"] != null && Session["UserName"]!=null)
            {
                
                int pageNumber = page ?? 1;

                var users = db.UserProfiles.OrderBy(u => u.UserName).ToPagedList(pageNumber, 10);

                return View(users);
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
        [ValidateAntiForgeryToken]
        public ActionResult AddUser(UserProfilesViewModel newUser)
        {
            // Validate Username
            if (!IsValidUserName(newUser.UserName))
            {
                ViewBag.Message = "Username must be a single word without numbers or special characters.";
                return View(newUser);
            }

            // Validate Password
            if (!IsValidPassword(newUser.Password))
            {
                ViewBag.Message = "Password must be at least 8 characters long, contain at least one number, and one special character.";
                return View(newUser);
            }

            if (ModelState.IsValid)
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }

                using (MediLinkDBEntities13 db = new MediLinkDBEntities13())
                {
                    // Check if the user already exists
                    if (db.UserProfiles.Any(u => u.UserName == newUser.UserName))
                    {
                        ViewBag.Message = "Username already exists.";
                        return View(newUser);
                    }

                    var userProfile = new UserProfile
                    {
                        UserName = newUser.UserName,
                        PasswordHash = HashPassword(newUser.Password), // Hash the password before storing
                        Email = newUser.Email,
                        UserType = newUser.UserType,
                        IsActive = newUser.IsActive,
                        Specialisation = newUser.UserType == "Doctor" ? newUser.Specialisation : null
                    };

                    // Generate OTP for account recovery
                    string otp = GenerateOTP();
                    userProfile.OTP = otp;
                    userProfile.OTPExpiry = DateTime.Now.AddMinutes(15); // OTP is valid for 15 minutes

                    db.UserProfiles.Add(userProfile);
                    db.SaveChanges();

                    ViewBag.Message = "User successfully added and OTP sent.";
                }
                return RedirectToAction("Index", "UserProfiles");
            }

            return View(newUser);
        }

        private bool IsValidPassword(string password)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$");
            if (!regex.IsMatch(password))
            {
                ViewBag.Message = "Password must be at least 8 characters long, contain at least one number, and one special character.";
                return false;
            }
            return true;
        }

        private bool IsValidUserName(string userName)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"^[A-Za-z]+$");
            if (!regex.IsMatch(userName))
            {
                ViewBag.Message = "Username must be a single word without numbers or special characters.";
                return false;
            }
            return true;
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
                Debug.WriteLine($"Attempting to send OTP to: {email}");

                MailMessage mail = new MailMessage();
                mail.To.Add(email);
                mail.From = new MailAddress("noreply.medilinkportal@gmail.com");  // Update to your new Gmail
                mail.Subject = "Your OTP Code";
                mail.Body = $"Your OTP code is {otp}. It is valid for 10 minutes.";

                Debug.WriteLine("Mail message created successfully.");

                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",   // Use Gmail's SMTP server
                    Port = 587,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("noreply.medilinkportal@gmail.com", "jtjz seqp mtst beva"), // Use Gmail App Password
                    EnableSsl = true
                };

                Debug.WriteLine("SMTP client configured. Attempting to send email.");
                smtp.Send(mail);

                Debug.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occurred while sending email: {ex.Message}");
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
            using (MediLinkDBEntities13 db = new MediLinkDBEntities13())
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
                using (MediLinkDBEntities13 db = new MediLinkDBEntities13())
                {
                    
                    var user = db.UserProfiles.FirstOrDefault(u => u.Email == email && u.OTP == otp && u.OTPExpiry > DateTime.Now);
                    if (user != null)
                    {
                        
                        user.OTP = null;
                        user.OTPExpiry = DateTime.Now;
                        db.SaveChanges();

                        
                        return RedirectToAction("ResetPassword", new { email = user.Email });
                    }
                    else
                    {
                        
                        ViewBag.Message = "Invalid OTP or OTP has expired.";
                    }
                }
            }
            catch (Exception)
            {
                
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

            // Validate Password Strength
            if (!IsValidPassword(newPassword))
            {
                ViewBag.Message = "Password must be at least 8 characters long, contain at least one number, and one special character.";
                return View();
            }

            using (MediLinkDBEntities13 db = new MediLinkDBEntities13())
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


            return RedirectToAction("Login", "Login");
        }


    }
}
