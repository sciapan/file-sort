namespace FileGenerator
{
    internal class Generator
    {
        #region Fields

        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private readonly string[] _requiredPart =
        {
            "415. Apple",
            "30432. Something something something",
            "1. Apple",
            "32. Cherry is the best",
            "2. Banana is yellow",
            "15000. Something something something",
            "1220. Cherry is the best",
        };

        private readonly string _filePath;

        private readonly double _fileSizeInBytes;

        private const int NumberPartMaxRange = 10000;

        private const int TextPartMaxLength = 6;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the Generator class.
        /// </summary>
        /// <param name="filePath">Full file path.</param>
        /// <param name="fileSizeInGbs">File size in gbs.</param>
        public Generator(string filePath, double fileSizeInGbs)
        {
            _filePath = filePath;
            _fileSizeInBytes = fileSizeInGbs * 1024 * 1024 * 1024;
        }

        #endregion

        #region Methods

        internal void GenerateFile()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }

            // to fill the file
            var rng = new Random();

            using var fStream = File.Create(_filePath);
            {
                using var writer = new StreamWriter(fStream);
                {
                    // insert fixed part to guarantee at least few duplicates string
                    foreach (var required in _requiredPart)
                    {
                        writer.WriteLine(required);
                    }

                    // fill file till size is not exceed specified
                    while (fStream.Position < _fileSizeInBytes)
                    {
                        var number = rng.Next(0, NumberPartMaxRange);
                        var rngString = new string(Enumerable.Repeat(Chars, TextPartMaxLength).Select(s => s[rng.Next(s.Length)]).ToArray());;

                        writer.WriteLine($"{number}. {rngString}");
                    }
                }
            }
        }

        #endregion
    }
}