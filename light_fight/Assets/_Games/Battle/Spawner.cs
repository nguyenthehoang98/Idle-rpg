using System.Collections;
using System.Collections.Generic;
using _KIT.Utils;
using _KITSystem.Grid;
using _KITSystem.Movement;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Config;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

// chuyển thành tickable
[DefaultExecutionOrder(10)]
public class Spawner : MonoBehaviour
{
    [SerializeField] private TickSystemOwner owner;
    private IGridManager gridManager;
    private List<OwnGameObject> list = new List<OwnGameObject>();
    private Dictionary<int, int> container = new Dictionary<int, int>();

    private MPU mpu;

    private void OnDrawGizmos()
    {
        if (mpu == null) return;

        Handles.color = Color.green;
        int count = list.Count;
        for (int i = 0; i < count; i++)
        {
            OwnGameObject obj = list[i];
            int hash = obj.GetHashCode();
            if(container.TryGetValue(hash, out int value))
            {
                Vector3 prev = obj.Position;
                Vector3 position = mpu.GetUnitPosition(container[hash]);
                gridManager.Insert(hash, position, out _);
                Handles.DrawWireDisc(Vector3.Lerp(prev, position, Time.deltaTime), Vector3.forward, 0.5f, 0.2f);
            }
        }
    }

    private void Start()
    {
        owner.TryGetTickable(out mpu);
        mpu.Initialize();
        
        gridManager = new FixedUniformGrid(1);
        StartCoroutine(SppawnIE());
        StartCoroutine(RemoveIE());
    }

    private void Update()
    {
        int count = list.Count;
        for (int i = 0; i < count; i++)
        {
            OwnGameObject obj = list[i];
            int hash = obj.GetHashCode();
            if (container.TryGetValue(hash, out int value))
            {
                Vector3 position = mpu.GetUnitPosition(container[hash]);
                obj.Position = position;
            }
        }
    }

    IEnumerator RemoveIE()
    {
        while (true)
        {
            Remove();
            yield return new WaitForSeconds(Random.Range(0.7f, 1.0f));
        }
    }

    void Remove()
    {
        if (list.Count <= 0) return;
        
        OwnGameObject first = list[0];
        int hash = first.GetHashCode();
        first.Dispose();
        list.RemoveAt(0);
        
        mpu.RequestRemoveUnit(container[hash]);
        gridManager.Remove(hash);
        container.Remove(hash);
    }

    IEnumerator SppawnIE()
    {
        while (true)
        {
            Spawn();
            yield return new WaitForSeconds(Random.Range(0.3f, 0.5f));
        }
    }

    void Spawn()
    {
        Vector3 position = new Vector3(Random.value - 0.5f, Random.value - 0.5f).normalized * Random.Range(10, 14);
        OwnGameObject go = new OwnGameObject(null);
        go.Position = position;

        Vector3 destination = Vector3.zero;
        int hash = go.GetHashCode();
        mpu.RequestAddUnit(position, destination, i =>
        {
            container[hash] = i;
            gridManager.Insert(hash, position, out _);
            list.Add(go);
            mpu.RequestAddAction(i, new RunMovementAction(position, 1, destination, 2));
        });
    }
}
