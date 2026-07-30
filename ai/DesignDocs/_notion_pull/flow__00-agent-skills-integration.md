# flow/00-agent-skills-integration

<!-- Pulled from Notion. Review before overwriting source design docs. -->

> Synced from Unity project: ai/DesignDocs/flow/00-agent-skills-integration.md

# 00 - Agent Skills Integration

Nguồn tham khảo: https://github.com/addyosmani/agent-skills

Mục tiêu: tích hợp cách làm việc kiểu `agent-skills` vào dự án Unity 2D `TDSurvivor`, đặc biệt cho giai đoạn thiết kế trước khi code.

## 1. Nguyên tắc chính

```plain text
Spec before code.
One question at a time.
Small atomic tasks.
Tests are proof.
Review before merge.
```

Với dự án game, đổi thành:

```plain text
Gameplay intent before prefab/code.
Player experience before system design.
Acceptance criteria before implementation.
Prototype slice before full architecture.
Unity Play Mode proof before expanding scope.
```

## 2. Flow áp dụng cho TDSurvivor

```plain text
DEFINE
  1. Interview
  2. Assumptions
  3. Gameplay goals

SPEC
  4. Game spec
  5. Feature spec
  6. Acceptance criteria

DESIGN
  7. Technical design
  8. Data model
  9. Scene/prefab design
  10. ADR decisions

PLAN
  11. Milestones
  12. Vertical slices
  13. Task backlog

BUILD
  14. Implement one slice
  15. Manual Unity setup or editor automation

VERIFY
  16. Play Mode checklist
  17. Console errors
  18. Gameplay acceptance tests

REVIEW
  19. Code/design review
  20. Update docs
```

## 3. Skill mapping

| Agent Skill | Áp dụng trong dự án |

|---|---|

| `using-agent-skills` | Chọn đúng workflow: design, build, test, review |

| `interview-me` | Hỏi từng câu để rõ gameplay, người chơi, scope |

| `idea-refine` | Biến ý tưởng tower defense/survivor thành nhiều option |

| `spec-driven-development` | Viết spec trước khi tạo code/prefab |

| `planning-and-task-breakdown` | Chia MVP thành vertical slices nhỏ |

| `doubt-driven-development` | Stress-test thiết kế: quá rộng? khó test? lệch mục tiêu? |

| `documentation-and-adrs` | Ghi lại quyết định kiến trúc |

| `source-driven-development` | Recovery là nguồn tham khảo, không copy mù |

| `test-driven-development` | Với Unity: test bằng checklist + Play Mode trước, unit test sau |

| `code-review-and-quality` | Review script, coupling, naming, folder, Unity references |

## 4. Slash command giả lập trong dự án

Không cần tool slash command thật. Khi người dùng nói các từ sau, agent sẽ theo flow tương ứng:

```plain text
/design      -> dùng Interview + Spec + Technical Design
/spec        -> tạo/cập nhật spec
/plan        -> tạo milestone + tasks
/build       -> chỉ build task đã được duyệt
/test        -> tạo/chạy checklist test
/review      -> review docs/code
/adr         -> ghi Architecture Decision Record
```

## 5. Quality Gates bắt buộc

Không chuyển phase nếu chưa đạt gate.

```plain text
Gate A - Interview đủ rõ:
- Biết người chơi mục tiêu
- Biết core loop
- Biết MVP gồm gì và không gồm gì
- Biết tiêu chí thành công

Gate B - Spec đủ rõ:
- Có objective
- Có scope/non-scope
- Có gameplay rules
- Có acceptance criteria

Gate C - Technical design đủ rõ:
- Có scene structure
- Có prefab/component list
- Có data model
- Có dependency boundaries

Gate D - Plan đủ nhỏ:
- Mỗi task có output rõ
- Mỗi task test được
- Không task nào quá lớn/mơ hồ

Gate E - Build được phép:
- Có task được duyệt
- Có rollback hoặc phạm vi file rõ
- Không copy Recovery khi chưa hỏi
```

## 6. Artifacts cần duy trì

```plain text
ai/DesignDocs/flow/01-interview.md
ai/DesignDocs/flow/02-spec.md
ai/DesignDocs/flow/03-technical-design.md
ai/DesignDocs/flow/04-plan.md
ai/DesignDocs/flow/05-quality-gates.md
ai/DesignDocs/adrs/ADR-0001-project-structure.md
ai/DesignDocs/tasks/000-design-backlog.md
```
