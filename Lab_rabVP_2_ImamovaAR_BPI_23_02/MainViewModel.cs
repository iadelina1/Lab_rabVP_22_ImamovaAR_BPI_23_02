using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lab_rabVP_2_ImamovaAR_BPI_23_02
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ArraySorter _sorter;
        private int[]? _originalArray;
        private CancellationTokenSource _cts = new();

        [ObservableProperty] private int _arraySize = 1000;
        [ObservableProperty] private int _maxThreads = 2;
        [ObservableProperty] private bool _useSharedArray;
        [ObservableProperty] private string _originalArrayString = "Массив не сгенерирован";
        [ObservableProperty] private string? _bubbleSortResult;
        [ObservableProperty] private string? _quickSortResult;
        [ObservableProperty] private string? _insertionSortResult;
        [ObservableProperty] private string? _shakerSortResult;
        [ObservableProperty] private string _totalComparisons = "Общее число сравнений: 0";

        [ObservableProperty] private double _bubbleProgress;
        [ObservableProperty] private double _quickProgress;
        [ObservableProperty] private double _insertionProgress;
        [ObservableProperty] private double _shakerProgress;

        public IAsyncRelayCommand GenerateArrayCommand { get; }
        public IAsyncRelayCommand RunBubbleCommand { get; }
        public IAsyncRelayCommand RunQuickCommand { get; }
        public IAsyncRelayCommand RunInsertionCommand { get; }
        public IAsyncRelayCommand RunShakerCommand { get; }
        public IAsyncRelayCommand RunAllCommand { get; }
        public IRelayCommand CancelAllCommand { get; }
        public IRelayCommand ResetAllCommand { get; }

        public MainViewModel()
        {
            _sorter = new ArraySorter();
            GenerateArrayCommand = new AsyncRelayCommand(GenerateArrayAsync);

            RunBubbleCommand = new AsyncRelayCommand(() => RunSortTask("Bubble"), () => _originalArray != null);
            RunQuickCommand = new AsyncRelayCommand(() => RunSortTask("Quick"), () => _originalArray != null);
            RunInsertionCommand = new AsyncRelayCommand(() => RunSortTask("Insertion"), () => _originalArray != null);
            RunShakerCommand = new AsyncRelayCommand(() => RunSortTask("Shaker"), () => _originalArray != null);

            RunAllCommand = new AsyncRelayCommand(async () => {
                await Task.WhenAll(
                    RunSortTask("Bubble"),
                    RunSortTask("Quick"),
                    RunSortTask("Insertion"),
                    RunSortTask("Shaker")
                );
            });

            CancelAllCommand = new RelayCommand(() => { _cts.Cancel(); _cts = new CancellationTokenSource(); });
            ResetAllCommand = new RelayCommand(Reset);
        }

        private async Task RunSortTask(string type)
        {
            if (_originalArray == null) return;
            var progress = new Progress<double>(v => SetProgress(type, v));
            SetStatus(type, "Сортируется...");

            var container = new ArrayContainer(_originalArray, UseSharedArray);
            int[] workingArray = container.GetArray();

            SortResult res = type switch
            {
                "Bubble" => await _sorter.BubbleSortAsync(workingArray, _cts.Token, progress),
                "Quick" => await _sorter.QuickSortAsync(workingArray, _cts.Token, progress),
                "Insertion" => await _sorter.InsertionSortAsync(workingArray, _cts.Token, progress),
                "Shaker" => await _sorter.ShakerSortAsync(workingArray, _cts.Token, progress),
                _ => SortResult.Empty
            };

            if (res.Comparisons != -1)
            {
                SetStatus(type, $"{res.ElapsedMilliseconds:F2} мс, {res.Comparisons} сравн.");
                TotalComparisons = $"Общее число сравнений: {_sorter.TotalComparisons}";
            }
            else
            {
                SetStatus(type, "Отменено");
            }
        }

        private async Task GenerateArrayAsync()
        {
            _originalArray = _sorter.GenerateRandomArray(ArraySize);
            OriginalArrayString = $"Массив на {ArraySize} эл. готов";
            ResetResults();
            NotifyCommands();
            await Task.CompletedTask;
        }

        private void Reset()
        {
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            _originalArray = null;
            _sorter.ResetTotal();
            OriginalArrayString = "Массив не сгенерирован";
            ResetResults();
            TotalComparisons = "Общее число сравнений: 0";
            NotifyCommands();
        }

        private void ResetResults()
        {
            BubbleSortResult = QuickSortResult = InsertionSortResult = ShakerSortResult = "Готов";
            BubbleProgress = QuickProgress = InsertionProgress = ShakerProgress = 0;
        }

        private void SetStatus(string t, string s)
        {
            if (t == "Bubble") BubbleSortResult = s;
            else if (t == "Quick") QuickSortResult = s;
            else if (t == "Insertion") InsertionSortResult = s;
            else if (t == "Shaker") ShakerSortResult = s;
        }

        private void SetProgress(string t, double v)
        {
            if (t == "Bubble") BubbleProgress = v;
            else if (t == "Quick") QuickProgress = v;
            else if (t == "Insertion") InsertionProgress = v;
            else if (t == "Shaker") ShakerProgress = v;
        }

        private void NotifyCommands()
        {
            RunBubbleCommand.NotifyCanExecuteChanged();
            RunQuickCommand.NotifyCanExecuteChanged();
            RunInsertionCommand.NotifyCanExecuteChanged();
            RunShakerCommand.NotifyCanExecuteChanged();
        }
        // здесь закончили
        private class ArrayContainer
        {
            private readonly int[] _source;
            private readonly bool _useShared;

            public ArrayContainer(int[] source, bool useShared)
            {
                _source = source;
                _useShared = useShared;
            }

            public int[] GetArray()
            {
                return _useShared ? _source : (int[])_source.Clone();
            }
        }
    }
}