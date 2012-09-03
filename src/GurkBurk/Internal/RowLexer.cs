using System.Collections.Generic;
using System.Linq;

namespace GurkBurk.Internal
{
    public class RowLexer : Lexer
    {
        private readonly IListener listener;

        public RowLexer(Lexer parent, LineEnumerator lineEnumerator, IListener listener, Language language)
            : base(parent, lineEnumerator, language)
        {
            this.listener = listener;
        }

        public override IEnumerable<string> TokenWords
        {
            get { return new[] {@"|"}; }
        }

        protected override IEnumerable<Lexer> Children
        {
            get { return new Lexer[0]; }
        }

        protected override bool CanSpanMultipleLines
        {
            get { return false; }
        }

        public override bool MustHaveSpaceOrKolonAfterToken
        {
            get { return false; }
        }

        protected override void HandleToken(LineMatch match)
        {
            var cols = match.ParsedLine.Text.Split(new[] {'|'});
            var l = cols.Skip(1).Take(cols.Length - 2).Select(column => column.Trim()).ToList();
            listener.Row(l, match.Line);
        }
    }
}