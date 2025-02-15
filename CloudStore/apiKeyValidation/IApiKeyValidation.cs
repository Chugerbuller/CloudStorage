namespace CloudStore.WebApi.apiKeyValidation;

public interface IApiKeyValidation
{
    bool IsValidApiKey(string userApiKey);
}
