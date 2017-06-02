using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Multi_Shutdown
{
    /// <summary>
    /// Interaction logic for InfoLight.xaml
    /// </summary>
    public partial class InfoLight : UserControl
    {
        private ImageSource imageSource
        {
            get
            {
                if (State == LightState.Off)
                {
                    return ImageResources.OffLight;
                }
                else if (State == LightState.On)
                {
                    return ImageResources.OnLight;
                }
                else if (State == LightState.Red)
                {
                    return ImageResources.RedLight;
                }
                else if (State == LightState.Green)
                {
                    return ImageResources.GreenLight;
                }
                else if (State == LightState.Blue)
                {
                    return ImageResources.BlueLight;
                }
                else if (State == LightState.Yellow)
                {
                    return ImageResources.YellowLight;
                }
                else
                {
                    throw new Exception("Incomplete code!");
                }
            }
        }

        private ImageSource blinkImageSource
        {
            get
            {
                if (BlinkState == LightState.Off)
                {
                    return ImageResources.OffLight;
                }
                else if (BlinkState == LightState.On)
                {
                    return ImageResources.OnLight;
                }
                else if (BlinkState == LightState.Red)
                {
                    return ImageResources.RedLight;
                }
                else if (BlinkState == LightState.Green)
                {
                    return ImageResources.GreenLight;
                }
                else if (BlinkState == LightState.Blue)
                {
                    return ImageResources.BlueLight;
                }
                else if (BlinkState == LightState.Yellow)
                {
                    return ImageResources.YellowLight;
                }
                else
                {
                    throw new Exception("Incomplete code!");
                }
            }
        }


        LightState state = LightState.Off;
        public LightState State { get { return state; } set { state = value; InvalidateVisual(); } }

        LightState blinkState = LightState.Off;
        public LightState BlinkState { get { return blinkState; } set { blinkState = value; InvalidateVisual(); } }

        public TimeSpan BlinkInterval
        {
            get
            {
                return BlinkTimer.Interval;
            }
            set
            {
                BlinkTimer.Interval = value;
            }
        }

        public bool BlinkLight
        {
            get
            {
                return BlinkTimer.IsEnabled;
            }
            set
            {
                if (value)
                {
                    BlinkTimer.Start();
                }
                else
                {
                    BlinkTimer.Stop();
                    Blink = false;
                }
            }
        }

        private DispatcherTimer BlinkTimer;

        private bool Blink = false;

        public InfoLight()
        {
            InitializeComponent();
            BlinkTimer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            BlinkTimer.Tick += new EventHandler(BlinkTimer_Tick);
            BlinkTimer.Interval = TimeSpan.FromSeconds(.75d);
        }

        void BlinkTimer_Tick(object sender, EventArgs e)
        {
            Blink = !Blink;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (!Blink)
            {
                drawingContext.DrawImage(imageSource, new Rect(0, 0, 20, 20));
            }
            else
            {
                drawingContext.DrawImage(blinkImageSource, new Rect(0, 0, 20, 20));
            }
        }
    }
    public enum LightState { On, Off, Red, Green, Blue, Yellow }
}
