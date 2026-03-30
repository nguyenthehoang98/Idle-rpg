import fs from "fs";
import path from "path";

export async function GET() {
  const dir = path.join(process.cwd(), "data/chats");

  // 👉 đảm bảo folder tồn tại
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }

  const files = fs.readdirSync(dir);

  const chats = files.map((file) => {
    const filePath = path.join(dir, file);
    const data = JSON.parse(fs.readFileSync(filePath, "utf-8"));
    return data;
  });

  return Response.json(chats);
}