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

namespace HelperBot
{
    [ApiVersion(2, 0)]
    public class HelperBot : TerrariaPlugin
    {
        #region Info
        public override string Name { get { return "HelperBot"; } }
        public override string Author { get { return "Ryozuki"; } }
        public override string Description { get { return "Replace me!!!"; } }
        public override Version Version { get { return new Version(1, 0, 0); } }
        #endregion

        public static IDbConnection Db { get; private set; }
        public static DBManager DbManager { get; private set; }
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
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnPostInitialize);
            }
            base.Dispose(disposing);
        }

        #region Hooks
        private void OnInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("HelperBot.help".ToLower(), CHelp, "chelp")
            {
                HelpText = "Usage: /chelp"
            });

            Db = new SqliteConnection("uri=file://" + Path.Combine(TShock.SavePath, "HelperBot.sqlite") + ",Version=3");
        }

        private void OnPostInitialize(EventArgs args)
        {
            DbManager = new DBManager(Db);
        }
        #endregion

        #region Commands
        private void CHelp(CommandArgs args)
        {
            args.Player.SendInfoMessage("Author, please change me.");
        }
        #endregion
    }
}
