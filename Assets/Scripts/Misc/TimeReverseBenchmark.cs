using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UsefulClasses;

public class TimeReverseBenchmark : MonoBehaviour
{
    [SerializeField] private int _snapshotCount = 10000;
    [SerializeField] private int _iterations = 100;

    private CircularBuffer<TransformSnapshot> _circularBuffer;
    private LinkedList<TransformSnapshot> _linkedlistBuffer;

    void Start()
    {
        int capacity = _snapshotCount;
        _circularBuffer = new CircularBuffer<TransformSnapshot>(capacity);
        _linkedlistBuffer = new LinkedList<TransformSnapshot>();

        var snapshots = new List<TransformSnapshot>(_snapshotCount);
        for (int i = 0; i < _snapshotCount; i++)
        {
            snapshots.Add(new TransformSnapshot(
            
                new Vector3(Random.Range(0f, 100f),
                                       Random.Range(0f, 100f),
                                       Random.Range(0f, 100f)),
                Quaternion.Euler(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f)),
                  0.016f
            ));
        }

        // Benchmark LinkedList
        long llElapsed = 0;
        for (int iter = 0; iter < _iterations; iter++)
        {
            _linkedlistBuffer.Clear();
            var sw = Stopwatch.StartNew();

            foreach (var s in snapshots)
            {
                _linkedlistBuffer.AddFirst(s);
                if (_linkedlistBuffer.Count > capacity)
                    _linkedlistBuffer.RemoveLast();
            }

            // Rewind-Mimik
            int index = 0;
            while (_linkedlistBuffer.Count > 0)
            {
                var first = _linkedlistBuffer.First.Value;
                _linkedlistBuffer.RemoveFirst();
                index++;
            }

            sw.Stop();
            llElapsed += sw.ElapsedTicks;
        }

        // Benchmark CircularBuffer
        long cbElapsed = 0;
        for (int iter = 0; iter < _iterations; iter++)
        {
            _circularBuffer.Clear();
            var sw = Stopwatch.StartNew();

            foreach (var s in snapshots)
            {
                _circularBuffer.Add(s);
            }

            // Rewind-Mimik
            int i = _circularBuffer.Count - 1;
            while (i >= 0)
            {
                var _ = _circularBuffer[i];
                i--;
            }

            sw.Stop();
            cbElapsed += sw.ElapsedTicks;
        }

        float llMs = llElapsed / (float)Stopwatch.Frequency * 1000f;
        float cbMs = cbElapsed / (float)Stopwatch.Frequency * 1000f;

        UnityEngine.Debug.Log($"LinkedList:      {llMs:F2} ms total");
        UnityEngine.Debug.Log($"CircularBuffer:  {cbMs:F2} ms total");
        UnityEngine.Debug.Log($"Ratio (LL/CB):   {llMs / cbMs:F2}");
    }
}