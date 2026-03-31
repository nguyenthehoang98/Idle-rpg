"use client";

import React, { useState, useCallback } from "react";
import {
  Background,
  ReactFlow,
  ReactFlowProvider,
  useNodesState,
  useEdgesState,
  addEdge,
  useReactFlow,
  Panel,
  Node,
  Edge,
  ReactFlowInstance,
  Connection,
} from "@xyflow/react";

import "@xyflow/react/dist/style.css";

const flowKey = "example-flow";

const initialNodes: Node[] = [
  {
    id: "1",
    data: { label: "Node 1" },
    position: { x: 0, y: -50 },
  },
  {
    id: "2",
    data: { label: "Node 2" },
    position: { x: 0, y: 50 },
  },
];

const initialEdges: Edge[] = [
  { id: "e1-2", source: "1", target: "2" },
];

function WorkflowEditor() {
  const [nodes, setNodes, onNodesChange] = useNodesState<Node[]>(initialNodes);
  const [edges, setEdges, onEdgesChange] = useEdgesState<Edge[]>(initialEdges);
  const [rfInstance, setRfInstance] = useState<ReactFlowInstance | null>(null);

  const { setViewport } = useReactFlow();

  const onConnect = useCallback(
    (params: Connection) => {
      setEdges((eds) => addEdge(params, eds));
    },
    [setEdges]
  );

  const onAdd = useCallback(() => {
    setNodes((nds) => [
      ...nds,
      {
        id: `${Date.now()}`,
        data: { label: "New Node" },
        position: { x: Math.random() * 300, y: Math.random() * 300 },
      },
    ]);
  }, [setNodes]);

  return (
    <div className="w-full h-screen">
      <ReactFlow
        nodes={nodes}
        edges={edges}
        onNodesChange={onNodesChange}
        onEdgesChange={onEdgesChange}
        onConnect={onConnect}
        onInit={setRfInstance}
        fitView
      >
        <Background />
        <Panel position="top-right">
          <button onClick={onAdd}>Add</button>
        </Panel>
      </ReactFlow>
    </div>
  );
}

export default function WorkflowEditorWrapper() {
  return (
    <ReactFlowProvider>
      <WorkflowEditor />
    </ReactFlowProvider>
  );
}