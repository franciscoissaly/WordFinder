namespace WordFinder.Logic
{
    public class WordFinder
    {
        public int MaxRowsCount = 64;
        public int MaxColumnsCount = 64;

        private readonly char[,] _matrix;

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
                if (string.IsNullOrEmpty(rowString))
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
                    columnsCount = rowStringLength;
                    _matrix = new char[rowsCount, columnsCount];
                }
                else if (rowStringLength != columnsCount)
                    throw new ArgumentException(
                        $"Invalid length {rowStringLength} on string '{rowString}' for row {rowIndex + 1}."
                        + $" Expected {columnsCount} characters."
                        , nameof(matrix));

                for (int columnIndex = 0; columnIndex < columnsCount; columnIndex++)
                {
                    char character = rowString[columnIndex];
                    if (!char.IsLetter(character))
                        throw new ArgumentException(
                            $"Invalid character '{character}' for column {columnIndex + 1} on string '{rowString}' for row {rowIndex + 1}."
                            + " Expected a letter."
                            , nameof(matrix));

                    _matrix[rowIndex, columnIndex] = char.ToLower(character);
                }
                rowIndex++;
            }

            if (_matrix is null)
                _matrix = new char[0, 0];// Unreachable. Added just to comply with non-null readonly field requirement
        }


        public IEnumerable<string> Find(IEnumerable<string> wordStream)
        {
            ArgumentNullException.ThrowIfNull(wordStream);

            var matchesByWord = new Dictionary<string, int>();

            foreach (var word in wordStream)
                if (!string.IsNullOrEmpty(word) && !matchesByWord.ContainsKey(word))
                    matchesByWord.Add(word, CountMatches(word));

            return (from pair in matchesByWord
                    where pair.Value > 0
                    orderby pair.Value descending
                    select pair.Key
                   ).Take(10);
        }


        private int CountMatches(string word)
        {
            string normalizedWord = word.ToLower();
            return CountMatchesInMatrixLines(normalizedWord, transposed: false) //horizontally
                 + CountMatchesInMatrixLines(normalizedWord, transposed: true); //vertically
        }


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


        private bool IsPresentInMatrixLine(string word, int lineNumber, int startingPosition, bool transposed)
        {
            int currentPosition = 0;
            foreach (char wordChar in word)
            {
                char lineChar = GetCharFromMatrix(lineNumber, startingPosition + currentPosition, transposed);
                if (lineChar != wordChar)
                    return false;

                currentPosition++;
            }
            return true;
        }


        private char GetCharFromMatrix(int lineNumber, int position, bool transposed)
        {
            if (transposed)
                return _matrix[position, lineNumber];
            else
                return _matrix[lineNumber, position];
        }


        private int GetMatrixLinesLength(bool transposed)
        {
            return GetMatrixLinesCount(!transposed);
        }


        private int GetMatrixLinesCount(bool transposed)
        {
            int rank = 0;//rows
            if (transposed)
                rank = 1;//columns

            return _matrix.GetLength(rank);
        }
    }
}