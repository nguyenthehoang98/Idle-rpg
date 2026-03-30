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
  const [messages, setMessages] = useState<any[]>([]);
  const [isTyping, setIsTyping] = useState(false);
  const [loadingMessages, setLoadingMessages] = useState(false);

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

  useEffect(() => {
    if (!selectedChatId) return;

    if (messages.length > 0) return;

    setLoadingMessages(true);

    fetch(`/api/assistant/message?chatId=${selectedChatId}`)
      .then((res) => res.json())
      .then((data) => {
        setMessages(data || []);
      })
      .finally(() => setLoadingMessages(false));
  }, [selectedChatId]);

  const handleNewChat = () => {
    setSelectedChatId(null);   // ❗ reset chat
    setMessages([]);           // ❗ clear UI
    setIsTyping(false);        // ❗ reset typing
  };


  // handle send
  const handleSend = async (message: string) => {
    if (!message) return;

    let chatId = selectedChatId;

    try {
      // 👉 tạo chat nếu chưa có
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

        chatId = newChat.id;
      }

      // ✅ 1. optimistic UI (hiện message ngay)
      const tempUserMessage = {
        id: Date.now(),
        role: "user",
        content: message,
      };

      setMessages((prev) => [...prev, tempUserMessage]);

      // 👉 show typing
      setIsTyping(true);

      // ✅ 2. call API
      const res = await fetch(`/api/assistant/message`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          chatId,
          content: message,
        }),
      });

      const data = await res.json();

      // ✅ 3. add assistant message (typing animation)
      const aiMsg = data.assistantMessage;

      await typeMessage(aiMsg);

      setIsTyping(false);
    } catch (err) {
      console.error(err);
      setIsTyping(false);
    }
  };

  const typeMessage = async (msg: any) => {
    let current = "";

    const newMsg = {
      id: msg.id,
      role: "assistant",
      content: "",
    };

    setMessages((prev) => [...prev, newMsg]);

    for (let i = 0; i < msg.content.length; i++) {
      current += msg.content[i];

      await new Promise((r) => setTimeout(r, 15)); // tốc độ gõ

      setMessages((prev) =>
        prev.map((m) =>
          m.id === msg.id ? { ...m, content: current } : m
        )
      );
    }
  };

  return (
    <div className="flex h-screen">
      <div className="relative w-64">
        <Sidebar
          selectedChatId={selectedChatId}
          chats={chats}
          onSelect={(id: any) => {
            console.log("SELECT CHAT:", id);
            setSelectedChatId(id);
            setMessages([]); // 👉 tránh dính chat cũ
          }}
          onNewChat={handleNewChat}
          openSidebar={openSidebar}
          setOpenSidebar={setOpenSidebar}
        />
      </div>

      {/* Main */}
      <div
        className={`flex-1 flex flex-col relative transition-all duration-300
  ${openSidebar ? "ml-0" : "-ml-[12rem]"}
`}
      >
        <div className="flex-1 overflow-auto">
          <MessageList messages={messages} isTyping={isTyping} />
        </div>

        <div className="border-t p-2">
          <ChatInput onSend={handleSend} />
        </div>
      </div>
    </div>
  );
}
