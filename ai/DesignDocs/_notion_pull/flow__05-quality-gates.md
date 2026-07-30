# flow/05-quality-gates

<!-- Pulled from Notion. Review before overwriting source design docs. -->

> Synced from Unity project: ai/DesignDocs/flow/05-quality-gates.md

# 05 - Quality Gates

Dựa trên các skill:

```plain text
spec-driven-development
test-driven-development
code-review-and-quality
doubt-driven-development
documentation-and-adrs
```

## Gate 1 - Interview Gate

Chỉ pass khi trả lời được:

```plain text
- Game dành cho ai?
- Người chơi làm gì trong 10 giây đầu?
- Người chơi làm gì sau mỗi wave?
- Thua khi nào?
- Thắng khi nào?
- MVP đầu tiên có gì?
- MVP đầu tiên không có gì?
```

Checklist:

```plain text
[ ] Player role rõ
[ ] Core loop rõ
[ ] MVP scope rõ
[ ] Non-scope rõ
[ ] Success criteria rõ
[ ] Confidence >= 90%
```

## Gate 2 - Spec Gate

Chỉ pass khi `02-spec.md` có:

```plain text
[ ] Objective
[ ] Target player
[ ] Core loop
[ ] MVP scope
[ ] Non-scope
[ ] Gameplay rules
[ ] Default data
[ ] Success criteria
[ ] Open questions resolved or accepted
```

Red flags:

```plain text
- Có chữ "vân vân", "tùy", "đại khái" trong yêu cầu chính.
- Không có điều kiện thắng/thua.
- Không biết test bằng cách nào.
```

## Gate 3 - Technical Design Gate

Chỉ pass khi `03-technical-design.md` có:

```plain text
[ ] Scene structure
[ ] Prefab/component list
[ ] Runtime component responsibilities
[ ] Dependency direction
[ ] Data strategy
[ ] Recovery reference boundary
[ ] Risks and mitigation
```

Red flags:

```plain text
- Component làm quá nhiều việc.
- UI/Core phụ thuộc ngược nhau.
- Đưa Pool/Addressables/Excel vào trước khi gameplay chạy.
- Copy Recovery vì "nhanh hơn" nhưng chưa hiểu dependency.
```

## Gate 4 - Plan Gate

Chỉ pass khi tasks:

```plain text
[ ] Chia theo vertical slices
[ ] Mỗi task có file output rõ
[ ] Mỗi task có acceptance criteria
[ ] Mỗi task có test manual hoặc automated
[ ] Task không vượt quá phạm vi nhỏ
```

## Gate 5 - Build Gate

Trước khi code:

```plain text
[ ] Task được user duyệt
[ ] Biết file nào sẽ thay đổi
[ ] Biết cách test
[ ] Không đụng Recovery nếu chưa hỏi
```

Sau khi code:

```plain text
[ ] Unity compile không lỗi đỏ
[ ] Play Mode test pass
[ ] Docs cập nhật nếu design đổi
```

## Gate 6 - Review Gate

Review theo 5 trục:

```plain text
1. Correctness - có đúng gameplay/spec không?
2. Simplicity - có quá phức tạp so với MVP không?
3. Maintainability - script có trách nhiệm rõ không?
4. Unity Safety - null refs, Inspector refs, lifecycle đúng không?
5. Performance - có vấn đề rõ ràng với số lượng monster không?
```

## Gate 7 - ADR Gate

Tạo ADR khi quyết định:

```plain text
- Chọn kiến trúc folder
- Chọn ScriptableObject vs JSON/Excel
- Chọn object pool
- Chọn Addressables
- Chọn target platform
- Chọn input/control model
```

Template ở:

```plain text
ai/DesignDocs/adrs/ADR-0001-project-structure.md
```
