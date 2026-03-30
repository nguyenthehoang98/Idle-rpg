"use client";

import { useState } from "react";

export default function ChatInput({ onSend }: any) {
  const [input, setInput] = useState("");

  const handleClick = () => {
    if (!input.trim()) return;

    console.log("SEND:", input); // 👉 debug xem có chạy không

    onSend(input); // 🔥 QUAN TRỌNG

    setInput("");
  };

  return (
    <div className="flex gap-2">
      <input
        className="flex-1 border p-2 rounded"
        value={input}
        onChange={(e) => setInput(e.target.value)}
        placeholder="Type message..."
      />

      <button
        onClick={handleClick}
        className="px-4 py-2 bg-blue-500 text-white rounded"
      >
        Send
      </button>
    </div>
  );
}