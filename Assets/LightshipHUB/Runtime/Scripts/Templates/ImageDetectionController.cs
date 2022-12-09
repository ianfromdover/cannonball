// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

using Niantic.ARDK.Extensions;
using Niantic.ARDK.AR;
using Niantic.ARDK.AR.Anchors;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.ReferenceImage;
using Niantic.ARDK.Utilities;

namespace Niantic.LightshipHub.Templates
{
  public class ImageDetectionController : MonoBehaviour 
  {
    [Tooltip("The name of the image file saved in the StreamingAssets folder. Don't add the extension, just the name")]
    public string ImageTracker;
    [HideInInspector]
    public ObjectHolderController OHcontroller;
    [HideInInspector]
    public ARImageDetectionManager ImageDetectionManager;
    
    private Dictionary<Guid, GameObject> _detectedImages = new Dictionary<Guid, GameObject>();
    private IARSession _session;
    public IARSession Session
    {
      get { return _session; }
    }
 
    private void Start()
    {
      ARSessionFactory.SessionInitialized += SetupSession;
      SetupUserImage();
    }

    private void SetupUserImage() {

      string filePathImage = Path.Combine(Application.streamingAssetsPath, ImageTracker + ".jpg");

      IARReferenceImage imgToFind = ARReferenceImageFactory.Create
        (
          ImageTracker.ToLower(),
          filePathImage,
          0.25f
        );
      ImageDetectionManager.AddImage(imgToFind);
    }

    private void SetupSession(AnyARSessionInitializedArgs arg)
    {
      _session = arg.Session;
      _session.SessionFailed += args => Debug.Log(args.Error);
      _session.AnchorsAdded += OnAnchorsAdded;
      _session.AnchorsUpdated += OnAnchorsUpdated;
      _session.AnchorsRemoved += OnAnchorsRemoved;
    }

    private void OnAnchorsAdded(AnchorsArgs args)
    {
      foreach (var anchor in args.Anchors)
      {
        if (anchor.AnchorType != AnchorType.Image)
          continue;

        IARImageAnchor imageAnchor = (IARImageAnchor) anchor;
        string imageName = imageAnchor.ReferenceImage.Name;

        GameObject newObj = Instantiate(OHcontroller.ObjectHolder);
        newObj.SetActive(true);
        _detectedImages[anchor.Identifier] = newObj;

        UpdateObjectTransform(imageAnchor);
      }
    }

    private void OnAnchorsUpdated(AnchorsArgs args)
    {
      foreach (var anchor in args.Anchors)
      {
        if (!_detectedImages.ContainsKey(anchor.Identifier))
          continue;

        IARImageAnchor imageAnchor = (IARImageAnchor)anchor;
        UpdateObjectTransform(imageAnchor);
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

    private void UpdateObjectTransform(IARImageAnchor imageAnchor)
    {
      Guid identifier = imageAnchor.Identifier;

      _detectedImages[identifier].transform.position = imageAnchor.Transform.ToPosition();
      _detectedImages[identifier].transform.rotation = imageAnchor.Transform.ToRotation();

      Vector3 localScale = _detectedImages[identifier].transform.localScale;
      localScale.x = localScale.y = localScale.z = imageAnchor.ReferenceImage.PhysicalSize.x;
      _detectedImages[identifier].transform.localScale = localScale;
    }
  }
}
