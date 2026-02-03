namespace Demo_Ecommerce.Models;

// Caller ID Request Models
public class CallerIdRequest
{
    public string OwnerName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PassportSizeImage { get; set; } = string.Empty; // Base64
    public string Nid { get; set; } = string.Empty;
    public string Dob { get; set; } = string.Empty; // YYYY-MM-DD
    public string Gender { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public string MotherName { get; set; } = string.Empty;
    public string? ResellerName { get; set; }
    public string? ResellerPhone { get; set; }
    public string? ResellerNID { get; set; }
    public string Signature { get; set; } = string.Empty; // Base64
    public string Seal { get; set; } = string.Empty; // Base64
    public string Date { get; set; } = string.Empty; // YYYY-MM-DD
    public string FlatNo { get; set; } = string.Empty;
    public string HouseNoOrName { get; set; } = string.Empty;
    public string RoadNoOrMoholla { get; set; } = string.Empty;
    public string AreaOrVillage { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string UpazilaOrThana { get; set; } = string.Empty;
    public string PostCode { get; set; } = string.Empty;
    public string CallerIdRequestHook { get; set; } = string.Empty;
    public string SmsEnabled { get; set; } = "Yes";
    public string CallerIdPayload { get; set; } = string.Empty;
}

// Call Automation Models
public class CallAutomationRequest
{
    public string CallPayload { get; set; } = string.Empty;
    public string CallerId { get; set; } = string.Empty;
    public string PerCallDuration { get; set; } = "5";
    public Dictionary<string, string> Messages { get; set; } = new();
    public string Number { get; set; } = string.Empty;
    public List<CallButton> Buttons { get; set; } = new();
    public string DeliveryHook { get; set; } = string.Empty;
}

public class CallButton
{
    public string Id { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

// Call Center Models
public class CallCenterRequest
{
    public string CallerId { get; set; } = string.Empty;
    public string CallPrefix { get; set; } = "1000";
    public string TotalAgents { get; set; } = "5";
    public string StatusHook { get; set; } = string.Empty;
    public string EndCallHook { get; set; } = string.Empty;
    public string? RedirectUrl { get; set; }
    public string DomainUrl { get; set; } = string.Empty;
}

public class AgentRequest
{
    public string CallerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string CallPermission { get; set; } = "both"; // inbound, outbound, both
    public bool IsIncomingCallAutoConnect { get; set; } = false;
    public string PhoneType { get; set; } = "WEBPHONE"; // WEBPHONE, IPPHONE
    public string ExpireDate { get; set; } = string.Empty; // YYYY-MM-DD
}

public class ClickToCallRequest
{
    public string CallerId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? Payload { get; set; }
}

// Webhook Response Models
public class DeliveryHookResponse
{
    public string CallPayload { get; set; } = string.Empty;
    public string CallerId { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public List<CallButton> Buttons { get; set; } = new();
    public string UserPressed { get; set; } = string.Empty;
    public string Actions { get; set; } = string.Empty;
    public List<SmsStatus> Sms { get; set; } = new();
    public string Duration { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ForwardNumber { get; set; }
    public string? RecordAudioURL { get; set; }
    public string? RecordTranscribed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SmsStatus
{
    public string Id { get; set; } = string.Empty;
    public string Sms { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Pending, Delivered, Failed
}

public class EndCallHookResponse
{
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyEmail { get; set; } = string.Empty;
    public string AgentEmail { get; set; } = string.Empty;
    public string CallerId { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string CallType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Duration { get; set; }
    public decimal Billing { get; set; }
    public string? RecFile { get; set; }
    public string? Payload { get; set; }
}

// API Response Models
public class ManyDialResponse
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
    public object? Data { get; set; }
}
