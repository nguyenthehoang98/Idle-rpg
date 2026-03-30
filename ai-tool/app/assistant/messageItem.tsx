export default function MessageItem({ message }: any) {
  return (
    <div
      className={`max-w-[70%] p-3 rounded ${
        message.role === "user"
          ? "bg-blue-500 text-white ml-auto"
          : "bg-gray-200"
      }`}
    >
      {message.content}
    </div>
  );
}