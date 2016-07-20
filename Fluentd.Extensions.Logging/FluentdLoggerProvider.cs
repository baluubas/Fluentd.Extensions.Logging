using System;
using Microsoft.Extensions.Logging;


namespace Fluentd.Extensions.Logging
{
	public class FluentdLoggerProvider : ILoggerProvider
	{
		private readonly Func<string, LogLevel, bool> _filter;
		private readonly FluentdOptions _options;
		private readonly FluentdClient _logClient;

		public FluentdLoggerProvider(Func<string, LogLevel, bool> filter, FluentdOptions options)
		{
			_filter = filter;
			_options = options;
			_logClient = new FluentdClient(options);
		}

		public ILogger CreateLogger(string name)
		{
			return new FluentdLogger(_logClient, name, _options.Tag, _filter);
		}

		public void Dispose()
		{
			_logClient.Dispose();
		}
	}
}
