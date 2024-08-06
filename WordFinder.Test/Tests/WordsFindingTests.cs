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

            var search = _helper.ExecuteSearch(finder,
                [
                    "cold",
                    "Wind", // uppercase
                    "snow", // missing
                    "cold", // duplicated
                    "chill"
                ]);

            _helper.AssertFindings(search,
                expectedWords: ["cold",
                                "Wind", // case insensitive finding
                                "chill"
                                ],
                expectedFoundCount: 3, // no duplicate members
                unexpectedWords: ["snow"],
                expectedRanking: ["chill"]); // highest matches number

            _helper.PassTest(search);
        }


        [Test]
        public void ExampleScenarioExtendedSizeTest()
        {
            var finder = _helper.PrepareFinder(
                columnsCloningIterations: 9, // concatenate many clones of the following matrix to create a bigger one, and enable rotated words
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
                [ // unless specified otherwise, words are present 1 time per matrix clone
                    "cold", 
                    "Wind", // uppercase
                    "snow", // missing
                    "cold", // duplicated
                    "chill", // present 3 times per matrix clone
                    
                    "heat",
                    "blow", 
                    "fire", 
                    "hot", // present 1 time per matrix clone + 1 time between clones concatenations
                    "burn", 
                    "warm", 
                    
                    "spring", // present only between clones concatenation
                    "anana" // present 1 time per matrix clone + 2 times between clones concatenations
                ]);

            var notExpected = new string[]
            {
                "snow", //not found
                "spring" // 11th place in matches ranking
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