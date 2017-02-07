using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                new SqlColumn("UserID", MySqlDbType.Int32),
                new SqlColumn("Name", MySqlDbType.Text)));

            using (QueryResult result = db.QueryReader("SELECT * FROM Users"))
            {
                while (result.Read())
                {
                    result.Get<string>("UserID");
                }
            }
        }

        public void addSomething(string something)
        {
            try
            {
                db.Query("INSERT INTO Users (Name) VALUES (@0)",
                    something
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public void delSomething(string something)
        {
            try
            {
                db.Query("DELETE FROM Users WHERE Name = @0", something);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

    }
}
