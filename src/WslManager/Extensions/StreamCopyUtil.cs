using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WslManager.Extensions
{
    // https://github.com/dotnet/runtime/issues/31479#issuecomment-578436466
    internal static class StreamCopyUtil
    {
		public const int PreferredBufferSize = 81920;

		public static async Task CopyStreamAsync(
			this Stream sourceStream, Stream destinationStream,
			int bufferSize = PreferredBufferSize,
			long? sourceStreamLength = default,
			CancellationToken cancellationToken = default,
			Action<long, long?> progressCallback = default)
        {
			if (sourceStream == null)
				throw new ArgumentNullException(nameof(sourceStream));
			if (destinationStream == null)
				throw new ArgumentNullException(nameof(destinationStream));
			if (!sourceStream.CanRead)
				throw new ArgumentException("Selected stream does not support read function.", nameof(sourceStream));
			if (!destinationStream.CanWrite)
				throw new ArgumentException("Selected stream does not support write function.", nameof(destinationStream));

			if (!sourceStreamLength.HasValue && sourceStream.CanSeek)
				sourceStreamLength = sourceStream.Length;

			bufferSize = Math.Max(bufferSize, 1);

			if (progressCallback == null)
				await sourceStream.CopyToAsync(destinationStream, bufferSize, cancellationToken).ConfigureAwait(false);
			else
            {
				var buffer = new byte[bufferSize];
				var read = default(int);
				var totalRead = default(int);

				while ((read = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
				{
					await destinationStream.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(false);
					totalRead += read;
					progressCallback(totalRead, sourceStreamLength);
				}

				Debug.Assert(totalRead == sourceStreamLength || !sourceStreamLength.HasValue);
			}
		}

		public static async Task CopyStreamAsync(
			this Uri sourceUri, Stream destinationStream,
			int bufferSize = PreferredBufferSize,
			CancellationToken cancellationToken = default,
			Action<long, long?> progressCallback = null)
		{
			if (sourceUri == null)
				throw new ArgumentNullException(nameof(sourceUri));
			if (destinationStream == null)
				throw new ArgumentNullException(nameof(destinationStream));

			if (!string.Equals(sourceUri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal) &&
				!string.Equals(sourceUri.Scheme, Uri.UriSchemeHttp, StringComparison.Ordinal))
				throw new ArgumentException("Selected URI has unsupported scheme.", nameof(sourceUri));

			using var client = new HttpClient();
			using var response = await client.GetAsync(sourceUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
			await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
			await CopyStreamAsync(stream, destinationStream, bufferSize, response.Content.Headers.ContentLength, cancellationToken, progressCallback).ConfigureAwait(false);
		}

		public static async Task CopyStreamAsync(
			this FileInfo sourceFileInfo, Stream destinationStream,
			int bufferSize = PreferredBufferSize,
			CancellationToken cancellationToken = default,
			Action<long, long?> progressCallback = null)
        {
			if (sourceFileInfo == null)
				throw new ArgumentNullException(nameof(sourceFileInfo));
			if (destinationStream == null)
				throw new ArgumentNullException(nameof(destinationStream));
			if (!sourceFileInfo.Exists)
				throw new FileNotFoundException("Selected file is not an available file.", sourceFileInfo.FullName);

			await using var file = sourceFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            await CopyStreamAsync(file, destinationStream, bufferSize, file.Length, cancellationToken, progressCallback).ConfigureAwait(false);
        }
	}
}
