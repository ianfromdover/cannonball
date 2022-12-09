using UnityEditor;

using UnityEngine;

namespace Niantic.ARDK.Editor
{
  // MIT License
  //
  // Copyright (c) 2020 NCEEGEE
  //
  // Permission is hereby granted, free of charge, to any person obtaining a copy
  // of this software and associated documentation files (the "Software"), to deal
  // in the Software without restriction, including without limitation the rights
  // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  // copies of the Software, and to permit persons to whom the Software is
  // furnished to do so, subject to the following conditions:
  //
  // The above copyright notice and this permission notice shall be included in all
  // copies or substantial portions of the Software.
  //
  // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
  // SOFTWARE.

  internal class _HierarchyColors
  {
    // DARK THEME - Background
    private static Color32 _darkBackground = new Color32(56, 56, 56, 255);
    private static Color32 _darkObjectSelectedBackground = new Color32(77, 77, 77, 255);
    private static Color32 _darkObjectSelectedWindowFocusedBackground = new Color32(44, 93, 134, 255);
    private static Color32 _darkHoverOverlay = new Color32(255, 255, 255, 15);
    // DARK THEME - Text
    private static Color32 _darkText = new Color32(210, 210, 210, 255);
    private static Color32 _darkTextHighlighted = new Color32(255, 255, 255, 255);
    private static byte _darkTextAlphaObjectEnabled = 255;
    private static byte _darkTextAlphaObjectDisabled = 103;

    // LIGHT THEME - Background
    private static Color32 _lightBackground = new Color32(200, 200, 200, 255);
    private static Color32 _lightObjectSelectedBackground = new Color32(178, 178, 178, 255);
    private static Color32 _lightObjectSelectedWindowFocusedBackground = new Color32(58, 114, 176, 255);
    private static Color32 _lightHoverOverlay = new Color32(0, 0, 0, 21);
    // LIGHT THEME - Text
    private static Color32 _lightText = new Color32(2, 2, 2, 255);
    private static Color32 _lightTextHighlighted = new Color32(255, 255, 255, 255);
    private static byte _lightTextAlphaObjectEnabled = 255;
    private static byte _lightTextAlphaObjectDisabled = 95;

    /// Returns the background color for a hierarchy object that is not selected.
    public static Color32 Background { get { return EditorGUIUtility.isProSkin ? _darkBackground : _lightBackground; } }

    /// Returns the background color for a hierarchy object that is selected while the hierarchy window is NOT the focus.
    public static Color32 ObjectSelectedBackground { get { return EditorGUIUtility.isProSkin ? _darkObjectSelectedBackground : _lightObjectSelectedBackground; } }

    /// Returns the text color for a hierarchy object that is selected while the hierarchy window is the focus.
    public static Color32 ObjectSelectedWindowFocusedBackground { get { return EditorGUIUtility.isProSkin ? _darkObjectSelectedWindowFocusedBackground : _lightObjectSelectedWindowFocusedBackground; } }

    /// Returns the background overlay color when the object is hovered in the hierarchy. This should be drawn on top of the background color.
    public static Color32 HoverOverlay { get { return EditorGUIUtility.isProSkin ? _darkHoverOverlay : _lightHoverOverlay; } }

    /// Returns the correct hierarchy background color depending on the context.
    public static Color32 GetDefaultBackgroundColor(bool windowIsFocused, bool selectionContainsObject)
    {
      return selectionContainsObject ? windowIsFocused ? ObjectSelectedWindowFocusedBackground : ObjectSelectedBackground : Background;
    }

    /// Returns the default text color
    public static Color32 Text { get { return EditorGUIUtility.isProSkin ? _darkText : _lightText; } }

    /// Returns the highlighted text color. Highlight happens when those 3 conditions are true: Object is selected / Object is active / Window is focused
    public static Color32 TextHighlighted { get { return EditorGUIUtility.isProSkin ? _darkTextHighlighted : _lightTextHighlighted; } }

    /// Returns the alpha of the text for a GameObject enabled in the hierarchy. This alpha should be applied to the final color.
    public static byte TextAlphaObjectEnabled { get { return EditorGUIUtility.isProSkin ? _darkTextAlphaObjectEnabled : _lightTextAlphaObjectEnabled; } }

    /// Returns the alpha of the text for a GameObject disabled in the hierarchy. This alpha should be applied to the final color.
    public static byte TextAlphaObjectDisabled { get { return EditorGUIUtility.isProSkin ? _darkTextAlphaObjectDisabled : _lightTextAlphaObjectDisabled; } }

    /// Returns the tint color of the "collapse" icon texture.
    public static Color CollapseIconTintColor { get { return EditorGUIUtility.isProSkin ? Color.white : Color.black; } }

    /// Returns the correct hierarchy text color depending on the context.
    public static Color32 GetDefaultTextColor(bool windowIsFocused, bool selectionContainsObject, bool objectIsEnabled)
    {
      bool textHighlighted = windowIsFocused && selectionContainsObject && objectIsEnabled;
      Color32 color = textHighlighted ? TextHighlighted : Text;
      color.a = objectIsEnabled ? TextAlphaObjectEnabled : TextAlphaObjectDisabled;
      return color;
    }

    /// Returns whether or not the text should be displayed as highlighted.
    public static bool IsTextHighlighted(bool windowIsFocused, bool selectionContainsObject, bool objectIsEnabled)
    {
      return windowIsFocused && selectionContainsObject && objectIsEnabled;
    }
  }
}
