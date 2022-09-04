# Autoinjector

## Automatically inject services using attributes

There are several Nuget packages that will load services automatically. However, most of these do things that I don't like:

1. **loading services by default** can lead to injecting services you don't want in DI. There are remedies for this, but loading by default won't tell you if you did something you didn't mean to do. 
2. **loading services by namespace** can lead to an unintuitive namespace structure just to make the DI framework happy.
3. **ignoring services by attribute** is a solution to #1, but again, this is easy to overlook and there's no way to know if you forgot to *ignore* a service.  This can load extra classes into DI if you don't ignore a service. This can also lead to a much larger attribute footprint in your app, dependening on your application structure, because you might need to suppress things like DTOs or data models.

I prefer to do things explicitly where possible, and automate where practical. By explicitly loading services, you still retain ultimate control over whether a service is injected or not. More to the point, you also get runtime feedback if you forget to register a service, whereas if services are loaded by default, you may forget to suppress loading classes into DI without error.

## Installation

You can install Autoinjector by running the following command:

```bash
dotnet add package Autoinjector
```

## Setup

There are two ways to set up Autoinjector.

### Method #1: Manually specify assemblies to scan (recommended)

If you're okay with a little manual work, you can cut down on some startup overhead by specifying which assemblies you want to scan for services.

```csharp
var app = WebApplication.CreateBuilder(args);

app.Services.AutoinjectServicesFromAssembly(typeof(Program).Assembly);
```

The `AutoinjectServicesFromAssembly()` extension method returns the `IServiceCollection`, so you can chain multiple calls together if need be. Say you have two projects, one for your web layer and one for your service layer:

```csharp
var app = WebApplication.CreateBuilder(args);

app.Services.AutoinjectServicesFromAssembly(typeof(MyService).Assembly)
            .AutoinjectServicesFromAssembly(typeof(Program).Assembly);
```

### Method #2: Automatically scan all assemblies for services (not recommended)

If you just want to autoinject and get it over with, you can scan all visible assemblies for services by using the `AutoinjectServices()` extension method:

```csharp
var app = WebApplication.CreateBuilder(args);

app.Services.AutoinjectServices();
```

This method is **not recommended**. This will scan *all visible assemblies* looking for services to inject, leading to iteration over dozens of assemblies at startup. This will add unnecessary overhead to your startup process since only a few assemblies at most will actually have services.

But I ain't yer papa, so have at it, if you insist.

## Registering services in DI

Now that you've set up your project to use Autoinjector, you can start injecting services. To do that, you decorate your service classes with `[Service]`.

### [Service] Attribute

The `ServiceAttribute` class has two public properties that we care about.

#### ServiceAttribute.Lifetime (default: `ServiceLifetime.Scoped`)

The `ServiceAttribute.Lifetime` property represents the lifetime of the service in DI. It is represented by the `Microsoft.Extensions.DependencyInjection.ServiceLifetime` enum, so it can accept any valid value on that enum.

#### ServiceAttribute.Types (default: `null`)

The `ServiceAttribute.Types` property represents an array of the Type(s) to register a service as in DI. If this array is null or empty, the service class is injected as its own type (i.e., `MyService` will be injected into DI as `MyService`). Otherwise, the service is injected into DI as each of the Types in the array.

### Using `[Service]`

There are four different constructors for `[Service]`, allowing you to fine-tune how you want to inject your service.

#### `ServiceAttribute(ServiceLifetime, params Type[])` (recommended)

This constructor gives you the most control over how your service is injected. It allows you to specify the lifetime of the service, and also allows you to specify which Type(s) your service should be injected as.

Assume you're building a login system. You have two interfaces that help out with the password part of your login system: `IPasswordHasher` for hashing user passwords, and `IPasswordValidator` for validating that passwords meet your security requirements.

Now, imagine you have a `DefaultPasswordValidator : IPasswordValidator` and you want to register this in DI as a transient service of type `IPasswordValidator`. To do so:

```csharp
using Autoinjector;

// Injected with Transient lifetime
// Injected as IPasswordValidator
[Service(ServiceLifetime.Transient, typeof(IPasswordValidator))]
public class DefaultPasswordValidator : IPasswordValidator
{
    // your code
}
```

Say this is a small app though, so you decide to put all your password-related functionality on a single class, `PasswordManager`. Now, you need to inject `PasswordManager` as both an `IPasswordHasher` and an `IPasswordValidator`:

```csharp
using Autoinjector;

// Injected with Transient lifetime
// Injected as IPasswordHasher
// Injected as IPasswordValidator
[Service(ServiceLifetime.Transient, typeof(IPasswordHasher), typeof(IPasswordValidator))]
public class PasswordManager : IPasswordHasher, IPasswordValidator
{
    // your code
}
```

#### ServiceAttribute(ServiceLifetime)

If you want to inject a service as its own type and not an interface or base class (i.e. you want to be able to request a `PasswordManager` as a `PasswordManager` from DI), supply only the `ServiceLifetime` to the constructor:

```csharp
using Autoinjector;

// Injected with Transient lifetime
// Injected as PasswordManager
[Service(ServiceLifetime.Transient)]
public class PasswordManager : IPasswordHasher, IPasswordValidator
{
    // your code
}
```

#### ServiceAttribute(params Type[])

If you want to save some typing, the `ServiceAttribute.Lifetime` property defaults to `ServiceLifetime.Scoped`, so you can omit it. To use the default scoped lifetime and inject a service as an array of Types, supply only the `Type[]` to the constructor:

```csharp
using Autoinjector;

// Injected with Scoped lifetime
// Injected as IPasswordHasher
// Injected as IPasswordValidator
[Service(typeof(IPasswordHasher), typeof(IPasswordValidator))]
public class PasswordManager : IPasswordHasher, IPasswordValidator
{
    // your code
}
```

#### ServiceAttribute()

If you want your service to be scoped, and if you want to inject it as its own type, just use `[Service]` by itself:

```csharp
using Autoinjector;

// Injected with Scoped lifetime
// Injected as PasswordManager
[Service]
public class PasswordManager : IPasswordHasher, IPasswordValidator
{
    // your code
}
```