export async function POST(req: Request) {
  const { message } = await req.json();

  return new Response(
    JSON.stringify({ reply: "Hello from AI 🤖" }),
    { headers: { "Content-Type": "application/json" } }
  );
}