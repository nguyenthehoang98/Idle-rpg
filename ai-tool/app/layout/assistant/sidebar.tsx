"use client";

export default function Sidebar({ chats, onSelect }: any) {
  return (
    <div>
      <div className="w-full text-center p-3 font-semibold border-b">
        History
      </div>

      {chats.map((c: any) => (
        <div
          key={c.id}
          onClick={() => onSelect(c.id)}
          className="p-2 cursor-pointer hover:bg-gray-200"
        >
          {c.title || "New Chat"}
        </div>
      ))}
    </div>
  );
}
