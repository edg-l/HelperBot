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

namespace HelperBot
{
    [ApiVersion(2, 1)]
    public class HelperBot : TerrariaPlugin
    {
        #region Plugin Info
        // What does Name => string ? -> https://goo.gl/o7WAvJ
        public override string Name => "HelperBot";
        public override string Author => "Ryozuki";
        public override string Description => "A bot with multiple utilities, such as stats gathering and a Q&A system.";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        #endregion

        public ConfigFile Config = new ConfigFile();

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

            if (Config.QuestionsAndAnswers.Any(x => Regex.Match(msg, x.Question).Success == true))
            {
                var answer = Config.QuestionsAndAnswers.Find(x => Regex.Match(msg, x.Question).Success == true).Answer;

                var colorarr = Config.BotColor.Split(',');
                try
                {
                    byte r = Byte.Parse(colorarr[0]);
                    byte g = Byte.Parse(colorarr[1]);
                    byte b = Byte.Parse(colorarr[2]);
                    TShockAPI.Utils.Instance.Broadcast(string.Format("{0}: {1}", Config.BotName, answer), r, g, b);
                }
                catch (FormatException exc)
                {
                    TShock.Log.ConsoleError("[Helperbot] Error parsing the BotColor to a byte, are you sure it is in this format \"255,255,255\"?");
                    TShock.Log.ConsoleError(exc.StackTrace);
                }
            }
            else if (msg.Contains("how much is"))
            {
                int index = msg.LastIndexOf('s');
                string op = msg.Substring(index + 1);
                op = op.Replace("?", "");

                try
                {
                    var colorarr = Config.BotColor.Split(',');
                    double result = Convert.ToDouble(new DataTable().Compute(op, null));
                    byte r = Byte.Parse(colorarr[0]);
                    byte g = Byte.Parse(colorarr[1]);
                    byte b = Byte.Parse(colorarr[2]);
                    TShockAPI.Utils.Instance.Broadcast(string.Format("{0}: {1}", Config.BotName,
                        "The result is: " + result.ToString()), r, g, b);
                }
                catch (System.Data.EvaluateException)
                {
                    // invalid operation
                }
                catch (FormatException a)
                {
                    TShock.Log.ConsoleError("[Helperbot] Error parsing the BotColor to a byte, are you sure it is in this format \"255,255,255\"?");
                    TShock.Log.ConsoleError(a.StackTrace);
                }
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
            var colorarr = Config.BotColor.Split(',');
            byte r = Byte.Parse(colorarr[0]);
            byte g = Byte.Parse(colorarr[1]);
            byte b = Byte.Parse(colorarr[2]);

            switch (cmd)
            {
                default:
                    {
                        ply.SendMessage(String.Format("[PM] {0}: Usage: {1}{2} <command>",
                                Config.BotName,
                                TShock.Config.CommandSpecifier,
                                Config.BaseCommandName),
                                r, g, b);
                        ply.SendMessage(String.Format("[PM] {0}: Commands: (no commands, WIP)",
                                Config.BotName),
                                r, g, b);
                        break;
                    }
            }
        }
        #endregion
    }
}
