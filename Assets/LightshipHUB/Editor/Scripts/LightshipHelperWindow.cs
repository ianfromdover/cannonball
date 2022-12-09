// Copyright 2022 Niantic, Inc. All Rights Reserved.

using UnityEngine;
using UnityEditor;

using Niantic.ARDK.Configuration.Authentication;

#if (UNITY_EDITOR)
namespace Niantic.LightshipHub
{
  public class LightshipHelperWindow : EditorWindow
  {
    private string _lightshipKey = "";

    public static void ShowHelperWindow()
    {
      GetWindow<LightshipHelperWindow>("Configuration Helper");
    }

    void OnGUI()
    {
      GUILayout.Label("Setup Lightship", EditorStyles.boldLabel);

      ArdkAuthConfig oldAuth = AssetDatabase.LoadAssetAtPath<ArdkAuthConfig>("Assets/LightshipHub/Resources/ARDK/ArdkAuthConfig.asset");
      ArdkAuthConfig auth = AssetDatabase.LoadAssetAtPath<ArdkAuthConfig>("Assets/Resources/ARDK/ArdkAuthConfig.asset");
      string oldKey = "";

      if (auth == null) {
        auth = ScriptableObject.CreateInstance<ArdkAuthConfig>();
        AssetDatabase.CreateFolder("Assets", "Resources");
        AssetDatabase.CreateFolder("Assets/Resources", "ARDK");
        AssetDatabase.CreateAsset(auth, "Assets/Resources/ARDK/ArdkAuthConfig.asset");
      }

      // Save old key and delete old ArdkAuthConfig
      if (oldAuth != null) 
      {
        SerializedObject sOldObject = new SerializedObject(oldAuth);
        SerializedProperty sOldProperty = sOldObject.FindProperty("_apiKey");
        oldKey = sOldProperty.stringValue;
        AssetDatabase.DeleteAsset("Assets/LightshipHub/Resources");
      }

      SerializedObject sObject = new SerializedObject(auth);
      SerializedProperty sProperty = sObject.FindProperty("_apiKey");
      string currentKey = sProperty.stringValue;

      // Set old key to new ArdkAuthConfig
      if (!oldKey.Equals("") && currentKey.Equals("")) 
      {
        sProperty.stringValue = oldKey;
        sObject.ApplyModifiedProperties();
      }

      _lightshipKey = EditorGUILayout.TextField("API Key", _lightshipKey);
      GUILayout.Label("Current API Key: " + sProperty.stringValue, EditorStyles.label);

      if (GUILayout.Button("Setup"))
      {
        if (!_lightshipKey.Equals(""))
        {
          sProperty.stringValue = _lightshipKey;
          sObject.ApplyModifiedProperties();
          EditorUtility.DisplayDialog("Lightship", "Lightship has been set correctly", "Ok");
        }
        else
        {
          EditorUtility.DisplayDialog("Lightship", "Insert a valid API Key and try again", "Ok");
        }
      }
    }
  }
}
#endif
