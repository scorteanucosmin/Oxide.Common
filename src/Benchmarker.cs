using System;
using System.Diagnostics;
using Oxide.Pooling;

namespace Oxide;

public class Benchmarker : IDisposable
{
    private string _input;
    private Stopwatch _stopwatch;

    public static Benchmarker Start(string input)
    {
        Console.WriteLine($"Started benchmarking {input}");

        Benchmarker benchmarker = PoolFactory<Benchmarker>.Shared.Take();
        benchmarker._input = input;
        benchmarker._stopwatch = PoolFactory<Stopwatch>.Shared.Take();
        benchmarker._stopwatch.Start();
        return benchmarker;
    }

    public void Dispose()
    {
        _stopwatch.Stop();

        Console.WriteLine($"{_input} took {_stopwatch.Elapsed} [{_stopwatch.ElapsedTicks} ticks]");

        _stopwatch.Reset();
        _input = null;

        PoolFactory<Stopwatch>.Shared.Return(_stopwatch);

        _stopwatch = null;
        PoolFactory<Benchmarker>.Shared.Return(this);
    }
}
