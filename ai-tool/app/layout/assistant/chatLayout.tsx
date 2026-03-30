"use client";

import { useEffect, useState } from "react";
import Sidebar from "./sidebar";
import ChatInput from "./chatInput";
import MessageList from "./messageList";

export default function ChatLayout() {
  // 👉 bỏ generic để tránh lỗi type
  const [refreshKey, setRefreshKey] = useState(0);
  const [chats, setChats] = useState<any[]>([]);
  const [selectedChatId, setSelectedChatId] = useState<any>(null);

  // load history
  useEffect(() => {
    fetch("/api/assistant/history")
      .then((res) => res.json())
      .then((data) => {
        if (Array.isArray(data)) {
          setChats(data);
        } else {
          setChats([]);
        }
      })
      .catch(() => setChats([]));
  }, []);

  // handle send
  const handleSend = async (message: string) => {
    if (!message) return;

    try {
      let chatId = selectedChatId;

      // 👉 nếu chưa có chat → tạo
      if (!chatId) {
        const res = await fetch("/api/assistant/create", {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({ message }),
        });

        const newChat = await res.json();

        setChats((prev: any[]) => [newChat, ...prev]);
        setSelectedChatId(newChat.id);

        chatId = newChat.id; // 🔥 dùng biến này ngay
      }

      // 👉 gửi message (LUÔN chạy, kể cả lần đầu)
      const res = await fetch(`/api/assistant/message?chatId=${chatId}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          chatId,
          content: message,
          role: "user",
        }),
      });

      setRefreshKey((prev) => prev + 1); // 🔥 update UI
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <div className="flex h-screen">
      {/* Sidebar */}
      <div className="w-64 border-r">
        <Sidebar chats={chats} onSelect={(id: any) => setSelectedChatId(id)} />
      </div>

      {/* Main */}
      <div className="flex-1 flex flex-col">
        {/* Message List */}
        <div className="flex-1 overflow-auto">
          {/* 👉 tránh lỗi undefined */}
          {selectedChatId && <MessageList chatId={selectedChatId} />}
        </div>
        <div>
          {" "}
          <MessageList chatId={selectedChatId} refreshKey={refreshKey} />
        </div>
        {/* Input */}
        <div className="border-t p-2">
          <ChatInput onSend={handleSend} />
        </div>
      </div>
    </div>
  );
}
