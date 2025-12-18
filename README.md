# OrderPatternsDemo (C# / .NET)

This solution demonstrates **10 classic design patterns** using the **same use case** every time:

> **Place an e-commerce order** (calculate shipping + tax, take payment, send confirmation)

## Projects

- **OrderDomain**: shared domain model and interfaces used by every demo
- **SingletonDemo** : The Singleton design pattern ensures that a class has only one instance throughout the application lifecycle and provides a global access point to that instance.
    > In simple terms:
          No matter how many times you request the object, you always get the same instance.
- **FactoryMethodDemo** : The Factory Method pattern defines an interface or abstract method for creating objects, but lets subclasses decide which concrete class to instantiate.
    > In simple terms:
                                You delegate object creation to a factory method instead of calling new directly.
- **AbstractFactoryDemo**: The Abstract Factory pattern provides an interface for creating families of related or dependent objects without specifying their concrete classes.

    > In simple terms: It’s a factory of factories that creates related objects together.
- **BuilderDemo** :The Builder pattern separates the construction of a complex object from its representation, allowing the same construction process to create different variations of an object.

    > In simple terms: It lets you build an object step by step instead of using a large constructor.
- **StrategyDemo** : The Strategy pattern defines a family of algorithms, encapsulates each one, and makes them interchangeable at runtime without changing the client code.

    > In simple terms: You select a behavior at runtime instead of hard-coding conditional logic.
- **ObserverDemo**: The Observer pattern defines a one-to-many dependency between objects so that when one object (the subject) changes state, all its dependent objects (observers) are notified automatically.

    > In simple terms: Observers “subscribe” to a subject and get notified when something changes.
- **DecoratorDemo** : The Decorator pattern allows you to add new behavior to an object dynamically at runtime without modifying its existing code.

    > In simple terms: You “wrap” an object to extend its behavior instead of subclassing it.
- **AdapterDemo**: The Adapter pattern allows incompatible interfaces to work together by converting the interface of an existing class into one that the client expects.

    > In simple terms: It acts like a translator between two incompatible systems.
- **FacadeDemo**: The Facade pattern provides a simplified, unified interface to a complex subsystem, making it easier for clients to use.

    > In simple terms: It hides complexity behind a single, easy-to-use interface.
- **RepositoryDemo**: The Repository pattern abstracts the data access layer and provides a collection-like interface for accessing domain objects, hiding the details of how data is stored or retrieved.

    > In simple terms: It separates business logic from database access.

## How to run

Open `OrderPatternsDemo.sln` in Visual Studio or Rider and run any demo project.

Or via CLI (if you have the .NET SDK installed):

```bash
dotnet run --project SingletonDemo/SingletonDemo.csproj
dotnet run --project FactoryMethodDemo/FactoryMethodDemo.csproj
# ...etc
```

Each `Program.cs` contains **heavy inline comments** explaining what the pattern is and where it is used.
