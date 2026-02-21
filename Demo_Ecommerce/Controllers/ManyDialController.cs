using Microsoft.AspNetCore.Mvc;
using Demo_Ecommerce.Models;
using Demo_Ecommerce.Services;

namespace Demo_Ecommerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManyDialController : ControllerBase
{
    private readonly IManyDialService _manyDialService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ManyDialController> _logger;

    public ManyDialController(IManyDialService manyDialService, IConfiguration configuration, ILogger<ManyDialController> logger)
    {
        _manyDialService = manyDialService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Request a new Caller ID for business registration
    /// </summary>
    [HttpPost("caller-id")]
    public async Task<ActionResult<ManyDialResponse>> RequestCallerId([FromBody] CallerIdRequest request)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        request.CallerIdRequestHook = $"{baseUrl}/api/webhooks/caller-id-status";
        
        var result = await _manyDialService.RequestCallerIdAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Dispatch an automated call
    /// </summary>
    [HttpPost("call/dispatch")]
    public async Task<ActionResult<ManyDialResponse>> DispatchCall([FromBody] CallAutomationRequest request)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        if (string.IsNullOrEmpty(request.DeliveryHook))
        {
            request.DeliveryHook = $"{baseUrl}/api/webhooks/call-delivery";
        }
        if (string.IsNullOrEmpty(request.CallerId))
        {
            request.CallerId = _configuration["ManyDial:CallerId"] ?? "";
        }
        
        var result = await _manyDialService.DispatchCallAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Create a new Call Center
    /// </summary>
    [HttpPost("call-center")]
    public async Task<ActionResult<ManyDialResponse>> CreateCallCenter([FromBody] CallCenterRequest request)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        request.StatusHook = $"{baseUrl}/api/webhooks/call-center-status";
        request.EndCallHook = $"{baseUrl}/api/webhooks/call-end";
        
        var result = await _manyDialService.CreateCallCenterAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Renew Call Center subscription
    /// </summary>
    [HttpPost("call-center/renew")]
    public async Task<ActionResult<ManyDialResponse>> RenewCallCenter([FromBody] RenewRequest request)
    {
        var result = await _manyDialService.RenewCallCenterAsync(request.CallerId, request.ExpireDate);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Create a new Agent
    /// </summary>
    [HttpPost("agent")]
    public async Task<ActionResult<ManyDialResponse>> CreateAgent([FromBody] AgentRequest request)
    {
        var result = await _manyDialService.CreateAgentAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete an Agent
    /// </summary>
    [HttpDelete("agent")]
    public async Task<ActionResult<ManyDialResponse>> DeleteAgent([FromQuery] string email, [FromQuery] string callerId)
    {
        var result = await _manyDialService.DeleteAgentAsync(email, callerId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get Agent List
    /// </summary>
    [HttpGet("agents")]
    public async Task<ActionResult<ManyDialResponse>> GetAgents([FromQuery] string callerId)
    {
        var result = await _manyDialService.GetAgentListAsync(callerId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Initiate Click-to-Call
    /// </summary>
    [HttpPost("click-to-call")]
    public async Task<ActionResult<ManyDialResponse>> ClickToCall([FromBody] ClickToCallRequest request)
    {
        var result = await _manyDialService.ClickToCallAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get Call Center iframe URL
    /// </summary>
    [HttpGet("call-center/iframe-url")]
    public ActionResult<object> GetCallCenterIframeUrl([FromQuery] string email, [FromQuery] string? callerId = null)
    {
        var usedCallerId = callerId ?? _configuration["ManyDial:CallerId"];
        var callCenterUrl = _configuration["ManyDial:CallCenterUrl"] ?? "https://callcenter.manydial.com";
        var iframeUrl = $"{callCenterUrl}?email={Uri.EscapeDataString(email)}&callerId={Uri.EscapeDataString(usedCallerId ?? "")}";
        
        return Ok(new { url = iframeUrl, email, callerId = usedCallerId });
    }

    /// <summary>
    /// Get ManyDial configuration (for demo purposes)
    /// </summary>
    [HttpGet("config")]
    public ActionResult<object> GetConfig()
    {
        return Ok(new 
        {
            callerId = _configuration["ManyDial:CallerId"],
            callCenterUrl = _configuration["ManyDial:CallCenterUrl"],
            apiConfigured = !string.IsNullOrEmpty(_configuration["ManyDial:ApiKey"])
        });
    }

    /// <summary>
    /// Quick test call - bypasses service to test directly
    /// </summary>
    [HttpGet("test-call/{phoneNumber}")]
    public async Task<ActionResult> TestCall(string phoneNumber)
    {
        var apiKey = _configuration["ManyDial:ApiKey"];
        var callerId = _configuration["ManyDial:CallerId"];
        
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        
        var formData = new MultipartFormDataContent
        {
            { new StringContent("test-" + DateTime.Now.Ticks), "callPayload" },
            { new StringContent(callerId ?? ""), "callerId" },
            { new StringContent("1"), "perCallDuration" },
            { new StringContent("{\"welcome\":\"আসসালামু আলাইকুম। এটা টোটো কোম্পানি থেকে টেস্ট কল। ধন্যবাদ।\",\"repeat\":\"1\",\"menuMessage1\":\"ধন্যবাদ\"}"), "messages" },
            { new StringContent(phoneNumber.StartsWith("+") ? phoneNumber : "+" + phoneNumber), "number" },
            { new StringContent("[{\"id\":\"menuMessage1\",\"key\":\"1\",\"value\":\"OK\"}]"), "buttons" },
            { new StringContent("https://webhook.site/test"), "deliveryHook" }
        };

        _logger.LogInformation("TEST CALL - Calling {Number} with CallerId {CallerId}", phoneNumber, callerId);

        var response = await httpClient.PostAsync("https://api.manydial.com/v1/portal/call/dispatch", formData);
        var content = await response.Content.ReadAsStringAsync();
        
        _logger.LogInformation("TEST CALL Response: {Content}", content);
        
        return Ok(new { 
            message = "Test call dispatched",
            apiResponse = content,
            phoneNumber = phoneNumber,
            callerId = callerId
        });
    }
}

public class RenewRequest
{
    public string CallerId { get; set; } = string.Empty;
    public string ExpireDate { get; set; } = string.Empty;
}
