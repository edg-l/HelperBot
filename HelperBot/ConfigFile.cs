using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace HelperBot
{
    public class ConfigFile
    {
        // Config variables here:
        public string PluginName = "HelperBot";
        public string BotName = "HelperBot";
        public string BaseCommandName = "hb";
        public string BotColor = "69,201,210";
        public bool EnableBotCommands = true;

        public class QA
        {
            public string Question { get; set; }
            public string Answer { get; set; }
        }

        public List<QA> QuestionsAndAnswers = new List<QA>();
        // End of config variables

        public static ConfigFile Read(string path)
        {
            if (!File.Exists(path))
            {
                ConfigFile config = new ConfigFile();

                config.QuestionsAndAnswers.Add(new QA()
                {
                    Question = @"(H|h)ow (can i|to) register(\?|)",
                    Answer = string.Format("Use {0}spawn!", TShock.Config.CommandSpecifier)
                });

                config.QuestionsAndAnswers.Add(new QA()
                {
                    Question = @"(H|h)ow (do i go|i go) to the spawn(\?|)",
                    Answer = string.Format("Use {0}spawn!", TShock.Config.CommandSpecifier)
                });

                config.QuestionsAndAnswers.Add(new QA()
                {
                    Question = @"(W|w)ho (made|created|coded) the (H|h)elper(B|b)ot(\?|)",
                    Answer = "Ryozuki made it, you can find his web here: teeland.ovh"
                });

                File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
                return config;
            }
            return JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(path));
        }
    }
}
