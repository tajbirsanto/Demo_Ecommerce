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
            var callerId = _configuration["ManyDial:CallerId"] ?? "";
            
            var callRequest = new CallAutomationRequest
            {
                CallPayload = order.Id,
                CallerId = callerId,
                PerCallDuration = "3",
                Number = order.CustomerPhone,
                DeliveryHook = $"{baseUrl}/api/webhooks/call-delivery",
                Messages = new Dictionary<string, string>
                {
                    { "welcome", $"Assalamu Alaikum {order.CustomerName}, Toto Company theke apnake dhonnobad. Apnar order er total holo {order.TotalAmount} Taka. Order confirm korte 1 press korun, cancel korte 2 press korun." },
                    { "repeat", "2" },
                    { "sms", $"Toto Company te order korar jonno dhonnobad! Order ID: {order.Id}, Total: {order.TotalAmount} Taka" },
                    { "menuMessage1", "Apnar order confirm hoye geche. Amra khub shighroi delivery dibo. Toto Company er shathe thakaar jonno dhonnobad!" },
                    { "sms1", $"Order {order.Id} confirm hoyeche! Amra shighroi delivery dibo. Dhonnobad!" },
                    { "menuMessage2", "Apnar order cancel kora hoyeche. Kono proshno thakle amader customer support e call korun. Dhonnobad." },
                    { "sms2", $"Order {order.Id} cancel kora hoyeche apnar request onujayi." }
                },
                Buttons = new List<CallButton>
                {
                    new CallButton { Id = "menuMessage1", Key = "1", Value = "Confirm Order" },
                    new CallButton { Id = "menuMessage2", Key = "2", Value = "Cancel Order" }
                }
            };

            var result = await _manyDialService.DispatchCallAsync(callRequest);
            order.CallStatus = result.Success ? "Call Initiated" : "Call Failed";
            order.CallPayload = order.Id;
            
            _logger.LogInformation("Call dispatch result for order {OrderId}: {Success} - {Message}", 
                order.Id, result.Success, result.Message);
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
