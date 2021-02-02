using Microsoft.Extensions.DependencyInjection;
using Quartz.Spi;
using Quartz;
using System.Collections.Concurrent;
using System;

namespace CumBot.Quartz
{
    public class JobFactory : IJobFactory
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly ConcurrentDictionary<IJob, IServiceScope> _scopes = new ConcurrentDictionary<IJob, IServiceScope>();

        public JobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var scope = _serviceProvider.CreateScope();
            IJob job;

            try
            {
                job = scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
            }
            catch
            {
                scope.Dispose();
                throw;
            }

            if (!_scopes.TryAdd(job, scope))
            {
                scope.Dispose();
                throw new Exception("Failed to track DI scope");
            }

            return job;
        }

        public void ReturnJob(IJob job)
        {
            if (_scopes.TryRemove(job, out var scope))
            {
                scope.Dispose();
            }
        }
    }
}