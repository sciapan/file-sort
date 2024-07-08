using System.Diagnostics;

namespace FileSorter
{
    internal class FilesMerger
    {
        #region Fields

        private readonly string _filePath;

        private readonly string[] _chunksFilesNames;

        private readonly Stopwatch _stopwatch;

        private readonly CustomSorter _customSorter = new();

        private int _numberOfChunks;

        private const int ChunksInParallel = 16; // allows to control how many chunks will be processed in parallel

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the FilesMerger class.
        /// </summary>
        /// <param name="filePath">File path to sorted file.</param>
        /// <param name="chunksFilesNames">Chunks files names.</param>
        /// <param name="stopwatch"><see cref="Stopwatch"/></param>
        public FilesMerger(string filePath, string[] chunksFilesNames, Stopwatch stopwatch)
        {
            _filePath = filePath;
            _chunksFilesNames = chunksFilesNames;
            _stopwatch = stopwatch;

            _numberOfChunks = chunksFilesNames.Length;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sorts the chunks and after merge-sort them.
        /// </summary>
        internal void SortAndMergeFiles()
        {
            switch (_chunksFilesNames.Length)
            {
                case 0:
                    return;
                case 1: // source file size is less or equal to chunk size - just sort it
                {
                    var sortChunk = SortChunkAsync(_chunksFilesNames[0]);
                    sortChunk.Start();
                    sortChunk.Wait();

                    File.Move(_chunksFilesNames[0], _filePath, true);
                    break;
                }
                default:
                {
                    var testFileDirectory = Path.GetDirectoryName(_filePath)!;
                    var fileName = Path.GetFileNameWithoutExtension(_filePath);

                    // used to track tasks which currently in progress
                    var mergeSortTasks = new Dictionary<string, Task>();

                    var numberOfInitRounds = Math.Round(_chunksFilesNames.Length / (double)ChunksInParallel,
                        MidpointRounding.ToPositiveInfinity);

                    // firstly sort initial chunks and merge-sort them
                    for (var i = 0; i < numberOfInitRounds; i++)
                    {
                        var chunksFilesNamesToProgress = _chunksFilesNames
                            .Skip(i * ChunksInParallel)
                            .Take(ChunksInParallel)
                            .ToArray();

                        // start to sort initial chunks
                        var sortingTasks = chunksFilesNamesToProgress.Select(SortChunkAsync).ToArray();
                        foreach (var sortingTask in sortingTasks)
                        {
                            sortingTask.Start();
                        }

                        // create merge-sort task for the chunks
                        var mergeSortChunkName = GetNextChunkName(testFileDirectory, fileName);
                        var mergeTask = MergeChunksAsync(chunksFilesNamesToProgress, mergeSortChunkName);

                        Task.Factory.ContinueWhenAll(sortingTasks, _ =>
                        {
                            mergeTask.Start();
                        });

                        mergeSortTasks.Add(mergeSortChunkName, mergeTask);

                        // if we don't have strong RAM limitations, we can remove parallel limitation by commenting this
                        Task.WaitAll(sortingTasks);
                    }

                    while (mergeSortTasks.Count > ChunksInParallel)
                    {
                        // when enough of merge-sort tasks completed - schedule next merge-sort task
                        var completedMergeSortTasks = mergeSortTasks.Where(x => x.Value.IsCompleted).ToArray();
                        if (completedMergeSortTasks.Length >= ChunksInParallel)
                        {
                            var numberOfRounds = Math.Round(completedMergeSortTasks.Length / (double)ChunksInParallel,
                                MidpointRounding.ToPositiveInfinity);

                            for (var i = 0; i < numberOfRounds; i++)
                            {
                                var chunksFilesNamesToProgress = completedMergeSortTasks
                                    .Select(x => x.Key)
                                    .Skip(i * ChunksInParallel)
                                    .Take(ChunksInParallel)
                                    .ToArray();

                                foreach (var chunkFileNameToProgress in chunksFilesNamesToProgress)
                                {
                                    mergeSortTasks.Remove(chunkFileNameToProgress);
                                }

                                var mergeSortChunkName = GetNextChunkName(testFileDirectory, fileName);
                                var mergeTask = MergeChunksAsync(chunksFilesNamesToProgress, mergeSortChunkName);
                                mergeTask.Start();

                                mergeSortTasks.Add(mergeSortChunkName, mergeTask);
                            }
                        }
                    }

                    // wait till last tasks completed
                    Task.WaitAll(mergeSortTasks.Values.ToArray());

                    Console.WriteLine($"Last merge stated at: {_stopwatch.Elapsed}");

                    var lastMerge = MergeChunksAsync(mergeSortTasks.Keys.ToArray(), _filePath);
                    lastMerge.Start();
                    lastMerge.Wait();
                    break;
                }
            }
        }

        #endregion

        #region Helpers

        private string GetNextChunkName(string testFileDirectory, string fileName)
        {
            _numberOfChunks++;

            return $"{Path.Combine(testFileDirectory, fileName)}_{_numberOfChunks}_chunk.txt";
        }

        private Task SortChunkAsync(string chunkFileName)
        {
            return new Task(() =>
            {
                var lines = File.ReadLines(chunkFileName);

                var buffer = lines.Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => new SortingItem(x))
                    .ToArray();

                Array.Sort(buffer, _customSorter);

                // write sorted content
                File.WriteAllLines(chunkFileName, buffer.Select(x => x.Number + ". " + x.Text));
            });
        }

        private Task MergeChunksAsync(string[] chunksFilesNames, string resultFileName)
        {
            return new Task(() =>
            {
                // create file to write sorted content
                using var fStream = new FileStream(resultFileName, FileMode.Create, FileAccess.Write);
                using var writer = new StreamWriter(fStream);

                // create reader for every chunk
                var readers = chunksFilesNames.Select(x => new StreamReader(x)).ToArray();

                // fill buffer for the first time
                var buffer = readers.Select((reader, i) => new SortingItem(reader.ReadLine()!, i)).ToList();
                buffer.Sort(_customSorter);

                // iterate till any line in buffer
                while (buffer.Any())
                {
                    var firstItem = buffer[0];

                    writer.WriteLine(firstItem.Number + ". " + firstItem.Text);

                    buffer.RemoveAt(0);

                    // get reader from which first entry comes
                    var reader = readers[firstItem.ReaderIndex];
                    var line = reader.ReadLine();
                    if (line != null) // end of the file
                    {
                        var newItem = new SortingItem(line, firstItem.ReaderIndex);

                        // insert new item in already sorted array
                        if (buffer.Count == 0)
                        {
                            buffer.Add(newItem);
                        }
                        else if (_customSorter.Compare(newItem, buffer[0]) <= 0)
                        {
                            buffer.Insert(0, newItem);
                        }
                        else if (_customSorter.Compare(newItem, buffer[^1]) >= 0)
                        {
                            buffer.Add(newItem);
                        }
                        else
                        {
                            var index = buffer.BinarySearch(newItem, _customSorter);
                            if (index < 0)
                            {
                                index = ~index;
                            }

                            buffer.Insert(index, newItem);
                        }
                    }
                }

                // clean-up
                foreach (var reader in readers)
                {
                    reader.Dispose();
                }

                foreach (var chunkFileName in chunksFilesNames)
                {
                    File.Delete(chunkFileName);
                }
            });
        }

        #endregion
    }
}