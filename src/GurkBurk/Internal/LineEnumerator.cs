using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            get { return (index == -1 || index == lines.Length) ? new ParsedLine("", "", -1) : lines[index]; }
        }

        public LineEnumerator(TextReader reader)
        {
            var lines = new List<ParsedLine>();
            var whiteSpace = new[] { '\n', ' ', '\r', '\t' };
            var content = reader.ReadToEnd().TrimEnd(whiteSpace);
            int line = 1;
            while (content.Length > 0)
            {
                string text = ReadLine(content);
                content = content.Remove(0, text.Length);
                var trimmedText = text.TrimEnd(new[] { '\n', '\r' });
                var lineEnd = text.Substring(trimmedText.Length);
                if (trimmedText != string.Empty)
                    lines.Add(new ParsedLine(trimmedText, lineEnd, line));
                line++;
            }
            this.lines = lines.ToArray();
        }

        private string ReadLine(string content)
        {
            var idx = content.IndexOf('\n') + 1;
            if (idx == 0)
                return content;
            if (idx < content.Length && content[idx] == '\r')
                idx++;
            return content.Substring(0, idx);
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