// StrategyDemo
// Pattern: Strategy
// Use case: Place an e-commerce order (same domain model reused across all demos).
//
// Tip: Each demo is a self-contained Console app. Run them individually.
// ------------------------------------------------------------

using OrderPatterns.OrderDomain;
using System;


// Strategy: swap algorithms at runtime without if/else explosion.
// Same use case: shipping calculation changes based on business rules.

sealed class GroundShipping : IShippingRateCalculator
{
    public string Name => "Ground";
    public decimal CalculateShipping(Order order) => 6.49m;
}
sealed class ExpressShipping : IShippingRateCalculator
{
    public string Name => "Express";
    public decimal CalculateShipping(Order order) => 18.99m;
}

sealed class SimpleTax : ITaxCalculator
{
    public string Name => "SimplePercent";
    public decimal CalculateTax(decimal taxableAmount, Address shipTo) => Math.Round(taxableAmount * 0.0825m, 2);
}

static class Program
{
    static void Main()
    {
        var order = SampleData.CreateOrder();

        // Business rule: if subtotal >= $100, ship via Ground for free; otherwise Ground paid.
        // Another rule: customer can choose Express.
        var customerWantsExpress = false;

        IShippingRateCalculator shippingStrategy =
            customerWantsExpress ? new ExpressShipping() :
            order.Subtotal >= 100m ? new FreeShippingDecorator(new GroundShipping()) :
            new GroundShipping();

        var shipping = shippingStrategy.CalculateShipping(order);
        var tax = new SimpleTax().CalculateTax(order.Subtotal + shipping, order.ShipTo);
        var receipt = OrderMath.BuildReceipt(order, shipping, tax);

        Console.WriteLine($"Shipping strategy: {shippingStrategy.Name}");
        Console.WriteLine($"Receipt total: {receipt.Total:C}");
    }

    // Small helper (not the Decorator pattern demo; just a convenience to show a "free shipping" option).
    private sealed class FreeShippingDecorator : IShippingRateCalculator
    {
        private readonly IShippingRateCalculator _inner;
        public FreeShippingDecorator(IShippingRateCalculator inner) => _inner = inner;
        public string Name => _inner.Name + " (Free)";
        public decimal CalculateShipping(Order order) => 0m;
    }
}

static class SampleData
{
    public static Order CreateOrder() =>
        new Order(
            orderId: "ORD-5001",
            customer: new Customer("CUST-5", "Ashar", "ashar@example.com"),
            items: new[]
            {
                new OrderItem("SKU-HEADSET", "Headset", 1, 79.99m),
                new OrderItem("SKU-WEBCAM", "Webcam", 1, 49.99m),
            },
            shipTo: new Address("123 Main St", "Wichita", "KS", "67202")
        );
}
