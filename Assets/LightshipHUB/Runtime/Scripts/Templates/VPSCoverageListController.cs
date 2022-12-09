// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

using Niantic.ARDK;
using Niantic.ARDK.LocationService;
using Niantic.ARDK.VirtualStudio.VpsCoverage;
using Niantic.ARDK.VPSCoverage;
using LocationServiceStatus = Niantic.ARDK.LocationService.LocationServiceStatus;

namespace Niantic.LightshipHub.Templates
{
  public class VPSCoverageListController : MonoBehaviour
  {
    public enum MapApps { GoogleMaps, AppleMaps }
    public RuntimeEnvironment RuntimeEnvironment = RuntimeEnvironment.LiveDevice;

    [Tooltip("GPS used in Editor")]
    // Default is the Ferry Building in San Francisco
    public LatLng SpoofLocation = new LatLng(37.79531921750984, -122.39360429639748);
    public MapApps MapApp = MapApps.GoogleMaps;
    [HideInInspector]
    public VpsCoverageResponses MockResponses;
    [HideInInspector]
    public ScrollRect ScrollList;
    [HideInInspector]
    public GameObject ItemPrefab;
    [HideInInspector]
    public RectTransform LoadMoreItemsThreshold;

    [Min(1)]
    public int LoadingBatchSize = 4;
    [HideInInspector]
    public Button RequestButton;
    [HideInInspector]
    public Text RequestButtonText;
    [HideInInspector]
    public Slider Slider;
    [HideInInspector]
    public Text QueryRadiusText;

    private ICoverageClient _coverageClient;
    private ILocationService _locationService;

    private readonly List<GameObject> _items = new List<GameObject>();
    private readonly List<CoverageArea> _CoverageAreas = new List<CoverageArea>();
    private GameObject _scrollListContent;

    private List<string> _targetIds = new List<string>();
    private int _nextItemToLoad = 0;

    private LatLng _requestLocation;
    private int _queryRadius;

    void Start()
    {
      // This is necessary for setting the user id associated with the current user. 
      // We strongly recommend generating and using User IDs. Accurate user information allows
      //  Niantic to support you in maintaining data privacy best practices and allows you to
      //  understand usage patterns of features among your users.  
      // ARDK has no strict format or length requirements for User IDs, although the User ID string
      //  must be a UTF8 string. We recommend avoiding using an ID that maps back directly to the
      //  user. So, for example, don’t use email addresses, or login IDs. Instead, you should
      //  generate a unique ID for each user. We recommend generating a GUID.
      // When the user logs out, clear ARDK's user id with ArdkGlobalConfig.ClearUserIdOnLogout

      //  Sample code:
      //  // GetCurrentUserId() is your code that gets a user ID string from your login service
      //  var userId = GetCurrentUserId(); 
      //  ArdkGlobalConfig.SetUserIdOnLogin(userId);

      _locationService = LocationServiceFactory.Create();
      _locationService.LocationUpdated += args =>
      {
        RequestButtonText.text = "Request for GPS location";
        RequestButton.interactable = true;
      };

      _coverageClient = CoverageClientFactory.Create(RuntimeEnvironment, MockResponses);
      _scrollListContent = ScrollList.content.gameObject;

#if UNITY_EDITOR
            var spoofService = (SpoofLocationService)_locationService;
            spoofService.SetLocation(SpoofLocation.Latitude, SpoofLocation.Longitude);
#elif UNITY_ANDROID
            MapApp = MapApps.GoogleMaps;
#elif UNITY_IOS
            MapApp = MapApps.AppleMaps;
#endif

      ScrollList.onValueChanged.AddListener(OnScroll);
      Slider.onValueChanged.AddListener(OnRadiusChanged);
      _queryRadius = (int)Slider.value;
      QueryRadiusText.text = _queryRadius.ToString();
    }

    private void Update()
    {
      if (_locationService.Status != LocationServiceStatus.Running)
        _locationService.Start();
    }

    public void RequestAreas(bool useSpoof)
    {
      ClearListContent();

      if (useSpoof)
      {
        _requestLocation = SpoofLocation;
        _coverageClient.RequestCoverageAreas(SpoofLocation, _queryRadius, ProcessAreasResult);
      }
      else
      {
        _requestLocation = new LatLng(_locationService.LastData);
        _coverageClient.RequestCoverageAreas(_locationService.LastData, _queryRadius, ProcessAreasResult);
      }
    }

    private void ProcessAreasResult(CoverageAreasResult areasResult)
    {
      if (CheckAreasResult(areasResult))
      {
        FillScrollList(areasResult);
        ResizeListContent();
      }
    }

    private void LoadNextTargetDetails(int batchSize)
    {
      int count = Math.Min(Math.Max(0, _targetIds.Count - _nextItemToLoad), batchSize);
      string[] targetIds = _targetIds.GetRange(_nextItemToLoad, count).ToArray();
      int itemIndex = _nextItemToLoad;
      _nextItemToLoad += batchSize;
      _coverageClient.RequestLocalizationTargets(targetIds, targetsResult => {
        if (targetsResult.Status != ResponseStatus.Success)
        {
          Debug.LogWarning
          (
          "LocalizationTarget request failed with status: " +
          targetsResult.Status +
          "\nSkipping batch"
          );

          return;
        }
        foreach (string targetId in targetIds)
        {
          LocalizationTarget target = targetsResult.ActivationTargets[targetId];
          FillTargetItem(itemIndex, target);
          itemIndex++;
        }
      });
    }

    private void OnRadiusChanged(float newRadius)
    {
      _queryRadius = (int)newRadius;
      QueryRadiusText.text = _queryRadius.ToString();
    }

    private bool IsUnloadedItemAboveThreshold()
    {
      return _nextItemToLoad < _targetIds.Count &&
          _items[_nextItemToLoad].transform.position.y > LoadMoreItemsThreshold.position.y;
    }

    private void OnScroll(Vector2 scrollDirection)
    {
      while (IsUnloadedItemAboveThreshold())
        LoadNextTargetDetails(LoadingBatchSize);
    }

    private void ClearListContent()
    {
      foreach (GameObject item in _items) Destroy(item);

      _CoverageAreas.Clear();
      _targetIds.Clear();
      _items.Clear();
      _nextItemToLoad = 0;
    }

    private bool CheckAreasResult(CoverageAreasResult areasResult)
    {
      if (areasResult.Status != ResponseStatus.Success)
      {
        Debug.LogWarning("CoverageAreas request failed with status: " + areasResult.Status);
        return false;
      }

      if (areasResult.Areas.Length == 0)
      {
        Debug.Log
        (
        "No areas found at " +
        _locationService.LastData.Coordinates +
        " with radius " +
        _queryRadius
        );

        return false;
      }

      return true;
    }

    private void FillScrollList(CoverageAreasResult result)
    {
      _CoverageAreas.AddRange(result.Areas.ToList());

      _CoverageAreas.Sort
      (
          (a, b) => a.Centroid.Distance(_requestLocation)
          .CompareTo(b.Centroid.Distance(_requestLocation))
      );

      foreach (var area in _CoverageAreas)
      {
        foreach (var target in area.LocalizationTargetIdentifiers)
        {
          _targetIds.Add(target);
          GameObject newTargetItem = Instantiate(ItemPrefab, _scrollListContent.transform, false);
          if (area.LocalizabilityQuality == CoverageArea.Localizability.EXPERIMENTAL)
          {
            newTargetItem.GetComponent<Image>().color = new Color(230f / 255f, 200f / 255f, 190f / 255f);
            newTargetItem.transform.Find("Image/Experimental").gameObject.SetActive(true);

          }

          _items.Add(newTargetItem);
        }
      }
    }

    private void ResizeListContent()
    {
      VerticalLayoutGroup layout = _scrollListContent.GetComponent<VerticalLayoutGroup>();
      RectTransform contentTransform = _scrollListContent.GetComponent<RectTransform>();
      float itemHeight = ItemPrefab.GetComponent<RectTransform>().sizeDelta.y;
      contentTransform.sizeDelta = new Vector2(
          contentTransform.sizeDelta.x,
          layout.padding.top + _scrollListContent.transform.childCount * (layout.spacing + itemHeight)
      );

      // Scroll all the way up
      contentTransform.anchoredPosition = new Vector2(0, Int32.MinValue);
    }

    private void FillTargetItem(int itemIndex, LocalizationTarget target)
    {
      Transform itemTransform = _items[itemIndex].transform;
      itemTransform.name = target.Name;

      Transform image = itemTransform.Find("Image");
      target.DownloadImage(downLoadedImage => image.GetComponent<RawImage>().texture = downLoadedImage);

      Transform title = itemTransform.Find("Info/Title");
      title.GetComponent<Text>().text = target.Name;

      Transform distance = itemTransform.Find("Info/Distance");
      double distanceInM = target.Center.Distance(_requestLocation);
      distance.GetComponent<Text>().text = "Distance: <b>" + distanceInM.ToString("N0") + "</b> m";

      Transform button = itemTransform.Find("Info/Button");
      button.GetComponent<Button>()
          .onClick.AddListener
          (delegate { OpenRouteInMapApp(_locationService.LastData.Coordinates, target.Center); });
    }

    private void OpenRouteInMapApp(LatLng from, LatLng to)
    {
      StringBuilder sb = new StringBuilder();

      if (MapApp == MapApps.GoogleMaps)
      {
        sb.Append("https://www.google.com/maps/dir/?api=1&origin=");
        sb.Append(from.Latitude);
        sb.Append("+");
        sb.Append(from.Longitude);
        sb.Append("&destination=");
        sb.Append(to.Latitude);
        sb.Append("+");
        sb.Append(to.Longitude);
        sb.Append("&travelmode=walking");
      }
      else if (MapApp == MapApps.AppleMaps)
      {
        sb.Append("http://maps.apple.com/?saddr=");
        sb.Append(from.Latitude);
        sb.Append("+");
        sb.Append(from.Longitude);
        sb.Append("&daddr=");
        sb.Append(to.Latitude);
        sb.Append("+");
        sb.Append(to.Longitude);
        sb.Append("&dirflg=w");
      }

      Application.OpenURL(sb.ToString());
    }
  }
}
