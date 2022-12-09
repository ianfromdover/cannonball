using UnityEditor;

using UnityEngine;

namespace Niantic.ARDK.Editor
{
  public static class CommonStyles
  {
    public const char FindIcon = '\u25c9';
    public const char RefreshArrow = '\u21bb';

    public const int RefreshIconWidth = 30;

    public const string NaNVector = "(NaN, NaN, NaN)";

    private static GUIStyle _boldLabelStyle;
    public static GUIStyle BoldLabelStyle
    {
      get
      {
        if (_boldLabelStyle == null)
        {
          _boldLabelStyle = new GUIStyle(EditorStyles.label);
          _boldLabelStyle.fontStyle = FontStyle.Bold;
        }

        return _boldLabelStyle;
      }
    }

    private static GUIStyle _boldFoldoutStyle;
    public static GUIStyle BoldFoldoutStyle
    {
      get
      {
        if (_boldFoldoutStyle == null)
        {
          _boldFoldoutStyle = new GUIStyle(EditorStyles.foldout);
          _boldFoldoutStyle.fontStyle = FontStyle.Bold;
        }

        return _boldFoldoutStyle;
      }
    }


    private static GUIStyle _boldTextAreaStyle;
    public static GUIStyle BoldTextFieldStyle
    {
      get
      {
        if (_boldTextAreaStyle == null)
        {
          _boldTextAreaStyle = new GUIStyle(EditorStyles.textField);
          _boldTextAreaStyle.fontStyle = FontStyle.Bold;
        }

        return _boldTextAreaStyle;
      }
    }

    private static GUIStyle _centeredLabelStyle;
    public static GUIStyle CenteredLabelStyle
    {
      get
      {
        if (_centeredLabelStyle == null)
        {
          _centeredLabelStyle = new GUIStyle(EditorStyles.label);
          _centeredLabelStyle.alignment = TextAnchor.MiddleCenter;
          _centeredLabelStyle.wordWrap = true;
        }

        return _centeredLabelStyle;
      }
    }

    public static bool RefreshButton()
    {
      return GUILayout.Button(RefreshArrow.ToString(), GUILayout.Width(RefreshIconWidth));
    }
  }
}
