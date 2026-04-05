using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Lab_rabVP_2_ImamovaAR_BPI_23_02
{
    public record SortResult(int[] SortedArray, long Comparisons, double ElapsedMilliseconds)
    {
        public static SortResult Empty => new SortResult(Array.Empty<int>(), -1, 0);
    }

    public class ArraySorter
    {
        private long _totalComparisons;
        private readonly object _locker = new();
        public long TotalComparisons => _totalComparisons;

        public int[] GenerateRandomArray(int size)
        {
            Random rand = new();
            int[] array = new int[size];
            for (int i = 0; i < size; i++) array[i] = rand.Next(1000);
            return array;
        }

        public async Task<SortResult> BubbleSortAsync(int[] array, CancellationToken ct, IProgress<double> progress)
        {
            if (array == null) return SortResult.Empty;
            return await Task.Run(() =>
            {
                long comps = 0;
                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 0; i < array.Length - 1; i++)
                {
                    if (ct.IsCancellationRequested) return SortResult.Empty;
                    for (int j = 0; j < array.Length - 1 - i; j++)
                    {
                        comps++;
                        if (array[j] > array[j + 1]) (array[j], array[j + 1]) = (array[j + 1], array[j]);
                    }
                    progress?.Report((double)i / (array.Length - 1) * 100);
                }
                sw.Stop();
                lock (_locker) { _totalComparisons += comps; }
                return new SortResult(array, comps, sw.Elapsed.TotalMilliseconds);
            }, ct);
        }

        public async Task<SortResult> QuickSortAsync(int[] array, CancellationToken ct, IProgress<double> progress)
        {
            if (array == null) return SortResult.Empty;
            return await Task.Run(() =>
            {
                long comps = 0;
                Stopwatch sw = Stopwatch.StartNew();
                if (QuickSortRecursive(array, 0, array.Length - 1, ref comps, ct)) return SortResult.Empty;
                sw.Stop();
                progress?.Report(100);
                lock (_locker) { _totalComparisons += comps; }
                return new SortResult(array, comps, sw.Elapsed.TotalMilliseconds);
            }, ct);
        }

        private bool QuickSortRecursive(int[] arr, int left, int right, ref long comps, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return true;
            if (left >= right) return false;
            int i = left, j = right;
            int pivot = arr[(left + right) / 2];
            while (i <= j)
            {
                while (arr[i] < pivot) { i++; comps++; }
                while (arr[j] > pivot) { j--; comps++; }
                if (i <= j) { (arr[i], arr[j]) = (arr[j], arr[i]); i++; j--; }
            }
            if (QuickSortRecursive(arr, left, j, ref comps, ct)) return true;
            if (QuickSortRecursive(arr, i, right, ref comps, ct)) return true;
            return false;
        }

        public async Task<SortResult> InsertionSortAsync(int[] array, CancellationToken ct, IProgress<double> progress)
        {
            if (array == null) return SortResult.Empty;
            return await Task.Run(() =>
            {
                long comps = 0;
                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 1; i < array.Length; i++)
                {
                    if (ct.IsCancellationRequested) return SortResult.Empty;
                    int key = array[i]; int j = i - 1;
                    while (j >= 0 && array[j] > key) { comps++; array[j + 1] = array[j]; j--; }
                    comps++; array[j + 1] = key;
                    progress?.Report((double)i / array.Length * 100);
                }
                sw.Stop();
                lock (_locker) { _totalComparisons += comps; }
                return new SortResult(array, comps, sw.Elapsed.TotalMilliseconds);
            }, ct);
        }

        public async Task<SortResult> ShakerSortAsync(int[] array, CancellationToken ct, IProgress<double> progress)
        {
            if (array == null) return SortResult.Empty;
            return await Task.Run(() =>
            {
                long comps = 0;
                Stopwatch sw = Stopwatch.StartNew();
                int left = 0, right = array.Length - 1;
                while (left <= right)
                {
                    if (ct.IsCancellationRequested) return SortResult.Empty;
                    for (int i = left; i < right; i++)
                    {
                        comps++;
                        if (array[i] > array[i + 1]) (array[i], array[i + 1]) = (array[i + 1], array[i]);
                    }
                    right--;
                    for (int i = right; i > left; i--)
                    {
                        comps++;
                        if (array[i - 1] > array[i]) (array[i - 1], array[i]) = (array[i], array[i - 1]);
                    }
                    left++;
                    progress?.Report((double)(array.Length - (right - left)) / (double)array.Length * 100);
                }
                sw.Stop();
                lock (_locker) { _totalComparisons += comps; }
                return new SortResult(array, comps, sw.Elapsed.TotalMilliseconds);
            }, ct);
        }

        public void ResetTotal() { lock (_locker) _totalComparisons = 0; }
    }
}