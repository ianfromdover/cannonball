// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Runtime.InteropServices;

using Niantic.ARDK.Internals;
using Niantic.ARDK.Utilities;

namespace Niantic.ARDK.AR.Mesh
{
  internal sealed class _NativeARMeshData:
    _IARMeshData
  {
    static _NativeARMeshData()
    {
      _Platform.Init();
    }

    private IntPtr _nativeHandle;

    // Used to inform the C# GC that there is managed memory held by this object
    // points + identifiers (estimating 200 points)
    private const long _MemoryPressure = (200L * (3L * 4L)) + (200L * 8L);

    public _NativeARMeshData(IntPtr nativeHandle)
    {
      _NativeAccess.AssertNativeAccessValid();

      if (nativeHandle == IntPtr.Zero)
        throw new ArgumentException("nativeHandle can't be Zero.", nameof(nativeHandle));

      _nativeHandle = nativeHandle;

      GC.AddMemoryPressure(_MemoryPressure);
    }

    private static void _ReleaseImmediate(IntPtr nativeHandle)
    {
      _NARMesh_Release(nativeHandle);
    }

    ~_NativeARMeshData()
    {
      _ReleaseImmediate(_nativeHandle);
      GC.RemoveMemoryPressure(_MemoryPressure);
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);

      var nativeHandle = _nativeHandle;
      if (nativeHandle != IntPtr.Zero)
      {
        _nativeHandle = IntPtr.Zero;

        _ReleaseImmediate(nativeHandle);
        GC.RemoveMemoryPressure(_MemoryPressure);
      }
    }

    public float MeshBlockSize
    {
      get => _NARMesh_GetMeshBlockSize(_nativeHandle);
    }

    public int GetBlockMeshInfo
    (
      out int blockBufferSizeOut,
      out int vertexBufferSizeOut,
      out int faceBufferSizeOut
    )
    {
      blockBufferSizeOut = 0;
      vertexBufferSizeOut = 0;
      faceBufferSizeOut = 0;

      return
        _NARMesh_GetBlockMeshInfo
        (
          _nativeHandle,
          out blockBufferSizeOut,
          out vertexBufferSizeOut,
          out faceBufferSizeOut
        );
    }

    public int GetBlockMesh
    (
      IntPtr blockBuffer,
      IntPtr vertexBuffer,
      IntPtr faceBuffer,
      int blockBufferSize,
      int vertexBufferSize,
      int faceBufferSize
    )
    {
      return
        _NARMesh_GetBlockMesh
        (
          _nativeHandle,
          blockBuffer,
          vertexBuffer,
          faceBuffer,
          blockBufferSize,
          vertexBufferSize,
          faceBufferSize
        );
    }

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARMesh_Release(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern float _NARMesh_GetMeshBlockSize(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern int _NARMesh_GetBlockMeshInfo
    (
      IntPtr nativeHandle,
      out int blockBufferSizeOut,
      out int vertexBufferSizeOut,
      out int faceBufferSizeOut
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern int _NARMesh_GetBlockMesh
    (
      IntPtr nativeHandle,
      IntPtr blockBuffer,
      IntPtr vertexBuffer,
      IntPtr faceBuffer,
      int blockBufferSize,
      int vertexBufferSize,
      int faceBufferSize
    );
  }
}
