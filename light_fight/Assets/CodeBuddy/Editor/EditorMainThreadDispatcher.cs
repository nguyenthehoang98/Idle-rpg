// Decompiled with JetBrains decompiler
// Type: CodeBuddy.EditorMainThreadDispatcher
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEditor;

#nullable enable
namespace CodeBuddy
{
  [InitializeOnLoad]
  public static class EditorMainThreadDispatcher
  {
    private static readonly ConcurrentQueue<Action> ActionQueue = new ConcurrentQueue<Action>();

    static EditorMainThreadDispatcher()
    {
      EditorApplication.update += new EditorApplication.CallbackFunction(EditorMainThreadDispatcher.ExecuteQueuedActions);
    }

    public static void Enqueue(Action action)
    {
      if (action == null)
        throw new ArgumentNullException(nameof (action));
      EditorMainThreadDispatcher.ActionQueue.Enqueue(action);
    }

    public static Task EnqueueAndWait(Action action)
    {
      if (action == null)
        throw new ArgumentNullException(nameof (action));
      TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
      EditorApplication.delayCall += (EditorApplication.CallbackFunction) (() =>
      {
        try
        {
          action();
        }
        finally
        {
          taskCompletionSource.TrySetResult(true);
        }
      });
      return (Task) taskCompletionSource.Task;
    }

    private static void ExecuteQueuedActions()
    {
      Action result;
      while (EditorMainThreadDispatcher.ActionQueue.TryDequeue(out result))
      {
        if (result != null)
          result();
      }
    }
  }
}
