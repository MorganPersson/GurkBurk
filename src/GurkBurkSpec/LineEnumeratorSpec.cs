using System.Collections.Generic;
using GurkBurk;
using NUnit.Framework;

namespace GurkBurkSpec
{
    [TestFixture]
    public class LineEnumeratorSpec
    {
        private LineEnumerator wordEnumerator;

        [SetUp]
        public void WordEnumerator()
        {
            wordEnumerator = new LineEnumerator(new[] { new ParsedLine("a", 2), new ParsedLine("b", 3), new ParsedLine("c", 5), });
        }

        [Test]
        public void Should_be_able_to_move_to_next_line()
        {
            wordEnumerator.MoveToNext();
            Assert.AreEqual("a", wordEnumerator.Current.Text);
            wordEnumerator.MoveToNext();
            Assert.AreEqual("b", wordEnumerator.Current.Text);

            var l = new List<string> { "a", "b", "C" };
            var e = l.GetEnumerator();
            var c = e.Current;
            e.MoveNext();
            c = e.Current;
            Assert.AreEqual("a", c);
        }

        [Test]
        public void Should_be_able_to_move_back_to_previous_line()
        {
            wordEnumerator.MoveToNext();
            wordEnumerator.MoveToNext();
            wordEnumerator.MoveToPrevious();
            Assert.AreEqual("a", wordEnumerator.Current.Text);
        }

        [Test]
        public void Should_be_able_to_move_back_and_then_forward_again()
        {
            wordEnumerator.MoveToNext();
            wordEnumerator.MoveToNext();
            wordEnumerator.MoveToPrevious();
            wordEnumerator.MoveToNext();
            Assert.AreEqual("b", wordEnumerator.Current.Text);
            wordEnumerator.MoveToNext();
            Assert.AreEqual("c", wordEnumerator.Current.Text);
        }


        [Test]
        public void Should_return_null_when_moved_pass_end()
        {
            wordEnumerator.MoveToNext();
            wordEnumerator.MoveToNext();
            wordEnumerator.MoveToNext();
            Assert.IsFalse(wordEnumerator.HasMore);
            Assert.AreEqual("c", wordEnumerator.Current.Text);
            wordEnumerator.MoveToNext();
            Assert.IsFalse(wordEnumerator.HasMore);
            Assert.AreEqual("", wordEnumerator.Current.Text);
        }
    }
}