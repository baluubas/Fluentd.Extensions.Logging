using System;

namespace Fluentd.Extensions.Logging
{
	public class FluentdOptions
	{
		public string Host { get; set; } = "localhost";
		public short Port { get; set; }= 24224;
		public short ReceiveBufferSize { get; set; } = 8192;
		public short SendBufferSize { get; set; } = 8192;
		public TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromMilliseconds(1000);
		public TimeSpan SendTimeout { get; set; } = TimeSpan.FromMilliseconds(1000);
		public bool LingerEnabled { get; set; } = true;
		public bool NoDelay { get; set; } = false;
		public TimeSpan LingerTime { get; set; } = TimeSpan.FromMilliseconds(1000);
		public bool EmitStackTraceWhenAvailable { get; set; } = false;

		public string Tag { get; set; } = "aspnetcore";
		public int MaxSendQueue { get; set; } = 500;
	}
}