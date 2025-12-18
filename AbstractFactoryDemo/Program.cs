// AbstractFactoryDemo
// Pattern: Abstract Factory
// Use case: Place an e-commerce order (same domain model reused across all demos).
//
// Tip: Each demo is a self-contained Console app. Run them individually.
// ------------------------------------------------------------

using OrderPatterns.OrderDomain;
using System;


// Abstract Factory: create families of related objects.
// Same checkout use case; we want a consistent set of services for a given "provider".

// Family products
sealed class SandboxPayment : IPaymentProcessor
{
    public string Name => "SandboxPay";
    public void Charge(string orderId, decimal amount) =>
        Console.WriteLine($"[{Name}] (NO-OP) Would charge {amount:C} for {orderId}");
}
sealed class ProdPayment : IPaymentProcessor
{
    public string Name => "ProdPay";
    public void Charge(string orderId, decimal amount) =>
        Console.WriteLine($"[{Name}] Charged {amount:C} for {orderId}");
}

sealed class SandboxNotify : INotificationSender
{
    public string Name => "SandboxNotify";
    public void SendOrderConfirmation(Customer customer, Receipt receipt) =>
        Console.WriteLine($"[{Name}] (NO-OP) Would email {customer.Email} total {receipt.Total:C}");
}
sealed class ProdNotify : INotificationSender
{
    public string Name => "ProdNotify";
    public void SendOrderConfirmation(Customer customer, Receipt receipt) =>
        Console.WriteLine($"[{Name}] Email sent to {customer.Email} total {receipt.Total:C}");
}

// Abstract factory
interface ICommerceProviderFactory
{
    IPaymentProcessor CreatePayment();
    INotificationSender CreateNotification();
}

// Concrete factories
sealed class SandboxProviderFactory : ICommerceProviderFactory
{
    public IPaymentProcessor CreatePayment() => new SandboxPayment();
    public INotificationSender CreateNotification() => new SandboxNotify();
}
sealed class ProductionProviderFactory : ICommerceProviderFactory
{
    public IPaymentProcessor CreatePayment() => new ProdPayment();
    public INotificationSender CreateNotification() => new ProdNotify();
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

sealed class CheckoutService
{
    private readonly IShippingRateCalculator _shipping;
    private readonly ITaxCalculator _tax;
    private readonly IPaymentProcessor _payment;
    private readonly INotificationSender _notify;

    public CheckoutService(ICommerceProviderFactory providerFactory)
    {
        // The factory guarantees consistent family: sandbox+mock, prod+real, etc.
        _payment = providerFactory.CreatePayment();
        _notify = providerFactory.CreateNotification();

        _shipping = new FlatRateShipping();
        _tax = new SimpleTax();
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

static class Program
{
    static void Main()
    {
        var order = SampleData.CreateOrder();

        // Flip this value to see the family swap.
        ICommerceProviderFactory factory = new SandboxProviderFactory();
        // ICommerceProviderFactory factory = new ProductionProviderFactory();

        var checkout = new CheckoutService(factory);
        checkout.PlaceOrder(order);
    }
}

static class SampleData
{
    public static Order CreateOrder() =>
        new Order(
            orderId: "ORD-3001",
            customer: new Customer("CUST-3", "Ashar", "ashar@example.com"),
            items: new[] { new OrderItem("SKU-BOOK", "Book", 1, 29.99m) },
            shipTo: new Address("123 Main St", "Wichita", "KS", "67202")
        );
}
