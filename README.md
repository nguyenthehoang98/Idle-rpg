# Idle-rpg

---
date: 2026-05-13
status: completed
agent: build
---

# Free Agent Group (f-plan, f-build, f-review, f-knowledge)

## Summary

Create a parallel free-model agent group (`f-` prefix) using OpenCode Go's free-tier models. Each free model is mapped to the role it excels at — Ring 2.6 1T for deep reasoning, DeepSeek V4 Flash for speed/code, and Nemotron 3 Super for consistency. Same system prompts as existing agents, same permissions, same shared knowledge base.

## Model Assignment (Option A: Best Fit)

| Agent | Model | Score | Rationale |
|-------|-------|:--:|-----------|
| `f-plan` | `opencode-go/ring-2.6-1t-free` | 9/10 | 1T params = deepest free reasoning for complex multi-step plans |
| `f-build` | `opencode-go/deepseek-v4-flash-free` | 8/10 | Best free code generation + fast iteration |
| `f-review` | `opencode-go/deepseek-v4-flash-free` | 9/10 | "Flash" = speed-optimized, same lineage as paid review model |
| `f-knowledge` | `opencode-go/nemotron-3-super-free` | 8/10 | NVIDIA's consistency = low hallucination for knowledge mgmt |

## Context from Knowledge

### Current State
- 4 active opencode-go agents: `plan`, `build`, `review`, `knowledge`
- NVIDIA agent group (`n-*`) was removed 2026-05-13 due to API rate limiting
- Pattern established: copy system prompts, swap model in frontmatter
- Agent config documented in [[config-reference]] (lines 47-58) and [[facts]] (lines 208-231)

### Relevant Knowledge Files
- [[config-reference]] — Agent Configuration table (will add Free Agent section)
- [[facts]] — Agent Configuration triples (will add f-agent triples)
- [[AGENT]] — Workflow contract (no changes needed)
- [[2026-05-12--nvidia-agent-group]] — Historical precedent for agent groups

### Model Score Matrix (from analysis)

| Model | f-plan | f-build | f-review | f-knowledge | Avg |
|-------|:--:|:--:|:--:|:--:|:--:|
| Ring 2.6 1T | **9** | 7 | 5 | **9** | 7.5 |
| Nemotron 3 Super | 8 | 7 | 6 | **8** | 7.3 |
| DeepSeek V4 Flash Free | 6 | **8** | **9** | 6 | 7.3 |
| MiniMax M2.5 Free | 7 | 7 | 7 | 7 | 7.0 |
| Big Pickle Free | 6 | 6 | 6 | 6 | 6.0 |

## Implementation Steps

### Step 1: Create `f-plan.md`

**File:** `.opencode/agents/f-plan.md`

Copy from `.opencode/agents/plan.md`, change frontmatter only:

```yaml
---
name: f-plan
description: Free-model plan agent using Ring 2.6 1T (1T params) for deepest free-tier reasoning. Creates detailed implementation plans with full knowledge context.
mode: primary
model: opencode-go/ring-2.6-1t-free
permission:
  edit: ask
  write:
    "docs/plans/**": allow
    "docs/knowledge/**": allow
    "*": ask
  bash:
    "*": ask
    "git status": allow
    "git diff*": allow
    "git log*": allow
    "mkdir -p docs/plans": allow
    "mkdir -p docs/knowledge": allow
  webfetch: allow
color: "#8b5cf6"
---
```

System prompt body: **Identical** to `plan.md` lines 23-99 (the "You are the **Plan** agent..." through end).

**Changes from plan.md:**
- `name`: `plan` → `f-plan`
- `description`: updated to mention Ring 2.6 1T and "free-tier"
- `model`: `opencode-go/deepseek-v4-pro` → `opencode-go/ring-2.6-1t-free`
- `color`: `#3b82f6` → `#8b5cf6` (purple, distinct from blue plan)

### Step 2: Create `f-build.md`

**File:** `.opencode/agents/f-build.md`

Copy from `.opencode/agents/build.md`:

```yaml
---
name: f-build
description: Free-model build agent using DeepSeek V4 Flash Free for cost-effective code generation and implementation. Reads knowledge before coding, updates knowledge after changes.
mode: primary
model: opencode-go/deepseek-v4-flash-free
permission:
  edit: allow
  write:
    "docs/knowledge/**": allow
    "docs/plans/**": allow
    "*": allow
  bash:
    "*": allow
  webfetch: allow
color: "#10b981"
---
```

System prompt body: **Identical** to `build.md` lines 18-121.

**Changes from build.md:**
- `name`: `build` → `f-build`
- `description`: updated to mention DeepSeek V4 Flash Free
- `model`: `opencode-go/kimi-k2.5` → `opencode-go/deepseek-v4-flash-free`
- `color`: unchanged (`#10b981` green — shared with build, visually groups "build" roles)

### Step 3: Create `f-review.md`

**File:** `.opencode/agents/f-review.md`

Copy from `.opencode/agents/review.md`:

```yaml
---
name: f-review
description: Free-model review agent using DeepSeek V4 Flash Free for fast, cost-effective code review. Reads all knowledge files before analyzing. Read-only.
mode: primary
model: opencode-go/deepseek-v4-flash-free
permission:
  edit: deny
  write:
    "*": deny
  bash:
    "*": ask
    "git status": allow
    "git diff*": allow
    "git log*": allow
  webfetch: allow
color: "#f59e0b"
---
```

System prompt body: **Identical** to `review.md` lines 18-93.

**Changes from review.md:**
- `name`: `review` → `f-review`
- `description`: updated to mention DeepSeek V4 Flash Free
- `model`: `opencode-go/deepseek-v4-flash` → `opencode-go/deepseek-v4-flash-free`
- `color`: unchanged (`#f59e0b` amber — shared with review, visually groups "review" roles)

### Step 4: Create `f-knowledge.md`

**File:** `.opencode/agents/f-knowledge.md`

Copy from `.opencode/agents/knowledge.md`:

```yaml
---
name: f-knowledge
description: Free-model knowledge agent using Nemotron 3 Super Free for consistent, low-hallucination knowledge management. Reads AGENT.md dynamically for workflow rules.
mode: subagent
model: opencode-go/nemotron-3-super-free
permission:
  edit: allow
  write:
    "docs/knowledge/**": allow
    "*": ask
  bash:
    "*": ask
    "git status": allow
    "git diff*": allow
    "git log*": allow
---
```

System prompt body: **Identical** to `knowledge.md` lines 18-127.

**Changes from knowledge.md:**
- `name`: `knowledge` → `f-knowledge`
- `description`: updated to mention Nemotron 3 Super Free
- `model`: `opencode-go/kimi-k2.6` → `opencode-go/nemotron-3-super-free`
- No color (subagents don't display in UI)

### Step 5: Update Knowledge — `KG/facts.md`

**File:** `docs/knowledge/KG/facts.md`

Add new section after line 231 (before `## Workflow`):

```markdown
## Free Agent Group

free_agents → prefix → f-
free_agents → provider → opencode-go
free_agents → tier → free

f_plan → name → f-plan
f_plan → model → ring-2.6-1t-free
f_plan → provider → opencode-go
f_plan → purpose → Deepest free-tier reasoning for implementation planning (1T params)

f_build → name → f-build
f_build → model → deepseek-v4-flash-free
f_build → provider → opencode-go
f_build → purpose → Cost-free code generation and implementation

f_review → name → f-review
f_review → model → deepseek-v4-flash-free
f_review → provider → opencode-go
f_review → purpose → Fast, cost-free code review

f_knowledge → name → f-knowledge
f_knowledge → model → nemotron-3-super-free
f_knowledge → provider → opencode-go
f_knowledge → purpose → Consistent, low-hallucination knowledge management

f_workflow → steps → f-plan → f-review → f-knowledge → f-plan → f-review → finalize → human: ask to implement → f-build
f_workflow → shared_resources → docs/plans/, docs/knowledge/
```

### Step 6: Update Knowledge — `Palace/config-reference.md`

**File:** `docs/knowledge/Palace/config-reference.md`

Add after line 58 (after the existing Agent Configuration table + note):

```markdown

## Free Agent Configuration

| Agent | Name | File | Model | Provider | Mode |
|---|---|---|---|---|---|
| f-Plan | `f-plan` | `.opencode/agents/f-plan.md` | `ring-2.6-1t-free` | `opencode-go` | primary |
| f-Review | `f-review` | `.opencode/agents/f-review.md` | `deepseek-v4-flash-free` | `opencode-go` | primary |
| f-Build | `f-build` | `.opencode/agents/f-build.md` | `deepseek-v4-flash-free` | `opencode-go` | primary |
| f-Knowledge | `f-knowledge` | `.opencode/agents/f-knowledge.md` | `nemotron-3-super-free` | `opencode-go` | subagent |

f-Plan uses Ring 2.6 1T for deepest free-tier reasoning (1T parameters). f-Build and f-Review share DeepSeek V4 Flash Free for speed and code quality. f-Knowledge uses Nemotron 3 Super Free for consistent knowledge management.
```

### Step 7: Verify `opencode.json` — No Changes Needed

Confirm `opencode.json` already has `"default_agent": "plan"` (not `f-plan`). The opencode-go agents remain the default. Users opt in to free agents explicitly.

## Knowledge Impact

| File | Action | Details |
|------|--------|---------|
| `.opencode/agents/f-plan.md` | **Create** | Copy of plan.md with Ring 2.6 1T Free model |
| `.opencode/agents/f-build.md` | **Create** | Copy of build.md with DeepSeek V4 Flash Free model |
| `.opencode/agents/f-review.md` | **Create** | Copy of review.md with DeepSeek V4 Flash Free model |
| `.opencode/agents/f-knowledge.md` | **Create** | Copy of knowledge.md with Nemotron 3 Super Free model |
| `docs/knowledge/KG/facts.md` | **Add** section `## Free Agent Group` (~20 triples) | After line 231 |
| `docs/knowledge/Palace/config-reference.md` | **Add** section `## Free Agent Configuration` table | After line 58 |

### KG Triples Added (17 triples)

```
free_agents → prefix → f-
free_agents → provider → opencode-go
free_agents → tier → free
f_plan → name → f-plan
f_plan → model → ring-2.6-1t-free
f_plan → provider → opencode-go
f_plan → purpose → Deepest free-tier reasoning for implementation planning (1T params)
f_build → name → f-build
f_build → model → deepseek-v4-flash-free
f_build → provider → opencode-go
f_build → purpose → Cost-free code generation and implementation
f_review → name → f-review
f_review → model → deepseek-v4-flash-free
f_review → provider → opencode-go
f_review → purpose → Fast, cost-free code review
f_knowledge → name → f-knowledge
f_knowledge → model → nemotron-3-super-free
f_knowledge → provider → opencode-go
f_knowledge → purpose → Consistent, low-hallucination knowledge management
f_workflow → steps → f-plan → f-review → f-knowledge → f-plan → f-review → finalize → human: ask to implement → f-build
f_workflow → shared_resources → docs/plans/, docs/knowledge/
```

### No Deletions or Modifications

- Existing `plan.md`, `build.md`, `review.md`, `knowledge.md` — **unchanged**
- Existing KB entries for paid agents — **unchanged**
- `opencode.json` — **unchanged** (`default_agent` stays `plan`)

## Workflow

```
f-plan (Ring 2.6 1T) → f-review (DeepSeek Flash Free) → f-knowledge (Nemotron 3 Super) → f-plan → f-review → finalize → human: ask to implement → f-build (DeepSeek Flash Free)
```

1. **f-plan**: Create implementation plan with full knowledge context
2. **f-review**: Review plan for issues (knowledge alignment, architecture, edge cases)
3. **f-knowledge**: Update knowledge base with plan context
4. **f-plan**: Refine plan based on review feedback
5. **f-review**: Final review of refined plan
6. **finalize**: Human reviews and approves the plan
7. **human: ask to implement**: Human explicitly commands implementation to begin
8. **f-build**: Execute the approved plan

Both opencode-go and free agent groups share:
- `docs/plans/` — Implementation plans
- `docs/knowledge/` — Knowledge base (Palace, KG, conflicts)
- Same workflow contract ([[AGENT]])
- Same drawer mapping

## Risk Assessment

### Potential Issues

1. **Model ID Verification**: The exact model IDs (`opencode-go/ring-2.6-1t-free`, `opencode-go/deepseek-v4-flash-free`, `opencode-go/nemotron-3-super-free`) need verification. These are inferred from the naming pattern. Run `/models` in opencode or test with a simple query before relying on all agents.
2. **Ring 2.6 1T Latency**: 1T parameter models are slow. f-plan responses may take 30-60+ seconds. Acceptable for planning (quality > speed), but users should know.
3. **Free Tier Rate Limits**: Free models may have stricter rate limits than paid ones. If f-build and f-review share the same model (DeepSeek Flash Free), concurrent usage could hit limits.
4. **Model Availability**: Free models may be deprioritized or removed without notice. Keep paid agents as fallback.
5. **Quality Gap**: Free models will underperform paid equivalents. Set expectations: f-plan < plan, f-build < build, etc.

### Mitigation

1. Test each model ID with a simple "Hello, what model are you?" query before creating all 4 agents
2. Start with f-plan only, verify Ring 2.6 1T works, then create remaining agents
3. f-build and f-review share DeepSeek Flash Free — if rate limits hit, swap f-build to MiniMax M2.5 Free (Option C fallback)
4. Paid agents (`plan`, `build`, `review`, `knowledge`) remain unchanged as reliable fallbacks

## Success Criteria

1. All 4 f-* agents appear in opencode agent list
2. f-plan creates plans using Ring 2.6 1T Free
3. f-build implements code using DeepSeek V4 Flash Free
4. f-review reviews code using DeepSeek V4 Flash Free
5. f-knowledge manages knowledge base using Nemotron 3 Super Free
6. Both agent groups share same `docs/plans/` and `docs/knowledge/`
7. Paid agents (`plan`, `build`, `review`, `knowledge`) continue to work unchanged

## Fallback

If any free model is unavailable or rate-limited:

| Failed Model | Fallback |
|-------------|----------|
| Ring 2.6 1T Free | Nemotron 3 Super Free (downgrade 9→8 reasoning) |
| DeepSeek V4 Flash Free | MiniMax M2.5 Free (downgrade 9→7 speed, 8→7 coding) |
| Nemotron 3 Super Free | MiniMax M2.5 Free (downgrade 8→7 consistency) |

Worst case: all f-* agents use MiniMax M2.5 Free (Option D single-model fallback).
