// FacadeDemo
// Pattern: Facade
// Use case: Place an e-commerce order (same domain model reused across all demos).
//
// Tip: Each demo is a self-contained Console app. Run them individually.
// ------------------------------------------------------------

using OrderPatterns.OrderDomain;
using System;


// Facade: provide a simple API over a complex subsystem.
// Same use case: placing an order requires shipping + tax + payment + notification.
// Facade makes it one call: checkout.Place(order).

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

/// <summary>
/// Facade that hides orchestration details.
/// </summary>
sealed class CheckoutFacade
{
    private readonly IShippingRateCalculator _shipping;
    private readonly ITaxCalculator _tax;
    private readonly IPaymentProcessor _payment;
    private readonly INotificationSender _notify;

    public CheckoutFacade(IShippingRateCalculator shipping, ITaxCalculator tax, IPaymentProcessor payment, INotificationSender notify)
    {
        _shipping = shipping; _tax = tax; _payment = payment; _notify = notify;
    }

    public Receipt Place(Order order)
    {
        var shipping = _shipping.CalculateShipping(order);
        var tax = _tax.CalculateTax(order.Subtotal + shipping, order.ShipTo);
        var receipt = OrderMath.BuildReceipt(order, shipping, tax);

        _payment.Charge(order.OrderId, receipt.Total);
        _notify.SendOrderConfirmation(order.Customer, receipt);

        return receipt;
    }
}

static class Program
{
    static void Main()
    {
        var order = SampleData.CreateOrder();

        // Client code stays simple
        var checkout = new CheckoutFacade(
            new FlatRateShipping(),
            new SimpleTax(),
            new ConsolePayment(),
            new ConsoleNotify());

        var receipt = checkout.Place(order);

        Console.WriteLine($"Facade result: {receipt.Total:C}");
    }
}

static class SampleData
{
    public static Order CreateOrder() =>
        new Order(
            orderId: "ORD-9001",
            customer: new Customer("CUST-9", "Ashar", "ashar@example.com"),
            items: new[] { new OrderItem("SKU-MIC", "Microphone", 1, 59.99m) },
            shipTo: new Address("123 Main St", "Wichita", "KS", "67202")
        );
}
