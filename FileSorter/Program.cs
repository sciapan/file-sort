using System.Diagnostics;
using FileSorter;

const string pathToTestFile = @"D:\random.txt";
const string pathToSortedFile = @"D:\random_sorted.txt";
    
Console.WriteLine("Starting to sort file.");

var stopwatch = new Stopwatch();
stopwatch.Start();

// split file to the chunks
var fileSplitter = new FileSplitter(pathToTestFile, stopwatch);
var chunksFilesNames = fileSplitter.SplitFileToChunks();

// sort initial chunks and after merge-sort them
var filesMerger = new FilesMerger(pathToSortedFile, chunksFilesNames, stopwatch);
filesMerger.SortAndMergeFiles();

stopwatch.Stop();

Console.WriteLine($"Sorting is done. Final time is: {stopwatch.Elapsed}");
Console.ReadKey();