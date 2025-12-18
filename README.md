# OrderPatternsDemo (C# / .NET)

This solution demonstrates **10 classic design patterns** using the **same use case** every time:

> **Place an e-commerce order** (calculate shipping + tax, take payment, send confirmation)

## Projects

- **OrderDomain**: shared domain model and interfaces used by every demo
- **SingletonDemo**
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
