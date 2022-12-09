using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.Awareness;
using Niantic.ARDK.AR.Mesh;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.Utilities.Input.Legacy;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;
using UnityEngine.UI;

namespace ARDKExamples.ContextAwareness.Meshing
{
  public class OcclusionStabilizationExample : MonoBehaviour
  {
    [SerializeField]
    private ARSessionManager _arSessionManager;
    
    [SerializeField]
    private ARDepthManager _depthManager;

    [Header("UI")]
    [SerializeField]
    private Text _meshStatusText;

    [SerializeField]
    private Text _toggleViewButtonText;
    
    [SerializeField]
    private Text _toggleOcclusionButtonText;
    
    [SerializeField]
    private GameObject _loadingStatusPanel;
    
    [Header("Game Objects")]
    [SerializeField]
    private GameObject _pointer;
    
    [SerializeField]
    private GameObject _cube;

    private bool _contextAwarenessLoadComplete = false;
    private bool _isShowingDepths = false;
    private bool _isUsingOcclusionStabilization;

    private void Awake()
    {
      // UnityEngine.Events.UnityAction is needed as a workaround as Unity's il2cpp inlines methods
      // in Release configuration, messing with the stack traces that the ARLog system relies on
      // to toggle logs on and off. This caller corresponds to UI button presses.
      var logFeatures = new[]
      {
        "Niantic.ARDK.Extensions.Meshing", "UnityEngine.Events.UnityAction"
      };
      ARLog.EnableLogFeatures(logFeatures);
    }

    private void Start()
    {
      ShowLoadingScenePanel(false);
      ARSessionFactory.SessionInitialized += OnSessionInitialized;
    }

    private void OnDestroy()
    {
      ARSessionFactory.SessionInitialized -= OnSessionInitialized;

      if (_arSessionManager.ARSession != null)
        _arSessionManager.ARSession.Mesh.MeshBlocksUpdated -= OnMeshUpdated;
    }

    private void OnSessionInitialized(AnyARSessionInitializedArgs args)
    {
      args.Session.Mesh.MeshBlocksUpdated += OnMeshUpdated;

      _contextAwarenessLoadComplete = false;
      ShowLoadingScenePanel(true);
    }

    private void Update()
    {
      if (_arSessionManager.ARSession != null && !_contextAwarenessLoadComplete)
      {
        var status =
          _arSessionManager.ARSession.GetAwarenessInitializationStatus
          (
            out AwarenessInitializationError error,
            out string errorMessage
          );

        if (status == AwarenessInitializationStatus.Ready)
        {
          _contextAwarenessLoadComplete = true;
          ShowLoadingScenePanel(false);
        }
        else if (status == AwarenessInitializationStatus.Failed)
        {
          _contextAwarenessLoadComplete = true;
          Debug.LogErrorFormat
          (
            "Failed to initialize Context Awareness processes. Error: {0} ({1})",
            error,
            errorMessage
          );
        }
      }
      
      var touchPosition = PlatformAgnosticInput.touchCount > 0
        ? PlatformAgnosticInput.GetTouch(0).position
        : new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);

      var worldPosition = _depthManager.DepthBufferProcessor.GetWorldPosition
      (
        (int)touchPosition.x,
        (int)touchPosition.y
      );

      var normal = _depthManager.DepthBufferProcessor.GetSurfaceNormal
      (
        (int)touchPosition.x,
        (int)touchPosition.y
      );
      
      var rotation = Quaternion.Slerp
      (
        _pointer.transform.rotation,
        Quaternion.FromToRotation(Vector3.up, normal),
        Time.deltaTime * 10.0f
      );

      var cameraTransform = _depthManager.Camera.transform;
      if (Physics.Raycast
      (
        new Ray(cameraTransform.position, cameraTransform.forward),
        maxDistance: 100.0f,
        layerMask: 1 << LayerMask.NameToLayer("ARDK_FusedDepth"),
        hitInfo: out var hit
      ))
      {
        if (hit.distance < Vector3.Distance(cameraTransform.position, worldPosition))
        {
          worldPosition = hit.point;
        }
      }

      _pointer.transform.position = worldPosition;
      _pointer.transform.rotation = rotation;
    }
    
    public void PlaceCube()
    {
      _cube.transform.position = _depthManager.DepthBufferProcessor.GetWorldPosition
        (Screen.width / 2, Screen.height / 2);
      
      Debug.Log("Placed cube at: " + _cube.transform.position);
    }

    private void OnMeshUpdated(MeshBlocksUpdatedArgs args)
    {
      var mesh = args.Mesh;
      int version = mesh.MeshVersion;

      if (!_contextAwarenessLoadComplete)
      {
        // clear the popup in case meshing uses LIDAR and won't load context awareness
        _contextAwarenessLoadComplete = true;
        ShowLoadingScenePanel(false);
      }

      if (_meshStatusText != null)
      {
        _meshStatusText.text = "Mesh v" +
          version +
          "\nb: " +
          (mesh.MeshBlockCount) +
          " v: " +
          (mesh.MeshVertexCount) +
          " f: " +
          (mesh.MeshFaceCount);
      }
    }

    private void ShowLoadingScenePanel(bool toggle)
    {
      if (_loadingStatusPanel)
        _loadingStatusPanel.gameObject.SetActive(toggle);
    }

    public void ToggleShowDepth()
    {
      _isShowingDepths = !_isShowingDepths;

      // Toggle UI elements
      _toggleViewButtonText.text = _isShowingDepths ? "Show Depth" : "Show Camera";
      _depthManager.ToggleDebugVisualization(_isShowingDepths);
    }

    public void ToggleOcclusionStabilization()
    {
      _isUsingOcclusionStabilization = !_isUsingOcclusionStabilization;
      
      // Toggle UI elements
      _toggleOcclusionButtonText.text = _isUsingOcclusionStabilization ? "Occlusion Stabilization on" : "Occlusion Stabilization off";
      _depthManager.StabilizeOcclusions = !_depthManager.StabilizeOcclusions;
    }
  }
}
