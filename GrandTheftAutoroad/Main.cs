using System;
using GTA;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Direct3D;
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
        }

        private bool _toggle;

        internal static int _min = 20;
        internal static int _max = 100;

        private void OnTick(object sender, EventArgs e)
        {
            World.CurrentDayTime = TimeSpan.FromHours(12);
            World.Weather = Weather.Clear;

            // Limpiar la calle

            Function.Call(Hash.SET_RANDOM_TRAINS, 0);
            Function.Call(Hash.CAN_CREATE_RANDOM_COPS, false);

            Function.Call(Hash.SET_NUMBER_OF_PARKED_VEHICLES, -1);
            Function.Call(Hash.SET_PARKED_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0f);
            Function.Call(Hash.SET_PED_POPULATION_BUDGET, 0);
            Function.Call(Hash.SET_VEHICLE_POPULATION_BUDGET, 0);

            Function.Call(Hash.SUPPRESS_SHOCKING_EVENTS_NEXT_FRAME);
            Function.Call(Hash.SUPPRESS_AGITATION_EVENTS_NEXT_FRAME);

            Function.Call(Hash.SET_FAR_DRAW_VEHICLES, false);
            Function.Call((Hash)0xF796359A959DF65D, false); // _DISPLAY_DISTANT_VEHICLES
            Function.Call(Hash.SET_ALL_LOW_PRIORITY_VEHICLE_GENERATORS_ACTIVE, false);

            Function.Call(Hash.SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0f);
            Function.Call(Hash.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0f);
            Function.Call(Hash.SET_PED_DENSITY_MULTIPLIER_THIS_FRAME, 0f);
            Function.Call(Hash.SET_SCENARIO_PED_DENSITY_MULTIPLIER_THIS_FRAME, 0f, 0f);

            Function.Call(Hash.DESTROY_MOBILE_PHONE);
            Function.Call((Hash)0x015C49A93E3E086E, true); //_DISABLE_PHONE_THIS_FRAME
            Function.Call(Hash.DISPLAY_CASH, false);

            Function.Call(Hash.HIDE_HELP_TEXT_THIS_FRAME);

            if (Game.IsControlJustPressed(0, Control.Context))
            {
                _toggle = !_toggle;
                UI.Notify("Mod status: " + _toggle);
            }

            if (Game.IsControlJustPressed(0, Control.PhoneDown))
            {
                if (_min > 0) _min--;
                UI.ShowSubtitle(string.Format("Min: {0} Max: {1}", _min, _max), 1000);
            }

            if (Game.IsControlJustPressed(0, Control.PhoneUp))
            {
                if (_min < 255) _min++;
                UI.ShowSubtitle(string.Format("Min: {0} Max: {1}", _min, _max), 1000);
            }

            if (Game.IsControlJustPressed(0, Control.PhoneLeft))
            {
                if (_max > 0) _max--;
                UI.ShowSubtitle(string.Format("Min: {0} Max: {1}", _min, _max), 1000);
            }

            if (Game.IsControlJustPressed(0, Control.PhoneRight))
            {

                if (_max < 255) _max++;
                UI.ShowSubtitle(string.Format("Min: {0} Max: {1}", _min, _max), 1000);
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
                    var opencvImage = swapchain.GetImage();
                    var output = OpenCV.LaneTracker.DetectLanes(opencvImage, null);

                    swapchain.FromImage(output);

                    output.Dispose();
                    opencvImage.Dispose();
                }
            }
            catch(Exception ex)
            {
                System.IO.File.AppendAllText("present.log", "\n\nException:\n" + ex);
            }
        }

    }
}
