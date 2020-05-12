using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using CU = CommonUtils.Threading;

namespace CommonUtils.Test.Threading
{
    [TestClass]
    public class AutoResetEventAsync
    {
        [TestMethod]
        public void Set_AllowsWaitingThreadToContinue()
        {
            var resetEvent = new CU.AutoResetEventAsync();
            bool taskCompleted = false;

            Task.Run(async () =>
            {
                await resetEvent.WaitOneAsync();
                taskCompleted = true;
            });

            // Sleep to allow other task to run
            Thread.Sleep(100);

            Assert.IsFalse(taskCompleted);

            resetEvent.Set();

            // Sleep to allow other task to run
            Thread.Sleep(50);

            Assert.IsTrue(taskCompleted);
        }

        [TestMethod]
        public void Set_BeforeWaitingThreads_AllowsWaitingThreadsToContinueStraightAway()
        {
            var resetEvent = new CU.AutoResetEventAsync();
            resetEvent.Set();
            resetEvent.Set();
            object lockObject = new object();
            int runCount = 0;

            Task.Run(async () =>
            {
                await resetEvent.WaitOneAsync();
                lock (lockObject)
                {
                    runCount += 1;
                }
            });

            Task.Run(async () =>
            {
                await resetEvent.WaitOneAsync();
                lock (lockObject)
                {
                    runCount += 1;
                }
            });

            Task.Run(async () =>
            {
                await resetEvent.WaitOneAsync();
                lock (lockObject)
                {
                    runCount += 1;
                }
            });

            // Sleep to allow other tasks to run
            Thread.Sleep(100);

            // we've only called Set twice, so only two tasks should have run.
            Assert.AreEqual(2, runCount);
        }

        [TestMethod]
        public void Set_AllowsOneThreadToContinueOnly()
        {
            var resetEvent = new CU.AutoResetEventAsync();
            object lockObject = new object();
            int runCount = 0;

            Task.Run(async () =>
            {
                await resetEvent.WaitOneAsync();
                lock (lockObject)
                {
                    runCount += 1;
                }
            });

            Task.Run(async () =>
            {
                await resetEvent.WaitOneAsync();
                lock (lockObject)
                {
                    runCount += 1;
                }
            });

            // Sleep to allow other tasks to run
            Thread.Sleep(100);

            Assert.AreEqual(0, runCount);

            resetEvent.Set();

            // Sleep to allow other tasks to run
            Thread.Sleep(100);

            // We've only called set once, so only one task should have run.
            Assert.AreEqual(1, runCount);
        }
    }
}
