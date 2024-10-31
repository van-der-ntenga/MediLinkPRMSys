using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MediLinkCB.Models;
using PagedList;

namespace MediLinkCB.Controllers
{
    public class UserProfilesController : Controller
    {
        private MediLinkDBEntities13 db = new MediLinkDBEntities13();

        // GET: UserProfiles
        public ActionResult Index(string searchUsername, string searchUserType, string searchSpecialisation, int? page)
        {
            int pageSize = 10;  // Number of records per page
            int pageNumber = (page ?? 1);  // Default to page 1 if null

            var users = db.UserProfiles.AsQueryable();

            // Search by Username
            if (!string.IsNullOrEmpty(searchUsername))
            {
                users = users.Where(u => u.UserName.Contains(searchUsername));
            }

            // Search by UserType
            if (!string.IsNullOrEmpty(searchUserType))
            {
                users = users.Where(u => u.UserType == searchUserType);
            }

            // Search by Specialisation (only if UserType is 'Doctor')
            if (!string.IsNullOrEmpty(searchSpecialisation) && searchUserType == "Doctor")
            {
                users = users.Where(u => u.Specialisation.Contains(searchSpecialisation));
            }

            // Paginate results
            var pagedUsers = users.OrderBy(u => u.UserID).ToPagedList(pageNumber, pageSize);

            return View(pagedUsers);
        }


        // GET: UserProfiles/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserProfile userProfile = db.UserProfiles.Find(id);
            if (userProfile == null)
            {
                return HttpNotFound();
            }
            return View(userProfile);
        }

        // GET: UserProfiles/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserProfiles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserId,UserName,PasswordHash,Email,UserType,IsActive,OTP,OTPExpiry")] UserProfile userProfile)
        {
            if (ModelState.IsValid)
            {
                db.UserProfiles.Add(userProfile);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(userProfile);
        }

        // GET: UserProfiles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserProfile userProfile = db.UserProfiles.Find(id);
            if (userProfile == null)
            {
                return HttpNotFound();
            }
            return View(userProfile);
        }

        // POST: UserProfiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserName,Email,UserType,IsActive")] UserProfile userProfile)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userProfile).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(userProfile);
        }

        // GET: UserProfiles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserProfile userProfile = db.UserProfiles.Find(id);
            if (userProfile == null)

            {
                return HttpNotFound();
            }
            return View(userProfile);
        }

        // POST: UserProfiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserProfile userProfile = db.UserProfiles.Find(id);
            db.UserProfiles.Remove(userProfile);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: Reports
        public ActionResult Reports()
        {
            // Calculate total user count
            var totalUserCount = db.UserProfiles.Count();

            // Calculate count for each user type
            var doctorCount = db.UserProfiles.Count(u => u.UserType == "Doctor");
            var clerkCount = db.UserProfiles.Count(u => u.UserType == "Clerk");
            var nurseCount = db.UserProfiles.Count(u => u.UserType == "Nurse");

            // Create a ViewModel to pass data to the view
            var reportViewModel = new UserReportViewModel
            {
                TotalUserCount = totalUserCount,
                DoctorCount = doctorCount,
                ClerkCount = clerkCount,
                NurseCount = nurseCount
            };

            return View(reportViewModel);
        }
    }
}
