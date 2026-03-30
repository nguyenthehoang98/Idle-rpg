import { NextRequest, NextResponse } from "next/server";
import fs from "fs";
import path from "path";

function getFilePath(chatId: string) {
  return path.join(process.cwd(), "data/messages", `${chatId}.json`);
}

function ensureFileExists(filePath: string) {
  if (!fs.existsSync(filePath)) {
    fs.mkdirSync(path.dirname(filePath), { recursive: true });
    fs.writeFileSync(filePath, JSON.stringify([]));
  }
}

export async function POST(req: NextRequest) {
  try {
    const body = await req.json();
    const { chatId, ...message } = body;

    if (!chatId) {
      return NextResponse.json({ error: "Missing chatId" }, { status: 400 });
    }

    const filePath = getFilePath(chatId);

    // 👇 đảm bảo file tồn tại
    if (!fs.existsSync(filePath)) {
      fs.mkdirSync(path.dirname(filePath), { recursive: true });
      fs.writeFileSync(filePath, JSON.stringify([]));
    }

    // 👇 đọc trước
    const messages = JSON.parse(fs.readFileSync(filePath, "utf-8"));

    // 👇 push sau (quan trọng)
    messages.push({
      role: "user",
      content: body.content || body.message,
      createdAt: Date.now(),
    });

    fs.writeFileSync(filePath, JSON.stringify(messages, null, 2));

    // 👇 trả luôn data mới để FE khỏi fetch lại
    return NextResponse.json(messages);
  } catch (err) {
    console.error(err);
    return NextResponse.json({ error: "Failed" }, { status: 500 });
  }
}

export async function GET(req: NextRequest) {
  try {
    const { searchParams } = new URL(req.url);
    const chatId = searchParams.get("chatId");
    console.log("GET chatId:", chatId);
    // ❗ validate
    if (!chatId) {
      return NextResponse.json(
        { error: "Missing chatId" },
        { status: 400 }
      );
    }

    const filePath = getFilePath(chatId);

    // 👉 nếu chưa có file → trả về []
    if (!fs.existsSync(filePath)) {
      return NextResponse.json([]);
    }

    const fileData = fs.readFileSync(filePath, "utf-8");
    const messages = JSON.parse(fileData);

    return NextResponse.json(messages);
  } catch (err) {
    console.error(err);
    return NextResponse.json({ error: "Failed" }, { status: 500 });
  }
}