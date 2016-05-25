using System.Linq;
using System.Web;
using Sitecore.Shell.Framework.Commands;

namespace Elision.Fragments
{
    public class EditFragmentCommand : Sitecore.Shell.Applications.WebEdit.Commands.WebEditCommand
    {
        public override void Execute(CommandContext context)
        {
            if (context.Items == null || !context.Items.Any())
                return;

            var fragmentId = context.Parameters.Get("fragmentid");
            if (string.IsNullOrWhiteSpace(fragmentId))
                return;

            var language = context.Parameters.Get("lang");

            var editUrl = string.Format("/?sc_mode=edit&sc_itemid={0}&sc_lang={1}",
                HttpUtility.UrlEncode(fragmentId),
                language
                );

            Sitecore.Web.UI.Sheer.SheerResponse.Eval("window.parent.location = '" + editUrl + "'");
        }
    }
}
