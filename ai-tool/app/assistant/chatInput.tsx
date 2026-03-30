"use client";

import { useState } from "react";

export default function ChatInput() {
  const [text, setText] = useState("");

  const send = () => {
    if (!text) return;
    console.log(text);
    setText("");
  };

  return (
    <div className="p-3 border-t flex gap-2">

      <input
        className="flex-1 border rounded px-3 py-2"
        value={text}
        onChange={(e) => setText(e.target.value)}
        placeholder="Ask anything..."
        onKeyDown={(e) => {
          if (e.key === "Enter") send();
        }}
      />

      <button
        className="bg-blue-500 text-white px-4 py-2 rounded"
        onClick={send}
      >
        Send
      </button>
    </div>
  );
}