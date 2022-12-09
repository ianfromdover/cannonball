// Copyright 2022 Niantic, Inc. All Rights Reserved.

#if (UNITY_EDITOR)
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

using Niantic.ARDK.Extensions;
using Niantic.ARDK.Extensions.Permissions;
using Niantic.ARDK.Utilities.Permissions;
using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.VirtualStudio.AR.Mock;
using Niantic.LightshipHub.Tools;

namespace Niantic.LightshipHub
{
  public class LightshipCommon
  {
    public static GameObject _target = null;
    public static Camera _camera = null;
    public static GameObject _objectHolder;
    public static FeaturePreloadManager _preloadManager;
    public static PreloadController _preloadController;
    public static GameObject _ARSceneManager;

    public enum MockupKind { Interior, Exterior };
    public enum Model3D { BALLOON, CAR, PAPYRUS, GIFT, SKYROCKET, MUSHROOM };
    
    public static GameObject Target
    {
      get { return _target; }
      set { _target = value; }
    }

    public static Camera Camera
    {
      get { return _camera; }
    }

    public static GameObject ObjectHolder
    {
      get { return _objectHolder; }
      set { _objectHolder = value; }
    }

    public static FeaturePreloadManager PreloadManager
    {
      get { return _preloadManager; }
      set { _preloadManager = value; }
    }

    public static PreloadController PreloadController
    {
      get { return _preloadController; }
      set { _preloadController = value; }
    }

    public static GameObject ARSceneManager
    {
      get { return _ARSceneManager; }
      set { _ARSceneManager = value; }
    }

    public static Scene GetScene()
    {
      return EditorSceneManager.GetActiveScene();
    }

    public static bool CheckARDK()
    {
      ARSessionManager obj = (ARSessionManager)GameObject.FindObjectOfType(typeof(ARSessionManager));
      if (obj != null) EditorUtility.DisplayDialog("ARDK already in project", "You already have ARDK's components in your project. Please create a new scene", "Ok");
      return (obj != null);
    }

    public static GameObject CheckSceneObjectComponent<T>(string goName) where T : Component
    {
      T obj = (T)GameObject.FindObjectOfType(typeof(T));

      if (obj == null)
      {
        GameObject newObj = CreateSceneObject(goName);
        AddComponentToGameObject<T>(newObj);
        return newObj;
      }
      else
      {
        return obj.gameObject;
      }
    }

    public static GameObject Change3DModel(Model3D model3D)
    {
      GameObject obj = GameObject.Find("[REPLACE ME]");
      Transform parent = obj.transform.parent;
      string assetPath = "Assets/LightshipHUB/Runtime/Models/" + model3D.ToString() + ".FBX";
      GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
      GameObject newObj = UnityEngine.Object.Instantiate(model, parent);
      newObj.name = "[REPLACE ME]";
      UnityEngine.Object.DestroyImmediate(obj);
      return newObj;
    }

    public static GameObject CreateSceneObject(string goName, GameObject parentGO = null)
    {
      GameObject obj = new GameObject(goName);

      if (parentGO) obj.transform.parent = parentGO.transform;

      return obj;
    }

    public static T AddComponentToGameObject<T>(GameObject go) where T : Component
    {
      T comp = go.GetComponent<T>();

      if (comp == null)
      {
        return (T)go.AddComponent(typeof(T));
      }
      else
      {
        return comp;
      }
    }

    public static T CheckPrefab<T>(string path) where T : Component
    {
      T obj = (T)GameObject.FindObjectOfType(typeof(T));

      if (obj == null)
      {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        return go.GetComponent<T>();
      }
      else
      {
        return obj;
      }
    }

    public static void SetupComponentProperty(Component component, string property, bool condition)
    {
      SerializedObject sObject = new SerializedObject(component);
      SerializedProperty sProperty = sObject.FindProperty(property);
      sProperty.boolValue = condition;
      sObject.ApplyModifiedProperties();
    }

    public static void SetupComponentProperty(Component component, string property, int num)
    {
      SerializedObject sObject = new SerializedObject(component);
      SerializedProperty sProperty = sObject.FindProperty(property);
      sProperty.intValue = num;
      sObject.ApplyModifiedProperties();
    }

    public static void SetupComponentProperty(Component component, string property, UnityEngine.Object obj)
    {
      SerializedObject sObject = new SerializedObject(component);
      SerializedProperty sProperty = sObject.FindProperty(property);
      sProperty.objectReferenceValue = obj;
      sObject.ApplyModifiedProperties();
    }

    public static void SetupComponentProperty(Component component, string property, string str)
    {
      SerializedObject sObject = new SerializedObject(component);
      SerializedProperty sProperty = sObject.FindProperty(property);
      sProperty.stringValue = str;
      sObject.ApplyModifiedProperties();
    }

    private static bool PropertyExists(SerializedProperty property, int start, int end, string value)
    {
      for (int i = start; i < end; i++)
      {
        SerializedProperty p = property.GetArrayElementAtIndex(i);

        if (p.stringValue.Equals(value))
        {
          return true;
        }
      }
      return false;
    }

    public static void CreateLayer(string layerName)
    {
      int maxLayers = 31;
      SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
      SerializedProperty layersProperty = tagManager.FindProperty("layers");

      if (!PropertyExists(layersProperty, 0, maxLayers, layerName))
      {
        SerializedProperty sp;
        // First 8 are reserved for Unity
        for (int i = 8, j = maxLayers; i < j; i++)
        {
          sp = layersProperty.GetArrayElementAtIndex(i);

          if (sp.stringValue == "")
          {
            sp.stringValue = layerName;
            tagManager.ApplyModifiedProperties();
            break;
          }
        }
      }
    }

    public static void SetLayerToGameObject(GameObject obj, string layerName)
    {
      if (obj == null) return;

      obj.layer = LayerMask.NameToLayer(layerName);
      foreach (Transform child in obj.transform)
      {
        if (null == child) continue;
        SetLayerToGameObject(child.gameObject, layerName);
      }
    }

    private static void SetMaterialToGameObject(GameObject obj, Material material)
    {
      if (obj == null) return;

      MeshRenderer mesh = obj.GetComponent<MeshRenderer>();
      if (mesh != null) mesh.sharedMaterial = material;

      foreach (Transform child in obj.transform)
      {
        if (null == child) continue;
        SetMaterialToGameObject(child.gameObject, material);
      }
    }
  
    public static GameObject SetupARSceneManager()
    {
      GameObject ARSceneManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ARDK/Extensions/Prefabs/ARSceneManager.prefab");
      GameObject ARSceneManager = (GameObject)PrefabUtility.InstantiatePrefab(ARSceneManagerPrefab);
      PrefabUtility.UnpackPrefabInstance(ARSceneManager.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
      _ARSceneManager = ARSceneManager;

      UnityEngine.Object.DestroyImmediate(ARSceneManager.transform.GetChild(0).gameObject);
      return ARSceneManager;
    }

    public static void SetupARCamera()
    {
      GameObject arCamera = CheckSceneObjectComponent<Camera>("ARSceneCamera");
      _camera = arCamera.GetComponent<Camera>();
      _camera.gameObject.transform.SetParent(ARSceneManager.transform);

      arCamera.tag = "MainCamera";
      AddComponentToGameObject<AudioListener>(arCamera);

      _camera.clearFlags = CameraClearFlags.SolidColor;
      _camera.backgroundColor = Color.black;
      _camera.depth = -1;

      ARCameraPositionHelper positionHelper = AddComponentToGameObject<ARCameraPositionHelper>(_camera.gameObject);
      positionHelper.Camera = _camera;

      ARRenderingManager renderingManager = AddComponentToGameObject<ARRenderingManager>(_camera.gameObject);
      SetupComponentProperty(renderingManager, "_camera", _camera);

      AddAndroidTools(ARDKPermission.Camera);
    }

    public static ARPlaneManager AddPlaneManager(GameObject go)
    {
      ARPlaneManager planeManager = AddComponentToGameObject<ARPlaneManager>(go);
      SetupComponentProperty(planeManager, "_detectedPlaneTypes", (int)PlaneDetection.Horizontal);

      return planeManager;
    }

    public static void AddPrefabToPlaneManager(ARPlaneManager planeManager)
    {
      GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LightshipHUB/Runtime/Prefabs/Tools/PlaneDotsPrefab.prefab");
      SetupComponentProperty(planeManager, "_planePrefab", obj);
    }

    public static PreloadController AddPreloadManager()
    {
      GameObject preloadPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LightshipHUB/Runtime/Prefabs/Tools/PreloadManager.prefab");
      GameObject preloadGameObject = (GameObject)PrefabUtility.InstantiatePrefab(preloadPrefab);
      _preloadController = preloadGameObject.GetComponent<PreloadController>();
      _preloadManager = preloadGameObject.GetComponent<FeaturePreloadManager>();
      return _preloadController;
    }

    public static ARSemanticSegmentationManager AddSemanticSegmentation(GameObject go)
    {
      ARSemanticSegmentationManager segmentationManager = AddComponentToGameObject<ARSemanticSegmentationManager>(go);
      SetupComponentProperty(segmentationManager, "_camera", _camera);
      return segmentationManager;
    }

    public static ARDepthManager AddDepthManager(GameObject go)
    {
      ARDepthManager depthManager = AddComponentToGameObject<ARDepthManager>(go);
      SetupComponentProperty(depthManager, "_camera", _camera);
      return depthManager;
    }

    public static void AddAndroidTools(ARDKPermission permission, GameObject go = null)
    {
      AndroidPermissionRequester req = AddComponentToGameObject<AndroidPermissionRequester>(go != null ? go : _ARSceneManager);
      SerializedObject so = new SerializedObject(req);
      SerializedProperty sp = so.FindProperty("_permissions");
      bool hasPermission = false;
      foreach (SerializedProperty perm in sp)
      {
        if (perm.intValue == (int)permission)
        {
          hasPermission = true;
          break;
        }
      }
      if (!hasPermission)
      {
        sp.InsertArrayElementAtIndex(0);
        sp.GetArrayElementAtIndex(0).intValue = (int)permission;
      }
      so.ApplyModifiedProperties();
    }

    /*public static GameObject AddMockupWorld(MockupKind world)
    {
      MockSceneConfiguration mock;
      if (world.Equals(MockupKind.Exterior))
      {
        mock = CheckPrefab<MockSceneConfiguration>("Assets/LightshipHUBMockEnviroments/ExteriorScene.prefab");
      }
      else
      {
        mock = CheckPrefab<MockSceneConfiguration>("Assets/LightshipHUBMockEnviroments/InteriorScene.prefab");
      }
      return mock.gameObject;
    }*/

    public static void ShowTarget()
    {
      if (_target != null) EditorGUIUtility.PingObject(_target);
    }
  }
}
#endif
