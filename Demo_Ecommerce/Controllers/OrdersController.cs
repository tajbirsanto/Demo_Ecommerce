using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Demo_Ecommerce.Data;
using Demo_Ecommerce.Models;
using Demo_Ecommerce.Services;
using System.Text.Json;

namespace Demo_Ecommerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IManyDialService _manyDialService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(AppDbContext context, IManyDialService manyDialService, IConfiguration configuration, ILogger<OrdersController> logger)
    {
        _context = context;
        _manyDialService = manyDialService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetAll()
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetById(string id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order == null)
            return NotFound();
        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderDto orderDto)
    {
        var order = new Order
        {
            Id = Guid.NewGuid().ToString(),
            CustomerName = orderDto.CustomerName,
            CustomerEmail = orderDto.CustomerEmail,
            CustomerPhone = orderDto.CustomerPhone,
            ShippingAddress = orderDto.ShippingAddress,
            TotalAmount = orderDto.TotalAmount,
            CreatedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        // Convert CartItemDto to OrderItem
        foreach (var item in orderDto.Items)
        {
            order.Items.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Price = item.Price,
                Quantity = item.Quantity,
                ImageUrl = item.ImageUrl
            });
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order created: {OrderId}", order.Id);

        // Trigger automated call to confirm order using ManyDial
        await TriggerOrderConfirmationCall(order);
        await _context.SaveChangesAsync(); // Save call status

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    private async Task TriggerOrderConfirmationCall(Order order)
    {
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var apiKey = _configuration["ManyDial:ApiKey"];
            var callerId = _configuration["ManyDial:CallerId"] ?? "";
            
            // Log configuration for debugging
            _logger.LogInformation("ManyDial Config - ApiKey exists: {HasKey}, CallerId: {CallerId}", 
                !string.IsNullOrEmpty(apiKey), callerId);
            
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("ManyDial API Key is not configured!");
                order.CallStatus = "Call Failed - No API Key";
                return;
            }
            
            if (string.IsNullOrEmpty(callerId))
            {
                _logger.LogError("ManyDial Caller ID is not configured!");
                order.CallStatus = "Call Failed - No Caller ID";
                return;
            }
            
            // Use direct HttpClient call - same approach that works in test endpoint
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            
            // Build messages JSON with natural conversational Bangla voice
            // When customer presses 2, call is forwarded to the configured number
            var forwardNumber = _configuration["ManyDial:ForwardNumber"] ?? "+8801743681683";
            
            // Natural, warm Bangla voice script — sounds like a real person calling
            var welcomeMsg = $"আসসালামু আলাইকুম। {order.CustomerName} ভাই, কেমন আছেন? আমি টোটো কোম্পানি থেকে বলছি। আপনি আমাদের কাছে একটি অর্ডার দিয়েছেন, যার মোট মূল্য {order.TotalAmount} টাকা। আপনার অর্ডারটি কনফার্ম করতে চাইলে, অনুগ্রহ করে আপনার ফোনের ১ নম্বর বাটন চাপুন। আর যদি আমাদের কারো সাথে কথা বলতে চান, তাহলে ২ নম্বর বাটন চাপুন। ধন্যবাদ।";

            var confirmMsg = $"অনেক ধন্যবাদ, {order.CustomerName} ভাই। আপনার অর্ডারটি সফলভাবে কনফার্ম হয়ে গেছে। ইনশাআল্লাহ, আমরা খুব শীঘ্রই আপনার ঠিকানায় পণ্য পৌঁছে দেবো। টোটো কোম্পানির সাথে থাকার জন্য আপনাকে আবারও ধন্যবাদ। আল্লাহ হাফেজ।";

            var forwardMsg = "ঠিক আছে ভাই, দয়া করে একটু অপেক্ষা করুন। আপনাকে এখন আমাদের কাস্টমার সার্ভিস প্রতিনিধির সাথে কানেক্ট করা হচ্ছে। কিছুক্ষণের মধ্যেই কথা বলতে পারবেন।";

            var messages = $"{{\"welcome\":\"{EscapeJson(welcomeMsg)}\",\"repeat\":\"2\",\"menuMessage1\":\"{EscapeJson(confirmMsg)}\",\"menuMessage2\":\"{EscapeJson(forwardMsg)}\",\"forwardNumber2\":\"{forwardNumber}\"}}";
            
            var buttons = "[{\"id\":\"menuMessage1\",\"key\":\"1\",\"value\":\"Confirm\"},{\"id\":\"menuMessage2\",\"key\":\"2\",\"value\":\"Support\"}]";
            
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

            _logger.LogInformation("Dispatching call to {Number} for order {OrderId} with CallerId {CallerId}", 
                order.CustomerPhone, order.Id, callerId);

            var response = await httpClient.PostAsync("https://api.manydial.com/v1/portal/call/dispatch", formData);
            var content = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("Call dispatch response for order {OrderId}: {Content}", order.Id, content);
            
            order.CallStatus = content.Contains("\"success\":true") ? "Call Initiated" : $"Call Failed: {content}";
            order.CallPayload = order.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger confirmation call for order {OrderId}", order.Id);
            order.CallStatus = "Call Error";
        }
    }

    [HttpPost("{id}/resend-call")]
    public async Task<ActionResult> ResendConfirmationCall(string id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return NotFound();

        await TriggerOrderConfirmationCall(order);
        await _context.SaveChangesAsync();
        
        return Ok(new { message = "Confirmation call resent", order.CallStatus });
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult> UpdateStatus(string id, [FromBody] string status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return NotFound();

        order.Status = status;
        await _context.SaveChangesAsync();
        
        return Ok(order);
    }

    private string EscapeJson(string text)
    {
        return text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }
}
