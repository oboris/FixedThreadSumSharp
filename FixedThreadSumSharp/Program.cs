using System;
using System.Threading;

namespace FixedThreadSumSharp
{
    class Program
    {
        private static readonly int dim = 10000000;
        private static readonly int threadNum = 5;
        private static readonly int partNum = 18;

        private readonly Thread[] thread = new Thread[threadNum];

        static void Main(string[] args)
        {
            Program main = new Program();
            main.InitArr();
            Console.WriteLine(main.PartSum(new Bound(0, dim, true)));

            Console.WriteLine(main.ParallelSum());
            Console.ReadKey();
        }

        private int threadCount = 0;

        private long ParallelSum()
        {
            for (int i = 0; i < threadNum; i++)
            {
                (new Thread(StarterThread)).Start();
            }
            
            lock (lockerForCount)
            {
                while (threadCount < threadNum)
                {
                    Monitor.Wait(lockerForCount);
                }
            }
            return sum;
        }

        private readonly int[] arr = new int[dim];

        private void InitArr()
        {
            for (int i = 0; i < dim; i++)
            {
                arr[i] = i;
            }
        }

        public class Bound
        {
            public Bound(int startIndex, int finishIndex, bool goodBound)
            {
                StartIndex = startIndex;
                FinishIndex = finishIndex;
                GoodBound = goodBound;
            }

            public int StartIndex { get; set; }
            public int FinishIndex { get; set; }
            public bool GoodBound { get; set; }
        }

        private int currentPartNum = 0;
        private int partLength;

        public Bound GetNextBounds()
        {
            int startIndex = 0;
            int finishIndex = dim;
            bool goodBounds = false;

            if (currentPartNum < partNum)
            {
                goodBounds = true;
                startIndex = currentPartNum * partLength;
                currentPartNum++;
                if (currentPartNum < partNum)
                {
                    finishIndex = currentPartNum * partLength;
                }
            }

            return new Bound(startIndex, finishIndex, goodBounds);
        }

        private readonly object lockerForSum = new object();
        private void StarterThread()
        {
            partLength = arr.Length / partNum;
            Bound bound = GetNextBounds();
            while (bound.GoodBound)
            {
                long sum = PartSum(bound);

                lock (lockerForSum)
                {
                    CollectSum(sum);
                }

                bound = GetNextBounds();
            }
            IncThreadCount();
        }

        private readonly object lockerForCount = new object();
        private void IncThreadCount()
        {
            lock (lockerForCount)
            {
                threadCount++;
                Monitor.Pulse(lockerForCount);
            }
        }

        private long sum = 0;
        public void CollectSum(long sum)
        {
            this.sum += sum;
        }

        public long PartSum(Bound bound)
        {
            long sum = 0;
            for (int i = bound.StartIndex; i < bound.FinishIndex; i++)
            {
                sum += arr[i];
            }
            return sum;
        }
    }
}
