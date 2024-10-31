using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MediLinkCB.Models;
using MediLinkCB.Attributes;
using System.Configuration;
using MediLinkCB.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PagedList;

namespace MediLinkCB.Controllers
{
    
    public class PatientsController : Controller
    {
        private MediLinkDBEntities13 db = new MediLinkDBEntities13();

        // GET: Patients
        public ActionResult Index(string searchSaID, int? page)
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


        // GET: Patients/Details/5
        public ActionResult Details(string id)
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
                Province=address.Province,

                // Next of Kin details
                NOKFirstName = nextOfKin.NOKFirstName,
                NOKLastName = nextOfKin.NOKLastName,
                Relationship = nextOfKin.Relationship,
                NOKCellNumber = nextOfKin.NOKCellNumber
            };

            return View(viewModel);
        }



        // GET: Patients/Create
        public ActionResult Create()
        {
            
            return View();
        }

        // POST: Patients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PatientResidentialAddressNOKViewModel m, string hiddenDisability)
        {
            if (ModelState.IsValid)
            {
                // Table 1: Patient Details
                var patientDetails = new Patient
                {
                    PatientID = m.PatientID,
                    PatientName = m.PatientName,
                    PatientSurname = m.PatientSurname,
                    DateOfBirth = m.DateOfBirth,
                    Age = m.Age,
                    PatientHeight = m.PatientHeight,
                    PatientWeight = m.PatientWeight,
                    Disability = hiddenDisability == "NONE" ? "NONE" : m.Disability,
                    Gender = m.Gender,
                    Race = m.Race,
                    HomeLang = m.HomeLang,
                    MaritalStatus = m.MaritalStatus,
                    EmailAddress = m.EmailAddress,
                    CellNumber=m.CellNumber,
                    Nationality = m.Nationality
                };

                // Table 2: Contact Information
                var address = new ResidentialAddress
                {
                    PatientID = m.PatientID,
                    StreetAddress = m.StreetAddress,
                    Suburb = m.Suburb,
                    City = m.City,
                    PostalCode = m.PostalCode, 
                    Province = m.Province
                };

                // Table 3: Medical Information
                var nextOfKin = new NextOfKin
                {
                    PatientID = m.PatientID,
                    NOKFirstName = m.NOKFirstName,
                    NOKLastName = m.NOKLastName,
                    Relationship = m.Relationship,
                    NOKCellNumber = m.NOKCellNumber
                    
                };

                // Save to the database
                db.Patients.Add(patientDetails);
                db.ResidentialAddresses.Add(address);
                db.NextOfKins.Add(nextOfKin);

                db.SaveChanges();

                // Redirect to a confirmation page or back to the form
                return RedirectToAction("Index");
            }

            // If we got this far, something failed; redisplay form
            return View(m);
        }



        // GET: Patients/Edit/5
        public ActionResult Edit(string id)
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


        // POST: Patients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PatientResidentialAddressNOKViewModel m)
        {
            if (ModelState.IsValid)
            {
                // Fetch existing records from database
                var patient = db.Patients.Find(m.PatientID);
                var address = db.ResidentialAddresses.FirstOrDefault(a => a.PatientID == m.PatientID);
                var nextOfKin = db.NextOfKins.FirstOrDefault(n => n.PatientID == m.PatientID);

                if (patient != null && address != null && nextOfKin != null)
                {
                    // Update Patient details
                    patient.PatientName = m.PatientName;
                    patient.PatientSurname = m.PatientSurname;
                    patient.DateOfBirth = m.DateOfBirth;
                    patient.Age = m.Age;
                    patient.PatientHeight = m.PatientHeight;
                    patient.PatientWeight = m.PatientWeight;
                    patient.Disability = m.Disability;
                    patient.Gender = m.Gender;
                    patient.Race = m.Race;
                    patient.HomeLang = m.HomeLang;
                    patient.MaritalStatus = m.MaritalStatus;
                    patient.EmailAddress = m.EmailAddress;
                    patient.CellNumber = m.CellNumber;
                    patient.Nationality = m.Nationality;

                    // Update Residential Address details
                    address.StreetAddress = m.StreetAddress;
                    address.Suburb = m.Suburb;
                    address.City = m.City;
                    address.PostalCode = m.PostalCode;
                    address.Province = m.Province;

                    // Update Next of Kin details
                    nextOfKin.NOKFirstName = m.NOKFirstName;
                    nextOfKin.NOKLastName = m.NOKLastName;
                    nextOfKin.Relationship = m.Relationship;
                    nextOfKin.NOKCellNumber = m.NOKCellNumber;

                    // Save changes to the database
                    db.Entry(patient).State = EntityState.Modified;
                    db.Entry(address).State = EntityState.Modified;
                    db.Entry(nextOfKin).State = EntityState.Modified;

                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            return View(m);
        }


        // GET: Patients/Delete/5
        public ActionResult Delete(string id)
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

            // Create ViewModel and populate with data for display
            var viewModel = new PatientResidentialAddressNOKViewModel
            {
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
                StreetAddress = address.StreetAddress,
                Suburb = address.Suburb,
                City = address.City,
                PostalCode = address.PostalCode,
                Province = address.Province,
                NOKFirstName = nextOfKin.NOKFirstName,
                NOKLastName = nextOfKin.NOKLastName,
                Relationship = nextOfKin.Relationship,
                NOKCellNumber = nextOfKin.NOKCellNumber
            };

            return View(viewModel);
        }


        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            // Find the patient by SaID
            Patient patient = db.Patients.Find(id);
            ResidentialAddress address = db.ResidentialAddresses.FirstOrDefault(a => a.PatientID == id);
            NextOfKin nextOfKin = db.NextOfKins.FirstOrDefault(n => n.PatientID == id);

            // First, remove all related records in the Prescription table
            var prescriptions = db.Prescriptions.Where(p => p.PatientID == id).ToList();
            if (prescriptions != null)
            {
                foreach (var prescription in prescriptions)
                {
                    db.Prescriptions.Remove(prescription);
                }
            }

            // Remove the patient and related data
            if (patient != null)
            {
                db.Patients.Remove(patient);
            }
            if (address != null)
            {
                db.ResidentialAddresses.Remove(address);
            }
            if (nextOfKin != null)
            {
                db.NextOfKins.Remove(nextOfKin);
            }

            // Save changes to the database
            db.SaveChanges();

            return RedirectToAction("Index");
        }


        public ActionResult PatientSummary()
        {
            var model = new PatientSummaryViewModel
            {
                // Gender counts
                MaleCount = db.Patients.Count(p => p.Gender == "Male"),
                FemaleCount = db.Patients.Count(p => p.Gender == "Female"),
                OtherGenderCount = db.Patients.Count(p => p.Gender == "Other"),

                // Race counts
                BlackCount = db.Patients.Count(p => p.Race == "Black"),
                WhiteCount = db.Patients.Count(p => p.Race == "White"),
                AsianCount = db.Patients.Count(p => p.Race == "Asian"),
                ColouredCount = db.Patients.Count(p => p.Race == "Coloured"),

                // Age group counts
                AgeGroup0To12 = db.Patients.Count(p => p.Age >= 0 && p.Age <= 12),
                AgeGroup13To19 = db.Patients.Count(p => p.Age >= 13 && p.Age <= 19),
                AgeGroup20To35 = db.Patients.Count(p => p.Age >= 20 && p.Age <= 35),
                AgeGroup36To60 = db.Patients.Count(p => p.Age >= 36 && p.Age <= 60),
                AgeGroup60Plus = db.Patients.Count(p => p.Age > 60),

                // Marital status counts
                SingleCount = db.Patients.Count(p => p.MaritalStatus == "Single"),
                MarriedCount = db.Patients.Count(p => p.MaritalStatus == "Married"),
                WidowedCount = db.Patients.Count(p => p.MaritalStatus == "Widowed"),

                // Total patients
                TotalPatientCount = db.Patients.Count()
            };

            return View(model);
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
