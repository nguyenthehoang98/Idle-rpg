import fs from 'node:fs';
import path from 'node:path';
import process from 'node:process';
import { fileURLToPath } from 'node:url';

const NOTION_VERSION = '2022-06-28';
const TOOL_DIR = path.dirname(fileURLToPath(import.meta.url));
const ROOT = path.resolve(TOOL_DIR, '../../..');
const MAP_FILE = path.join(TOOL_DIR, '.notion-sync-map.json');

loadEnv(path.join(TOOL_DIR, '.env'));
loadEnv(path.join(ROOT, '.env'));

const TOKEN = process.env.NOTION_TOKEN;
const PARENT_PAGE_ID = normalizeId(process.env.NOTION_PARENT_PAGE_ID || '');
const DOCS_DIR = path.resolve(ROOT, process.env.DOCS_DIR || 'Assets/_DesignDocs');
const PULL_DIR = path.resolve(ROOT, process.env.PULL_DIR || 'Assets/_DesignDocs/_notion_pull');
const mode = process.argv[2] || 'help';

if (mode === 'help') {
  printHelp();
  process.exit(0);
}

if (!TOKEN || !PARENT_PAGE_ID) {
  console.error('Missing NOTION_TOKEN or NOTION_PARENT_PAGE_ID. Create tools/notion-sync/.env from .env.example.');
  process.exit(1);
}

if (mode === 'check') await checkConnection();
else if (mode === 'push') await pushDocs();
else if (mode === 'pull') await pullDocs();
else {
  console.error(`Unknown mode: ${mode}`);
  printHelp();
  process.exit(1);
}

function printHelp() {
  console.log(`Usage:
  node tools/notion-sync/sync.js check  # verify token + page access
  node tools/notion-sync/sync.js push   # Unity markdown -> Notion child pages
  node tools/notion-sync/sync.js pull   # Notion child pages -> markdown backup
`);
}

function loadEnv(file) {
  if (!fs.existsSync(file)) return;
  const text = fs.readFileSync(file, 'utf8');
  for (const line of text.split(/\r?\n/)) {
    const trimmed = line.trim();
    if (!trimmed || trimmed.startsWith('#')) continue;
    const eq = trimmed.indexOf('=');
    if (eq === -1) continue;
    const key = trimmed.slice(0, eq).trim();
    let value = trimmed.slice(eq + 1).trim();
    value = value.replace(/^['"]|['"]$/g, '');
    if (!(key in process.env)) process.env[key] = value;
  }
}

function normalizeId(id) {
  return String(id).trim().replace(/-/g, '');
}

async function notion(method, endpoint, body = undefined) {
  const res = await fetch(`https://api.notion.com/v1${endpoint}`, {
    method,
    headers: {
      Authorization: `Bearer ${TOKEN}`,
      'Notion-Version': NOTION_VERSION,
      'Content-Type': 'application/json'
    },
    body: body ? JSON.stringify(body) : undefined
  });

  const text = await res.text();
  let json;
  try { json = text ? JSON.parse(text) : {}; } catch { json = { raw: text }; }

  if (!res.ok) {
    const msg = json?.message || json?.code || text;
    throw new Error(`${method} ${endpoint} failed ${res.status}: ${msg}`);
  }

  return json;
}

async function checkConnection() {
  const page = await notion('GET', `/pages/${PARENT_PAGE_ID}`);
  const title = getPageTitle(page) || '(untitled)';
  console.log(`Connected to Notion page: ${title}`);
  console.log(`Parent page id: ${PARENT_PAGE_ID}`);
}

async function pushDocs() {
  await checkConnection();

  const files = listMarkdownFiles(DOCS_DIR)
    .filter(file => !file.includes(`${path.sep}_notion_pull${path.sep}`));

  const children = await getChildPages(PARENT_PAGE_ID);
  const map = readMap();

  let pushed = 0;
  for (const file of files) {
    const rel = path.relative(ROOT, file).replaceAll(path.sep, '/');
    const title = titleForFile(file);
    const content = fs.readFileSync(file, 'utf8');
    const blocks = markdownToBlocks(content, rel);

    let pageId = map[rel]?.pageId;
    if (!pageId || !(await pageExists(pageId))) {
      pageId = children.get(title)?.id;
    }

    if (!pageId) {
      const page = await createChildPage(PARENT_PAGE_ID, title);
      pageId = page.id;
      console.log(`Created: ${title}`);
    } else {
      await clearChildren(pageId);
      console.log(`Updated: ${title}`);
    }

    await appendBlocks(pageId, blocks);
    map[rel] = { pageId, title, lastPushedAt: new Date().toISOString() };
    pushed++;
  }

  writeMap(map);
  console.log(`Done. Pushed ${pushed} markdown files to Notion.`);
}

async function pullDocs() {
  await checkConnection();
  fs.mkdirSync(PULL_DIR, { recursive: true });

  const pages = await getChildPages(PARENT_PAGE_ID);
  let pulled = 0;

  for (const [title, page] of pages) {
    const blocks = await getAllChildren(page.id);
    const markdown = blocksToMarkdown(title, blocks);
    const file = path.join(PULL_DIR, `${slugify(title)}.md`);
    fs.writeFileSync(file, markdown, 'utf8');
    console.log(`Pulled: ${title} -> ${path.relative(ROOT, file).replaceAll(path.sep, '/')}`);
    pulled++;
  }

  console.log(`Done. Pulled ${pulled} Notion child pages.`);
}

function readMap() {
  if (!fs.existsSync(MAP_FILE)) return {};
  return JSON.parse(fs.readFileSync(MAP_FILE, 'utf8'));
}

function writeMap(map) {
  fs.writeFileSync(MAP_FILE, JSON.stringify(map, null, 2), 'utf8');
}

async function pageExists(pageId) {
  try {
    await notion('GET', `/pages/${normalizeId(pageId)}`);
    return true;
  } catch {
    return false;
  }
}

async function getChildPages(parentId) {
  const pages = new Map();
  let cursor;
  do {
    const qs = cursor ? `?page_size=100&start_cursor=${cursor}` : '?page_size=100';
    const list = await notion('GET', `/blocks/${parentId}/children${qs}`);
    for (const block of list.results || []) {
      if (block.type === 'child_page') {
        pages.set(block.child_page.title, { id: block.id, title: block.child_page.title });
      }
    }
    cursor = list.has_more ? list.next_cursor : undefined;
  } while (cursor);
  return pages;
}

async function getAllChildren(blockId) {
  const blocks = [];
  let cursor;
  do {
    const qs = cursor ? `?page_size=100&start_cursor=${cursor}` : '?page_size=100';
    const list = await notion('GET', `/blocks/${blockId}/children${qs}`);
    blocks.push(...(list.results || []));
    cursor = list.has_more ? list.next_cursor : undefined;
  } while (cursor);
  return blocks;
}

async function clearChildren(blockId) {
  const blocks = await getAllChildren(blockId);
  for (const block of blocks) {
    await notion('PATCH', `/blocks/${block.id}`, { archived: true });
  }
}

async function createChildPage(parentId, title) {
  return notion('POST', '/pages', {
    parent: { type: 'page_id', page_id: parentId },
    properties: {
      title: {
        title: [{ type: 'text', text: { content: title } }]
      }
    }
  });
}

async function appendBlocks(pageId, blocks) {
  for (let i = 0; i < blocks.length; i += 100) {
    await notion('PATCH', `/blocks/${pageId}/children`, { children: blocks.slice(i, i + 100) });
  }
}

function listMarkdownFiles(dir) {
  if (!fs.existsSync(dir)) return [];
  const out = [];
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) out.push(...listMarkdownFiles(full));
    else if (entry.isFile() && entry.name.toLowerCase().endsWith('.md')) out.push(full);
  }
  return out.sort();
}

function titleForFile(file) {
  const rel = path.relative(DOCS_DIR, file).replaceAll(path.sep, '/');
  return rel.replace(/\.md$/i, '');
}

function markdownToBlocks(markdown, sourcePath) {
  const blocks = [calloutBlock(`Synced from Unity project: ${sourcePath}`)];
  const lines = markdown.split(/\r?\n/);
  let inCode = false;
  let codeLang = 'plain text';
  let codeLines = [];

  for (const line of lines) {
    if (line.startsWith('```')) {
      if (!inCode) {
        inCode = true;
        codeLang = normalizeLanguage(line.slice(3).trim());
        codeLines = [];
      } else {
        blocks.push(...codeBlocks(codeLines.join('\n'), codeLang));
        inCode = false;
      }
      continue;
    }

    if (inCode) {
      codeLines.push(line);
      continue;
    }

    if (!line.trim()) continue;

    const h = line.match(/^(#{1,3})\s+(.+)$/);
    if (h) {
      blocks.push(headingBlock(h[1].length, h[2]));
      continue;
    }

    const bullet = line.match(/^[-*]\s+(.+)$/);
    if (bullet) {
      blocks.push(listBlock('bulleted_list_item', bullet[1]));
      continue;
    }

    const numbered = line.match(/^\d+\.\s+(.+)$/);
    if (numbered) {
      blocks.push(listBlock('numbered_list_item', numbered[1]));
      continue;
    }

    blocks.push(...paragraphBlocks(line));
  }

  if (inCode) blocks.push(...codeBlocks(codeLines.join('\n'), codeLang));
  return blocks;
}

function richText(content) {
  return [{ type: 'text', text: { content: String(content).slice(0, 2000) } }];
}

function paragraphBlocks(text) {
  return chunk(text, 1900).map(part => ({ type: 'paragraph', paragraph: { rich_text: richText(part) } }));
}

function headingBlock(level, text) {
  const type = level === 1 ? 'heading_1' : level === 2 ? 'heading_2' : 'heading_3';
  return { type, [type]: { rich_text: richText(text) } };
}

function listBlock(type, text) {
  return { type, [type]: { rich_text: richText(text) } };
}

function codeBlocks(text, language) {
  return chunk(text || ' ', 1900).map(part => ({
    type: 'code',
    code: { rich_text: richText(part), language }
  }));
}

function calloutBlock(text) {
  return {
    type: 'callout',
    callout: { rich_text: richText(text), icon: { type: 'emoji', emoji: '🔄' } }
  };
}

function chunk(text, size) {
  const result = [];
  const s = String(text);
  for (let i = 0; i < s.length; i += size) result.push(s.slice(i, i + size));
  return result.length ? result : [''];
}

function normalizeLanguage(lang) {
  const l = (lang || 'plain text').toLowerCase();
  const allowed = new Set(['abap','arduino','bash','basic','c','clojure','coffeescript','c++','c#','css','dart','diff','docker','elixir','elm','erlang','flow','fortran','f#','gherkin','glsl','go','graphql','groovy','haskell','html','java','javascript','json','julia','kotlin','latex','less','lisp','livescript','lua','makefile','markdown','markup','matlab','mermaid','nix','objective-c','ocaml','pascal','perl','php','plain text','powershell','prolog','protobuf','python','r','reason','ruby','rust','sass','scala','scheme','scss','shell','sql','swift','typescript','vb.net','verilog','vhdl','visual basic','webassembly','xml','yaml','java/c/c++/c#']);
  if (l === 'cs' || l === 'csharp') return 'c#';
  if (l === 'sh') return 'shell';
  if (l === 'md') return 'markdown';
  if (l === 'yml') return 'yaml';
  if (l === 'js') return 'javascript';
  if (l === 'ts') return 'typescript';
  return allowed.has(l) ? l : 'plain text';
}

function blocksToMarkdown(title, blocks) {
  const out = [`# ${title}`, '', '<!-- Pulled from Notion. Review before overwriting source design docs. -->', ''];
  for (const block of blocks) {
    const type = block.type;
    if (type === 'paragraph') out.push(textOf(block.paragraph.rich_text), '');
    else if (type === 'heading_1') out.push(`# ${textOf(block.heading_1.rich_text)}`, '');
    else if (type === 'heading_2') out.push(`## ${textOf(block.heading_2.rich_text)}`, '');
    else if (type === 'heading_3') out.push(`### ${textOf(block.heading_3.rich_text)}`, '');
    else if (type === 'bulleted_list_item') out.push(`- ${textOf(block.bulleted_list_item.rich_text)}`);
    else if (type === 'numbered_list_item') out.push(`1. ${textOf(block.numbered_list_item.rich_text)}`);
    else if (type === 'code') out.push('```' + (block.code.language || ''), textOf(block.code.rich_text), '```', '');
    else if (type === 'callout') out.push(`> ${textOf(block.callout.rich_text)}`, '');
  }
  return out.join('\n').replace(/\n{4,}/g, '\n\n\n');
}

function textOf(rich = []) {
  return rich.map(r => r.plain_text || r.text?.content || '').join('');
}

function getPageTitle(page) {
  const props = page.properties || {};
  for (const value of Object.values(props)) {
    if (value?.type === 'title') return textOf(value.title);
  }
  return '';
}

function slugify(text) {
  return text.toLowerCase().replace(/[^a-z0-9\-_\/]+/g, '-').replace(/[\/]+/g, '__').replace(/^-+|-+$/g, '') || 'untitled';
}
