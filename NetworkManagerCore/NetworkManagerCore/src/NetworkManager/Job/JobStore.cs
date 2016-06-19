using System.Linq;
using SQLite;
using System;

namespace NetworkManager.Job {
    public class JobStore {
        private SQLiteConnection conn;

        public JobStore(SQLiteConnection conn) {
            this.conn = conn;

            conn.CreateTable<Job>();
            conn.CreateTable<JobTask>();
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
        }

        /// <summary>
        /// Return a job by its ID
        /// </summary>
        /// <param name="id">The job ID</param>
        /// <returns></returns>
        public Job getJobById(string id) {
            Job j = conn.Get<Job>(id);

            if(j != null) {
                j.tasks = conn.Table<JobTask>().Where(jtask => jtask.jobId == j.id).ToList();
            }

            return j;
        }

        /// <summary>
        /// Delete the specified job from the database
        /// </summary>
        /// <param name="job"></param>
        public void deleteJob(Job job) {
            foreach (var jtask in job.tasks)
                conn.Delete(job.tasks);
            conn.Delete(job);
        }


    }
}
