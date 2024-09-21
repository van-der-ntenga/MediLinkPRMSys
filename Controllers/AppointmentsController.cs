using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using MediLinkCB.Models;
using MediLinkCB.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MediLinkCB.Controllers
{
    public class AppointmentsController : Controller
    {
        private MediLinkDBEntities7 db = new MediLinkDBEntities7();

        // GET: Appointments
        public ActionResult Index()
        {
            var appointments = db.Appointments.Include(a => a.UserProfile).Include(a => a.Patient);
            return View(appointments.ToList());
        }

        // GET: Appointments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            return View(appointment);
        }

        // GET: Appointments/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                // Fetch the UserID from the session (assuming it's stored during login)
                if (Session["UserId"] != null)
                {
                    appointment.UserID = int.Parse(Session["UserId"].ToString());
                }
                else
                {
                    // Handle the case where the session is empty or the user is not logged in
                    return RedirectToAction("Login", "Login"); // Redirect to login if session is invalid
                }

                db.Appointments.Add(appointment);
                db.SaveChanges();

                // Send an email after appointment creation
                SendEmailAppointmentConfirmation(appointment.EmailAddress, appointment.AppointmentDateTime.ToString(), appointment.AppointmentReason);

                return RedirectToAction("Index");
            }

            return View(appointment);
        }


        private void SendEmailAppointmentConfirmation(string email, string appointmentDate, string appointmentReason)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(email);
                mail.From = new MailAddress("medilinkcode@outlook.com");
                mail.Subject = "Your Appointment Confirmation";
                mail.Body = $"Your appointment is confirmed for {appointmentDate} regarding {appointmentReason}. Please arrive 10 minutes early.";

                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.office365.com",
                    Port = 587,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("medilinkcode@outlook.com", "MediLink2024"),
                    EnableSsl = true
                };

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error sending email: {ex.Message}";
            }
        }

        // GET: Appointments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserID = new SelectList(db.UserProfiles, "UserID", "UserName", appointment.UserID);
            ViewBag.SaID = new SelectList(db.Patients, "SaID", "PatientName", appointment.SaID);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AppointmentID,UserID,SaID,EmailAddress,AppointmentDate,AppointmentReason,STATUS")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(appointment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserID = new SelectList(db.UserProfiles, "UserID", "UserName", appointment.UserID);
            ViewBag.SaID = new SelectList(db.Patients, "SaID", "PatientName", appointment.SaID);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Appointment appointment = db.Appointments.Find(id);
            db.Appointments.Remove(appointment);
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
    }
}
