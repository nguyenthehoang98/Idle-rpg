import fs from "fs";
import path from "path";

export async function GET() {
  const dir = path.join(process.cwd(), "data/chats");

  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }

  const files = fs.readdirSync(dir);

  const chats: any[] = [];

  for (const file of files) {
    try {
      const filePath = path.join(dir, file);
      const raw = fs.readFileSync(filePath, "utf-8");

      if (!raw) continue;

      const data = JSON.parse(raw);

      // ❗ validate data chặt hơn
      if (!data || !data.id) continue;

      chats.push({
        id: data.id,
        title: data.title || "New Chat",
        createdAt:
          typeof data.createdAt === "number"
            ? data.createdAt
            : fs.statSync(filePath).mtimeMs,
      });
    } catch (err) {
      console.error("❌ Read chat error:", file);
    }
  }

  // ❗ nếu không có chat nào → vẫn return [] nhưng FE sẽ handle
  chats.sort((a, b) => b.createdAt - a.createdAt);

  return Response.json(chats);
}