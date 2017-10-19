using System;


namespace Microsoft.Xna.Framework.Graphics
{
    public static class MonoGameExtensions
    {
        public static void GetNativeDxDeviceAndContext(this GraphicsDevice graphicsDevice, out IntPtr dxDevicePtr, out IntPtr dxContextPtr)
        {
            var graphicsDeviceType = typeof(GraphicsDevice);

            var d3dDeviceInfo  = graphicsDeviceType.GetField("_d3dDevice", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var device = d3dDeviceInfo.GetValue(graphicsDevice) as SharpDX.Direct3D11.Device;
            dxDevicePtr = device.NativePointer;
            var d3dContextInfo = graphicsDeviceType.GetField("_d3dContext", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var context = d3dContextInfo.GetValue(graphicsDevice) as SharpDX.Direct3D11.DeviceContext;
            dxContextPtr = context.NativePointer;
        }

        public static IntPtr GetNativeDxResource(this RenderTarget2D renderTarget2D)
        {
            var renderTarget2DType = typeof(RenderTarget2D);

            var textureInfo = renderTarget2DType.GetField("_texture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var resource = textureInfo.GetValue(renderTarget2D) as SharpDX.Direct3D11.Resource;
            return resource.NativePointer;
        }
    }
}
