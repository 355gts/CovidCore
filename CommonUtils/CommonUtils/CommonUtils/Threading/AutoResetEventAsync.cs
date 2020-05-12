using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonUtils.Threading
{
    /// <summary>
    /// An Asynchronous version of the AutoResetEvent.
    /// Notifies a waiting thread that an event has occurred. This class cannot be inherited.
    /// Calling Set() when no threads are waiting will allow multiple threads to proceed without blocking.
    /// </summary>
    public sealed class AutoResetEventAsync
    {
        /// <summary>
        /// A lock object to control access to the waiters Queue and the noWaitersSignalledCount integer.
        /// </summary>
        private readonly object resetEventLock = new object();

        /// <summary>
        /// A queue of waiting threads that are waiting for a signal from this lock
        /// </summary>
        private readonly Queue<TaskCompletionSource<bool>> waiters = new Queue<TaskCompletionSource<bool>>();
        
        /// <summary>
        /// A count to keep track of how many times Set() has been called if no threads are waiting on the lock.
        /// </summary>
        private int noWaitersSignalledCount = 0;

        /// <summary>
        /// Asynchronously delays the current thread until the lock receives a signal
        /// </summary>
        /// <returns></returns>
        public Task WaitOneAsync()
        {
            lock (resetEventLock)
            {
                if (noWaitersSignalledCount > 0)
                {
                    noWaitersSignalledCount -= 1;
                    return Task.CompletedTask;
                }
                else
                {
                    var tcs = new TaskCompletionSource<bool>();
                    waiters.Enqueue(tcs);
                    return tcs.Task;
                }
            }
        }

        /// <summary>
        /// Lets one waiting thread proceed. 
        /// If no threads are waiting on this lock, calling Set will increase the number of threads 
        /// allowed to proceed before any thread blocks.
        /// </summary>
        public void Set()
        {
            TaskCompletionSource<bool> waiterToRelease = null;
            lock (resetEventLock)
            {
                if (waiters.Any())
                {
                    waiterToRelease = waiters.Dequeue();
                }
                else
                {
                    noWaitersSignalledCount += 1;
                }
            }

            if (waiterToRelease != null)
            {
                waiterToRelease.SetResult(true);
            }
        }
    }
}
