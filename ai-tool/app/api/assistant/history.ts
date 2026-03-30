// /app/api/chat/list/route.ts
import fs from "fs";
import path from "path";

export async function GET() {
  const dir = path.join(process.cwd(), "data/chats");

  const files = fs.readdirSync(dir);

  const chats = files.map((file) => {
    const data = JSON.parse(
      fs.readFileSync(path.join(dir, file), "utf-8")
    );

    return {
      id: data.id,
      title: data.title,
    };
  });

  return Response.json(chats);
}