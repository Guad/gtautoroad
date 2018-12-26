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

namespace GrandTheftAutoroad
{
    public class Test : GTA.Script
    {
        public Test()
        {
            base.Tick += OnTick;
            base.Present += OnPresent;

            //_hooker = new DXHookD3D11(1920, 1080); // TODO: dont hardcode
        }

        //private DXHookD3D11 _hooker;
        private bool _save;

        private void OnTick(object sender, EventArgs e)
        {
            if (Game.IsControlJustPressed(0, Control.Context))
            {
                _save = true;
                //_hooker.
            }
        }
        
        private void OnPresent(object sender, EventArgs e)
        {
            IntPtr swapchainPtr = (IntPtr) sender;
            SwapChain swapchain = (SwapChain)swapchainPtr;

            var bb = swapchain.GetBackBuffer<Texture2D>(0);
            SharpDX.Direct3D11.Device dev = bb.Device;

            var origDesc = bb.Description;

            // Create Staging texture CPU-accessible
            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = origDesc.Width,
                Height = origDesc.Height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };
            var screenTexture = new Texture2D(dev, textureDesc);

            dev.ImmediateContext.CopyResource(bb, screenTexture);

            var mapSource = dev.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read, 0);

            // Create bitmap
            var bitmap = new System.Drawing.Bitmap(origDesc.Width, origDesc.Height, PixelFormat.Format32bppArgb);
            var boundsRect = new Rectangle(0, 0, origDesc.Width, origDesc.Height);

            var mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            var sourcePtr = mapSource.DataPointer;

            var destPtr = mapDest.Scan0;

            int height = origDesc.Height;
            int width = origDesc.Width;

            for (int y = 0; y < height; y++)
            {
                // Copy a single line 
                Utilities.CopyMemory(destPtr, sourcePtr, width * 4);

                // Advance pointers
                sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                destPtr = IntPtr.Add(destPtr, mapDest.Stride);
            }

            bitmap.UnlockBits(mapDest);
            dev.ImmediateContext.UnmapSubresource(screenTexture, 0);

            bitmap.Save("screencapture.png");

            //_hooker.ManualPresentHook(swapchainPtr);
        }

    }
}
