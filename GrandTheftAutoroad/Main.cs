using System;
using GTA;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using GTANetwork.GUI.DirectXHook.Hook;
using GTANetwork.GUI.DirectXHook.Hook.Common;
using System.Drawing.Imaging;
using System.Drawing;
using GTA.Native;

namespace GrandTheftAutoroad
{
    public class Main : GTA.Script
    {
        public Main()
        {
            base.Tick += OnTick;
            base.Present += OnPresent;

            // Get screen resolution

            int width, height;

            unsafe
            {
                Function.Call(Hash._GET_SCREEN_ACTIVE_RESOLUTION, &width, &height);
            }

            _hooker = new DXHookD3D11(width, height); // TODO: dont hardcode

            _bg = new ImageElement(null, true);
            _bg.Hidden = true;
            _bg.Location = new System.Drawing.Point(0, 0);

            _hooker.AddImage(_bg);            
        }

        private DXHookD3D11 _hooker;
        private ImageElement _bg;
        private bool _toggle;

        private void OnTick(object sender, EventArgs e)
        {
            World.CurrentDayTime = TimeSpan.FromHours(12);
            World.Weather = Weather.Clear;

            if (Game.IsControlJustPressed(0, Control.Context))
            {
                _toggle = !_toggle;
                _bg.Hidden = !_toggle;
                UI.Notify("Mod status: " + _toggle);
            }
        }

        private void OnPresent(object sender, EventArgs e)
        {
            try
            {
                IntPtr swapchainPtr = (IntPtr)sender;
                SwapChain swapchain = (SwapChain)swapchainPtr;

                if (_toggle)
                {
                    var btm = swapchain.GetBitmap();
                    var opencvImage = OpenCV.OpenCVUtil.BitmapToImage(btm);
                    var g = Graphics.FromImage(btm);

                    OpenCV.LaneTracker.DetectLanes(opencvImage, g);

                    g.Dispose();
                    _bg.SetBitmap(btm);
                }

                _hooker.ManualPresentHook(swapchainPtr);
            }
            catch(Exception ex)
            {
                System.IO.File.AppendAllText("present.log", "\n\nException:\n" + ex);
            }
        }

    }
}
