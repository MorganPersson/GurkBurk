using System.Collections.Generic;
using GurkBurk.Internal;
using NUnit.Framework;

namespace GurkBurkSpec
{
    [TestFixture]
    public class LineEnumeratorSpec
    {
        private LineEnumerator lineEnumerator;

        [SetUp]
        public void Initialize()
        {
            lineEnumerator = new LineEnumerator(new[] { new ParsedLine("a", "\n", 2), new ParsedLine("b", "\n", 3), new ParsedLine("c", "\n", 5), });
        }

        [Test]
        public void Should_be_able_to_move_to_next_line()
        {
            lineEnumerator.MoveToNext();
            Assert.AreEqual("a", lineEnumerator.Current.Text);
            lineEnumerator.MoveToNext();
            Assert.AreEqual("b", lineEnumerator.Current.Text);

            var l = new List<string> {"a", "b", "C"};
            var e = l.GetEnumerator();
            var c = e.Current;
            e.MoveNext();
            c = e.Current;
            Assert.AreEqual("a", c);
        }

        [Test]
        public void Should_be_able_to_move_back_to_previous_line()
        {
            lineEnumerator.MoveToNext();
            lineEnumerator.MoveToNext();
            lineEnumerator.MoveToPrevious();
            Assert.AreEqual("a", lineEnumerator.Current.Text);
        }

        [Test]
        public void Should_be_able_to_move_back_and_then_forward_again()
        {
            lineEnumerator.MoveToNext();
            lineEnumerator.MoveToNext();
            lineEnumerator.MoveToPrevious();
            lineEnumerator.MoveToNext();
            Assert.AreEqual("b", lineEnumerator.Current.Text);
            lineEnumerator.MoveToNext();
            Assert.AreEqual("c", lineEnumerator.Current.Text);
        }

        [Test]
        public void Should_return_null_when_moved_pass_end()
        {
            lineEnumerator.MoveToNext();
            lineEnumerator.MoveToNext();
            lineEnumerator.MoveToNext();
            Assert.IsFalse(lineEnumerator.HasMore);
            Assert.AreEqual("c", lineEnumerator.Current.Text);
            lineEnumerator.MoveToNext();
            Assert.IsFalse(lineEnumerator.HasMore);
            Assert.AreEqual("", lineEnumerator.Current.Text);
        }
    }
}