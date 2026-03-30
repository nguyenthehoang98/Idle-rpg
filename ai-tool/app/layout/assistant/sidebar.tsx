"use client";

export default function Sidebar({
  chats,
  onSelect,
  onNewChat,
  openSidebar,
  setOpenSidebar,
}: any) {
  const baseBtn =
    "flex items-center p-2 rounded-lg cursor-pointer hover:bg-gray-200 transition";

  const textClass = `whitespace-nowrap overflow-hidden transition-all duration-300`;

  const textState = openSidebar
    ? "opacity-100 translate-x-0 ml-2 max-w-[200px]"
    : "opacity-0 -translate-x-2 ml-0 max-w-0";

  return (
    <div
      className={`h-full border-r bg-white
      transition-all duration-300
      ${openSidebar ? "w-64" : "w-16"}`}
    >
      <div className="p-2 space-y-1">
        {/* Toggle */}
        <div
          onClick={() => setOpenSidebar(!openSidebar)}
          className={baseBtn}
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            className="w-5 h-5 shrink-0"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            strokeWidth={2}
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M4 6h16M4 12h16M4 18h16"
            />
          </svg>

          <span className={`${textClass} ${textState}`}>
            Menu
          </span>
        </div>

        {/* Divider */}
        <div className="my-2 h-px bg-gray-200"></div>

        {/* New Chat */}
        <div
          onClick={onNewChat}
          title="New Chat"
          className={baseBtn}
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            className="w-5 h-5 shrink-0"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            strokeWidth={2}
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M12 4v16m8-8H4"
            />
          </svg>

          <span className={`${textClass} ${textState}`}>
            New Chat
          </span>
        </div>

        {/* Chat list */}
        <div className="pt-2 space-y-1">
          {chats.map((c: any) => (
            <div
              key={c.id}
              onClick={() => onSelect(c.id)}
              className={baseBtn}
              title={c.title || "New Chat"}
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                className="w-4 h-4 text-gray-500 shrink-0"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                strokeWidth={2}
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M8 10h.01M12 10h.01M16 10h.01M21 12c0 4.418-4.03 8-9 8a9.77 9.77 0 01-4-.8L3 20l1.8-3.2A7.96 7.96 0 013 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"
                />
              </svg>

              <span
                className={`text-sm truncate ${textClass} ${textState}`}
              >
                {c.title || "New Chat"}
              </span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}