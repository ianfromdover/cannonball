using Niantic.ARDK.AR.Awareness;
using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Editor;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;

namespace ARDK.Extensions
{
  [DisallowMultipleComponent]
  public sealed class ARHandTrackingManager:
    ARSessionListener
  {
    /// Event for when the contents of the detection is updated
    /// @note this is an experimental feature
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public event ArdkEventHandler<HumanTrackingArgs> HandTrackingUpdated;

    [SerializeField]
    [_Autofill]
    private Camera _arCamera;

    protected override void ListenToSession()
    {
      ARSession.HandTracker.HandTrackingStreamUpdated += HandTrackingUpdated;
    }

    protected override void StopListeningToSession()
    {
      ARSession.HandTracker.HandTrackingStreamUpdated -= HandTrackingUpdated;
    }

    protected override void InitializeImpl()
    {
      if (_arCamera == null)
      {
        var warning = "The Camera field is not set on the HandTrackingManager before use, " +
          "grabbing Unity's Camera.main";

        ARLog._Warn(warning);
        _arCamera = Camera.main;
      }
      ARSession?.HandTracker?.AssignViewport(_arCamera);

      base.InitializeImpl();
    }

    public override void ApplyARConfigurationChange
      (ARSessionChangesCollector.ARSessionRunProperties properties)
    {
      if (properties.ARConfiguration is IARWorldTrackingConfiguration worldConfig)
      {
        worldConfig.IsPalmDetectionEnabled = true;
      }
    }
  }
}
