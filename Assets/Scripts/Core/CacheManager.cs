using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CacheManager
{
    #region Singleton
    private static class Instance
    {
        static Instance() { }

        internal static readonly CacheManager value = new CacheManager();
    }

    public static CacheManager instance { get { return Instance.value; } }

    private CacheManager() { }
    #endregion Singleton

    private class Pair
    {
        public float time;
        public object obj;

        public Pair(float time, object obj)
        {
            this.time = time;
            this.obj = obj;
        }
    }

    private Dictionary<object, Pair> cached = new Dictionary<object, Pair>();

    private int count;

    private int missCount;
    private int catchCount;
    
    public static T Get<T>(string resourcePath) where T : class
    {
        Pair pair = null;
        instance.cached.TryGetValue(resourcePath, out pair);
        if (pair == null)
        {
            instance.missCount++;

            instance.cached.Add(resourcePath, new Pair(0.0F, Resources.Load(resourcePath)));
            return Get<T>(resourcePath);
        }
        else
        {
            instance.catchCount++;

            pair.time = Time.time;
            return pair.obj as T;
        }
    }

    public static T Get<T>(Component type) where T : Component
    {
        var gameObject = type.gameObject;

        return Get<T>(gameObject);
    }

    public static T Get<T>(GameObject gameObject) where T : Component
    {
        Pair pair = null;
        instance.cached.TryGetValue(gameObject, out pair);
        if (pair == null)
        {
            instance.missCount++;

            instance.cached.Add(gameObject, new Pair(0.0F, new Dictionary<Type, object>()));
            return Get<T>(gameObject);
        }
        else
        {
            instance.catchCount++;

            Dictionary<Type, object> components = pair.obj as Dictionary<Type, object>;
            object value = null;
            components.TryGetValue(typeof(T), out value);
            if (value == null)
            {
                components.Add(typeof(T), gameObject.GetComponent<T>());
                return Get<T>(gameObject);
            }
            else
            {
                pair.time = Time.time;
                return value as T;
            }
        }
    }

    public static T[] Gets<T>(GameObject gameObject) where T : Component
    {
        Pair pair = null;
        instance.cached.TryGetValue(gameObject, out pair);
        if (pair == null)
        {
            instance.missCount++;

            instance.cached.Add(gameObject, new Pair(0.0F, new Dictionary<Type, object>()));
            return Gets<T>(gameObject);
        }
        else
        {
            instance.catchCount++;

            Dictionary<Type, object> components = pair.obj as Dictionary<Type, object>;
            object value = null;
            components.TryGetValue(typeof(T[]), out value);
            if (value == null)
            {
                components.Add(typeof(T[]), gameObject.GetComponents<T>());
                return Gets<T>(gameObject);
            }
            else
            {
                pair.time = Time.time;
                return value as T[];
            }
        }
    }

    public static void UnLoadCacheData(float time)
    {
        int beforeClear = instance.cached.Count;

        List<object> Keys = new List<object>();

        foreach (KeyValuePair<object, Pair> pair in instance.cached)
        {
            if (Time.time - pair.Value.time >= time)
            {
                Keys.Add(pair.Key);
            }
        }

        foreach (object obj in Keys)
        {
            instance.cached.Remove(obj);
        }

        int afterClear = instance.cached.Count;

        GC.Collect();

        Debug.LogFormat("Cache Cleared : {0} -> {1} ,Total {2} is Removed.", beforeClear, afterClear, beforeClear - afterClear);
        Debug.LogFormat("Cache accuracy: {0} / {1} ", instance.missCount, instance.catchCount);

        instance.missCount = 0;
        instance.catchCount = 0;
    }
}