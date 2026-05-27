using System.Collections.Generic;
using _KITSystem.Schedule;

[System.Serializable]
public class MonsterTickable : ITickable
{
    private List<Monster> monsters = new List<Monster>();
    private Queue<Monster> additionQueue = new Queue<Monster>();
    private Queue<Monster> removeQueue = new Queue<Monster>();
    
    public void Tick(float deltaTime)
    {
        while (additionQueue.Count > 0)
        {
            monsters.Add(additionQueue.Dequeue());
        }

        while (removeQueue.Count > 0)
        {
            var m = removeQueue.Dequeue();
            monsters.Remove(m);
            m.Dispose();
        }

        for (int i = 0; i < monsters.Count; i++)
        {
            monsters[i].Tick(deltaTime);
        }
    }

    public void Draw()
    {
        for (int i = 0; i < monsters.Count; i++)
        {
            monsters[i].Draw();
        }
    }
    
    public void AddMonster(Monster monster) => additionQueue.Enqueue(monster);
    
    public void RemoveMonster(Monster monster) => removeQueue.Enqueue(monster);
}