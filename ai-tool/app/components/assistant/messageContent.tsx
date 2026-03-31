"use client";

import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";

export default function MessageContent({ content }: { content: string }) {
    return (
        <div className="prose max-w-none text-sm">
            <ReactMarkdown
                remarkPlugins={[remarkGfm]}
                components={{
                    code({ node, className, children, ...props }) {
                        const isInline = !className;

                        if (isInline) {
                            return (
                                <code className="bg-gray-200 px-1 rounded text-red-500">
                                    {children}
                                </code>
                            );
                        }

                        return (
                            <pre className="bg-black text-green-400 p-3 rounded overflow-auto">
                                <code className={className} {...props}>
                                    {children}
                                </code>
                            </pre>
                        );
                    },
                }}
            >
                {content}
            </ReactMarkdown>
        </div>
    );
}