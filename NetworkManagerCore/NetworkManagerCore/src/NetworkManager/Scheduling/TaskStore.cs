using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;

namespace NetworkManager.Scheduling {

    class TaskStore {
        private SQLiteConnection conn;

        public TaskStore(SQLiteConnection conn) {
            this.conn = conn;
            string sql = @"CREATE TABLE IF NOT EXISTS `task` (
	                           `UUID`	CHARACTER(36) NOT NULL,
	                           `type`	INTEGER NOT NULL,
	                           `date`	DATETIME NOT NULL,
	                           `value`	TEXT,
	                           `extra`	TEXT,
	                           PRIMARY KEY(UUID)
                           );

                           CREATE TABLE IF NOT EXISTS `taskResult` (
	                           `UUID`	      CHARACTER(36) NOT NULL,
	                           `task_UUID`	  CHARACTER(36) NOT NULL,
	                           `status`	      INTEGER NOT NULL,
	                           `computerName` TEXT NOT NULL,
	                           `domainName`	  TEXT NOT NULL,
	                           `start`	      DATETIME,
	                           `end`	      DATETIME,
	                           `returnValue`  INTEGER,
	                           `output`	      TEXT,
	                           `err`	      TEXT,
	                            PRIMARY KEY(UUID)
	                            FOREIGN KEY(task_UUID) REFERENCES task(UUID)
                           );";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public int insertTaskResult(TaskResult taskResult) {
            string sql = @"INSERT INTO taskResult (UUID, task_UUID, status, computerName, domainName, start, end, returnValue, output, err)
                           VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            var command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter() { Value = taskResult.UUID });
            command.Parameters.Add(new SQLiteParameter() { Value = taskResult.task.UUID });
            command.Parameters.Add(new SQLiteParameter() { Value = taskResult.status });
            command.Parameters.Add(new SQLiteParameter() { Value = taskResult.computerName });
            command.Parameters.Add(new SQLiteParameter() { Value = taskResult.domainName });
            command.Parameters.Add(new SQLiteParameter() { Value = taskResult.start.ToString("yyyy-MM-dd HH:mm:ss") });
            command.Parameters.Add(new SQLiteParameter() { Value = taskResult.end.ToString("yyyy-MM-dd HH:mm:ss") });
            command.Parameters.Add(new SQLiteParameter() { Value = taskResult.returnValue });
            command.Parameters.Add(new SQLiteParameter() { Value = taskResult.output });
            command.Parameters.Add(new SQLiteParameter() { Value = taskResult.err });

            return command.ExecuteNonQuery();
        }

        public int deleteTaskResult(TaskResult taskResult) {
            string sql = "DELETE FROM taskResult WHERE taskResult.UUID = ?";

            var command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter() { Value = taskResult.UUID });

            return command.ExecuteNonQuery();
        }

        private TaskResult getTaskResultFromReader(SQLiteDataReader reader, Task task = null) {
            return new TaskResult() {
                UUID = (string) reader["uuid"],
                task = task ?? getTaskByUUID((string) reader["task_UUID"]),
                status = (TaskStatus) (long) reader["status"],
                computerName = (string) reader["computerName"],
                domainName = (string) reader["domainName"],
                start = (DateTime) reader["start"],
                end = (DateTime) reader["end"],
                returnValue = (int) (long) reader["returnValue"],
                output = (string) (reader["output"] != DBNull.Value ? reader["output"] : null),
                err = (string) (reader["err"] != DBNull.Value ? reader["err"] : null),
            };
        }

        public void updateTaskResult(TaskResult taskResult) {
            string sql = @"UPDATE taskResult
                           SET status = ?, start = ?, end = ?,
                               returnValue = ?, output = ?, err = ?
                           WHERE taskResult.UUID = ?";

            var command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter() { Value = (int)taskResult.status });
            command.Parameters.Add(new SQLiteParameter() { Value = taskResult.start.ToString("yyyy-MM-dd HH:mm:ss") });
            command.Parameters.Add(new SQLiteParameter() { Value = taskResult.end.ToString("yyyy-MM-dd HH:mm:ss") });
            command.Parameters.Add(new SQLiteParameter() { Value = taskResult.returnValue });
            command.Parameters.Add(new SQLiteParameter() { Value = taskResult.output });
            command.Parameters.Add(new SQLiteParameter() { Value = taskResult.err });
            command.Parameters.Add(new SQLiteParameter() { Value = taskResult.UUID });

            command.ExecuteNonQuery();
        }

        public TaskResult getTaskResultByComputer(string computerName, string computerDomain) {
            string sql = "SELECT * FROM taskResult WHERE taskResult.computerName = ? AND taskResult.domainName = ?";

            var command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter() { Value = computerName });
            command.Parameters.Add(new SQLiteParameter() { Value = computerDomain });

            var reader = command.ExecuteReader();
            if (reader.Read())
                return getTaskResultFromReader(reader);

            return null; // Not found
        }

        public IEnumerable<TaskResult> getTasksResultByTask(Task task) {
            string sql = "SELECT * FROM taskResult WHERE taskResult.task_UUID = ?";

            var command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter() { Value = task.UUID });

            var results = new List<TaskResult>();

            var reader = command.ExecuteReader();
            while (reader.Read())
                results.Add(getTaskResultFromReader(reader, task));

            return results;
        }

        public int insertTask(Task task) {
            string sql = @"INSERT INTO task (UUID, type, date, value, extra)
                           VALUES (?, ?, ?, ?, ?)";

            var command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter() { Value = task.UUID });
            command.Parameters.Add(new SQLiteParameter() { Value = (int)task.type });
            command.Parameters.Add(new SQLiteParameter() { Value = task.date.ToString("yyyy-MM-dd HH:mm:ss") });
            command.Parameters.Add(new SQLiteParameter() { Value = task.value });
            command.Parameters.Add(new SQLiteParameter() { Value = task.extra });

            return command.ExecuteNonQuery();
        }

        public int updateTask(Task task) {
            string sql = "UPDATE task SET type = ?, date = ?, value = ?, extra = ? WHERE task.UUID = ?";

            var command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter() { Value = (int)task.type });
            command.Parameters.Add(new SQLiteParameter() { Value = task.date.ToString("yyyy-MM-dd HH:mm:ss") });
            command.Parameters.Add(new SQLiteParameter() { Value = task.value });
            command.Parameters.Add(new SQLiteParameter() { Value = task.extra });
            command.Parameters.Add(new SQLiteParameter() { Value = task.UUID });

            return command.ExecuteNonQuery();
        }

        public int deleteTask(Task task) {
            string sql = "DELETE FROM task WHERE task.UUID = ?";

            var command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter() { Value = task.UUID });

            return command.ExecuteNonQuery();
        }

        private Task getTaskFromReader(SQLiteDataReader reader) {
            return new Task() {
                UUID = (string)reader["UUID"],
                type = (TaskType)((long)reader["type"]),
                date = (DateTime)reader["date"],
                value = (string)(reader["value"] != DBNull.Value ? reader["value"] : null),
                extra = (string)(reader["extra"] != DBNull.Value ? reader["extra"] : null),
            };
        }

        public IEnumerable<Task> getTaskToPerforms() {
            string sql = @"SELECT *, COUNT(taskResult.UUID) AS nb
                           FROM task
                           INNER JOIN taskResult ON taskResult.task_UUID = task.UUID AND taskResult.status = 1
                           GROUP BY task.UUID";

            var command = new SQLiteCommand(sql, conn);

            var results = new List<Task>();

            var reader = command.ExecuteReader();
            while (reader.Read())
                results.Add(getTaskFromReader(reader));

            return results;
        }

        public Task getTaskByUUID(string uuid) {
            string sql = "SELECT * FROM task WHERE task.UUID = ?";

            var command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter() { Value = uuid });

            var reader = command.ExecuteReader();
            if (reader.Read())
                return getTaskFromReader(reader);

            return null; // Not found
        }
    }
}
