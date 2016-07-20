using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Fluentd.Extensions.Logging
{
	public class FluentdLogger : ILogger
	{
		private FluentdClient _client;
		private string _categoryName;
		private string _source;
		private Func<string, LogLevel, bool> _filter;

		public FluentdLogger(FluentdClient client, string categoryName, string source, Func<string, LogLevel, bool> filter)
		{
			_client = client;
			_categoryName = categoryName;
			_source = source;
			_filter = filter;
		}
		
		public bool IsEnabled(LogLevel logLevel)
		{
			return (_filter == null || _filter(_categoryName, logLevel));
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			if (formatter == null)
			{
				throw new ArgumentNullException(nameof(formatter));
			}

			var message = formatter(state, exception);

			if (string.IsNullOrEmpty(message))
			{
				return;
			}
			
			_client.Send(message, _categoryName, logLevel.ToString(), DateTime.UtcNow, exception, _source);

		}
	}
}
