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

        }

        private bool _toggle;
        private Camera _camera;
        private Vector3 _offset = new Vector3(0f, 1f, 1f);
        private Vector3 _rot;
        private LaneTracker _tracker;
        private float _correction = 0f;

        private int _pidMode = 0;
        private string[] modenames = new[] { "Kp", "Ki", "Kd" };
        internal static int _tolerance = 400;

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
                    _camera = World.CreateCamera(Game.Player.Character.Position + new Vector3(0, 0, 10f), new GTA.Math.Vector3(), 30f);
                }
                else if (!_toggle)
                {
                    _camera.Destroy();
                    _camera = null;
                }

                World.RenderingCamera = _toggle ? _camera : null;
                UI.Notify("Mod status: " + _toggle);
                //Function.Call(Hash.DISPLAY_RADAR, !_toggle);
                //Function.Call(Hash._SET_RADAR_BIGMAP_ENABLED, _toggle, false);
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


            float roc = 0.0001f;
            if (Game.IsControlJustPressed(0, Control.PhoneLeft))
            {
                float newval = 0;
                switch(_pidMode)
                {
                    case 0: // Kp
                        _tracker.PID.KP -= roc;
                        newval = _tracker.PID.KP;
                        break;
                    case 1: // Ki
                        _tracker.PID.KI -= roc;
                        newval = _tracker.PID.KI;
                        break;
                    case 2: // Kd
                        _tracker.PID.KD -= roc;
                        newval = _tracker.PID.KD;
                        break;
                }

                UI.ShowSubtitle("[" + modenames[_pidMode] + "] = " + newval);
            }

            if (Game.IsControlJustPressed(0, Control.PhoneRight))
            {
                float newval = 0;
                switch (_pidMode)
                {
                    case 0: // Kp
                        _tracker.PID.KP += roc;
                        newval = _tracker.PID.KP;
                        break;
                    case 1: // Ki
                        _tracker.PID.KI += roc;
                        newval = _tracker.PID.KI;
                        break;
                    case 2: // Kd
                        _tracker.PID.KD += roc;
                        newval = _tracker.PID.KD;
                        break;
                }

                UI.ShowSubtitle("[" + modenames[_pidMode] + "] = " + newval);
            }
            
            /*
            if (Game.IsControlJustPressed(0, Control.PhoneUp))
            {
                _pidMode = (_pidMode - 1);
                if (_pidMode < 0) _pidMode = 2;

                UI.ShowSubtitle("PIDMODE: " + modenames[_pidMode]);
            }

            if (Game.IsControlJustPressed(0, Control.PhoneDown))
            {
                _pidMode = (_pidMode + 1) % 3;
                UI.ShowSubtitle("PIDMODE: " + modenames[_pidMode]);
            }
            
            if (Game.IsControlJustPressed(0, Control.PhoneUp))
            {
                _tolerance += 10;

                UI.ShowSubtitle("Tolerance: " + _tolerance);
            }

            if (Game.IsControlJustPressed(0, Control.PhoneDown))
            {
                _tolerance -= 10;
                UI.ShowSubtitle("Tolerance: " + _tolerance);
            }
            */
            if (Game.IsControlJustPressed(0, Control.CreatorDelete))
            {
                _tracker = new LaneTracker();
                UI.Notify("Settings reset!");
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
                    _correction = _tracker.CalculateCorrection();

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
