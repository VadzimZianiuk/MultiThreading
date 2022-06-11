/*
 * 4.	Write a program which recursively creates 10 threads.
 * Each thread should be with the same body and receive a state with integer number, decrement it,
 * print and pass as a state into the newly created thread.
 * Use Thread class for this task and Join for waiting threads.
 * 
 * Implement all of the following options:
 * - a) Use Thread class for this task and Join for waiting threads.
 * - b) ThreadPool class for this task and Semaphore for waiting threads.
 */

using System;
using System.Threading;

namespace MultiThreading.Task4.Threads.Join
{
    class Program
    {
        private const int ThreadCount = 10;
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(0,1);

        static void Main(string[] args)
        {
            Console.WriteLine("4.	Write a program which recursively creates 10 threads.");
            Console.WriteLine("Each thread should be with the same body and receive a state with integer number, decrement it, print and pass as a state into the newly created thread.");
            Console.WriteLine("Implement all of the following options:");
            Console.WriteLine();
            Console.WriteLine("- a) Use Thread class for this task and Join for waiting threads.");
            Console.WriteLine("- b) ThreadPool class for this task and Semaphore for waiting threads.");

            Console.WriteLine();

            InitializeMenu();
        }

        private static void InitializeMenu()
        {
            Console.WriteLine("Main thread #{0}", Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine("Type 'a' or 'b' or 'x' to exit:");
            while (true)
            {
                char key = Console.ReadKey().KeyChar;
                Console.WriteLine();
                switch (key)
                {
                    case 'a':
                        CreateThreads(ThreadCount);
                        break;
                    case 'b':
                        CreateThreadsThreadPool(ThreadCount);
                        break;
                    case 'x':
                        return;
                    default:
                        Console.WriteLine("incorrect input.");
                        break;
                };
            }
        }

        private static void CreateThreads(object sourceState)
        {
            int state = ChangeStateIfBackground(sourceState);
            if (state > 0)
            {
                var tread = new Thread(new ParameterizedThreadStart(CreateThreads)){ IsBackground = true };
                tread.Start(state);
                tread.Join();
            }
        }

        private static void CreateThreadsThreadPool(object sourceState)
        {
            int state = ChangeStateIfBackground(sourceState);
            if (state > 0)
            {
                ThreadPool.QueueUserWorkItem(CreateThreadsThreadPool, state);
                semaphore.Wait();
            }
            if (Thread.CurrentThread.IsBackground)
            {
                semaphore.Release();
            }
        }

        private static int ChangeStateIfBackground(object sourceState)
        {
            int state = (int)sourceState;
            if (Thread.CurrentThread.IsBackground)
            {
                Console.WriteLine("Thread #{0} state {1}", Thread.CurrentThread.ManagedThreadId, --state);
            }
            return state;
        }
    }
}
