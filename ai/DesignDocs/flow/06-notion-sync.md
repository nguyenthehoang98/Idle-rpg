# 06 - Notion Sync

Mục tiêu: đồng bộ tài liệu thiết kế giữa Unity project và Notion page.

Parent Notion page:

```text
Tower Defense Game
Page ID: 3ac41ed0c2cd804f9bc4f5cdb96fc344
```

## 1. Cảnh báo bảo mật

Không commit API key.

File chứa key local ở folder parent:

```text
C:/Users/Hoang PC/Documents/Idle-rpg/notion/tools/notion-sync/.env
```

File này đã được ignore trong `.gitignore`.

Nếu token từng bị gửi công khai/chat, nên rotate token trong Notion integration.

## 2. Setup

Từ folder parent `C:/Users/Hoang PC/Documents/Idle-rpg`, copy file mẫu:

```powershell
copy tools\notion-sync\.env.example tools\notion-sync\.env
```

Sửa `notion/tools/notion-sync/.env`:

```env
NOTION_TOKEN=ntn_xxxxxxxxxxxxxxxxx
NOTION_PARENT_PAGE_ID=3ac41ed0c2cd804f9bc4f5cdb96fc344
DOCS_DIR=ai/DesignDocs
PULL_DIR=ai/DesignDocs/_notion_pull
```

## 3. Share Notion Page cho integration

Trong Notion:

```text
Open page Tower Defense Game
-> Share
-> Invite
-> chọn Notion integration
-> Invite
```

Nếu chưa share, script sẽ lỗi `object_not_found` hoặc `404`.

## 4. Commands

Từ folder parent `C:/Users/Hoang PC/Documents/Idle-rpg`:

```powershell
node notion/tools/notion-sync/sync.js check
node notion/tools/notion-sync/sync.js push
node notion/tools/notion-sync/sync.js pull
```

Hoặc double click:

```text
C:/Users/Hoang PC/Documents/Idle-rpg/notion/notion-push.bat
C:/Users/Hoang PC/Documents/Idle-rpg/notion/notion-pull.bat
```

Hoặc:

```powershell
cd notion/tools/notion-sync
npm run check
npm run push
npm run pull
```

## 5. Unity -> Notion

Command:

```powershell
node notion/tools/notion-sync/sync.js push
```

Behavior:

```text
- Đọc tất cả file .md trong ai/DesignDocs
- Tạo child page dưới Notion parent page
- Tên page = đường dẫn tương đối trong ai/DesignDocs
- Nếu page đã tồn tại, xóa nội dung cũ và append nội dung mới
- Lưu map ở notion/tools/notion-sync/.notion-sync-map.json
```

Ví dụ page tạo trên Notion:

```text
flow/00-agent-skills-integration
flow/01-interview
flow/02-spec
flow/03-technical-design
flow/04-plan
flow/05-quality-gates
adrs/ADR-0001-project-structure
tasks/000-design-backlog
```

## 6. Notion -> Unity

Command:

```powershell
node notion/tools/notion-sync/sync.js pull
```

Behavior:

```text
- Đọc các child page dưới Notion parent page
- Xuất markdown vào ai/DesignDocs/_notion_pull
- Không tự ghi đè file nguồn
```

Lý do không ghi đè trực tiếp:

```text
- Tránh conflict khi Notion và Unity đều có thay đổi
- Cho phép review diff trước khi merge thủ công
```

## 7. Two-way sync policy

Chiều chính:

```text
Unity markdown -> Notion để review
```

Chiều ngược:

```text
Notion -> _notion_pull -> review -> merge thủ công
```

Không auto-merge 2 chiều ở giai đoạn đầu vì Notion block format và Markdown format không hoàn toàn tương đương.

## 8. Known limitations

```text
- Markdown table chưa convert thành Notion table thật; hiện xử lý như paragraph/code đơn giản.
- Nested blocks chưa sync sâu.
- Image/file attachment chưa hỗ trợ.
- Pull từ Notion là backup/review, không thay thế source docs ngay.
```
