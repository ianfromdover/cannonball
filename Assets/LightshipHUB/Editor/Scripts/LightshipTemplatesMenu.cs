// Copyright 2022 Niantic, Inc. All Rights Reserved.

using UnityEngine;
using UnityEditor;

#if (UNITY_EDITOR)
namespace Niantic.LightshipHub
{
  public class LightshipTemplatesMenu
  {
    /// Welcome.
    [MenuItem("Lightship/Lightship Hub/🚀 Welcome", false, 0)]
    private static void OpenHub()
    {
      LightshipWelcomeWindow.ShowWindow();
    }

    /// Helper window.
    [MenuItem("Lightship/Lightship Hub/Configuration Helper Window", false, 1)]
    private static void OpenHelperWindow()
    {
      LightshipHelperWindow.ShowHelperWindow();
    }

    /// AR Fundamentals Templates.
    [MenuItem("Lightship/Lightship Hub/Templates/AR Fundamentals/Object Placement", false, 50)]
    public static void Template_AnchorPlacement()
    {
      TemplateFactory.CreateTemplate_AnchorPlacement();
    }
    [MenuItem("Lightship/Lightship Hub/Templates/AR Fundamentals/Object Placement Without Planes", false, 51)]
    public static void Template_AnchorPlacementWithoutPlanes()
    {
      TemplateFactory.CreateTemplate_AnchorPlacementWithoutPlanes();
    }
    // [MenuItem("Lightship/Lightship Hub/Templates/AR Fundamentals/Object Interaction",false,51)]
    // public static void Template_AnchorInteraction()
    // {
    //     TemplateFactory.CreateTemplate_AnchorInteraction();
    // }
    [MenuItem("Lightship/Lightship Hub/Templates/AR Fundamentals/Plane Tracker", false, 52)]
    public static void Template_PlaneTracker()
    {
      TemplateFactory.CreateTemplate_PlaneTracker(false);
    }

    [MenuItem("Lightship/Lightship Hub/Templates/AR Fundamentals/Image Detection", false, 54)]
    public static void Template_ImageDetection()
    {
      TemplateFactory.CreateTemplate_ImageDetection();
    }

    /// Contextual Awareness Templates.
    [MenuItem("Lightship/Lightship Hub/Templates/Contextual Awareness/Texture Occlusion", false, 60)]
    public static void Template_DepthTextureOcclusion()
    {
      TemplateFactory.CreateTemplate_DepthTextureOcclusion();
    }

    [MenuItem("Lightship/Lightship Hub/Templates/Contextual Awareness/Mesh Occlusion", false, 61)]
    public static void Template_MeshOcclusion()
    {
      TemplateFactory.CreateTemplate_MeshOcclusion();
    }

    [MenuItem("Lightship/Lightship Hub/Templates/Contextual Awareness/Realtime Meshing", false, 62)]
    public static void Template_RealtimeMeshing()
    {
      TemplateFactory.CreateTemplate_RealtimeMeshing();
    }

    [MenuItem("Lightship/Lightship Hub/Templates/Contextual Awareness/Mesh Collider", false, 63)]
    public static void Template_MeshCollider()
    {
      TemplateFactory.CreateTemplate_MeshCollider();
    }

    [MenuItem("Lightship/Lightship Hub/Templates/Contextual Awareness/Meshing Shaders", false, 64)]
    public static void Template_MeshingShaders()
    {
      TemplateFactory.CreateTemplate_MeshingShaders();
    }

    [MenuItem("Lightship/Lightship Hub/Templates/Contextual Awareness/Mesh Garden", false, 65)]
    public static void Template_MeshGarden()
    {
      TemplateFactory.CreateTemplate_MeshGarden();
    }

    [MenuItem("Lightship/Lightship Hub/Templates/Contextual Awareness/Advanced Physics", false, 66)]
    public static void Template_AdvancedPhysics()
    {
      TemplateFactory.CreateTemplate_AdvancedPhysics();
    }

    [MenuItem("Lightship/Lightship Hub/Templates/Contextual Awareness/Character Controller", false, 67)]
    public static void Template_CharacterController()
    {
      TemplateFactory.CreateTemplate_CharacterController();
    }

    [MenuItem("Lightship/Lightship Hub/Templates/Contextual Awareness/Semantic Segmentation", false, 70)]
    public static void Template_SemanticSegmentation()
    {
      TemplateFactory.CreateTemplate_SemanticSegmentation();
    }
    // [MenuItem("Lightship/Lightship Hub/Templates/Contextual Awareness/Object Masking",false,71)]
    // public static void Template_ObjectMasking() 
    // {
    //     TemplateFactory.CreateTemplate_ObjectMasking();
    // }

    [MenuItem("Lightship/Lightship Hub/Templates/Contextual Awareness/Semantic Masking", false, 72)]
    public static void Template_OptimizedObjectMasking()
    {
      TemplateFactory.CreateTemplate_OptimizedObjectMasking();
    }

    /// Shared AR Templates.
    [MenuItem("Lightship/Lightship Hub/Templates/Shared AR/Shared Object Interaction", false, 80)]
    public static void Template_SharedObjectInteraction()
    {
      TemplateFactory.CreateTemplate_SharedObjectInteraction();
    }

    /// VPS Templates.
    [MenuItem("Lightship/Lightship Hub/Templates/Visual Positioning System/VPS Coverage", false, 90)]
    public static void Template_VPSCoverage()
    {
      TemplateFactory.CreateTemplate_VPSCoverage();
    }

    [MenuItem("Lightship/Lightship Hub/Templates/Visual Positioning System/VPS Coverage List", false, 91)]
    public static void Template_VPSCoverageList()
    {
      TemplateFactory.CreateTemplate_VPSCoverageList();
    }

    [MenuItem("Lightship/Lightship Hub/Templates/Visual Positioning System/Wayspot Anchors", false, 92)]
    public static void Template_WayspotAnchors()
    {
      TemplateFactory.CreateTemplate_WayspotAnchors();
    }

    [MenuItem("Lightship/Lightship Hub/Templates/Visual Positioning System/Leave Messages", false, 93)]
    public static void Template_LeaveMessages()
    {
      TemplateFactory.CreateTemplate_LeaveMessages();
    }


    [MenuItem("Lightship/Lightship Hub/Sample Projects/ARHockey", false, 203)]
    public static void SampleProject_ARHockey()
    {
      TemplateFactory.OpenSampleProject_ARHockey();
    }


    [MenuItem("Lightship/Lightship Hub/Help/Build on IOS", false, 533)]
    private static void HelpTheIOS()
    {
      Application.OpenURL("https://lightship.dev/docs/building_ios.html");
    }

    [MenuItem("Lightship/Lightship Hub/Help/Build on Android", false, 534)]
    private static void HelpTheAndroid()
    {
      Application.OpenURL("https://lightship.dev/docs/building_android.html");
    }

    [MenuItem("Lightship/Lightship Hub/Help/Lightship Documentation", false, 501)]
    private static void HelpTheLightship()
    {
      Application.OpenURL("https://lightship.dev/account/documentation");
    }

    [MenuItem("Lightship/Lightship Hub/Help/Getting Started", false, 532)]
    private static void HelpTheGettingStarted()
    {
      Application.OpenURL("https://lightship.dev/docs/getting_started.html");
    }
  }
}
#endif
