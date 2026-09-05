const BRIDGE_URL = "http://127.0.0.1:8787";
let lastCommandId = "";

figma.showUI(__html__, { width: 300, height: 220 });

figma.ui.onmessage = async (message) => {
  if (message.type === "pull") {
    await pullCommand();
  }

  if (message.type === "export-home") {
    await exportHome();
  }
};

async function pullCommand() {
  try {
    const response = await fetch(`${BRIDGE_URL}/command`);
    const command = await response.json();
    if (!command.id || command.id === lastCommandId) {
      notify("No new command");
      return;
    }

    lastCommandId = command.id;
    if (command.type === "build_home") {
      await buildHome(command.spec);
      notify(`Applied ${command.id}`);
      return;
    }

    notify(`Unknown command: ${command.type}`);
  } catch (error) {
    notify(`Bridge error: ${error.message}`);
  }
}

async function buildHome(spec) {
  await figma.loadFontAsync({ family: "Inter", style: "Regular" });
  await figma.loadFontAsync({ family: "Inter", style: "Bold" });

  const existing = figma.currentPage.findOne((node) => node.name === "Home" && node.type === "FRAME");
  if (existing) {
    existing.remove();
  }

  const home = figma.createFrame();
  home.name = "Home";
  home.resize(900, 600);
  home.x = 80;
  home.y = 80;
  home.fills = [paint("0B1220")];
  home.setPluginData("screen", "home");

  const title = createText(home, "Title", spec.title, 34, 40, 32, true);
  title.setPluginData("role", "title");

  const subtitle = createText(home, "Subtitle", spec.subtitle, 16, 40, 88, false);
  subtitle.setPluginData("role", "subtitle");

  const grid = figma.createFrame();
  grid.name = "LevelGrid";
  grid.x = 40;
  grid.y = 170;
  grid.resize(820, 220);
  grid.fills = [];
  grid.setPluginData("role", "level-grid");
  home.appendChild(grid);

  const buttons = spec.buttons || [];
  buttons.forEach((buttonSpec, index) => {
    const card = figma.createFrame();
    card.name = `Level${buttonSpec.level}`;
    card.resize(148, 180);
    card.x = index * 164;
    card.y = 20;
    card.cornerRadius = 12;
    card.fills = [paint(buttonSpec.level === 1 ? "5EEAD4" : "243552")];
    card.setPluginData("role", "level-button");
    card.setPluginData("level", String(buttonSpec.level));
    card.setPluginData("target", "gameplay");
    grid.appendChild(card);

    const label = createText(card, "Label", buttonSpec.label, 18, 20, 55, true);
    label.textAlignHorizontal = "CENTER";
    label.textAlignVertical = "CENTER";
    label.resize(108, 70);
    label.setPluginData("role", "button-label");
  });

  const hint = createText(home, "Hint", spec.hint, 14, 40, 470, false);
  hint.setPluginData("role", "hint");

  figma.currentPage.selection = [home];
  figma.viewport.scrollAndZoomIntoView([home]);
}

function createText(parent, name, characters, fontSize, x, y, bold) {
  const text = figma.createText();
  text.name = name;
  text.fontName = { family: "Inter", style: bold ? "Bold" : "Regular" };
  text.fontSize = fontSize;
  text.characters = characters || "";
  text.fills = [paint("F8FAFC")];
  text.x = x;
  text.y = y;
  parent.appendChild(text);
  return text;
}

async function exportHome() {
  const home = figma.currentPage.findOne((node) => node.name === "Home" && node.type === "FRAME");
  if (!home) {
    notify("Home frame not found");
    return;
  }

  const title = findText(home, "Title");
  const subtitle = findText(home, "Subtitle");
  const hint = findText(home, "Hint");
  const grid = home.findOne((node) => node.name === "LevelGrid" && node.type === "FRAME");
  const buttons = grid
    ? grid.children
        .filter((node) => node.type === "FRAME" && node.getPluginData("role") === "level-button")
        .map((node) => {
          const level = Number(node.getPluginData("level"));
          const label = findText(node, "Label");
          return { level, label: label ? label.characters : node.name };
        })
        .sort((a, b) => a.level - b.level)
    : [];

  const spec = {
    screen: "home",
    title: title ? title.characters : "IDLE // CIRCUIT",
    subtitle: subtitle ? subtitle.characters : "SELECT A LEVEL",
    hint: hint ? hint.characters : "CHOOSE A LEVEL TO START THE RUN",
    buttons,
  };

  try {
    await fetch(`${BRIDGE_URL}/export`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(spec),
    });
    notify("Exported to Unity");
  } catch (error) {
    notify(`Export error: ${error.message}`);
  }
}

function findText(parent, name) {
  return parent.findOne((node) => node.type === "TEXT" && node.name === name);
}

function paint(hex) {
  return { type: "SOLID", color: hexToRgb(hex) };
}

function hexToRgb(hex) {
  const value = parseInt(hex, 16);
  return {
    r: ((value >> 16) & 255) / 255,
    g: ((value >> 8) & 255) / 255,
    b: (value & 255) / 255,
  };
}

function notify(message) {
  figma.ui.postMessage({ type: "status", message });
}
