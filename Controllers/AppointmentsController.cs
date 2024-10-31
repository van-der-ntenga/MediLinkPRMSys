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
using System.Threading.Tasks;
using PagedList;
using System.Diagnostics;

namespace MediLinkCB.Controllers
{
    public class AppointmentsController : Controller
    {
        private MediLinkDBEntities13 db = new MediLinkDBEntities13();

        // GET: Appointments
        public ActionResult Index(string searchPatientID, int? page)
        {
            int pageSize = 10;  // Set the number of records per page
            int pageNumber = (page ?? 1);  // If no page is specified, default to the first page

            IQueryable<Appointment> appointmentsQuery = db.Appointments.Include(a => a.UserProfile)
                                                                         .Include(a => a.Patient)
                                                                         .OrderBy(a => a.AppointmentDateTime);  // Start with all appointments

            if (!string.IsNullOrEmpty(searchPatientID))
            {
                // Search for appointments using PatientID
                appointmentsQuery = appointmentsQuery.Where(a => a.Patient.PatientID.Contains(searchPatientID));

                if (!appointmentsQuery.Any())
                {
                    // If no appointments found, return an empty list with a message
                    ViewBag.Message = "No appointments found for the provided Patient ID.";
                    return View(new List<Appointment>().ToPagedList(pageNumber, pageSize));
                }
            }

            // Return paginated results
            var appointmentsPaged = appointmentsQuery.ToPagedList(pageNumber, pageSize);

            return View(appointmentsPaged);  // Pass the paginated appointments to the view
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
        public async Task<ActionResult> Create(Appointment appointment)
        {
            // Validate PatientID exists
            var patientExists = db.Patients.Any(p => p.PatientID == appointment.PatientID);
            if (!patientExists)
            {
                ModelState.AddModelError("PatientID", "Patient ID not found. Please provide a valid ID.");
                return View(appointment);  // Return the view with validation error
            }

            if (ModelState.IsValid)
            {
                // Fetch the UserID from session
                if (Session["UserId"] != null)
                {
                    appointment.UserID = int.Parse(Session["UserId"].ToString());
                }
                else
                {
                    return RedirectToAction("Login", "Login");
                }

                // Validate Appointment DateTime
                if (appointment.AppointmentDateTime <= DateTime.Now)
                {
                    ModelState.AddModelError("", "You cannot book an appointment in the past. Please select a future date and time.");
                    return View(appointment);
                }

                if (!IsValidAppointmentTime(appointment.AppointmentDateTime, appointment.AppointmentReason))
                {
                    ModelState.AddModelError("", "Invalid appointment time. Please select a valid time within working hours.");
                    return View(appointment);
                }

                if (!IsTimeSlotAvailable(appointment.AppointmentDateTime, appointment.AppointmentReason))
                {
                    ModelState.AddModelError("", "This time slot is already booked. Please select another time.");
                    return View(appointment);
                }

                db.Appointments.Add(appointment);
                await db.SaveChangesAsync();

                // Send appointment confirmation email
                SendEmailAppointmentConfirmation(appointment.EmailAddress,
                    appointment.AppointmentDateTime?.ToString("yyyy-MM-dd HH:mm"), appointment.AppointmentReason);

                return RedirectToAction("Index");
            }

            return View(appointment);  // In case of other validation errors
        }


        [HttpGet]
        public JsonResult CheckSaID(string saId)
        {
            var exists = db.Patients.Any(p => p.PatientID == saId);
            return Json(new { exists = exists, message = exists ? "Patient found." : "Patient not found." }, JsonRequestBehavior.AllowGet);
        }


        private bool IsTimeSlotValid(Appointment appointment)
        {
            if (!appointment.AppointmentDateTime.HasValue)
                return false;

            var appointmentTime = appointment.AppointmentDateTime.Value.TimeOfDay;

            var specialistServices = new[] { "Optometrist", "Gynaecologist", "Audiologist" };
            if (specialistServices.Contains(appointment.AppointmentReason))
            {
                if (appointment.AppointmentDateTime.Value.DayOfWeek == DayOfWeek.Saturday ||
                    appointment.AppointmentDateTime.Value.DayOfWeek == DayOfWeek.Sunday ||
                    appointmentTime < new TimeSpan(10, 0, 0) || appointmentTime >= new TimeSpan(15, 0, 0))
                {
                    return false;
                }
            }
            else
            {
                if (appointmentTime < new TimeSpan(8, 0, 0) || appointmentTime >= new TimeSpan(16, 0, 0))
                {
                    return false;
                }
            }
            return true;
        }
        private bool IsTimeSlotAvailable(DateTime? appointmentDateTime, string appointmentReason)
        {
            if (!appointmentDateTime.HasValue) return false;

            var appointmentStart = appointmentDateTime.Value;
            var appointmentEnd = appointmentStart.AddMinutes(20); 

            var overlappingAppointments = db.Appointments
                .Where(a => a.AppointmentDateTime.HasValue
                            && a.AppointmentReason == appointmentReason
                            && a.AppointmentDateTime.Value < appointmentEnd
                            && a.AppointmentDateTime.Value >= appointmentStart)
                .ToList();

            return !overlappingAppointments.Any();
        }

        private bool IsValidAppointmentTime(DateTime? appointmentDateTime, string appointmentReason)
        {
            if (!appointmentDateTime.HasValue) return false;

            var appointmentTime = appointmentDateTime.Value.TimeOfDay;
            var isSpecialist = new[] { "Optometrist", "Gynaecologist", "Audiologist" }.Contains(appointmentReason);

            // Ensure that time is in 20-minute increments
            if (appointmentDateTime.Value.Minute % 20 != 0)
            {
                return false;
            }

            if (isSpecialist)
            {
                return appointmentTime >= new TimeSpan(10, 0, 0) && appointmentTime <= new TimeSpan(15, 0, 0);
            }
            else
            {
                return appointmentTime >= new TimeSpan(8, 0, 0) && appointmentTime <= new TimeSpan(16, 0, 0);
            }
        }


        private bool IsWithin20Minutes(DateTime? appointmentDateTime, string appointmentReason)
        {
            if (!appointmentDateTime.HasValue) return false;

            var appointmentStart = appointmentDateTime.Value;
            var appointmentEnd = appointmentStart.AddMinutes(20);

            var conflictingAppointments = db.Appointments
                .Where(a => a.AppointmentDateTime.HasValue
                            && a.AppointmentReason == appointmentReason
                            && a.AppointmentDateTime.Value < appointmentEnd
                            && a.AppointmentDateTime.Value >= appointmentStart)
                .ToList();

            return !conflictingAppointments.Any();
        }


        public void SendEmailAppointmentConfirmation(string email, string appointmentDate, string appointmentReason)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(email);
                mail.From = new MailAddress("noreply.medilinkportal@gmail.com");  // Update to your new Gmail
                mail.Subject = "Your Appointment Confirmation";
                
                mail.Body = $"Your appointment is confirmed for {appointmentDate} regarding {appointmentReason}. Please arrive 10 minutes early.";

                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",   // Use Gmail's SMTP server
                    Port = 587,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("noreply.medilinkportal@gmail.com", "jtjz seqp mtst beva"), // Use Gmail App Password
                    EnableSsl = true
                };

                smtp.Send(mail);
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Error occurred while sending email: {ex.Message}");
                ViewBag.Message = "There was an error sending the email. Please try again.";
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
            ViewBag.PatientID = new SelectList(db.Patients, "PatientID", "PatientName", appointment.PatientID);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AppointmentID,UserID,PatientID,EmailAddress,AppointmentDate,AppointmentReason")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(appointment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserID = new SelectList(db.UserProfiles, "UserID", "UserName", appointment.UserID);
            ViewBag.SaID = new SelectList(db.Patients, "PatientID", "PatientName", appointment.PatientID);
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

        // GET: Appointments/GetBookedSlots
        [HttpGet]
        public JsonResult GetBookedSlots(string appointmentReason)
        {
            DateTime startDate = DateTime.Today;  // Start from today
            DateTime endDate = startDate.AddDays(14);  // Next 14 days

            var bookedAppointments = db.Appointments
                .Where(a => a.AppointmentDateTime >= startDate
                            && a.AppointmentDateTime <= endDate
                            && a.AppointmentReason == appointmentReason)
                .OrderBy(a => a.AppointmentDateTime)
                .ToList()
                .Select(a => new
                {
                    AppointmentDateTime = a.AppointmentDateTime.HasValue
                        ? a.AppointmentDateTime.Value.ToString("yyyy-MM-ddTHH:mm:ss") // Proper date format
                        : string.Empty,
                    AppointmentReason = a.AppointmentReason
                })
                .ToList();

            return Json(new { success = true, bookedSlots = bookedAppointments }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult CheckIn(string searchSaID, int? page)
        {
            int pageSize = 10;  // Number of appointments per page
            int pageNumber = (page ?? 1);  // Default to page 1 if null

            IQueryable<Appointment> appointmentsQuery = db.Appointments.Include(a => a.Patient);

            if (!string.IsNullOrEmpty(searchSaID))
            {
                // Search for the patient using SaID
                appointmentsQuery = appointmentsQuery.Where(a => a.Patient.PatientID.Contains(searchSaID));

                if (!appointmentsQuery.Any())
                {
                    // If no patient found, return an empty list with a message
                    ViewBag.Message = "No patient or appointment found with the provided ID/Passport Number.";
                    return View(new List<Appointment>().ToPagedList(pageNumber, pageSize));
                }
            }

            // Return paginated results
            var appointmentsPaged = appointmentsQuery.OrderBy(a => a.AppointmentDateTime).ToPagedList(pageNumber, pageSize);

            return View(appointmentsPaged);
        }

        public ActionResult CheckInDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var appointment = db.Appointments.Include(a => a.Patient).FirstOrDefault(a => a.AppointmentID == id);

            if (appointment == null)
            {
                return HttpNotFound();
            }

            // Assigning doctors or nurses based on appointment reason
            if (appointment.AppointmentReason == "Optometrist" || appointment.AppointmentReason == "Gynaecologist" || appointment.AppointmentReason == "Audiologist" || appointment.AppointmentReason == "General Practitioner")
            {
                ViewBag.Doctors = db.UserProfiles.Where(u => u.UserType == "Doctor" && u.Specialisation == appointment.AppointmentReason).ToList();
            }
            else if (appointment.AppointmentReason == "Family Planning" || appointment.AppointmentReason == "Routine Check-Up")
            {
                ViewBag.Nurses = db.UserProfiles.Where(u => u.UserType == "Nurse").ToList();
            }

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckInDetails(int id, bool isCheckedIn, int? selectedDoctorId, int? selectedNurseId)
        {
            var appointment = db.Appointments.Include(a => a.Patient).FirstOrDefault(a => a.AppointmentID == id);

            if (appointment != null)
            {
                // Check if no doctor or nurse is selected
                if (!selectedDoctorId.HasValue && !selectedNurseId.HasValue)
                {
                    ModelState.AddModelError("MedicalStaffError", "Please select a Doctor or Nurse.");
                }

                // Check if the 'Check In' checkbox is selected
                if (!isCheckedIn)
                {
                    ModelState.AddModelError("CheckInError", "Please check the 'Check In' box.");
                }

                // If validation fails, return to the view with error messages
                if (!ModelState.IsValid)
                {
                    // Reload the list of doctors and nurses for the dropdowns
                    if (appointment.AppointmentReason == "Optometrist" || appointment.AppointmentReason == "Gynaecologist" || appointment.AppointmentReason == "Audiologist" || appointment.AppointmentReason == "General Practitioner")
                    {
                        ViewBag.Doctors = db.UserProfiles.Where(u => u.UserType == "Doctor" && u.Specialisation == appointment.AppointmentReason).ToList();
                    }
                    else if (appointment.AppointmentReason == "Family Planning" || appointment.AppointmentReason == "Routine Check-Up")
                    {
                        ViewBag.Nurses = db.UserProfiles.Where(u => u.UserType == "Nurse").ToList();
                    }

                    return View(appointment);  // Return the view with error messages
                }

                // Continue with the usual process if validation passes
                appointment.IsCheckedIn = isCheckedIn;
                db.Entry(appointment).State = EntityState.Modified;
                db.SaveChanges();

                // Determine whether a doctor or nurse was selected
                int? userId = selectedDoctorId ?? selectedNurseId;

                // Create Prescription record
                var prescription = new Prescription
                {
                    UserID = userId.Value,  // Doctor or Nurse ID
                    PatientID = appointment.PatientID,
                    AppointmentID = appointment.AppointmentID,
                    PrescriptionDateTime = DateTime.Now,  // Automatically set the current time
                    IsCollected = false
                };
                db.Prescriptions.Add(prescription);
                db.SaveChanges();

                // Create Medical History record with Patient Name and Surname
                MedicalHistory medicalRecord = new MedicalHistory
                {
                    AppointmentID = appointment.AppointmentID,
                    PatientID = appointment.PatientID,
                    UserID = userId.Value,
                    PrescriptionID = prescription.PrescriptionID,  // Link to the created prescription
                    AppointmentReason = appointment.AppointmentReason,
                    AppointmentDateTime = appointment.AppointmentDateTime,
                    PatientName = appointment.Patient.PatientName,
                    PatientSurname = appointment.Patient.PatientSurname,
                    Diagnosis = "",
                    Notes = "",
                    FollowUp = false
                };
                db.MedicalHistories.Add(medicalRecord);
                db.SaveChanges();

                return RedirectToAction("Index", "Appointments");
            }

            return View(appointment);
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
