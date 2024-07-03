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

        public async Task StopAsync()
        {
            foreach (var scheduleName in scheduleNames)
            {
                await DisableScheduleAsync(scheduleName);
            }
        }

        private async Task EnableScheduleAsync(string scheduleName)
        {
            try
            {
                // Get the existing schedule
                var getRequest = new GetScheduleRequest
                {
                    Name = scheduleName,
                    GroupName = scheduleGroupName
                };

                var getResponse = await _client.GetScheduleAsync(getRequest);

                // Update the state to ENABLED
                var updateRequest = new UpdateScheduleRequest
                {
                    Name = scheduleName,
                    GroupName = scheduleGroupName,
                    State = ScheduleState.ENABLED,
                    ScheduleExpression = getResponse.ScheduleExpression,
                    FlexibleTimeWindow = getResponse.FlexibleTimeWindow,
                    Target = getResponse.Target
                };

                await _client.UpdateScheduleAsync(updateRequest);
                Console.WriteLine($"Schedule {scheduleName} in group {scheduleGroupName} enabled successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to enable schedule {scheduleName} in group {scheduleGroupName}: {ex.Message}");
            }
        }

        private async Task DisableScheduleAsync(string scheduleName)
        {
            try
            {
                // Get the existing schedule
                var getRequest = new GetScheduleRequest
                {
                    Name = scheduleName,
                    GroupName = scheduleGroupName
                };

                var getResponse = await _client.GetScheduleAsync(getRequest);

                // Update the state to DISABLED
                var updateRequest = new UpdateScheduleRequest
                {
                    Name = scheduleName,
                    GroupName = scheduleGroupName,
                    State = ScheduleState.DISABLED,
                    ScheduleExpression = getResponse.ScheduleExpression,
                    FlexibleTimeWindow = getResponse.FlexibleTimeWindow,
                    Target = getResponse.Target
                };

                await _client.UpdateScheduleAsync(updateRequest);
                Console.WriteLine($"Schedule {scheduleName} in group {scheduleGroupName} disabled successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to disable schedule {scheduleName} in group {scheduleGroupName}: {ex.Message}");
            }
        }
    }
}
