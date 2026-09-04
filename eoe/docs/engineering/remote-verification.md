# Remote Verification

## Mục tiêu

Có thể verify game từ terminal hoặc CI mà không cần mở Unity Editor bằng UI. Pi agent đọc exit code, log và test result XML để quyết định task pass/fail.

## Nguyên tắc

- Unity chạy `-batchmode -nographics`.
- Compile và test không phụ thuộc thao tác chuột trong Editor.
- Logic mới ưu tiên test thuần state/simulation.
- Gameplay flow có headless PlayMode smoke test.
- Không đánh dấu todo hoàn tất nếu chỉ compile pass mà chưa có test phù hợp.

## Local/remote command

Từ project root:

```powershell
powershell -ExecutionPolicy Bypass -File tools/verify-unity.ps1 -Mode all
```

Các mode:

```powershell
# Chỉ compile script assemblies
powershell -ExecutionPolicy Bypass -File tools/verify-unity.ps1 -Mode compile

# Chỉ chạy EditMode tests
powershell -ExecutionPolicy Bypass -File tools/verify-unity.ps1 -Mode editmode

# Chạy PlayMode smoke tests khi test harness đã có
powershell -ExecutionPolicy Bypass -File tools/verify-unity.ps1 -Mode playmode
```

Nếu Unity không nằm ở path mặc định, set:

```powershell
$env:UNITY_PATH = 'C:\Program Files\Unity\Hub\Editor\6000.3.21f1\Editor\Unity.exe'
powershell -ExecutionPolicy Bypass -File tools/verify-unity.ps1 -Mode all
```

Script trả exit code khác 0 nếu:

- Unity process fail.
- Compile log có compile error.
- Test result không phải `Passed`.
- Không tìm thấy test result XML.

Log và XML được lưu trong `Temp/Verification/`. Đây là artifact để agent đọc khi debug.

## Các tầng kiểm thử

### Level 0 - Static/diff check

Chạy trước mọi task:

```powershell
git diff --check
git status --short
```

Mục đích: bắt whitespace lỗi, file ngoài scope và file generated bị thay đổi.

### Level 1 - Compile

```powershell
powershell -ExecutionPolicy Bypass -File tools/verify-unity.ps1 -Mode compile
```

Mục đích: xác nhận Unity script assemblies compile được. Đây là gate bắt buộc trước khi chuyển task.

### Level 2 - EditMode tests

```powershell
powershell -ExecutionPolicy Bypass -File tools/verify-unity.ps1 -Mode editmode
```

Dùng cho:

- Energy Circuit pulse/stack.
- Shop transaction.
- Board mutation.
- Augment state.
- Run reset.

Đây là tầng test chính vì nhanh và không cần scene/rendering.

### Level 3 - Headless PlayMode smoke test

Sau khi có gameplay integration, thêm test chạy được bằng:

```powershell
powershell -ExecutionPolicy Bypass -File tools/verify-unity.ps1 -Mode playmode

# Có thể thêm `-testFilter` vào script sau khi smoke test đã tồn tại.
```

Smoke test phải:

1. Tạo run mới.
2. Load gameplay scene hoặc test scene tối thiểu.
3. Chạy qua các tick cần thiết.
4. Cho bot mua/sắp xếp một build mặc định.
5. Chạy đến checkpoint Augment.
6. Chạy đến boss hoặc timeout rõ ràng.
7. Ghi kết quả: wave hiện tại, gold, activation count, monster killed, win/fail.

Không cần kiểm tra chất lượng hình ảnh ở level này. Mục tiêu là bắt lỗi flow, null reference, event duplicate, state reset và crash runtime.

### Level 4 - Visual/manual review

Chỉ dùng khi cần kiểm tra:

- VFX/SFX Overdrive.
- Readability của pulse/stack.
- UI Shop/Augment.
- Cảm giác dọn quái.

Level này là bổ sung, không thay thế Level 0-3.

## Agent self-play

Pi agent không cần tự điều khiển chuột để test logic. Thay vào đó dùng deterministic bot:

- Seed cố định.
- Có chiến lược mua tối thiểu: ưu tiên Generator, Amplifier, DPS hero.
- Chọn Augment theo tag đã định trước.
- Không dùng random không seed trong test.
- Test xuất summary dạng log/XML.

Ví dụ acceptance của self-play MVP:

```text
run started
wave 1 cleared
shop completed
wave 2 cleared
shop completed
wave 3 cleared
augment selected: Overcharge
shop completed
wave 4 cleared
boss started
run completed
```

Nếu self-play fail, agent đọc artifact và sửa theo lỗi đầu tiên, không bỏ qua test.

## CI sau khi core loop ổn định

Khi script local ổn định, dùng chính script này trong CI:

1. Checkout repo.
2. Cài/chuẩn bị đúng Unity version.
3. Set `UNITY_PATH` hoặc dùng Unity builder.
4. Chạy `tools/verify-unity.ps1 -Mode all`.
5. Upload `Temp/Verification/` khi fail.
6. Chặn merge nếu compile hoặc test fail.

Chưa cần dựng CI ngay trước khi test contract và command local ổn định.

## Definition of Done cho một feature task

- Có test phù hợp với logic mới.
- `git diff --check` pass.
- Compile pass.
- EditMode tests pass.
- Nếu task chạm gameplay flow: headless PlayMode smoke test pass.
- Cập nhật checkbox/status trong `tasks/todo.md`.
- Commit chỉ chứa một logical increment.
