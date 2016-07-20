﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Fluentd.Extensions.Logging
{
	public class LoggerSwitches : ILoggerSwitches
	{

		public IDictionary<string, LogLevel> Switches { get; set; } = new Dictionary<string, LogLevel>();


		public bool TryGetSwitch(string name, out LogLevel level)
		{
			return Switches.TryGetValue(name, out level);
		}
	}

	public interface ILoggerSwitches
	{
		bool TryGetSwitch(string name, out LogLevel level);
	}

	public class ConfigurationLoggerSwitches : ILoggerSwitches
	{
		private readonly IConfiguration _configuration;

		public ConfigurationLoggerSwitches(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public bool TryGetSwitch(string name, out LogLevel level)
		{
			var switches = _configuration.GetSection("LogLevel");
			if (switches == null)
			{
				level = LogLevel.None;
				return false;
			}

			var value = switches[name];
			if (string.IsNullOrEmpty(value))
			{
				level = LogLevel.None;
				return false;
			}

			if (Enum.TryParse(value, out level))
			{
				return true;
			}

			var message = $"Configuration value '{value}' for category '{name}' is not supported.";
			throw new InvalidOperationException(message);
		}
	}
}

