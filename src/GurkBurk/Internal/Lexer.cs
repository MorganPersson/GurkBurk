using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GurkBurk.Internal
{
    public abstract class Lexer
    {
        protected Language Language { get; private set; }
        private LineMatcher lineMatcher;
        protected LineEnumerator LineEnumerator { get; private set; }
        private Regex regex;

        protected readonly char[] WhiteSpace = new[] { '\n', ' ', '\r', '\t' };

        protected Lexer(Lexer parent, LineEnumerator lineEnumerator, Language language)
        {
            this.parent = parent;
            LineEnumerator = lineEnumerator;
            Language = language;
            //TODO: Should probably unsubscribe to event at some point
            Language.LanguageChanged += ChangeLanguage;
            SetupRegex();
        }

        public abstract IEnumerable<string> TokenWords { get; }
        protected abstract IEnumerable<Lexer> Children { get; }
        protected abstract void HandleToken(LineMatch match);

        public virtual bool MustHaveSpaceOrKolonAfterToken
        {
            get { return true; }
        }

        public virtual LineMatch Match(ParsedLine line)
        {
            var match = regex.Match(line.Text);
            if (match.Success == false)
                return null;
            var keyword = match.Groups["keyword"].Value.Trim().TrimEnd(new[] { ':' });
            return new LineMatch(keyword, match.Groups["text"].Value.Trim(), line, this);
        }

        public void Parse()
        {
            bool continueToParse = true;
            LineMatch lineMatch;
            do
            {
                lineMatch = ReadNextStep();
                if (lineMatch == null)
                    continue;
                lineMatch.Lexer.Parse(lineMatch);

                continueToParse = ContinueToParse();
            } while (continueToParse && lineMatch != null);
        }

        protected virtual bool CanSpanMultipleLines
        {
            get { return true; }
        }

        private void Parse(LineMatch lineMatch)
        {
            lineMatch.Lexer.HandleToken(lineMatch);
            LineEnumerator.MoveToNext();
            Parse();
        }

        private LineMatch ReadNextStep()
        {
            var text = (LineEnumerator.Current.Text ?? "").Trim(WhiteSpace);
            while (LineEnumerator.HasMore && string.IsNullOrEmpty(text))
            {
                LineEnumerator.MoveToNext();
                text = (LineEnumerator.Current.Text ?? "").Trim(WhiteSpace);
            }
            LineMatch lineMatch = Children.Any() ? LineMatcher.Match(LineEnumerator.Current) : null;
            if (lineMatch == null)
                return null;
            lineMatch.Lexer.ReadMultiLineStep(this, lineMatch);

            return lineMatch;
        }

        private void ReadMultiLineStep(Lexer parentLexer, LineMatch lineMatch)
        {
            if (CanSpanMultipleLines
                && NextLineIsStep(parentLexer) == false
                && NextLineIsChildStep(lineMatch) == false)
            {
                string moreTitle = GetStepText();
                lineMatch.Text = (string.IsNullOrEmpty(moreTitle)) ? lineMatch.Text : lineMatch.Text + lineMatch.ParsedLine.LineEnd + moreTitle;
            }
        }

        private bool NextLineIsChildStep(LineMatch lineMatch)
        {
            return NextLineIsStep(lineMatch.Lexer);
        }

        private bool ContinueToParse()
        {
            return (LineEnumerator.HasMore || (string.IsNullOrEmpty(LineEnumerator.Current.Text) == false));
        }

        private bool NextLineIsStep(Lexer lexer)
        {
            if (LineEnumerator.HasMore == false)
                return false;
            LineEnumerator.MoveToNext();
            var lineMatch = lexer.LineMatcher.Match(LineEnumerator.Current);
            LineEnumerator.MoveToPrevious();
            return (lineMatch != null);
        }

        private void ChangeLanguage(object sender, EventArgs args)
        {
            CreateLineMatcher();
            SetupRegex();
        }

        private LineMatcher LineMatcher
        {
            get
            {
                if (lineMatcher == null)
                    CreateLineMatcher();
                return lineMatcher;
            }
        }

        private void CreateLineMatcher()
        {
            lineMatcher = new LineMatcher(Children);
        }

        private string GetStepText()
        {
            var matcher = BuildMatcherForAllLines();
            string stepText = "";
            LineMatch nextMatch = null;
            while (nextMatch == null && LineEnumerator.HasMore)
            {
                LineEnumerator.MoveToNext();
                nextMatch = matcher.Match(LineEnumerator.Current);
                if (nextMatch == null)
                    stepText += LineEnumerator.Current.Text + LineEnumerator.Current.LineEnd;
                else
                    LineEnumerator.MoveToPrevious();
            }

            return stepText.TrimEnd(WhiteSpace);
        }

        private LineMatcher matchAllLines;
        private readonly Lexer parent;

        //Is this obsolete now?
        private LineMatcher BuildMatcherForAllLines()
        {
            if (matchAllLines == null)
            {
                var root = GetLexerRoot();
                var lexers = new Dictionary<Type, Lexer>();
                BuildMatcherForAllLines(root.Children, lexers);
                matchAllLines = new LineMatcher(lexers.Values);
            }
            return matchAllLines;
        }

        private Lexer GetLexerRoot()
        {
            Lexer root = this;
            while (root.parent != null)
                root = root.parent;
            return root;
        }

        private void BuildMatcherForAllLines(IEnumerable<Lexer> children, Dictionary<Type, Lexer> lexers)
        {
            foreach (var child in children)
            {
                if (lexers.ContainsKey(child.GetType()) == false)
                {
                    lexers.Add(child.GetType(), child);
                    BuildMatcherForAllLines(child.Children, lexers);
                }
            }
        }

        private const string LineMatch = @"^\s*(?<keyword>{0})(?<text>.*)";

        private void SetupRegex()
        {
            var words = TokenWords.Select(t => t.Replace("|", @"\|") + (MustHaveSpaceOrKolonAfterToken ? @"(\s|:)" : "")).ToArray();
            string allWords = "(" + string.Join(")|(", words) + ")";
            regex = new Regex(string.Format(LineMatch, allWords));
        }
    }
}