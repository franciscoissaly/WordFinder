namespace WordFinder.Test
{
    public class WordsFindingTests
    {
        private WordFinderTestHelper _helper = new();

        [Test]
        public void ExampleScenarioTest()
        {
            var finder = _helper.PrepareFinder(
            [
                "a b c d c",
                "f g w i o",
                "c h i l l",
                "p q n s d",
                "u v d x y"
            ]);

            var search = _helper.ExecuteSearch(finder, ["cold", "wind", "snow", "chill"]);

            _helper.AssertFindings(search,
                expectedWords: ["cold", "wind", "chill"],
                unexpectedWords: ["snow"]);

            _helper.PassTest(search);
        }


        [Test]
        public void ExampleScenarioExtendedCasesTest()
        {
            var finder = _helper.PrepareFinder(
            [ // non-square matrix
                "a b c d c c",
                "f g w i o h",
                "c h i l l i",
                "p q n s d l",
                "u v d x y l"
            ]);

            var search = _helper.ExecuteSearch(finder, ["cold", "Wind", "snow", "cold", "chill"]);

            _helper.AssertFindings(search,
                expectedWords: ["cold",
                                "Wind", // case insensitive finding
                                "chill"],
                expectedFoundCount: 3, // no duplicate members
                unexpectedWords: ["snow"],
                expectedRanking: ["chill"]); // highest matches number

            _helper.PassTest(search);
        }


        [Test]
        public void ExampleScenarioExtendedSizeTest()
        {
            var finder = _helper.PrepareFinder(columnsCloningIterations: 9,
            matrixStrings: 
            [
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
            ]);

            var search = _helper.ExecuteSearch(finder,
                ["cold", "Wind", "snow", "cold", "chill",
                 "heat", "blow", "fire", "hot", "burn", "warm",
                 "spring", "anana"
                ]);

            var notExpected = new string[]
            {
                "snow", //not found
                "spring" // eleventh place in matches ranking
            };

            _helper.AssertFindings(search,
                expectedFoundCount: 10, // only top 10, even though 11 can be found 
                unexpectedWords: notExpected,
                expectedWords: search.SearchedWords.Except(notExpected),
                expectedRanking: ["chill", "anana", "hot"]);

            _helper.PassTest(search);
        }
    }
}