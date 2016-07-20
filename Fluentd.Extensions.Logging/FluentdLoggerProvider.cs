using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;


namespace Fluentd.Extensions.Logging
{
	public class FluentdLoggerProvider : ILoggerProvider
	{
		private readonly ConcurrentDictionary<string, FluentdLogger> _loggers = new ConcurrentDictionary<string, FluentdLogger>();
		private readonly Func<string, LogLevel, bool> _filter;
		private readonly ILoggerSwitches _switches;
		private readonly FluentdOptions _options;
		private readonly FluentdClient _logClient;

		public FluentdLoggerProvider(Func<string, LogLevel, bool> filter, FluentdOptions options)
		{
			_filter = filter;
			_options = options;
			_logClient = new FluentdClient(options);
		}

		public FluentdLoggerProvider(ILoggerSwitches switches, FluentdOptions options)
		{
			_switches = switches;
			_options = options;
			_logClient = new FluentdClient(options);
		}

		public ILogger CreateLogger(string name)
		{
			return _loggers.GetOrAdd(name, CreateLoggerImplementation); 
		}

		public void Dispose()
		{
			_logClient.Dispose();
		}

		private FluentdLogger CreateLoggerImplementation(string name)
		{
			return new FluentdLogger(_logClient, name, _options.Tag, GetFilter(name, _switches));
		}

		private Func<string, LogLevel, bool> GetFilter(string name, ILoggerSwitches switches)
		{
			if (_filter != null)
			{
				return _filter;
			}

			if (switches != null)
			{
				foreach (var prefix in GetKeyPrefixes(name))
				{
					LogLevel level;
					if (switches.TryGetSwitch(prefix, out level))
					{
						return (n, l) => l >= level;
					}
				}
			}

			return (n, l) => false;
		}

		private IEnumerable<string> GetKeyPrefixes(string name)
		{
			while (!string.IsNullOrEmpty(name))
			{
				yield return name;
				var lastIndexOfDot = name.LastIndexOf('.');
				if (lastIndexOfDot == -1)
				{
					yield return "Default";
					break;
				}
				name = name.Substring(0, lastIndexOfDot);
			}
		}
	}
}
