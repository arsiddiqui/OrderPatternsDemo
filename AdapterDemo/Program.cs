// AdapterDemo
// Pattern: Adapter
// Use case: Place an e-commerce order (same domain model reused across all demos).
//
// Tip: Each demo is a self-contained Console app. Run them individually.
// ------------------------------------------------------------

using OrderPatterns.OrderDomain;
using System;


// Adapter: make an incompatible API look like the interface you want.
// Same use case: payment processor, but we must integrate a legacy API that charges in cents.

sealed class LegacyPaymentApi
{
    // Can't change this signature (imagine it comes from an old DLL or third-party library).
    public void MakePaymentInCents(string legacyOrderRef, int cents) =>
        Console.WriteLine($"[LegacyPay] Charged {cents} cents for legacyRef={legacyOrderRef}");
}

// Adapter implements our domain interface by translating calls.
sealed class LegacyPaymentAdapter : IPaymentProcessor
{
    private readonly LegacyPaymentApi _legacy = new();
    public string Name => "LegacyPaymentAdapter";

    public void Charge(string orderId, decimal amount)
    {
        var cents = (int)Math.Round(amount * 100m);
        _legacy.MakePaymentInCents(orderId, cents);
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

        var shipping = new FlatRateShipping().CalculateShipping(order);
        var tax = new SimpleTax().CalculateTax(order.Subtotal + shipping, order.ShipTo);
        var receipt = OrderMath.BuildReceipt(order, shipping, tax);

        // Use adapter wherever an IPaymentProcessor is expected.
        IPaymentProcessor payment = new LegacyPaymentAdapter();
        payment.Charge(order.OrderId, receipt.Total);

        new ConsoleNotify().SendOrderConfirmation(order.Customer, receipt);
    }
}

static class SampleData
{
    public static Order CreateOrder() =>
        new Order(
            orderId: "ORD-8001",
            customer: new Customer("CUST-8", "Ashar", "ashar@example.com"),
            items: new[] { new OrderItem("SKU-USBHUB", "USB Hub", 1, 24.99m) },
            shipTo: new Address("123 Main St", "Wichita", "KS", "67202")
        );
}
