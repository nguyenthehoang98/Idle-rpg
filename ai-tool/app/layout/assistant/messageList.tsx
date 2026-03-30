export default function MessageList({ messages, isTyping }: any) {
  return (
    <div className="p-4 space-y-3">
      {messages.map((m: any) => (
        <div
          key={m.id}
          className={`p-2 rounded max-w-[70%] ${
            m.role === "user"
              ? "bg-blue-500 text-white ml-auto"
              : "bg-gray-200"
          }`}
        >
          {m.content}
        </div>
      ))}

      {isTyping && (
        <div className="text-gray-400 text-sm">AI is typing...</div>
      )}
    </div>
  );
}