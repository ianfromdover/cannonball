// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.Anchors;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.ReferenceImage;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.Utilities;

using UnityEngine;
using UnityEngine.UI;

namespace Niantic.ARDKExamples
{
  // Image Detection example. Shows how to create and use an ARImageDetectionManager, both through
  // the inspector and through code. For the manager created through code, shows how to create
  // ARReferenceImages both from a byte stream and from a file.
  // Also includes adding and removing an image from a manager at runtime.
  //
  // The expected behavior is that color-coded rectangles will appear over the image if it shows up
  // in the real environment (such as pulled up on a computer monitor). The rectangle will follow if
  // the image moves, but it jumps a few times a second rather than smoothly.
  // For the inspector created manager, a blue rectangle will appear over the image of the crowd.
  // For the code created manager, red and green rectangles will appear over the images of the
  // Niantic yeti and logo.
  // If the detected images are changed (by switching between managers, or by enabling/disabling the
  // yeti) the detected anchors will be cleared.
  //
  // See the "Detecting Images" page in the User Manual for further information on how to optimally
  // detect images and use image anchors.
  public class ImageDetectionExampleManager:
    MonoBehaviour
  {
    [SerializeField]
    private ARSessionManager _arSessionManager;

    [SerializeField]
    private ARImageDetectionManager _imageDetectionManager;

    [SerializeField]
    [Tooltip("Prefab to spawn on top of detected images.")]
    private GameObject _plane = null;

    [Header("Reference Image Input")]
    
    [SerializeField]
    private CreateReferenceImageFunction _selectedReferenceImageFunction =
      CreateReferenceImageFunction.FromBytesSync;

    [SerializeField]
    [Tooltip("Raw bytes of the jpg image used to test creating an image reference from a byte buffer." +
    "Use a .jpg file by adding .bytes extensions to the file.")]
    private TextAsset _imageAsBytes;
    
    [SerializeField]
    [Tooltip("Path of the jpg image used to test creating an image reference from a local file.")]
    private string _imagePath = "ImageMarkers/Yeti.jpg";

    [SerializeField]
    [Tooltip("Size (meters) of the yeti image in physical form.")]
    private float _physicalImageWidth;

    [Header("Controls")]
    [Tooltip("A button that enables/disables the tracking of the yeti image.")] 
    [SerializeField]
    private Button _toggleYetiButton;
    
    // A handle to the yeti image, used to remove and insert it into the _codeImageDetectionManager.
    private IARReferenceImage _yetiImage;
    
    // Chooses different colors for different reference images. The "crowd" reference image is
    // added via the inspector of the ARImageDetectionManager.
    static Dictionary<string, Color> _imageColors = new Dictionary<string, Color>
    {
      { "yeti", Color.green },
      { "crowd", Color.blue },
    };

    public enum CreateReferenceImageFunction
    {
      FromBytesSync,
      FromBytesAsync,
      FromPathSync,
      FromPathAsync,
    }
    
    private Dictionary<Guid, GameObject> _detectedImages = new Dictionary<Guid, GameObject>();

    private void Start()
    {
      ARSessionFactory.SessionInitialized += SetupSession;
      SetupCodeImageDetectionManager();
    }

    private void SetupSession(AnyARSessionInitializedArgs arg)
    {
      // Add listeners to all relevant ARSession events.
      var session = arg.Session;
      session.SessionFailed += args => Debug.Log(args.Error);
      session.AnchorsAdded += OnAnchorsAdded;
      session.AnchorsUpdated += OnAnchorsUpdated;
      session.AnchorsRemoved += OnAnchorsRemoved;
    }

    public void SetRunOptions(bool removeExistingAnchors)
    {
      if (removeExistingAnchors)
        _arSessionManager.RunOptions = ARSessionRunOptions.RemoveExistingAnchors;
      else
        _arSessionManager.RunOptions = ARSessionRunOptions.None;
    }
    
    private void SetupCodeImageDetectionManager()
    {
      // The StreamingAsset Folder has to be created manually for each new project. Create a new folder at Assets/StreamingAssets/ImageMarkers and copy the yeti.jpg image into it.
      // The contents of Assets/StreamingAssets are copied to device when installing an app.
      string filePathImageBytes = Path.Combine(Application.streamingAssetsPath, _imagePath);
      
      switch (_selectedReferenceImageFunction)
      {
        case CreateReferenceImageFunction.FromBytesSync:
          // Create an ARReferenceImage from raw bytes of a jpeg. In a real application, these bytes
          // could have been received over the network.
          byte[] rawByteBuffer = _imageAsBytes.bytes;
          _yetiImage =
            ARReferenceImageFactory.Create
            (
              "yeti",
              rawByteBuffer,
              rawByteBuffer.Length,
              _physicalImageWidth
            );
          
          _imageDetectionManager.AddImage(_yetiImage);
          break;

        case CreateReferenceImageFunction.FromBytesAsync:
          // Create an ARReferenceImage from raw bytes of a jpeg. In a real application, these bytes
          // could have been received over the network.
          byte[] rawByteBufferAsync = _imageAsBytes.bytes;
          ARReferenceImageFactory.CreateAsync
          (
            "yeti",
            rawByteBufferAsync,
            rawByteBufferAsync.Length,
            _physicalImageWidth,
            arReferenceImage =>
            {
              _yetiImage = arReferenceImage;
              _imageDetectionManager.AddImage(_yetiImage);
            }
          );
          break;
        
        case CreateReferenceImageFunction.FromPathSync:
          
          // Create an ARReferenceImage from the local file path.
          _yetiImage =
            ARReferenceImageFactory.Create
            (
              "yeti",
              filePathImageBytes,
              _physicalImageWidth
            );
          
          _imageDetectionManager.AddImage(_yetiImage);
          break;

        case CreateReferenceImageFunction.FromPathAsync:
          // Create an ARReferenceImage from the local file path.

          ARReferenceImageFactory.CreateAsync
          (
            "yeti",
            filePathImageBytes,
            _physicalImageWidth,
            arReferenceImage =>
            {
              _yetiImage = arReferenceImage;
              _imageDetectionManager.AddImage(_yetiImage);
            }
          );
          break;
        
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public void ToggleYetiImage(bool add)
    {
      // This enables/disables the Yeti image by removing it from the manager.
      // This doesn't do anything to the created GameObject. If the yeti hasn't been detected, no
      // new GameObject will be created. If the yeti has already been detected, the GameObject will
      // remain in place but not update if the yeti image is moved.
      if (add)
        _imageDetectionManager.AddImage(_yetiImage);
      else
        _imageDetectionManager.RemoveImage(_yetiImage);
    }

    private void OnAnchorsAdded(AnchorsArgs args)
    {
      foreach (var anchor in args.Anchors)
      {
        if (anchor.AnchorType != AnchorType.Image)
          continue;

        IARImageAnchor imageAnchor = (IARImageAnchor) anchor;
        string imageName = imageAnchor.ReferenceImage.Name;

        GameObject newPlane = Instantiate(_plane);
        newPlane.name = "Image-" + imageName;
        SetPlaneColor(newPlane, imageName);
        _detectedImages[anchor.Identifier] = newPlane;

        UpdatePlaneTransform(imageAnchor);
      }
    }

    private void SetPlaneColor(GameObject plane, string imageName)
    {
      var renderer = plane.GetComponentInChildren<MeshRenderer>();
      Color planeColor = Color.black;
      _imageColors.TryGetValue(imageName, out planeColor);
      renderer.material.color = planeColor;
    }

    private void OnAnchorsUpdated(AnchorsArgs args)
    {
      foreach (var anchor in args.Anchors)
      {
        if (!_detectedImages.ContainsKey(anchor.Identifier))
          continue;

        IARImageAnchor imageAnchor = (IARImageAnchor)anchor;
        UpdatePlaneTransform(imageAnchor);
      }
    }

    private void OnAnchorsRemoved(AnchorsArgs args)
    {
      foreach (var anchor in args.Anchors)
      {
        if (!_detectedImages.ContainsKey(anchor.Identifier))
          continue;

        Destroy(_detectedImages[anchor.Identifier]);
        _detectedImages.Remove(anchor.Identifier);
      }
    }

    private void UpdatePlaneTransform(IARImageAnchor imageAnchor)
    {
      Guid identifier = imageAnchor.Identifier;

      _detectedImages[identifier].transform.position = imageAnchor.Transform.ToPosition();
      _detectedImages[identifier].transform.rotation = imageAnchor.Transform.ToRotation();

      Vector3 localScale = _detectedImages[identifier].transform.localScale;
      localScale.x = imageAnchor.ReferenceImage.PhysicalSize.x;
      localScale.z = imageAnchor.ReferenceImage.PhysicalSize.y;
      _detectedImages[identifier].transform.localScale = localScale;
    }
  }
}
