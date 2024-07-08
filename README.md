# K-Way Merge File Sorter

This project implements a K-way merge sort for sorting large text files. 
The input text file consists of rows with a number followed by random text.
 The sorter first sorts the file by number and then by text.

## Features

- Generate initial file with required size and information.
- Splits large files into smaller chunks.
- Sorts each chunk individually.
- Merges sorted chunks into a single sorted file.
- Uses asynchronous file I/O operations for improved performance.

### Example

Given an input file `random.txt` with the following content:

1. random text C
2. random text A
3. random text B

The sorted output file `random_sorted.txt` will contain:

1. random text A
2. random text B
3. random text C

## Project Structure

- `Program.cs`: Entry point of the application.
- `FileSplitter.cs`: Splits the input file into smaller chunks.
- `FilesMerger.cs`: Sorts and merges the chunks.
- `SortingItem.cs`: Represents a sortable item.
- `CustomSorter.cs`: Custom comparer for sorting items.