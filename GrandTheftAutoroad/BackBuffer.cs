using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandTheftAutoroad
{
    public static class BackBuffer
    {
        public static Bitmap GetBitmap(this SwapChain swapchain)
        {
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
            var boundsRect = new System.Drawing.Rectangle(0, 0, origDesc.Width, origDesc.Height);

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

            return bitmap;
        }
    }
}
