using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Sitefinity.Abstractions.VirtualPath;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Modules.Pages;
using Telerik.Sitefinity.Pages.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.Web.Configuration;
using Telerik.Sitefinity.Web.UI;
using System.Text;
using Telerik.Sitefinity.Modules.GenericContent.Web.UI;
using Telerik.Sitefinity.Utilities.HtmlParsing;
using Telerik.Sitefinity.Web.UI;
using Telerik.Sitefinity.Web.Utilities;
using Telerik.Sitefinity.GenericContent.Model;
using SitefinityWebApp.LayoutControls;

namespace SitefinityWebApp
{
    public partial class TemplateLayoutControls : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var manager = PageManager.GetManager();
            var helper = new LayoutControlsHelper();
            var mycontrol = helper.GetLayoutControlFromPage(manager, "MyPage", "customclass");
            if (mycontrol.Id != Guid.Empty)
            {
                var html = helper.GetControlHtml(mycontrol);

                // change the html of the layout control
                html += "<span>This is from debug</span>";
                helper.ChangeControlHtml(manager, mycontrol, html);
            }
        }
    }
}