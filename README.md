# CaptureTest
Sample project for demonstrating the image quality of the images captured from Essential Objects WebView

Open the solution in Visual Studio (2019)
Put Essential Objects license data into "eo-license" application settings element.
Run the web project (in IISExpress or IIS, it does not matter).
After the default page is loaded, click the left mouse button, pointing anywhere on the page. A few moments later the snapshot, created with EO WebView will be downloaded.
Application setting "highDPI" allows to choose run SetProcessDPIAware() or not.
Comparing images, obtained with highDPI=false and highDPI=true, I cannot find any differences.

