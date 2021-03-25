using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CaptureTest
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            NameValueCollection appSettings = WebConfigurationManager.AppSettings;
            string filePath = Server.MapPath(appSettings["contentPath"]);
            string content = File.ReadAllText(filePath);
            Response.Write(content);
        }
    }
}