﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Badges.Core.Repositories;
using Badges.Core.Domain;
using System;
using Badges.Models.Student;
using Badges.Services;
using UCDArch.Core.PersistanceSupport;

namespace Badges.Controllers
{ 
    [Authorize] //TODO: Implement roles, restrict to student role
    public class StudentController : ApplicationController
    {
        private readonly IUserService _userService;
        //
        // GET: /Student/

        public StudentController(IRepositoryFactory repositoryFactory, IUserService userService) : base(repositoryFactory)
        {
            _userService = userService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Portfolio()
        {
            var experiences =
                RepositoryFactory.ExperienceRepository.Queryable.Where(
                    x => x.Creator.Identifier == CurrentUser.Identity.Name);
            
            ViewBag.Name = _userService.GetCurrent().Profile.DisplayName;

            return View(experiences);
        }

        public ActionResult AddExperience()
        {
            var model = GetEditModel(new Experience {Start = DateTime.Now});

            return View(model);
        }

        [HttpPost]
        public ActionResult AddExperience(Experience experience)
        {
            if (ModelState.IsValid)
            {
                RepositoryFactory.ExperienceRepository.EnsurePersistent(experience);

                Message = "Experience Added!";
                return RedirectToAction("ViewExperience", "Student", new {id = experience.Id});
            }

            var model = GetEditModel(experience);

            return View(model);
        }

        public ActionResult EditExperience(Guid id)
        {
            var experience =
                RepositoryFactory.ExperienceRepository.Queryable.SingleOrDefault(
                    x => x.Id == id && x.Creator.Identifier == CurrentUser.Identity.Name);

            if (experience == null)
            {
                return new HttpNotFoundResult("Could not find the requested experience");
            }

            var model = GetEditModel(experience);

            return View(model);
        }

        [HttpPost]
        public ActionResult EditExperience(Guid id, Experience experience)
        {
            var experienceToEdit =
                RepositoryFactory.ExperienceRepository.Queryable.SingleOrDefault(
                    x => x.Id == id && x.Creator.Identifier == CurrentUser.Identity.Name);

            if (experienceToEdit == null)
            {
                return new HttpNotFoundResult("Could not find the requested experience");
            }

            UpdateModel(experienceToEdit, "Experience");
                
            if (ModelState.IsValid)
            {
                RepositoryFactory.ExperienceRepository.EnsurePersistent(experienceToEdit);

                Message = "Experience Updated!";
                return RedirectToAction("ViewExperience", "Student", new {id});
            }

            var model = GetEditModel(experience);

            return View(model);
        }

        public ActionResult ViewExperience(Guid id)
        {
            var experience =
                RepositoryFactory.ExperienceRepository.Queryable.SingleOrDefault(
                    x => x.Id == id && x.Creator.Identifier == CurrentUser.Identity.Name);
            
            if (experience == null)
            {
                return new HttpNotFoundResult("Could not find the requested experience");
            }

            var model = new ExperienceViewModel
                {
                    Experience = experience,
                    SupportingWorks = experience.SupportingWorks.ToList(),
                    ExperienceOutcomes = experience.ExperienceOutcomes,
                    Instructors = new MultiSelectList(RepositoryFactory.InstructorRepository.GetAll(), "Id", "DisplayName"),
                    Outcomes = new SelectList(RepositoryFactory.OutcomeRepository.GetAll(), "Id", "Name")
                };

            return View(model);
        }

        /// <summary>
        /// Add supporting work to experience given by id
        /// TODO: allow adding links as well as files
        /// TODO: maybe figure out a better place to store files
        /// </summary>
        /// <param name="id"></param>
        /// <param name="description">description of work</param>
        /// <param name="workFile">file given along with work</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddSupportingWork(Guid id, string description, HttpPostedFileBase workFile)
        {
            var experience =
                RepositoryFactory.ExperienceRepository.Queryable.SingleOrDefault(
                    x => x.Id == id && x.Creator.Identifier == CurrentUser.Identity.Name);

            if (experience == null)
            {
                return new HttpNotFoundResult("Could not find the requested experience");
            }

            var work = new SupportingWork { Experience = experience, Description = description, Name = workFile.FileName, ContentType = workFile.ContentType};

            using (var binaryReader = new BinaryReader(workFile.InputStream))
            {
                work.Content = binaryReader.ReadBytes((int) workFile.InputStream.Length);
            }

            experience.AddSupportingWork(work);

            RepositoryFactory.ExperienceRepository.EnsurePersistent(experience);

            return RedirectToAction("ViewExperience", "Student", new {id});
        }

        /// <summary>
        /// Out the outcome with given notes to the experience
        /// </summary>
        /// <param name="id">Experience ID</param>
        /// <param name="outcomeId">Outcome ID</param>
        /// <param name="notes">Notes</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddOutcome(Guid id, Guid outcomeId, string notes)
        {
            var experience =
                RepositoryFactory.ExperienceRepository.Queryable.SingleOrDefault(
                    x => x.Id == id && x.Creator.Identifier == CurrentUser.Identity.Name);

            if (experience == null)
            {
                return new HttpNotFoundResult("Could not find the requested experience");
            }

            experience.AddOutcome(new ExperienceOutcome
                {
                    Outcome = RepositoryFactory.OutcomeRepository.GetById(outcomeId),
                    Notes = notes
                });

            RepositoryFactory.ExperienceRepository.EnsurePersistent(experience);

            return RedirectToAction("ViewExperience", "Student", new {id});
        }

        /// <summary>
        /// Request feedback via email from instructors
        /// </summary>
        /// <param name="id">experienceID</param>
        /// <param name="message">feedback request message</param>
        /// <param name="instructors">instructors to send feedback request to</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RequestFeedback(Guid id, string message, Guid[] instructors)
        {
            var experience =
                RepositoryFactory.ExperienceRepository.Queryable.SingleOrDefault(
                    x => x.Id == id && x.Creator.Identifier == CurrentUser.Identity.Name);

            if (experience == null)
            {
                return new HttpNotFoundResult("Could not find the requested experience");
            }

            var instructorsToNotify =
                RepositoryFactory.InstructorRepository.Queryable.Where(x => instructors.Contains(x.Id)).ToList();

            Message = string.Format("FAKE Notification Sent to {0} with message {1}",
                                    string.Join(", ",
                                                instructorsToNotify.Select(
                                                    x => string.Format("{0} [{1}]", x.DisplayName, x.Email))), message);

            return RedirectToAction("ViewExperience", "Student", new {id});
        }

        /// <summary>
        /// Allows student to view their work, for now just pictures
        /// TODO: allow more than just pictures
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ViewWork(Guid id)
        {
            var work = RepositoryFactory.SupportingWorkRepository.Queryable.SingleOrDefault(
                x => x.Id == id && x.Experience.Creator.Identifier == CurrentUser.Identity.Name);

            if (work == null)
            {
                return new HttpNotFoundResult();
            }

            return File(work.Content, work.ContentType);
        }

        private ExperienceEditModel GetEditModel(Experience experience)
        {
            return new ExperienceEditModel
            {
                User = _userService.GetCurrent(),
                Experience = experience,
                Instructors = new MultiSelectList(RepositoryFactory.InstructorRepository.GetAll(), "Id", "DisplayName"),
                ExperienceTypes = new SelectList(RepositoryFactory.ExperienceTypeRepository.GetAll(), "Id", "Name")
            };
        }
    }
}