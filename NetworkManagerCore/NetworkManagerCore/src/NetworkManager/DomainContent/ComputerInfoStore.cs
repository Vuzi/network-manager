using System;
using System.Data.SQLite;

namespace NetworkManager.DomainContent {
    public class ComputerInfoStore {
        private SQLiteConnection conn;

        public ComputerInfoStore(SQLiteConnection conn) {
            this.conn = conn;
            string sql = @"CREATE TABLE IF NOT EXISTS `computerInfo` (
	                           `name`	TEXT NOT NULL,
	                           `ipAddress`	TEXT NOT NULL,
	                           `macAddress`	TEXT NOT NULL,
	                           `date`	DATETIME NOT NULL,
	                           PRIMARY KEY(name)
                           );";

            SQLiteCommand command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public int insertComputerInfo(ComputerInfo computerInfo) {
            string sql = @"INSERT INTO computerInfo (name, ipAddress, macAddress, date)
                           VALUES (?, ?, ?, ?)";

            var command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter() { Value = computerInfo.name });
            command.Parameters.Add(new SQLiteParameter() { Value = computerInfo.ipAddress });
            command.Parameters.Add(new SQLiteParameter() { Value = computerInfo.macAddress });
            command.Parameters.Add(new SQLiteParameter() { Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });

            return command.ExecuteNonQuery();
        }
        
        public ComputerInfo getComputerInfoByName(string name) {
            string sql = "SELECT * FROM computerInfo WHERE computerInfo.name = ?";

            var command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter() { Value = name });
            
            var reader = command.ExecuteReader();
            if (reader.Read())
                return new ComputerInfo() {
                    name = (string)reader["name"],
                    ipAddress = (string)reader["ipAddress"],
                    macAddress = (string)reader["macAddress"]
                };

            return null;
        }


        public void updateComputerInfo(ComputerInfo computerInfo) {
            string sql = @"UPDATE computerInfo
                           SET ipAddress = ?, macAddress = ?, date = ?
                           WHERE computerInfo.name = ?";

            var command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter() { Value = computerInfo.ipAddress });
            command.Parameters.Add(new SQLiteParameter() { Value = computerInfo.macAddress });
            command.Parameters.Add(new SQLiteParameter() { Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
            command.Parameters.Add(new SQLiteParameter() { Value = computerInfo.name });

            command.ExecuteNonQuery();
        }

        public void updateOrInsertComputerInfo(ComputerInfo computerInfo) {
            string sql = @"INSERT OR REPLACE INTO computerInfo (name, ipAddress, macAddress, date) 
                              VALUES (?, ?, ?, ?);";

            var command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter() { Value = computerInfo.name });
            command.Parameters.Add(new SQLiteParameter() { Value = computerInfo.ipAddress });
            command.Parameters.Add(new SQLiteParameter() { Value = computerInfo.macAddress });
            command.Parameters.Add(new SQLiteParameter() { Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });

            command.ExecuteNonQuery();
        }
    }
}
