// Decompiled with JetBrains decompiler
// Type: CodeBuddy.AiSettings
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  [Serializable]
  internal abstract class AiSettings
  {
    [SerializeField]
    private int modelIndex;
    [SerializeField]
    private bool useCustomModel;
    [SerializeField]
    private string customModelName;
    [SerializeField]
    private float temperature;
    [SerializeField]
    private string baseUrl;

    public AiSettings() => this.Reset();

    public virtual IAiService Service { get; }

    public virtual string Name { get; }

    public event Action<string> OnSettingsChanged;

    protected void InvokeOnSettingsChanged(string name)
    {
      Action<string> onSettingsChanged = this.OnSettingsChanged;
      if (onSettingsChanged == null)
        return;
      onSettingsChanged(name);
    }

    public abstract string[] ModelList { get; }

    public int ModelIndex
    {
      get => this.modelIndex;
      set
      {
        if (this.modelIndex == value)
          return;
        this.modelIndex = value;
        this.InvokeOnSettingsChanged(nameof (ModelIndex));
      }
    }

    public bool UseCustomModel
    {
      get => this.useCustomModel;
      set
      {
        if (this.useCustomModel == value)
          return;
        this.useCustomModel = value;
        this.InvokeOnSettingsChanged(nameof (UseCustomModel));
      }
    }

    public string CustomModelName
    {
      get => this.customModelName;
      set
      {
        if (!(this.customModelName != value))
          return;
        this.customModelName = value;
        this.InvokeOnSettingsChanged(nameof (CustomModelName));
      }
    }

    public string Model
    {
      get => this.UseCustomModel ? this.CustomModelName : this.ModelList[this.ModelIndex];
    }

    public float Temperature
    {
      get => this.temperature;
      set
      {
        if ((double) this.temperature == (double) value)
          return;
        this.temperature = value;
        this.InvokeOnSettingsChanged(nameof (Temperature));
      }
    }

    public string BaseUrl
    {
      get => this.baseUrl;
      set
      {
        if (!(this.baseUrl != value))
          return;
        this.baseUrl = value;
        this.InvokeOnSettingsChanged(nameof (BaseUrl));
      }
    }

    public virtual async Task UpdateModelList() => await Task.CompletedTask;

    public virtual void Reset()
    {
      this.ModelIndex = 0;
      this.Temperature = 0.01f;
      this.CustomModelName = "";
      this.UseCustomModel = false;
    }
  }
}
