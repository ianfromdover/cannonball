// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;

namespace Niantic.Experimental.ARDK.SharedAR
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public interface IPeerID : 
    IEquatable<IPeerID>
  {
    public Guid Identifier { get; }
  };

} // namespace Niantic.ARDK.SharedAR
