using System;
using GTA;
using SharpDX.DXGI;
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
        }

        private bool _toggle;
        private Camera _camera;
        private Vector3 _offset = new Vector3(0f, 1f, 1f);
        private LaneTracker _tracker;
        private float _correction = 0f;

        private void OnTick(object sender, EventArgs e)
        {
            CleanWorld();

            if (Game.IsControlJustPressed(0, Control.Context))
            {
                _toggle = !_toggle;

                if (_toggle && _camera == null)
                {
                    _camera = World.CreateCamera(Game.Player.Character.Position + new Vector3(0, 0, 10f), new GTA.Math.Vector3(), 30f);
                }
                else if (!_toggle)
                {
                    _camera.Destroy();
                    _camera = null;
                }

                World.RenderingCamera = _toggle ? _camera : null;
                UI.Notify("Mod status: " + _toggle);
            }

            if (!Game.Player.Character.IsInVehicle())
            {
                _toggle = false;
            }

            if (_toggle)
            {
                _camera.Position = Game.Player.Character.Position + new Vector3(0, 0, 40f);
                float h = Game.Player.Character.Rotation.Z;
                _camera.Rotation = new GTA.Math.Vector3(-90f, 0, h);

                Game.SetControlNormal(0, Control.VehicleMoveLeftRight, _correction);

                var v = Game.Player.Character.CurrentVehicle;
                v.Speed = 10f;
            }

            if (Game.IsControlJustPressed(0, Control.CreatorDelete))
            {
                _tracker = new LaneTracker();
                UI.Notify("Settings reset!");
            }
        }

        private void OnPresent(object sender, EventArgs e)
        {
            if (_toggle)
            {
                IntPtr swapchainPtr = (IntPtr)sender;
                SwapChain swapchain = (SwapChain)swapchainPtr;

                var opencvImage = swapchain.GetImage();
                var output = _tracker.DetectLanes(opencvImage);
                _correction = _tracker.CalculateCorrection();

                swapchain.FromImage(output);

                output.Dispose();
                opencvImage.Dispose();
            }
        }

        private void CleanWorld()
        {
            World.CurrentDayTime = TimeSpan.FromHours(12);
            World.Weather = Weather.Clear;

            // Clean up the street

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
        }

    }
}
