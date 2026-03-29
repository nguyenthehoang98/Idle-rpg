// Decompiled with JetBrains decompiler
// Type: CodeBuddy.DomainReloadWatcher
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  public class DomainReloadWatcher : ScriptableSingleton<DomainReloadWatcher>
  {
    [SerializeField]
    private bool _isCompiling = false;
    private Action<bool> _onCompilationComplete;

    public void TrackCompilation(Action<bool> onCompilationComplete)
    {
      this._onCompilationComplete = onCompilationComplete;
      EditorApplication.update += new EditorApplication.CallbackFunction(this.OnUpdate);
    }

    private void OnUpdate()
    {
      if (!this._isCompiling)
      {
        if (!EditorApplication.isCompiling)
          return;
        this._isCompiling = true;
      }
      else
      {
        if (EditorApplication.isCompiling)
          return;
        this._isCompiling = false;
        bool compilationFailed = EditorUtility.scriptCompilationFailed;
        EditorApplication.update -= new EditorApplication.CallbackFunction(this.OnUpdate);
        if (this._onCompilationComplete == null)
          return;
        Action<bool> compilationComplete = this._onCompilationComplete;
        if (compilationComplete != null)
          compilationComplete(compilationFailed);
        this._onCompilationComplete = (Action<bool>) null;
      }
    }
  }
}
