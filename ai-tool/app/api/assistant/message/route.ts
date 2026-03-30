import { NextRequest, NextResponse } from "next/server";
import fs from "fs";
import path from "path";

function getFilePath(chatId: string) {
  return path.join(process.cwd(), "data/messages", `${chatId}.json`);
}

// giả lập AI response (sau này thay bằng OpenAI / local model)
async function fakeAIResponse(text: string) {
  await new Promise((r) => setTimeout(r, 800)); // delay cho giống thật
  return `AI trả lời: ${text}`;
}

export async function POST(req: NextRequest) {
  try {
    const body = await req.json();
    const { chatId, content } = body;

    // 👉 message user
    const userMessage = {
      id: Date.now() + "_user",
      role: "user",
      content,
      createdAt: new Date(),
    };

    // 👉 gọi AI
    const aiContent = await fakeAIResponse(content);

    const assistantMessage = {
      id: Date.now() + "_assistant",
      role: "assistant",
      content: aiContent,
      createdAt: new Date(),
    };

    return NextResponse.json({
      userMessage,
      assistantMessage,
    });
  } catch (err) {
    return NextResponse.json({ error: "failed" }, { status: 500 });
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