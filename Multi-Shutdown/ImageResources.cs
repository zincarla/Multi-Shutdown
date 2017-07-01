using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Reflection;

namespace Multi_Shutdown
{
    //Cache of image utilized by application
    public static class ImageResources
    {
        static BitmapImage onLight = null;
        static BitmapImage offLight = null;
        static BitmapImage redLight = null;
        static BitmapImage greenLight = null;
        static BitmapImage blueLight = null;
        static BitmapImage yellowLight = null;

        public static BitmapImage OnLight
        {
            get
            {
                if (onLight == null)
                {
                    BitmapImage BMI = new BitmapImage();
                    BMI.BeginInit();
                    BMI.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Multi_Shutdown.Resources.On.png");
                    BMI.EndInit();
                    onLight = BMI;
                }
                return onLight;
            }
        }

        public static BitmapImage OffLight
        {
            get
            {
                if (offLight == null)
                {
                    BitmapImage BMI = new BitmapImage();
                    BMI.BeginInit();
                    BMI.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Multi_Shutdown.Resources.Off.png");
                    BMI.EndInit();
                    offLight = BMI;
                }
                return offLight;
            }
        }

        public static BitmapImage RedLight
        {
            get
            {
                if (redLight == null)
                {
                    BitmapImage BMI = new BitmapImage();
                    BMI.BeginInit();
                    BMI.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Multi_Shutdown.Resources.Red.png");
                    BMI.EndInit();
                    redLight = BMI;
                }
                return redLight;
            }
        }

        public static BitmapImage GreenLight
        {
            get
            {
                if (greenLight == null)
                {
                    BitmapImage BMI = new BitmapImage();
                    BMI.BeginInit();
                    BMI.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Multi_Shutdown.Resources.Green.png");
                    BMI.EndInit();
                    greenLight = BMI;
                }
                return greenLight;
            }
        }

        public static BitmapImage BlueLight
        {
            get
            {
                if (blueLight == null)
                {
                    BitmapImage BMI = new BitmapImage();
                    BMI.BeginInit();
                    BMI.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Multi_Shutdown.Resources.Blue.png");
                    BMI.EndInit();
                    blueLight = BMI;
                }
                return blueLight;
            }
        }

        public static BitmapImage YellowLight
        {
            get
            {
                if (yellowLight == null)
                {
                    BitmapImage BMI = new BitmapImage();
                    BMI.BeginInit();
                    BMI.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Multi_Shutdown.Resources.Yellow.png");
                    BMI.EndInit();
                    yellowLight = BMI;
                }
                return yellowLight;
            }
        }
    }
}
