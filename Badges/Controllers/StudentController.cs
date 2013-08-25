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

            return View(experiences);
        }

        public ActionResult AddExperience()
        {
            var model = new ExperienceEditModel
                {
                    User = _userService.GetCurrent(),
                    Experience = new Experience { Start = DateTime.Now },
                    ExperienceTypes = new SelectList(RepositoryFactory.ExperienceTypeRepository.GetAll(), "Id", "Name")
                };

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

            var model = new ExperienceEditModel
                {
                    User = _userService.GetCurrent(),
                    Experience = experience,
                    ExperienceTypes = new SelectList(RepositoryFactory.ExperienceTypeRepository.GetAll(), "Id", "Name")
                };

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

            var model = new ExperienceEditModel
            {
                User = _userService.GetCurrent(),
                Experience = experience,
                ExperienceTypes = new SelectList(RepositoryFactory.ExperienceTypeRepository.GetAll(), "Id", "Name", experience.ExperienceType)
            };

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

            var model = new ExperienceEditModel
            {
                User = _userService.GetCurrent(),
                Experience = experience,
                ExperienceTypes = new SelectList(RepositoryFactory.ExperienceTypeRepository.GetAll(), "Id", "Name")
            };

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

            return View(experience);
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

            var work = new SupportingFile { Experience = experience, Description = description, Name = workFile.FileName, ContentType = workFile.ContentType};

            using (var binaryReader = new BinaryReader(workFile.InputStream))
            {
                work.Content = binaryReader.ReadBytes((int) workFile.InputStream.Length);
            }

            RepositoryFactory.SupportingFileRepository.EnsurePersistent(work);

            return RedirectToAction("ViewExperience", "Student", new {id});
        }
    }
}