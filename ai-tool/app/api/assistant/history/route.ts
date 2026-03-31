import fs from "fs";
import path from "path";

export async function POST(req: Request) {
  const dir = path.join(process.cwd(), "data/chats");

  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }

  // 👉 nhận local từ FE
  const body = await req.json();
  const localChats = body.local || [];

  console.log("💾 LOCAL:", localChats.map((c: any) => c.id));

  const files = fs.readdirSync(dir);

  const map = new Map<string, any>();

  // 👉 đọc server data
  for (const file of files) {
    try {
      const filePath = path.join(dir, file);
      const raw = fs.readFileSync(filePath, "utf-8");

      if (!raw) continue;

      const data = JSON.parse(raw);
      if (!data || !data.id) continue;

      const chat = {
        id: data.id,
        title: data.title || "New Chat",
        createdAt:
          typeof data.createdAt === "number"
            ? data.createdAt
            : fs.statSync(filePath).mtimeMs,
      };

      map.set(String(chat.id), chat);
    } catch (err) {
      console.error("❌ Read error:", file);
    }
  }

  console.log(
    "🌐 SERVER:",
    Array.from(map.values()).map((c) => c.id)
  );

  // 👉 chỉ giữ local nếu còn tồn tại trên server
  const serverIds = new Set(map.keys());

  const validLocal = localChats.filter((c: any) =>
    serverIds.has(String(c.id))
  );

  console.log(
    "🧹 VALID LOCAL:",
    validLocal.map((c: any) => c.id)
  );

  // 👉 merge (server là chính)
  validLocal.forEach((c: any) => {
    if (!map.has(String(c.id))) {
      map.set(String(c.id), c);
    }
  });

  const final = Array.from(map.values());

  final.sort((a, b) => b.createdAt - a.createdAt);

  console.log(
    "✅ FINAL:",
    final.map((c) => c.id)
  );

  return Response.json(final);
}

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