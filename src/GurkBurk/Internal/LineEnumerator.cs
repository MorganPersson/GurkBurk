using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GurkBurk.Internal
{
    public class LineEnumerator
    {
        private readonly ParsedLine[] lines;
        private int index = -1;

        public bool HasMore
        {
            get { return index < lines.Length - 1; }
        }

        public ParsedLine Current
        {
            get { return (index == -1 || index == lines.Length) ? new ParsedLine("", -1) : lines[index]; }
        }

        public LineEnumerator(TextReader reader)
        {
            var lines = new List<ParsedLine>();
            string text = "";
            int line = 1;
            while (text != null)
            {
                text = reader.ReadLine();
                if (text != null)
                    lines.Add(new ParsedLine(text, line));
                line++;
            }
            this.lines = lines.ToArray();
        }

        public LineEnumerator(IEnumerable<ParsedLine> lines)
        {
            this.lines = lines.ToArray();
        }

        public void MoveToNext()
        {
            if (index < lines.Length)
                index++;
        }

        public void MoveToPrevious()
        {
            if (index > 0)
                index--;
        }
    }
}