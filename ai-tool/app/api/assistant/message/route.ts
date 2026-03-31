import { NextRequest, NextResponse } from "next/server";
import { callAI, buildPrompt } from "@/app/services/ai";
import fs from "fs";
import path from "path";

// 👉 RAM DB
const db: Record<string, any[]> = {};

function getFilePath(chatId: string) {
  return path.join(process.cwd(), "data/messages", `${chatId}.json`);
}

function ensureDir() {
  const dir = path.join(process.cwd(), "data/messages");
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
}

function saveToFile(chatId: string, messages: any[]) {
  ensureDir();
  const filePath = getFilePath(chatId);
  fs.writeFileSync(filePath, JSON.stringify(messages, null, 2));
}

function loadFromFile(chatId: string) {
  const filePath = getFilePath(chatId);
  if (!fs.existsSync(filePath)) return [];
  return JSON.parse(fs.readFileSync(filePath, "utf-8"));
}

export async function POST(req: NextRequest) {
  try {
    const body = await req.json();
    const { chatId, content } = body;

    if (!chatId || !content) {
      return NextResponse.json({ error: "Missing data" }, { status: 400 });
    }

    // 👉 load existing
    let messages = loadFromFile(chatId);

    if (messages.length === 0 && db[chatId]) {
      messages = db[chatId];
    }

    // 👉 user message
    const userMessage = {
      id: Date.now() + "_user",
      role: "user",
      content,
      createdAt: new Date().toISOString(),
    };

    messages.push(userMessage);

    // 👉 build prompt từ history
    const prompt = buildPrompt(messages);

    // 👉 call AI thật
    let aiContent = "";
    try {
      aiContent = await callAI(prompt);
    } catch (e: any) {
      console.error("AI ERROR:", e);
      aiContent = "⚠️ AI đang lỗi, thử lại sau nhé";
    }

    // 👉 assistant message
    const assistantMessage = {
      id: Date.now() + "_assistant",
      role: "assistant",
      content: aiContent,
      createdAt: new Date().toISOString(),
    };

    messages.push(assistantMessage);

    // 👉 save
    db[chatId] = messages;
    saveToFile(chatId, messages);

    return NextResponse.json({
      userMessage,
      assistantMessage,
    });
  } catch (err) {
    console.error(err);
    return NextResponse.json({ error: "failed" }, { status: 500 });
  }
}

export async function GET(req: NextRequest) {
  try {
    const { searchParams } = new URL(req.url);
    const chatId = searchParams.get("chatId");

    if (!chatId) {
      return NextResponse.json([]);
    }

    const filePath = getFilePath(chatId);

    if (!fs.existsSync(filePath)) {
      return NextResponse.json([]);
    }

    const data = fs.readFileSync(filePath, "utf-8");
    const messages = JSON.parse(data);

    return NextResponse.json(messages);
  } catch (err) {
    console.error(err);
    return NextResponse.json([], { status: 500 });
  }
}