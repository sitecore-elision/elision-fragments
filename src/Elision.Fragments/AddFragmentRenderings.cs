using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using Sitecore.Mvc.Pipelines.Response.GetXmlBasedLayoutDefinition;

namespace Elision.Fragments
{
    public class AddFragmentRenderings : GetXmlBasedLayoutDefinitionProcessor
    {
        public override void Process(GetXmlBasedLayoutDefinitionArgs args)
        {
            if (args.Result == null) return;

            var pageItem = Sitecore.Mvc.Presentation.PageContext.Current.Item;
            var device = Sitecore.Mvc.Presentation.PageContext.Current.Device;
            if (device == null) return;

            var deviceId = ID.Parse(string.Format("{{{0}}}", device.Id.ToString().ToUpper()));
            args.Result = MergeWithFragmentRenderings(args.Result, deviceId, Sitecore.Mvc.Presentation.PageContext.Current.Database, pageItem);
        }

        protected virtual XElement MergeWithFragmentRenderings(XElement self, ID deviceId, Database db, Item pageItem)
        {
            var selfParsed = LayoutDefinition.Parse(self.ToString());
            var selfDevice = selfParsed.GetDevice(deviceId.ToString());

            if (selfDevice == null || selfDevice.Renderings == null)
                return self;

            var existingRenderingsWithDatasource = selfDevice.Renderings
                                               .Cast<RenderingDefinition>()
                                               .Where(x => !string.IsNullOrWhiteSpace(x.Datasource))
                                               .ToArray();

            var fragmentPlaceholderPattern = new Regex(@"^/?" + "fragmentcontainer" + @"(/|$)",
                                                       RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            foreach (var rendering in existingRenderingsWithDatasource)
            {
                var fragment = db.ResolveDatasource(rendering.Datasource, pageItem);
                if (fragment == null || !fragment.InheritsFrom(Templates.Fragment.TemplateId))
                    continue;

                rendering.Parameters += (string.IsNullOrWhiteSpace(rendering.Parameters) ? "" : "&")
                                       + "wasfragment=1&fragmentid=" + fragment.ID;

                var newPlaceholderPath = "/" + rendering.Placeholder.Trim('/')
                                         + "/fragment_" + Guid.Parse(rendering.UniqueId) + "/";

                var layout = fragment.GetLayoutDefinition();
                var device = layout.GetDevice(ID.Parse(deviceId).ToString());
                var fragmentRenderings = device
                    .Renderings.Cast<RenderingDefinition>()
                    .Where(r => fragmentPlaceholderPattern.IsMatch(r.Placeholder ?? ""));

                foreach (var renderingDefinition in fragmentRenderings)
                {
                    renderingDefinition.Placeholder = fragmentPlaceholderPattern
                        .Replace(renderingDefinition.Placeholder, newPlaceholderPath)
                        .TrimEnd('/');

                    renderingDefinition.Parameters += (string.IsNullOrWhiteSpace(rendering.Parameters) ? "" : "&")
                                           + "disablewebedit=1&wasfragment=1&fragmentid=" + fragment.ID;

                    selfDevice.AddRendering(renderingDefinition);
                }
            }
            return XDocument.Parse(selfParsed.ToXml()).Root;
        }
    }
}
