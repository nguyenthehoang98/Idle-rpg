// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ToolManager
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

#nullable enable
namespace CodeBuddy
{
  internal class ToolManager
  {
    private static ToolManager instance;
    private Dictionary<string, ToolBase> _tools;
    private Dictionary<string, ToolCall> _toolCallsMap = new Dictionary<string, ToolCall>();

    public static ToolManager Instance
    {
      get
      {
        if (ToolManager.instance == null)
          ToolManager.instance = new ToolManager();
        return ToolManager.instance;
      }
    }

    public ToolManager()
    {
      this._tools = new Dictionary<string, ToolBase>();
      this.LoadTools();
      foreach (ToolBase toolBase in this._tools.Values)
        toolBase.ExecutionFinished += new Action<string, string, bool>(this.Tool_ExecutionFinished);
    }

    public event Action<ToolCall, string, bool> ToolExecutionFinished;

    public void Tool_ExecutionFinished(
      string toolCallId,
      string toolResult,
      bool invokeAfterReload)
    {
      ToolCall toolCalls = this._toolCallsMap[toolCallId];
      this._toolCallsMap.Remove(toolCallId);
      Action<ToolCall, string, bool> executionFinished = this.ToolExecutionFinished;
      if (executionFinished == null)
        return;
      executionFinished(toolCalls, toolResult, invokeAfterReload);
    }

    private void LoadTools()
    {
      GetSourceCodeTool getSourceCodeTool = new GetSourceCodeTool();
      this._tools[getSourceCodeTool.name] = (ToolBase) getSourceCodeTool;
      GetSceneHierarchyTool sceneHierarchyTool = new GetSceneHierarchyTool();
      this._tools[sceneHierarchyTool.name] = (ToolBase) sceneHierarchyTool;
      CreatePrimitiveTool createPrimitiveTool = new CreatePrimitiveTool();
      this._tools[createPrimitiveTool.name] = (ToolBase) createPrimitiveTool;
      CreateGameObjectTool createGameObjectTool = new CreateGameObjectTool();
      this._tools[createGameObjectTool.name] = (ToolBase) createGameObjectTool;
      GetAssetsListTool getAssetsListTool = new GetAssetsListTool();
      this._tools[getAssetsListTool.name] = (ToolBase) getAssetsListTool;
      InstantiatePrefabTool instantiatePrefabTool = new InstantiatePrefabTool();
      this._tools[instantiatePrefabTool.name] = (ToolBase) instantiatePrefabTool;
      GetComponentsTool getComponentsTool = new GetComponentsTool();
      this._tools[getComponentsTool.name] = (ToolBase) getComponentsTool;
      AssignComponentValuesTool componentValuesTool = new AssignComponentValuesTool();
      this._tools[componentValuesTool.name] = (ToolBase) componentValuesTool;
      GetClassesWithCommentsTool withCommentsTool = new GetClassesWithCommentsTool();
      this._tools[withCommentsTool.name] = (ToolBase) withCommentsTool;
      GetPublicMembersTool publicMembersTool = new GetPublicMembersTool();
      this._tools[publicMembersTool.name] = (ToolBase) publicMembersTool;
      CreateNewScriptTool createNewScriptTool = new CreateNewScriptTool();
      this._tools[createNewScriptTool.name] = (ToolBase) createNewScriptTool;
      ReplaceSourceCodeTool replaceSourceCodeTool = new ReplaceSourceCodeTool();
      this._tools[replaceSourceCodeTool.name] = (ToolBase) replaceSourceCodeTool;
      TakeScreenshotTool takeScreenshotTool = new TakeScreenshotTool();
      this._tools[takeScreenshotTool.name] = (ToolBase) takeScreenshotTool;
      ExecuteMenuItemTool executeMenuItemTool = new ExecuteMenuItemTool();
      this._tools[executeMenuItemTool.name] = (ToolBase) executeMenuItemTool;
    }

    public object[] GetToolsJson(IAiService service)
    {
      return JsonToolSchemaGenerator.GenerateSchema((IEnumerable<ToolBase>) this._tools.Values, service);
    }

    public void ExecuteTool(ToolCall toolCall)
    {
      this._toolCallsMap[toolCall.id] = toolCall;
      string name = toolCall.function.name;
      string arguments = toolCall.function.arguments;
      ToolBase tool = this._tools[name];
      try
      {
        if (!tool.GetType().BaseType.IsGenericType)
          return;
        Type genericTypeArgument = tool.GetType().BaseType.GenericTypeArguments[0];
        object obj = JsonConvert.DeserializeObject(arguments, genericTypeArgument);
        tool.GetType().GetMethod("Execute", new Type[2]
        {
          typeof (string),
          genericTypeArgument
        }).Invoke((object) tool, new object[2]
        {
          (object) toolCall.id,
          obj
        });
      }
      catch (Exception ex)
      {
        this.Tool_ExecutionFinished(toolCall.id, "[FATAL ERROR] Tool execution failed with exception: " + ex.Message, false);
      }
    }

    public string GetToolMessageForUser(ToolCall toolCall)
    {
      ToolBase tool = this._tools[toolCall.function.name];
      string toolMessageForUser = string.Empty;
      try
      {
        if (tool.GetType().BaseType.IsGenericType)
        {
          Type genericTypeArgument = tool.GetType().BaseType.GenericTypeArguments[0];
          object obj = JsonConvert.DeserializeObject(toolCall.function.arguments, genericTypeArgument);
          toolMessageForUser = (string) tool.GetType().GetMethod("GetMessageForUser", new Type[1]
          {
            genericTypeArgument
          }).Invoke((object) tool, new object[1]{ obj });
        }
      }
      catch (Exception ex)
      {
        toolMessageForUser = "Error in getting tool execution message";
      }
      return toolMessageForUser;
    }
  }
}
