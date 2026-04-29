using System.Collections;
using _KITSystem.Movement;
using _KITSystem.Schedule;
using UnityEngine;

public class MPUDemo : MonoBehaviour
{
    [SerializeField] private TickSystemOwner owner;
    [SerializeField] private GameObject unit;

    private MPU mpu;
    private int unitId = -1;
    private bool hasInitialized = false;
    
    void Start()
    {
        owner.TryGetTickable(out mpu);
        mpu.Initialize();
        mpu.RequestAddUnit(unit.transform.position, i =>
        {
            unitId = i;
            mpu.RequestAddModifier(unitId, new RunModifier(new Vector3(1, 1, 0), 5, 0.5f));
        });
    }

    private void Update()
    {
        if (unitId != -1)
        {
            unit.transform.position = mpu.GetUnitPosition(unitId);
        }
    }
}
