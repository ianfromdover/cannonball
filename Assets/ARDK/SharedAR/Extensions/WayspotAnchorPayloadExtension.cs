// Copyright 2022 Niantic, Inc. All Rights Reserved.

using Niantic.ARDK.AR.WayspotAnchors;

namespace Niantic.Experimental.ARDK.SharedAR
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public static class WayspotAnchorPayloadExtension
  {
    internal static byte[] _CopyBlob(this WayspotAnchorPayload wayspotAnchorPayload)
    {
      return (byte[])wayspotAnchorPayload._Blob.Clone();
    }
  }
}
