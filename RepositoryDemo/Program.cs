// RepositoryDemo
// Pattern: Repository
// Use case: Place an e-commerce order (same domain model reused across all demos).
//
// Tip: Each demo is a self-contained Console app. Run them individually.
// ------------------------------------------------------------

using OrderPatterns.OrderDomain;
using System;


// Repository: abstract data access so business logic doesn't care where data is stored.
// Same use case: place an order and store it; later load it by ID.

interface IOrderRepository
{
    void Save(Order order);
    Order? GetById(string orderId);
}

sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly Dictionary<string, Order> _db = new(StringComparer.OrdinalIgnoreCase);

    public void Save(Order order) => _db[order.OrderId] = order;

    public Order? GetById(string orderId) =>
        _db.TryGetValue(orderId, out var order) ? order : null;
}

sealed class FlatRateShipping : IShippingRateCalculator
{
    public string Name => "Flat";
    public decimal CalculateShipping(Order order) => 7.99m;
}
sealed class SimpleTax : ITaxCalculator
{
    public string Name => "SimplePercent";
    public decimal CalculateTax(decimal taxableAmount, Address shipTo) => Math.Round(taxableAmount * 0.0825m, 2);
}
sealed class ConsolePayment : IPaymentProcessor
{
    public string Name => "ConsolePay";
    public void Charge(string orderId, decimal amount) => Console.WriteLine($"Charged {amount:C} for {orderId}");
}
sealed class ConsoleNotify : INotificationSender
{
    public string Name => "ConsoleNotify";
    public void SendOrderConfirmation(Customer customer, Receipt receipt) =>
        Console.WriteLine($"Email to {customer.Email}: total {receipt.Total:C}");
}

sealed class OrderService
{
    private readonly IOrderRepository _repo;
    private readonly IShippingRateCalculator _shipping = new FlatRateShipping();
    private readonly ITaxCalculator _tax = new SimpleTax();
    private readonly IPaymentProcessor _payment = new ConsolePayment();
    private readonly INotificationSender _notify = new ConsoleNotify();

    public OrderService(IOrderRepository repo) => _repo = repo;

    public Receipt PlaceOrder(Order order)
    {
        // Business logic
        var shipping = _shipping.CalculateShipping(order);
        var tax = _tax.CalculateTax(order.Subtotal + shipping, order.ShipTo);
        var receipt = OrderMath.BuildReceipt(order, shipping, tax);

        _payment.Charge(order.OrderId, receipt.Total);
        _notify.SendOrderConfirmation(order.Customer, receipt);

        // Persist via repository abstraction
        _repo.Save(order);

        return receipt;
    }

    public Order? FindOrder(string orderId) => _repo.GetById(orderId);
}

static class Program
{
    static void Main()
    {
        var repo = new InMemoryOrderRepository();
        var service = new OrderService(repo);

        var order = SampleData.CreateOrder();
        service.PlaceOrder(order);

        var loaded = service.FindOrder(order.OrderId);
        Console.WriteLine($"Loaded from repo: {loaded}");
    }
}

static class SampleData
{
    public static Order CreateOrder() =>
        new Order(
            orderId: "ORD-10001",
            customer: new Customer("CUST-10", "Ashar", "ashar@example.com"),
            items: new[]
            {
                new OrderItem("SKU-BOOK", "Book", 2, 29.99m),
                new OrderItem("SKU-PEN", "Pen", 5, 1.99m),
            },
            shipTo: new Address("123 Main St", "Wichita", "KS", "67202")
        );
}
