// Decompiled with JetBrains decompiler
// Type: CodeBuddy.UnityTypeParser
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  public static class UnityTypeParser
  {
    private static Dictionary<System.Type, Func<string, object>> parsingMap = new Dictionary<System.Type, Func<string, object>>();

    static UnityTypeParser()
    {
      UnityTypeParser.parsingMap[typeof (Color)] = (Func<string, object>) (s => (object) UnityTypeParser.ParseColor(s));
      UnityTypeParser.parsingMap[typeof (Vector2)] = (Func<string, object>) (s => (object) UnityTypeParser.ParseVector2(s));
      UnityTypeParser.parsingMap[typeof (Vector3)] = (Func<string, object>) (s => (object) UnityTypeParser.ParseVector3(s));
      UnityTypeParser.parsingMap[typeof (Vector4)] = (Func<string, object>) (s => (object) UnityTypeParser.ParseVector4(s));
      UnityTypeParser.parsingMap[typeof (Quaternion)] = (Func<string, object>) (s => (object) UnityTypeParser.ParseQuaternion(s));
      UnityTypeParser.parsingMap[typeof (Rect)] = (Func<string, object>) (s => (object) UnityTypeParser.ParseRect(s));
      UnityTypeParser.parsingMap[typeof (Bounds)] = (Func<string, object>) (s => (object) UnityTypeParser.ParseBounds(s));
      UnityTypeParser.parsingMap[typeof (Matrix4x4)] = (Func<string, object>) (s => (object) UnityTypeParser.ParseMatrix4x4(s));
    }

    public static bool HasParser(System.Type type)
    {
      return type.IsEnum || UnityTypeParser.parsingMap.ContainsKey(type);
    }

    public static object Parse(System.Type propertyType, string objectString)
    {
      if (propertyType.IsEnum)
        return Enum.Parse(propertyType, objectString);
      return UnityTypeParser.parsingMap.ContainsKey(propertyType) ? UnityTypeParser.parsingMap[propertyType](objectString) : (object) null;
    }

    public static Color ParseColor(string colorString)
    {
      if (colorString.Contains("#"))
      {
        Color color;
        return ColorUtility.TryParseHtmlString(colorString, out color) ? color : Color.white;
      }
      string[] strArray = colorString.Split(',', StringSplitOptions.None);
      if (strArray.Length == 3)
        return new Color(float.Parse(strArray[0], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[1], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[2], (IFormatProvider) CultureInfo.InvariantCulture));
      return strArray.Length != 4 ? Color.clear : new Color(float.Parse(strArray[0], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[1], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[2], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[3], (IFormatProvider) CultureInfo.InvariantCulture));
    }

    public static Vector2 ParseVector2(string vectorString)
    {
      string[] strArray = vectorString.Split(',', StringSplitOptions.None);
      return strArray.Length != 2 ? Vector2.zero : new Vector2(float.Parse(strArray[0], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[1], (IFormatProvider) CultureInfo.InvariantCulture));
    }

    public static Vector3 ParseVector3(string vectorString)
    {
      string[] strArray = vectorString.Split(',', StringSplitOptions.None);
      return strArray.Length != 3 ? Vector3.zero : new Vector3(float.Parse(strArray[0], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[1], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[2], (IFormatProvider) CultureInfo.InvariantCulture));
    }

    public static Vector4 ParseVector4(string vectorString)
    {
      string[] strArray = vectorString.Split(',', StringSplitOptions.None);
      return strArray.Length != 4 ? Vector4.zero : new Vector4(float.Parse(strArray[0], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[1], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[2], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[3], (IFormatProvider) CultureInfo.InvariantCulture));
    }

    public static Quaternion ParseQuaternion(string quaternionString)
    {
      string[] strArray = quaternionString.Split(',', StringSplitOptions.None);
      return strArray.Length != 4 ? Quaternion.identity : new Quaternion(float.Parse(strArray[0], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[1], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[2], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[3], (IFormatProvider) CultureInfo.InvariantCulture));
    }

    public static Rect ParseRect(string rectString)
    {
      string[] strArray = rectString.Split(',', StringSplitOptions.None);
      return strArray.Length != 4 ? new Rect() : new Rect(float.Parse(strArray[0], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[1], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[2], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[3], (IFormatProvider) CultureInfo.InvariantCulture));
    }

    public static Bounds ParseBounds(string boundsString)
    {
      string[] strArray = boundsString.Split(',', StringSplitOptions.None);
      return strArray.Length != 6 ? new Bounds() : new Bounds(new Vector3(float.Parse(strArray[0], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[1], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[2], (IFormatProvider) CultureInfo.InvariantCulture)), new Vector3(float.Parse(strArray[3], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[4], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[5], (IFormatProvider) CultureInfo.InvariantCulture)));
    }

    public static Matrix4x4 ParseMatrix4x4(string matrixString)
    {
      string[] strArray = matrixString.Split(',', StringSplitOptions.None);
      return strArray.Length != 16 ? Matrix4x4.identity : new Matrix4x4(new Vector4(float.Parse(strArray[0], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[1], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[2], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[3], (IFormatProvider) CultureInfo.InvariantCulture)), new Vector4(float.Parse(strArray[4], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[5], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[6], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[7], (IFormatProvider) CultureInfo.InvariantCulture)), new Vector4(float.Parse(strArray[8], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[9], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[10], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[11], (IFormatProvider) CultureInfo.InvariantCulture)), new Vector4(float.Parse(strArray[12], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[13], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[14], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[15], (IFormatProvider) CultureInfo.InvariantCulture)));
    }
  }
}
