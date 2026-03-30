"use client";

import { useEffect, useState } from "react";
import { Chat } from "@/app/types/assistant/chat";
export default function Sidebar({ onSelect }: any) {
  const [chats, setChats] = useState([]);

  useEffect(() => {
    fetch("/api/chat/list")
      .then((res) => res.json())
      .then(setChats);
  }, []);

  return (
    <div>
      {chats.map((c: any) => (
        <div
          key={c.id}
          onClick={() => onSelect(c.id)}
          className="p-2 cursor-pointer hover:bg-gray-200"
        >
          {c.title}
        </div>
      ))}
    </div>
  );
}