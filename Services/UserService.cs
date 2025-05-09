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
    public interface IUserService
    {
        Task<User> AddUserAsync(User user);
        Task<bool> UserNameAndEmailExists(string userName, string email);
        Task<User> UserIsAuthenticatedAsync(User user);
    }

    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _config;
        public UserService(ILogger<UserService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<User> AddUserAsync(User user)
        {
            _logger.LogInformation($"Adding user: {user.UserName}, {user.Email}");
            if (string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
            {
                _logger.LogWarning("Invalid user data received.");
                return null;
            }

            var conn = Util.GetConnectionString();
            _logger.LogInformation($"Connection string: {conn}");

            try
            {
                await using (var connection = new SqlConnection(conn))
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand("INSERT INTO Users (UserName, Email, Password) OUTPUT INSERTED.Id VALUES (@UserName, @Email, @Password)", connection);
                    command.Parameters.AddWithValue("@UserName", user.UserName);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@Password", user.Password);

                    var id = await command.ExecuteScalarAsync();
                    if (id != null)
                    {
                        user.Id = Convert.ToInt32(id);
                        _logger.LogInformation($"User added successfully: Id-{user.Id}, UserName-{user.UserName}, Email-{user.Email}");
                        return user;
                    }
                    else
                    {
                        _logger.LogError("Failed to add user.");
                        return null;
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError($"SQL Exception: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UserNameAndEmailExists(string userName, string email)
        {
            _logger.LogInformation($"Checking user: {userName}, email: {email}");
            if(string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Invalid user data received.");
                return false;
            }

            var conn = Util.GetConnectionString();
            _logger.LogInformation($"Connection string: {conn}");

            try
            {
                await using (var connection = new SqlConnection(conn))
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand("SELECT COUNT(1) FROM Users WHERE UserName = @UserName OR Email = @Email", connection);
                    command.Parameters.AddWithValue("@UserName", userName);
                    command.Parameters.AddWithValue("@Email", email);

                    var count = (int?)await command.ExecuteScalarAsync();
                    
                    return count.HasValue && count > 0;
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError($"SQL Exception: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<User> UserIsAuthenticatedAsync(User user)
        {
            _logger.LogInformation($"Authenticating user: email: {user.Email}");
            if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
            {
                _logger.LogWarning("Invalid user data received.");
                return null;
            }

            var conn = Util.GetConnectionString();
            _logger.LogInformation($"Connection string: {conn}");
            try
            {
                await using (var connection = new SqlConnection(conn))
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand("SELECT Id, USerName FROM Users WHERE Email = @Email AND Password = @Password", connection);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@Password", user.Password);

                    var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        user.Id = Convert.ToInt32(reader["Id"]);
                        user.UserName = reader["UserName"] == null ? string.Empty : reader["UserName"].ToString();
                        // user.Email = reader["Email"].ToString();
                        return user;
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError($"SQL Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
            }
            throw null;
        }
    }
}