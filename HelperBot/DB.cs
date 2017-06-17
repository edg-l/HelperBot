using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI.DB;
using MySql.Data.MySqlClient;
using System.Data;
using TShockAPI;
using Mono.Data.Sqlite;
using System.IO;

namespace HelperBot
{
    public class DB
    {
        private static IDbConnection db;

        public static void Connect()
        {
            switch (TShock.Config.StorageType.ToLower())
            {
                case "mysql":
                    string[] dbHost = TShock.Config.MySqlHost.Split(':');
                    db = new MySqlConnection()
                    {
                        ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
                            dbHost[0],
                            dbHost.Length == 1 ? "3306" : dbHost[1],
                            TShock.Config.MySqlDbName,
                            TShock.Config.MySqlUsername,
                            TShock.Config.MySqlPassword)

                    };
                    break;

                case "sqlite":
                    string sql = Path.Combine(TShock.SavePath, "HelperBot.sqlite");
                    db = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
                    break;

            }

            SqlTableCreator sqlcreator = new SqlTableCreator(db,
                db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());

            sqlcreator.EnsureTableStructure(new SqlTable("HelperBot",
                new SqlColumn("ID", MySqlDbType.Int32) { Primary = true, Unique = true, Length = 7, AutoIncrement = true },
                new SqlColumn("UserID", MySqlDbType.Int32) { Length = 6 },
                new SqlColumn("Skillz", MySqlDbType.Int32) { Length = 6, DefaultValue = "0" }));
        }

        // Your methods here:

        public static void addSomething(int something)
        {
            try
            {
                db.Query("INSERT INTO HelperBot (Name) VALUES (@0)",
                    something
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public static void delSomething(int something)
        {
            try
            {
                db.Query("DELETE FROM HelperBot WHERE UserID = @0", something);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

    }
}
