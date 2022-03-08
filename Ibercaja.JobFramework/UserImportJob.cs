using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ibercaja.Utils.Config;
using log4net;
using Meniga.Core.Accounts;
using Meniga.Core.Users;
using Meniga.Runtime.Cluster;
using Meniga.Runtime.Job;
using JobLog = Meniga.Runtime.Job.JobLog;
using Job = Meniga.Runtime.Data.App.Job;
using Meniga.Runtime.Data.App;
using Meniga.Core.BusinessModels;
using Meniga.Core.Data;
using Meniga.Core.DataConsolidation;
using Meniga.Runtime.Extensions;

namespace IberCaja.JobFramework
{
    public class UserImportJob : IMenigaJob<Job, JobType>
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IJobManager<Job, JobType> _jobManager;
        private readonly IClusterManager _clusterManager;
        private readonly IAccountSetupCache _accountSetupCache;
        private readonly ICoreContextProvider _dataContextProvider;
        private readonly IDataConsolidationManager _dataConsolidationManager;
        private readonly IUserManager _userManager;
        private readonly IAccountsManager _accountsManager;
        private readonly IIbercajaConfiguration _ibercajaConfiguration;

        private long _jobId;
        private string _realmIdentifier = "Ibercaja";
        private readonly int _maxProcessingThreads;

        private static CreateRealmUserInfo RealmUserInfo = new CreateRealmUserInfo
        {
            Culture = "es-ES",
            IsInitialSetupDone = false
        };

        public UserImportJob(IClusterManager clusterManager, ICoreContextProvider dataContextProvider, IAccountSetupCache accountSetupCache
                            ,IUserManager userManager, IDataConsolidationManager dataConsolidationManager, IAccountsManager accountsManager)
        {
            _clusterManager = clusterManager;
            _dataContextProvider = dataContextProvider;
            _accountSetupCache = accountSetupCache;
            _userManager = userManager;
            _dataConsolidationManager = dataConsolidationManager;
            _accountsManager = accountsManager;

            _ibercajaConfiguration = ConfigurationManager.GetSection("Ibercaja") as IIbercajaConfiguration ?? new IbercajaConfiguration();
            _maxProcessingThreads = _ibercajaConfiguration.JobsMaxProcessingThreads;
        }

        public JobResult Execute(MenigaJobContext job, IJobManager<Job, JobType> jobManager)
        {
            _logger.Debug("Starting JobFramework job");

            _jobManager = jobManager;
            _jobId = job.JobId;
            int currentNodeId = _clusterManager.ThisNode.Id;
            
            Job dbJob = _jobManager.GetJob(job.JobId);
            if (dbJob.Status == (int)JobStatusEnum.Running && dbJob.StartedByNodeId.GetValueOrDefault() != currentNodeId)
            {
                LogEntry(string.Format("Will not run job because it is already in progress at node {0}", dbJob.StartedByNodeId.GetValueOrDefault()));
                return new JobResult { JobStatus = JobStatusEnum.Running };
            }

            if (dbJob.Status == (int)JobStatusEnum.Done)
            {
                LogEntry(string.Format("Will not run job because it is has already been completed at node {0}", dbJob.StartedByNodeId.GetValueOrDefault()));
                return new JobResult { JobStatus = JobStatusEnum.Done };
            }

            try
            {
                _jobManager.ChangeJobStatus(dbJob.Id, JobStatusEnum.Running); // To avoid multiple nodes processing this job

                LogEntry("User processing...");
                var userQueue = new BlockingCollection<IbercajaUserInfo>();
                var tasks = new List<Task>();
                var threadsFinishedCreatingUsers = 0;
                var threadFinishedCreatingUsers = new bool[_maxProcessingThreads];

                for (var i = 0; i < _maxProcessingThreads; i++)
                {
                    var currentThreadNr = i;
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        _logger.DebugFormat("Starting User processing thread nr. {0}.", currentThreadNr);
                        while (!userQueue.IsCompleted)
                        {
                            try
                            {
                                var ibercajaUser = userQueue.Take();
                                ProcessUser(ibercajaUser);
                            }
                            catch (InvalidOperationException)
                            {
                                // This happens when trying to Take() from an empty/completed queue. We will mark this thread as finished and check if others are also finished.
                                if (!threadFinishedCreatingUsers[currentThreadNr])
                                {
                                    Interlocked.Increment(ref threadsFinishedCreatingUsers);
                                    threadFinishedCreatingUsers[currentThreadNr] = true;
                                    _logger.DebugFormat("Thread nr. {0} is finished creating users. Total number of finished threads is now = {1}.", currentThreadNr, threadsFinishedCreatingUsers);
                                }
                                else
                                {
                                    Thread.Sleep(500);
                                    _logger.DebugFormat("Thread nr. {0} is still waiting for other threads to finish. Total number of finished threads is now = {1}.", currentThreadNr, threadsFinishedCreatingUsers);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error("Unexpected Error", ex);
                            }
                        }
                        _logger.DebugFormat("User processing thread nr. {0} is done processing users.", currentThreadNr);
                    }));
                }

                
                string connectionString = ConfigurationManager.ConnectionStrings["MenigaBatchEntities"].ConnectionString;
                string query = "Select * from batch.ibercaja_user_info where last_update is null";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand(query, con))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            IbercajaUserInfo ibercajaUserInfo = new IbercajaUserInfo
                            {
                                UserIdentifier = reader["user_identifier"].ToString(),
                                FirstName = reader["first_name"].ToString(),
                                LastName = reader["last_name"].ToString(),
                                DateOfBirth = Convert.ToDateTime(reader["date_of_Birth"].ToString()),
                                Gender = reader["gender"].ToString(),
                                PostalCode = reader["postal_code"].ToString(),
                                Email = reader["email"].ToString()
                                //IsActive = Convert.ToBoolean(reader["is_Active"].ToString())
                            };

                            userQueue.Add(ibercajaUserInfo);
                        }

                        userQueue.CompleteAdding();
                    }
                }

                Task.WaitAll(tasks.ToArray());
                _logger.Info("Done processing users");
                userQueue.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Info("Failed to process users", ex);
                return new JobResult { JobStatus = JobStatusEnum.Done };
            }
            return new JobResult { JobStatus = JobStatusEnum.Done };
        }

        private void ProcessUser(IbercajaUserInfo ibercajaUserInfo)
        {
            string personId;
            if (ibercajaUserInfo.IsActive == true)
            {
                personId = CreateOrUpdateUser(ibercajaUserInfo);
            }
            else
            {
                personId = DeleteUser(ibercajaUserInfo);
            }

            if (!string.IsNullOrEmpty(personId))
            {
                UpdateLastUpdate(ibercajaUserInfo, personId);
            }
        }
        
        private string DeleteUser(IbercajaUserInfo ibercajaUserInfo)
        {
            var realm = _accountSetupCache.GetRealm(_realmIdentifier);
            var realmUser = _userManager.GetPersonIdByRealmUserIdentifier(ibercajaUserInfo.UserIdentifier,realm.Id);

            if (realmUser.HasValue)
            {
                _userManager.DeletePerson(realmUser.Value);

                return realmUser.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        private void UpdateLastUpdate(IbercajaUserInfo ibercajaUserInfo, string personId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MenigaBatchEntities"].ConnectionString;
            string query = "Update batch.ibercaja_user_info set last_update = getdate() where user_identifier = " + ibercajaUserInfo.UserIdentifier;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    int updateCount = command.ExecuteNonQuery();
                    if (updateCount != 1)
                    {
                        _logger.ErrorFormat("Update 'Last_update' field for PersonId {0} and userIdentifier {1} failed", personId, ibercajaUserInfo.UserIdentifier);
                    }
                }
            }

            _logger.InfoFormat("Finished processing user with PersonId {0} and userIdentifier {1} ", personId, ibercajaUserInfo.UserIdentifier);
        }

        private string CreateOrUpdateUser(IbercajaUserInfo ibercajaUserInfo)
        {
            var userProfile = new UserProfile
            {
                BirthYear = ibercajaUserInfo.DateOfBirth,
                PostalCode = ibercajaUserInfo.PostalCode,
                Created = DateTime.Now
            };

            if (ibercajaUserInfo.Gender.ToLower().Equals("h")) //Hombres
            {
                userProfile.Gender = 2;
            }
            else if (ibercajaUserInfo.Gender.ToLower().Equals("m")) //Mujeres
            {
                userProfile.Gender = 1;
            }
            else
            {
                userProfile.Gender = 0;
            }

            var realm = _accountSetupCache.GetRealm(_realmIdentifier);
            var realmUser = _userManager.GetOrCreateUserForRealmUser(realm.Id, ibercajaUserInfo.UserIdentifier, 1, RealmUserInfo);
            _accountsManager.CreateRealmUserInfo(ibercajaUserInfo.UserIdentifier, realm.Id, 1, null);

            using (var context = _dataContextProvider.UserContext(realmUser.UserId))
            {
                var person = context.Persons.FirstOrDefault(x => x.Id == realmUser.PersonId);
                if (person == null)
                {
                    return string.Empty;
                }

                person.FirstName = ibercajaUserInfo.FirstName.Trim();
                person.LastName = ibercajaUserInfo.LastName.Trim();
                person.Gender = userProfile.Gender;
                person.DateOfBirth = userProfile.BirthYear;
                person.User.PostalCode = userProfile.PostalCode.Replace(" ", "");
                person.Email = ibercajaUserInfo.Email.Replace(" ", "");
                person.Culture = "es-ES";

                if (person.Gender != null && person.DateOfBirth != null)
                {
                    person.IsPersonalSetupDone = true;
                }

                context.SaveChanges();
            }
            return realmUser.PersonId.ToString();
        }

        public void LogEntry(string message)
        {
            _logger.Debug(message);
            _jobManager.LogEntry(new JobLog { JobId = _jobId, LogDate = DateTime.Now, LogText = message });
        }
    }

    internal struct IbercajaUserInfo
    {
        public string UserIdentifier { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PostalCode { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }
}
