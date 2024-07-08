using System.Diagnostics;

namespace FileSorter
{
    internal class FileSplitter
    {
        #region Fields

        private readonly string _filePath;

        private readonly Stopwatch _stopwatch;

        private int _numberOfChunks; // stores number of chunks

        private const char NewLineSeparator = '\n';

        private const int ChunkSize = 1024 * 1024 * 8; // initial chunk size 8MB

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the FileSplitter class.
        /// </summary>
        /// <param name="filePath">Path to file which should be sorted.</param>
        /// <param name="stopwatch"><see cref="Stopwatch"/></param>
        public FileSplitter(string filePath, Stopwatch stopwatch)
        {
            _filePath = filePath;
            _stopwatch = stopwatch;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Split file to the chunks.
        /// </summary>
        /// <returns>Chunks files names.</returns>
        internal string[] SplitFileToChunks()
        {
            var testFileDirectory = Path.GetDirectoryName(_filePath);
            var fileName = Path.GetFileNameWithoutExtension(_filePath);

            var chunksFilesNames = new List<string>();

            // buffers for the chunk
            var buffer = new byte[ChunkSize];
            var additionalBuffer = new List<byte>();
            
            var bytesReadInCurrentChunk = 0;

            using (var reader = new StreamReader(_filePath))
            {
                int value;
                while ((value = reader.Read()) != -1)
                {
                    var nextChar = (byte)value; 

                    // have read enough data for the chunk & line ended
                    if (bytesReadInCurrentChunk >= ChunkSize && nextChar == NewLineSeparator)
                    {
                        additionalBuffer.Add(nextChar);

                        CreateChunk(buffer.Length);

                        // clear buffer
                        additionalBuffer.Clear();
                        bytesReadInCurrentChunk = 0;
                    }
                    else if (bytesReadInCurrentChunk >= ChunkSize)
                    {
                        additionalBuffer.Add(nextChar);
                        bytesReadInCurrentChunk++;
                    }
                    else
                    {
                        buffer[bytesReadInCurrentChunk] = nextChar;
                        bytesReadInCurrentChunk++;
                    }
                }
            }
            
            if (buffer.Any() || additionalBuffer.Any())
            {
                CreateChunk(bytesReadInCurrentChunk);
            }

            void CreateChunk(int count)
            {
                _numberOfChunks++;

                var chunkFileName = $"{Path.Combine(testFileDirectory!, fileName)}_{_numberOfChunks}_chunk.txt";
                chunksFilesNames.Add(chunkFileName);

                // create chunk file
                using var writer = File.Create(chunkFileName);
                {
                    writer.Write(buffer, 0, count);
                    writer.Write(additionalBuffer.ToArray(), 0, additionalBuffer.Count);
                }
            }

            Console.WriteLine($"Initial file split for: {_stopwatch.Elapsed}");

            return chunksFilesNames.ToArray();
        }

        #endregion
    }
}