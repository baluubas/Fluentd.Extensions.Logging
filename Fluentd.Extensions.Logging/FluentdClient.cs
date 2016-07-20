using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MsgPack;
using MsgPack.Serialization;

namespace Fluentd.Extensions.Logging
{
	public class FluentdClient : IDisposable
	{
		private readonly FluentdOptions _options;
		private Task<FluentdPacker> _tail;
		private readonly object syncOjb = new object();
		private int _pendingSendTask = 0;
		private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

		public FluentdClient(FluentdOptions options)
		{
			_options = options;

		}

		public void Send(string message, string loggerName, string level, DateTime timestamp, Exception exception, string source)
		{
			if (_pendingSendTask >= _options.MaxSendQueue)
			{
				return;
			}

			var record = CreateRecord(message, loggerName, level, exception);

			lock (syncOjb)
			{
				if (_tail == null)
				{
					_tail = EnsureConnected();
				}
				_tail = _tail.ContinueWith(t => CreateSendTask(t.Result, record, timestamp)).Unwrap();
			}
		}

		private async Task<FluentdPacker> CreateSendTask(
			FluentdPacker packer,
			IDictionary<string, object> record, 
			DateTime timestamp)
		{
			Interlocked.Increment(ref _pendingSendTask); 
			while (true)
			{
				if (cancellationTokenSource.IsCancellationRequested)
				{
					packer.Dispose();
					return null;
				}

				try
				{
					await packer.SendAsync(record, _options.Tag, timestamp, cancellationTokenSource.Token);
					Interlocked.Decrement(ref _pendingSendTask);
					return packer;
				}
				catch
				{
					await Task.Delay(5, cancellationTokenSource.Token);
					packer.Dispose();
					packer = await EnsureConnected();
				}
			}
		}

		private async Task<FluentdPacker> EnsureConnected()
		{
			var tcpClient = CreateTcpClient();
			while (true)
			{
				if (cancellationTokenSource.IsCancellationRequested)
				{
					return null;
				}

				try
				{
					await tcpClient.ConnectAsync(_options.Host, _options.Port);
					return new FluentdPacker(tcpClient);
				}
				catch
				{
					tcpClient.Dispose();
					await Task.Delay(TimeSpan.FromSeconds(5), cancellationTokenSource.Token);
				}
			}
		}

		private IDictionary<string, object> CreateRecord(string message, string loggerName, string level, Exception exception)
		{
			var record = new Dictionary<string, object>
			{
				{"level", level},
				{"message", message},
				{"logger_name", loggerName},
			};

			if (_options.EmitStackTraceWhenAvailable && exception != null)
			{
				record.Add("stacktrace", exception.StackTrace);
			}

			if (!string.IsNullOrEmpty(_options.ServerName))
			{
				record.Add("server_name", _options.ServerName);
			}

			if (!string.IsNullOrEmpty(_options.AppInstanceName))
			{
				record.Add("instance_name", _options.AppInstanceName);
			}

			return record;
		}

		private TcpClient CreateTcpClient()
		{
			var tcpClient = new TcpClient();
			tcpClient.NoDelay = _options.NoDelay;
			tcpClient.ReceiveBufferSize = _options.ReceiveBufferSize;
			tcpClient.SendBufferSize = _options.SendBufferSize;
			tcpClient.SendTimeout = (int)_options.SendTimeout.TotalMilliseconds;
			tcpClient.ReceiveTimeout = (int)_options.ReceiveTimeout.TotalMilliseconds;
			tcpClient.LingerState = new LingerOption(_options.LingerEnabled, (int)_options.LingerTime.TotalSeconds);
			return tcpClient;
		}

		public void Dispose()
		{
			cancellationTokenSource.Cancel();
	
			try
			{
				_tail.Wait();
			}
			catch (TaskCanceledException)
			{
			}
		}
	}
}
