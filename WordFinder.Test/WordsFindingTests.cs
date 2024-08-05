
using System.Text;

namespace WordFinder.Test
{
    public class WordsFindingTests
    {

        [Test]
        public void ExampleScenarioTest()
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
            Assert.That(found, Has.Member("cold"));
            Assert.That(found, Has.Member("wind"));
            Assert.That(found, Has.Member("chill"));
            Assert.That(found, Has.No.Member("snow"));
        }


        [Test]
        public void ExampleScenarioExtendedCasesTest()
        {
            var finder = new WordFinder.Logic.WordFinder(
            [ // non-square matrix
                "abcdcc",
                "fgwioh",
                "chilli",
                "pqnsdl",
                "uvdxyl"
            ]);

            var found = finder.Find(["cold", "Wind", "snow", "cold", "chill"]);
            Assert.That(found, Is.Not.Null);

            Assert.That(found, Has.No.Member("snow"));

            Assert.That(found, Has.Member("cold"));
            Assert.That(found, Has.Member("Wind"));// case insensitive finding
            Assert.That(found, Has.Member("chill"));
            Assert.That(found.Count, Is.EqualTo(3));// no duplicate members

            Assert.That(found.First, Is.EqualTo("chill")); // highest matches number
        }


        [Test]
        public void ExampleScenarioExtendedSizeTest()
        {
            var matrixStrings = new string[] 
            {
                "a b c d c c",
                "o t w b o h",
                "c h i l l i",
                "p q n o d l",
                "u v d w y l",
                "w x F b x c", // uppercase F
                "a b i u h h",
                "r g r r o i",
                "m h e n t l",
                "o h e a t l",
                "g s p r i n",
                "a n a n a n"
            };

            // clone columns 10 times
            var matrix = new List<string>();
            foreach (var rowString in matrixStrings)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < 10; i++)
                    sb.Append(rowString.Replace(" ","")); // remove spaces
                matrix.Add(sb.ToString());
            }        
            var finder = new WordFinder.Logic.WordFinder(matrix);

            var words = new string[] {"cold", "Wind", "snow", "cold", "chill",
                                      "heat", "blow", "fire", "hot", "burn", "warm",
                                      "spring", "anana" };
            
            var mostFound = finder.Find(words);
            Assert.That(mostFound, Is.Not.Null);
            Assert.That(mostFound.Count, Is.EqualTo(10)); // top ten by matches number

            var notExpected = new string[]
            { 
                "snow", //not found
                "spring" // eleventh place in matches ranking
            };
            foreach (var unexpectedWord in notExpected)
                Assert.That(mostFound, Has.No.Member(unexpectedWord)); 

            foreach (var expectedWord in words.Except(notExpected))
                Assert.That(mostFound, Has.Member(expectedWord)); 

            // check that the order agrees with the number of matches
            var mostMatching = new string[] {"chill", "anana", "hot"};
            for (var i = 0; i < mostMatching.Length; i++)
                Assert.That(mostFound.Skip(i).First, Is.EqualTo(mostMatching[i])); 
        }
    }
}