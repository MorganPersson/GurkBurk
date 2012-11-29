using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GurkBurk;
using GurkBurk.Internal;
using NUnit.Framework;
using Rhino.Mocks;

namespace GurkBurkSpec
{
    [TestFixture]
    public class I18NLexerSpec
    {
        private IListener listener;
        private I18nLexer lexer;

        [SetUp]
        public void Setup()
        {
            listener = MockRepository.GenerateMock<IListener>();
            lexer = new I18nLexer(listener);
        }

        [TearDown]
        public void Cleanup()
        {
            UriFactory.ResetToDefault();
        }
        [Test]
        public void Should_parse_feature_title()
        {
            const string words = "Feature: this is the title";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "this is the title", "", 1));
        }

        [Test]
        public void Should_parse_feature_title_and_narrative()
        {
            const string words = "Feature: title line\nAs a x\nI want y\nSo that z";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "title line", "As a x\nI want y\nSo that z", 1));
        }

        [Test]
        public void Should_parse_scenario_title_over_multiple_lines()
        {
            const string words = "Feature: foo\nScenario: bar\n  baz";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "", 1));
            listener.AssertWasCalled(_ => _.Scenario("Scenario", "bar\n  baz", 2));
        }

        [Test]
        public void Should_parse_scenario_title()
        {
            const string words = "Feature: foo\nScenario: bar";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "", 1));
            listener.AssertWasCalled(_ => _.Scenario("Scenario", "bar", 2));
        }

        [Test]
        public void Should_parse_scenario_without_feature()
        {
            const string words = "Scenario: bar";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Scenario("Scenario", "bar", 1));
        }

        [Test]
        public void Should_parse_feature_with_title_if_no_space_between_kolon_and_title()
        {
            const string words = "Feature:this is the title";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "this is the title", "", 1));
        }

        [Test]
        public void Should_parse_feature_with_title_with_only_space_to_title()
        {
            const string words = "Feature this is the title";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "this is the title", "", 1));
        }

        [Test]
        public void Should_parse_scenario_with_title_if_no_space_between_kolon_and_title()
        {
            const string words = "Feature:foo\nScenario:bar";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "", 1));
            listener.AssertWasCalled(_ => _.Scenario("Scenario", "bar", 2));
        }

        [Test]
        public void Should_parse_scenario_with_title_with_only_space_to_title()
        {
            const string words = "Feature this is the title\nScenario title";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "this is the title", "", 1));
            listener.AssertWasCalled(_ => _.Scenario("Scenario", "title", 2));
        }

        [Test]
        public void Should_parse_feature_title_with_many_spaces_in_it()
        {
            const string words = "Feature: long   space";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "long   space", "", 1));
        }

        [Test]
        public void Should_parse_background()
        {
            const string words = "Feature: foo\nBackground: bar\nGiven background step\nScenario:the scenario\nGiven scenario step";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "", 1));
            listener.AssertWasCalled(_ => _.Background("Background", "bar", 2));
            listener.AssertWasCalled(_ => _.Step("Given", "background step", 3));
            listener.AssertWasCalled(_ => _.Scenario("Scenario", "the scenario", 4));
            listener.AssertWasCalled(_ => _.Step("Given", "scenario step", 5));
        }

        [Test]
        public void Should_parse_Examples()
        {
            const string words = "Feature: foo\nScenario: bar\nGiven a\nExamples:\n  | x | y | z |\n |a| b| c |";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Examples("Examples", "", 4));
            var args = listener.GetArgumentsForCallsMadeOn(_ => _.Row(null, 0));
            listener.AssertWasCalled(_ => _.Row(null, 0), opt => opt.IgnoreArguments().Repeat.Twice());
            //first row
            CollectionAssert.AreEqual(new List<string> { "x", "y", "z" }, (List<string>)args[0][0]);
            Assert.AreEqual(5, args[0][1]);
            // 2nd row
            CollectionAssert.AreEqual(new List<string> { "a", "b", "c" }, (List<string>)args[1][0]);
            Assert.AreEqual(6, args[1][1]);
        }

        [Test]
        public void Should_parse_Scenario_outline_with_Examples()
        {
            const string words = "Feature: foo\nScenario Outline: bar\nGiven a";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "", 1));
            listener.AssertWasCalled(_ => _.ScenarioOutline("Scenario Outline", "bar", 2));
            listener.AssertWasCalled(_ => _.Step("Given", "a", 3));
        }

        [Test, Ignore("Should this be an error?")]
        public void Should_throw_error_when_examples_has_no_rows()
        {
            const string words = "Feature: foo\nScenario: bar\nGiven a\nExamples:";
            Assert.Throws<LexerError>(() => lexer.scan(words));
        }

        [Test]
        public void Should_be_able_to_load_table_data_from_external_uri()
        {
            const string words = "Feature: foo\nScenario: bar\nGiven a\nExamples:\n  file:///z:/foo/bar.txt";
            UriFactory.fileReader = f =>
                                        {
                                            var ms = new MemoryStream();
                                            const string content = "|x|y|z|\n|a|b|c|";
                                            var fw = new StreamWriter(ms);
                                            fw.Write(content);
                                            fw.Flush();
                                            ms.Seek(0, 0);
                                            return new StreamReader(ms);
                                        };

            lexer.scan(words);

            listener.AssertWasCalled(_ => _.Examples("Examples", "", 4));
            var args = listener.GetArgumentsForCallsMadeOn(_ => _.Row(null, 0));
            listener.AssertWasCalled(_ => _.Row(null, 0), opt => opt.IgnoreArguments().Repeat.Twice());
            //first row
            CollectionAssert.AreEqual(new List<string> { "x", "y", "z" }, (List<string>)args[0][0]);
            Assert.AreEqual(5, args[0][1]);
            // 2nd row
            CollectionAssert.AreEqual(new List<string> { "a", "b", "c" }, (List<string>)args[1][0]);
            Assert.AreEqual(5, args[1][1]);
        }

        [TestCase("Given")]
        [TestCase("When")]
        [TestCase("Then")]
        [TestCase("And")]
        [TestCase("But")]
        public void Should_parse_step(string step)
        {
            string words = "Feature: foo\nScenario: bar\n  " + step + " a b c";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "", 1));
            listener.AssertWasCalled(_ => _.Scenario("Scenario", "bar", 2));
            listener.AssertWasCalled(_ => _.Step(step, "a b c", 3));
        }

        [TestCase("Given")]
        [TestCase("When")]
        [TestCase("Then")]
        [TestCase("And")]
        [TestCase("But")]
        public void Should_parse_step_with_text_over_multiple_lines(string step)
        {
            string words = "Feature: foo\nScenario: bar\n  " + step + " a\n\t\tb c";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "", 1));
            listener.AssertWasCalled(_ => _.Scenario("Scenario", "bar", 2));
            listener.AssertWasCalled(_ => _.Step(step, "a\n\t\tb c", 3));
        }

        [Test]
        public void Should_parse_Given_When_Then()
        {
            const string words = "Feature: foo\nScenario: bar\nGiven  a\n When b\nbb\nThen c";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "", 1));
            listener.AssertWasCalled(_ => _.Scenario("Scenario", "bar", 2));
            listener.AssertWasCalled(_ => _.Step("Given", "a", 3));
            listener.AssertWasCalled(_ => _.Step("When", "b\nbb", 4));
            listener.AssertWasCalled(_ => _.Step("Then", "c", 6));
        }

        [Test]
        public void Should_parse_step_with_table()
        {
            const string words = "Feature: foo\nScenario: bar\n  Given [a] [b]\n|a|b|\n|1|2|";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Step("Given", "[a] [b]", 3));

            var args = listener.GetArgumentsForCallsMadeOn(_ => _.Row(null, 0));
            //first row
            CollectionAssert.AreEqual(new List<string> { "a", "b" }, (List<string>)args[0][0]);
            Assert.AreEqual(4, args[0][1]);
            // 2nd row
            CollectionAssert.AreEqual(new List<string> { "1", "2" }, (List<string>)args[1][0]);
            Assert.AreEqual(5, args[1][1]);
        }

        [Test]
        public void Should_parse_tag()
        {
            const string words = "\t@tag\nFeature: foo";
            lexer.scan(words);
            listener.AssertWasNotCalled(_ => _.Tag("@", 1));
            listener.AssertWasCalled(_ => _.Tag("@tag", 1));
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "", 2));
        }

        [Test]
        public void Should_parse_multiple_tags_on_same_line()
        {
            const string words = "@tag @tag2\n@tag med space\nFeature: foo";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Tag("@tag", 1));
            listener.AssertWasCalled(_ => _.Tag("@tag2", 1));
            listener.AssertWasCalled(_ => _.Tag("@tag med space", 2));
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "", 3));
        }

        [Test]
        public void Should_parse_tags_on_scenario()
        {
            const string words = "Feature: foo\n@tag1 @tag2\nScenario: xyz";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "", 1));
            listener.AssertWasCalled(_ => _.Tag("@tag1", 2));
            listener.AssertWasCalled(_ => _.Tag("@tag2", 2));
            listener.AssertWasCalled(_ => _.Scenario("Scenario", "xyz", 3));
        }

        [Test]
        public void Should_ignore_empty_lines()
        {
            const string words = "\n\t \nFeature: foo\n\n\nScenario: bar";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "", 3));
            listener.AssertWasCalled(_ => _.Scenario("Scenario", "bar", 6));
        }

        [Test, Ignore(@"Seems the ""official"" Gherkin runner allows scenarios without features")]
        public void scenario_must_be_preceeded_with_a_feature()
        {
            const string words = "Scenario: bar";
            Assert.Throws<LexerError>(() => lexer.scan(words));
        }

        [Test]
        public void background_must_be_preceeded_with_a_feature()
        {
            const string words = "Background: bar";
            Assert.Throws<LexerError>(() => lexer.scan(words));
        }

        [Test]
        public void step_must_be_preceeded_with_scenario_or_it_will_be_interpreted_as_narrative()
        {
            const string words = "Feature: foo\nGiven a\n Scenario: bar";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "Given a", 1));
            listener.AssertWasCalled(_ => _.Scenario("Scenario", "bar", 3));
        }

        [Test]
        public void Should_ignore_lines_that_start_with_a_comment()
        {
            const string words = "  # comment\nFeature: foo\n# comment2";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Comment("comment", 1));
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "", 2));
            listener.AssertWasCalled(_ => _.Comment("comment2", 3));
        }

        [Test]
        public void Should_detect_language_if_first_comment_is_language_comment()
        {
            const string words = "  # language: sv\nEgenskap: foo\nScenario: bar\nGivet a";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Comment("language: sv", 1));
            listener.AssertWasCalled(_ => _.Feature("Egenskap", "foo", "", 2));
            listener.AssertWasCalled(_ => _.Scenario("Scenario", "bar", 3));
            listener.AssertWasCalled(_ => _.Step("Givet", "a", 4));
        }

        [Test]
        public void Should_raise__LexerError_if_invalid_language_detected()
        {
            const string words = "  # language: xx-yy\nEgenskap: foo\nScenario: bar\nGivet a";
            var ex = Assert.Throws<LexerError>(() => lexer.scan(words));
            Assert.AreEqual("Line: 1. Unknown language 'xx-yy'", ex.Message);
        }

        [Test]
        public void Should_handle_docstring_over_single_line()
        {
            const string words = "Feature: foo\nScenario: bar\nGiven  a\n\"\"\"this spans one line\"\"\"\nWhen b";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "", 1));
            listener.AssertWasCalled(_ => _.Scenario("Scenario", "bar", 2));
            listener.AssertWasCalled(_ => _.Step("Given", "a", 3));
            listener.AssertWasCalled(_ => _.DocString("this spans one line", 4));
            listener.AssertWasCalled(_ => _.Step("When", "b", 5));
        }

        [Test]
        public void Should_handle_docstring_over_multiple_lines()
        {
            const string words = "Feature: foo\nScenario: bar\nGiven  a\n   \"\"\"\nthis\n    spans \n    multiple lines\n\"\"\"\nWhen b";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "", 1));
            listener.AssertWasCalled(_ => _.Scenario("Scenario", "bar", 2));
            listener.AssertWasCalled(_ => _.Step("Given", "a", 3));
            listener.AssertWasCalled(_ => _.DocString("this\n spans \n multiple lines", 4));
            listener.AssertWasCalled(_ => _.Step("When", "b", 9));
        }

        [Test]
        public void Should_raise_eof_when_file_is_parsed()
        {
            const string words = "Feature: foo\nScenario: bar\nGiven a\nWhen b\nThen z";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Eof());
        }

        [Test]
        public void Should_trim_whitespace_at_end_of_token()
        {
            const string words = "Feature: foo  \nScenario: bar  \nGiven  a \t  \n\n When b  \n     \nFeature: bar";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "", 1));
            listener.AssertWasCalled(_ => _.Scenario("Scenario", "bar", 2));
            listener.AssertWasCalled(_ => _.Step("Given", "a", 3));
            listener.AssertWasCalled(_ => _.Step("When", "b", 5));
            listener.AssertWasCalled(_ => _.Feature("Feature", "bar", "", 7));
        }

        [Test]
        public void Should_keep_line_feed_and_carriage_return_intact_in_step()
        {
            const string words = "Feature: foo  \nScenario: bar  \nGiven  foo\r\nbar\r\nbaz";
            lexer.scan(words);
            listener.AssertWasCalled(_ => _.Step("Given", "foo\r\nbar\r\nbaz", 3));
        }

        [Test]
        public void Should_throw_LexerError_when_unparsable_row_found()
        {
            const string words = "Feature: foo\n# comment\nthis will throw\nScenario: bar";
            var ex = Assert.Throws<LexerError>(() => lexer.scan(words));
            Assert.AreEqual("Line: 3. Failed to parse 'this will throw'", ex.Message);
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "", 1));
            listener.AssertWasCalled(_ => _.Comment("comment", 2));
            listener.AssertWasNotCalled(_ => _.Scenario("Scenario", "bar", 4));
        }

        [Test]
        public void AcceptanceTest_Feature_and_scenario_with_tags()
        {
            string words = TestData.AcceptanceTest.Replace("\r\n", "\n");
            lexer.scan(words);
            listener.AssertWasNotCalled(_ => _.Tag("@", 2));
            listener.AssertWasCalled(_ => _.Tag("@tag1", 2));
            listener.AssertWasCalled(_ => _.Tag("@tag2", 2));
            listener.AssertWasCalled(_ => _.Feature("Feature", "foo", "\tAs a\n\tI want\n\tSo that", 3));
            listener.AssertWasCalled(_ => _.Tag("@tag3", 8));
            listener.AssertWasCalled(_ => _.Scenario("Scenario", "x", 9));
            listener.AssertWasCalled(_ => _.Step("Given", "a\n\tb\n\tc", 10));
            listener.AssertWasCalled(_ => _.Step("When", "d", 13));
            listener.AssertWasCalled(_ => _.Step("Then", "e", 14));

            var args = listener.GetArgumentsForCallsMadeOn(_ => _.Row(null, 0));
            //first row
            CollectionAssert.AreEqual(new List<string> { "x", "y", "z" }, (List<string>)args[0][0]);
            Assert.AreEqual(15, args[0][1]);
            // 2nd row
            CollectionAssert.AreEqual(new List<string> { "1", "2", "3" }, (List<string>)args[1][0]);
            Assert.AreEqual(16, args[1][1]);
        }

        [Test, Explicit]
        public void PerformanceTest()
        {
            listener = new DummyListener();
            lexer = new I18nLexer(listener);
            //simple perf test, runs around 390ms om my machine.
            const string words = TestData.AcceptanceTest;
            lexer.scan(words);
            var str = "";
            for (int i = 0; i < 1000; i++)
                str += TestData.AcceptanceTest + "\n";
            var t = new Stopwatch();
            t.Start();
            lexer.scan(str);
            t.Stop();
            Assert.That(t.ElapsedMilliseconds, Is.LessThan(0));
        }
    }
}