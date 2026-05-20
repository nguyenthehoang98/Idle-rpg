using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Config;
using _KITSystem.SkillSystem.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

public class SkillDemo : MonoBehaviour
{
    [SerializeField] private TickSystemOwner owner;
    [FormerlySerializedAs("skillConfig")] [SerializeField] private SkillFrameConfig skillFrameConfig;

    private SPU spu;

    private void Start()
    {
        owner.TryGetTickable(out spu);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SkillFactory.Build(spu, skillFrameConfig);
        }
    }
}
