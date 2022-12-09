using System;
using System.IO;

using UnityEditor;

using UnityEngine;

namespace Niantic.ARDK.Utilities.Editor
{
  internal static class _ProjectBrowserUtilities
  {
    // @param fileName Includes file extension (i.e. "foo.fbx" or "bar.asset")
    // @param targetDirectory The directory the file wants to live in
    public static string BuildAssetPath(string fileName, string targetDirectory)
    {
      var assetPath = targetDirectory + "/" + fileName;

      if (File.Exists(assetPath))
      {
        var split = fileName.Split('.');
        var assetName = split[0];
        var assetExt = split[1];
        var count = 0;

        while (File.Exists(assetPath))
        {
          count += 1;
          assetPath = $"{targetDirectory}/{assetName}_{count}.{assetExt}";
        }
      }

      return assetPath;
    }
  }
}
