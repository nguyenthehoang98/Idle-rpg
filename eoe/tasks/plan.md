# Implementation Plan: Energy Circuit MVP

## Trạng thái

**Draft - chỉ bắt đầu implement sau khi gameplay spec được xác nhận.**

## Overview

Thêm Energy Circuit, Overdrive, Shop giữa wave và Augment checkpoint vào combat hiện tại. Giữ nguyên core auto-combat và wave spawn; chỉ thêm lớp chuẩn bị/build để người chơi có quyết định.

## Progress snapshot

| Hạng mục | Trạng thái | Ghi chú |
|---|---|---|
| Game design docs | Done | MVP, Energy Circuit, Shop/Augment đã ghi lại |
| Existing auto-combat | Baseline ready | Hero, monster, projectile, status effect, wave đã có |
| Circuit simulation | Not started | P0 - cần làm trước các phần UI |
| Combat integration | Not started | Phụ thuộc circuit simulation |
| Shop | Not started | Phụ thuộc board state và run gold |
| Augment | Not started | Phụ thuộc Energy Circuit và Shop flow |
| Content/tuning | Not started | Làm sau khi full loop chạy được |

**Tiến độ feature mới:** Design 100%, implementation 0%.

## Nguyên tắc triển khai

- Ưu tiên simulation logic trước UI.
- Tái sử dụng hero, skill, stat và wave hiện có.
- Không trộn Energy Circuit với RVO movement.
- Mỗi task để project ở trạng thái build được.
- Dùng config/data đơn giản trước, chưa tạo hệ thống generic cho mọi loại effect.

## Phases

### Phase 1: Contract và simulation

- Định nghĩa slot, pulse, stack, threshold và Overdrive state.
- Viết test cho pulse và activation.
- Chưa cần Shop UI.

### Phase 2: Board và combat integration

- Tạo board 8 slot.
- Cho phép đặt hero/item.
- Kết nối Overdrive với hero hiện tại.
- Thêm VFX/SFX hoặc placeholder rõ ràng.

### Phase 3: Wave result và Shop

- Chặn giữa các wave tại Shop.
- Thêm gold trong run.
- Mua/bán/đổi vị trí hero và item.
- Thêm refresh giới hạn.

### Phase 4: Augment

- Checkpoint sau wave 3.
- Hiển thị 3 lựa chọn.
- Lưu một Augment đến hết run.
- Kết nối một số Augment với Energy Circuit.

### Phase 5: Content và tuning

- 2 hero active.
- Item Generator, Amplifier, Battery, Relay.
- 3 Overdrive identity.
- 6-10 Augment.
- Boss wave.
- Tuning pulse, stack, gold và shop offer.

## Checkpoints

### Checkpoint 1 - Circuit simulation

- Pulse chạy đúng thứ tự.
- Stack tăng đúng slot.
- Hero kích hoạt Overdrive khi đủ stack.
- Không có lỗi job/NativeContainer.

### Checkpoint 2 - Playable combat loop

- Có thể chạy hết một wave.
- Hero/item đặt trên board tạo khác biệt thực tế.
- Overdrive có feedback nhìn thấy được.

### Checkpoint 3 - Shop loop

- Wave kết thúc mở Shop.
- Người chơi mua/bán/thay đổi slot.
- Wave mới dùng đúng board state.

### Checkpoint 4 - MVP run

- Run 5 wave hoàn chỉnh.
- Augment sau wave 3 hoạt động.
- Wave 5 boss cần build chứ không chỉ chờ auto-combat.

## Risks và hướng giảm thiểu

| Risk | Tác động | Giảm thiểu |
|---|---|---|
| Pulse quá chậm | Không có cảm giác power | Tuning để hero có Overdrive trong 8-15 giây |
| Build tối ưu duy nhất | Shop mất ý nghĩa | Có build DPS, AOE, defense và boss |
| UI khó đọc | Người chơi không hiểu circuit | Pulse/stack/Overdrive phải hiển thị trực tiếp |
| Shop random quá tệ | Người chơi thua vì may rủi | 2 offer liên quan build, 1 offer mở hướng mới |
| Scope phình to | Không hoàn thành MVP | Chưa làm meta progression, PvP, crafting |
| Overdrive chỉ tăng số | Thiếu feeling | Mỗi hero có thay đổi projectile/behavior rõ ràng |

## Open questions

- Dạng hiển thị circuit: vòng tròn, line hay grid.
- Board có cho mọi slot nhận hero/item hay phân loại slot.
- Có bench cho hero chưa dùng hay không.
- Gold nhận theo kill, wave hay cả hai.
- Có cần pause thật sự trong Shop hay chỉ chuyển scene/panel.
