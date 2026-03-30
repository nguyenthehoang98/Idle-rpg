// /app/api/chat/message/route.ts
import fs from "fs";
import path from "path";

export async function POST(req: Request) {
  const { chatId, message } = await req.json();

  const filePath = path.join(
    process.cwd(),
    "data/chats",
    `${chatId}.json`
  );

  const data = JSON.parse(fs.readFileSync(filePath, "utf-8"));

  data.messages.push({
    id: "msg_" + Date.now(),
    ...message,
    createdAt: new Date().toISOString(),
  });

  fs.writeFileSync(filePath, JSON.stringify(data, null, 2));

  return Response.json(data);
}