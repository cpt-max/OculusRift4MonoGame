# OculusRift4MonoGame


- The OculusRift folder contains the source code for the C++ dll that talks to the Rift. 
  It's based on the Oculus SDK. 

- The OculusRiftSample folder contains a MonoGame sample project that PInvokes into OculusRift.dll  
  In order to compile this, you first need to fix up the Monogame.Framework.Windows reference. 
  Also this sample requires access to some native DirectX objects, that are currently not exposed in MonoGame. 
  You need to add two new functions to MonoGame:
     
  
  add this to the GraphicsDevice class in GraphicsDevice.DirectX.cs
  ```
  public void GetNativeDxDeviceAndContext(out IntPtr dxDevice, out IntPtr dxContext)
  {
      dxDevice = _d3dDevice.NativePointer;
      dxContext = _d3dContext.NativePointer;
  }
  ```
  
  add this to the Texture class in Texture.DirectX.cs
  ```
  public IntPtr GetNativeDxResource()
  {
      return _texture.NativePointer;
  }
  ```

- The OculusRiftSample_prebuilt folder contains the prebuilt binary version of the OculusRiftSample.
  If everything goes well you'll find yourself surrounded by pyramids. Use the mouse to move around.


