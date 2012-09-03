using System.Collections.Generic;

namespace GurkBurk
{
    //The interface exposed by Gherkin
    public interface IListener
    {
        void DocString(string str, int line);
        void Feature(string feature, string title, string narrative, int line);
        void Background(string background, string title, int line);
        void Scenario(string scenario, string title, int line);
        void ScenarioOutline(string outline, string title, int line);
        void Examples(string examples, string str2, int line);
        void Step(string step, string stepText, int line);
        void Comment(string str, int line);
        void Tag(string str, int line);
        void Row(List<string> l, int line);
        void Eof();
        void Language(string language, int line);
    }
}