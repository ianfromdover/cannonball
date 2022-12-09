// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Niantic.LightshipHub.Templates
{
  public class MessagePanels : MonoBehaviour
  {
    [HideInInspector]
    public GameObject LeaveMessagePanel, FindMessagePanel;
    [HideInInspector]
    public InputField InputMessage;
    [HideInInspector] 
    public Text Message;
    [HideInInspector] 
    public Button PlaceButton;

    void Update()
    {
      PlaceButton.interactable = !InputMessage.text.Equals("");
    }
  }
}
