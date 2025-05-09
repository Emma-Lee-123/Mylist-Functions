using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyList.Function.Models;
using Microsoft.Data.SqlClient;
using MyList.Function.Utils;

namespace Mylist.Function.Services
{
    public interface ITaskService
    {
        Task<TaskItem?> AddTaskAsync(TaskItem task);
        Task<List<TaskItem>> GetTasksByUserIdAsync(int userId);

        // Task<bool> UpdateTaskAsync(TaskItem task);
        Task<bool> DeleteTaskAsync(int taskId);

        Task<bool> SetTaskCompletedAsync(int taskId, bool isCompleted);
    }

    public class TaskService : ITaskService
    {
        private readonly ILogger<TaskService> _logger;
        private readonly IConfiguration _config;
        public TaskService(ILogger<TaskService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<TaskItem?> AddTaskAsync(TaskItem task)
        {
            _logger.LogInformation($"Adding task: {task.TaskName} for user {task.UserId}");
            if (string.IsNullOrWhiteSpace(task.TaskName) || task.UserId <= 0)
            {
                _logger.LogWarning("Invalid task data received.");
                return null;
            }

            try
            {
                var conn = Util.GetConnectionString();
                _logger.LogInformation($"Connection string: {conn}");

                await using (var connection = new SqlConnection(conn))
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand("INSERT INTO Tasks (UserId, TaskName) OUTPUT INSERTED.Id VALUES (@UserId, @TaskName)", connection);
                    command.Parameters.AddWithValue("@UserId", task.UserId);
                    command.Parameters.AddWithValue("@TaskName", task.TaskName);

                    var id = await command.ExecuteScalarAsync();
                    if (id != null)
                    {
                        task.Id = Convert.ToInt32(id);
                        task.IsCompleted = false; // Default value for new tasks
                        _logger.LogInformation($"Task added successfully: Id-{task.Id}, TaskName-{task.TaskName}, UserId-{task.UserId}");
                        return task;
                    }
                    else
                    {
                        _logger.LogError("Failed to add task.");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding task.");
                return null;
            }
        }

        public async Task<List<TaskItem>> GetTasksByUserIdAsync(int userId)
        {
            _logger.LogInformation($"Fetching tasks for user {userId}");
            if (userId <= 0)
            {
                _logger.LogWarning("Invalid user ID received.");
                return new List<TaskItem>();
            }

            try
            {
                var conn = Util.GetConnectionString();
                _logger.LogInformation($"Connection string: {conn}");

                await using (var connection = new SqlConnection(conn))
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand("SELECT Id, TaskName, IsCompleted FROM Tasks WHERE UserId = @UserId", connection);
                    command.Parameters.AddWithValue("@UserId", userId);

                    var reader = await command.ExecuteReaderAsync();
                    var tasks = new List<TaskItem>();
                    while (await reader.ReadAsync())
                    {
                        tasks.Add(new TaskItem
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            // UserId = Convert.ToInt32(reader["UserId"]),
                            TaskName = reader["TaskName"] == null ? string.Empty : reader["TaskName"].ToString(),
                            // Category = reader["Category"].ToString(),
                            IsCompleted = Convert.ToBoolean(reader["IsCompleted"]),
                        });
                    }
                    return tasks;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tasks.");
                return new List<TaskItem>();
            }
        }

        // public async Task<bool> UpdateTaskAsync(TaskItem task)
        // {
        //     _logger.LogInformation($"Updating task: {task.Id} for user {task.UserId}");
        //     if (task.Id <= 0 || string.IsNullOrWhiteSpace(task.TaskName) || task.UserId <= 0)
        //     {
        //         _logger.LogWarning("Invalid task data received.");
        //         return false;
        //     }

        //     try
        //     {
        //         var conn = _config.GetConnectionString("SqlConnectionString");
        //         _logger.LogInformation($"Connection string: {conn}");

        //         await using (var connection = new SqlConnection(conn))
        //         {
        //             await connection.OpenAsync();
        //             var command = new SqlCommand("UPDATE Tasks SET TaskName = @TaskName, IsCompleted = @IsCompleted WHERE Id = @Id", connection);
        //             command.Parameters.AddWithValue("@TaskName", task.TaskName);
        //             command.Parameters.AddWithValue("@IsCompleted", task.IsCompleted);
        //             command.Parameters.AddWithValue("@Id", task.Id);
        //             // command.Parameters.AddWithValue("@UserId", task.UserId);

        //             var rowsAffected = await command.ExecuteNonQueryAsync();
        //             return rowsAffected > 0;
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error updating task.");
        //         return false;
        //     }
        // }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            _logger.LogInformation($"Deleting task: {taskId}");
            if (taskId <= 0)
            {
                _logger.LogWarning("Invalid task ID received.");
                return false;
            }

            try
            {
                var conn = Util.GetConnectionString();
                _logger.LogInformation($"Connection string: {conn}");

                await using (var connection = new SqlConnection(conn))
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand("DELETE FROM Tasks WHERE Id = @Id", connection);
                    command.Parameters.AddWithValue("@Id", taskId);

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task.");
                return false;
            }
        }

        public async Task<bool> SetTaskCompletedAsync(int taskId, bool isCompleted)
        {
            _logger.LogInformation($"Setting task {taskId} as completed: {isCompleted}");
            if (taskId <= 0)
            {
                _logger.LogWarning("Invalid task ID received.");
                return false;
            }

            try
            {
                var conn = Util.GetConnectionString();
                _logger.LogInformation($"Connection string: {conn}");

                using (var connection = new SqlConnection(conn))
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand("UPDATE Tasks SET IsCompleted = @IsCompleted WHERE Id = @Id", connection);
                    command.Parameters.AddWithValue("@IsCompleted", isCompleted);
                    command.Parameters.AddWithValue("@Id", taskId);

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting task as completed.");
                return false;
            }
        }
    }
}