using System.Linq;
using System.Web;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;

namespace Elision.Feature.Library.Fragments
{
    public class EditFragmentCommand : WebEditCommand
    {
        public override void Execute(CommandContext context)
        {
            if (context.Items == null || !context.Items.Any())
                return;

            var fragmentId = context.Parameters.Get("fragmentid");
            if (string.IsNullOrWhiteSpace(fragmentId))
                return;

            var language = context.Parameters.Get("lang");

            var editUrl = $"/?sc_mode=edit&sc_itemid={HttpUtility.UrlEncode(fragmentId)}&sc_lang={language}";

            Sitecore.Web.UI.Sheer.SheerResponse.Eval("window.parent.location = '" + editUrl + "'");
        }
    }
}
