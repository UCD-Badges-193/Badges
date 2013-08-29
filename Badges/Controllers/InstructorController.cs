﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Badges.Core.Repositories;
using Badges.Models.Shared;
using NHibernate.Linq;

namespace Badges.Controllers
{
    [Authorize]
    public class InstructorController : ApplicationController
    {
        //
        // GET: /Instructor/
        public InstructorController(IRepositoryFactory repositoryFactory) : base(repositoryFactory)
        {
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MyStudents()
        {
            var experiences =
                RepositoryFactory.ExperienceRepository.Queryable.Where(
                    x => x.Instructors.Any(i => i.Identifier == CurrentUser.Identity.Name)).ToList();

            return View(experiences);
        }

        /// <summary>
        /// Show your pending notifications
        /// </summary>
        /// <returns></returns>
        public ActionResult Notifications()
        {
            var myNotifications =
                RepositoryFactory.FeedbackRequestRepository.Queryable.Where(
                    x => x.Instructor.Identifier == CurrentUser.Identity.Name)
                                 .Fetch(x => x.Experience)
                                 .ThenFetch(x=>x.Creator)
                                 .ToList();

            return View(myNotifications);
        }

        /// <summary>
        /// View an experience as an instructor
        /// NOTE: Instructor must have acccess to the experience AND the experience must have instructorViewable = true
        /// </summary>
        /// <param name="id">Experience Id</param>
        /// <returns></returns>
        public ActionResult ViewExperience(Guid id)
        {
            var experience =
                RepositoryFactory.ExperienceRepository.Queryable.SingleOrDefault(x => x.Id == id &&
                                                                                      x.Instructors.Any(i => i.Identifier == CurrentUser.Identity.Name));

            if (experience == null)
            {
                return new HttpNotFoundResult();
            }
            if (experience.InstructorViewable == false)
            {
                return new HttpUnauthorizedResult();
            }

            var model = new ExperienceViewModel
                {
                    Experience = experience,
                    SupportingWorks = experience.SupportingWorks.ToList(),
                    ExperienceOutcomes = experience.ExperienceOutcomes.ToList()
                };

            return View(model);
        }

        /// <summary>
        /// Allows instructor to view a student's work
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ViewWork(Guid id)
        {
            var work = RepositoryFactory.SupportingWorkRepository.Queryable.SingleOrDefault(
                x => x.Id == id && x.Experience.Instructors.Any(i => i.Identifier == CurrentUser.Identity.Name));
            
            if (work == null)
            {
                return new HttpNotFoundResult();
            }

            if (work.Experience.InstructorViewable == false)
            {
                return new HttpUnauthorizedResult();
            }

            return File(work.Content, work.ContentType);
        }
    }
}
