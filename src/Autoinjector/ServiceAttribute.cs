using System;
using Microsoft.Extensions.DependencyInjection;

namespace Autoinjector;

/// <summary>
/// Designates a class as an injectable service
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ServiceAttribute : Attribute
{
	/// <summary>
	/// Represents how the service should be retained and reused by the DI framework
	/// </summary>
    public ServiceLifetime Lifetime { get; }

	/// <summary>
	/// Represents the type(s) the DI framework should use to register this service
	/// </summary>
    public Type[]? Types { get; set; }

    /// <summary>
    /// Designates a class as an injectable service with a default lifetime of <see cref="ServiceLifetime.Scoped"/>
    /// </summary>
    public ServiceAttribute()
    {
	    Lifetime = ServiceLifetime.Scoped;
    }

    /// <summary>
    /// Designates a class as an injectable service with the specified <see cref="ServiceLifetime"/>
    /// </summary>
    /// <param name="lifetime">the <see cref="ServiceLifetime"/> with which to inject the service</param>
    public ServiceAttribute(ServiceLifetime lifetime)
    {
    	Lifetime = lifetime;
    }

    /// <summary>
    /// Designates a class as an injectable service with the specified <see cref="ServiceLifetime"/> and <see cref="Type"/>(s)
    /// </summary>
    /// <param name="lifetime">the <see cref="ServiceLifetime"/> with which to inject the service</param>
    /// <param name="types">the <see cref="Type"/>(s) as which to inject the service</param>
    public ServiceAttribute(ServiceLifetime lifetime, params Type[] types) : this(lifetime)
    {
	    Types = types;
    }

    /// <summary>
    /// Designates a class as an injectable service with the specified <see cref="Type"/>(s) a default lifetime of <see cref="ServiceLifetime.Scoped"/>
    /// </summary>
    /// <param name="types">the <see cref="Type"/>(s) as which to inject the service</param>
    public ServiceAttribute(params Type[] types) : this()
    {
	    Types = types;
    }
}