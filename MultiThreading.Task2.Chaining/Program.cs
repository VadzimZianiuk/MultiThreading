/*
 * 2.	Write a program, which creates a chain of four Tasks.
 * First Task – creates an array of 10 random integer.
 * Second Task – multiplies this array with another random integer.
 * Third Task – sorts this array by ascending.
 * Fourth Task – calculates the average value. All this tasks should print the values to console.
 */
using System;
using System.Threading.Tasks;

namespace MultiThreading.Task2.Chaining
{
    class Program
    {
        private static readonly Random random = new Random();

        static void Main(string[] args)
        {
            Console.WriteLine(".Net Mentoring Program. MultiThreading V1 ");
            Console.WriteLine("2.	Write a program, which creates a chain of four Tasks.");
            Console.WriteLine("First Task – creates an array of 10 random integer.");
            Console.WriteLine("Second Task – multiplies this array with another random integer.");
            Console.WriteLine("Third Task – sorts this array by ascending.");
            Console.WriteLine("Fourth Task – calculates the average value. All this tasks should print the values to console");
            Console.WriteLine();

            const int arrayLength = 10;
            Task.Run(() => GenerateRandomArray(arrayLength))
                .ContinueWith(task => Multiply(task.Result, GetRandom()))
                .ContinueWith(task => Sort(task.Result))
                .ContinueWith(task => Average(task.Result))
                .Wait();

            Console.ReadLine();
        }

        private static int GetRandom()
        {
            lock (random)
            {
                return random.Next();
            }
        }

        private static long[] GenerateRandomArray(int length)
        {
            Console.WriteLine("Task #{0}. Generating an array of random numbers.", Task.CurrentId);
            var data = new long[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = GetRandom();
                Console.WriteLine(data[i]);
            }
            return data;
        }

        private static long[] Multiply(long[] data, int multiplier)
        {
            Console.WriteLine("Task #{0}. Multiplying an array by a {1}.", Task.CurrentId, multiplier);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] *= multiplier;
                Console.WriteLine(data[i]);
            }
            return data;
        }

        private static long[] Sort(long[] data)
        {
            Console.WriteLine("Task #{0}. Sorting an array by ascending.", Task.CurrentId);
            Array.Sort(data);
            foreach (var item in data)
            {
                Console.WriteLine(item);
            }
            return data;
        }

        private static long Average(long[] collection)
        {
            double sum = 0;
            foreach (var item in collection)
            {
                sum += item;
            }

            var average = (long)(sum / collection.Length);
            Console.WriteLine("Task #{0}. The average value of an array is {1}.", Task.CurrentId, average);
            return average;
        }
    }
}
