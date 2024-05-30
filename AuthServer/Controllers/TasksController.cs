using System.Text.Json;
using DittoSDK;
using Microsoft.AspNetCore.Mvc;

namespace DittoWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DittoController : ControllerBase
{
    private const string DittoGetTasksQuery = $"SELECT * FROM {DittoTask.CollectionName} WHERE isDeleted = false";

    private readonly ILogger<DittoController> _logger;
    private readonly Ditto _ditto; 

    public DittoController(ILogger<DittoController> logger, Ditto ditto)
    {
        _logger = logger;
        _ditto = ditto;

        if (!ditto.Sync.Subscriptions.Any(s => s.QueryString.Equals(DittoGetTasksQuery)))
        {
            ditto.Sync.RegisterSubscription(DittoGetTasksQuery);            
        }
    }

    [HttpGet(Name = "Tasks")]
    public async Task<IEnumerable<DittoTask>> Get()
    {
        var result = await _ditto.Store.ExecuteAsync(DittoGetTasksQuery);
        var items = result.Items.ConvertAll(d => JsonSerializer.Deserialize<DittoTask>(d.JsonString()));

        return items;
    }

    [HttpPost(Name = "Tasks")]
    public async Task<DittoTask> Post(string taskName)
    {
        var task = new DittoTask(taskName, false);

        await _ditto.Store.ExecuteAsync($"INSERT INTO {DittoTask.CollectionName} DOCUMENTS (:task)", new Dictionary<string, object>()
        {
            { "task", task.ToDictionary() }
        });

        return task;
    }

    [HttpPut(Name = "Tasks")]
    public async Task<bool> Put(string taskId, bool isCompleted)
    {
        var updateQuery = $"UPDATE {DittoTask.CollectionName} " +
            $"SET isCompleted = {isCompleted} " +
            $"WHERE _id = '{taskId}' AND isCompleted != {isCompleted}";
        await _ditto.Store.ExecuteAsync(updateQuery);

        return true;
    }


    [HttpDelete(Name = "Tasks")]
    public async Task<bool> Delete(string taskId)
    {
        var updateQuery = $"UPDATE {DittoTask.CollectionName} " +
            "SET isDeleted = true " +
            $"WHERE _id = '{taskId}'";
        var result = await _ditto.Store.ExecuteAsync(updateQuery);

        return true;
    }
}

