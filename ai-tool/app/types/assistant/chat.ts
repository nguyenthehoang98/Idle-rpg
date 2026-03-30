// /types/chat.ts

export type Message = {
  id: string;
  role: "user" | "assistant";
  content: string;
  createdAt?: string;
};

export type Chat = {
  id: string;
  title: string;
  messages: Message[];
  createdAt?: string;
};

export const db: Record<string, Message[]> = {};