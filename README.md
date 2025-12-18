# OrderPatternsDemo (C# / .NET)

This solution demonstrates **10 classic design patterns** using the **same use case** every time:

> **Place an e-commerce order** (calculate shipping + tax, take payment, send confirmation)

## Projects

- **OrderDomain**: shared domain model and interfaces used by every demo
- **SingletonDemo** : The Singleton design pattern ensures that a class has only one instance throughout the application lifecycle and provides a global access point to that instance.
    In simple terms:
          No matter how many times you request the object, you always get the same instance.
- **FactoryMethodDemo**
- **AbstractFactoryDemo**
- **BuilderDemo**
- **StrategyDemo**
- **ObserverDemo**
- **DecoratorDemo**
- **AdapterDemo**
- **FacadeDemo**
- **RepositoryDemo**

## How to run

Open `OrderPatternsDemo.sln` in Visual Studio or Rider and run any demo project.

Or via CLI (if you have the .NET SDK installed):

```bash
dotnet run --project SingletonDemo/SingletonDemo.csproj
dotnet run --project FactoryMethodDemo/FactoryMethodDemo.csproj
# ...etc
```

Each `Program.cs` contains **heavy inline comments** explaining what the pattern is and where it is used.
