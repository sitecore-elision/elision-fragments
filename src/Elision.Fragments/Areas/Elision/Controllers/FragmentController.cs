using System.Web.Mvc;
using Sitecore.Mvc.Controllers;

namespace Elision.Fragments.Areas.Elision.Controllers
{
    public class FragmentController : SitecoreController
    {
        public ActionResult Fragment()
        {
            return View();
        }

        public ActionResult FragmentContainer()
        {
            return View();
        }
    }
}
