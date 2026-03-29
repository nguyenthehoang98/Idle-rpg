// Decompiled with JetBrains decompiler
// Type: CodeBuddy.TypeFinder
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  public static class TypeFinder
  {
    public static System.Type FindType(string name, string assemblyName)
    {
      System.Type type1 = System.Type.GetType(name + ", " + assemblyName);
      if (type1 != (System.Type) null)
        return type1;
      Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
      Assembly assembly1 = ((IEnumerable<Assembly>) assemblies).FirstOrDefault<Assembly>((Func<Assembly, bool>) (x => x.GetName().Name == assemblyName));
      if (assembly1 != (Assembly) null && !assembly1.IsDynamic)
      {
        System.Type[] exportedTypes = assembly1.GetExportedTypes();
        System.Type type2 = ((IEnumerable<System.Type>) exportedTypes).FirstOrDefault<System.Type>((Func<System.Type, bool>) (t => t.FullName == name));
        if (type2 != (System.Type) null)
          return type2;
        System.Type type3 = ((IEnumerable<System.Type>) exportedTypes).FirstOrDefault<System.Type>((Func<System.Type, bool>) (t => t.Name == name));
        if (type3 != (System.Type) null)
          return type3;
      }
      foreach (Assembly assembly2 in assemblies)
      {
        if (!assembly2.IsDynamic)
        {
          System.Type[] exportedTypes = assembly2.GetExportedTypes();
          System.Type type4 = ((IEnumerable<System.Type>) exportedTypes).FirstOrDefault<System.Type>((Func<System.Type, bool>) (t => t.FullName == name));
          if (type4 != (System.Type) null)
            return type4;
          System.Type type5 = ((IEnumerable<System.Type>) exportedTypes).FirstOrDefault<System.Type>((Func<System.Type, bool>) (t => t.Name == name));
          if (type5 != (System.Type) null)
            return type5;
        }
      }
      Debug.LogWarning((object) ("Type: " + name + ", " + assemblyName + " not found"));
      return (System.Type) null;
    }
  }
}
