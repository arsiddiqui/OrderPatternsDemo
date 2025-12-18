// BuilderDemo
// Pattern: Builder
// Use case: Place an e-commerce order (same domain model reused across all demos).
//
// Tip: Each demo is a self-contained Console app. Run them individually.
// ------------------------------------------------------------

using OrderPatterns.OrderDomain;
using System;


// Builder: construct a complex object step-by-step (fluent, readable).
// Same use case: creating an Order to place.

// Builder specifically for Order creation.
sealed class OrderBuilder
{
    private string _orderId = "ORD-NEW";
    private Customer? _customer;
    private Address? _shipTo;
    private readonly List<OrderItem> _items = new();

    public OrderBuilder WithOrderId(string orderId) { _orderId = orderId; return this; }
    public OrderBuilder ForCustomer(string id, string name, string email) { _customer = new Customer(id, name, email); return this; }
    public OrderBuilder ShipTo(string line1, string city, string state, string postal, string country = "US")
    {
        _shipTo = new Address(line1, city, state, postal, country);
        return this;
    }
    public OrderBuilder AddItem(string sku, string name, int qty, decimal price)
    {
        _items.Add(new OrderItem(sku, name, qty, price));
        return this;
    }

    public Order Build()
    {
        if (_customer is null) throw new InvalidOperationException("Customer must be set.");
        if (_shipTo is null) throw new InvalidOperationException("Shipping address must be set.");
        return new Order(_orderId, _customer, _items, _shipTo);
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
        Console.WriteLine($"Receipt sent to {customer.Email} total {receipt.Total:C}");
}

static class Program
{
    static void Main()
    {
        // Build the order in a readable way
        var order =
            new OrderBuilder()
                .WithOrderId("ORD-4001")
                .ForCustomer("CUST-4", "Ashar", "ashar@example.com")
                .ShipTo("123 Main St", "Wichita", "KS", "67202")
                .AddItem("SKU-ROUTER", "Wi-Fi Router", 1, 119.99m)
                .AddItem("SKU-CABLE", "Ethernet Cable", 3, 6.99m)
                .Build();

        Console.WriteLine(order);

        // Place order (same checkout math)
        var shipping = new FlatRateShipping().CalculateShipping(order);
        var tax = new SimpleTax().CalculateTax(order.Subtotal + shipping, order.ShipTo);
        var receipt = OrderMath.BuildReceipt(order, shipping, tax);

        new ConsolePayment().Charge(order.OrderId, receipt.Total);
        new ConsoleNotify().SendOrderConfirmation(order.Customer, receipt);
    }
}
