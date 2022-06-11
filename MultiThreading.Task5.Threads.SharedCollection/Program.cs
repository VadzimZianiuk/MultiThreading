/*
 * 5. Write a program which creates two threads and a shared collection:
 * the first one should add 10 elements into the collection and the second should print all elements
 * in the collection after each adding.
 * Use Thread, ThreadPool or Task classes for thread creation and any kind of synchronization constructions.
 */
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreading.Task5.Threads.SharedCollection
{
    class Program
    {
        private static readonly AutoResetEvent addEventWaitHandle = new AutoResetEvent(true);
        private static readonly AutoResetEvent printEventWaitHandle = new AutoResetEvent(false);
        private const int CollectionSize = 10;
        private static readonly List<int> collection = new List<int>(CollectionSize);

        static void Main(string[] args)
        {
            Console.WriteLine("5. Write a program which creates two threads and a shared collection:");
            Console.WriteLine("the first one should add 10 elements into the collection and the second should print all elements in the collection after each adding.");
            Console.WriteLine("Use Thread, ThreadPool or Task classes for thread creation and any kind of synchronization constructions.");
            Console.WriteLine();

            Task.Run(() =>
            {
                for (int i = 0; i < CollectionSize; i++)
                {
                    addEventWaitHandle.WaitOne();
                    collection.Add(i);
                    printEventWaitHandle.Set();
                }
            });
            Task.Run(() =>
            {
                for (int i = 0; i < CollectionSize; i++)
                {
                    printEventWaitHandle.WaitOne();
                    Console.WriteLine(string.Join(',', collection));
                    addEventWaitHandle.Set();
                }
            });

            Console.ReadLine();
        }
    }
}
