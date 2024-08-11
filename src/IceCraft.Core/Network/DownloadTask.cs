namespace IceCraft.Core.Network;
using System;
using System.Buffers;
using System.Diagnostics.Metrics;

public class DownloadTask
{
    private readonly HttpResponseMessage _response;
    private readonly string _targetPath;
    private readonly INetworkDownloadTask? _notifyTask;
    private int _counter;

    public DownloadTask(HttpResponseMessage response, string targetPath, INetworkDownloadTask? notifyTask = null)
    {
        _response = response;
        _targetPath = targetPath;
        _notifyTask = notifyTask;
    }

    public int Size { get; }

    /// <summary>
    /// Gets the expected size of the content.
    /// </summary>
    /// <returns>The expected size if any; <see langword="null"/> if no content size is provided.</returns>
    public long? GetExpectedSize()
    {
        return _response.Content.Headers.ContentLength;
    }

    public async Task Perform()
    {
        // For performance we use -1 to indicate no size whatsoever.
        const long NoSize = -1;
        var spanSize = 250;

        var size = GetExpectedSize() ?? NoSize;
        if (size == NoSize)
        {
            _notifyTask?.SetIntermediateProgress();
        }

        // Create streams.
        await using var file = File.Create(_targetPath);
        await using var stream = await _response.Content.ReadAsStreamAsync();

        if (_notifyTask == null)
        {
            spanSize = 1;
        }

        using var bufferOwner = MemoryPool<byte>.Shared.Rent(spanSize);

        while (true)
        {
            var buffer = bufferOwner.Memory;

            // Read single byte logic, for faster processing.
            if (spanSize == 1)
            {
                if (!ReadSingleByte(file, stream))
                {
                    _notifyTask?.Complete();
                    break;
                }

                continue;
            }

            // Read span logic.
            // Allocate span and download into it.

            var read = await stream.ReadAsync(buffer);
            if (read == 0)
            {
                // Download complete.
                _notifyTask?.Complete();
                break;
            }

            _counter += read;

            // Slice the span to write.
            var writeBuffer = read != spanSize
            ? buffer[..read]
            : buffer;

            await file.WriteAsync(writeBuffer);

            // Notify progress.
            if (size != NoSize && _notifyTask != null)
            {
                _notifyTask.SetDefiniteProgress(_counter, size);
            }
        }

        // Returns true if can continue; false if break.
        bool ReadSingleByte(FileStream file, Stream stream)
        {
            var b = stream.ReadByte();

            if (b == -1)
            {
                return false;
            }

            _counter++;
            file.WriteByte((byte)b);
            return true;
        }
    }
}
