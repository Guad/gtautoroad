using System;
using GTA;
using GTA.Math;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using System.Drawing.Imaging;
using System.Drawing;
using GTA.Native;
using Vector3 = GTA.Math.Vector3;
using GrandTheftAutoroad.OpenCV;

namespace GrandTheftAutoroad
{
    public class Main : GTA.Script
    {
        public Main()
        {
            base.Tick += OnTick;
            base.Present += OnPresent;

            _tracker = new LaneTracker();

            /*
            // Get screen resolution

            int width, height;

            unsafe
            {
                Function.Call(Hash._GET_SCREEN_ACTIVE_RESOLUTION, &width, &height);
            }
            */
        }

        private bool _toggle;
        private Camera _camera;
        private Vector3 _offset = new Vector3(0f, 1f, 1f);
        private Vector3 _rot;
        private LaneTracker _tracker;

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
            Function.Call(Hash.STOP_GAMEPLAY_CAM_SHAKING, true);

            if (Game.IsControlJustPressed(0, Control.Context))
            {
                _toggle = !_toggle;

                if (_toggle && _camera == null)
                {
                    _camera = World.CreateCamera(Game.Player.Character.Position, new GTA.Math.Vector3(), 30f);
                    _camera.AttachTo(Game.Player.Character, _offset);
                }
                else if (!_toggle)
                {
                    _camera.Destroy();
                    _camera = null;
                }

                World.RenderingCamera = _toggle ? _camera : null;
                UI.Notify("Mod status: " + _toggle);
                Function.Call(Hash.DISPLAY_RADAR, !_toggle);
            }

            if (!Game.Player.Character.IsInVehicle())
            {
                _toggle = false;
            }

            if (_toggle)
            {
                _camera.Rotation = new GTA.Math.Vector3(_rot.X, _rot.Y, Game.Player.Character.Rotation.Z);
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
                    var output = _tracker.DetectLanes(opencvImage);

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
