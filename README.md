# WordFinder

This project addresses the requirements for a code challenge from Qu, detailed in the included PDF file.

## Overview

The solution implements a class `WordFinder`, which can be built by passing to its constructor a collection of strings of the same length, representing rows in a character matrix. Once initialized, the `WordFinder` instance exposes a `Find` method that accepts a stream of arbitrary strings to search within the matrix, both horizontally and vertically. It returns a collection with the top ten found words with the highest number of matches.

The code aims to optimize performance and memory usage by using an approach conceptually similar to how regular expression engines search for matches. Within each matrix line, it tries to find a position where the searched word may start (by finding a character in the line that matches the first character of the word) and then checks whether the following characters in the line match the corresponding word characters. If a mismatch is found, it backtracks and tries again from the next position. If the word is fully matched, a counter increases and the algorithm continues looking for more matches until there are not enough characters left in the line for a full word match.

To avoid using more memory than needed, the algorithm does not copy the characters in the internal matrix or the searched words, but only traverses their content using indexed iterations. To search in columns instead of rows, it swaps the used indexes while accessing the matrix, effectively executing a transposed traverse. The number of matches per word is held in a dictionary that also enables the algorithm to skip already searched words.

For an example of its usage, check the FinderTests.FindTheThreeFromTheExample test method in the WordFinder.Test project, which implements the same example detailed in the PDF requirement file.
