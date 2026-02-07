#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

public static class DefineSymbolUtils
{
    public static void Add(string symbol)
    {
        Modify(symbol, add: true);
    }

    public static void Remove(string symbol)
    {
        Modify(symbol, add: false);
    }

    public static bool Has(string symbol)
    {
        var target = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
        return defines.Split(';').Contains(symbol);
    }

    private static void Modify(string symbol, bool add)
    {
        var target = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);

        var list = defines.Split(';')
            .Where(d => !string.IsNullOrEmpty(d))
            .ToList();

        bool changed = false;

        if (add)
        {
            if (!list.Contains(symbol))
            {
                list.Add(symbol);
                changed = true;
            }
        }
        else
        {
            changed = list.Remove(symbol);
        }

        if (!changed)
            return;

        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            target,
            string.Join(";", list)
        );

        Debug.Log($"[DefineSymbol] {(add ? "Added" : "Removed")} {symbol}");
    }
}
#endif