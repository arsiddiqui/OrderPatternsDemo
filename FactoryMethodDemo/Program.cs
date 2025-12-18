// FactoryMethodDemo
// Pattern: Factory Method
// Use case: Place an e-commerce order (same domain model reused across all demos).
//
// Tip: Each demo is a self-contained Console app. Run them individually.
// ------------------------------------------------------------

using OrderPatterns.OrderDomain;
using System;


// Factory Method: defer creation of a product (here: payment processor) to subclasses.
// Same checkout use case; only payment creation varies.

// "Product"
sealed class CardPayment : IPaymentProcessor
{
    public string Name => "CardPayment";
    public void Charge(string orderId, decimal amount) =>
        Console.WriteLine($"[{Name}] Charged card for {orderId}: {amount:C}");
}
sealed class InvoicePayment : IPaymentProcessor
{
    public string Name => "InvoicePayment";
    public void Charge(string orderId, decimal amount) =>
        Console.WriteLine($"[{Name}] Created invoice for {orderId}: {amount:C}");
}

// "Creator"
abstract class PaymentCreator
{
    public void CollectPayment(Order order, decimal total)
    {
        // creator controls "when" payment happens,
        // subclasses decide "what concrete payment processor to use"
        var processor = CreatePaymentProcessor(order);
        processor.Charge(order.OrderId, total);
    }

    protected abstract IPaymentProcessor CreatePaymentProcessor(Order order);
}

// Concrete creators
sealed class CardPaymentCreator : PaymentCreator
{
    protected override IPaymentProcessor CreatePaymentProcessor(Order order) => new CardPayment();
}
sealed class InvoicePaymentCreator : PaymentCreator
{
    protected override IPaymentProcessor CreatePaymentProcessor(Order order) => new InvoicePayment();
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

static class Program
{
    static void Main()
    {
        var order = SampleData.CreateOrder();

        var shipping = new FlatRateShipping().CalculateShipping(order);
        var tax = new SimpleTax().CalculateTax(order.Subtotal + shipping, order.ShipTo);
        var receipt = OrderMath.BuildReceipt(order, shipping, tax);

        // Choose the creator based on a "business rule":
        // Example rule: B2B customers pay via invoice, others pay by card.
        PaymentCreator creator = order.Customer.CustomerId.StartsWith("B2B", StringComparison.OrdinalIgnoreCase)
            ? new InvoicePaymentCreator()
            : new CardPaymentCreator();

        creator.CollectPayment(order, receipt.Total);

        Console.WriteLine($"Receipt Total: {receipt.Total:C}");
    }
}

static class SampleData
{
    public static Order CreateOrder() =>
        new Order(
            orderId: "ORD-2001",
            customer: new Customer("CUST-2", "Ashar", "ashar@example.com"),
            items: new[]
            {
                new OrderItem("SKU-SSD", "SSD", 1, 89.99m),
                new OrderItem("SKU-CABLE", "USB-C Cable", 2, 9.99m),
            },
            shipTo: new Address("123 Main St", "Wichita", "KS", "67202")
        );
}
