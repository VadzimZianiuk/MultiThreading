/*
*  Create a Task and attach continuations to it according to the following criteria:
   a.    Continuation task should be executed regardless of the result of the parent task.
   b.    Continuation task should be executed when the parent task finished without success.
   c.    Continuation task should be executed when the parent task would be finished with fail and parent task thread should be reused for continuation
   d.    Continuation task should be executed outside of the thread pool when the parent task would be cancelled
   Demonstrate the work of the each case with console utility.
*/
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreading.Task6.Continuation
{
    class Program
    {
        private const string TextBeforeSuccessfulCompletion = "will be successfully completed...";
        private const string TextBeforeCancellation = "will be canceled...";
        private const string TextBeforeException = "will throw an exception...";

        static void Main(string[] args)
        {
            Console.WriteLine("Create a Task and attach continuations to it according to the following criteria:");
            Console.WriteLine("a.    Continuation task should be executed regardless of the result of the parent task.");
            Console.WriteLine("b.    Continuation task should be executed when the parent task finished without success.");
            Console.WriteLine("c.    Continuation task should be executed when the parent task would be finished with fail and parent task thread should be reused for continuation.");
            Console.WriteLine("d.    Continuation task should be executed outside of the thread pool when the parent task would be cancelled.");
            Console.WriteLine("Demonstrate the work of the each case with console utility.");
            Console.WriteLine();

            InitializeMenu();
        }

        private static void InitializeMenu()
        {
            while (true)
            {
                Console.Write("Type 'a', 'b', 'c', 'd' or 'x' to exit: ");
                char key = Console.ReadKey().KeyChar;
                Console.WriteLine();
                switch (key)
                {
                    case 'a':
                        Demo(TaskContinuationOptions.None);
                        break;
                    case 'b':
                        Demo(TaskContinuationOptions.NotOnRanToCompletion);
                        break;
                    case 'c':
                        Demo(TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
                        break;
                    case 'd':
                        Demo(TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.LongRunning);
                        break;
                    case 'x':
                        return;
                    default:
                        Console.WriteLine("incorrect input.");
                        break;
                };
            }
        }


        ////Continuation task should be executed regardless of the result of the parent task.
        ////Задача продолжения должна выполняться независимо от результата родительской задачи.

        ////Continuation task should be executed when the parent task was completed without success.
        ////Задача продолжения должна выполняться, когда родительская задача завершилась безуспешно.

        ////Continuation task should be executed when the parent task failed and the parent task thread should be reused for continuation
        ////Задача продолжения должна выполняться при сбое родительской задачи, а поток родительской задачи следует повторно использовать для продолжения.

        ////Continuation task should be executed outside of the thread pool when the parent task is canceled
        ////Задача продолжения должна выполняться вне пула потоков, когда родительская задача отменяется.


        private static void Demo(TaskContinuationOptions continuationOptions)
        {
            ContinuationStatus(() => Console.WriteLine(TextBeforeSuccessfulCompletion), CancellationToken.None, continuationOptions);
            Console.WriteLine();
            ContinuationStatus(() =>
            {
                Console.WriteLine(TextBeforeException);
                throw new Exception();
            }, CancellationToken.None, continuationOptions);
            Console.WriteLine();
            using (var cts = new CancellationTokenSource())
            {
                ContinuationStatus(() =>
                {
                    Console.WriteLine(TextBeforeCancellation);
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();
                }, cts.Token, continuationOptions);
            }
        }

        private static void ContinuationStatus(Action parentAction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions)
        {
            try
            {
                Task.Run(() =>
                {
                    WriteStats("Parent");
                    parentAction();
                }, cancellationToken).ContinueWith(antecedent =>
                {
                    WriteStats("Continuation");
                    Console.WriteLine("Previous task was {0}.", TaskStatus(antecedent));
                }, continuationOptions).Wait();
            }
            catch (AggregateException){ }
        }

        private static void WriteStats(string name)
        {
            Console.Write("Thread #{0}{1}: {2} task. ",
                Thread.CurrentThread.ManagedThreadId,
                Thread.CurrentThread.IsThreadPoolThread ? " (ThreadPoolThread)" : string.Empty,
                name);
        }

        private static string TaskStatus(Task task)
        {
            if (task.IsCanceled) return "canceled";
            if (task.IsFaulted) return "faulted";
            if (task.IsCompletedSuccessfully) return "completed successfully";
            if (task.IsCompleted) return "completed";
            return task.Status.ToString();
        }
    }
}
