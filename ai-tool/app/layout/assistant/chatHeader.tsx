export default function ChatHeader({ onOpen }: any) {
  return (
    <div className="p-3 border-b flex items-center gap-2">
      <button onClick={onOpen}>☰</button>
      <span className="font-semibold">AI Assistant</span>
    </div>
  );
}