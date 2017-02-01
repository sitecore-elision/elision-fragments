using System.Web.Mvc;
using Sitecore.Mvc.Controllers;

namespace Elision.Feature.Library.Fragments.Areas.ElisionFeature.Controllers
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
