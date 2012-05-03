using GurkBurk;
using GurkBurk.Internal;
using NUnit.Framework;
using Rhino.Mocks;

namespace GurkBurkSpec
{
    [TestFixture]
    public class LineMatcherSpec
    {
        [Test]
        public void Should_not_match_if_token_in_middle_of_string()
        {
            var listener = MockRepository.GenerateMock<Listener>();
            var parsedLine = new ParsedLine("Given R#", "\n", 1);
            var commentLexer = new CommentLexer(null, new LineEnumerator(new[] { parsedLine }), listener, new Language("en"));
            var match = commentLexer.Match(parsedLine);
            Assert.IsNull(match);
        }
    }
}