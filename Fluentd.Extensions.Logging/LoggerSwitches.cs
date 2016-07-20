using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
}

