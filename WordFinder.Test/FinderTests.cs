using System.Runtime.Serialization;
using WordFinder.Logic;

namespace WordFinder.Test
{
    public class FinderTests
    {
        [Test]
        public void FindTheWordsFromTheRequirementExample()
        {
            var finder = new WordFinder.Logic.WordFinder(
            [
                "abcdc",
                "fgwio",
                "chill",
                "pqnsd",
                "uvdxy"
            ]);

            var found = finder.Find(["cold", "wind", "snow", "chill"]);
            Assert.That(found, Is.Not.Null);
            Assert.That(found.Count, Is.EqualTo(3));
            Assert.That(found, Has.Member("cold"));
            Assert.That(found, Has.Member("wind"));
            Assert.That(found, Has.Member("chill"));
            Assert.That(found, Has.No.Member("snow"));
        }
    }
}