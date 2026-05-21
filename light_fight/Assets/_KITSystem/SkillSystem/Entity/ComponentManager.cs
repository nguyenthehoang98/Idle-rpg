using System;
using System.Collections.Generic;

namespace _KITSystem.SkillSystem.Entity
{
    public static class ComponentManager<T> where T : unmanaged
    {
        private static T[] components = new T[256];
        private static int[] entityToIndex = new int[256];
        private static int[] indexToEntity = new int[256];
        private static int count = 0;

        public static int Count => count;

        static ComponentManager()
        {
            Array.Fill(entityToIndex, -1);
        }

        public static void Add(int entityId, in T component)
        {
            if (entityId >= entityToIndex.Length)
                ResizeEntityMap(entityId * 2);

            if (count >= components.Length)
                Resize(components.Length * 2);

            int index = count++;
            components[index] = component;
            entityToIndex[entityId] = index;
            indexToEntity[index] = entityId;
        }

        public static ref T Get(int entityId)
        {
            int index = entityId < entityToIndex.Length ? entityToIndex[entityId] : -1;
            if (index < 0 || index >= count)
                throw new KeyNotFoundException($"Entity {entityId} has no component {typeof(T).Name}");
            return ref components[index];
        }

        public static bool TryGet(int entityId, out T component)
        {
            int index = entityId < entityToIndex.Length ? entityToIndex[entityId] : -1;
            if (index >= 0 && index < count)
            {
                component = components[index];
                return true;
            }

            component = default;
            return false;
        }

        public static bool Has(int entityId)
        {
            int index = entityId < entityToIndex.Length ? entityToIndex[entityId] : -1;
            return index >= 0 && index < count;
        }

        public static void Remove(int entityId)
        {
            int index = entityId < entityToIndex.Length ? entityToIndex[entityId] : -1;
            if (index < 0 || index >= count)
                return;

            count--;
            int lastEntityId = indexToEntity[count];

            components[index] = components[count];
            entityToIndex[lastEntityId] = index;
            indexToEntity[index] = lastEntityId;

            entityToIndex[entityId] = -1;
        }

        public static void Clear()
        {
            for (int i = 0; i < count; i++)
            {
                entityToIndex[indexToEntity[i]] = -1;
            }

            count = 0;
        }

        public static IEnumerable<int> EntityIds
        {
            get
            {
                for (int i = 0; i < count; i++)
                    yield return indexToEntity[i];
            }
        }

        private static void Resize(int newSize)
        {
            Array.Resize(ref components, newSize);
            Array.Resize(ref indexToEntity, newSize);
        }

        private static void ResizeEntityMap(int newSize)
        {
            var newMap = new int[newSize];
            Array.Fill(newMap, -1);
            Array.Copy(entityToIndex, newMap, entityToIndex.Length);
            entityToIndex = newMap;
        }
    }
}