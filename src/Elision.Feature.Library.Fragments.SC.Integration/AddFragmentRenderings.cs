using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Elision.Foundation.Kernel;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using Sitecore.Mvc.Pipelines.Response.GetXmlBasedLayoutDefinition;

namespace Elision.Feature.Library.Fragments.SC.Integration
{
    public class AddFragmentRenderings : GetXmlBasedLayoutDefinitionProcessor
    {
        protected Regex FragmentPlaceholderPattern;

        public AddFragmentRenderings()
        {
            FragmentPlaceholderPattern = new Regex(@"^/?" + "fragmentcontainer" + @"(/|$)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        }

        public override void Process(GetXmlBasedLayoutDefinitionArgs args)
        {
            if (args.Result == null) return;

            var pageItem = Sitecore.Mvc.Presentation.PageContext.Current.Item;
            var device = Sitecore.Mvc.Presentation.PageContext.Current.Device;
            if (device == null) return;

            var deviceId = ID.Parse($"{{{device.Id.ToString().ToUpper()}}}");
            args.Result = MergeWithFragmentRenderings(args.Result, deviceId, Sitecore.Mvc.Presentation.PageContext.Current.Database, pageItem);
        }

        protected virtual XElement MergeWithFragmentRenderings(XElement self, ID deviceId, Database db, Item pageItem)
        {
            var selfParsed = LayoutDefinition.Parse(self.ToString());
            var selfDevice = selfParsed.GetDevice(deviceId.ToString());

            if (selfDevice?.Renderings == null)
                return self;

            var existingRenderingsWithDatasource = selfDevice.Renderings
                                               .Cast<RenderingDefinition>()
                                               .Where(x => !string.IsNullOrWhiteSpace(x.Datasource))
                                               .ToArray();

            foreach (var rendering in existingRenderingsWithDatasource)
            {
                var fragment = db.ResolveDatasource(rendering.Datasource, pageItem);
                if (fragment == null || !fragment.InheritsFrom(Templates.Fragment.TemplateId))
                    continue;

                rendering.Parameters += (string.IsNullOrWhiteSpace(rendering.Parameters) ? "" : "&")
                                       + "wasfragment=1&fragmentid=" + fragment.ID;

                var newPlaceholderPath = $"/{rendering.Placeholder.Trim('/')}/fragment-{{{Guid.Parse(rendering.UniqueId)}}}-0/";

                var layout = fragment.GetLayoutDefinition();
                var device = layout.GetDevice(ID.Parse(deviceId).ToString());
                var fragmentRenderings = device
                    .Renderings.Cast<RenderingDefinition>()
                    .Where(r => FragmentPlaceholderPattern.IsMatch(r.Placeholder ?? ""));

                foreach (var renderingDefinition in fragmentRenderings)
                {
                    renderingDefinition.Placeholder = FragmentPlaceholderPattern
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
