using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Mylist.Function.Services;
using MyList.Function.Models;
using MyList.Function.Utils;

namespace MyList.Function.Functions
{
    public class UserFunction
    {
        private readonly ILogger<UserFunction> _logger;
        private readonly IUserService _userService;
        public UserFunction(ILogger<UserFunction> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [Function("AddUser")]
        public async Task<HttpResponseData> AddUser([HttpTrigger(AuthorizationLevel.Function, "post", Route = "users/add")] HttpRequestData req, FunctionContext context)
        {
            _logger.LogInformation("Processing AddUser request.");

            try
            {
                var requestBody = await req.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<User>(requestBody, Util._serializerOptions);

                if (user == null || string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
                {
                    _logger.LogWarning("Invalid user data received.");
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("Invalid user data.");
                    return badResponse;
                }
                _logger.LogInformation($"User data received: {user.UserName}, {user.Email}");

                if (await _userService.UserNameAndEmailExists(user.UserName, user.Email))
                {
                    _logger.LogWarning("UserName or Email already exists.");
                    var conflictResponse = req.CreateResponse(HttpStatusCode.Conflict);
                    await conflictResponse.WriteStringAsync("UserName or Email already exists.");
                    return conflictResponse;
                }

                user = await _userService.AddUserAsync(user); // Simulate saving to DB
                if (user == null)
                {
                    _logger.LogError("Failed to add user.");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync("Failed to add user.");
                    return errorResponse;
                }
                _logger.LogInformation($"User added successfully: Id-{user.Id}, UserName-{user.UserName}, Email-{user.Email}");
                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(user);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in AddUser: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An error occurred.");
                return errorResponse;
            }
        }

        [Function("UserExists")]
        public async Task<HttpResponseData> UserExists([HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/exists")] HttpRequestData req, FunctionContext context)
        {
            _logger.LogInformation("Processing UserExists request.");

            try
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
                var userName = queryParams["userName"];
                var email = queryParams["email"];
                if (string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("No user data provided.");
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("No user data provided.");
                    return badResponse;
                }
                _logger.LogInformation($"User data received: UserName-{userName}, Email-{email}");

                var exists = await _userService.UserNameAndEmailExists(userName, email); // Simulate checking in DB
                if (!exists)
                {
                    _logger.LogInformation("UserName and Email are not in use.");
                }
                else
                {
                    _logger.LogInformation("UserName and/or Email are in use.");
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(exists);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in UserExists: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An error occurred.");
                return errorResponse;
            }
        }

        [Function("UserIsAuthenticated")]
        public async Task<HttpResponseData> UserIsAuthenticated([HttpTrigger(AuthorizationLevel.Function, "post", Route = "users/authenticated")] HttpRequestData req, FunctionContext context)
        {
            _logger.LogInformation("Processing UserIsAuthenticated request.");

            try
            {
                var requestBody = await req.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<User>(requestBody, Util._serializerOptions);

                if (user == null || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
                {
                    _logger.LogWarning("Invalid user data received.");
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("Invalid user data.");
                    return badResponse;
                }
                _logger.LogInformation($"User data received: {user.UserName}, {user.Email}");

                var authenticatedUser = await _userService.UserIsAuthenticatedAsync(user); // Simulate checking in DB
                if (authenticatedUser == null)
                {
                    _logger.LogError("User authentication failed.");
                    var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await errorResponse.WriteStringAsync("Authentication failed.");
                    return errorResponse;
                }
                _logger.LogInformation($"User authenticated successfully: Id-{authenticatedUser.Id}, UserName-{authenticatedUser.UserName}, Email-{authenticatedUser.Email}");
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(authenticatedUser, Util.GetObjectSerializer());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in UserIsAuthenticated: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An error occurred.");
                return errorResponse;
            }
        }
    }
}