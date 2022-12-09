using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Niantic.ARDK.Internals;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;

namespace Niantic.ARDK.AR.WayspotAnchors
{
  internal class _WayspotAnchorFactory
  {
    // In WayspotAnchorService, WayspotAnchors are created in C# before
    // they are in C++, using the identifiers from WayspotAnchorController

    // In WayspotAnchorController, anchor identifiers are returned immediately
    // from "creating" anchors. In NAR, actually identifiers are being generated
    // and then a creation request is sent for those identifiers.

    public static IWayspotAnchor Create(Guid identifier, Matrix4x4 localPose)
    {
      if (_allAnchors.ContainsKey(identifier))
        throw new InvalidOperationException($"Wayspot anchor with identifier {identifier} already exists.");

      IWayspotAnchor anchor;
      if (_NativeAccess.IsNativeAccessValid())
        anchor = new _NativeWayspotAnchor(identifier, localPose);
#pragma warning disable 0162
      else
        anchor = new _MockWayspotAnchor(identifier, localPose);
#pragma warning restore 0162

      _allAnchors.TryAdd(identifier, anchor);
      return anchor;
    }

    public static IWayspotAnchor Create(byte[] blob)
    {
      IWayspotAnchor anchor;

      if (_NativeAccess.IsNativeAccessValid())
        anchor = new _NativeWayspotAnchor(blob);
      else
#pragma warning disable 0162
        anchor = new _MockWayspotAnchor(blob);
#pragma warning restore 0162

      if (!_allAnchors.TryAdd(anchor.ID, anchor))
      {
        ARLog._WarnRelease($"Tried to restore a Wayspot anchor (ID: {anchor.ID}) that already exists.");
        anchor.Dispose();
        anchor = _allAnchors.TryGetValue(anchor.ID);
      }

      return anchor;
    }

    public static IWayspotAnchor GetOrCreateFromNativeHandle(IntPtr nativeHandle)
    {
      _NativeAccess.AssertNativeAccessValid();

      if (nativeHandle == IntPtr.Zero)
        throw new ArgumentException(nameof(nativeHandle));

      var valid = _NativeWayspotAnchor._NAR_ManagedPose_GetIdentifier(nativeHandle, out Guid identifier);
      if (!valid)
        throw new ArgumentException(nameof(nativeHandle));

      var anchor = _allAnchors.TryGetValue(identifier);
      if (anchor == null)
      {
        anchor = new _NativeWayspotAnchor(nativeHandle);
        _allAnchors.TryAdd(identifier, anchor);
      }

      ((_NativeWayspotAnchor)anchor).SetNativeHandle(nativeHandle);
      return anchor;
    }

    public static IWayspotAnchor GetOrCreateFromIdentifier(Guid identifier)
    {
      var anchor = _allAnchors.TryGetValue(identifier);
      if (anchor == null)
        anchor = Create(identifier, Matrix4x4.zero);

      return anchor;
    }

    public static void Remove(Guid id)
    {
      _allAnchors.Remove(id);
    }

    //TODO: Factories should be stateless AR-12521
    private static _WeakValueDictionary<Guid, IWayspotAnchor> _allAnchors =
      new _WeakValueDictionary<Guid, IWayspotAnchor>();
  }
}