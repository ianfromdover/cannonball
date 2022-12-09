using System;

using UnityEngine;

namespace Niantic.ARDK.Utilities.Editor
{
  internal class _PrettyHeirarchyItem: MonoBehaviour
  {
    public bool UseDefaultBackgroundColor;

    [SerializeField]
    private Color32 _backgroundColor;
    public Color32 BackgroundColor
    {
      get => _backgroundColor;
      set
      {
        _backgroundColor = value;
        var scale = 0.8f;
        _selectedBackgroundColor =
          new Color32
          (
            (byte)(value.r * scale),
            (byte)(value.g * scale),
            (byte)(value.b * scale),
            value.a
          );
      }
    }

    public FontStyle FontStyle;
    public bool IsEditorOnly;

    private Color32 _selectedBackgroundColor;
    public Color32 SelectedBackgroundColor { get => _selectedBackgroundColor; }

    private void Reset()
    {
      hideFlags = HideFlags.HideInInspector;
    }
  }
}
