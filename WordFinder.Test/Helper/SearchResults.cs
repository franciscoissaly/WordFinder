using Finder = WordFinder.Logic.WordFinder;

namespace WordFinder.Test
{
    public class SearchResults
    {
        public required Finder Finder { get; init; }
        public required string[] SearchedWords { get; init; }

        public IEnumerable<string> FoundWords { get; set; } = Enumerable.Empty<string>();
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public double Duration { get; set; }
    }
}