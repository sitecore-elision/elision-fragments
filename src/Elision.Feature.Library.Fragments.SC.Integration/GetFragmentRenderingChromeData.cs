using System;
using System.Linq;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.GetChromeData;

namespace Elision.Feature.Library.Fragments.SC.Integration
{
    public class GetFragmentRenderingChromeData : GetChromeDataProcessor
    {
        public override void Process(GetChromeDataArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.IsNotNull(args.ChromeData, "Chrome Data");
            if (!"rendering".Equals(args.ChromeType, StringComparison.OrdinalIgnoreCase))
                return;

            if (args.Item == null)
                return;

            var renderingReference = args.CustomData["renderingReference"] as Sitecore.Layouts.RenderingReference;
            if (renderingReference?.Settings == null)
                return;

            var fragmentId = StringUtil.ExtractParameter("fragmentid", renderingReference.Settings.Parameters);
            var wasfragment = "1" == StringUtil.ExtractParameter("wasfragment", renderingReference.Settings.Parameters);
            
            var editFragmentCommand = args.ChromeData.Commands
                                          .FirstOrDefault(x => x.Click.Contains("elision:feature:fragments()"));
            if (editFragmentCommand != null)
            {
                if (string.IsNullOrWhiteSpace(fragmentId) || !wasfragment)
                {
                    args.ChromeData.Commands.Remove(editFragmentCommand);
                }
                else
                {
                    editFragmentCommand.Click = editFragmentCommand
                        .Click.Replace("elision:feature:fragments()",
                            $"elision:feature:fragments(fragmentid={fragmentId})");
                }
            }
        }
    }
}
