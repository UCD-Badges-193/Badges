using System;
using System.Linq;
using System.Web.Mvc;
using UCDArch.Core.PersistanceSupport;
using UCDArch.Core.Utils;

namespace Badges.Models.Student
{
    /// <summary>
    /// Controller for the Evidece class
    /// </summary>
    public class EvideceController : ApplicationController
    {
	    private readonly IRepository<Evidece> _evideceRepository;

        public EvideceController(IRepository<Evidece> evideceRepository)
        {
            _evideceRepository = evideceRepository;
        }
    
        //
        // GET: /Evidece/
        public ActionResult Index()
        {
            var evideceList = _evideceRepository.Queryable;

            return View(evideceList.ToList());
        }

    }

	/// <summary>
    /// ViewModel for the Evidece class
    /// </summary>
    public class EvideceViewModel
	{
		public Evidece Evidece { get; set; }
 
		public static EvideceViewModel Create(IRepository repository)
		{
			Check.Require(repository != null, "Repository must be supplied");
			
			var viewModel = new EvideceViewModel {Evidece = new Evidece()};
 
			return viewModel;
		}
	}
}
