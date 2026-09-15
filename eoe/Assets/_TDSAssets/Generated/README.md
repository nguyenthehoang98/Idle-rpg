# Generated 3D - Per-model pipeline
Mỗi model tính tùy theo dáng, không hardcode.

Cấu trúc:
Generated/<TenModel>/
  ref.png              - ảnh T-pose input
  mesh.glb             - Hunyuan/TripoSR output
  mesh_decimated.fbx   - 3k-6k verts
  rigged.fbx           - Mixamo / Rigify / Meshy (tùy biped/quadruped/blob)
  QA.json              - verts, bones, T-pose check
  config.json          - {rig: "mixamo"|"rigify", bones: 7}

Pipeline pi tự chọn:
- Biped (orc, hero, goblin) -> Mixamo free
- Quadruped (sói, rồng) -> Meshy free / Rigify wolf
- Blob (slime) -> Rigify simple

Chạy: pi gen --image ref.png --out Generated/<Name>/
