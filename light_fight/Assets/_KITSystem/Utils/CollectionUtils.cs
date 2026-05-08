using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace _KITSystem.Utils
{
    public static class CollectionUtils
    {
        #region List

        public static void Shuffle<T>(ref List<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void RemoveAtFast<T>(List<T> list, int index)
        {
            int lastIndex = list.Count - 1;

            if (index < 0 || index > lastIndex)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (index != lastIndex)
            {
                list[index] = list[lastIndex];
            }

            list.RemoveAt(lastIndex);
        }
        
        public static bool RemoveFast<T>(List<T> list, T value)
        {
            int index = list.IndexOf(value);
            if (index < 0) return false;

            RemoveAtFast(list, index);
            return true;
        }

        #endregion
        
        public static void Shuffle<TKey, TValue>(ref Dictionary<TKey, TValue> input)
        {
            Dictionary<TKey, TValue> dic = input.OrderBy(x => Guid.NewGuid()).ToDictionary(x => x.Key, x => x.Value);
            input = dic;
        }

        #region Array

        public static void InsertAt<T>(ref T[] array, int index, T newItem)
        {
            if (index < 0 || index > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }

            T[] newArray = new T[array.Length + 1];
            for (int i = 0; i < index; i++)
            {
                newArray[i] = array[i];
            }

            newArray[index] = newItem;
            for (int i = index; i < array.Length; i++)
            {
                newArray[i + 1] = array[i];
            }

            array = newArray;
        }

        public static bool Remove<T>(ref T[] array, T item)
        {
            int index = Array.IndexOf(array, item);
            if (index < 0)
                return false;

            RemoveAt(ref array, index);
            return true;
        }
        
        public static void RemoveAt<T>(ref T[] array, int index)
        {
            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            T[] newArray = new T[array.Length - 1];

            for (int i = 0, j = 0; i < array.Length; i++)
            {
                if (i == index) continue;
                newArray[j++] = array[i];
            }

            array = newArray;
        }
        
        public static void EnsureCapacity<T>(ref T[] arr, int required)
        {
            if (arr == null)
            {
                arr = new T[Mathf.NextPowerOfTwo(required)];
                return;
            }

            if (required <= arr.Length)
                return;

            int newSize = Mathf.NextPowerOfTwo(required);
            var newArr = new T[newSize];
            Array.Copy(arr, newArr, arr.Length);
            arr = newArr;
        }

        #endregion
    }
}