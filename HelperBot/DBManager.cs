using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using TShockAPI.DB;
using MySql.Data.MySqlClient;
using System.Data;

namespace HelperBot
{
    public class DBManager
    {
        private IDbConnection db;

        public DBManager(IDbConnection db)
        {
            this.db = db;

            var sqlCreator = new SqlTableCreator(db, (IQueryBuilder)new SqliteQueryCreator());
            sqlCreator.EnsureTableStructure(new SqlTable("Users",
                new SqlColumn("ID", MySqlDbType.Int32) { AutoIncrement = true, Primary = true },
                new SqlColumn("Name", MySqlDbType.Text),
                new SqlColumn("Kills", MySqlDbType.Int32),
                new SqlColumn("Deaths", MySqlDbType.Int32),
                new SqlColumn("PlayTime", MySqlDbType.Int32),
                new SqlColumn("Biography", MySqlDbType.Text)
                ));
        }

        public void LoadPlayerInfo(TSPlayer ply)
        {
            if (ply == null)
                return;

            using (QueryResult result = db.QueryReader("SELECT * FROM Users WHERE Name=@0", ply.Name))
            {
                while (result.Read())
                {
                    //TShock.Log.ConsoleInfo("USER FOUND IN DB!");
                    PTime playTime = new PTime();
                    playTime.RawSeconds = result.Get<int>("PlayTime");
                    playTime.Update();
                    HelperBot.players.Add(new PlayerInfo()
                    {
                        Player = ply,
                        Kills = result.Get<int>("Kills"),
                        Deaths = result.Get<int>("Deaths"),
                        PlayTime = playTime,
                        Biography = result.Get<string>("Biography")
                    });
                    return;
                }
            }

            //TShock.Log.ConsoleInfo("USER NOT FOUND IN DB! CREATING IT!");
            PTime ptime = new PTime()
            {
                Hours = 0,
                Minutes = 0,
                Seconds = 0,
                RawSeconds = 0
            };

            PlayerInfo pinf = new PlayerInfo()
            {
                Player = ply,
                Deaths = 0,
                Kills = 0,
                Biography = "",
                PlayTime = ptime
            };
            CreatePlayerInfo(pinf);
            HelperBot.players.Add(pinf);
        }

        public void CreatePlayerInfo(PlayerInfo pinfo)
        {
            try
            {
                db.Query("INSERT INTO Users (Name, Kills, Deaths, PlayTime, Biography) VALUES (@0,@1,@2,@3,@4)",
                    pinfo.Player.Name,
                    pinfo.Kills,
                    pinfo.Deaths,
                    pinfo.PlayTime.RawSeconds,
                    pinfo.Biography
                );
            }
            catch (Exception e)
            {
                TShock.Log.ConsoleError(e.StackTrace);
            }
        }

        public void Save(PlayerInfo pinfo)
        {
            try
            {
                db.Query("UPDATE Users SET Kills=@0, Deaths=@1, PlayTime=@2, Biography=@3 WHERE Name=@4",
                    pinfo.Kills,
                    pinfo.Deaths,
                    pinfo.PlayTime.RawSeconds,
                    pinfo.Biography,
                    pinfo.Player.Name
                );
            }
            catch (Exception e)
            {
                TShock.Log.ConsoleError(e.StackTrace);
            }
        }

        public PlayerInfo GetPlayerInfo(string name)
        {
            using (QueryResult result = db.QueryReader("SELECT * FROM Users WHERE Name=@0", name))
            {
                while (result.Read())
                {
                    PTime playTime = new PTime();
                    playTime.RawSeconds = result.Get<int>("PlayTime");
                    PlayerInfo pinf = new PlayerInfo()
                    {
                        Kills = result.Get<int>("Kills"),
                        Deaths = result.Get<int>("Deaths"),
                        PlayTime = playTime,
                        Biography = result.Get<string>("Biography")
                    };
                    pinf.PlayTime.Update();
                    return pinf;
                }
            }
            return null;
        }
    }
}
