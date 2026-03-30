"use client";

import { useEffect, useState } from "react";

export default function MessageList({ chatId, refreshKey }: any) {
  const [messages, setMessages] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!chatId) {
      setMessages([]);
      return;
    }

    const fetchMessages = async () => {
      setLoading(true);

      try {
        const res = await fetch(`/api/assistant/message?chatId=${chatId}`);
        const data = await res.json();

        setMessages(Array.isArray(data) ? data : []);
      } catch (err) {
        console.error(err);
        setMessages([]);
      }

      setLoading(false);
    };

    fetchMessages();
  }, [chatId, refreshKey]); // 🔥 THÊM refreshKey

  return (
    <div className="p-4 space-y-2">
      {/* loading */}
      {loading && <div className="text-gray-400">Loading...</div>}

      {/* empty */}
      {!loading && messages.length === 0 && (
        <div className="text-gray-400">No messages</div>
      )}

      {/* list */}
      {messages.map((m: any, index: number) => (
        <div
          key={index}
          className={`p-2 rounded max-w-[70%] ${m.role === "user"
            ? "bg-blue-500 text-white ml-auto"
            : "bg-gray-200 text-black"
            }`}
        >
          {m.content}
        </div>
      ))}
    </div>
  );
}