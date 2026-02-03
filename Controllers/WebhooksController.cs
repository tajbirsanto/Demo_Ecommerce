using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Demo_Ecommerce.Data;
using Demo_Ecommerce.Models;
using System.Text.Json;

namespace Demo_Ecommerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(AppDbContext context, ILogger<WebhooksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Webhook for Call Delivery status from ManyDial
    /// </summary>
    [HttpPost("call-delivery")]
    public async Task<ActionResult> HandleCallDelivery([FromBody] DeliveryHookResponse payload)
    {
        _logger.LogInformation("Call Delivery Webhook received: {Payload}", JsonSerializer.Serialize(payload));
        
        _context.WebhookLogs.Add(new WebhookLog
        {
            Type = "call-delivery",
            Payload = JsonSerializer.Serialize(payload),
            ReceivedAt = DateTime.UtcNow
        });

        // Process the call result - update order status based on user response
        if (!string.IsNullOrEmpty(payload.CallPayload))
        {
            var orderId = payload.CallPayload;
            var order = await _context.Orders.FindAsync(orderId);
            
            if (order != null)
            {
                // Update order status based on user input
                if (payload.UserPressed.StartsWith("1"))
                {
                    order.Status = "Confirmed";
                    order.CallStatus = "Confirmed via Call";
                }
                else if (payload.UserPressed.StartsWith("2"))
                {
                    order.Status = "Cancelled";
                    order.CallStatus = "Cancelled via Call";
                }
                else
                {
                    order.CallStatus = $"Call {payload.Status}";
                }
            }
            
            _logger.LogInformation("Order {OrderId}: User pressed {UserPressed}, Actions: {Actions}, Status: {Status}", 
                orderId, payload.UserPressed, payload.Actions, payload.Status);
        }

        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "Webhook received" });
    }

    /// <summary>
    /// Webhook for Caller ID request status updates
    /// </summary>
    [HttpPost("caller-id-status")]
    public async Task<ActionResult> HandleCallerIdStatus([FromBody] object payload)
    {
        _logger.LogInformation("Caller ID Status Webhook received: {Payload}", JsonSerializer.Serialize(payload));
        
        _context.WebhookLogs.Add(new WebhookLog
        {
            Type = "caller-id-status",
            Payload = JsonSerializer.Serialize(payload),
            ReceivedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "Webhook received" });
    }

    /// <summary>
    /// Webhook for Call Center status updates
    /// </summary>
    [HttpPost("call-center-status")]
    public async Task<ActionResult> HandleCallCenterStatus([FromBody] object payload)
    {
        _logger.LogInformation("Call Center Status Webhook received: {Payload}", JsonSerializer.Serialize(payload));
        
        _context.WebhookLogs.Add(new WebhookLog
        {
            Type = "call-center-status",
            Payload = JsonSerializer.Serialize(payload),
            ReceivedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "Webhook received" });
    }

    /// <summary>
    /// Webhook for Call End events
    /// </summary>
    [HttpPost("call-end")]
    public async Task<ActionResult> HandleCallEnd([FromBody] EndCallHookResponse payload)
    {
        _logger.LogInformation("Call End Webhook received: {Payload}", JsonSerializer.Serialize(payload));
        
        _context.WebhookLogs.Add(new WebhookLog
        {
            Type = "call-end",
            Payload = JsonSerializer.Serialize(payload),
            ReceivedAt = DateTime.UtcNow
        });

        // Process call end - log duration, billing, etc.
        _logger.LogInformation("Call ended - Agent: {Agent}, Number: {Number}, Duration: {Duration}s, Status: {Status}", 
            payload.AgentEmail, payload.Number, payload.Duration, payload.Status);

        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "Webhook received" });
    }

    /// <summary>
    /// Get all webhook logs (for demo/debugging)
    /// </summary>
    [HttpGet("logs")]
    public async Task<ActionResult<IEnumerable<WebhookLog>>> GetLogs()
    {
        var logs = await _context.WebhookLogs
            .OrderByDescending(l => l.ReceivedAt)
            .Take(50)
            .ToListAsync();
        return Ok(logs);
    }

    /// <summary>
    /// Clear webhook logs
    /// </summary>
    [HttpDelete("logs")]
    public async Task<ActionResult> ClearLogs()
    {
        _context.WebhookLogs.RemoveRange(_context.WebhookLogs);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Logs cleared" });
    }
}
