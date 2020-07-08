using GurkBurk;
using NUnit.Framework;

namespace GurkBurkSpec
{
    [TestFixture]
    public class UriFactorySpec
    {
        [Test]
        public void Should_ba_able_to_read_from_http()
        {
            var stream = UriFactory.GetStream("https://www.google.com/");
            var content = stream.ReadToEnd();
            Assert.Greater(content.Length, 0);
        }
    }
}
