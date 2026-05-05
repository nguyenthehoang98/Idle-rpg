using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Config;
using _KITSystem.SkillSystem.Runtime;
using UnityEngine;

public class SkillDemo : MonoBehaviour
{
    [SerializeField] private TickSystemOwner owner;
    [SerializeField] private SkillConfig skillConfig;

    private SPU spu;

    private void Start()
    {
        owner.TryGetTickable(out spu);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SkillFactory.Build(spu, skillConfig);
        }
    }
}
