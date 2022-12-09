// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;

using UnityEngine;

namespace Niantic.ARDK.VirtualStudio.AR
{
  internal static class _MockCameraConfiguration
  {
    // Camera control keys
    private const string FPS_KEY = "ARDK_Mock_FPS";
    private const string MOVESPEED_KEY = "ARDK_Mock_Movespeed";
    private const string LOOKSPEED_KEY = "ARDK_Mock_Lookspeed";
    private const string SCROLLDIRECTION_KEY = "ARDK_Mock_ScrollDirection";

    private const int _DefaultFps = 30;
    private const float _DefaultMoveSpeed = 10f;
    private const int _DefaultLookSpeed = 180;
    private const int _DefaultScrollDirection = -1;

    internal static int FPS
    {
      get { return PlayerPrefs.GetInt(FPS_KEY, _DefaultFps); }
      set { PlayerPrefs.SetInt(FPS_KEY, value);}
    }

    internal static float MoveSpeed
    {
      get { return PlayerPrefs.GetFloat(MOVESPEED_KEY, _DefaultMoveSpeed); }
      set { PlayerPrefs.SetFloat(MOVESPEED_KEY, value);}
    }

    internal static int LookSpeed
    {
      get { return PlayerPrefs.GetInt(LOOKSPEED_KEY, _DefaultLookSpeed); }
      set { PlayerPrefs.SetInt(LOOKSPEED_KEY, value);}
    }

    internal static int ScrollDirection
    {
      get { return PlayerPrefs.GetInt(SCROLLDIRECTION_KEY, _DefaultScrollDirection); }
      set { PlayerPrefs.SetInt(SCROLLDIRECTION_KEY, value);}
    }

    // Cannot use Unity's Screen properties in Editor due to this bug: Unity Issue-598763
    private static int _correctedScreenWidth;
    internal static int CorrectedScreenWidth
    {
      get
      {
        return _correctedScreenWidth;
      }
      set
      {
        _correctedScreenWidth = value;
      }
    }

    private static int _correctedScreenHeight;

    internal static int CorrectedScreenHeight
    {
      get
      {
        return _correctedScreenHeight;
      }
      set
      {
        _correctedScreenHeight = value;
      }
    }
  }
}
