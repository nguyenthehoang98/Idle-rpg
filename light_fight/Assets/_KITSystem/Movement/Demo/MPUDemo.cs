using System.Collections;
using System.Collections.Generic;
using _KITSystem.Movement;
using _KITSystem.Schedule;
using UnityEngine;

public class MPUDemo : MonoBehaviour
{
    [SerializeField] private TickSystemOwner owner;
    [SerializeField] private GameObject unitPreafab;

    private MPU mpu;
    private List<GameObject> units = new List<GameObject>();
    private List<int> unitIds = new List<int>();
    private bool hasInitialized = false;
    
    void Start()
    {
        owner.TryGetTickable(out mpu);
        mpu.Initialize();
        StartCoroutine(AutoSpawn());
    }

    private IEnumerator AutoSpawn()
    {
        int i = 3;
        while ( i > 0)
        {
            Spawn();
            yield return new WaitForSeconds(3);
            i--;
        }
    }

    private void Spawn()
    {
        List<Vector3> positions = new List<Vector3>
        {
            new Vector3(-4f, 1f, 0),
            new Vector3(3f, -2f, 0),
            new Vector3(-2.5f, 2.5f, 0),
            new Vector3(1.5f, -3f, 0),
            new Vector3(4f, 0.5f, 0),
            new Vector3(-3.5f, -1.5f, 0),
            new Vector3(2f, 3f, 0),
            new Vector3(-1f, -4f, 0),
            new Vector3(0.5f, 2f, 0),
            new Vector3(-2f, -2f, 0),
        };

        List<Vector3> directions = new List<Vector3>
        {
            (-positions[0]).normalized,
            (-positions[1]).normalized,
            (-positions[2]).normalized,
            (-positions[3]).normalized,
            (-positions[4]).normalized,
            (-positions[5]).normalized,
            (-positions[6]).normalized,
            (-positions[7]).normalized,
            (-positions[8]).normalized,
            (-positions[9]).normalized,
        };
        
        int count = Mathf.Min(positions.Count, directions.Count);
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(unitPreafab, positions[i] * 5, Quaternion.identity);
            go.SetActive(true);
            units.Add(go);
        }
        
        for (int i = 0; i < count; i++)
        {
            int index = i;
            mpu.RequestAddUnit(positions[i] * 5, unitId =>
            {
                unitIds.Add(unitId);
                mpu.RequestAddModifier(unitId, new RunModifier(directions[index], 3));
            });
        }
    }

    private void Update()
    {
        int count = unitIds.Count;
        for (var id = 0; id < count; id++)
        {
            units[id].transform.position = mpu.GetUnitPosition(unitIds[id]);
        }
    }
}
