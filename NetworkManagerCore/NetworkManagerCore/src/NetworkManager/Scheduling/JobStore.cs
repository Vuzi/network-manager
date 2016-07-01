using System.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using NetworkManager.DomainContent;

namespace NetworkManager.Scheduling {
    public class JobStore {
        private SQLiteConnection conn;

        public JobStore(SQLiteConnection conn) {
            this.conn = conn;

            conn.CreateTable<Job>();
            conn.CreateTable<JobTask>();
            conn.CreateTable<ComputerInJob>();

            conn.CreateTable<JobReport>();
            conn.CreateTable<JobTaskReport>();
        }

        /// <summary>
        /// Insert a new job. The IDs will be created
        /// </summary>
        /// <param name="job">The job to be inserted</param>
        public void insertJob(Job job) {
            // Create IDs
            job.id = Guid.NewGuid().ToString();

            int i = 0;
            foreach (var jtask in job.tasks) {
                jtask.id = Guid.NewGuid().ToString();
                jtask.jobId = job.id;
                jtask.order = i++;
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

        private Job fillJob(Job j) {
            if (j == null)
                return null;
            
            j.tasks = conn.Table<JobTask>().Where(jtask => jtask.jobId == j.id).OrderBy(jtask => jtask.order).ToList();
            j.computers = new List<ComputerInfo>();

            foreach (var relation in conn.Table<ComputerInJob>().Where(relation => relation.jobId == j.id).ToList()) {
                var computer = conn.Find<ComputerInfo>(relation.computerName);
                if (computer != null)
                    j.computers.Add(computer);
            }

            j.reports = getJobReport(j);

            return j;
        }

        /// <summary>
        /// Return a job by its ID
        /// </summary>
        /// <param name="id">The job ID</param>
        /// <returns></returns>
        public Job getJobById(string id) {
            return fillJob(conn.Find<Job>(id));
        }

        public List<Job> getJobByStatus(JobStatus status) {
            var jobs = conn.Table<Job>().Where(j => j.status == status).ToList();

            foreach (Job j in jobs)
                fillJob(j);

            return jobs;
        }

        /// <summary>
        /// Return the list of job report of a job
        /// </summary>
        /// <param name="job">The job</param>
        /// <returns>The found report, or null</returns>
        public List<JobReport> getJobReport(Job job) {
            List <JobReport> jobReports = conn.Table<JobReport>().Where(jr => jr.jobId == job.id).ToList();

            foreach (JobReport jobReport in jobReports) {
                jobReport.tasksReports = conn.Table<JobTaskReport>().Where(jtask => jtask.jobReportId == jobReport.id).OrderBy(jtask => jtask.order).ToList();
                for (int i = 0; i < jobReport.tasksReports.Count && i < job.tasks.Count; i++) {
                    jobReport.tasksReports[i].task = job.tasks[i]; // Orders should matche
                }
            }

            return jobReports;
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

        /// <summary>
        /// Update the specified job
        /// </summary>
        /// <param name="job"></param>
        public void updateJob(Job job) {
            conn.Update(job);
        }
        
        /// <summary>
        /// Insert a job report to a defined job. Job report IDs will be defined
        /// </summary>
        /// <param name="job">The job report's job</param>
        /// <param name="jobReport">The job report to insert</param>
        public void insertJobReport(Job job, JobReport jobReport) {
            // Create IDs
            jobReport.id = Guid.NewGuid().ToString();
            jobReport.jobId = job.id;

            int i = 0;
            foreach (var jtask in jobReport.tasksReports) {
                jtask.id = Guid.NewGuid().ToString();
                jtask.jobReportId = jobReport.id;
                jtask.order = i++;
            }

            // Insert
            conn.Insert(jobReport);
            foreach(var taskReport in jobReport.tasksReports)
                conn.Insert(taskReport);
        }
    }
}
