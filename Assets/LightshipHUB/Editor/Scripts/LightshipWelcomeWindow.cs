// Copyright 2022 Niantic, Inc. All Rights Reserved.

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if (UNITY_EDITOR)
using System.IO;
namespace Niantic.LightshipHub
{
  public class LightshipWelcomeWindow : EditorWindow
  {
    private static VisualElement _root;
    private ScrollView _templates;
    private ScrollView _projects;
    private ScrollView _aboutUs;
    private ScrollView _help;


    public static void ShowWindow()
    {
      var window = GetWindow<LightshipWelcomeWindow>();
      window.titleContent = new GUIContent("Welcome to Lightship Templates");
      window.minSize = new Vector2(970, 680);
      window.maxSize = new Vector2(970, 680);
      window.Show();
    }

    private void OnEnable()
    {
      VisualTreeAsset design = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/LightShipHUB/Editor/Scripts/LightshipWelcomeWindow.uxml");
      TemplateContainer structure = design.CloneTree();
      rootVisualElement.Add(structure);
      StyleSheet style = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/LightShipHUB/Editor/Scripts/LightshipWelcomeWindowStyles.uss");
      rootVisualElement.styleSheets.Add(style);
      _root = rootVisualElement;
      AddLogo();
      CreateMenu();
      CreateTemplates();
      CreateSampleProjects();
      CreateHelp();
    }

    private void AddLogo()
    {
    }

    public void ShowTemplates()
    {
      _templates.RemoveFromClassList("hide");
      _projects.AddToClassList("hide");
      _aboutUs.AddToClassList("hide");
      _help.AddToClassList("hide");
    }

    public void ShowProjects()
    {
      _templates.AddToClassList("hide");
      _projects.RemoveFromClassList("hide");
      _aboutUs.AddToClassList("hide");
      _help.AddToClassList("hide");
    }

    public void ShowAboutUs()
    {
      _templates.AddToClassList("hide");
      _projects.AddToClassList("hide");
      _aboutUs.RemoveFromClassList("hide");
      _help.AddToClassList("hide");
    }

    public void ShowHelp()
    {
      _templates.AddToClassList("hide");
      _projects.AddToClassList("hide");
      _aboutUs.AddToClassList("hide");
      _help.RemoveFromClassList("hide");
    }

    private string GetVersions()
    {
      //Read the text from directly from the test.txt file
      string versions = "Versions: \n";
      StreamReader HUBVersion_reader = new StreamReader("Assets/LightshipHUB/VERSION"); 
      versions += "HUB "+HUBVersion_reader.ReadToEnd()+"\n";
      HUBVersion_reader.Close();

      StreamReader ARDKVersion_reader = new StreamReader("Assets/ARDK/VERSION"); 
      versions += "ARDK "+ARDKVersion_reader.ReadToEnd();
      ARDKVersion_reader.Close();
      return versions;

    }

    private void CreateMenu()
    {
      rootVisualElement.Query<Label>("version").First().text = GetVersions();
      
      Label templatesBtn = rootVisualElement.Query<Label>("menuTemplates").First();
      templatesBtn.RegisterCallback<MouseDownEvent>(evt => {
        rootVisualElement.Query<Label>("menuTemplates").First().AddToClassList("active");
        rootVisualElement.Query<Label>("menuProjects").First().RemoveFromClassList("active");
        rootVisualElement.Query<Label>("menuAbout").First().RemoveFromClassList("active");
        rootVisualElement.Query<Label>("menuHelp").First().RemoveFromClassList("active");
        ShowTemplates();
      });

      Label projectsBtn = rootVisualElement.Query<Label>("menuProjects").First();
      projectsBtn.RegisterCallback<MouseDownEvent>(evt => {
        rootVisualElement.Query<Label>("menuTemplates").First().RemoveFromClassList("active");
        rootVisualElement.Query<Label>("menuProjects").First().AddToClassList("active");
        rootVisualElement.Query<Label>("menuAbout").First().RemoveFromClassList("active");
        rootVisualElement.Query<Label>("menuHelp").First().RemoveFromClassList("active");
        ShowProjects();
      });

      Label aboutBtn = rootVisualElement.Query<Label>("menuAbout").First();
      aboutBtn.RegisterCallback<MouseDownEvent>(evt => {
        rootVisualElement.Query<Label>("menuTemplates").First().RemoveFromClassList("active");
        rootVisualElement.Query<Label>("menuProjects").First().RemoveFromClassList("active");
        rootVisualElement.Query<Label>("menuAbout").First().AddToClassList("active");
        rootVisualElement.Query<Label>("menuHelp").First().RemoveFromClassList("active");
        ShowAboutUs();
      });

      Label helpBtn = rootVisualElement.Query<Label>("menuHelp").First();
      helpBtn.RegisterCallback<MouseDownEvent>(evt => {
        rootVisualElement.Query<Label>("menuTemplates").First().RemoveFromClassList("active");
        rootVisualElement.Query<Label>("menuProjects").First().RemoveFromClassList("active");
        rootVisualElement.Query<Label>("menuAbout").First().RemoveFromClassList("active");
        rootVisualElement.Query<Label>("menuHelp").First().AddToClassList("active");
        ShowHelp();
      });

      _templates = _root.Query<ScrollView>("tutorials").First() as ScrollView;
      _projects = _root.Query<ScrollView>("projects").First() as ScrollView;
      _aboutUs = _root.Query<ScrollView>("aboutus").First() as ScrollView;
      _help = _root.Query<ScrollView>("help").First() as ScrollView;
    }

    private void CreateTemplates()
    {
      rootVisualElement.Query<Box>("tuto1_1").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_AnchorPlacement();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto1_2").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_AnchorPlacementWithoutPlanes();
        this.Close();
      });
      // rootVisualElement.Query<Box>("tuto1_2").First().RegisterCallback<MouseDownEvent>( evt=>{
      //     TemplateFactory.CreateTemplate_AnchorInteraction();
      //     this.Close();
      // });
      rootVisualElement.Query<Box>("tuto1_3").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_PlaneTracker(false);
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto1_4").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_ImageDetection();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto2_1").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_DepthTextureOcclusion();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto2_2").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_MeshOcclusion();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto2_3").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_RealtimeMeshing();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto2_4").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_MeshCollider();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto2_5").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_MeshingShaders();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto2_6").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_MeshGarden();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto2_7").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_AdvancedPhysics();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto2_8").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_CharacterController();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto3_1").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_SemanticSegmentation();
        this.Close();
      });
      // rootVisualElement.Query<Box>("tuto3_2").First().RegisterCallback<MouseDownEvent>( evt=>{
      //     TemplateFactory.CreateTemplate_ObjectMasking();
      //     this.Close();
      // });
      rootVisualElement.Query<Box>("tuto3_3").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_OptimizedObjectMasking();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto4_1").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_SharedObjectInteraction();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto5_1").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_VPSCoverage();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto5_2").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_VPSCoverageList();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto5_3").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_WayspotAnchors();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto5_4").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_LeaveMessages();
        this.Close();
      });
    }

    public void CreateSampleProjects()
    {
      rootVisualElement.Query<Box>("project_ARHockey").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.OpenSampleProject_ARHockey();
        this.Close();
      });
    }

    public void CreateHelp()
    {
      rootVisualElement.Query<Label>("learn_more").First().RegisterCallback<MouseDownEvent>(evt => {
        Application.OpenURL("https://lightship.dev/");
      });

      rootVisualElement.Query<Box>("help_iosbuild").First().RegisterCallback<MouseDownEvent>(evt => {
        Application.OpenURL("https://lightship.dev/docs/building_ios.html");
      });

      rootVisualElement.Query<Box>("help_androidbuild").First().RegisterCallback<MouseDownEvent>(evt => {
        Application.OpenURL("https://lightship.dev/docs/building_android.html");
      });

      rootVisualElement.Query<Box>("help_gettingstarted").First().RegisterCallback<MouseDownEvent>(evt => {
        Application.OpenURL("https://lightship.dev/docs/getting_started.html");
      });

      rootVisualElement.Query<Box>("help_documentation").First().RegisterCallback<MouseDownEvent>(evt => {
        Application.OpenURL("https://lightship.dev/account/documentation");
      });
    }

    public void SendMouseUpEventTo(IEventHandler handler)
    {
      using (var mouseUp = MouseUpEvent.GetPooled(new Event()))
      {
        mouseUp.target = handler;
        handler.SendEvent(mouseUp);
      }
    }
  }
}
#endif