using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.IO;

using Autodesk.Revit.UI;
using Beacon.Properties;

namespace Beacon
{
    public class App : IExternalApplication
    {
        // Icon for WPF
        public static System.Windows.Media.ImageSource BeaconIcon;

        /// <summary>
        /// Implements OnStartup interface to add Beacon external command on Revit Startup.
        /// </summary>
        /// <param name="application">A handle to the application being started.</param>
        /// <returns>Indicates if the external application completes its work successfully.</returns>
        public Result OnStartup(UIControlledApplication application)
        {
            AddMenu(application);
            BeaconIcon = GetSourceFromBitmap(Resources.icon_32, new System.Drawing.Size(32, 32));
            return Result.Succeeded;
        }

        /// <summary>
        /// Implements OnShutdown interface.
        /// </summary>
        /// <param name="application">A handle to the application being shut down.</param>
        /// <returns>Indicates if the external application completes its work successfully.</returns>
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        /// <summary>
        /// Add Beacon to Add-Ins Ribbon
        /// </summary>
        /// <param name="app">A handle to the Revit application.</param>
        private void AddMenu(UIControlledApplication app)
        {
            RibbonPanel rvtRibbonPanel = app.CreateRibbonPanel("Beacon");

            string ExecutingAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var button = new PushButtonData("Beacon", "Embodied Carbon", ExecutingAssemblyPath, "Beacon.BeaconCommand");
            button.Image = GetSourceFromBitmap(Resources.icon_16, new System.Drawing.Size(16, 16));
            button.LargeImage = GetSourceFromBitmap(Resources.icon_32, new System.Drawing.Size(32, 32));
            rvtRibbonPanel.AddItem(button);
        }

        /// <summary>
        /// Convert Bitmap to BitmapSource
        /// </summary>
        /// <param name="bitmap">The Bitmap to convert.</param>
        /// <param name="size">The desired converted size.</param>
        /// <returns>Converted BitmapSource.</returns>
        private BitmapSource GetSourceFromBitmap(System.Drawing.Bitmap bitmap, System.Drawing.Size size)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(size.Width, size.Height)
                );
        }
    }
}
