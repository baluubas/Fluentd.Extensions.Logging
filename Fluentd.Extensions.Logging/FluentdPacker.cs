using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MsgPack;
using MsgPack.Serialization;

namespace Fluentd.Extensions.Logging
{
	public class FluentdPacker : IDisposable
	{
		private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		private readonly TcpClient _tcpClient;
		private readonly Packer _packer;
		private readonly NetworkStream _stream;

		public FluentdPacker(TcpClient tcpClient)
		{
			_tcpClient = tcpClient;
			_stream = _tcpClient.GetStream();
			_packer = Packer.Create(_stream, PackerCompatibilityOptions.PackBinaryAsRaw);
		}

		public async Task SendAsync(IDictionary<string, object> record, string tag, DateTime timestamp, CancellationToken cancellationToken)
		{
			long unixTimestamp = timestamp.ToUniversalTime().Subtract(UnixEpoch).Ticks / 10000000;
			await _packer.PackArrayHeaderAsync(3, cancellationToken);
			await _packer.PackStringAsync(tag, Encoding.UTF8, cancellationToken);
			await _packer.PackAsync((ulong)unixTimestamp, cancellationToken);
			await _packer.PackAsync(record, cancellationToken);
		}

		public void Dispose()
		{
			_stream.Dispose();
			_tcpClient.Dispose();
			_packer.Dispose();
		}
	}
}
