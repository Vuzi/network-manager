using System.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using NetworkManager.DomainContent;

namespace NetworkManager.Job {
    public class JobStore {
        private SQLiteConnection conn;

        public JobStore(SQLiteConnection conn) {
            this.conn = conn;

            conn.CreateTable<Job>();
            conn.CreateTable<JobTask>();
            conn.CreateTable<ComputerInJob>();
        }

        /// <summary>
        /// Insert a new job. The IDs will be created
        /// </summary>
        /// <param name="job">The job to be inserted</param>
        public void insertJob(Job job) {
            // Create IDs
            job.id = Guid.NewGuid().ToString();
            foreach (var jtask in job.tasks) {
                jtask.id = Guid.NewGuid().ToString();
                jtask.jobId = job.id;
            }

            // Insert
            conn.Insert(job);
            conn.InsertAll(job.tasks);

            // Insert relations with computers
            var relations = new List<ComputerInJob>();
            foreach (var c in job.computers) {
                relations.Add(new ComputerInJob() { jobId = job.id, computerName = c.name });
            }

            conn.InsertAll(relations);
        }

        /// <summary>
        /// Return a job by its ID
        /// </summary>
        /// <param name="id">The job ID</param>
        /// <returns></returns>
        public Job getJobById(string id) {
            Job j = conn.Find<Job>(id);

            if(j != null) {
                j.tasks = conn.Table<JobTask>().Where(jtask => jtask.jobId == j.id).ToList();
                j.computers = new List<ComputerInfo>();

                foreach(var relation in conn.Table<ComputerInJob>().Where(relation => relation.jobId == j.id).ToList()) {
                    var computer = conn.Find<ComputerInfo>(relation.computerName);
                    if (computer != null)
                        j.computers.Add(computer);
                }
            }

            return j;
        }

        /// <summary>
        /// Delete the specified job from the database
        /// </summary>
        /// <param name="job"></param>
        public void deleteJob(Job job) {
            conn.Execute("DELETE FROM JobTask WHERE JobTask.JobId = ?", job.id);
            conn.Execute("DELETE FROM ComputerInJob WHERE ComputerInJob.JobId = ?", job.id);
            conn.Delete(job);
        }


    }
}
