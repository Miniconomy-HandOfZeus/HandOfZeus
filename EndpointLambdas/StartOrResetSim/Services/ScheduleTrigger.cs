using Amazon.Scheduler;
using Amazon.Scheduler.Model;

namespace StartOrResetSim.Services
{
    public class ScheduleTrigger
    {
        private readonly AmazonSchedulerClient _client = new AmazonSchedulerClient();
        private readonly List<string> scheduleNames = new List<string> { "RandomEventSchedule-scheduler" };
        private readonly string scheduleGroupName = "hand-of-zeus-events-scheduler-group";

        public async Task StartAsync()
        {
            foreach (var scheduleName in scheduleNames)
            {
                await EnableScheduleAsync(scheduleName);
            }
        }

        private async Task EnableScheduleAsync(string scheduleName)
        {
            try
            {
                var request = new UpdateScheduleRequest
                {
                    Name = scheduleName,
                    GroupName = scheduleGroupName,
                    State = ScheduleState.ENABLED
                };

                await _client.UpdateScheduleAsync(request);
                Console.WriteLine($"Schedule {scheduleName} in group {scheduleGroupName} enabled successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to enable schedule {scheduleName} in group {scheduleGroupName}: {ex.Message}");
            }
        }
    }
}
