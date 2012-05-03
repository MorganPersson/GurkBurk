using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GurkBurk;
using GurkBurk.Internal;
using NUnit.Framework;

namespace GurkBurkSpec
{
    [TestFixture]
    public class LexerErrorSpec
    {
        [Test]
        public void Should_be_able_to_serialize_exception()
        {
            var e = new LexerError(new ParsedLine("Fooo !!", "\n", 1));
            var formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            formatter.Serialize(stream, e);
            stream.Flush();
            Assert.Greater(stream.Length, 0);
        }

        [Test]
        public void Should_be_able_to_Deserialize_exception()
        {
            var e = new LexerError(new ParsedLine("Fooo !!", "\n", 1));
            var formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            formatter.Serialize(stream, e);
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            var obj = formatter.Deserialize(stream);
            Assert.IsNotNull(obj);
            Assert.AreEqual(e.Message, ((LexerError)obj).Message);
        }
    }
}