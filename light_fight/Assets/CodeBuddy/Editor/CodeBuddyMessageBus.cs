// Decompiled with JetBrains decompiler
// Type: CodeBuddy.CodeBuddyMessageBus
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;
using System.Collections.Generic;
using UnityEditor;

#nullable enable
namespace CodeBuddy
{
  [InitializeOnLoad]
  internal class CodeBuddyMessageBus
  {
    private Dictionary<System.Type, List<Delegate>> _eventSubscriptions = new Dictionary<System.Type, List<Delegate>>();

    public static CodeBuddyMessageBus Instance { get; private set; }

    static CodeBuddyMessageBus() => CodeBuddyMessageBus.Instance = new CodeBuddyMessageBus();

    public void Subscribe<T>(Action<T> listener)
    {
      System.Type key = typeof (T);
      if (!this._eventSubscriptions.ContainsKey(key))
        this._eventSubscriptions[key] = new List<Delegate>();
      this._eventSubscriptions[key].Add((Delegate) listener);
    }

    public void Unsubscribe<T>(Action<T> listener)
    {
      System.Type key = typeof (T);
      if (!this._eventSubscriptions.ContainsKey(key))
        return;
      this._eventSubscriptions[key].Remove((Delegate) listener);
      if (this._eventSubscriptions[key].Count == 0)
        this._eventSubscriptions.Remove(key);
    }

    public void Publish<T>(T message)
    {
      System.Type key = typeof (T);
      if (!this._eventSubscriptions.ContainsKey(key))
        return;
      foreach (Delegate @delegate in this._eventSubscriptions[key])
      {
        Action<T> action = @delegate as Action<T>;
        if (action != null)
          EditorMainThreadDispatcher.Enqueue((Action) (() => action(message)));
      }
    }
  }
}
