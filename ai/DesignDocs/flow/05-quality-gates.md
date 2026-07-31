# 05 - Quality Gates

Dựa trên các skill:

```text
spec-driven-development
test-driven-development
code-review-and-quality
doubt-driven-development
documentation-and-adrs
```

## Gate 1 - Interview Gate

Chỉ pass khi trả lời được:

```text
- Game dành cho ai?
- Người chơi làm gì trong 10 giây đầu?
- Người chơi làm gì sau mỗi wave?
- Thua khi nào?
- Thắng khi nào?
- MVP đầu tiên có gì?
- MVP đầu tiên không có gì?
```

Checklist:

```text
[x] Player role rõ
[x] Core loop rõ
[x] MVP scope rõ
[x] Non-scope rõ
[x] Success criteria rõ
[x] Confidence >= 90%
```

## Gate 2 - Spec Gate

Chỉ pass khi `02-spec.md` có:

```text
[x] Objective
[x] Target player
[x] Core loop
[x] MVP scope
[x] Non-scope
[x] Gameplay rules
[x] Default data
[x] Success criteria
[x] Open questions resolved or accepted
```

Red flags:

```text
- Có chữ "vân vân", "tùy", "đại khái" trong yêu cầu chính.
- Không có điều kiện thắng/thua.
- Không biết test bằng cách nào.
```

## Gate 3 - Technical Design Gate

Chỉ pass khi `03-technical-design.md` có:

```text
[x] Scene structure
[x] Prefab/component list
[x] Runtime component responsibilities
[x] Dependency direction
[x] Data strategy
[x] Recovery reference boundary
[x] Risks and mitigation
```

Red flags:

```text
- Component làm quá nhiều việc.
- UI/Core phụ thuộc ngược nhau.
- Đưa Pool/Addressables/Excel vào trước khi gameplay chạy.
- Copy Recovery vì "nhanh hơn" nhưng chưa hiểu dependency.
```

## Gate 4 - Plan Gate

Chỉ pass khi tasks:

```text
[x] Chia theo vertical slices
[x] Mỗi task có file output rõ
[x] Mỗi task có acceptance criteria
[x] Mỗi task có test manual hoặc automated
[x] Task không vượt quá phạm vi nhỏ
```

## Gate 5 - Build Gate

Trước khi code:

```text
[x] Task được user duyệt
[x] Biết file nào sẽ thay đổi
[x] Biết cách test
[x] Không đụng Recovery nếu chưa hỏi
```

Sau khi code (Slice 1 passé, Slice 2 in progress):

```text
[x] Unity compile không lỗi đỏ
[x] Play Mode test pass (Slice 1: Spawn + RVO)
[ ] Slice 2: BaseCore + Health → GameOver
[ ] Docs cập nhật nếu design đổi
```

## Gate 6 - Review Gate

Review theo 5 trục:

```text
1. Correctness - có đúng gameplay/spec không?
2. Simplicity - có quá phức tạp so với MVP không?
3. Maintainability - script có trách nhiệm rõ không?
4. Unity Safety - null refs, Inspector refs, lifecycle đúng không?
5. Performance - có vấn đề rõ ràng với số lượng monster không?
```

## Gate 7 - ADR Gate

Tạo ADR khi quyết định:

```text
[x] ADR-0001: Kiến trúc folder (accepted 2026-07-31)
[x] ADR-0002: Excel → JSON vs ScriptableObject (accepted 2026-07-31)
[x] ADR-0003: Object pool strategy (accepted 2026-07-31)
[x] ADR-0004: Addressables (accepted 2026-07-31)
[x] ADR-0005: Target platform – Mobile (accepted 2026-07-31)
[-] ADR-0006: Input/control model (deferred – hero + input chưa trong scope)
[x] ADR-0007: Skill System architecture (accepted 2026-07-31)
```

Template ở:

```text
ai/DesignDocs/adrs/ADR-0001-project-structure.md
```
