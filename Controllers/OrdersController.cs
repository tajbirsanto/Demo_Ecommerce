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
            
            // Use direct HttpClient call - same approach that works in test endpoint
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            
            // Build messages JSON with actual Bangla text
            var messages = $"{{\"welcome\":\"আসসালামু আলাইকুম {order.CustomerName}, টোটো কোম্পানি থেকে আপনাকে ধন্যবাদ। আপনার অর্ডারের মোট মূল্য {order.TotalAmount} টাকা। অর্ডার কনফার্ম করতে ১ চাপুন, কাস্টমার কেয়ার এর সাথে কথা বলতে ২ চাপুন।\",\"repeat\":\"2\",\"menuMessage1\":\"আপনার অর্ডার কনফার্ম হয়ে গেছে। আমরা শীঘ্রই ডেলিভারি দিব। ধন্যবাদ!\",\"menuMessage2\":\"ধন্যবাদ। আপনাকে কাস্টমার কেয়ার এর সাথে কানেক্ট করা হচ্ছে।\"}}";
            
            var buttons = "[{\"id\":\"menuMessage1\",\"key\":\"1\",\"value\":\"Confirm\"},{\"id\":\"menuMessage2\",\"key\":\"2\",\"value\":\"Support\"}]";
            
            var formData = new MultipartFormDataContent
            {
                { new StringContent(order.Id), "callPayload" },
                { new StringContent(callerId), "callerId" },
                { new StringContent("2"), "perCallDuration" },
                { new StringContent(messages), "messages" },
                { new StringContent(order.CustomerPhone), "number" },
                { new StringContent(buttons), "buttons" },
                { new StringContent($"{baseUrl}/api/webhooks/call-delivery"), "deliveryHook" }
            };

            _logger.LogInformation("Dispatching call to {Number} for order {OrderId}", order.CustomerPhone, order.Id);

            var response = await httpClient.PostAsync("https://api.manydial.com/v1/portal/call/dispatch", formData);
            var content = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("Call dispatch response for order {OrderId}: {Content}", order.Id, content);
            
            order.CallStatus = content.Contains("\"success\":true") ? "Call Initiated" : "Call Failed";
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
}
