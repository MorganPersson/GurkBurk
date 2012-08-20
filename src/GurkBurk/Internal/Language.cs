using System;
using System.Collections.Generic;
using System.Linq;
using GurkBurk.Yml;

namespace GurkBurk.Internal
{
    public class Language
    {
        private YmlEntry languages;
        public string[] Feature { get; private set; }
        public string[] Background { get; private set; }
        public string[] Scenario { get; private set; }
        public string[] ScenarioOutline { get; private set; }
        public string[] Examples { get; private set; }
        public string[] Steps { get; private set; }

        public event LanguageChanged LanguageChanged;

        public Language()
            : this("en")
        {
        }

        public Language(string language)
        {
            LoadLanguages();
            UseLanguage(language);
        }

        private void LoadLanguages()
        {
            using (var r = GetType().Assembly.GetManifestResourceStream(GetType().Assembly.GetName().Name + ".i18n.yml"))
            {
                var ymlParser = new YmlParser();
                languages = ymlParser.Parse(r);
            }
        }

        public bool HasLanguage(string language)
        {
            return languages.Values.Any(_ => _.Key == language);
        }

        public void UseLanguage(string language)
        {
            var lang = languages[language];
            Feature = lang["feature"].Values.Select(_ => _.Key).ToArray();
            Background = lang["background"].Values.Select(_ => _.Key).ToArray();
            Scenario = lang["scenario"].Values.Select(_ => _.Key).ToArray();
            ScenarioOutline = lang["scenario_outline"].Values.Select(_ => _.Key).ToArray();
            Examples = lang["examples"].Values.Select(_ => _.Key).ToArray();
            var steps = new List<string>();
            steps.AddRange(lang["given"].Values.Select(_ => _.Key));
            steps.AddRange(lang["when"].Values.Select(_ => _.Key));
            steps.AddRange(lang["then"].Values.Select(_ => _.Key));
            steps.AddRange(lang["and"].Values.Select(_ => _.Key));
            steps.AddRange(lang["but"].Values.Select(_ => _.Key));
            Steps = steps.Select(_ => _.Trim(new[] {'"'})).Where(_ => _ != "*").ToArray();

            if (LanguageChanged != null)
                LanguageChanged.Invoke(this, new EventArgs());
        }
    }

    public delegate void LanguageChanged(object sender, EventArgs args);
}