import { Message } from "@/app/types/assistant/chat";

export default function MessageList({
  messages = [],
}: {
  messages?: Message[];
}) {
  return (
    <div className="flex-1 overflow-y-auto p-4 space-y-4">
      {messages.map((m) => (
        <div
          key={m.id}
          className={`max-w-[70%] p-3 rounded ${
            m.role === "user"
              ? "bg-blue-500 text-white ml-auto"
              : "bg-gray-200"
          }`}
        >
          {m.content}
        </div>
      ))}
    </div>
  );
}