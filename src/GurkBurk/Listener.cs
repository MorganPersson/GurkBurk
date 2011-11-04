using System.Collections.Generic;

namespace GurkBurk
{
    //The interface exposed by Gherkin
    public interface Listener
    {
        void docString(string str, int line);
        void feature(string feature, string title, string narrative, int line);
        void background(string background, string title, string str3, int line);
        void scenario(string scenario, string title, string str3, int line);
        void scenarioOutline(string outline, string title, string str3, int line);
        void examples(string examples, string str2, string str3, int line);
        void step(string step, string stepText, int line);
        void comment(string str, int line);
        void tag(string str, int line);
        void row(List<string> l, int line);
        void eof();
    }
}