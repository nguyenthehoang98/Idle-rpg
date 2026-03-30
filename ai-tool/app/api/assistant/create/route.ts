let chats: any[] = []; // fake DB

export async function POST(req: Request) {
  console.log("API CREATE HIT"); 
  const body = await req.json();

  const newChat = {
    id: Date.now().toString(),
    title: body.message?.slice(0, 30) || "New Chat",
    createdAt: Date.now(),
  };

  return Response.json(newChat);
}

export async function GET() {
  return Response.json(chats);
}