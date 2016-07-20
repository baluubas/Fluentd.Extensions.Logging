using Microsoft.Extensions.Logging;
using System;

namespace Fluentd.Extensions.Logging
{
	public static class FluentdLoggerFactoryExtensions
	{
		/// <summary>
		/// Add fluentd to the logging pipeline.
		/// </summary>
		public static ILoggerFactory AddFluentd(
			this ILoggerFactory factory,
			Func<string, LogLevel, bool> filter,
			FluentdOptions options)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			factory.AddProvider(new FluentdLoggerProvider(filter, options));
			return factory;
		}

		/// <summary>
		/// Add fluentd to the logging pipeline.
		/// </summary>
		public static ILoggerFactory AddFluentd(
			this ILoggerFactory factory,
			LogLevel logLevel)
		{
			return AddFluentd(
				factory,
				(_, level) => level >= logLevel,
				new FluentdOptions());
		}

		/// <summary>
		/// Add fluentd to the logging pipeline.
		/// </summary>
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
	}
}
