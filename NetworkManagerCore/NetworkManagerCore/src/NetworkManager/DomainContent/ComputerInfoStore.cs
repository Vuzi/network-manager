using System;
using SQLite;

namespace NetworkManager.DomainContent {
    public class ComputerInfoStore {
        private SQLiteConnection conn;

        public ComputerInfoStore(SQLiteConnection conn) {
            this.conn = conn;
            conn.CreateTable<ComputerInfo>();
        }

        public void insertComputerInfo(ComputerInfo computerInfo) {
            conn.Insert(computerInfo);
        }
        
        public ComputerInfo getComputerInfoByName(string name) {
            return conn.Find<ComputerInfo>(name);
        }

        public void updateComputerInfo(ComputerInfo computerInfo) {
            conn.Update(computerInfo);
        }

        public void updateOrInsertComputerInfo(ComputerInfo computerInfo) {
            conn.InsertOrReplace(computerInfo);
        }
    }
}
