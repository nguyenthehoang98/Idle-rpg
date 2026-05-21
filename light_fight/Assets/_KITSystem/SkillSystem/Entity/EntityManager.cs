using System;
using System.Collections.Generic;

namespace _KITSystem.SkillSystem.Entity
{
    public static class EntityManager
    {
        private static int[] entityVersions = new int[256];
        private static readonly Stack<int> freeIds = new();
        private static int nextId = 1;

        static EntityManager()
        {
            Array.Fill(entityVersions, -1);
        }

        public static int ActiveCount { get; private set; }

        #region Entity Lifecycle

        public static int NewEntity()
        {
            int id = freeIds.Count > 0 ? freeIds.Pop() : AllocateId();
            entityVersions[id] = 0;
            ActiveCount++;
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

        #endregion

        #region Component Operations

        public static void AddComponent<T>(int entity, in T component) where T : unmanaged
        {
            if (!IsAlive(entity))
            {
#if DEBUG
                throw new ArgumentException($"Entity {entity} is not alive.");       
#endif
                return;
            }

            if (ComponentManager<T>.Has(entity))
            {
#if DEBUG
                throw new InvalidOperationException($"Entity {entity} already has {typeof(T).Name}.");       
#endif
                return;
            }

            ComponentManager<T>.Add(entity, component);
            ComponentRegistry.Register(typeof(T), ComponentPoolDelegates<T>.Instance);
        }

        public static ref T GetComponent<T>(int entity) where T : unmanaged
        {
            return ref ComponentManager<T>.Get(entity);
        }

        public static bool TryGetComponent<T>(int entity, out T component) where T : unmanaged
        {
            return ComponentManager<T>.TryGet(entity, out component);
        }

        public static bool HasComponent<T>(int entity) where T : unmanaged
        {
            return ComponentManager<T>.Has(entity);
        }

        public static void RemoveComponent<T>(int entity) where T : unmanaged
        {
            ComponentManager<T>.Remove(entity);
        }

        #endregion

        #region Query

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

        #endregion

        #region Cleanup

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

        #endregion
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