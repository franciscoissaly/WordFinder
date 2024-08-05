using System.Collections.Concurrent;
using System.Diagnostics;

namespace WordFinder.Logic
{
    /// <summary>
    /// Manages the searching of arbitrary strings within the horizontal and vertical lines of a given character matrix. 
    /// </summary>
    public class WordFinder
    {
        public const int MaxRowsCount = 64;
        public const int MaxColumnsCount = 64;

        private readonly char[,] _matrix;


        /// <summary>
        /// Initializes a new instance with information for the internal character matrix to search in.
        /// </summary>
        /// <param name="matrix">A non empty collection of strings representing rows in the character matrix. 
        /// All the strings must be of the same length and contain only letters</param>
        /// <exception cref="ArgumentException"></exception>
        public WordFinder(IEnumerable<string> matrix)
        {
            ArgumentNullException.ThrowIfNull(matrix);

            int rowsCount = matrix.Count();
            if (rowsCount == 0)
                throw new ArgumentException(
                    "Invalid empty matrix. Expected at least 1 row."
                    , nameof(matrix));

            else if (rowsCount > MaxRowsCount)
                throw new ArgumentException(
                    $"Invalid rows count {rowsCount}. Expected at most {MaxRowsCount} rows."
                    , nameof(matrix));

            int columnsCount = 0;
            int rowIndex = 0;

            foreach (string rowString in matrix)
            {
                if (string.IsNullOrWhiteSpace(rowString))
                    throw new ArgumentException(
                        $"Invalid empty string for row {rowIndex + 1}."
                        , nameof(matrix));

                int rowStringLength = rowString.Length;
                if (rowStringLength > MaxColumnsCount)
                    throw new ArgumentException(
                        $"Invalid length {rowStringLength} on string '{rowString}' for row {rowIndex + 1}."
                        + $" Expected at most {MaxColumnsCount} characters."
                        , nameof(matrix));

                if (_matrix is null)
                {
                    // Create the internal matrix
                    columnsCount = rowStringLength;
                    _matrix = new char[rowsCount, columnsCount];
                }
                else if (rowStringLength != columnsCount)
                    throw new ArgumentException(
                        $"Invalid length {rowStringLength} on string '{rowString}' for row {rowIndex + 1}."
                        + $" Expected {columnsCount} characters."
                        , nameof(matrix));

                // Populate the matrix's current row
                for (int columnIndex = 0; columnIndex < columnsCount; columnIndex++)
                {
                    char character = rowString[columnIndex];
                    if (!char.IsLetter(character))
                        throw new ArgumentException(
                            $"Invalid character '{character}' for column {columnIndex + 1} on string '{rowString}' for row {rowIndex + 1}."
                            + " Expected a letter."
                            , nameof(matrix));

                    _matrix[rowIndex, columnIndex] = char.ToLower(character);  // To avoid case difference (and convert to lower once)
                }
                rowIndex++;
            }

            if (_matrix is null)
                _matrix = new char[0, 0];// Unreachable. Added just to comply with non-null readonly field requirement
        }


        /// <summary>
        /// Searches the passed strings within the matrix lines, horizontally and vertically.
        /// </summary>
        /// <param name="wordStream">A collection of words to be searched</param>
        /// <returns>The top 10 found words with the highest number of matches</returns>
        public IEnumerable<string> Find(IEnumerable<string> wordStream)
        {
            ArgumentNullException.ThrowIfNull(wordStream);

            IEnumerable<KeyValuePair<string, int>> matches;

            bool doSequentialSearch = true;
            if (doSequentialSearch)
            {
                var matchesByWord = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);// Avoid case difference while searching keys
                foreach (var word in wordStream)
                    if (!string.IsNullOrWhiteSpace(word)) // Skip empty words
                        if (!matchesByWord.ContainsKey(word)) // Skip already processed words
                            matchesByWord.Add(word, CountMatches(word));
                matches = matchesByWord;
            }
            else
            {
                // Alternative parallel approach. 
                // Dismissed for actually being much slower than going non-parallel,
                // possibly due to the small size of the wordStreams used for testing.
                // More benchmarking should be done to find a convenient size of wordStream for switching between algorithms.

                var matchesByWord = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);// Avoid case difference while searching keys
                Parallel.ForEach(wordStream, (word) =>
                {
                    if (!string.IsNullOrWhiteSpace(word)) // Skip empty words
                        matchesByWord.GetOrAdd(word, CountMatches(word));
                });
                matches = matchesByWord;
            }

            return (from pair in matches
                    where pair.Value > 0
                    orderby pair.Value descending
                    select pair.Key
                   ).Take(10);
        }


        /// <summary>
        /// Searches the passed word within the internal matrix in both orientations
        /// </summary>
        /// <param name="word">The word to search</param>
        /// <returns>The number of the word instances within the internal matrix lines</returns>
        private int CountMatches(string word)
        {
            string normalizedWord = word.ToLower(); // To avoid case difference (converting to lower once per word)
            return CountMatchesInMatrixLines(normalizedWord, transposed: false) // Search horizontally
                 + CountMatchesInMatrixLines(normalizedWord, transposed: true); // Search vertically
        }


        /// <summary>
        /// Iterates the matrix lines testing each position within the line as a candidate start for a match.
        /// Skips testing for words too long to fit in the matrix.
        /// </summary>
        /// <param name="word">The word to search</param>
        /// <param name="transposed">If true, treats the matrix as transposed, searching within columns instead of rows</param>
        /// <returns>The number of the word instances within the internal matrix lines</returns>
        private int CountMatchesInMatrixLines(string word, bool transposed)
        {
            int linesLength = GetMatrixLinesLength(transposed);
            int lastViablePosition = linesLength - word.Length;
            if (lastViablePosition < 0)
                return 0; //the word does not fit within the lines

            int linesCount = GetMatrixLinesCount(transposed);
            int matchesCount = 0;

            for (int lineNumber = 0; lineNumber < linesCount; lineNumber++)
                for (int startingPosition = 0; startingPosition <= lastViablePosition; startingPosition++)
                    if (IsPresentInMatrixLine(word, lineNumber, startingPosition, transposed))
                        matchesCount++;

            return matchesCount;
        }


        /// <summary>
        /// Checks whether characters in the passed word match the characters in the specified matrix line, 
        /// starting from the specified position.
        /// </summary>
        /// <param name="word">The word to search</param>
        /// <param name="lineNumber">The zero-based index of the line to search in</param>
        /// <param name="startingPosition">The zero-based index of the character, within the specified line, to start searching from</param>
        /// <param name="transposed">If true, treats the matrix as transposed, searching within columns instead of rows</param>
        /// <returns>True if the word matches the characters in the matrix, starting from the specified position</returns>
        private bool IsPresentInMatrixLine(string word, int lineNumber, int startingPosition, bool transposed)
        {
            int currentPosition = 0;
            foreach (char wordChar in word)
            {
                char lineChar = GetCharFromMatrix(lineNumber, startingPosition + currentPosition, transposed);
                if (lineChar != wordChar) // Non-match
                    return false;

                currentPosition++;
            }
            return true;
        }


        /// <summary>
        /// Reads from the internal matrix, the character in the specified line and position.
        /// </summary>
        /// <param name="lineNumber">The zero-based index of the line to access</param>
        /// <param name="position">The zero-based index of the character to access, within the specified line</param>
        /// <param name="transposed">If true, treats the matrix as transposed, swapping columns and rows</param>
        /// <returns>The specified character from the matrix</returns>
        private char GetCharFromMatrix(int lineNumber, int position, bool transposed)
        {
            if (transposed)
                return _matrix[position, lineNumber];
            else
                return _matrix[lineNumber, position];
        }


        /// <summary>
        /// Gets the number of characters of the lines in the matrix.
        /// </summary>
        /// <param name="transposed">If true, treats the matrix as transposed, swapping columns and rows</param>
        /// <returns>The amount of rows if transposed; the amount of columns, otherwise</returns>
        private int GetMatrixLinesLength(bool transposed)
        {
            return GetMatrixLinesCount(!transposed);
        }


        /// <summary>
        /// Gets the number lines in the matrix
        /// </summary>
        /// <param name="transposed">If true, treats the matrix as transposed, swapping columns and rows</param>
        /// <returns>The amount of columns if transposed; the amount of rows, otherwise</returns>
        private int GetMatrixLinesCount(bool transposed)
        {
            if (transposed)
                return Columns;
            else
                return Rows;
        }

        /// <summary>
        /// Get the number of rows in the internal matrix
        /// </summary>
        public int Rows => _matrix.GetLength(0);

        /// <summary>
        /// Get the number of columns in the internal matrix
        /// </summary>
        public int Columns => _matrix.GetLength(1);
    }
}