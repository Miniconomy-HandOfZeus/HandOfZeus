using Amazon.EventBridge;
using Amazon.EventBridge.Model;

namespace StartOrResetSim.Services
{
    public class ScheduleTrigger
    {
        private readonly AmazonEventBridgeClient _client = new();
        private readonly List<string> scheduleNames = ["RandomEventSchedule-scheduler"];

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
                var request = new EnableRuleRequest
                {
                    Name = scheduleName
                };

                await _client.EnableRuleAsync(request);
                Console.WriteLine($"Schedule {scheduleName} enabled successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to enable schedule {scheduleName}: {ex.Message}");
            }
        }
    }
}
