using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Timers;

namespace HelperBot
{
    [ApiVersion(2, 1)]
    public class HelperBot : TerrariaPlugin
    {
        #region Plugin Info
        public override string Name => "HelperBot";
        public override string Author => "Ryozuki";
        public override string Description => "A bot with multiple utilities, such as stats gathering and a Q&A system.";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        #endregion

        public ConfigFile Config = new ConfigFile();
        public Regex ReminderRegex = new Regex(@"remind me to ([\w+ ]+) in (\d+) (minutes|mins|hours|seconds|secs|days)");

        public HelperBot(Main game) : base(game)
        {
        }

        private void LoadConfig()
        {
            string path = Path.Combine(TShock.SavePath, "HelperBot.json");
            Config = ConfigFile.Read(path);
        }

        public override void Initialize()
        {
            LoadConfig();
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.ServerChat.Register(this, onChat, -666);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
            }
            base.Dispose(disposing);
        }

        #region Hooks
        private void OnInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("helperbot.cmds", CommandHandler, Config.BaseCommandName)
            {
                HelpText = string.Format("Usage: {0}{1} <command>", TShock.Config.CommandSpecifier, Config.BaseCommandName)
            });
        }

        private void onChat(ServerChatEventArgs e)
        {
            string msg = e.Text;

            var ply = TShock.Players[e.Who];

            if (ply == null)
                return;

            msg = msg.ToLower();

            if (Config.QuestionsAndAnswers.Any(x => Regex.Match(msg.ToLower(), x.Question.ToLower()).Success == true))
            {
                var answer = Config.QuestionsAndAnswers.Find(x => Regex.Match(msg.ToLower(), x.Question.ToLower()).Success == true).Answer;

                TShockAPI.Utils.Instance.Broadcast(string.Format("{0}: {1}", Config.BotName, answer), (byte)Config.BotColor[0], (byte)Config.BotColor[1], (byte)Config.BotColor[2]);
            }
            else if (msg.Contains("how much is"))
            {
                int index = msg.LastIndexOf('s');
                string op = msg.Substring(index + 1);
                op = op.Replace("?", "");

                try
                {
                    double result = Convert.ToDouble(new DataTable().Compute(op, null));
                    TShockAPI.Utils.Instance.Broadcast(string.Format("{0}: {1}", Config.BotName,
                        "The result is: " + result.ToString()), (byte)Config.BotColor[0], (byte)Config.BotColor[1], (byte)Config.BotColor[2]);
                }
                catch (System.Data.EvaluateException)
                {
                    // invalid operation
                }
            }
            else if(ReminderRegex.IsMatch(msg) && ply.HasPermission("helperbot.remind"))
            {
                Match m = ReminderRegex.Match(msg);

                string what = m.Groups[1].Value;
                float time = float.Parse(m.Groups[2].Value);
                string time_unit = m.Groups[3].Value;

                if (time_unit == "mins")
                    time_unit = "minutes";
                if (time_unit == "secs")
                    time_unit = "seconds";

                TShockAPI.Utils.Instance.Broadcast(String.Format("{0}: {4} I'll remind you to {1} in {2} {3}",
                    Config.BotName, what, time, time_unit, ply.Name), (byte)Config.BotColor[0], (byte)Config.BotColor[1], (byte)Config.BotColor[2]);

                if (time_unit == "days")
                    time *= 24 * 3600 * 1000;
                if (time_unit == "hours")
                    time *= 3600 * 1000;
                else if (time_unit == "minutes")
                    time *= 60 * 1000;
                else if (time_unit == "seconds")
                    time *= 1000;

                Timer timer = new Timer(time);
                timer.Elapsed += (sender, ee) => TShockAPI.Utils.Instance.Broadcast(String.Format("{0}: {1} remember to {2}",
                    Config.BotName, ply.Name, what), (byte)Config.BotColor[0], (byte)Config.BotColor[1], (byte)Config.BotColor[2]);
                timer.AutoReset = false;
                timer.Start();
            }
        }
        #endregion

        #region Commands
        private void CommandHandler(CommandArgs e)
        {
            if (Config.EnableBotCommands == false)
                return;

            var ply = e.Player;
            var cmd = "";

            if (e.Parameters.Count != 0)
            {
                cmd = e.Parameters[0];
            }

            var args = e.Parameters.Skip(1).ToArray();

            switch (cmd)
            {
                case "reload":
                    {
                        if(ply.HasPermission("helperbot.admin"))
                        {
                            LoadConfig();
                            Commands.ChatCommands.RemoveAll(x => x.Permissions.Contains("helperbot.cmds"));
                            Commands.ChatCommands.Add(new Command("helperbot.cmds", CommandHandler, Config.BaseCommandName)
                            {
                                HelpText = string.Format("Usage: {0}{1} <command>", TShock.Config.CommandSpecifier, Config.BaseCommandName)
                            });
                            ply.SendMessage(String.Format("[PM] {0}: Reload complete",
                                Config.BotName),
                                (byte)Config.BotColor[0], (byte)Config.BotColor[1], (byte)Config.BotColor[2]);
                        }
                        else
                        {
                            ply.SendMessage(String.Format("[PM] {0}: You don't have enough permissions to run this command!",
                                Config.BotName),
                                (byte)Config.BotColor[0], (byte)Config.BotColor[1], (byte)Config.BotColor[2]);
                        }
                        break;
                    }
                default:
                    {
                        ply.SendMessage(String.Format("[PM] {0}: Usage: {1}{2} <command>",
                                Config.BotName,
                                TShock.Config.CommandSpecifier,
                                Config.BaseCommandName),
                                (byte)Config.BotColor[0], (byte)Config.BotColor[1], (byte)Config.BotColor[2]);
                        ply.SendMessage(String.Format("[PM] {0}: Commands: reload",
                                Config.BotName),
                                (byte)Config.BotColor[0], (byte)Config.BotColor[1], (byte)Config.BotColor[2]);
                        break;
                    }
            }
        }
        #endregion
    }
}
