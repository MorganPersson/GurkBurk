using System.IO;
using System.Linq;
using GurkBurk.Yml;
using NUnit.Framework;

namespace GurkBurkSpec
{
    [TestFixture]
    public class YmlParserSpec
    {
        private YmlParser _ymlParser;
        private YmlEntry _parsedYmlRoot;

        [SetUp]
        public void SetUp()
        {
            Establish_context();
            Because_of();
        }

        private void Establish_context()
        {
            _ymlParser = new YmlParser();
        }

        private void Because_of()
        {
            var ms = new MemoryStream();
            var sr = new StreamWriter(ms);
            sr.WriteLine("# comment");
            sr.WriteLine("\"se\":");
            sr.WriteLine("# comment");
            sr.WriteLine("  example: Exempel");
            sr.WriteLine("  feature: Story|Krav|Feature");
            sr.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            _parsedYmlRoot = _ymlParser.Parse(ms);
        }

        [Test]
        public void Should_find_categories()
        {
            Assert.That(_parsedYmlRoot.Values.First().Key, Is.EqualTo("se"));
        }

        [Test]
        public void Should_ignore_comments()
        {
            Assert.That(_parsedYmlRoot.Values.Count, Is.EqualTo(1));
        }

        [Test]
        public void Should_find_subcategories()
        {
            Assert.That(_parsedYmlRoot["se"]["example"].Values.First().Key, Is.EqualTo("Exempel"));
        }

        [Test]
        public void Should_have_multiple_values_for_same_key()
        {
            Assert.That(_parsedYmlRoot["se"]["feature"].Values.Count(), Is.EqualTo(3));
            Assert.That(_parsedYmlRoot["se"]["feature"].Values.First().Key, Is.EqualTo("Story"));
            Assert.That(_parsedYmlRoot["se"]["feature"].Values.Skip(1).First().Key, Is.EqualTo("Krav"));
            Assert.That(_parsedYmlRoot["se"]["feature"].Values.Last().Key, Is.EqualTo("Feature"));
        }
    }
}