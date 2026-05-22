using System;
using System.Collections.Generic;
using UnityEngine;

namespace _KITSystem.SkillSystem.Entity
{
    public static class EntityManager
    {
        private static int[] entityVersions = new int[256];
        private static readonly Stack<int> freeIds = new();
        private static int nextId = 1;
        
        public static event Action<int> OnEntityCreated;
        public static event Action<int> OnEntityRemoved;

        static EntityManager()
        {
            Array.Fill(entityVersions, -1);
        }

        public static int ActiveCount { get; private set; }

        public static int CreateEntity()
        {
            int id = freeIds.Count > 0 ? freeIds.Pop() : AllocateId();
            entityVersions[id] = 0;
            ActiveCount++;
            OnEntityCreated?.Invoke(id);
            Debug.Log("CreateEntity " + id);
            return id;
        }

        public static void DestroyEntity(int entity)
        {
            if (!IsAlive(entity))
                return;

            foreach (KeyValuePair<Type, IComponentPool> pool in ComponentRegistry.Pools)
            {
                pool.Value.Remove(entity);
            }

            entityVersions[entity] = -1;
            freeIds.Push(entity);
            ActiveCount--;
            Debug.Log("DestroyEntity " + entity);
            OnEntityRemoved?.Invoke(entity);
        }

        public static bool IsAlive(int entity)
        {
            return entity > 0 && entity < entityVersions.Length && entityVersions[entity] >= 0;
        }

        private static int AllocateId()
        {
            int id = nextId++;
            if (id >= entityVersions.Length)
                Array.Resize(ref entityVersions, id * 2);
            return id;
        }

        public static IEnumerable<int> Query<T>() where T : unmanaged
        {
            foreach (var id in ComponentManager<T>.EntityIds)
                yield return id;
        }

        public static IEnumerable<int> Query<T1, T2>()
            where T1 : unmanaged
            where T2 : unmanaged
        {
            foreach (var id in ComponentManager<T1>.EntityIds)
            {
                if (ComponentManager<T2>.Has(id))
                    yield return id;
            }
        }

        public static IEnumerable<int> Query<T1, T2, T3>()
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
        {
            foreach (var id in ComponentManager<T1>.EntityIds)
            {
                if (ComponentManager<T2>.Has(id) && ComponentManager<T3>.Has(id))
                    yield return id;
            }
        }

        public static void Clear()
        {
            foreach (KeyValuePair<Type, IComponentPool> pool in ComponentRegistry.Pools)
            {
                pool.Value.Clear();
            }

            Array.Fill(entityVersions, -1);
            freeIds.Clear();
            nextId = 1;
            ActiveCount = 0;
        }
    }

    internal static class ComponentPoolDelegates<T> where T : unmanaged
    {
        public static readonly IComponentPool Instance = new ComponentPoolImpl<T>();
    }

    internal interface IComponentPool
    {
        void Remove(int id);
        void Clear();
    }

    internal sealed class ComponentPoolImpl<T> : IComponentPool where T : unmanaged
    {
        public void Remove(int id) => ComponentManager<T>.Remove(id);
        public void Clear() => ComponentManager<T>.Clear();
    }

    internal static class ComponentRegistry
    {
        private static readonly Dictionary<Type, IComponentPool> pools = new();

        public static IReadOnlyDictionary<Type, IComponentPool> Pools => pools;

        public static void Register(Type type, IComponentPool pool)
        {
            if (!pools.ContainsKey(type))
                pools[type] = pool;
        }
    }
}