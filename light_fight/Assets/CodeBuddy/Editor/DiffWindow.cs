// Decompiled with JetBrains decompiler
// Type: CodeBuddy.DiffWindow
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#nullable enable
namespace CodeBuddy
{
  public class DiffWindow : EditorWindow
  {
    private ScrollView leftPane;
    private ScrollView rightPane;
    private string oldFile;
    private string newFile;
    private bool isLeftPaneScrolling = false;
    private bool isRightPaneScrolling = false;
    private TextField feedbackField;
    private Button declineButton;
    private Action<bool, string> _onResultAction;
    private bool hasScrolledToFirstChange = false;

    public static void ShowWindow(
      string orignialFileContent,
      string modifiedFileContent,
      Action<bool, string> onResultAction)
    {
      DiffWindow window = EditorWindow.GetWindow<DiffWindow>();
      window.titleContent = new GUIContent("Diff Window");
      window.minSize = new Vector2(600f, 400f);
      window.oldFile = orignialFileContent;
      window.newFile = modifiedFileContent;
      window._onResultAction = onResultAction;
      window.CompareTexts();
    }

    public void CreateGUI()
    {
      StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("8a7cda571b596a340a68b12ef1ac1f46"));
      if ((UnityEngine.Object) styleSheet != (UnityEngine.Object) null)
        this.rootVisualElement.styleSheets.Add(styleSheet);
      else
        Debug.LogWarning((object) "DiffWindowStyles.uss not found. Please ensure it's in the correct Editor folder.");
      VisualElement child1 = new VisualElement();
      child1.AddToClassList("split-container");
      this.rootVisualElement.Add(child1);
      VisualElement child2 = new VisualElement();
      child2.style.width = (StyleLength) Length.Percent(50f);
      child2.style.flexDirection = (StyleEnum<FlexDirection>) FlexDirection.Column;
      Label child3 = new Label("Original file");
      child3.style.fontSize = (StyleLength) 20f;
      child3.style.unityFontStyleAndWeight = (StyleEnum<FontStyle>) FontStyle.Bold;
      child3.style.unityTextAlign = (StyleEnum<TextAnchor>) TextAnchor.MiddleCenter;
      child2.Add((VisualElement) child3);
      this.leftPane = new ScrollView();
      this.leftPane.style.height = (StyleLength) Length.Percent(100f);
      this.leftPane.AddToClassList("split-pane");
      child2.Add((VisualElement) this.leftPane);
      VisualElement child4 = new VisualElement();
      child4.style.width = (StyleLength) Length.Percent(50f);
      child4.style.flexDirection = (StyleEnum<FlexDirection>) FlexDirection.Column;
      Label child5 = new Label("Changes");
      child5.style.fontSize = (StyleLength) 20f;
      child5.style.unityFontStyleAndWeight = (StyleEnum<FontStyle>) FontStyle.Bold;
      child5.style.unityTextAlign = (StyleEnum<TextAnchor>) TextAnchor.MiddleCenter;
      child4.Add((VisualElement) child5);
      this.rightPane = new ScrollView();
      this.rightPane.style.height = (StyleLength) Length.Percent(100f);
      this.rightPane.AddToClassList("split-pane");
      child4.Add((VisualElement) this.rightPane);
      child1.Add(child2);
      child1.Add(child4);
      VisualElement child6 = new VisualElement();
      child6.AddToClassList("input-container");
      this.rootVisualElement.Add(child6);
      Button button1 = new Button(new Action(this.OnAccept));
      button1.text = "Accept";
      Button child7 = button1;
      child6.Add((VisualElement) child7);
      this.feedbackField = new TextField("Feedback");
      this.feedbackField.style.minWidth = (StyleLength) 300f;
      this.feedbackField.style.display = (StyleEnum<DisplayStyle>) DisplayStyle.None;
      child6.Add((VisualElement) this.feedbackField);
      Button button2 = new Button(new Action(this.OnDecline));
      button2.text = "Decline";
      this.declineButton = button2;
      child6.Add((VisualElement) this.declineButton);
      this.SetupScrollSynchronization();
    }

    private void OnAccept()
    {
      this._onResultAction(true, "Changes applied successfully.");
      this.Close();
    }

    private void OnDecline()
    {
      if (this.feedbackField.style.display == (StyleEnum<DisplayStyle>) DisplayStyle.None)
      {
        this.feedbackField.style.display = (StyleEnum<DisplayStyle>) DisplayStyle.Flex;
        this.declineButton.text = "Submit feedback";
      }
      else
      {
        this._onResultAction(false, this.feedbackField.value);
        this.Close();
      }
    }

    private void SetupScrollSynchronization()
    {
      this.leftPane.schedule.Execute((Action) (() =>
      {
        Scroller verticalScroller1 = this.leftPane.verticalScroller;
        Scroller verticalScroller2 = this.rightPane.verticalScroller;
        if (verticalScroller1 == null || verticalScroller2 == null)
          return;
        verticalScroller1.valueChanged -= new Action<float>(this.OnLeftScroll);
        verticalScroller2.valueChanged -= new Action<float>(this.OnRightScroll);
        verticalScroller1.valueChanged += new Action<float>(this.OnLeftScroll);
        verticalScroller2.valueChanged += new Action<float>(this.OnRightScroll);
      })).Until((Func<bool>) (() => this.leftPane.verticalScroller != null && this.rightPane.verticalScroller != null));
    }

    private void OnLeftScroll(float value)
    {
      if (this.isRightPaneScrolling)
        return;
      this.isLeftPaneScrolling = true;
      this.rightPane.verticalScroller.value = value;
      this.isLeftPaneScrolling = false;
    }

    private void ScrollToLine(
      ScrollView leftPane,
      ScrollView rightPane,
      Label leftLabel,
      Label rightLabel)
    {
      float yMin = leftLabel.layout.yMin;
      leftPane.scrollOffset = new Vector2(0.0f, yMin);
      rightPane.scrollOffset = new Vector2(0.0f, yMin);
    }

    private void OnRightScroll(float value)
    {
      if (this.isLeftPaneScrolling)
        return;
      this.isRightPaneScrolling = true;
      this.leftPane.verticalScroller.value = value;
      this.isRightPaneScrolling = false;
    }

    private void CompareTexts()
    {
      this.leftPane.Clear();
      this.rightPane.Clear();
      List<DiffWindow.DiffLine> diffLineList = this.Diff(this.oldFile.Split(new string[3]
      {
        "\r\n",
        "\r",
        "\n"
      }, StringSplitOptions.None), this.newFile.Split(new string[3]
      {
        "\r\n",
        "\r",
        "\n"
      }, StringSplitOptions.None));
      this.hasScrolledToFirstChange = false;
      foreach (DiffWindow.DiffLine diffLine in diffLineList)
      {
        Label label1 = new Label();
        Label label2 = new Label();
        switch (diffLine.State)
        {
          case DiffWindow.DiffState.Unchanged:
            label1.text = diffLine.Text;
            label2.text = diffLine.Text;
            break;
          case DiffWindow.DiffState.Added:
            label1.text = " ";
            label1.AddToClassList("placeholder");
            label2.text = diffLine.Text;
            label2.AddToClassList("added");
            break;
          case DiffWindow.DiffState.Removed:
            label1.text = diffLine.Text;
            label1.AddToClassList("removed");
            label2.text = " ";
            label2.AddToClassList("placeholder");
            break;
        }
        this.leftPane.Add((VisualElement) label1);
        this.rightPane.Add((VisualElement) label2);
        if (!this.hasScrolledToFirstChange && (diffLine.State == DiffWindow.DiffState.Added || diffLine.State == DiffWindow.DiffState.Removed))
        {
          this.ScrollToLine(this.leftPane, this.rightPane, label1, label2);
          this.hasScrolledToFirstChange = true;
        }
      }
    }

    private List<DiffWindow.DiffLine> Diff(string[] oldText, string[] newText)
    {
      int[,] numArray = new int[oldText.Length + 1, newText.Length + 1];
      for (int index1 = 0; index1 < oldText.Length; ++index1)
      {
        for (int index2 = 0; index2 < newText.Length; ++index2)
          numArray[index1 + 1, index2 + 1] = !(oldText[index1] == newText[index2]) ? Mathf.Max(numArray[index1 + 1, index2], numArray[index1, index2 + 1]) : numArray[index1, index2] + 1;
      }
      List<DiffWindow.DiffLine> diffLineList = new List<DiffWindow.DiffLine>();
      int length1 = oldText.Length;
      int length2 = newText.Length;
      while (length1 > 0 || length2 > 0)
      {
        if (length1 > 0 && length2 > 0 && oldText[length1 - 1] == newText[length2 - 1])
        {
          diffLineList.Add(new DiffWindow.DiffLine()
          {
            Text = oldText[length1 - 1],
            State = DiffWindow.DiffState.Unchanged
          });
          --length1;
          --length2;
        }
        else if (length2 > 0 && (length1 == 0 || numArray[length1, length2 - 1] >= numArray[length1 - 1, length2]))
        {
          diffLineList.Add(new DiffWindow.DiffLine()
          {
            Text = newText[length2 - 1],
            State = DiffWindow.DiffState.Added
          });
          --length2;
        }
        else if (length1 > 0 && (length2 == 0 || numArray[length1, length2 - 1] < numArray[length1 - 1, length2]))
        {
          diffLineList.Add(new DiffWindow.DiffLine()
          {
            Text = oldText[length1 - 1],
            State = DiffWindow.DiffState.Removed
          });
          --length1;
        }
      }
      diffLineList.Reverse();
      return diffLineList;
    }

    private enum DiffState
    {
      Unchanged,
      Added,
      Removed,
    }

    private class DiffLine
    {
      public string Text;
      public DiffWindow.DiffState State;
    }
  }
}
