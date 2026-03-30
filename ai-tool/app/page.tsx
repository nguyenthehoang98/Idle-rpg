import tools from "@/app/configs/dashboard.json";
import { iconMap } from "@/app/lib/iconMap";
import Link from "next/link";
import { Card, CardContent } from "@/components/ui/card";

export default function Dashboard() {
  return (
    <main className="p-8">
      <h1 className="text-3xl font-bold mb-6">AI Tool Dashboard 🚀</h1>

      <div className="grid grid-cols-3 gap-6">
        {tools.map((tool: any, index: number) => {
          const Icon = iconMap[tool.icon];

          return (
            <Link key={index} href={tool.href}>
              <Card className="hover:shadow-xl transition">
                <CardContent className="p-6">
                  <div className="flex gap-2 items-center mb-2">
                    {Icon && <Icon className="w-5 h-5" />}
                    <h2>{tool.title}</h2>
                  </div>
                  <p className="text-sm text-gray-500">
                    {tool.desc}
                  </p>
                </CardContent>
              </Card>
            </Link>
          );
        })}
      </div>
    </main>
  );
}