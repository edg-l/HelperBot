using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TShockAPI;

namespace HelperBot
{
    public class ConfigFile
    {
        public string BotName = "HelperBot";
        public string BaseCommandName = "hb";
        public string BotColor = "69,201,210";

        public class QA
        {
            public string Question { get; set; }
            public string Answer { get; set; }
        }

        public List<QA> QuestionsAndAnswers = new List<QA>();

        public static ConfigFile Read(string path)
        {
            if (!File.Exists(path))
            {
                ConfigFile config = new ConfigFile();

                config.QuestionsAndAnswers.Add(new QA()
                {
                    Question = "How do i go to the spawn?",
                    Answer = string.Format("Use {0}spawn!", TShock.Config.CommandSpecifier)
                });

                config.QuestionsAndAnswers.Add(new QA()
                {
                    Question = "Who made the HelperBot?",
                    Answer = "Ryozuki made it, you can find his web here: teeland.ovh"
                });

                File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
                return config;
            }
            return JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(path));
        }
    }
}
