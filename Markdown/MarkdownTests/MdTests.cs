﻿using System.Diagnostics;
using System.Text;
using FluentAssertions;
using Markdown;
using NUnit.Framework;

namespace MarkdownTests;

[TestFixture]
public class MdTests
{
	[SetUp]
	public void SetUp()
	{
		markdown = new Md();
	}
	
	private Md markdown;

	[TestCase("__Bold__", "<strong>Bold</strong>", TestName = "Bold tag")]
	[TestCase("_Italic_", "<em>Italic</em>", TestName = "Italic tag")]
	[TestCase("# Header", "<h1>Header</h1>", TestName = "Top level header tag")]
	[TestCase("[aaa](abcd)", "<a href=\"abcd\">aaa</a>", TestName = "Link tag")]
	[TestCase("aaa __Bold__ aaa", "aaa <strong>Bold</strong> aaa", TestName = "Bold tag in text")]
	[TestCase("aaa _Italic_ aaa", "aaa <em>Italic</em> aaa", TestName = "Italic tag in text")]
	[TestCase("# Header aaa", "<h1>Header aaa</h1>", TestName = "Top level header tag with spaces")]
	[TestCase("aa [aa](abcd) bb", "aa <a href=\"abcd\">aa</a> bb", TestName = "Link tag in text")]
	[TestCase("[](abcd)", "<a href=\"abcd\"></a>", TestName = "Empty link description")]
	[TestCase("[aaa]()", "<a href=\"\">aaa</a>", TestName = "Empty link")]
	[TestCase(
		"# Header aaa\r\n\r\n__Second _para_graph__",
		"<h1>Header aaa</h1>\r\n\r\n<strong>Second <em>para</em>graph</strong>",
		TestName = "Two paragraphs")]
	public void Render_ShouldReturnHTMLText_WithCorrectTags(string mdText, string expected)
	{
		var result = markdown.Render(mdText);

		result.Should().Be(expected);
	}

	[TestCase("\\__Bold\\__", "__Bold__", TestName = "Bold tag")]
	[TestCase("\\_Italic\\_", "_Italic_", TestName = "Italic tag")]
	[TestCase("\\# Header", "# Header", TestName = "Top level header tag")]
	[TestCase("\\(Link\\)", "(Link)", TestName = "Link tag")]
	[TestCase("\\_", "_", TestName = "only one italic tag")]
	[TestCase("\\__", "__", TestName = "only one bold tag")]
	[TestCase("\\# ", "# ", TestName = "only one header tag")]
	public void Render_ShouldNotRenderEscapedTags_WithCorrectTags(string mdText, string expected)
	{
		var result = markdown.Render(mdText);

		result.Should().Be(expected);
	}


	[TestCase(
		"aa aaa _ aa_ aaa",
		"aa aaa _ aa_ aaa",
		TestName = "Space after opening em tag")]
	[TestCase(
		"aa aaa _aa _ aaa",
		"aa aaa _aa _ aaa",
		TestName = "Space before closing em tag")]
	[TestCase(
		"aa aaa __ aa__ aaa",
		"aa aaa __ aa__ aaa",
		TestName = "Space after opening strong tag")]
	[TestCase(
		"aa aaa __aa __ aaa",
		"aa aaa __aa __ aaa",
		TestName = "Space before closing strong tag")]
	[TestCase(
		"aa_aa aa_aa bb c__ccc cc c__c cc",
		"aa_aa aa_aa bb c__ccc cc c__c cc",
		TestName = "Tags beginning or ending in different words")]
	[TestCase(
		"__aaa _aaa__ a_ aaaaaa",
		"__aaa _aaa__ a_ aaaaaa",
		TestName = "Strong intersect with italic")]
	[TestCase(
		"__aaa [a](aaa__a) aaa__aaa",
		"__aaa <a href=\"aaa__a\">a</a> aaa__aaa",
		TestName = "Strong intersect with link")]
	[TestCase(
		"__",
		"__",
		TestName = "Em tag without value")]
	[TestCase(
		"____",
		"____",
		TestName = "Strong tag without value")]
	[TestCase(
		"[]",
		"[]",
		TestName = "Link tag without value")]
	[TestCase(
		"abcd # abcd",
		"abcd # abcd",
		TestName = "Header not at the beginning")]
	[TestCase(
		"__Unclosed_ (tags",
		"__Unclosed_ (tags",
		TestName = "Unclosed tags")]
	[TestCase("[aa aa](abcd)", "[aa aa](abcd)", TestName = "Space in link description")]
	[TestCase("[aaaa](ab cd)", "[aaaa](ab cd)", TestName = "Space in link")]
	[TestCase("[aaaa] (abcd)", "[aaaa] (abcd)", TestName = "Space between description and link")]
	public void Render_ShouldNotRender_WithIncorrectTags(string mdText, string expected)
	{
		var result = markdown.Render(mdText);

		result.Should().Be(expected);
	}

	[TestCase("_abc_def", "<em>abc</em>def")]
	[TestCase("a_bcde_f", "a<em>bcde</em>f")]
	[TestCase("abc_def_", "abc<em>def</em>")]
	[TestCase("__abc__def", "<strong>abc</strong>def")]
	[TestCase("a__bcde__f", "a<strong>bcde</strong>f")]
	[TestCase("abc__def__", "abc<strong>def</strong>")]
	public void Render_ShouldRender_WithTagsInPartOfWord(string mdText, string expected)
	{
		var result = markdown.Render(mdText);

		result.Should().Be(expected);
	}

	[TestCase(" _1abc1_ ", " _1abc1_ ")]
	[TestCase(" _a1b1c_ ", " _a1b1c_ ")]
	[TestCase(" 1_abc_ ", " 1_abc_ ")]
	[TestCase(" _abc_1 ", " _abc_1 ")]
	public void Render_ShouldNotRender_WithDigitsInsideTags(string mdText, string expected)
	{
		var result = markdown.Render(mdText);

		result.Should().Be(expected);
	}

	[TestCase(
		"a __aaa a _aa_ a aaaaa__",
		"a <strong>aaa a <em>aa</em> a aaaaa</strong>",
		TestName = "Italic can be nesting in bold")]
	[TestCase(
		"# aaaa __aaa _a_ a__ aaa",
		"<h1>aaaa <strong>aaa <em>a</em> a</strong> aaa</h1>",
		TestName = "Header can contain nesting tags")]
	[TestCase(
		"__aa _a_ _a_ aa__",
		"<strong>aa <em>a</em> <em>a</em> aa</strong>",
		TestName = "Multiple nesting")]
	public void Render_ShouldRenderTags_WithCorrectNestingTags(string mdText, string expected)
	{
		var result = markdown.Render(mdText);

		result.Should().Be(expected);
	}

	[TestCase(
		"aa _bbb c __dddd__ e_ ff",
		"aa <em>bbb c __dddd__ e</em> ff",
		TestName = "Bold can't be nesting in italic")]
	[TestCase(
		"# First header # second header",
		"<h1>First header # second header</h1>",
		TestName = "Header in header")]
	[TestCase(
		"a [a](__aaa__aa_aa_aaaaaa)",
		"a <a href=\"__aaa__aa_aa_aaaaaa\">a</a>",
		TestName = "Link tag can't contain other tags")]
	public void Render_ShouldNotRenderTags_WithIncorrectNestingTags(string mdText, string expected)
	{
		var result = markdown.Render(mdText);

		result.Should().Be(expected);
	}

	[TestCaseSource(nameof(LongTextSource))]
	public void Render_PerformanceTest(string text)
	{
		var sw = new Stopwatch();

		sw.Start();
		DoSomethingSlow(text);
		sw.Stop();
		var slowMethodTimeElapsed = sw.Elapsed;
		
		sw.Restart();
		var result = markdown.Render(text);
		sw.Stop();
		var mdRendererTimeElapsed = sw.Elapsed;

		mdRendererTimeElapsed.Should().BeLessThan(slowMethodTimeElapsed);
	}

	public static IEnumerable<TestCaseData> LongTextSource()
	{
		var rnd = new Random();
		var textSize = 500_000;
		var result = new StringBuilder(textSize);
		result.Append("# ");
		var counter = 0;
		while (counter < textSize)
		{
			if (rnd.Next(0, 100) == 100)
			{
				result.Append($"{Environment.NewLine}{Environment.NewLine}");
				counter += $"{Environment.NewLine}{Environment.NewLine}".Length;
			}

			result.Append((char)rnd.Next(35, 122));
			counter++;
		}

		yield return new TestCaseData(result.ToString()).SetName($"Text with {textSize} symbols");
	}

	private void DoSomethingSlow(string text)
	{
		var counter = 0;
		//O(n * log(n)^2)
		for (var i = 0; i < text.Length; i++)
		{
			for (var j = 1; j < text.Length; j*=2)
			{
				counter++;
				for (var c = 1; c < text.Length; c *= 2)
				{
					counter++;
				}
			}
			
		}
	}
}