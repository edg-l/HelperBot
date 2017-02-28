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
using TShockAPI.Hooks;
using System.Timers;

namespace HelperBot
{
    [ApiVersion(2, 0)]
    public class HelperBot : TerrariaPlugin
    {
        #region Info
        public override string Name { get { return "HelperBot"; } }
        public override string Author { get { return "Ryozuki"; } }
        public override string Description { get { return "A bot with multiple utilities, such as stats gathering and custom commands made in js."; } }
        public override Version Version { get { return new Version(1, 0, 0); } }
        #endregion

        public static IDbConnection Db { get; private set; }
        public static DBManager DbManager { get; private set; }
        public ConfigFile Config = new ConfigFile();
        public Timer sTimer = new Timer();
        public static List<PlayerInfo> players = new List<PlayerInfo>();

        public HelperBot(Main game) : base(game)
        {
            /*
             * TODO:
             * - Add more features
             * 
             * Feature ideas:
             * - Games?
             */
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
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
            ServerApi.Hooks.ServerChat.Register(this, onChat, -666);
            ServerApi.Hooks.NetGetData.Register(this, onGetData);
            TShockAPI.Hooks.PlayerHooks.PlayerPostLogin += PlayerHooks_PlayerPostLogin;
            TShockAPI.Hooks.PlayerHooks.PlayerLogout += PlayerHooks_PlayerLogout;

            sTimer.Elapsed += Second_Elapsed;
            sTimer.Interval = 1000;
            sTimer.Enabled = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnPostInitialize);
                ServerApi.Hooks.ServerChat.Deregister(this, onChat);
                ServerApi.Hooks.NetGetData.Deregister(this, onGetData);
                TShockAPI.Hooks.PlayerHooks.PlayerPostLogin -= PlayerHooks_PlayerPostLogin;
                TShockAPI.Hooks.PlayerHooks.PlayerLogout -= PlayerHooks_PlayerLogout;
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

            Db = new SqliteConnection("uri=file://" + Path.Combine(TShock.SavePath, "HelperBot.sqlite") + ",Version=3");
        }

        private void OnPostInitialize(EventArgs args)
        {
            DbManager = new DBManager(Db);
        }

        private void onGetData(GetDataEventArgs e)
        {
            if (e.Handled)
                return;

            TSPlayer sender = TShock.Players[e.Msg.whoAmI];

            if (sender == null)
                return;

            switch (e.MsgID)
            {
                case PacketTypes.NpcStrike:
                    {
                        using (var reader = new BinaryReader(new MemoryStream(e.Msg.readBuffer, e.Index, e.Length)))
                        {
                            short npcid = reader.ReadInt16();
                            short damage = reader.ReadInt16();
                            float knockback = reader.ReadSingle();
                            byte dir = reader.ReadByte();
                            bool crit = reader.ReadBoolean();

                            NPC npc = TShock.Utils.GetNPCById(npcid);

                            if (npc == null)
                                return;

                            var pinfo = players.FirstOrDefault(x => x.Player.User.ID == sender.User.ID);

                            if (pinfo == null)
                                return;

                            if ((npc.life - damage) < 0)
                            {
                                pinfo.Kills += 1;
                                return;
                            }
                        }
                        break;
                    }
                case PacketTypes.PlayerDeathV2:
                    {
                        using (var reader = new BinaryReader(new MemoryStream(e.Msg.readBuffer, e.Index, e.Length)))
                        {
                            
                            byte victimid = reader.ReadByte();

                            Terraria.DataStructures.PlayerDeathReason reason = Terraria.DataStructures.PlayerDeathReason.FromReader(reader);

                            TSPlayer victim = TShock.Players[victimid];
                            
                            var pinfo = players.FirstOrDefault(x => x.Player.Name == victim.Name);

                            if (pinfo == null)
                                return;

                            pinfo.Deaths += 1;

                            if(reason.SourcePlayerIndex != -1)
                            {
                                TSPlayer killer = TShock.Players[reason.SourcePlayerIndex];
                                var pkinfo = players.FirstOrDefault(x => x.Player.Name == killer.Name);

                                if (pkinfo == null)
                                    return;

                                pkinfo.Kills += 1;
                            }
                        }
                        break;
                    }
            }
        }

        private void onChat(ServerChatEventArgs e)
        {
            string msg = e.Text;

            var ply = TShock.Players[e.Who];

            if (ply == null)
                return;

            msg = msg.ToLower();

            if (Config.QuestionsAndAnswers.Any(x => msg.Contains(x.Question.ToLower()) == true))
            {
                var answer = Config.QuestionsAndAnswers.Find(x => msg.Contains(x.Question.ToLower()) == true).Answer;

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
                catch(System.Data.EvaluateException)
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

        private void PlayerHooks_PlayerPostLogin(PlayerPostLoginEventArgs e)
        {
            TSPlayer ply = e.Player;

            if (ply == null)
                return;

            DbManager.LoadPlayerInfo(ply);
        }

        private void PlayerHooks_PlayerLogout(PlayerLogoutEventArgs e)
        {
            TSPlayer ply = e.Player;

            if (ply == null)
                return;

            if (players.Count == 0)
                return;

            PlayerInfo pinfo = players.FirstOrDefault(x => x.Player.Name == ply.Name);

            if (pinfo == null)
                return;

            DbManager.Save(pinfo);
            players.RemoveAll(x => x.Player.Name == ply.Name);
        }
        #endregion

        #region Commands
        private void CommandHandler(CommandArgs e)
        {
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
                case "stats":
                    {
                        if(args.Length == 0)
                        {
                            ply.SendMessage(String.Format("[PM] {0}: Usage: {1}{2} stats <name>", 
                                Config.BotName,
                                TShock.Config.CommandSpecifier,
                                Config.BaseCommandName),
                                r,g,b);
                        }
                        else
                        {
                            var name = args[0];

                            PlayerInfo pinf = players.ToList().FirstOrDefault(x => x.Player.Name == name);

                            if(pinf == null)
                            {
                                pinf = DbManager.GetPlayerInfo(args[0]);

                                if(pinf == null)
                                {
                                    ply.SendMessage(String.Format("[PM] {0}: Player not found.", Config.BotName), r, g, b);
                                    return;
                                }
                            }
                            pinf.PlayTime.Update();
                            ply.SendMessage(String.Format("[PM] {0}: {1}'s Stats:", Config.BotName, args[0]), r, g, b);
                            ply.SendMessage(String.Format("[PM] {0}: Kills: {1}", Config.BotName, pinf.Kills), r, g, b);
                            ply.SendMessage(String.Format("[PM] {0}: Deaths: {1}", Config.BotName, pinf.Deaths), r, g, b);
                            ply.SendMessage(String.Format("[PM] {0}: PlayTime: {1}:{2}:{3}",
                                Config.BotName,
                                pinf.PlayTime.Hours,
                                pinf.PlayTime.Minutes,
                                pinf.PlayTime.Seconds),
                                r, g, b);
                        }

                        break;
                    }
                default:
                    {
                        ply.SendMessage(String.Format("[PM] {0}: Usage: {1}{2} <command>",
                                Config.BotName,
                                TShock.Config.CommandSpecifier,
                                Config.BaseCommandName),
                                r, g, b);
                        ply.SendMessage(String.Format("[PM] {0}: Commands: stats",
                                Config.BotName),
                                r, g, b);
                        break;
                    }
            }
        }
        #endregion

        private void Second_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach(var pinfo in players)
            {
                pinfo.PlayTime.RawSeconds += 1;
            }
        }
    }
}
