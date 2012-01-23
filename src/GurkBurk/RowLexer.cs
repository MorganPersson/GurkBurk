using System.Collections.Generic;
using System.Linq;

namespace GurkBurk
{
    public class RowLexer : Lexer
    {
        private readonly Listener listener;

        public RowLexer(Lexer parent, LineEnumerator lineEnumerator, Listener listener, Language language)
            : base(parent, lineEnumerator, language)
        {
            this.listener = listener;
        }

        public override IEnumerable<string> TokenWords { get { return new[] { @"|" }; } }
        protected override IEnumerable<Lexer> Children { get { return new Lexer[0]; } }
        protected override bool CanSpanMultipleLines { get { return false; } }
        public override bool MustHaveSpaceOrKolonAfterToken { get { return false; } }

        protected override void HandleToken(LineMatch match)
        {
            var cols = match.ParsedLine.Text.Split(new[] { '|' });
            var l = cols.Skip(1).Take(cols.Length - 2).Select(column => column.Trim()).ToList();
            listener.row(l, match.Line);
        }
    }
}