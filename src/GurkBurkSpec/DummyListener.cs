using System.Collections.Generic;
using GurkBurk;

namespace GurkBurkSpec
{
    public class DummyListener : Listener
    {
        public void docString(string str, int line)
        { }

        public void feature(string feature, string title, string narrative, int line)
        { }

        public void background(string background, string title, string str3, int line)
        { }

        public void scenario(string scenario, string title, string str3, int line)
        { }

        public void scenarioOutline(string outline, string title, string str3, int line)
        { }

        public void examples(string examples, string str2, string str3, int line)
        { }

        public void step(string step, string stepText, int line)
        { }

        public void comment(string str, int line)
        { }

        public void tag(string str, int line)
        { }

        public void row(List<string> l, int line)
        { }

        public void eof()
        { }
    }
}