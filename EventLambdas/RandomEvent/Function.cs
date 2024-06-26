using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RandomEvent;

public class Function
{

  private static readonly Random random = new Random();
  private static readonly List<string> events = new List<string>
  {
    "Death",
    "Marriage",
    "Birth",
    "Hunger",
    "Sickness",
    "Breakages",
    "Salary",
    "Fired from job",
    "Famine",
    "Plague",
    "War",
    "Apocalypse",
    "Inflation"
  };


  public string FunctionHandler(string input, ILambdaContext context)
    {
        var selectedEvent = events[random.Next(events.Count)];
        context.Logger.LogLine($"Selected Event : {selectedEvent}");
        return selectedEvent;
    }
}
