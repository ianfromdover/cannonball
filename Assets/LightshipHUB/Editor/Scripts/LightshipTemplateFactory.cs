// Copyright 2022 Niantic, Inc. All Rights Reserved.

#if (UNITY_EDITOR)
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEditor.Animations;
using System.IO;

using Niantic.ARDK.Extensions;
using Niantic.ARDK.Utilities.Permissions;
using Niantic.ARDK.Extensions.Meshing;
using Niantic.ARDK.VirtualStudio.AR.Mock;
using Niantic.ARDK.VirtualStudio.VpsCoverage;
using Niantic.ARDK.Utilities.Preloading;
using Niantic.LightshipHub.Templates;
using Niantic.LightshipHub.Tools;

namespace Niantic.LightshipHub
{
  public class TemplateFactory
  {
    private static GameObject _ARSceneManager
    {
      get
      {
        return LightshipCommon.ARSceneManager;
      }
    }

    private static GameObject _target
    {
      get
      {
        return LightshipCommon.Target;
      }
      set
      {
        LightshipCommon.Target = value;
      }
    }

    private static Camera _camera
    {
      get
      {
        return LightshipCommon.Camera;
      }
    }

    private static bool requestPreloadManager = true;

    public static GameObject CreateTemplate_AnchorPlacement()
    {
      if (LightshipCommon.CheckARDK()) return null;

      LightshipCommon.SetupARSceneManager();
      LightshipCommon.SetupARCamera();
      LightshipCommon.AddPlaneManager(_ARSceneManager);
      //LightshipCommon.AddMockupWorld(LightshipCommon.MockupKind.Interior);

      ObjectHolderController controller = LightshipCommon.CheckPrefab<ObjectHolderController>("Assets/LightshipHUB/Runtime/Prefabs/Templates/ARController.prefab");

      try
      {
        PrefabUtility.UnpackPrefabInstance(controller.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
      }
      catch { }

      controller.Camera = _camera;
      LightshipCommon.ObjectHolder = controller.ObjectHolder;

      LightshipCommon.Change3DModel(LightshipCommon.Model3D.GIFT);

      _target = GameObject.Find("[REPLACE ME]");
      LightshipCommon.ShowTarget();

      return controller.gameObject;
    }

    public static GameObject CreateTemplate_AnchorPlacementWithoutPlanes()
    {
      if (LightshipCommon.CheckARDK()) return null;

      GameObject controllerGO = CreateTemplate_AnchorPlacement();
      PlacementControllerWithoutPlanes controller = LightshipCommon.AddComponentToGameObject<PlacementControllerWithoutPlanes>(controllerGO);
      controller.OHcontroller = controllerGO.GetComponent<ObjectHolderController>();
      controller.MultipleInstances = true;

      UnityEngine.Object.DestroyImmediate(controllerGO.GetComponent<PlacementController>());
      UnityEngine.Object.DestroyImmediate(_ARSceneManager.GetComponent<ARPlaneManager>());

      _target = GameObject.Find("[REPLACE ME]");
      LightshipCommon.ShowTarget();

      return controllerGO;
    }
    
    // public static GameObject CreateTemplate_AnchorInteraction() 
    // {
    //   if (LightshipCommon.CheckARDK()) return null;

    //   GameObject controllerGO = CreateTemplate_AnchorPlacement();
    //   InteractionController controller = LightshipCommon.AddComponentToGameObject<InteractionController>(controllerGO);
    //   controller.OHcontroller = controllerGO.GetComponent<ObjectHolderController>();

    //   GameObject holder = controllerGO.GetComponent<ObjectHolderController>().ObjectHolder;
    //   ObjectAnimation anchorController = holder.GetComponent<ObjectAnimation>();

    //   ObjectInteraction objectAnimation = LightshipCommon.AddComponentToGameObject<ObjectInteraction>(holder);
    //   objectAnimation.InteractionController = controller;
    //   UnityEventTools.AddPersistentListener(objectAnimation.OnClick, anchorController.ScaleInOut);
    //   UnityEventTools.AddPersistentListener(objectAnimation.OnDistance, anchorController.Rotate);

    //   UnityEngine.Object.DestroyImmediate(controllerGO.GetComponent<PlacementController>());

    //   GameObject newObj = LightshipCommon.Change3DModel(LightshipCommon.Model3D.SKYROCKET);
    //   newObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

    //   _target = GameObject.Find("[REPLACE ME]");
    //   LightshipCommon.ShowTarget();

    //   return controllerGO;
    // }
    //
    public static GameObject CreateTemplate_PlaneTracker(bool preloadManagerNeeded)
    {
      if (LightshipCommon.CheckARDK()) return null;

      GameObject controllerGO = CreateTemplate_AnchorPlacement();
      PlaneTrackerController controller = LightshipCommon.AddComponentToGameObject<PlaneTrackerController>(controllerGO);
      controller.OHcontroller = controllerGO.GetComponent<ObjectHolderController>();

      ARPlaneManager planeManager = _ARSceneManager.GetComponent<ARPlaneManager>();
      controller.PlaneManager = planeManager;
      LightshipCommon.AddPrefabToPlaneManager(planeManager);

      UnityEngine.Object.DestroyImmediate(controllerGO.GetComponent<PlacementController>());

      GameObject newObj = LightshipCommon.Change3DModel(LightshipCommon.Model3D.CAR);
      newObj.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);

      _target = GameObject.Find("[REPLACE ME]");
      LightshipCommon.ShowTarget();

      if (preloadManagerNeeded) LightshipCommon.AddPreloadManager();

      return controllerGO;
    }

    public static GameObject CreateTemplate_CharacterController()
    {
      if (LightshipCommon.CheckARDK()) return null;

      LightshipCommon.SetupARSceneManager();
      LightshipCommon.SetupARCamera();
      LightshipCommon.AddDepthManager(_camera.gameObject);

      GameObject controllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LightshipHUB/Runtime/Prefabs/Templates/ARCharacterController.prefab");
      GameObject controllerGO = (GameObject)PrefabUtility.InstantiatePrefab(controllerPrefab);
      PrefabUtility.UnpackPrefabInstance(controllerGO, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

      ObjectHolderController controller = controllerGO.GetComponent<ObjectHolderController>();

      LightshipCommon.AddPreloadManager();
      LightshipCommon.PreloadManager.AddFeature(Feature.ContextAwareness);
      LightshipCommon.PreloadController.ARController = controllerGO;

      ARMeshManager arMesh = LightshipCommon.CheckPrefab<ARMeshManager>("Assets/ARDK/Extensions/Meshing/ARMesh.prefab");
      arMesh.UseInvisibleMaterial = true;
      arMesh.MeshPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ARDK/Extensions/Meshing/MeshColliderChunk.prefab");

      controller.Camera = _camera;
      LightshipCommon.ObjectHolder = controller.ObjectHolder;

      _target = GameObject.Find("[REPLACE ME]");
      LightshipCommon.ShowTarget();

      return controllerGO;
    }

    public static GameObject CreateTemplate_DepthTextureOcclusion()
    {
      if (LightshipCommon.CheckARDK()) return null;

      GameObject controllerGO = CreateTemplate_PlaneTracker(requestPreloadManager);
      LightshipCommon.PreloadManager.AddFeature(Feature.ContextAwareness);
      LightshipCommon.PreloadController.ARController = controllerGO;
      ARDepthManager depthManager = LightshipCommon.AddDepthManager(_camera.gameObject);
      DepthTextureController controller = LightshipCommon.AddComponentToGameObject<DepthTextureController>(controllerGO);
      controller.DepthManager = depthManager;

      return controllerGO;
    }

    public static GameObject CreateTemplate_ImageDetection()
    {
      if (LightshipCommon.CheckARDK()) return null;

      LightshipCommon.SetupARSceneManager();
      LightshipCommon.SetupARCamera();

      ObjectHolderController goController = LightshipCommon.CheckPrefab<ObjectHolderController>("Assets/LightshipHUB/Runtime/Prefabs/Templates/ImageDetectionController.prefab");

      try
      {
        PrefabUtility.UnpackPrefabInstance(goController.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
      }
      catch { }

      goController.Camera = _camera;
      LightshipCommon.ObjectHolder = goController.ObjectHolder;

      ImageDetectionController controller = goController.GetComponent<ImageDetectionController>();
      controller.OHcontroller = goController;
      controller.ImageTracker = "Yeti";

      ARImageDetectionManager imageDetectionManager = LightshipCommon.AddComponentToGameObject<ARImageDetectionManager>(_ARSceneManager);
      controller.ImageDetectionManager = imageDetectionManager;

      _target = GameObject.Find("[REPLACE ME]");
      LightshipCommon.ShowTarget();

      return controller.gameObject;
    }

    public static GameObject CreateTemplate_MeshOcclusion()
    {
      if (LightshipCommon.CheckARDK()) return null;

      GameObject controllerGO = CreateTemplate_PlaneTracker(requestPreloadManager);
      LightshipCommon.PreloadManager.AddFeature(Feature.ContextAwareness);
      LightshipCommon.PreloadController.ARController = controllerGO;
      MeshController controller = LightshipCommon.AddComponentToGameObject<MeshController>(controllerGO);
      controller.ARSessionManager = _ARSceneManager.GetComponent<ARSessionManager>();
      controller.WorldMeshMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/ARDK/Extensions/Meshing/Materials/MeshNormalFresnel.mat");

      ARMeshManager arMesh = LightshipCommon.CheckPrefab<ARMeshManager>("Assets/ARDK/Extensions/Meshing/ARMesh.prefab");
      controller.ARMeshManager = arMesh;
      arMesh.transform.parent = controller.transform;
      controller.InvisibleMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/ARDK/Extensions/Meshing/Materials/MeshInvisible.mat");

      return controllerGO;
    }

    public static GameObject CreateTemplate_MeshingShaders()
    {
      if (LightshipCommon.CheckARDK()) return null;

      LightshipCommon.SetupARSceneManager();
      LightshipCommon.SetupARCamera();

      MeshingShadersController controller = LightshipCommon.CheckPrefab<MeshingShadersController>("Assets/LightshipHUB/Runtime/Prefabs/Templates/ARMeshingShaderController.prefab");
      controller.ARSessionManager = _ARSceneManager.GetComponent<ARSessionManager>();
      ARMeshManager arMesh = LightshipCommon.CheckPrefab<ARMeshManager>("Assets/ARDK/Extensions/Meshing/ARMesh.prefab");
      controller.ARMeshManager = arMesh;

      LightshipCommon.AddPreloadManager();
      LightshipCommon.PreloadManager.AddFeature(Feature.ContextAwareness);
      LightshipCommon.PreloadController.ARController = controller.gameObject;

      _target = controller.gameObject;
      LightshipCommon.ShowTarget();

      return controller.gameObject;
    }

    public static GameObject CreateTemplate_RealtimeMeshing()
    {
      if (LightshipCommon.CheckARDK()) return null;

      GameObject controllerGO = CreateTemplate_MeshOcclusion();
      LightshipCommon.PreloadManager.AddFeature(Feature.ContextAwareness);
      LightshipCommon.PreloadController.ARController = controllerGO;
      ARMeshManager arMesh = LightshipCommon.CheckPrefab<ARMeshManager>("Assets/ARDK/Extensions/Meshing/ARMesh.prefab");
      GameObject meshPref = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ARDK/Extensions/Meshing/MeshColliderChunk.prefab");

      LightshipCommon.SetupComponentProperty(arMesh, "_meshPrefab", meshPref);

      MeshPlacementController meshPController = LightshipCommon.AddComponentToGameObject<MeshPlacementController>(controllerGO);
      meshPController.OHcontroller = controllerGO.GetComponent<ObjectHolderController>();

      UnityEngine.Object.DestroyImmediate(controllerGO.GetComponent<PlaneTrackerController>());
      UnityEngine.Object.DestroyImmediate(controllerGO.GetComponent<MeshController>().ARSessionManager.gameObject.GetComponent<ARPlaneManager>());

      GameObject newObj = LightshipCommon.Change3DModel(LightshipCommon.Model3D.MUSHROOM);
      newObj.transform.localPosition = Vector3.zero;
      newObj.transform.localScale = new Vector3(0.26f, 0.26f, 0.26f);

      _target = GameObject.Find("[REPLACE ME]");
      LightshipCommon.AddComponentToGameObject<MushroomAnimation>(_target);
      LightshipCommon.ShowTarget();

      return controllerGO;
    }

    public static GameObject CreateTemplate_MeshCollider()
    {
      if (LightshipCommon.CheckARDK()) return null;

      GameObject controllerGO = CreateTemplate_MeshOcclusion();
      LightshipCommon.PreloadManager.AddFeature(Feature.ContextAwareness);
      LightshipCommon.PreloadController.ARController = controllerGO;
      ARMeshManager arMesh = LightshipCommon.CheckPrefab<ARMeshManager>("Assets/ARDK/Extensions/Meshing/ARMesh.prefab");
      GameObject meshPref = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ARDK/Extensions/Meshing/MeshColliderChunk.prefab");

      LightshipCommon.SetupComponentProperty(arMesh, "_meshPrefab", meshPref);

      MeshColliderController colliderController = LightshipCommon.AddComponentToGameObject<MeshColliderController>(controllerGO);
      colliderController.OHcontroller = controllerGO.GetComponent<ObjectHolderController>();

      GameObject newObj = LightshipCommon.Change3DModel(LightshipCommon.Model3D.GIFT);
      newObj.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

      LightshipCommon.AddComponentToGameObject<Rigidbody>(controllerGO.transform.GetChild(0).gameObject);

      UnityEngine.Object.DestroyImmediate(controllerGO.GetComponent<PlaneTrackerController>());
      UnityEngine.Object.DestroyImmediate(controllerGO.GetComponent<MeshController>().ARSessionManager.gameObject.GetComponent<ARPlaneManager>());

      _target = GameObject.Find("[REPLACE ME]");
      LightshipCommon.ShowTarget();

      return controllerGO;
    }

    public static GameObject CreateTemplate_AdvancedPhysics()
    {
      if (LightshipCommon.CheckARDK()) return null;

      LightshipCommon.SetupARSceneManager();
      LightshipCommon.SetupARCamera();
      LightshipCommon.AddDepthManager(_camera.gameObject);

      ARMeshManager arMesh = LightshipCommon.CheckPrefab<ARMeshManager>("Assets/ARDK/Extensions/Meshing/ARMesh.prefab");
      arMesh.UseInvisibleMaterial = true;

      GameObject meshPref = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ARDK/Extensions/Meshing/MeshColliderChunk.prefab");
      LightshipCommon.SetupComponentProperty(arMesh, "_meshPrefab", meshPref);

      GameObject controllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LightshipHUB/Runtime/Prefabs/Templates/ARPhysicsController.prefab");
      GameObject controller = (GameObject)PrefabUtility.InstantiatePrefab(controllerPrefab);
      PrefabUtility.UnpackPrefabInstance(controller, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

      LightshipCommon.AddPreloadManager();
      LightshipCommon.PreloadManager.AddFeature(Feature.ContextAwareness);
      LightshipCommon.PreloadController.ARController = controller;

      _target = controller.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
      LightshipCommon.ShowTarget();

      return controller;
    }

    public static GameObject CreateTemplate_MeshGarden()
    {
      if (LightshipCommon.CheckARDK()) return null;

      LightshipCommon.SetupARSceneManager();
      LightshipCommon.SetupARCamera();

      MeshGardenController gardenController = LightshipCommon.CheckPrefab<MeshGardenController>("Assets/LightshipHUB/Runtime/Prefabs/Templates/ARGardenController.prefab");
      gardenController.Camera = _camera;

      MeshController meshController = gardenController.GetComponent<MeshController>();

      try
      {
        PrefabUtility.UnpackPrefabInstance(gardenController.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
      }
      catch { }

      LightshipCommon.AddPreloadManager();
      LightshipCommon.PreloadManager.AddFeature(Feature.ContextAwareness);
      LightshipCommon.PreloadController.ARController = gardenController.gameObject;

      ARMeshManager arMesh = LightshipCommon.CheckPrefab<ARMeshManager>("Assets/ARDK/Extensions/Meshing/ARMesh.prefab");
      GameObject meshPref = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ARDK/Extensions/Meshing/MeshColliderChunk.prefab");
      LightshipCommon.SetupComponentProperty(arMesh, "_meshPrefab", meshPref);

      meshController.ARMeshManager = arMesh;
      meshController.InvisibleMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/ARDK/Extensions/Meshing/Materials/MeshInvisible.mat");
      meshController.WorldMeshMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/ARDK/Extensions/Meshing/Materials/MeshNormalFresnel.mat");
      meshController.ARSessionManager = _ARSceneManager.GetComponent<ARSessionManager>();

      _target = GameObject.Find("[REPLACE ME]");
      LightshipCommon.ShowTarget();

      return gardenController.gameObject;
    }

    public static GameObject CreateTemplate_SemanticSegmentation()
    {
      if (LightshipCommon.CheckARDK()) return null;

      LightshipCommon.SetupARSceneManager();
      LightshipCommon.SetupARCamera();
      LightshipCommon.AddSemanticSegmentation(_camera.gameObject);

      //GameObject mockup = LightshipCommon.AddMockupWorld(LightshipCommon.MockupKind.Exterior);
      GameObject controllerGO = LightshipCommon.CheckSceneObjectComponent<SegmentationController>("ARController");
      _target = controllerGO;
      SegmentationController controller = controllerGO.GetComponent<SegmentationController>();

      GameObject canvas = LightshipCommon.CheckSceneObjectComponent<Canvas>("Canvas");
      canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
      LightshipCommon.AddComponentToGameObject<CanvasScaler>(canvas);
      canvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

      controller.ARSessionManager = _ARSceneManager.GetComponent<ARSessionManager>();
      controller.SemanticSegmentationManager = _camera.gameObject.GetComponent<ARSemanticSegmentationManager>();
      controller.Canvas = canvas.GetComponent<Canvas>();
      controller.CustomShader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/LightshipHUB/Runtime/Shaders/Segmentation.shader");

      int index = 0;
      controller.Segmentations = new SegmentationController.Segmentation[6];

      foreach (MockSemanticLabel.ChannelName channel in Enum.GetValues(typeof(MockSemanticLabel.ChannelName)))
      {
        if (channel == MockSemanticLabel.ChannelName.grass) continue;
        if (!File.Exists("Assets/LightshipHUB/Runtime/Textures/SegmentationExamples/" + channel.ToString().ToLower() + ".png")) continue;
        controller.Segmentations[index].ChannelType = channel;
        controller.Segmentations[index].Texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LightshipHUB/Runtime/Textures/SegmentationExamples/" + channel.ToString().ToLower() + ".png");
        index++;
      }

      LightshipCommon.ShowTarget();

      PreloadController preloader = LightshipCommon.AddPreloadManager();
      LightshipCommon.PreloadManager.AddFeature(Feature.ContextAwareness);
      preloader.ARController = controllerGO;

      return controllerGO;
    }

    public static GameObject CreateTemplate_ObjectMasking()
    {
      if (LightshipCommon.CheckARDK()) return null;

      LightshipCommon.SetupARSceneManager();
      LightshipCommon.SetupARCamera();
      LightshipCommon.AddSemanticSegmentation(_camera.gameObject);

      //GameObject mockup = LightshipCommon.AddMockupWorld(LightshipCommon.MockupKind.Exterior);
      GameObject controllerGO = LightshipCommon.CheckSceneObjectComponent<ObjectMaskingController>("ARController");
      GameObject prefabOH = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LightshipHUB/Runtime/Prefabs/Templates/MaskedObjectsHolder.prefab");
      GameObject objectsHolder = (GameObject)PrefabUtility.InstantiatePrefab(prefabOH, controllerGO.transform);
      PrefabUtility.UnpackPrefabInstance(objectsHolder.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
      _target = objectsHolder.transform.GetChild(0).GetChild(0).gameObject;

      ObjectMaskingController controller = controllerGO.GetComponent<ObjectMaskingController>();

      foreach (MockSemanticLabel.ChannelName channel in Enum.GetValues(typeof(MockSemanticLabel.ChannelName)))
      {
        LightshipCommon.CreateLayer(channel.ToString().ToLower());
      }

      _camera.cullingMask = (1 << 0 |
                          1 << 1 |
                          1 << 2 |
                          1 << 3 |
                          1 << 4 |
                          1 << 5);

      GameObject canvas = LightshipCommon.CheckSceneObjectComponent<Canvas>("Canvas");
      canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
      LightshipCommon.AddComponentToGameObject<CanvasScaler>(canvas);
      canvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

      controller.Camera = _camera;
      controller.ARSessionManager = _ARSceneManager.GetComponent<ARSessionManager>();
      controller.SemanticSegmentationManager = _camera.gameObject.GetComponent<ARSemanticSegmentationManager>();
      controller.Canvas = canvas.GetComponent<Canvas>();
      controller.CustomShader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/LightshipHUB/Runtime/Shaders/ObjectMasking.shader");

      GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LightshipHUB/Runtime/Prefabs/Templates/SegmentationCameras.prefab");
      foreach (Transform child in prefab.transform)
      {
        string layerName = child.name.Replace("_Camera", string.Empty);
        child.GetComponent<Camera>().cullingMask = 1 << LayerMask.NameToLayer(layerName);
      }
      GameObject cameras = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
      controller.SegmentationCamerasGO = cameras;

      LightshipCommon.ShowTarget();

      PreloadController preloader = LightshipCommon.AddPreloadManager();
      LightshipCommon.PreloadManager.AddFeature(Feature.ContextAwareness);
      preloader.ARController = controllerGO;

      return controllerGO;
    }

    public static GameObject CreateTemplate_OptimizedObjectMasking()
    {
      if (LightshipCommon.CheckARDK()) return null;

      GameObject controllerGO = CreateTemplate_ObjectMasking();
      ObjectMaskingController controller = controllerGO.GetComponent<ObjectMaskingController>();
      controller.CustomShader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/LightshipHUB/Runtime/Shaders/OptimizedObjectMasking.shader");

      return controllerGO;
    }

    public static GameObject CreateTemplate_SharedObjectInteraction()
    {
      if (LightshipCommon.CheckARDK()) return null;

      LightshipCommon.SetupARSceneManager();
      LightshipCommon.SetupARCamera();
      LightshipCommon.AddPlaneManager(_ARSceneManager);
      //LightshipCommon.AddMockupWorld(LightshipCommon.MockupKind.Interior);

      NetworkSessionManager nSession = LightshipCommon.AddComponentToGameObject<NetworkSessionManager>(_ARSceneManager);
      ARNetworkingManager ARnetworking = LightshipCommon.AddComponentToGameObject<ARNetworkingManager>(_ARSceneManager);

      SharedSession controller = LightshipCommon.CheckPrefab<SharedSession>("Assets/LightshipHUB/Runtime/Prefabs/Templates/SharedARController.prefab");

      try
      {
        PrefabUtility.UnpackPrefabInstance(controller.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
      }
      catch { }

      LightshipCommon.SetupComponentProperty(_ARSceneManager.GetComponent<ARSessionManager>(), "_manageUsingUnityLifecycle", false);
      LightshipCommon.SetupComponentProperty(_ARSceneManager.GetComponent<ARSessionManager>(), "_useWithARNetworkingSession", true);

      LightshipCommon.SetupComponentProperty(nSession, "_manageUsingUnityLifecycle", false);
      LightshipCommon.SetupComponentProperty(nSession, "_inputField", GameObject.Find("SessionIDField"));
      LightshipCommon.SetupComponentProperty(nSession, "_useWithARNetworkingSession", true);

      controller._camera = _camera;
      controller._arManager = ARnetworking;

      ARnetworking.enabled = false;

      PreloadController preloader = LightshipCommon.AddPreloadManager();
      LightshipCommon.PreloadManager.AddFeature(Feature.Dbow);
      preloader.ARController = controller.gameObject;

      _target = GameObject.Find("[REPLACE ME]");
      LightshipCommon.ShowTarget();

      return controller.gameObject;
    }

    public static GameObject CreateTemplate_VPSCoverage()
    {
      if (LightshipCommon.CheckARDK()) return null;

      GameObject controller = LightshipCommon.CreateSceneObject("Controller");
      VPSCoverageController vpsCoverage = LightshipCommon.AddComponentToGameObject<VPSCoverageController>(controller);
      vpsCoverage.MockResponses = AssetDatabase.LoadAssetAtPath<VpsCoverageResponses>("Assets/ARDK/VirtualStudio/VpsCoverage/VPSCoverageResponses.asset");

      LightshipCommon.AddAndroidTools(ARDKPermission.Camera, controller);

      GameObject canvas = LightshipCommon.CheckSceneObjectComponent<Canvas>("Canvas");
      canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
      CanvasScaler canvasScaler = LightshipCommon.AddComponentToGameObject<CanvasScaler>(canvas);
      canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
      LightshipCommon.AddComponentToGameObject<GraphicRaycaster>(canvas);

      GameObject targetImage = LightshipCommon.CreateSceneObject("TargetImage");
      targetImage.transform.parent = canvas.transform;
      RawImage rawImage = LightshipCommon.AddComponentToGameObject<RawImage>(targetImage);
      vpsCoverage.TargetImage = rawImage;
      rawImage.rectTransform.anchoredPosition = new Vector2(15, -470);
      rawImage.rectTransform.sizeDelta = new Vector2(777, 662);

      LightshipCommon.SetLayerToGameObject(canvas, "UI");

      return controller;
    }

    public static GameObject CreateTemplate_VPSCoverageList()
    {
      if (LightshipCommon.CheckARDK()) return null;

      VPSCoverageListController controller = LightshipCommon.CheckPrefab<VPSCoverageListController>("Assets/LightshipHUB/Runtime/Prefabs/Templates/ListController.prefab");
      controller.MockResponses = AssetDatabase.LoadAssetAtPath<VpsCoverageResponses>("Assets/ARDK/VirtualStudio/VpsCoverage/VPSCoverageResponses.asset");

      return controller.gameObject;
    }

    public static GameObject CreateTemplate_WayspotAnchors()
    {
      if (LightshipCommon.CheckARDK()) return null;

      GameObject controllerGO = CreateTemplate_AnchorPlacement();
      WayspotAnchorTemplateController controller = LightshipCommon.AddComponentToGameObject<WayspotAnchorTemplateController>(controllerGO);
      controller.OHcontroller = controllerGO.GetComponent<ObjectHolderController>();

      ARPlaneManager planeManager = _ARSceneManager.GetComponent<ARPlaneManager>();

      UnityEngine.Object.DestroyImmediate(controllerGO.GetComponent<PlacementController>());

      GameObject canvasPref = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LightshipHUB/Runtime/Prefabs/Templates/WayspotAnchorsCanvas.prefab");
      GameObject canvas = (GameObject)PrefabUtility.InstantiatePrefab(canvasPref);

      foreach (Transform child in canvas.transform)
      {
        switch (child.name)
        {
          case "Status Panel":
            foreach (Transform secondChild in child)
            {
              if (secondChild.name == "Status Log") controller.StatusLog = secondChild.GetComponent<Text>();
              else if (secondChild.name == "Localization Status") controller.LocalizationStatus = secondChild.GetComponent<Text>();
            }
            break;
          case "Restart Service Button":
            UnityEventTools.AddPersistentListener(child.GetComponent<Button>().onClick, controller.RestartWayspotAnchorService);
            break;
          case "Pause Session Button":
            UnityEventTools.AddPersistentListener(child.GetComponent<Button>().onClick, controller.PauseARSession);
            break;
          case "Resume Session Button":
            UnityEventTools.AddPersistentListener(child.GetComponent<Button>().onClick, controller.ResumeARSession);
            break;
          case "Clear Anchors Button":
            UnityEventTools.AddPersistentListener(child.GetComponent<Button>().onClick, controller.ClearAnchorGameObjects);
            break;
          case "Load Anchors Button":
            UnityEventTools.AddPersistentListener(child.GetComponent<Button>().onClick, controller.LoadWayspotAnchors);
            break;
          case "Save Anchors Button":
            UnityEventTools.AddPersistentListener(child.GetComponent<Button>().onClick, controller.SaveWayspotAnchors);
            break;
          default:
            break;
        }
      }

      _target = GameObject.Find("[REPLACE ME]");
      LightshipCommon.ShowTarget();

      return controllerGO;
    }

    public static GameObject CreateTemplate_LeaveMessages()
    {
      if (LightshipCommon.CheckARDK()) return null;

      LightshipCommon.SetupARSceneManager();
      LightshipCommon.SetupARCamera();
      LightshipCommon.AddPlaneManager(_ARSceneManager);

      ObjectHolderController OHcontroller = LightshipCommon.CheckPrefab<ObjectHolderController>("Assets/LightshipHUB/Runtime/Prefabs/Templates/ARController.prefab");

      try
      {
        PrefabUtility.UnpackPrefabInstance(OHcontroller.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
      }
      catch { }

      OHcontroller.Camera = _camera;
      LightshipCommon.ObjectHolder = OHcontroller.ObjectHolder;

      LeaveMessagesTemplateController controller = LightshipCommon.AddComponentToGameObject<LeaveMessagesTemplateController>(OHcontroller.gameObject);
      controller.OHcontroller = OHcontroller;

      UnityEngine.Object.DestroyImmediate(OHcontroller.GetComponent<PlacementController>());

      GameObject canvasPref = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LightshipHUB/Runtime/Prefabs/Templates/WayspotAnchorsCanvas.prefab");
      GameObject messagePanelsPref = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LightshipHUB/Runtime/Prefabs/Templates/MessagePanels.prefab");
      GameObject canvas = (GameObject)PrefabUtility.InstantiatePrefab(canvasPref);
      GameObject messagePanels = (GameObject)PrefabUtility.InstantiatePrefab(messagePanelsPref, canvas.transform);

      controller.MessagePanels = messagePanels.GetComponent<MessagePanels>();
      UnityEventTools.AddPersistentListener(controller.MessagePanels.PlaceButton.onClick, controller.PlaceAnchor);

      foreach (Transform child in canvas.transform)
      {
        switch (child.name)
        {
          case "Status Panel":
            foreach (Transform secondChild in child)
            {
              if (secondChild.name == "Status Log") controller.StatusLog = secondChild.GetComponent<Text>();
              else if (secondChild.name == "Localization Status") controller.LocalizationStatus = secondChild.GetComponent<Text>();
            }
            break;
          case "Restart Service Button":
            UnityEventTools.AddPersistentListener(child.GetComponent<Button>().onClick, controller.RestartWayspotAnchorService);
            break;
          case "Pause Session Button":
            UnityEventTools.AddPersistentListener(child.GetComponent<Button>().onClick, controller.PauseARSession);
            break;
          case "Resume Session Button":
            UnityEventTools.AddPersistentListener(child.GetComponent<Button>().onClick, controller.ResumeARSession);
            break;
          case "Clear Anchors Button":
            UnityEventTools.AddPersistentListener(child.GetComponent<Button>().onClick, controller.ClearMessages);
            break;
          case "Load Anchors Button":
            UnityEventTools.AddPersistentListener(child.GetComponent<Button>().onClick, controller.LoadMessages);
            break;
          case "Save Anchors Button":
            UnityEventTools.AddPersistentListener(child.GetComponent<Button>().onClick, controller.SaveMessages);
            break;
          default:
            break;
        }
      }

      GameObject newObj = LightshipCommon.Change3DModel(LightshipCommon.Model3D.PAPYRUS);
      newObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

      _target = GameObject.Find("[REPLACE ME]");

      Animator animator = _target.AddComponent<Animator>();
      animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/LightshipHUB/Runtime/AnimatorControllers/Papyrus.controller");

      LightshipCommon.ShowTarget();

      return controller.gameObject;
    }

    public static void OpenSampleProject_ARHockey()
    {
      EditorSceneManager.OpenScene("Assets/LightshipHUB/SampleProjects/ARHockey.unity");
    }
  }
}
#endif