import { useState } from "react";

export default function ChatInput({ chatId, onNewMessage }: any) {
  const [text, setText] = useState("");

  const send = async () => {
    if (!text) return;

    const message = {
      role: "user",
      content: text,
    };

    const res = await fetch("/api/chat/message", {
      method: "POST",
      body: JSON.stringify({ chatId, message }),
    });

    const data = await res.json();

    onNewMessage(data.messages); // update UI

    setText("");
  };

  return (
    <div className="p-3 border-t flex gap-2">
      <input
        className="flex-1 border rounded px-3 py-2"
        value={text}
        onChange={(e) => setText(e.target.value)}
      />
      <button onClick={send}>Send</button>
    </div>
  );
}