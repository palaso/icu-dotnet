﻿// Copyright (c) 2013 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Linq;
using NUnit.Framework;

namespace Icu.Tests
{
	[TestFixture]
	[Category("Full ICU")]
	public class BreakIteratorTests
	{
		[Test]
		public void Split_Character()
		{
			var parts = BreakIterator.Split(BreakIterator.UBreakIteratorType.CHARACTER, "en-US", "abc");

			Assert.That(parts.Count(), Is.EqualTo(3));
			Assert.That(parts.ToArray(), Is.EquivalentTo(new[] { "a", "b", "c" }));
		}

		[Test]
		public void Split_Word()
		{
			var parts = BreakIterator.Split(BreakIterator.UBreakIteratorType.WORD, "en-US", "Aa Bb. Cc");
			Assert.That(parts.Count(), Is.EqualTo(3));
			Assert.That(parts.ToArray(), Is.EquivalentTo(new[] { "Aa", "Bb", "Cc" }));
		}

		[Test]
		public void Split_Line()
		{
			var parts = BreakIterator.Split(BreakIterator.UBreakIteratorType.LINE, "en-US", "Aa Bb. Cc");
			Assert.That(parts.Count(), Is.EqualTo(3));
			Assert.That(parts.ToArray(), Is.EquivalentTo(new[] { "Aa ", "Bb. ", "Cc" }));
		}

		[Test]
		public void Split_Sentence()
		{
			var parts = BreakIterator.Split(BreakIterator.UBreakIteratorType.SENTENCE, "en-US", "Aa bb. Cc 3.5 x? Y?x! Z");
			Assert.That(parts.ToArray(), Is.EquivalentTo(new[] { "Aa bb. ", "Cc 3.5 x? ", "Y?", "x! ", "Z" }));
			Assert.That(parts.Count(), Is.EqualTo(5));
		}

		[Test]
		public void GetBoundaries_Character()
		{
			var text = "abc? 1";
			var expected = new[] {
				new Boundary(0, 1), new Boundary(1, 2), new Boundary(2, 3), new Boundary(3, 4), new Boundary(4, 5), new Boundary(5, 6)
			};

			var parts = BreakIterator.GetBoundaries(BreakIterator.UBreakIteratorType.CHARACTER, new Locale("en-US"), text);

			Assert.That(parts.Count(), Is.EqualTo(expected.Length));
			Assert.That(parts.ToArray(), Is.EquivalentTo(expected));
		}

		[Test]
		public void GetBoundaries_Word()
		{
			var parts = BreakIterator.GetBoundaries(BreakIterator.UBreakIteratorType.WORD, new Locale("en-US"), WordBoundaryTestData.Text);

			Assert.That(parts.Count(), Is.EqualTo(WordBoundaryTestData.ExpectedOnlyWords.Length));
			Assert.That(parts.ToArray(), Is.EquivalentTo(WordBoundaryTestData.ExpectedOnlyWords));
		}

		[Test]
		public void GetBoundaries_Line()
		{
			var text = "Aa bb. Ccdef 3.5 x? Y?x! Z";
			var expected = new[] {
				new Boundary(0, 3), new Boundary(3, 7), new Boundary(7, 13), new Boundary(13, 17), new Boundary(17, 20),
				new Boundary(20, 22), new Boundary(22, 25), new Boundary(25, 26)
			};

			var parts = BreakIterator.GetBoundaries(BreakIterator.UBreakIteratorType.LINE, new Locale("en-US"), text);

			Assert.That(parts.Count(), Is.EqualTo(expected.Length));
			Assert.That(parts.ToArray(), Is.EquivalentTo(expected));
		}

		[Test]
		public void GetBoundaries_Sentence()
		{
			var text = "Aa bb. Ccdef 3.5 x? Y?x! Z";
			var expected = new[] {
				new Boundary(0, 7), new Boundary(7, 20), new Boundary(20, 22), new Boundary(22, 25), new Boundary(25, 26)
			};

			var parts = BreakIterator.GetBoundaries(BreakIterator.UBreakIteratorType.SENTENCE, new Locale("en-US"), text);

			Assert.That(parts.Count(), Is.EqualTo(expected.Length));
			Assert.That(parts.ToArray(), Is.EquivalentTo(expected));
		}

		[Test]
		public void GetWordBoundaries_IgnoreSpacesAndPunctuation()
		{
			var onlyWords = BreakIterator.GetWordBoundaries(new Locale("en-US"), WordBoundaryTestData.Text, false);

			Assert.That(onlyWords.Count(), Is.EqualTo(WordBoundaryTestData.ExpectedOnlyWords.Length));
			Assert.That(onlyWords.ToArray(), Is.EquivalentTo(WordBoundaryTestData.ExpectedOnlyWords));
		}

		[Test]
		public void GetWordBoundaries_IncludeSpacesAndPunctuation()
		{
			var allBoundaries = BreakIterator.GetWordBoundaries(new Locale("en-US"), WordBoundaryTestData.Text, true);

			Assert.That(allBoundaries.Count(), Is.EqualTo(WordBoundaryTestData.ExpectedAllBoundaries.Length));
			Assert.That(allBoundaries.ToArray(), Is.EquivalentTo(WordBoundaryTestData.ExpectedAllBoundaries));
		}

		/// <summary>
		/// The hypenated text case tests the difference between Word and Line
		/// breaks described in:
		/// http://userguide.icu-project.org/boundaryanalysis#TOC-Line-break-Boundary
		/// </summary>
		[Test]
		public void GetWordAndLineBoundariesWithHyphenatedText()
		{
			var text = "Good-day, kind sir !";
			var expectedWords = new[] {
				new Boundary(0, 4), new Boundary(5, 8), new Boundary(10, 14), new Boundary(15, 18)
			};
			var expectedLines = new[] {
				new Boundary(0, 5), new Boundary(5, 10), new Boundary(10, 15), new Boundary(15, 20)
			};

			var wordBoundaries = BreakIterator.GetBoundaries(BreakIterator.UBreakIteratorType.WORD, new Locale("en-US"), text);
			var lineBoundaries = BreakIterator.GetBoundaries(BreakIterator.UBreakIteratorType.LINE, new Locale("en-US"), text);

			Assert.That(wordBoundaries.Count(), Is.EqualTo(expectedWords.Length));
			Assert.That(wordBoundaries.ToArray(), Is.EquivalentTo(expectedWords));

			Assert.That(lineBoundaries.Count(), Is.EqualTo(expectedLines.Length));
			Assert.That(lineBoundaries.ToArray(), Is.EquivalentTo(expectedLines));
		}

		[Test]
		public void CreateChracterInstanceTest()
		{
			var locale = new Locale("de-DE");
			var text = "Good-bye, dear!";
			var expected = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

			using (var bi = BreakIterator.CreateCharacterInstance(locale))
			{
				bi.SetText(text);

				Assert.AreEqual(locale, bi.Locale);
				Assert.AreEqual(text, bi.Text);
				CollectionAssert.AreEqual(expected, bi.Boundaries);

				// Verify each boundary and rule status.
				for (int i = 0; i < expected.Length; i++)
				{
					int current = bi.Current;
					int status = bi.GetRuleStatus();

					Assert.AreEqual(expected[i], current);
					Assert.AreEqual(0, status);

					int moveNext = bi.MoveNext();
					int next = i + 1;

					if (next < expected.Length)
					{
						Assert.AreEqual(expected[next], moveNext);
					}
					else
					{
						// Verify that the BreakIterator is exhausted because we've
						// moved past every item.
						Assert.AreEqual(BreakIterator.DONE, moveNext);
					}
				}

				// Verify that the BreakIterator is exhausted because we've
				// moved past every item, so current should be the last offset.
				int lastIndex = expected.Length - 1;
				Assert.AreEqual(expected[lastIndex], bi.Current);
			}
		}

		/// <summary>
		/// Checking that when a break iterator is created with "", that it
		/// returns the correct properties.
		/// </summary>
		[Test]
		public void BreakIteratorThatIsEmpty()
		{
			var locale = new Locale("de-DE");
			string text = string.Empty;

			using (var bi = BreakIterator.CreateCharacterInstance(locale))
			{
				bi.SetText(text);

				Assert.AreEqual(locale, bi.Locale);
				Assert.AreEqual(text, bi.Text);
				Assert.AreEqual(0, bi.Boundaries.Length);

				Assert.AreEqual(0, bi.Current);
				Assert.AreEqual(BreakIterator.DONE, bi.MoveNext());
				Assert.AreEqual(0, bi.MoveFirst());
				Assert.AreEqual(0, bi.MoveLast());
				Assert.AreEqual(BreakIterator.DONE, bi.MovePrevious());

				// Default value is 0 when there was no rule applied.
				Assert.AreEqual(0, bi.GetRuleStatus());
				// When iterator is at DONE, it returns the default rule status vector.
				CollectionAssert.AreEqual(new[] { 0 }, bi.GetRuleStatusVector());
			}
		}

		/// <summary>
		/// Checking that when a break iterator is created with null that it
		/// throws an ArgumentNullException.
		/// </summary>
		[Test]
		public void BreakIteratorThatIsNull()
		{
			var locale = new Locale("de-DE");

			using (var bi = BreakIterator.CreateCharacterInstance(locale))
			{
				Assert.Throws<ArgumentNullException>(() =>
				{
					bi.SetText(null);
				});
			}
		}

		[Test]
		public void CanIterateForwards()
		{
			var locale = new Locale("de-DE");
			var text = "Good-day, kind sir !";
			var expected = new int[] { 0, 4, 5, 8, 9, 10, 14, 15, 18, 19, 20 };

			var none = BreakIterator.UWordBreak.NONE;
			var letter = BreakIterator.UWordBreak.LETTER;
			var ruleStatus = new[] { none, letter, none, letter, none, none, letter, none, letter, none, none };

			using (var bi = BreakIterator.CreateWordInstance(locale))
			{
				bi.SetText(text);

				CollectionAssert.AreEqual(expected, bi.Boundaries);

				// Verify each boundary and rule status.
				for (int i = 0; i < expected.Length; i++)
				{
					int current = bi.Current;
					int status = bi.GetRuleStatus();
					int[] ruleStatusVector = bi.GetRuleStatusVector();

					Assert.AreEqual(expected[i], current);
					Assert.AreEqual((int)ruleStatus[i], status);
					// There should only be one rule that parsed these.
					Assert.AreEqual(1, ruleStatusVector.Length);
					Assert.AreEqual((int)ruleStatus[i], ruleStatusVector[0]);

					int moveNext = bi.MoveNext();
					int next = i + 1;

					if (next < expected.Length)
					{
						Assert.AreEqual(expected[next], moveNext);
					}
					else
					{
						// Verify that the BreakIterator is exhausted because we've
						// moved past every item.
						Assert.AreEqual(BreakIterator.DONE, moveNext);
					}
				}

				// Verify that the BreakIterator is exhausted because we've
				// moved past every item. It should return the last offset found.
				int lastIndex = expected.Length - 1;
				Assert.AreEqual(expected[lastIndex], bi.Current);

				// We've moved past the last word, it should return the last offset.
				Assert.AreEqual(BreakIterator.DONE, bi.MoveNext());
				Assert.AreEqual(expected[lastIndex], bi.Current);

				// Verify that the first element is correct now that we've moved to the end.
				Assert.AreEqual(expected[0], bi.MoveFirst());
				Assert.AreEqual(expected[0], bi.Current);
			}
		}

		[Test]
		public void CanIterateBackwards()
		{
			var locale = new Locale("de-DE");
			var text = "Good-day, kind sir !";
			var expected = new int[] { 0, 5, 10, 15, 20 };
			// RuleStatus only applies to BreakIterator.UBreakIteratorType.WORD.
			var expectedStatusRuleVector = new[] { 0 };
			var expectedEmptyRuleStatusVector = new[] { 0 };

			using (var bi = BreakIterator.CreateLineInstance(locale))
			{
				bi.SetText(text);

				CollectionAssert.AreEqual(expected, bi.Boundaries);

				int current = 0;
				var currentBoundary = expected[current];

				Assert.AreEqual(currentBoundary, bi.Current);
				Assert.AreEqual(0, bi.GetRuleStatus());
				CollectionAssert.AreEqual(expectedStatusRuleVector, bi.GetRuleStatusVector());

				// Increment the index and verify that the next Boundary is correct.
				current++;
				currentBoundary = expected[current];
				Assert.AreEqual(currentBoundary, bi.MoveNext());
				Assert.AreEqual(currentBoundary, bi.Current);
				Assert.AreEqual(0, bi.GetRuleStatus());
				CollectionAssert.AreEqual(expectedStatusRuleVector, bi.GetRuleStatusVector());

				current++;
				currentBoundary = expected[current];
				Assert.AreEqual(currentBoundary, bi.MoveNext());
				Assert.AreEqual(currentBoundary, bi.Current);
				Assert.AreEqual(0, bi.GetRuleStatus());
				CollectionAssert.AreEqual(expectedStatusRuleVector, bi.GetRuleStatusVector());

				current--;
				currentBoundary = expected[current];
				Assert.AreEqual(currentBoundary, bi.MovePrevious());
				Assert.AreEqual(currentBoundary, bi.Current);
				CollectionAssert.AreEqual(expectedStatusRuleVector, bi.GetRuleStatusVector());

				current--;
				currentBoundary = expected[current];
				Assert.AreEqual(currentBoundary, bi.MovePrevious());
				Assert.AreEqual(currentBoundary, bi.Current);
				Assert.AreEqual(0, bi.GetRuleStatus());
				CollectionAssert.AreEqual(expectedStatusRuleVector, bi.GetRuleStatusVector());

				// We've moved past the first word, it should return 0.
				Assert.AreEqual(BreakIterator.DONE, bi.MovePrevious());
				Assert.AreEqual(0, bi.Current);
				Assert.AreEqual(0, bi.GetRuleStatus());
				CollectionAssert.AreEqual(expectedEmptyRuleStatusVector, bi.GetRuleStatusVector());

				// Verify that the element is correct now that we've moved to the end.
				var last = expected.Last();
				Assert.AreEqual(last, bi.MoveLast());
				Assert.AreEqual(last, bi.Current);
				Assert.AreEqual(0, bi.GetRuleStatus());
				CollectionAssert.AreEqual(expectedStatusRuleVector, bi.GetRuleStatusVector());
			}
		}

		[Test]
		public void CanSetNewText()
		{
			var locale = new Locale("en-US");
			var text = "Good-day, kind sir !  Can I have a glass of water?  I am very parched.";
			var expected = new[] { 0, 22, 52, 70 };
			// RuleStatus only applies to BreakIterator.UBreakIteratorType.WORD.
			var expectedRuleStatusVector = new[] { 0 };

			var secondText = "It is my birthday!  I hope something exciting happens.";
			var secondExpected = new[] { 0, 20, 54 };

			using (var bi = BreakIterator.CreateSentenceInstance(locale))
			{
				bi.SetText(text);

				Assert.AreEqual(text, bi.Text);
				CollectionAssert.AreEqual(expected, bi.Boundaries);

				// Move the iterator to the next boundary
				Assert.AreEqual(expected[1], bi.MoveNext());
				Assert.AreEqual(expected[1], bi.Current);
				Assert.AreEqual(0, bi.GetRuleStatus());
				CollectionAssert.AreEqual(expectedRuleStatusVector, bi.GetRuleStatusVector());

				// Assert that the new set of boundaries were found.
				bi.SetText(secondText);
				Assert.AreEqual(secondText, bi.Text);

				// Assert that the iterator was reset back to the first element
				// when we set new text.
				Assert.AreEqual(secondExpected[0], bi.Current);
				Assert.AreEqual(0, bi.GetRuleStatus());
				CollectionAssert.AreEqual(expectedRuleStatusVector, bi.GetRuleStatusVector());

				CollectionAssert.AreEqual(secondExpected, bi.Boundaries);
			}
		}

		/// <summary>
		/// Assert that when we set the text to empty that it will reset all the values.
		/// </summary>
		[Test]
		public void CanSetNewText_Empty()
		{
			var locale = new Locale("en-US");
			var text = "Good-day, kind sir !  Can I have a glass of water?  I am very parched.";
			string secondText = string.Empty;
			var expected = new[] { 0, 22, 52, 70 };
			// RuleStatus only applies to BreakIterator.UBreakIteratorType.WORD.
			var expectedRuleStatusVector = new[] { 0 };

			using (var bi = BreakIterator.CreateSentenceInstance(locale))
			{
				bi.SetText(text);

				Assert.AreEqual(text, bi.Text);
				CollectionAssert.AreEqual(expected, bi.Boundaries);

				// Move the iterator to the next boundary
				Assert.AreEqual(expected[1], bi.MoveNext());
				Assert.AreEqual(expected[1], bi.Current);
				Assert.AreEqual(0, bi.GetRuleStatus());
				CollectionAssert.AreEqual(expectedRuleStatusVector, bi.GetRuleStatusVector());

				// Assert that the new set of boundaries were found.
				bi.SetText(secondText);
				Assert.AreEqual(secondText, bi.Text);

				// Assert that the iterator was reset back to the first element
				// and is now null.
				Assert.AreEqual(0, bi.Current);
				Assert.AreEqual(BreakIterator.DONE, bi.MoveNext());
				Assert.AreEqual(0, bi.MoveFirst());
				Assert.AreEqual(0, bi.MoveLast());
				Assert.AreEqual(BreakIterator.DONE, bi.MovePrevious());
				Assert.AreEqual(0, bi.GetRuleStatus());
				Assert.AreEqual(new[] { 0 }, bi.GetRuleStatusVector());

				CollectionAssert.IsEmpty(bi.Boundaries);
			}
		}

		/// <summary>
		/// Assert that when we set the text to null, an ArgumentNullException is thrown.
		/// </summary>
		[Test]
		public void CanSetNewText_Null()
		{
			var locale = new Locale("en-US");
			var text = "Good-day, kind sir !  Can I have a glass of water?  I am very parched.";
			string secondText = null;

			using (var bi = BreakIterator.CreateCharacterInstance(locale))
			{
				bi.SetText(text);

				Assert.Throws<ArgumentNullException>(() => bi.SetText(secondText));
			}
		}

		[Test]
		public void CreateSentenceInstanceTest()
		{
			var locale = new Locale("de-DE");
			var text = "Good-bye, dear! That was a delicious dinner.";
			var expected = new[] { 0, 16, 44 };

			using (var bi = BreakIterator.CreateSentenceInstance(locale))
			{
				bi.SetText(text);

				Assert.AreEqual(locale, bi.Locale);
				Assert.AreEqual(text, bi.Text);
				CollectionAssert.AreEqual(expected, bi.Boundaries);
			}
		}

		[Test]
		public void CreateWordInstanceTest()
		{
			var locale = new Locale("de-DE");
			var text = "Good-day, kind sir !";
			var expected = new int[] { 0, 4, 5, 8, 9, 10, 14, 15, 18, 19, 20 };

			using (var bi = BreakIterator.CreateWordInstance(locale))
			{
				bi.SetText(text);

				Assert.AreEqual(locale, bi.Locale);
				Assert.AreEqual(text, bi.Text);
				CollectionAssert.AreEqual(expected, bi.Boundaries);
			}
		}

		[Test]
		public void CreateLineInstanceTest()
		{
			var locale = new Locale("de-DE");
			var text = "Good-day, kind sir !";
			var expected = new[] { 0, 5, 10, 15, 20 };

			using (var bi = BreakIterator.CreateLineInstance(locale))
			{
				bi.SetText(text);

				Assert.AreEqual(locale, bi.Locale);
				Assert.AreEqual(text, bi.Text);
				CollectionAssert.AreEqual(expected, bi.Boundaries);
			}
		}

		[Test]
		public void IsBoundaryTest_Empty()
		{
			string text = string.Empty;
			var offsetsToTest = new[] { 0, -1, 100 };
			var locale = new Locale("de-DE");

			using (var bi = BreakIterator.CreateWordInstance(locale))
			{
				bi.SetText(text);

				for (int i = 0; i < offsetsToTest.Length; i++)
				{
					var isBoundary = bi.IsBoundary(offsetsToTest[i]);
					Assert.IsFalse(isBoundary);
					Assert.AreEqual(0, bi.Current);
				}
			}
		}

		[Test]
		[TestCase(-1, false, 0)]
		[TestCase(21, false, 20)]
		[TestCase(11, false, 14)]
		[TestCase(5, true, 5)]
		[TestCase(0, true, 0)]
		[TestCase(20, true, 20)]
		public void IsBoundaryTest(int offset, bool expectedIsBoundary, int expectedOffset)
		{
			var locale = new Locale("de-DE");
			var text = "Good-day, kind sir !";

			using (var bi = BreakIterator.CreateWordInstance(locale))
			{
				bi.SetText(text);

				var isBoundary = bi.IsBoundary(offset);

				Assert.AreEqual(expectedIsBoundary, isBoundary);
				Assert.AreEqual(expectedOffset, bi.Current);
			}
		}

		[Test]
		[TestCase(-10, 0, 0)] // Offset < 0 returns the first offset.
		[TestCase(0, 22, 22)] // Offset equals to the first offset should give the 2nd offset.
		[TestCase(75, BreakIterator.DONE, 70)]
		[TestCase(70, BreakIterator.DONE, 70)] // Expect that if we give it an exact offset, it will return us one previous.
		[TestCase(30, 52, 52)]
		[TestCase(52, 70, 70)]
		public void MoveFollowingTest(int offset, int expectedOffset, int expectedCurrent)
		{
			var locale = new Locale("de-DE");
			var text = "Good-day, kind sir !  Can I have a glass of water?  I am very parched.";

			using (var bi = BreakIterator.CreateSentenceInstance(locale))
			{
				bi.SetText(text);

				int actualOffset = bi.MoveFollowing(offset);

				Assert.AreEqual(expectedOffset, actualOffset);
				Assert.AreEqual(expectedCurrent, bi.Current);
			}
		}

		[Test]
		[TestCase(-10, 0, 0)]
		[TestCase(0, BreakIterator.DONE, 0)]
		[TestCase(10, BreakIterator.DONE, 0)]
		public void MoveFollowingTest_Empty(int offset, int expectedOffset, int expectedCurrent)
		{
			var locale = new Locale("de-DE");

			using (var bi = BreakIterator.CreateSentenceInstance(locale))
			{
				bi.SetText(string.Empty);

				int actualOffset = bi.MoveFollowing(offset);

				Assert.AreEqual(expectedOffset, actualOffset);
				Assert.AreEqual(expectedCurrent, bi.Current);
			}
		}

		[Test]
		[TestCase(-1, 0, 0)] // Offset < 0 returns the first offset.
		[TestCase(0, BreakIterator.DONE, 0)]
		[TestCase(25, 20, 20)] // Offset > length of text should return last offset.
		[TestCase(20, 19, 19)] // Expect that if we give it an exact offset, it will return us one previous.
		[TestCase(7, 5, 5)]
		[TestCase(14, 10, 10)]
		public void MovePrecedingTest(int offset, int expectedOffset, int expectedCurrent)
		{
			var text = "Good-day, kind sir !";
			var locale = new Locale("de-DE");

			using (var bi = BreakIterator.CreateWordInstance(locale))
			{
				bi.SetText(text);

				int actualOffset = bi.MovePreceding(offset);

				Assert.AreEqual(expectedOffset, actualOffset);
				Assert.AreEqual(expectedCurrent, bi.Current);
			}
		}

		[Test]
		[TestCase(0, BreakIterator.DONE, 0)]
		[TestCase(-5, 0, 0)]
		[TestCase(10, 0, 0)]
		public void MovePrecedingTest_Empty(int offset, int expectedOffset, int expectedCurrent)
		{
			var locale = new Locale("de-DE");

			using (var bi = BreakIterator.CreateWordInstance(locale))
			{
				bi.SetText(string.Empty);

				int actualOffset = bi.MovePreceding(offset);

				Assert.AreEqual(expectedOffset, actualOffset);
				Assert.AreEqual(expectedCurrent, bi.Current);
			}
		}

		/// <summary>
		/// Test data for GetBoundaries_Word and GetWordBoundaries  tests
		/// </summary>
		internal static class WordBoundaryTestData
		{
			public const string Text = "Aa bb. Ccdef 3.5 x? Y?x! Z";

			public static readonly Boundary[] ExpectedOnlyWords = new[] {
				new Boundary(0, 2), new Boundary(3, 5), new Boundary(7, 12), new Boundary(13, 16),
				new Boundary(17, 18), new Boundary(20, 21), new Boundary(22, 23), new Boundary(25, 26)
			};

			public static readonly Boundary[] ExpectedAllBoundaries = new[] {
				new Boundary(0, 2), new Boundary(2, 3), new Boundary(3, 5), new Boundary(5, 6),
				new Boundary(6, 7), new Boundary(7, 12), new Boundary(12, 13), new Boundary(13, 16),
				new Boundary(16, 17), new Boundary(17, 18), new Boundary(18, 19), new Boundary(19, 20),
				new Boundary(20, 21), new Boundary(21, 22), new Boundary(22, 23), new Boundary(23, 24),
				new Boundary(24, 25), new Boundary(25, 26)
			};
		}
	}
}
