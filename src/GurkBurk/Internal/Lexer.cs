using System;
using System.Collections.Generic;
using System.Linq;

namespace GurkBurk.Internal
{
    public abstract class Lexer
    {
        protected Language Language { get; private set; }
        private LineMatcher lineMatcher;
        protected LineEnumerator LineEnumerator { get; private set; }

        private readonly char[] whiteSpace = new[] {'\n', ' ', '\t'};

        protected Lexer(Lexer parent, LineEnumerator lineEnumerator, Language language)
        {
            this.parent = parent;
            LineEnumerator = lineEnumerator;
            Language = language;
            //TODO: Should probably unsubscribe to event at some point
            Language.LanguageChanged += ChangeLanguage;
        }

        protected abstract void HandleToken(LineMatch match);

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

        private void Parse(LineMatch lineMatch)
        {
            lineMatch.Lexer.HandleToken(lineMatch);
            LineEnumerator.MoveToNext();
            Parse();
        }

        private LineMatch ReadNextStep()
        {
            var text = (LineEnumerator.Current.Text ?? "").Trim(whiteSpace);
            while (LineEnumerator.HasMore && string.IsNullOrEmpty(text))
            {
                LineEnumerator.MoveToNext();
                text = (LineEnumerator.Current.Text ?? "").Trim(whiteSpace);
            }
            LineMatch lineMatch = Children.Any() ? LineMatcher.Match(LineEnumerator.Current) : null;
            if (lineMatch == null)
                return null;
            lineMatch.Lexer.ReadMultiLineStep(this, lineMatch);

            return lineMatch;
        }

        private void ReadMultiLineStep(Lexer parent, LineMatch lineMatch)
        {
            if (CanSpanMultipleLines
                && NextLineIsStep(parent) == false
                && NextLineIsChildStep(lineMatch) == false)
            {
                string moreTitle = GetStepText();
                lineMatch.Text = (string.IsNullOrEmpty(moreTitle)) ? lineMatch.Text : lineMatch.Text + "\n" + moreTitle;
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
        }

        public abstract IEnumerable<string> TokenWords { get; }
        protected abstract IEnumerable<Lexer> Children { get; }

        protected virtual bool CanSpanMultipleLines
        {
            get { return true; }
        }

        public virtual bool MustHaveSpaceOrKolonAfterToken
        {
            get { return true; }
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
                    stepText += LineEnumerator.Current.Text + "\n";
                else
                    LineEnumerator.MoveToPrevious();
            }

            return stepText.TrimEnd(whiteSpace);
        }

        private LineMatcher matchAllLines;
        private readonly Lexer parent;

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

        protected void UseLanguage(string language)
        {
            Language.UseLanguage(language);
        }
    }
}