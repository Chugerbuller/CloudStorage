using CloudStore.BL;

namespace CloudStore.WebApi.apiKeyValidation;

public class ApiKeyValidation : IApiKeyValidation
{
    private readonly CSUsersDbHelper _dbHelper;

    public ApiKeyValidation(CSUsersDbHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public bool IsValidApiKey(string userApiKey)
    {
        if (string.IsNullOrWhiteSpace(userApiKey))
            return false;
        var user = _dbHelper.GetUserByApiKey(userApiKey);
        if (user is null)
            return false;
        return true;
    }
}