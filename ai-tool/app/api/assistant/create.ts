// /app/api/chat/create/route.ts
import fs from "fs";
import path from "path";

export async function POST() {
  const id = "chat_" + Date.now();

  const filePath = path.join(process.cwd(), "data/chats", `${id}.json`);

  const newChat = {
    id,
    title: "New Chat",
    createdAt: new Date().toISOString(),
    messages: [],
  };

  fs.writeFileSync(filePath, JSON.stringify(newChat, null, 2));

  return Response.json(newChat);
}