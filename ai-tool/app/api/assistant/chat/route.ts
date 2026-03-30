// /app/api/chat/[id]/route.ts
import fs from "fs";
import path from "path";

export async function GET(
  req: Request,
  { params }: { params: { id: string } }
) {
  const filePath = path.join(
    process.cwd(),
    "data/chats",
    `${params.id}.json`
  );

  const data = JSON.parse(fs.readFileSync(filePath, "utf-8"));

  return Response.json(data);
}