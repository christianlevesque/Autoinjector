using System;
using System.Linq;
using System.Reflection;
using Autoinjector;

namespace Microsoft.Extensions.DependencyInjection;

public static class Autoinjector
{
	/// <summary>
	/// Registers all services in all visible assemblies via the <see cref="ServiceAttribute"/>
	/// </summary>
	/// <remarks>
	/// The method will iterate over all visible assemblies in the current <see cref="AppDomain"/>. It's strongly recommeded that you use <see cref="AutoinjectServicesFromAssembly"/> and manually specify each assembly with services. By using this method, you will be needlessly searching through dozens of assemblies at startup that are guaranteed not to have any services at all. The option is here, but we strongly recommend you not use it.
	/// </remarks>
	/// <param name="self">the <see cref="IServiceCollection"/> for registering services</param>
	/// <returns></returns>
	public static IServiceCollection AutoinjectServices(this IServiceCollection self)
	{
		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			self.AutoinjectServicesFromAssembly(assembly);
		}

		return self;
	}

	/// <summary>
	/// Registers services from an assembly via the <see cref="ServiceAttribute"/>
	/// </summary>
	/// <param name="self">the <see cref="IServiceCollection"/> for registering services</param>
	/// <param name="assembly">the <see cref="Assembly"/> from which to add services</param>
	/// <returns></returns>
	public static IServiceCollection AutoinjectServicesFromAssembly(this IServiceCollection self, Assembly assembly)
	{
		var exportedTypes = assembly.GetExportedTypes()
		                            .Where(a => a.IsDefined(typeof(ServiceAttribute)));

		foreach (var type in exportedTypes)
		{
			var attribute = (Attribute.GetCustomAttribute(type, typeof(ServiceAttribute)) as ServiceAttribute)!;

			if (attribute.Types is { Length: > 0 })
			{
				foreach (var injectedType in attribute.Types)
				{
					self.AddService(injectedType, type, attribute.Lifetime);
				}
			}
			else
			{
				self.AddService(type, attribute.Lifetime);
			}
		}

		return self;
	}

	private static void AddService(this IServiceCollection self, Type alias, Type implementation, ServiceLifetime lifetime)
	{
		switch (lifetime)
		{
			case ServiceLifetime.Singleton:
				self.AddSingleton(alias, implementation);
				break;
			case ServiceLifetime.Scoped:
				self.AddScoped(alias, implementation);
				break;
			case ServiceLifetime.Transient:
			default:
				self.AddTransient(alias, implementation);
				break;
		}
	}

	private static void AddService(this IServiceCollection self, Type implementation, ServiceLifetime lifetime)
	{
		switch (lifetime)
		{
			case ServiceLifetime.Singleton:
				self.AddSingleton(implementation);
				break;
			case ServiceLifetime.Scoped:
				self.AddScoped(implementation);
				break;
			case ServiceLifetime.Transient:
			default:
				self.AddTransient(implementation);
				break;
		}
	}
}