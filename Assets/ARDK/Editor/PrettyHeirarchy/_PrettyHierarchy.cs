using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Niantic.ARDK.Utilities.Editor;

using UnityEditor;
using UnityEditor.SceneManagement;

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

  [InitializeOnLoad]
  internal class _PrettyHierarchy
  {
    static _PrettyHierarchy()
    {
      EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
    }

    private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
      var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
      if (go == null)
        return;

      var prettyItem = go.GetComponent<_PrettyHeirarchyItem>();
      if (prettyItem == null)
        return;

      var isSelected = Selection.Contains(instanceID);
      var isHierarchyFocused = EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.titleContent.text == "Hierarchy";


      PaintBackground(prettyItem, isHierarchyFocused, isSelected, selectionRect, out Rect backgroundRect);
      if (!isSelected)
        PaintHover(backgroundRect);

      PaintText(prettyItem, isHierarchyFocused, isSelected, selectionRect);

      PaintCollapseToggleIcon(prettyItem, instanceID, selectionRect);
    }

    private static void PaintBackground
    (
      _PrettyHeirarchyItem item,
      bool isHierarchyFocused,
      bool isSelected,
      Rect selectionRect,
      out Rect backgroundRect
    )
    {
      var xPos = selectionRect.position.x + 60f - 28f - selectionRect.xMin;
      var yPos = selectionRect.position.y;
      var xSize = selectionRect.size.x + selectionRect.xMin + 28f - 60 + 16f;
      var ySize = selectionRect.size.y;

      Color32 backgroundColor;
      if (item.UseDefaultBackgroundColor)
        backgroundColor = _HierarchyColors.GetDefaultBackgroundColor(isHierarchyFocused, isSelected);
      else if (isSelected)
        backgroundColor = item.SelectedBackgroundColor;
      else
        backgroundColor = item.BackgroundColor;

      backgroundRect = new Rect(xPos, yPos, xSize, ySize);
      EditorGUI.DrawRect(backgroundRect, backgroundColor);
    }

    private static void PaintHover(Rect backgroundRect)
    {
      var isHovered = backgroundRect.Contains(Event.current.mousePosition);
      if (isHovered)
        EditorGUI.DrawRect(backgroundRect, _HierarchyColors.HoverOverlay);
    }

    private static void PaintText
    (
      _PrettyHeirarchyItem item,
      bool isHierarchyFocused,
      bool isSelected,
      Rect selectionRect
    )
    {
      var xPos = selectionRect.position.x + 18f;
      var yPos = selectionRect.position.y;
      var xSize = selectionRect.size.x - 18f;
      var ySize = selectionRect.size.y;
      var textRect = new Rect(xPos, yPos, xSize, ySize);

      var textColor =
        _HierarchyColors.GetDefaultTextColor
        (
          isHierarchyFocused,
          isSelected,
          item.gameObject.activeInHierarchy
        );

      var style =
        new GUIStyle
        {
          normal = new GUIStyleState { textColor = textColor },
          fontStyle = item.FontStyle
        };

      var label = item.gameObject.name;
      if (item.IsEditorOnly)
        label += " (Editor Only)";

      EditorGUI.LabelField(textRect, label, style);
    }

    private static Type _sceneHierarchyWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
    private static MethodInfo _getExpandedIDsMethod = _sceneHierarchyWindowType.GetMethod("GetExpandedIDs", BindingFlags.NonPublic | BindingFlags.Instance);
    private static void PaintCollapseToggleIcon(_PrettyHeirarchyItem item, int instanceID, Rect selectionRect)
    {
      if (item.gameObject.transform.childCount > 0)
      {
        var xPos = selectionRect.position.x - 14f;
        var yPos = selectionRect.position.y + 1f;
        var xSize = 13f;
        var ySize = 13f;
        var collapseToggleIconRect = new Rect(xPos, yPos, xSize, ySize);

        var sceneHierarchyWindow = _sceneHierarchyWindowType.GetProperty("lastInteractedHierarchyWindow", BindingFlags.Public | BindingFlags.Static);
        var expandedIDs = (int[])_getExpandedIDsMethod.Invoke(sceneHierarchyWindow.GetValue(null), null);

        var iconID = expandedIDs.Contains(instanceID) ? "IN Foldout on" : "IN foldout";

        GUI.DrawTexture
        (
          collapseToggleIconRect,
          EditorGUIUtility.IconContent(iconID).image,
          ScaleMode.StretchToFill,
          true,
          0f,
          _HierarchyColors.CollapseIconTintColor,
          0f,
          0f
        );
      }
    }
  }
}
