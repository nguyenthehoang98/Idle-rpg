import { AI_CONFIG } from "@/app/configs/ai";

type Message = {
  role: "user" | "assistant";
  content: string;
};

export function buildPrompt(messages: Message[]) {
  return `
You are a helpful AI assistant.
Always format your response using Markdown.
- Use bullet points
- Use headings
- Use code blocks when needed

${messages
  .map((m) => `${m.role === "user" ? "User" : "AI"}: ${m.content}`)
  .join("\n")}

Answer is Vietnamese:
`;
}

type AIResponse = {
  response: string;
};

export async function callAI(prompt: string): Promise<string> {
  const payload = {
    model: AI_CONFIG.MODEL,
    prompt,
    stream: false,
  };

  console.log("===== AI REQUEST =====");
  console.log("URL:", AI_CONFIG.BASE_URL);
  console.log("BODY:", JSON.stringify(payload, null, 2));

  try {
    const res = await fetch(AI_CONFIG.BASE_URL, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(payload),
    });

    console.log("===== AI RESPONSE STATUS =====");
    console.log(res.status, res.statusText);

    const text = await res.text(); // 👈 lấy raw trước
    console.log("===== AI RAW RESPONSE =====");
    console.log(text);

    if (!res.ok) {
      throw new Error(`HTTP ${res.status}: ${text}`);
    }

    let data: AIResponse;

    try {
      data = JSON.parse(text);
    } catch (e) {
      console.error("JSON PARSE ERROR:", e);
      throw new Error("Invalid JSON from AI");
    }

    console.log("===== AI PARSED =====");
    console.log(data);

    return data?.response || "No response";
  } catch (err: any) {
    console.error("===== AI ERROR =====");
    console.error(err);

    throw err;
  }
}