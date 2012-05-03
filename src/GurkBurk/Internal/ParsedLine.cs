namespace GurkBurk.Internal
{
    public class ParsedLine
    {
        public ParsedLine(string text, string lineEnd, int line)
        {
            Text = text;
            Line = line;
            LineEnd = lineEnd;
        }

        public int Line { get; private set; }
        public string Text { get; private set; }
        public string LineEnd { get; private set; }

        public bool Equals(ParsedLine other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Line == Line && Equals(other.Text, Text);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ParsedLine)) return false;
            return Equals((ParsedLine) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Line * 397) ^ Text.GetHashCode();
            }
        }

        public static bool operator ==(ParsedLine left, ParsedLine right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ParsedLine left, ParsedLine right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return string.Format("{1}:{0}", Text, Line);
        }
    }
}