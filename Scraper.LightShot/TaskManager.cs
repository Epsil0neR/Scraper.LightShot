using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scraper.LightShot
{
    public class TaskManager
    {
        private readonly object _lock = new object();
        private readonly CancellationToken _token;
        private readonly Func<int, Task> _taskAction;
        private readonly int _count;

        private int _inProgress = 0;
        private int _counter = 0;

        public int ConcurrencyLimit { get; }

        public TaskManager(int concurrencyLimit, CancellationToken token, Func<int, Task> taskAction, int count)
        {
            if (concurrencyLimit < 1)
                throw new ArgumentException("Must be greater than 0.", nameof(concurrencyLimit));
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            _token = token;
            _taskAction = taskAction;
            _count = count;

            ConcurrencyLimit = concurrencyLimit;
        }

        public void Run()
        {
            RunNextInQueue();
        }

        private void RunNextInQueue()
        {
            lock (_lock)
            {
                while (_inProgress < ConcurrencyLimit && _counter < _count)
                {
                    var task = _taskAction(++_counter);
                    ++_inProgress;
                    task.ContinueWith(_ =>
                    {
                        lock (_lock)
                            --_inProgress;

                        RunNextInQueue();
                    }, _token);
                }
            }
        }
    }
}