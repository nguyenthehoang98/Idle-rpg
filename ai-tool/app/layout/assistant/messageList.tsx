import MessageContent from "@/app/components/assistant/messageContent";

export default function MessageList({ messages, isTyping }: any) {
  return (
    <div className="p-4 space-y-3">
      {messages.map((m:any) => (
  <div
    key={m.id}
    className={`p-3 my-2 rounded-lg max-w-[80%] ${
      m.role === "user"
        ? "bg-blue-500 text-white ml-auto"
        : "bg-gray-100 text-black"
    }`}
  >
    <MessageContent content={m.content} />
  </div>
))}

      {isTyping && (
        <div className="flex items-center gap-2 text-gray-500">
          <div className="w-4 h-4 border-2 border-gray-300 border-t-blue-500 rounded-full animate-spin" />
          <span>AI đang suy nghĩ...</span>
        </div>
      )}
    </div>
  );
}