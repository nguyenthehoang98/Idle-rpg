"use client";

import { useState } from "react";
import ChatLayout from "@/app/assistant/chatLayout";

export default function AssistantPage() {
  const [openSidebar, setOpenSidebar] = useState(true);

  return (
    <ChatLayout
      openSidebar={openSidebar}
      setOpenSidebar={setOpenSidebar}
    />
  );
}