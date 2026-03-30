import MessageItem from "@/app/assistant/messageItem";

export default function MessageList() {
  const messages = [
    { role: "user", content: "Hello" },
    { role: "assistant", content: "Hi 👋" },
  ];

  return (
    <div className="flex-1 overflow-y-auto p-4 space-y-4">
      {messages.map((m, i) => (
        <MessageItem key={i} message={m} />
      ))}
    </div>
  );
}