using System.Collections.Generic;
using GurkBurk;

namespace GurkBurkSpec
{
    public class DummyListener : IListener
    {
        public void DocString(string str, int line)
        { }

        public void Feature(string feature, string title, string narrative, int line)
        { }

        public void Background(string background, string title, int line)
        { }

        public void Scenario(string scenario, string title, int line)
        { }

        public void ScenarioOutline(string outline, string title, int line)
        { }

        public void Examples(string examples, string str2, int line)
        { }

        public void Step(string step, string stepText, int line)
        { }

        public void Comment(string str, int line)
        { }

        public void Tag(string str, int line)
        { }

        public void Row(List<string> l, int line)
        { }

        public void Eof()
        { }

        public void Language(string language, int line)
        { }
    }
}