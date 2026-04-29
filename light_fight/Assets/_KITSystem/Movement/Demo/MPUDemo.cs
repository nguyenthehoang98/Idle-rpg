using System.Collections.Generic;
using _KITSystem.Movement;
using _KITSystem.Schedule;
using UnityEngine;

public class MPUDemo : MonoBehaviour
{
    [SerializeField] private TickSystemOwner owner;
    [SerializeField] private GameObject unitA;
    [SerializeField] private GameObject unitB;

    private MPU mpu;
    private List<GameObject> units = new List<GameObject>();
    private List<int> unitIds = new List<int>();
    private bool hasInitialized = false;
    
    void Start()
    {
        owner.TryGetTickable(out mpu);
        mpu.Initialize();

        List<Vector3> positions = new List<Vector3>
        {
            new Vector3(-2, 0, 0),
            new Vector3(2, 0, 0),
        };

        List<Vector3> directions = new List<Vector3>
        {
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
        };
        
        units.Add(unitA);
        units.Add(unitB);

        for (int i = 0; i < 2; i++)
        {
            int index = i;
            mpu.RequestAddUnit(positions[i], unitId =>
            {
                unitIds.Add(unitId);
                mpu.RequestAddModifier(unitId, new RunModifier(directions[index], 5));
            });
        }
    }

    private void Update()
    {
        for (var id = 0; id < unitIds.Count; id++)
        {
            units[id].transform.position = mpu.GetUnitPosition(unitIds[id]);
        }
    }
}
