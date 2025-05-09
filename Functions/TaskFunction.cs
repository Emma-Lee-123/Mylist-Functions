using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Mylist.Function.Services;
using MyList.Function.Models;
using MyList.Function.Utils;

namespace MyList.Function.Functions
{
    public class TaskFunction
    {
        private readonly ILogger<TaskFunction> _logger;
        private readonly ITaskService _taskService;

        public TaskFunction(ILogger<TaskFunction> logger, ITaskService taskService)
        {
            _logger = logger;
            _taskService = taskService;
        }

        [Function("AddTask")]
        public async Task<HttpResponseData> AddTask([HttpTrigger(AuthorizationLevel.Function, "post", Route = "tasks/add")] HttpRequestData req, FunctionContext context)
        {
            _logger.LogInformation("Processing AddTask request.");
            var requestBody = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                _logger.LogWarning("Request body is empty.");
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Request body is empty.");
                return badResponse;
            }
            var taskItem = JsonSerializer.Deserialize<TaskItem>(requestBody, Util._serializerOptions);
            if (taskItem == null || string.IsNullOrEmpty(taskItem.TaskName) || taskItem.UserId <= 0)
            {
                _logger.LogWarning("Invalid task item data.");
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid task item data.");
                return badResponse;
            }

            try
            {
                var addedTask = await _taskService.AddTaskAsync(taskItem);
                if (addedTask == null)
                {
                    _logger.LogError("Failed to add task.");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync("Failed to add task.");
                    return errorResponse;
                }
                _logger.LogInformation($"Task added successfully: Id-{addedTask.Id}, TaskName-{addedTask.TaskName}, UserId-{addedTask.UserId}");
                var response = req.CreateResponse(HttpStatusCode.Created);

                await response.WriteAsJsonAsync(addedTask, Util.GetObjectSerializer());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occurred: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An error occurred while processing the request.");
                return errorResponse;
            }
        }

        [Function("GetTasksByUserId")]
        public async Task<HttpResponseData> GetTasksByUserId([HttpTrigger(AuthorizationLevel.Function, "get", Route = "tasks/user/{userId}")] HttpRequestData req, FunctionContext context, int userId)
        {
            _logger.LogInformation($"Processing GetTasksByUserId request for UserId: {userId}");
            if (userId <= 0)
            {
                _logger.LogWarning("Invalid UserId.");
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid UserId.");
                return badResponse;
            }

            try
            {
                var tasks = await _taskService.GetTasksByUserIdAsync(userId);
                if (tasks == null || tasks.Count == 0)
                {
                    _logger.LogInformation($"No tasks found for UserId: {userId}");
                }
                //     var notFoundResponse = req.CreateResponse(HttpStatusCode.OK);
                //     await notFoundResponse.WriteAsJsonAsync(new List<TaskItem>(), Util.GetObjectSerializer());//.WriteStringAsync("No tasks found for the specified UserId.");
                //     return notFoundResponse;
                // }
                var response = req.CreateResponse(HttpStatusCode.OK);
                //await response.WriteAsJsonAsync(tasks, Util.GetObjectSerializer());
                var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });
                response.Headers.Add("Content-Type", "application/json");
                await response.WriteStringAsync(json);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occurred: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An error occurred while processing the request.");
                return errorResponse;
            }
        }

        [Function("SetTaskCompleted")]
        public async Task<HttpResponseData> SetTaskCompleted([HttpTrigger(AuthorizationLevel.Function, "put", Route = "tasks/complete")] HttpRequestData req, FunctionContext context)
        {
            _logger.LogInformation($"Processing SetTaskCompleted request");
            var requestBody = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                _logger.LogWarning("Request body is empty.");
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Request body is empty.");
                return badResponse;
            }

            var taskItem = JsonSerializer.Deserialize<TaskItem>(requestBody, Util._serializerOptions);
            if (taskItem == null || taskItem.Id <= 0)
            {
                _logger.LogWarning("Invalid task item data.");
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid task item data.");
                return badResponse;
            }

            try
            {
                var updated = await _taskService.SetTaskCompletedAsync(taskItem.Id, taskItem.IsCompleted);
                if (!updated)
                {
                    _logger.LogError($"Failed to update task for TaskId: {taskItem.Id}");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync("Failed to update task isCompleted.");
                    return errorResponse;
                }
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(true);//.WriteStringAsync($"Task {taskItem.Id} isCompleted was updated successfully.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occurred: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An error occurred while processing the request.");
                return errorResponse;
            }
        }

        [Function("DeleteTask")]
        public async Task<HttpResponseData> DeleteTask([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "tasks/delete/{taskId}")] HttpRequestData req, int taskId)
        {
            _logger.LogInformation($"Processing DeleteTask request for TaskId: {taskId}");
            if (taskId <= 0)
            {
                _logger.LogWarning("Invalid TaskId.");
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid TaskId.");
                return badResponse;
            }

            try
            {
                var deleted = await _taskService.DeleteTaskAsync(taskId);
                if (!deleted)
                {
                    _logger.LogError($"Failed to delete task for TaskId: {taskId}");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync("Failed to delete task.");
                    return errorResponse;
                }
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(true);//.WriteStringAsync("Task deleted successfully.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occurred: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An error occurred while processing the request.");
                return errorResponse;
            }
        }

        // [Function("UpdateTask")]
        // public async Task<HttpResponseData> UpdateTask([HttpTrigger(AuthorizationLevel.Function, "put", Route = "tasks/update")] HttpRequestData req)
        //         {
        //     _logger.LogInformation("Processing UpdateTask request.");
        //     var requestBody = await req.ReadAsStringAsync();
        //     if (string.IsNullOrEmpty(requestBody))
        //     {
        //         _logger.LogWarning("Request body is empty.");
        //         var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
        //         await badResponse.WriteStringAsync("Request body is empty.");
        //         return badResponse;
        //     }
        //     var taskItem = JsonSerializer.Deserialize<TaskItem>(requestBody, Util._serializerOptions);
        //     if (taskItem == null || taskItem.Id <= 0 || string.IsNullOrEmpty(taskItem.TaskName))
        //     {
        //         _logger.LogWarning("Invalid task item data.");
        //         var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
        //         await badResponse.WriteStringAsync("Invalid task item data.");
        //         return badResponse;
        //     }

        //     try
        //     {
        //         var updated = await _taskService.UpdateTaskAsync(taskItem);
        //         if (!updated)
        //         {
        //             _logger.LogError("Failed to update task.");
        //             var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
        //             await errorResponse.WriteStringAsync("Failed to update task.");
        //             return errorResponse;
        //         }
        //         var response = req.CreateResponse(HttpStatusCode.OK);
        //         await response.WriteAsJsonAsync(updated);
        //         return response;
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError($"Exception occurred: {ex.Message}");
        //         var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
        //         await errorResponse.WriteStringAsync("An error occurred while processing the request.");
        //         return errorResponse;
        //     }
        // }
    }
}