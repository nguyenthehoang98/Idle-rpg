"use client";

import Sidebar from "@/app/layout/assistant/sidebar";
import ChatHeader from "@/app/layout/assistant/chatHeader";
import MessageList from "@/app/layout/assistant/messageList";
import ChatInput from "@/app/layout/assistant/chatInput";

export default function ChatLayout({
  openSidebar,
  setOpenSidebar,
}: any) {
  return (
    <div className="flex h-screen overflow-hidden">

      {/* Sidebar */}
      <div
        className={`absolute left-0 top-0 h-full w-64 bg-gray-100 transition-transform duration-300 z-20 ${
          openSidebar ? "translate-x-0" : "-translate-x-full"
        }`}
      >
        <Sidebar onClose={() => setOpenSidebar(false)} />
      </div>

      {/* Overlay */}
      {openSidebar && (
        <div
          className="fixed inset-0 bg-black/20 z-10"
          onClick={() => setOpenSidebar(false)}
        />
      )}

      {/* Main */}
      <div className="flex-1 flex flex-col">
        <ChatHeader onOpen={() => setOpenSidebar(true)} />

        <MessageList />

        <ChatInput />
      </div>
    </div>
  );
}