export default function Sidebar({ onClose }: any) {
  const history = [
    "Fix Unity crash",
    "Shader optimize",
    "Leaderboard design",
  ];

  return (
    <div className="h-full p-3 flex flex-col">

      <div className="flex justify-between mb-4">
        <span className="font-semibold">History</span>
        <button onClick={onClose}>✕</button>
      </div>

      <div className="flex-1 overflow-y-auto space-y-2">
        {history.map((item, i) => (
          <div
            key={i}
            className="p-2 rounded hover:bg-gray-200 cursor-pointer"
          >
            {item}
          </div>
        ))}
      </div>
    </div>
  );
}