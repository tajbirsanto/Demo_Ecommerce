using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Demo_Ecommerce.Data;
using Demo_Ecommerce.Models;
using System.Text.Json;

namespace Demo_Ecommerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminController> _logger;

    public AdminController(AppDbContext context, IConfiguration configuration, ILogger<AdminController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Get admin dashboard stats
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult> GetDashboard()
    {
        var totalOrders = await _context.Orders.CountAsync();
        var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
        var confirmedOrders = await _context.Orders.CountAsync(o => o.Status == "Confirmed");
        var cancelledOrders = await _context.Orders.CountAsync(o => o.Status == "Cancelled");
        var totalRevenue = await _context.Orders.Where(o => o.Status == "Confirmed").SumAsync(o => o.TotalAmount);
        var totalProducts = await _context.Products.CountAsync();
        var recentOrders = await _context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .ToListAsync();

        return Ok(new
        {
            totalOrders,
            pendingOrders,
            confirmedOrders,
            cancelledOrders,
            totalRevenue,
            totalProducts,
            recentOrders
        });
    }

    /// <summary>
    /// Get all orders for admin
    /// </summary>
    [HttpGet("orders")]
    public async Task<ActionResult> GetOrders([FromQuery] string? status = null)
    {
        var query = _context.Orders.Include(o => o.Items).AsQueryable();
        
        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status.ToLower() == status.ToLower());
        
        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(orders);
    }

    /// <summary>
    /// Admin manually calls a specific customer
    /// </summary>
    [HttpPost("call-customer/{orderId}")]
    public async Task<ActionResult> CallCustomer(string orderId, [FromBody] AdminCallRequest? request = null)
    {
        var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
            return NotFound(new { message = "Order not found" });

        var apiKey = _configuration["ManyDial:ApiKey"];
        var callerId = _configuration["ManyDial:CallerId"] ?? "";
        var forwardNumber = _configuration["ManyDial:ForwardNumber"] ?? "+8801743681683";

        if (string.IsNullOrEmpty(apiKey))
            return BadRequest(new { message = "ManyDial API Key not configured" });

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

            // Natural conversational Bangla voice messages
            var customMessage = request?.Message ?? 
                $"আসসালামু আলাইকুম। {order.CustomerName} ভাই, কেমন আছেন? আমি টোটো কোম্পানি থেকে বলছি। আপনার অর্ডার সম্পর্কে জানাতে কল করলাম। আপনার অর্ডারের মোট মূল্য {order.TotalAmount} টাকা। অর্ডারটি কনফার্ম করতে চাইলে ১ নম্বর বাটন চাপুন। আমাদের কারো সাথে কথা বলতে চাইলে ২ নম্বর বাটন চাপুন। ধন্যবাদ।";

            var confirmMessage = request?.ConfirmMessage ??
                $"অনেক ধন্যবাদ, {order.CustomerName} ভাই। আপনার অর্ডারটি সফলভাবে কনফার্ম হয়ে গেছে। ইনশাআল্লাহ, আমরা খুব শীঘ্রই আপনার ঠিকানায় পণ্য পৌঁছে দেবো। টোটো কোম্পানির সাথে থাকার জন্য আপনাকে ধন্যবাদ। আল্লাহ হাফেজ।";

            var forwardMsg = "ঠিক আছে ভাই, দয়া করে একটু অপেক্ষা করুন। আপনাকে এখন আমাদের কাস্টমার সার্ভিস প্রতিনিধির সাথে কানেক্ট করা হচ্ছে। কিছুক্ষণের মধ্যেই কথা বলতে পারবেন।";

            var messages = $"{{\"welcome\":\"{EscapeJson(customMessage)}\",\"repeat\":\"2\",\"menuMessage1\":\"{EscapeJson(confirmMessage)}\",\"menuMessage2\":\"{EscapeJson(forwardMsg)}\",\"forwardNumber2\":\"{forwardNumber}\"}}";

            var buttons = "[{\"id\":\"menuMessage1\",\"key\":\"1\",\"value\":\"Confirm\"},{\"id\":\"menuMessage2\",\"key\":\"2\",\"value\":\"Talk to Agent\"}]";

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var formData = new MultipartFormDataContent
            {
                { new StringContent(order.Id), "callPayload" },
                { new StringContent(callerId), "callerId" },
                { new StringContent("3"), "perCallDuration" },
                { new StringContent(messages), "messages" },
                { new StringContent(order.CustomerPhone), "number" },
                { new StringContent(buttons), "buttons" },
                { new StringContent($"{baseUrl}/api/webhooks/call-delivery"), "deliveryHook" },
                { new StringContent("bn-BD"), "language" },
                { new StringContent("female"), "voice" }
            };

            _logger.LogInformation("Admin dispatching call to {Phone} for order {OrderId}", order.CustomerPhone, order.Id);

            var response = await httpClient.PostAsync("https://api.manydial.com/v1/portal/call/dispatch", formData);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Admin call response: {Content}", content);

            var success = content.Contains("\"success\":true");
            order.CallStatus = success ? "Admin Call Initiated" : $"Admin Call Failed: {content}";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success,
                message = success ? "Call dispatched successfully" : "Call dispatch failed",
                phone = order.CustomerPhone,
                orderId = order.Id,
                apiResponse = content
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin call failed for order {OrderId}", orderId);
            return StatusCode(500, new { message = $"Call failed: {ex.Message}" });
        }
    }

    /// <summary>
    /// Admin calls any phone number directly (not tied to an order)
    /// </summary>
    [HttpPost("call-direct")]
    public async Task<ActionResult> CallDirect([FromBody] DirectCallRequest request)
    {
        if (string.IsNullOrEmpty(request.Phone))
            return BadRequest(new { message = "Phone number is required" });

        var apiKey = _configuration["ManyDial:ApiKey"];
        var callerId = _configuration["ManyDial:CallerId"] ?? "";
        var forwardNumber = _configuration["ManyDial:ForwardNumber"] ?? "+8801743681683";

        if (string.IsNullOrEmpty(apiKey))
            return BadRequest(new { message = "ManyDial API Key not configured" });

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var welcomeMsg = request.Message ?? "আসসালামু আলাইকুম। আমি টোটো কোম্পানি থেকে বলছি। আপনাকে একটি জরুরি বিষয়ে জানাতে কল করেছি। আমাদের একজন প্রতিনিধির সাথে সরাসরি কথা বলতে চাইলে, অনুগ্রহ করে আপনার ফোনের ১ নম্বর বাটন চাপুন। ধন্যবাদ।";

            var forwardReply = "ঠিক আছে, দয়া করে একটু অপেক্ষা করুন। আপনাকে এখন আমাদের প্রতিনিধির সাথে কানেক্ট করা হচ্ছে। কিছুক্ষণের মধ্যেই কথা বলতে পারবেন।";

            var messages = $"{{\"welcome\":\"{EscapeJson(welcomeMsg)}\",\"repeat\":\"2\",\"menuMessage1\":\"{EscapeJson(forwardReply)}\",\"forwardNumber1\":\"{forwardNumber}\"}}";

            var buttons = "[{\"id\":\"menuMessage1\",\"key\":\"1\",\"value\":\"Connect\"}]";

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var formData = new MultipartFormDataContent
            {
                { new StringContent($"admin-direct-{DateTime.Now.Ticks}"), "callPayload" },
                { new StringContent(callerId), "callerId" },
                { new StringContent("3"), "perCallDuration" },
                { new StringContent(messages), "messages" },
                { new StringContent(request.Phone.StartsWith("+") ? request.Phone : $"+880{request.Phone}"), "number" },
                { new StringContent(buttons), "buttons" },
                { new StringContent($"{baseUrl}/api/webhooks/call-delivery"), "deliveryHook" },
                { new StringContent("bn-BD"), "language" },
                { new StringContent("female"), "voice" }
            };

            _logger.LogInformation("Admin direct call to {Phone}", request.Phone);

            var response = await httpClient.PostAsync("https://api.manydial.com/v1/portal/call/dispatch", formData);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Admin direct call response: {Content}", content);

            return Ok(new
            {
                success = content.Contains("\"success\":true"),
                message = content.Contains("\"success\":true") ? "Call dispatched" : "Call failed",
                phone = request.Phone,
                apiResponse = content
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin direct call failed to {Phone}", request.Phone);
            return StatusCode(500, new { message = $"Call failed: {ex.Message}" });
        }
    }

    /// <summary>
    /// Update order status manually
    /// </summary>
    [HttpPut("orders/{orderId}/status")]
    public async Task<ActionResult> UpdateOrderStatus(string orderId, [FromBody] UpdateStatusRequest request)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
            return NotFound(new { message = "Order not found" });

        order.Status = request.Status;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Status updated", orderId, status = request.Status });
    }

    /// <summary>
    /// Delete an order
    /// </summary>
    [HttpDelete("orders/{orderId}")]
    public async Task<ActionResult> DeleteOrder(string orderId)
    {
        var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
            return NotFound(new { message = "Order not found" });

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Order deleted" });
    }

    private string EscapeJson(string text)
    {
        return text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }
}

public class AdminCallRequest
{
    public string? Message { get; set; }
    public string? ConfirmMessage { get; set; }
}

public class DirectCallRequest
{
    public string Phone { get; set; } = string.Empty;
    public string? Message { get; set; }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
