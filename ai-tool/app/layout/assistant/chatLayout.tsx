"use client";

import { useEffect, useState } from "react";
import Sidebar from "./sidebar";
import ChatInput from "./chatInput";
import MessageList from "./messageList";

export default function ChatLayout() {
  // 👉 bỏ generic để tránh lỗi type
  const [openSidebar, setOpenSidebar] = useState(true);
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
      <div
        className={`border-r transition-all duration-300 overflow-hidden ${openSidebar ? "w-64" : "w-0"
          }`}
      >
        <Sidebar chats={chats} onSelect={(id: any) => setSelectedChatId(id)} />
        <button
          onClick={() => setOpenSidebar(!openSidebar)}
          className="absolute top-2 left-2 z-10 
             bg-white border rounded-lg p-2 
             shadow hover:bg-gray-100 transition"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            className={`w-5 h-5 transition-transform ${openSidebar ? "" : "rotate-180"
              }`}
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            strokeWidth={2}
          >
            <path strokeLinecap="round" strokeLinejoin="round" d="M15 19l-7-7 7-7" />
          </svg>
        </button>
      </div>

      {/* Main */}
      <div className="flex-1 flex flex-col relative">
        {/* Toggle button */}

        <div className="flex-1 overflow-auto">
          <MessageList chatId={selectedChatId} refreshKey={refreshKey} />
        </div>

        <div className="border-t p-2">
          <ChatInput onSend={handleSend} />
        </div>
      </div>
    </div>
  );
}
