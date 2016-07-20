using Microsoft.Extensions.Logging;

namespace Fluentd.Extensions.Logging
{
	public interface ILoggerSwitches
	{
		bool TryGetSwitch(string name, out LogLevel level);
	}
}