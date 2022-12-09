// Copyright 2022 Niantic, Inc. All Rights Reserved.

using UnityEngine;

using Niantic.ARDK.VirtualStudio.AR.Mock;

namespace Niantic.LightshipHub.Templates
{
  public class ObjectMasking : MonoBehaviour
  {
    [HideInInspector]
    public GameObject Holder;
    public MockSemanticLabel.ChannelName ChannelType;

    void Awake()
    {
      string layerName = ChannelType.ToString();
      if (LayerMask.NameToLayer(layerName) < 0) 
      {
        Debug.LogWarning($"The layer {layerName} does not exist. Please open the Semantic Masking Template from the Lightship Hub menu to create it, or do it manually.");
      } else 
      {
        SetLayerToGameObject(this.gameObject, layerName);
      }

      bool hasObject = false;
      if (Holder)
      {
        foreach (Transform child in Holder.transform)
        {
          hasObject = true;
          break;
        }
      }
      else
      {
        hasObject = true;
      }

      if (hasObject)
      {
        ObjectMaskingController controller = (ObjectMaskingController)GameObject.FindObjectOfType(typeof(ObjectMaskingController));
        controller.AllChannels.Add(ChannelType);
      }
    }
    private static void SetLayerToGameObject(GameObject obj, string layerName)
    {
      if (obj == null) return;
      obj.layer = LayerMask.NameToLayer(layerName);

      foreach (Transform child in obj.transform)
      {
        if (null == child) continue;
        SetLayerToGameObject(child.gameObject, layerName);
      }
    }
  }
}