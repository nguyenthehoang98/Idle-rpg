figma.showUI(__html__, { width: 360, height: 420, themeColors: true });

const MAX_NAME_LENGTH = 120;
const MAX_TEXT_LENGTH = 2000;

function fail(code, message) {
  return { ok: false, error: { code, message } };
}

function text(value, fallback = "") {
  return typeof value === "string" ? value.slice(0, MAX_TEXT_LENGTH) : fallback;
}

function name(value, fallback) {
  const result = typeof value === "string" && value.trim() ? value.trim() : fallback;
  return result.slice(0, MAX_NAME_LENGTH);
}

function number(value, fallback = 0, min = -100000, max = 100000) {
  if (typeof value !== "number" || !Number.isFinite(value)) return fallback;
  return Math.max(min, Math.min(max, value));
}

function color(value) {
  if (!value || typeof value !== "object") return null;
  const result = { r: number(value.r, -1, 0, 1), g: number(value.g, -1, 0, 1), b: number(value.b, -1, 0, 1) };
  if (Object.values(result).some((channel) => channel < 0)) return null;
  return result;
}

function getAnyNode(id) {
  if (typeof id !== "string" || !id) return null;
  return figma.getNodeById(id);
}

function getNode(id) {
  const node = getAnyNode(id);
  return node && "appendChild" in node ? node : null;
}

function attach(node, args) {
  const parent = args.parentId ? getNode(args.parentId) : figma.currentPage;
  if (!parent || !("appendChild" in parent)) throw new Error("Parent node is not available");
  parent.appendChild(node);
  node.x = number(args.x);
  node.y = number(args.y);
  return node;
}

function resize(node, args, defaultWidth = 1080, defaultHeight = 2400) {
  node.resizeWithoutConstraints(
    number(args.width, defaultWidth, 1, 10000),
    number(args.height, defaultHeight, 1, 10000),
  );
}

function resultFor(node) {
  return { nodeId: node.id, name: node.name, type: node.type };
}

function selectNode(node) {
  let page = node;
  while (page && page.type !== "PAGE") page = page.parent;
  if (page && page.type === "PAGE") {
    figma.currentPage = page;
    figma.currentPage.selection = [node];
  }
}

async function execute(command) {
  if (!command || typeof command.op !== "string") return fail("INVALID_COMMAND", "Missing operation");
  const args = command.args && typeof command.args === "object" ? command.args : {};

  switch (command.op) {
    case "ping":
      return { ok: true, message: "Figma plugin connected", pageId: figma.currentPage.id };
    case "create_page": {
      const page = figma.createPage();
      page.name = name(args.name, "AI Draft");
      figma.currentPage = page;
      figma.currentPage.selection = [];
      return { ok: true, node: resultFor(page) };
    }
    case "create_frame": {
      const frame = figma.createFrame();
      frame.name = name(args.name, "Draft Frame");
      resize(frame, args);
      attach(frame, args);
      selectNode(frame);
      return { ok: true, node: resultFor(frame) };
    }
    case "create_rectangle": {
      const rectangle = figma.createRectangle();
      rectangle.name = name(args.name, "Rectangle");
      resize(rectangle, args, 240, 120);
      attach(rectangle, args);
      selectNode(rectangle);
      return { ok: true, node: resultFor(rectangle) };
    }
    case "create_text": {
      const font = { family: name(args.fontFamily, "Inter"), style: "Regular" };
      await figma.loadFontAsync(font);
      const textNode = figma.createText();
      textNode.name = name(args.name, "Text");
      textNode.characters = text(args.text, "Draft text");
      textNode.fontSize = number(args.fontSize, 24, 8, 240);
      attach(textNode, args);
      selectNode(textNode);
      return { ok: true, node: resultFor(textNode) };
    }
    case "set_fill": {
      const node = getAnyNode(args.nodeId);
      const fill = color(args.color);
      if (!node || !fill || !("fills" in node)) return fail("INVALID_NODE", "Node or color is invalid");
      node.fills = [{ type: "SOLID", color: fill, opacity: number(args.opacity, 1, 0, 1) }];
      return { ok: true, node: resultFor(node) };
    }
    case "set_stroke": {
      const node = getAnyNode(args.nodeId);
      const stroke = color(args.color);
      if (!node || !stroke || !("strokes" in node)) return fail("INVALID_NODE", "Node or color is invalid");
      node.strokes = [{ type: "SOLID", color: stroke, opacity: number(args.opacity, 1, 0, 1) }];
      node.strokeWeight = number(args.weight, 1, 0, 100);
      return { ok: true, node: resultFor(node) };
    }
    case "set_corner_radius": {
      const node = getAnyNode(args.nodeId);
      if (!node || !("cornerRadius" in node)) return fail("INVALID_NODE", "Node does not support corner radius");
      node.cornerRadius = number(args.radius, 0, 0, 500);
      return { ok: true, node: resultFor(node) };
    }
    case "set_name": {
      const node = getAnyNode(args.nodeId);
      if (!node) return fail("INVALID_NODE", "Node is not available");
      node.name = name(args.name, node.name);
      return { ok: true, node: resultFor(node) };
    }
    case "select": {
      const node = getAnyNode(args.nodeId);
      if (!node) return fail("INVALID_NODE", "Node is not available");
      selectNode(node);
      figma.viewport.scrollAndZoomIntoView([node]);
      return { ok: true, node: resultFor(node) };
    }
    default:
      return fail("OPERATION_NOT_ALLOWED", "Operation is not implemented by the safe plugin");
  }
}

figma.ui.onmessage = async (message) => {
  if (!message || message.type !== "execute-command") return;
  let result;
  try {
    result = await execute(message.command);
  } catch (error) {
    result = fail("PLUGIN_ERROR", error instanceof Error ? error.message : "Unknown plugin error");
  }
  figma.ui.postMessage({ type: "command-result", id: message.command && message.command.id, result });
};
