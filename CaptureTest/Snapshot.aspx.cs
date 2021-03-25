using EO.WebBrowser;
using EO.WebEngine;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CaptureTest
{
    [Serializable]
    public class CaptureException: Exception
    {
        public CaptureException(string aMessage) : base(aMessage) { }
    }

    public partial class Snapshot : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            NameValueCollection appSettings = WebConfigurationManager.AppSettings;

            Size snapshotSize = new Size(int.Parse(appSettings["snapshotContentWidth"]), int.Parse(appSettings["snapshotContentHeight"]));
            string url = appSettings["captureUrl"];

            using (System.Drawing.Image image = GetSnapshot(url, snapshotSize))
            {
                string fileName = "eo-snapshot";
                if (_highDPI) fileName += "-highDPI";
                Response.ContentType = "image/bmp";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName + ".bmp");

                image.Save(Response.OutputStream, System.Drawing.Imaging.ImageFormat.Bmp);
                Response.Flush();
            }
        }

        private static Lazy<ThreadRunner> _lazyThreadRunner =
            new Lazy<ThreadRunner>(() => {
                Engine.CleanUpCacheFolders(CacheFolderCleanUpPolicy.AllVersions);
                ThreadRunner e = new ThreadRunner("InternalWebBrowserRunner", Engine.Default);
                return e;
            }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private ThreadRunner _threadRunner => _lazyThreadRunner.Value;

        private int _completionCode = -1;
        private ErrorCode _errorCode;
        private string _errorMessage;
        private bool _loadFailed = false;

        private static bool _highDPI = false;

        private const int extraSpace = 20;
        private const int extraScreenSpace = 10;
        System.Drawing.Image GetSnapshot(string url, Size snapshotSize)
        {
            int width = snapshotSize.Width + extraSpace;
            int height = snapshotSize.Height + extraSpace;

            using (WebView vw = _threadRunner.CreateWebView(width + extraScreenSpace, height + extraScreenSpace))
            {
                vw.LoadCompleted += LoadCompleted;
                vw.LoadFailed += LoadFailed;
                
                _threadRunner.Send(() =>
                {
                    vw.LoadUrlAndWait(url);
                });

                if (_loadFailed)
                {
                    throw new CaptureException($"url '{url}' load failed. error code: {_errorCode}, error message: '{_errorMessage}', HTTP status: {_completionCode}");
                }

                System.Drawing.Image img = null;

                _threadRunner.Send(() =>
                {
                    img = vw.Capture(new Rectangle(0, 0, width, height));
                });

                return img;
            }
        }

        private void LoadFailed(object sender, LoadFailedEventArgs e)
        {
            _errorCode = e.ErrorCode;
            _completionCode = e.HttpStatusCode;
            _errorMessage = e.ErrorMessage;
            _loadFailed = true;
        }

        private void LoadCompleted(object sender, LoadCompletedEventArgs e)
        {
            _completionCode = e.HttpStatusCode;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetProcessDPIAware();

        public static void InitializeDPI()
        {
            string setDPIStr = WebConfigurationManager.AppSettings["highDPI"];
            _highDPI = string.IsNullOrEmpty(setDPIStr) ? false : bool.Parse(setDPIStr);
            
            if (!_highDPI) return;

            _highDPI = SetProcessDPIAware();

            if (!_highDPI)
            {
                throw new CaptureException("Could not set DPI awareness for the process.");
            }
        }

        static Snapshot()
        {
            Runtime.AddLicense(WebConfigurationManager.AppSettings["eo-license"]);

            EO.Base.Runtime.EnableEOWP = true;
        }
    }
}