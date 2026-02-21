using System.Text;
using System.Text.Json;
using Demo_Ecommerce.Models;

namespace Demo_Ecommerce.Services;

public interface IManyDialService
{
    Task<ManyDialResponse> RequestCallerIdAsync(CallerIdRequest request);
    Task<ManyDialResponse> DispatchCallAsync(CallAutomationRequest request);
    Task<ManyDialResponse> CreateCallCenterAsync(CallCenterRequest request);
    Task<ManyDialResponse> RenewCallCenterAsync(string callerId, string expireDate);
    Task<ManyDialResponse> CreateAgentAsync(AgentRequest request);
    Task<ManyDialResponse> DeleteAgentAsync(string email, string callerId);
    Task<ManyDialResponse> GetAgentListAsync(string callerId);
    Task<ManyDialResponse> ClickToCallAsync(ClickToCallRequest request);
}

public class ManyDialService : IManyDialService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ManyDialService> _logger;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _callerId;

    public ManyDialService(HttpClient httpClient, IConfiguration configuration, ILogger<ManyDialService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _apiKey = _configuration["ManyDial:ApiKey"] ?? throw new ArgumentNullException("ManyDial:ApiKey");
        _baseUrl = _configuration["ManyDial:BaseUrl"] ?? "https://api.manydial.com/v1/portal";
        _callerId = _configuration["ManyDial:CallerId"] ?? "";
    }

    private void SetHeaders()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
    }

    public async Task<ManyDialResponse> RequestCallerIdAsync(CallerIdRequest request)
    {
        SetHeaders();
        
        var formData = new MultipartFormDataContent
        {
            { new StringContent(request.OwnerName), "ownerName" },
            { new StringContent(request.BusinessName), "businessName" },
            { new StringContent(request.Email), "email" },
            { new StringContent(request.Phone), "phone" },
            { new StringContent(request.PassportSizeImage), "passportSizeImage" },
            { new StringContent(request.Nid), "nid" },
            { new StringContent(request.Dob), "dob" },
            { new StringContent(request.Gender), "gender" },
            { new StringContent(request.FatherName), "fatherName" },
            { new StringContent(request.MotherName), "motherName" },
            { new StringContent(request.Signature), "signature" },
            { new StringContent(request.Seal), "seal" },
            { new StringContent(request.Date), "date" },
            { new StringContent(request.FlatNo), "flatNo" },
            { new StringContent(request.HouseNoOrName), "houseNoOrName" },
            { new StringContent(request.RoadNoOrMoholla), "roadNoOrMoholla" },
            { new StringContent(request.AreaOrVillage), "areaOrVillage" },
            { new StringContent(request.Division), "division" },
            { new StringContent(request.District), "district" },
            { new StringContent(request.UpazilaOrThana), "upazilaOrThana" },
            { new StringContent(request.PostCode), "postCode" },
            { new StringContent(request.CallerIdRequestHook), "callerIdRequestHook" },
            { new StringContent(request.SmsEnabled), "smsEnabled" },
            { new StringContent(request.CallerIdPayload), "callerIdPayload" }
        };

        if (!string.IsNullOrEmpty(request.ResellerName))
            formData.Add(new StringContent(request.ResellerName), "resellerName");
        if (!string.IsNullOrEmpty(request.ResellerPhone))
            formData.Add(new StringContent(request.ResellerPhone), "resellerPhone");
        if (!string.IsNullOrEmpty(request.ResellerNID))
            formData.Add(new StringContent(request.ResellerNID), "resellerNID");

        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/callerId", formData);
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Caller ID Request Response: {Content}", content);
            return JsonSerializer.Deserialize<ManyDialResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                   ?? new ManyDialResponse { Success = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting caller ID");
            return new ManyDialResponse { Success = false, Message = ex.Message, Error = ex.ToString() };
        }
    }

    public async Task<ManyDialResponse> DispatchCallAsync(CallAutomationRequest request)
    {
        SetHeaders();
        
        // Serialize with camelCase for ManyDial API
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        
        var formData = new MultipartFormDataContent
        {
            { new StringContent(request.CallPayload), "callPayload" },
            { new StringContent(string.IsNullOrEmpty(request.CallerId) ? _callerId : request.CallerId), "callerId" },
            { new StringContent(request.PerCallDuration), "perCallDuration" },
            { new StringContent(JsonSerializer.Serialize(request.Messages)), "messages" },
            { new StringContent(request.Number), "number" },
            { new StringContent(JsonSerializer.Serialize(request.Buttons, jsonOptions)), "buttons" },
            { new StringContent(request.DeliveryHook), "deliveryHook" }
        };

        _logger.LogInformation("Dispatching call to {Number} with CallerId {CallerId}", request.Number, request.CallerId);
        _logger.LogInformation("Messages: {Messages}", JsonSerializer.Serialize(request.Messages));
        _logger.LogInformation("Buttons: {Buttons}", JsonSerializer.Serialize(request.Buttons, jsonOptions));

        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/call/dispatch", formData);
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Call Dispatch Response: {Content}", content);
            return JsonSerializer.Deserialize<ManyDialResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                   ?? new ManyDialResponse { Success = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching call");
            return new ManyDialResponse { Success = false, Message = ex.Message, Error = ex.ToString() };
        }
    }

    public async Task<ManyDialResponse> CreateCallCenterAsync(CallCenterRequest request)
    {
        SetHeaders();
        _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
        
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/call-center", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Call Center Create Response: {Content}", responseContent);
            return JsonSerializer.Deserialize<ManyDialResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                   ?? new ManyDialResponse { Success = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating call center");
            return new ManyDialResponse { Success = false, Message = ex.Message, Error = ex.ToString() };
        }
    }

    public async Task<ManyDialResponse> RenewCallCenterAsync(string callerId, string expireDate)
    {
        SetHeaders();
        
        var data = new { callerId, expireDate };
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/call-center/renew", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Call Center Renew Response: {Content}", responseContent);
            return JsonSerializer.Deserialize<ManyDialResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                   ?? new ManyDialResponse { Success = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing call center");
            return new ManyDialResponse { Success = false, Message = ex.Message, Error = ex.ToString() };
        }
    }

    public async Task<ManyDialResponse> CreateAgentAsync(AgentRequest request)
    {
        SetHeaders();
        
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/agent/create", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Agent Create Response: {Content}", responseContent);
            return JsonSerializer.Deserialize<ManyDialResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                   ?? new ManyDialResponse { Success = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent");
            return new ManyDialResponse { Success = false, Message = ex.Message, Error = ex.ToString() };
        }
    }

    public async Task<ManyDialResponse> DeleteAgentAsync(string email, string callerId)
    {
        SetHeaders();

        try
        {
            var url = $"{_baseUrl}/agent/delete?email={Uri.EscapeDataString(email)}&callerId={Uri.EscapeDataString(callerId)}";
            var response = await _httpClient.DeleteAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Agent Delete Response: {Content}", responseContent);
            return JsonSerializer.Deserialize<ManyDialResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                   ?? new ManyDialResponse { Success = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting agent");
            return new ManyDialResponse { Success = false, Message = ex.Message, Error = ex.ToString() };
        }
    }

    public async Task<ManyDialResponse> GetAgentListAsync(string callerId)
    {
        SetHeaders();

        try
        {
            var url = $"{_baseUrl}/call-center/agent-list?callerId={Uri.EscapeDataString(callerId)}";
            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Agent List Response: {Content}", responseContent);
            return JsonSerializer.Deserialize<ManyDialResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                   ?? new ManyDialResponse { Success = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting agent list");
            return new ManyDialResponse { Success = false, Message = ex.Message, Error = ex.ToString() };
        }
    }

    public async Task<ManyDialResponse> ClickToCallAsync(ClickToCallRequest request)
    {
        SetHeaders();
        
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/click-to-call", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Click To Call Response: {Content}", responseContent);
            return JsonSerializer.Deserialize<ManyDialResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                   ?? new ManyDialResponse { Success = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating click to call");
            return new ManyDialResponse { Success = false, Message = ex.Message, Error = ex.ToString() };
        }
    }
}
