// DecoratorDemo
// Pattern: Decorator
// Use case: Place an e-commerce order (same domain model reused across all demos).
//
// Tip: Each demo is a self-contained Console app. Run them individually.
// ------------------------------------------------------------

using OrderPatterns.OrderDomain;
using System;


// Decorator: add behavior without changing the wrapped class.
// Same use case: place an order, but add logging/timing around it.

interface IOrderService
{
    Receipt PlaceOrder(Order order);
}

sealed class CoreOrderService : IOrderService
{
    private readonly IShippingRateCalculator _shipping;
    private readonly ITaxCalculator _tax;
    private readonly IPaymentProcessor _payment;
    private readonly INotificationSender _notify;

    public CoreOrderService(IShippingRateCalculator shipping, ITaxCalculator tax, IPaymentProcessor payment, INotificationSender notify)
    {
        _shipping = shipping; _tax = tax; _payment = payment; _notify = notify;
    }

    public Receipt PlaceOrder(Order order)
    {
        var shipping = _shipping.CalculateShipping(order);
        var tax = _tax.CalculateTax(order.Subtotal + shipping, order.ShipTo);
        var receipt = OrderMath.BuildReceipt(order, shipping, tax);

        _payment.Charge(order.OrderId, receipt.Total);
        _notify.SendOrderConfirmation(order.Customer, receipt);

        return receipt;
    }
}

// Decorator #1: logging
sealed class LoggingOrderService : IOrderService
{
    private readonly IOrderService _inner;
    public LoggingOrderService(IOrderService inner) => _inner = inner;

    public Receipt PlaceOrder(Order order)
    {
        Console.WriteLine($"[LOG] Starting order {order.OrderId}...");
        var receipt = _inner.PlaceOrder(order);
        Console.WriteLine($"[LOG] Completed order {order.OrderId}. Total={receipt.Total:C}");
        return receipt;
    }
}

// Decorator #2: timing
sealed class TimingOrderService : IOrderService
{
    private readonly IOrderService _inner;
    public TimingOrderService(IOrderService inner) => _inner = inner;

    public Receipt PlaceOrder(Order order)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var receipt = _inner.PlaceOrder(order);
        sw.Stop();
        Console.WriteLine($"[METRIC] PlaceOrder took {sw.ElapsedMilliseconds}ms");
        return receipt;
    }
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

static class Program
{
    static void Main()
    {
        var order = SampleData.CreateOrder();

        IOrderService core = new CoreOrderService(new FlatRateShipping(), new SimpleTax(), new ConsolePayment(), new ConsoleNotify());

        // Wrap core with decorators (stackable)
        IOrderService decorated = new TimingOrderService(new LoggingOrderService(core));

        decorated.PlaceOrder(order);
    }
}

static class SampleData
{
    public static Order CreateOrder() =>
        new Order(
            orderId: "ORD-7001",
            customer: new Customer("CUST-7", "Ashar", "ashar@example.com"),
            items: new[] { new OrderItem("SKU-LAPTOPSTAND", "Laptop Stand", 1, 34.99m) },
            shipTo: new Address("123 Main St", "Wichita", "KS", "67202")
        );
}
