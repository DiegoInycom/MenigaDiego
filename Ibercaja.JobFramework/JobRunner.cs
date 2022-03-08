using System;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Meniga.Runtime.Cluster;
using Meniga.Runtime.Data.App;
using Meniga.Runtime.Job;
using Quartz;
using Job = Meniga.Runtime.Data.App.Job;
using JobGroup = Meniga.Runtime.Data.App.JobGroup;

namespace IberCaja.JobFramework
{
    public class JobRunner : IJob
    {
        private readonly IJobManager<Job, JobType> _jobManager;
        private readonly IJobGroupManager<JobGroup> _jobGroupManager;
        private readonly IClusterManager _clusterManager;

        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public JobRunner(IJobManager<Job, JobType> jobManager, IJobGroupManager<JobGroup> jobGroupManager, IClusterManager clusterManager)
        {
            _jobManager = jobManager;
            _jobGroupManager = jobGroupManager;
            _clusterManager = clusterManager;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.Info("Configuring Jobs to run...");

            var currentNodeId = _clusterManager.ThisNode.Id;

            var jobGroup = _jobGroupManager.Create(1);
            var userImportJobName = typeof(UserImportJob).FullName;
            var userImportJobType = _jobManager.GetJobType(userImportJobName);

            var userImportJob = new Meniga.Runtime.Job.Job
            {
                JobType = userImportJobType.Id,
                Status = (int)JobStatusEnum.New,
                CreationDate = DateTime.Now,
                CreatedByNodeId = currentNodeId,
                JobGroupId = jobGroup.Id,
                Parameters = string.Empty,
                Identifier = string.Format("{0}-{1}", userImportJobName, DateTime.UtcNow.Ticks)
            };

            _jobManager.CreateJob(userImportJob);

            return Task.CompletedTask;
        }
    }
}