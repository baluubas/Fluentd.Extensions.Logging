using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.Configuration;

namespace Fluentd.Extensions.Logging
{
	public static class FluentdLoggerFactoryExtensions
	{
		public static ILoggerFactory AddFluentd(
			this ILoggerFactory factory,
			Func<string, LogLevel, bool> filter,
			FluentdOptions options)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			factory.AddProvider(new FluentdLoggerProvider(filter, options));
			return factory;
		}

		public static ILoggerFactory AddFluentd(
			this ILoggerFactory factory,
			LogLevel logLevel)
		{
			return AddFluentd(
				factory,
				(_, level) => level >= logLevel,
				new FluentdOptions());
		}

		public static ILoggerFactory AddFluentd(
			this ILoggerFactory factory,
			LogLevel logLevel,
			FluentdOptions options)
		{
			return AddFluentd(
				factory,
				(_, level) => level >= logLevel,
				options);
		}

		public static ILoggerFactory AddFluentd(
			this ILoggerFactory factory,
			IConfiguration configuration,
			FluentdOptions options)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			factory.AddProvider(new FluentdLoggerProvider(new ConfigurationLoggerSwitches(configuration), options));
			return factory;
		}

		public static ILoggerFactory AddFluentd(
			this ILoggerFactory factory,
			ILoggerSwitches swithes,
			FluentdOptions options)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			factory.AddProvider(new FluentdLoggerProvider(swithes, options));
			return factory;
		}
	}
}
