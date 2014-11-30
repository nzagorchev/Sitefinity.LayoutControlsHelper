using System;
using System.IO;
using System.Linq;
using System.Text;
using Telerik.Sitefinity.Abstractions.VirtualPath;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Modules.Pages;
using Telerik.Sitefinity.Pages.Model;
using Telerik.Sitefinity.Utilities.HtmlParsing;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.Web.Configuration;
using Telerik.Sitefinity.Web.UI;

namespace SitefinityWebApp.LayoutControls
{
    public class LayoutControlsHelper
    {
        internal virtual TemplateDraftControl GetLayoutControlFromPage(PageManager manager, string pageTitle, string @class)
        {
            var pageNode = manager.GetPageNodes().Where(n => n.Title == pageTitle).FirstOrDefault();
            var pageData = pageNode.GetPageData();
            var template = pageData.Template;

            var masterTemplate = manager.TemplatesLifecycle.GetMaster(template);
            var draftTemplate = manager.TemplatesLifecycle.CheckOut(masterTemplate);

            return GetLayoutControl(manager, @class, draftTemplate);
        }

        internal virtual TemplateDraftControl GetLayoutControlFromTemplate(PageManager manager, string templateTitle, string @class)
        {
            var template = manager.GetTemplates()
                .Where(t => t.Title == templateTitle && t.Status == ContentLifecycleStatus.Live)
                .FirstOrDefault();

            var masterTemplate = manager.TemplatesLifecycle.GetMaster(template);
            var draftTemplate = manager.TemplatesLifecycle.CheckOut(masterTemplate);

            return GetLayoutControl(manager, @class, draftTemplate);
        }

        private static TemplateDraftControl GetLayoutControl(PageManager manager, string @class, TemplateDraft draftTemplate)
        {
            var layoutControls = draftTemplate.Controls;
            foreach (var control in layoutControls)
            {
                var ctrlData = manager.GetControl<TemplateDraftControl>(control.Id);
                var layProp = ctrlData.PropertiesLang.Single(p => p.Name == "Layout");
                var layout = layProp.Value;
                if (!layout.StartsWith("~/") && !layout.EndsWith(".ascx", StringComparison.OrdinalIgnoreCase))
                {
                    var found = TryFindControl(ctrlData, layout, @class);
                    if (found)
                    {
                        return ctrlData;
                    }
                }
                else
                {
                    layout = GetDefaultLayoutControlsHtml(ctrlData, layout);
                    var found = TryFindControl(ctrlData, layout, @class);
                    if (found)
                    {
                        return ctrlData;
                    }
                }
            }

            return new TemplateDraftControl();
        }

        private static bool TryFindControl(TemplateDraftControl ctrlData, string layout, string @class)
        {
            HtmlChunk chunk = null;
            StringBuilder sBuilder = new StringBuilder(layout.Length);
            HtmlParser parser = new HtmlParser(layout);
            parser.SetChunkHashMode(false);
            parser.AutoExtractBetweenTagsOnly = false;
            parser.KeepRawHTML = true;
            var tagToSkip = String.Empty;
            int count = 0;
            while ((chunk = parser.ParseNext()) != null)
            {
                Telerik.Sitefinity.Web.Utilities.LinkParser.ResolveResult resolveResult =
                    Telerik.Sitefinity.Web.Utilities.LinkParser.ResolveResult.Default;

                if (chunk.Type == HtmlChunkType.OpenTag)
                {
                    for (int i = 0; i < chunk.ParamsCount; i++)
                    {
                        string param = chunk.Attributes[i];
                    }

                    var index = chunk.Attributes.ToList().FindIndex(c => c == "class");
                    if (index != -1)
                    {
                        if (chunk.Values[index].Contains(@class))
                        {
                            return true;
                        }
                    }
                }
                if (resolveResult.SkipWholeTag && count == 0)
                {
                    tagToSkip = chunk.TagName;
                }

                if (chunk.Type == HtmlChunkType.OpenTag && tagToSkip == chunk.TagName)
                    count++;

                if ((chunk.Type == HtmlChunkType.CloseTag || chunk.IsEndClosure) && tagToSkip == chunk.TagName)
                    count--;
            }

            return false;
        }

        internal virtual void ChangeControlHtml(PageManager manager, TemplateDraftControl ctrlData, string layout)
        {
            var layProp = ctrlData.PropertiesLang.Single(p => p.Name == "Layout");
            layProp.Value = layout;
            manager.SaveChanges();
        }

        internal virtual string GetControlHtml(TemplateDraftControl ctrlData)
        {
            var layProp = ctrlData.PropertiesLang.Single(p => p.Name == "Layout");
            var layout = layProp.Value;
            if (!layout.StartsWith("~/") && !layout.EndsWith(".ascx", StringComparison.OrdinalIgnoreCase))
            {
                return layout;
            }
            else
            {
                layout = GetDefaultLayoutControlsHtml(ctrlData, layout);
                return layout;
            }
        }

        internal static string GetDefaultLayoutControlsHtml(TemplateDraftControl ctrlData, string layout)
        {
            // this is for getting default layout controls html
            if (layout.StartsWith("~/"))
            {
                using (Stream templateStream = SitefinityFile.Open(layout))
                using (StreamReader reader = new StreamReader(templateStream))
                {
                    layout = reader.ReadToEnd();
                }
            }
            else if (layout.EndsWith(".ascx", StringComparison.OrdinalIgnoreCase))
            {
                Type assInfo;
                var assProb = ctrlData.PropertiesLang.SingleOrDefault(p => p.Name == "AssemblyInfo");
                if (assProb != null && !String.IsNullOrEmpty(assProb.Value))
                    assInfo = TypeResolutionService.ResolveType(assProb.Value, true);
                else
                    assInfo = Config.Get<ControlsConfig>().ResourcesAssemblyInfo;

                layout = ControlUtilities.GetTextResource(layout, assInfo);
            }
            return layout;
        }
    }
}