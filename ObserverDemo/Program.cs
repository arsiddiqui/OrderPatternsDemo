// ObserverDemo
// Pattern: Observer (Publish/Subscribe)
// Use case: Place an e-commerce order (same domain model reused across all demos).
//
// Tip: Each demo is a self-contained Console app. Run them individually.
// ------------------------------------------------------------

using OrderPatterns.OrderDomain;
using System;


// Observer: one-to-many notifications when something happens.
// Same use case: when an order is placed, multiple subscribers react (email, audit log, etc.).

interface IOrderObserver
{
    void OnOrderEvent(OrderEvent evt);
}

sealed class OrderEventPublisher
{
    private readonly List<IOrderObserver> _observers = new();

    public void Subscribe(IOrderObserver observer) => _observers.Add(observer);
    public void Unsubscribe(IOrderObserver observer) => _observers.Remove(observer);

    public void Publish(OrderEvent evt)
    {
        foreach (var o in _observers)
            o.OnOrderEvent(evt);
    }
}

sealed class EmailObserver : IOrderObserver
{
    public void OnOrderEvent(OrderEvent evt) =>
        Console.WriteLine($"[Email] {evt.Type} for {evt.OrderId}: {evt.Message}");
}

sealed class AuditObserver : IOrderObserver
{
    public void OnOrderEvent(OrderEvent evt) =>
        Console.WriteLine($"[Audit] {evt.Utc:u} {evt.Type} {evt.OrderId} :: {evt.Message}");
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

sealed class CheckoutService
{
    private readonly OrderEventPublisher _events;
    private readonly IShippingRateCalculator _shipping = new FlatRateShipping();
    private readonly ITaxCalculator _tax = new SimpleTax();
    private readonly IPaymentProcessor _pay = new ConsolePayment();

    public CheckoutService(OrderEventPublisher events) => _events = events;

    public Receipt PlaceOrder(Order order)
    {
        _events.Publish(new OrderEvent(order.OrderId, "OrderPlaced", DateTimeOffset.UtcNow, "Checkout started"));

        var shipping = _shipping.CalculateShipping(order);
        _events.Publish(new OrderEvent(order.OrderId, "ShippingCalculated", DateTimeOffset.UtcNow, $"{shipping:C}"));

        var tax = _tax.CalculateTax(order.Subtotal + shipping, order.ShipTo);
        _events.Publish(new OrderEvent(order.OrderId, "TaxCalculated", DateTimeOffset.UtcNow, $"{tax:C}"));

        var receipt = OrderMath.BuildReceipt(order, shipping, tax);

        _pay.Charge(order.OrderId, receipt.Total);
        _events.Publish(new OrderEvent(order.OrderId, "PaymentCaptured", DateTimeOffset.UtcNow, $"{receipt.Total:C}"));

        _events.Publish(new OrderEvent(order.OrderId, "OrderComplete", DateTimeOffset.UtcNow, "Checkout finished"));
        return receipt;
    }
}

static class Program
{
    static void Main()
    {
        var events = new OrderEventPublisher();
        events.Subscribe(new EmailObserver());
        events.Subscribe(new AuditObserver());

        var order = SampleData.CreateOrder();
        var checkout = new CheckoutService(events);
        checkout.PlaceOrder(order);
    }
}

static class SampleData
{
    public static Order CreateOrder() =>
        new Order(
            orderId: "ORD-6001",
            customer: new Customer("CUST-6", "Ashar", "ashar@example.com"),
            items: new[] { new OrderItem("SKU-MONITOR", "Monitor", 1, 159.99m) },
            shipTo: new Address("123 Main St", "Wichita", "KS", "67202")
        );
}
