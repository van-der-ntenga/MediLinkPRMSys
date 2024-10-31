using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MediLinkCB.Models;
using PagedList;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace MediLinkCB.Controllers
{
    public class MedicalHistoriesController : Controller
    {
        private MediLinkDBEntities13 db = new MediLinkDBEntities13();

        // GET: MedicalHistories
        public ActionResult Index(string searchPatientID, int? page)
        {
            int pageSize = 10; // Number of records per page
            int pageNumber = (page ?? 1); // Default to page 1 if null

            IQueryable<MedicalHistory> medicalHistoryQuery = db.MedicalHistories.Include(m => m.Patient).Include(m => m.Appointment);

            if (!string.IsNullOrEmpty(searchPatientID))
            {
                // Search for medical history by patient ID
                medicalHistoryQuery = medicalHistoryQuery.Where(m => m.PatientID.Contains(searchPatientID));

                if (!medicalHistoryQuery.Any())
                {
                    ViewBag.Message = "No medical history found for the provided Patient ID.";
                    return View(new List<MedicalHistory>().ToPagedList(pageNumber, pageSize));
                }
            }

            var medicalHistoriesPaged = medicalHistoryQuery.OrderBy(m => m.AppointmentDateTime).ToPagedList(pageNumber, pageSize);

            return View(medicalHistoriesPaged);
        }

        // GET: MedicalHistories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Include related Prescription and Medications
            var medicalHistory = db.MedicalHistories
                                   .Include(m => m.Prescription)
                                   .Include(m => m.Prescription.Medications)
                                   .FirstOrDefault(m => m.MedicalHistoryID == id);

            if (medicalHistory == null)
            {
                return HttpNotFound();
            }

            return View(medicalHistory);
        }


        // GET: MedicalHistories/Create
        public ActionResult Create()
        {
            ViewBag.AppointmentID = new SelectList(db.Appointments, "AppointmentID", "PatientID");
            ViewBag.PatientID = new SelectList(db.Patients, "PatientID", "PatientName");
            ViewBag.PrescriptionID = new SelectList(db.Prescriptions, "PrescriptionID", "PatientID");
            ViewBag.UserID = new SelectList(db.UserProfiles, "UserID", "UserName");
            return View();
        }

        // POST: MedicalHistories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MedicalHistoryID,UserID,PatientID,AppointmentID,PrescriptionID,PatientName,PatientSurname,AppointmentReason,Diagnosis,Notes,AppointmentDateTime,FollowUp")] MedicalHistory medicalHistory)
        {
            if (ModelState.IsValid)
            {
                db.MedicalHistories.Add(medicalHistory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AppointmentID = new SelectList(db.Appointments, "AppointmentID", "PatientID", medicalHistory.AppointmentID);
            ViewBag.PatientID = new SelectList(db.Patients, "PatientID", "PatientName", medicalHistory.PatientID);
            ViewBag.PrescriptionID = new SelectList(db.Prescriptions, "PrescriptionID", "PatientID", medicalHistory.PrescriptionID);
            ViewBag.UserID = new SelectList(db.UserProfiles, "UserID", "UserName", medicalHistory.UserID);
            return View(medicalHistory);
        }

        // GET: MedicalHistories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MedicalHistory medicalHistory = db.MedicalHistories.Find(id);
            if (medicalHistory == null)
            {
                return HttpNotFound();
            }
            ViewBag.AppointmentID = new SelectList(db.Appointments, "AppointmentID", "PatientID", medicalHistory.AppointmentID);
            ViewBag.PatientID = new SelectList(db.Patients, "PatientID", "PatientName", medicalHistory.PatientID);
            ViewBag.PrescriptionID = new SelectList(db.Prescriptions, "PrescriptionID", "PatientID", medicalHistory.PrescriptionID);
            ViewBag.UserID = new SelectList(db.UserProfiles, "UserID", "UserName", medicalHistory.UserID);
            return View(medicalHistory);
        }

        // POST: MedicalHistories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MedicalHistoryID,UserID,PatientID,AppointmentID,PrescriptionID,PatientName,PatientSurname,AppointmentReason,Diagnosis,Notes,AppointmentDateTime,FollowUp")] MedicalHistory medicalHistory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(medicalHistory).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AppointmentID = new SelectList(db.Appointments, "AppointmentID", "PatientID", medicalHistory.AppointmentID);
            ViewBag.PatientID = new SelectList(db.Patients, "PatientID", "PatientName", medicalHistory.PatientID);
            ViewBag.PrescriptionID = new SelectList(db.Prescriptions, "PrescriptionID", "PatientID", medicalHistory.PrescriptionID);
            ViewBag.UserID = new SelectList(db.UserProfiles, "UserID", "UserName", medicalHistory.UserID);
            return View(medicalHistory);
        }

        // GET: MedicalHistories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MedicalHistory medicalHistory = db.MedicalHistories.Find(id);
            if (medicalHistory == null)
            {
                return HttpNotFound();
            }
            return View(medicalHistory);
        }

        // POST: MedicalHistories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MedicalHistory medicalHistory = db.MedicalHistories.Find(id);
            db.MedicalHistories.Remove(medicalHistory);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult EditMedicalRecord(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var medicalHistory = db.MedicalHistories
                       .Include(m => m.Prescription)
                       .Include(m => m.Prescription.Medications)  // Ensure the Medications are loaded
                       .FirstOrDefault(m => m.MedicalHistoryID == id);


            if (medicalHistory == null)
            {
                return HttpNotFound();
            }

            // Return the view with the fetched medical history data
            return View(medicalHistory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMedicalRecord(MedicalHistory medicalHistory, List<string> medicationNames)
        {
            
            Debug.WriteLine("Entering EditMedicalRecord POST method");
            Debug.WriteLine("MedicalHistoryID: " + medicalHistory.MedicalHistoryID);// Log entry point

            if (ModelState.IsValid)
            {
                Debug.WriteLine("ModelState is valid");  // Log model validation success

                var existingMedicalHistory = db.MedicalHistories.Find(medicalHistory.MedicalHistoryID);
                if (existingMedicalHistory == null)
                {
                    Debug.WriteLine("MedicalHistory not found in database"); // Log missing record
                    ModelState.AddModelError("", "The medical record does not exist anymore.");
                    return View(medicalHistory);
                }
                //why not add else to continue processing --

                // Log before updating the medical record
                Debug.WriteLine($"Updating MedicalHistory ID: {existingMedicalHistory.MedicalHistoryID}");

                existingMedicalHistory.Diagnosis = medicalHistory.Diagnosis;
                existingMedicalHistory.Notes = medicalHistory.Notes;
                existingMedicalHistory.FollowUp = medicalHistory.FollowUp;

                // Save updated medical history
                db.Entry(existingMedicalHistory).State = EntityState.Modified;

                Debug.WriteLine("Attempting to save MedicalHistory changes to the database"); // Log before SaveChanges()
                db.SaveChanges();  // Check if this line executes
                Debug.WriteLine("MedicalHistory changes saved successfully"); // Log success after SaveChanges()

                // Add medications to the Medications table (if any)
                foreach (var medication in medicationNames)
                {
                    if (!string.IsNullOrEmpty(medication))
                    {
                        var med = new Medication
                        {
                            PrescriptionID = existingMedicalHistory.PrescriptionID,  // Link to the prescription
                            MedicationName = medication
                        };
                        db.Medications.Add(med);
                    }
                }

                Debug.WriteLine("Attempting to save Medications to the database"); // Log before medication SaveChanges()
                db.SaveChanges();  // Save medications
                Debug.WriteLine("Medications saved successfully"); // Log success after SaveChanges()

                return RedirectToAction("MyAppointments");
            }
            else
            {
                Debug.WriteLine("ModelState is not valid");  // Log model validation failure
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Debug.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
            }

            return View(medicalHistory);
        }




        public ActionResult SearchPatients(string searchSaID)
        {
            var patients = db.Patients.Where(p => p.PatientID.Contains(searchSaID)).ToList();

            if (patients == null || !patients.Any())
            {
                ViewBag.Message = "No patient found.";
                return View(new List<Patient>());
            }

            return View(patients);
        }

        public ActionResult MyAppointments()
        {
            if (Session["UserId"] == null)
            {
                // If the session is null, redirect to login
                return RedirectToAction("Login", "Login");
            }

            // Get the current logged-in user's ID
            int userId = int.Parse(Session["UserId"].ToString());

            // Retrieve medical histories where the user (doctor/nurse) is associated with the appointment
            var medicalHistories = db.MedicalHistories
                             .Include(m => m.Patient)
                             .Include(m => m.Appointment)
                             .Where(m => m.UserID == userId)
                             .ToList();

            // We will return the associated appointments, as they are tied to the medical histories
            var appointments = medicalHistories.Select(m => m.Appointment).Distinct().ToList();

            return View(medicalHistories);
        }


        public ActionResult SearchMedicalRecords(string searchPatientID)
        {
            var records = db.MedicalHistories.Include(m => m.Patient).Include(m => m.Prescription).AsQueryable();

            if (!string.IsNullOrEmpty(searchPatientID))
            {
                records = records.Where(r => r.PatientID.Contains(searchPatientID));
            }

            // Check if records were found
            if (!records.Any())
            {
                ViewBag.Message = "No medical records found for the provided Patient ID.";
            }

            return View(records.ToList());
        }


        public ActionResult IndexPatients(string searchSaID, int? page)
        {
            int pageSize = 10; // Number of patients per page
            int pageNumber = (page ?? 1); // Default to page 1 if null

            IQueryable<Patient> patientsQuery = db.Patients;

            if (!string.IsNullOrEmpty(searchSaID))
            {
                // Search for the patient using SaID
                patientsQuery = patientsQuery.Where(p => p.PatientID.Contains(searchSaID));

                if (!patientsQuery.Any())
                {
                    // If no patient found, return an empty list with a message
                    ViewBag.Message = "No patient found with the provided ID/Passport Number.";
                    return View(new List<Patient>().ToPagedList(pageNumber, pageSize));
                }
            }

            // Return paginated results
            var patientsPaged = patientsQuery.OrderBy(p => p.PatientSurname).ToPagedList(pageNumber, pageSize);

            return View(patientsPaged);
        }

        public ActionResult DetailsPatients(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Fetch data from all three tables
            Patient patient = db.Patients.Find(id);
            ResidentialAddress address = db.ResidentialAddresses.FirstOrDefault(a => a.PatientID == id);
            NextOfKin nextOfKin = db.NextOfKins.FirstOrDefault(n => n.PatientID == id);

            if (patient == null || address == null || nextOfKin == null)
            {
                return HttpNotFound();
            }

            // Create ViewModel and populate with data
            var viewModel = new PatientResidentialAddressNOKViewModel
            {
                // Patient details
                PatientID = patient.PatientID,
                PatientName = patient.PatientName,
                PatientSurname = patient.PatientSurname,
                DateOfBirth = patient.DateOfBirth,
                Age = patient.Age,
                PatientHeight = patient.PatientHeight,
                PatientWeight = patient.PatientWeight,
                Disability = patient.Disability,
                Gender = patient.Gender,
                Race = patient.Race,
                HomeLang = patient.HomeLang,
                MaritalStatus = patient.MaritalStatus,
                EmailAddress = patient.EmailAddress,
                CellNumber = patient.CellNumber,
                Nationality = patient.Nationality,

                // Residential Address details
                StreetAddress = address.StreetAddress,
                Suburb = address.Suburb,
                City = address.City,
                PostalCode = address.PostalCode,
                Province = address.Province,

                // Next of Kin details
                NOKFirstName = nextOfKin.NOKFirstName,
                NOKLastName = nextOfKin.NOKLastName,
                Relationship = nextOfKin.Relationship,
                NOKCellNumber = nextOfKin.NOKCellNumber
            };

            return View(viewModel);
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
