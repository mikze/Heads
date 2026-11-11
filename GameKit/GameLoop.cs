using System.Diagnostics;

namespace GameKit
{
    /// <summary>
    /// Pętla meczu we własnym wątku. Każda czynność ma swój stały odstęp,
    /// np. krok fizyki co 10 ms, a sterowanie 60 razy na sekundę.
    /// </summary>
    public sealed class GameLoop : IDisposable
    {
        // Po dłuższej przerwie (np. GC) nie nadrabiamy setek kroków naraz.
        private static readonly TimeSpan MaxLag = TimeSpan.FromMilliseconds(100);

        private readonly List<Timer> timers = new();
        private readonly CancellationTokenSource cts = new();
        private readonly string name;
        private Thread thread;

        private sealed class Timer(TimeSpan interval, Action action)
        {
            public readonly TimeSpan Interval = interval;
            public readonly Action Action = action;
            public TimeSpan Next;
        }

        public GameLoop(string name = "GameLoop") => this.name = name;

        /// <summary>Dodaje czynność. Czynności w jednym przebiegu wykonują się w kolejności dodania.</summary>
        public GameLoop Every(TimeSpan interval, Action action)
        {
            if (thread != null) throw new InvalidOperationException("Pętla już działa");
            timers.Add(new Timer(interval, action));
            return this;
        }

        public void Start()
        {
            thread = new Thread(Run) { IsBackground = true, Name = name };
            thread.Start();
        }

        private void Run()
        {
            var clock = Stopwatch.StartNew();
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    var now = clock.Elapsed;
                    foreach (var t in timers)
                    {
                        if (now - t.Next > MaxLag) t.Next = now;
                        if (now >= t.Next)
                        {
                            t.Action();
                            t.Next += t.Interval;
                        }
                    }
                    Thread.Sleep(1);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Błąd w pętli {name}: {e}");
                }
            }
        }

        public void Dispose()
        {
            cts.Cancel();
            if (thread != null && thread != Thread.CurrentThread)
                thread.Join(TimeSpan.FromSeconds(1));
            cts.Dispose();
        }
    }
}
