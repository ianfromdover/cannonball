using System;
using System.Linq;

namespace ARDK.Utilities
{
  internal static class _ArrayUtility
  {
    /// Check to see whether or not all of the arrays are the same length
    /// @param arraysThe arrays to check
    /// @return Whether or not the arrays are the same length
    internal static bool ArraysAreSameLength(params Array[] arrays)
    {
      return arrays.All(a => a.Length == arrays[0].Length);
    }
  }
}
