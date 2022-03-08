using System.Reflection;
using log4net;
using Meniga.Runtime.Component;
using Meniga.Runtime.Data.App;
using Meniga.Runtime.Job;
using Meniga.ServiceContract;
using Microsoft.Practices.Unity;
using Quartz;
using Job = Meniga.Runtime.Data.App.Job;

namespace IberCaja.JobFramework
{
    public class JobFrameworkComponent : IMenigaComponent
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        public JobFrameworkComponent()
        {

        }

        public void RegisterBindings(IUnityContainer container)
        {
            //TODO: Fix type registration
            container.RegisterType<IMenigaJob<Job, JobType>, UserImportJob>(typeof(UserImportJob).FullName);


            //_logger.Debug("Setting up job triggers...");
            //var algo = JobBuilder.Create<JobRunner>().WithIdentity("ICJ", "Juan").Build();

            //var otro = TriggerBuilder.Create()
            //    .WithIdentity("ICJ", "Juan")
            //    .StartAt(DateBuilder.FutureDate(45, IntervalUnit.Second))
            //    .WithSimpleSchedule(x => x.RepeatForever().WithIntervalInSeconds(60)) // 86400 Once every day
            //    .Build();
            //var sch = container.Resolve<ISchedulerFactory>().GetScheduler();
            //sch.ScheduleJob(algo, otro);


            //var jobRunner = JobBuilder.Create<JobRunner>().WithIdentity("IberCajaJobs", "IberCajaJobGroup").Build();
            //var jobRunnerTrigger = TriggerBuilder.Create()
            //    .WithIdentity("IberCajaJobTrigger", "IberCajaJobGroup")
            //    .StartAt(DateBuilder.FutureDate(45, IntervalUnit.Second))
            //    .WithSimpleSchedule(x => x.RepeatForever().WithIntervalInSeconds(60)) // 86400 Once every day
            //    .Build();

            //var scheduler = container.Resolve<ISchedulerFactory>().GetScheduler();
            //scheduler.ScheduleJob(jobRunner, jobRunnerTrigger);
        }

        public void RegisterEventListeners(IUnityContainer container, Meniga.Runtime.Events.IEventBus eventBus)
        {
        }

        public void Start(IUnityContainer container)
        {

        }

        public void Stop(IUnityContainer container)
        {

        }
    }
}
