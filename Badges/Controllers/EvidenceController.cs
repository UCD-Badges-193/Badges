using Badges.Core.Repositories;
using Badges.Services;
using System;
using System.Linq;
using System.Security.Policy;
using System.Web.Mvc;
using UCDArch.Core.PersistanceSupport;
using UCDArch.Core.Utils;

namespace Badges.Controllers
{
    /// <summary>
    /// Controller for the Evidence class
    /// </summary>
    public class EvidenceController : ApplicationController
    {
	    private readonly IRepository<Evidence> _evidenceRepository;

        //public EvidenceController(IRepository<Evidence> evidenceRepository)
        public EvidenceController(IRepositoryFactory repositoryFactory, IFileService fileService): base(repositoryFactory)
        {
            _evidenceRepository = fileService;
        }
    
        //
        // GET: /Evidence/
        public ActionResult Index()
        {
            var evidenceList = _evidenceRepository.Queryable;

            return View(evidenceList.ToList());
        }

    }

	/// <summary>
    /// ViewModel for the Evidence class
    /// </summary>
    public class EvidenceViewModel
	{
		public Evidence Evidence { get; set; }
 
		public static EvidenceViewModel Create(IRepository repository)
		{
			Check.Require(repository != null, "Repository must be supplied");
			
			var viewModel = new EvidenceViewModel {Evidence = new Evidence()};
 
			return viewModel;
		}
	}
}
