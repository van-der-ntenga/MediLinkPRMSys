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

namespace MediLinkCB.Controllers
{
    
    public class PatientsController : Controller
    {
        private MediLinkDBEntities7 db = new MediLinkDBEntities7();

        // GET: Patients
        public ActionResult Index(string searchSaID)
        {
            if (!string.IsNullOrEmpty(searchSaID))
            {
                // Search for the patient using SaID
                var patient = db.Patients.FirstOrDefault(p => p.SaID == searchSaID);

                if (patient != null)
                {
                    // If a patient is found, return it in a list
                    return View(new List<Patient> { patient });
                }

                // If no patient found, return an empty list with a message
                ViewBag.Message = "No patient found with the provided SaID.";
                return View(new List<Patient>());
            }

            // If no search criteria, return all patients
            var patients = db.Patients.ToList();
            return View(patients);
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
            ResidentialAddress address = db.ResidentialAddresses.FirstOrDefault(a => a.SaID == id);
            NextOfKin nextOfKin = db.NextOfKins.FirstOrDefault(n => n.SaID == id);

            if (patient == null || address == null || nextOfKin == null)
            {
                return HttpNotFound();
            }

            // Create ViewModel and populate with data
            var viewModel = new PatientResidentialAddressNOKViewModel
            {
                // Patient details
                SaID = patient.SaID,
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
        public ActionResult Create(PatientResidentialAddressNOKViewModel m)
        {
            if (ModelState.IsValid)
            {
                // Table 1: Patient Details
                var patientDetails = new Patient
                {
                    SaID = m.SaID,
                    PatientName = m.PatientName,
                    PatientSurname = m.PatientSurname,
                    DateOfBirth = m.DateOfBirth,
                    Age = m.Age,
                    PatientHeight = m.PatientHeight,
                    PatientWeight = m.PatientWeight,
                    Disability = m.Disability,
                    Gender = m.Gender,
                    Race = m.Race,
                    HomeLang = m.HomeLang,
                    MaritalStatus = m.MaritalStatus,
                    EmailAddress = m.EmailAddress,
                    CellNumber=m.CellNumber
                };

                // Table 2: Contact Information
                var address = new ResidentialAddress
                {
                    SaID = m.SaID,
                    StreetAddress = m.StreetAddress,
                    Suburb = m.Suburb,
                    City = m.City,
                    PostalCode = m.PostalCode, 
                    Province = m.Province
                };

                // Table 3: Medical Information
                var nextOfKin = new NextOfKin
                {
                    SaID = m.SaID,
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
            ResidentialAddress address = db.ResidentialAddresses.FirstOrDefault(a => a.SaID == id);
            NextOfKin nextOfKin = db.NextOfKins.FirstOrDefault(n => n.SaID == id);

            if (patient == null || address == null || nextOfKin == null)
            {
                return HttpNotFound();
            }

            // Create ViewModel and populate with data
            var viewModel = new PatientResidentialAddressNOKViewModel
            {
                // Patient details
                SaID = patient.SaID,
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
                var patient = db.Patients.Find(m.SaID);
                var address = db.ResidentialAddresses.FirstOrDefault(a => a.SaID == m.SaID);
                var nextOfKin = db.NextOfKins.FirstOrDefault(n => n.SaID == m.SaID);

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
            ResidentialAddress address = db.ResidentialAddresses.FirstOrDefault(a => a.SaID == id);
            NextOfKin nextOfKin = db.NextOfKins.FirstOrDefault(n => n.SaID == id);

            if (patient == null || address == null || nextOfKin == null)
            {
                return HttpNotFound();
            }

            // Create ViewModel and populate with data for display
            var viewModel = new PatientResidentialAddressNOKViewModel
            {
                SaID = patient.SaID,
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
            // Fetch data from all three tables
            Patient patient = db.Patients.Find(id);
            ResidentialAddress address = db.ResidentialAddresses.FirstOrDefault(a => a.SaID == id);
            NextOfKin nextOfKin = db.NextOfKins.FirstOrDefault(n => n.SaID == id);

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
