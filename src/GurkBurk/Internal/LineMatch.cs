namespace GurkBurk.Internal
{
    public class LineMatch
    {
        public LineMatch(string token, string text, ParsedLine parsedLine, Lexer lexer)
        {
            Token = token;
            Text = text;
            ParsedLine = parsedLine;
            Lexer = lexer;
        }

        public ParsedLine ParsedLine { get; private set; }
        public Lexer Lexer { get; private set; }

        public int Line
        {
            get { return ParsedLine.Line; }
        }

        public string Text { get; set; }
        public string Token { get; private set; }
    }
}