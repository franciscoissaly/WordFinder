using System.Text;
using Finder = WordFinder.Logic.WordFinder;

namespace WordFinder.Test
{
    public class WordFinderTestHelper
    {

        public Finder PrepareFinder(IEnumerable<string> matrixStrings, int columnsCloningIterations = 0)
        {
            var matrix = new List<string>();
            if (matrixStrings is not null)
                foreach (var matrixString in matrixStrings)
                {
                    string rowString = matrixString;
                    if (rowString is not null)
                        rowString = rowString.Replace(" ", ""); // remove spaces

                    if (columnsCloningIterations > 0)
                    {
                        var sb = new StringBuilder();
                        for (int i = 0; i <= columnsCloningIterations; i++)
                            sb.Append(rowString);
                        rowString = sb.ToString();
                    }
                    matrix.Add(rowString!);
                }

            var finder = new Finder(matrix);
            Assert.That(finder.Rows, Is.EqualTo(matrix.Count()));
            Assert.That(finder.Columns, Is.EqualTo(matrix.First().Length));

            return finder;
        }


        public SearchResults ExecuteSearch(Finder finder, string[] words)
        {
            var result = new SearchResults() { Finder = finder, SearchedWords = words };
            result.Start = DateTime.Now;
            result.FoundWords = finder.Find(words);
            result.End = DateTime.Now;
            result.Duration = result.End.Subtract(result.Start).TotalMilliseconds;

            Assert.That(result.FoundWords, Is.Not.Null);
            return result;

        }

        public void AssertFindings(SearchResults search,
            int? expectedFoundCount = null,
            IEnumerable<string>? expectedWords = null,
            IEnumerable<string>? unexpectedWords = null,
            string[]? expectedRanking = null)
        {
            if (expectedFoundCount is not null)
                Assert.That(search.FoundWords.Count, Is.EqualTo(expectedFoundCount.Value));

            if (unexpectedWords is not null)
                foreach (var unexpectedWord in unexpectedWords)
                    Assert.That(search.FoundWords, Has.No.Member(unexpectedWord));
            else
                unexpectedWords = Enumerable.Empty<string>();

            if (expectedWords is not null)
                foreach (var expectedWord in search.SearchedWords.Except(unexpectedWords))
                    Assert.That(search.FoundWords, Has.Member(expectedWord));

            if (expectedRanking is not null)
                for (var i = 0; i < expectedRanking.Length; i++)
                    Assert.That(search.FoundWords.Skip(i).First, Is.EqualTo(expectedRanking[i]));
        }


        public void PassTest(SearchResults search)
        {
            Assert.Pass($"Searching {search.SearchedWords.Count()} words in a {search.Finder.Rows}x{search.Finder.Columns} matrix took {search.Duration.ToString("N2")} ms");
        }
    }
}