using GurkBurk.Internal;
using NUnit.Framework;

namespace GurkBurkSpec
{
    [TestFixture]
    public class LanguageSpec
    {
        [Test]
        public void Should_default_language_to_english()
        {
            var l = new Language();
            CollectionAssert.AreEqual(new[] {"Feature"}, l.Feature);
        }

        [Test]
        public void Should_have_multiple_words_for_same_type()
        {
            var l = new Language();
            CollectionAssert.AreEqual(new[] {"Scenario Outline", "Scenario Template"}, l.ScenarioOutline);
        }

        [Test]
        public void Should_put_all_steps_in_same_collection()
        {
            var l = new Language();
            CollectionAssert.AreEqual(new[] {"Given", "When", "Then", "And", "But"}, l.Steps);
        }

        [Test]
        public void Should_be_able_to_select_language()
        {
            var l = new Language();
            l.UseLanguage("sv");
            CollectionAssert.AreEqual(new[] {"Egenskap"}, l.Feature);
        }
    }
}