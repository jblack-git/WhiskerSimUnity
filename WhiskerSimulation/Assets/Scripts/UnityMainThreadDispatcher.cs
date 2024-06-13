// Whisker Simulation
// 
// UnityMainThreadDispatcher enables multiple threads so ConversionMain can run and output can be updated simultaneously
// 6/14/2024
// Authors: Joseph Black
// For Use By UAH/MDA/NASA
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();
    private static UnityMainThreadDispatcher instance;

    public static void Enqueue(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        actions.Enqueue(action);
    }

    private void Update()
    {
        while (actions.TryDequeue(out var action))
        {
            action();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (instance == null)
        {
            var go = new GameObject("UnityMainThreadDispatcher");
            instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }
    }
}
