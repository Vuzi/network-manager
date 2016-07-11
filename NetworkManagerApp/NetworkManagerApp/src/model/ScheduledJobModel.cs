
namespace NetworkManager.Model {
    class ScheduledJobModel {
        public bool selected { get; set; }

        public string name {
            get {
                return job.name != null ? job.name : "<unnamed task>";
            }
        }

        public Scheduling.Job job { get; set; }
    }
}
