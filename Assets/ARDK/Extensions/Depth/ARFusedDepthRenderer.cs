using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.Rendering;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;

namespace ARDK.Extensions.Depth
{
  /// This component is used to capture the depth of the fused mesh.
  /// The view is provided by a camera owned by this component.
  /// The transform of the camera is updated using the tracking 
  /// information extracted from the latest AR frame.
  public class ARFusedDepthRenderer: 
    ARSessionListener
  {
    private Camera _camera;
    private Shader _shader;
    private Material _material;
    private RenderTarget _target;

    /// The GPU texture containing depth values of the fused mesh.
    public RenderTexture GPUTexture { get; private set; }

    public void Configure(RenderTarget target, LayerMask meshLayer)
    {
      if (!Initialized)
      {
        ARLog._Error("Initialize the component first.");
        return;
      }

      _target = target;

      // Configure camera
      _camera.clearFlags = CameraClearFlags.Depth;
      _camera.depthTextureMode = DepthTextureMode.Depth;
      _camera.cullingMask = 1 << meshLayer;

    }

    public override void ApplyARConfigurationChange
    (
      ARSessionChangesCollector.ARSessionRunProperties properties
    )
    {
    }

    protected override void ListenToSession()
    {
      if (ARSession.Configuration is IARWorldTrackingConfiguration {IsMeshingEnabled: false})
        ARLog._Error("Rendering depth values from the fused mesh requires meshing to be enabled.");
      
      ARSession.FrameUpdated += ARSession_OnFrameUpdated;
    }

    protected override void StopListeningToSession()
    {
      ARSession.FrameUpdated -= ARSession_OnFrameUpdated;
    }

    protected override void InitializeImpl()
    {
      base.InitializeImpl();

      // Allocate GPU texture
      GPUTexture = new RenderTexture(256, 256, 24, RenderTextureFormat.Depth)
      {
        filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp
      };

      GPUTexture.Create();

      // Allocate a camera
      _camera = gameObject.AddComponent<Camera>();
      _camera.targetTexture = GPUTexture;

      _shader = Resources.Load<Shader>("ARFusedDepthShader");
      _material = new Material(_shader)
      {
        hideFlags = HideFlags.HideAndDontSave
      };
    }

    private void ARSession_OnFrameUpdated(FrameUpdatedArgs args)
    {
      var arCamera = args.Frame?.Camera;
      if (arCamera == null)
        return;

      // Update projection
      if (!_target.IsTargetingCamera || _target.Camera == null)
      {
        // Calculate a new projection matrix
        var orientation = MathUtils.CalculateScreenOrientation();
        var resolution = _target.GetResolution(orientation);
        _camera.projectionMatrix = arCamera.CalculateProjectionMatrix
        (
          orientation,
          resolution.width,
          resolution.height,
          0.1f,
          100.0f
        );
      }
      else
        // Use the projection of the render target
        _camera.projectionMatrix = _target.Camera.projectionMatrix;


      // Update the pose
      var worldTransform = arCamera.GetViewMatrix(Screen.orientation).inverse;
      var position = worldTransform.ToPosition();
      var rotation = worldTransform.ToRotation();
      _camera.transform.SetPositionAndRotation(position, rotation);
    }

    protected override void DeinitializeImpl()
    {
      base.DeinitializeImpl();

      Destroy(_camera);

      if (GPUTexture != null)
        Destroy(GPUTexture);

      if (_material != null)
        Destroy(_material);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
      Graphics.Blit(src, dest, _material);
    }
  }
}
