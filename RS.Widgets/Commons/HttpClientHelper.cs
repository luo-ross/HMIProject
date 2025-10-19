

using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Net.Http.Headers;

namespace RS.Widgets.Commons
{
    /// <summary>
    /// 使用 HttpClient 实现的 WebClient 替代品
    /// 保留 WebClient 的所有方法名和逻辑，只改变底层 HTTP 实现
    /// </summary>
    public class HttpClientHelper : IDisposable
    {
        private const int DefaultCopyBufferLength = 8192;
        private const int DefaultDownloadBufferLength = 65536;
        private const string DefaultUploadFileContentType = "application/octet-stream";
        private const string UploadFileContentType = "multipart/form-data";
        private const string UploadValuesContentType = "application/x-www-form-urlencoded";

        private readonly HttpClient _httpClient;
        private readonly bool _disposeClient;
        private Uri? _baseAddress;
        private WebHeaderCollection? _headers;
        private NameValueCollection? _requestParameters;
        private Encoding _encoding = Encoding.UTF8;
        private bool _canceled;
        private ProgressData? _progress;
        private int _callNesting;

        // EAP 异步相关
        private SendOrPostCallback? _downloadDataOperationCompleted;
        private SendOrPostCallback? _downloadStringOperationCompleted;
        private SendOrPostCallback? _downloadFileOperationCompleted;
        private SendOrPostCallback? _uploadStringOperationCompleted;
        private SendOrPostCallback? _uploadDataOperationCompleted;
        private SendOrPostCallback? _uploadFileOperationCompleted;
        private SendOrPostCallback? _uploadValuesOperationCompleted;
        private SendOrPostCallback? _openReadOperationCompleted;
        private SendOrPostCallback? _openWriteOperationCompleted;
        private SendOrPostCallback? _reportDownloadProgressChanged;
        private SendOrPostCallback? _reportUploadProgressChanged;

        public HttpClientHelper()
        {
            _httpClient = new HttpClient();
            _disposeClient = true;
        }

        public HttpClientHelper(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _disposeClient = false;
        }

        #region 属性

        public Encoding Encoding
        {
            get => _encoding;
            set => _encoding = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string? BaseAddress
        {
            get => _baseAddress?.ToString();
            set
            {
                if (string.IsNullOrEmpty(value))
                    _baseAddress = null;
                else
                    _baseAddress = new Uri(value);
            }
        }

        public WebHeaderCollection Headers
        {
            get => _headers ??= new WebHeaderCollection();
            set => _headers = value;
        }

        public NameValueCollection QueryString
        {
            get => _requestParameters ??= new NameValueCollection();
            set => _requestParameters = value;
        }

        public TimeSpan Timeout
        {
            get => _httpClient.Timeout;
            set => _httpClient.Timeout = value;
        }

        public bool IsBusy => Volatile.Read(ref _callNesting) > 0;

        #endregion

        #region 事件

        public event DownloadStringCompletedEventHandler? DownloadStringCompleted;
        public event DownloadDataCompletedEventHandler? DownloadDataCompleted;
        public event AsyncCompletedEventHandler? DownloadFileCompleted;
        public event UploadStringCompletedEventHandler? UploadStringCompleted;
        public event UploadDataCompletedEventHandler? UploadDataCompleted;
        public event UploadFileCompletedEventHandler? UploadFileCompleted;
        public event UploadValuesCompletedEventHandler? UploadValuesCompleted;
        public event OpenReadCompletedEventHandler? OpenReadCompleted;
        public event OpenWriteCompletedEventHandler? OpenWriteCompleted;
        public event DownloadProgressChangedEventHandler? DownloadProgressChanged;
        public event UploadProgressChangedEventHandler? UploadProgressChanged;

        protected virtual void OnDownloadStringCompleted(DownloadStringCompletedEventArgs e) => DownloadStringCompleted?.Invoke(this, e);
        protected virtual void OnDownloadDataCompleted(DownloadDataCompletedEventArgs e) => DownloadDataCompleted?.Invoke(this, e);
        protected virtual void OnDownloadFileCompleted(AsyncCompletedEventArgs e) => DownloadFileCompleted?.Invoke(this, e);
        protected virtual void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e) => DownloadProgressChanged?.Invoke(this, e);
        protected virtual void OnUploadStringCompleted(UploadStringCompletedEventArgs e) => UploadStringCompleted?.Invoke(this, e);
        protected virtual void OnUploadDataCompleted(UploadDataCompletedEventArgs e) => UploadDataCompleted?.Invoke(this, e);
        protected virtual void OnUploadFileCompleted(UploadFileCompletedEventArgs e) => UploadFileCompleted?.Invoke(this, e);
        protected virtual void OnUploadValuesCompleted(UploadValuesCompletedEventArgs e) => UploadValuesCompleted?.Invoke(this, e);
        protected virtual void OnUploadProgressChanged(UploadProgressChangedEventArgs e) => UploadProgressChanged?.Invoke(this, e);
        protected virtual void OnOpenReadCompleted(OpenReadCompletedEventArgs e) => OpenReadCompleted?.Invoke(this, e);
        protected virtual void OnOpenWriteCompleted(OpenWriteCompletedEventArgs e) => OpenWriteCompleted?.Invoke(this, e);

        #endregion

        #region 操作管理（复制自 WebClient）

        private void StartOperation()
        {
            if (Interlocked.Increment(ref _callNesting) > 1)
            {
                EndOperation();
                throw new NotSupportedException("不支持并发 IO 操作");
            }
            _canceled = false;
            _progress?.Reset();
        }

        private void EndOperation() => Interlocked.Decrement(ref _callNesting);

        private void InitWebClientAsync()
        {
            if (_progress == null)
            {
                _downloadDataOperationCompleted = arg => OnDownloadDataCompleted((DownloadDataCompletedEventArgs)arg!);
                _downloadStringOperationCompleted = arg => OnDownloadStringCompleted((DownloadStringCompletedEventArgs)arg!);
                _downloadFileOperationCompleted = arg => OnDownloadFileCompleted((AsyncCompletedEventArgs)arg!);
                _uploadStringOperationCompleted = arg => OnUploadStringCompleted((UploadStringCompletedEventArgs)arg!);
                _uploadDataOperationCompleted = arg => OnUploadDataCompleted((UploadDataCompletedEventArgs)arg!);
                _uploadFileOperationCompleted = arg => OnUploadFileCompleted((UploadFileCompletedEventArgs)arg!);
                _uploadValuesOperationCompleted = arg => OnUploadValuesCompleted((UploadValuesCompletedEventArgs)arg!);
                _openReadOperationCompleted = arg => OnOpenReadCompleted((OpenReadCompletedEventArgs)arg!);
                _openWriteOperationCompleted = arg => OnOpenWriteCompleted((OpenWriteCompletedEventArgs)arg!);
                _reportDownloadProgressChanged = arg => OnDownloadProgressChanged((DownloadProgressChangedEventArgs)arg!);
                _reportUploadProgressChanged = arg => OnUploadProgressChanged((UploadProgressChangedEventArgs)arg!);

                _progress = new ProgressData();
            }
        }

        private void PostProgressChanged(SynchronizationContext? syncContext, ProgressData progress)
        {
            if (syncContext != null && (progress.BytesSent > 0 || progress.BytesReceived > 0))
            {
                int progressPercentage;
                if (progress.HasUploadPhase)
                {
                    if (UploadProgressChanged != null)
                    {
                        progressPercentage = progress.TotalBytesToReceive < 0 && progress.BytesReceived == 0 ?
                            progress.TotalBytesToSend < 0 ? 0 : progress.TotalBytesToSend == 0 ? 50 : (int)((50 * progress.BytesSent) / progress.TotalBytesToSend) :
                            progress.TotalBytesToSend < 0 ? 50 : progress.TotalBytesToReceive == 0 ? 100 : (int)((50 * progress.BytesReceived) / progress.TotalBytesToReceive + 50);

                        syncContext.Post(_reportUploadProgressChanged!, new UploadProgressChangedEventArgs(
                            progressPercentage, null, progress.BytesSent, progress.TotalBytesToSend,
                            progress.BytesReceived, progress.TotalBytesToReceive));
                    }
                }
                else if (DownloadProgressChanged != null)
                {
                    progressPercentage = progress.TotalBytesToReceive < 0 ? 0 :
                        progress.TotalBytesToReceive == 0 ? 100 :
                        (int)((100 * progress.BytesReceived) / progress.TotalBytesToReceive);

                    syncContext.Post(_reportDownloadProgressChanged!, new DownloadProgressChangedEventArgs(
                        progressPercentage, null, progress.BytesReceived, progress.TotalBytesToReceive));
                }
            }
        }

        #endregion

        #region DownloadBits / UploadBits 核心方法（直接复制逻辑，只改 HTTP 实现）

        // 从 HTTP 响应下载数据到流中（同步版本）
        private byte[]? DownloadBits(HttpRequestMessage request, HttpResponseMessage response, Stream writeStream)
        {
            try
            {
                long contentLength = response.Content.Headers.ContentLength ?? -1;
                byte[] copyBuffer = new byte[contentLength == -1 || contentLength > DefaultDownloadBufferLength ?
                    DefaultDownloadBufferLength : contentLength];

                using (Stream readStream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                {
                    if (readStream != null)
                    {
                        int bytesRead;
                        while ((bytesRead = readStream.Read(copyBuffer, 0, copyBuffer.Length)) != 0)
                        {
                            writeStream.Write(copyBuffer, 0, bytesRead);
                        }
                    }
                }

                return (writeStream as MemoryStream)?.ToArray();
            }
            catch (Exception)
            {
                writeStream?.Close();
                throw;
            }
        }

        // 从 HTTP 响应下载数据到流中（异步版本，带进度）
        private async void DownloadBitsAsync(
            HttpRequestMessage request, Stream writeStream,
            SynchronizationContext? syncContext, Action<byte[]?, Exception?, object?> completionDelegate, object? userToken)
        {
            Exception? exception = null;
            try
            {
                using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    long contentLength = response.Content.Headers.ContentLength ?? -1;
                    byte[] copyBuffer = new byte[contentLength == -1 || contentLength > DefaultDownloadBufferLength ?
                        DefaultDownloadBufferLength : contentLength];

                    if (contentLength >= 0)
                    {
                        _progress!.TotalBytesToReceive = contentLength;
                    }

                    using (writeStream)
                    using (Stream readStream = await response.Content.ReadAsStreamAsync())
                    {
                        if (readStream != null)
                        {
                            while (true)
                            {
                                int bytesRead = await readStream.ReadAsync(copyBuffer, 0, copyBuffer.Length);
                                if (bytesRead == 0) break;

                                _progress!.BytesReceived += bytesRead;
                                if (_progress.BytesReceived != _progress.TotalBytesToReceive)
                                {
                                    PostProgressChanged(syncContext, _progress);
                                }

                                await writeStream.WriteAsync(copyBuffer, 0, bytesRead);
                            }
                        }

                        if (_progress!.TotalBytesToReceive < 0)
                        {
                            _progress.TotalBytesToReceive = _progress.BytesReceived;
                        }
                        PostProgressChanged(syncContext, _progress);
                    }

                    completionDelegate((writeStream as MemoryStream)?.ToArray(), null, userToken);
                }
            }
            catch (Exception e)
            {
                exception = e;
                writeStream?.Close();
            }
            finally
            {
                if (exception != null)
                {
                    completionDelegate(null, exception, userToken);
                }
            }
        }

        // 上传数据到 HTTP 请求（异步版本，带进度）
        private async void UploadBitsAsync(
            HttpRequestMessage request, FileStream? readStream, byte[]? buffer, int chunkSize,
            byte[]? header, byte[]? footer,
            SynchronizationContext? syncContext, Action<byte[]?, Exception?, object?> completionDelegate, object? userToken)
        {
            _progress!.HasUploadPhase = true;

            Exception? exception = null;
            try
            {
                var content = new ProgressStreamContent(readStream, buffer, chunkSize, header, footer, _progress, syncContext, this);
                request.Content = content;

                using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    // 上传完成后，下载响应
                    var memoryStream = new MemoryStream();
                    long contentLength = response.Content.Headers.ContentLength ?? -1;
                    byte[] copyBuffer = new byte[contentLength == -1 || contentLength > DefaultDownloadBufferLength ?
                        DefaultDownloadBufferLength : contentLength];

                    if (contentLength >= 0)
                    {
                        _progress.TotalBytesToReceive = contentLength;
                    }

                    using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        if (responseStream != null)
                        {
                            while (true)
                            {
                                int bytesRead = await responseStream.ReadAsync(copyBuffer, 0, copyBuffer.Length);
                                if (bytesRead == 0) break;

                                _progress.BytesReceived += bytesRead;
                                PostProgressChanged(syncContext, _progress);

                                await memoryStream.WriteAsync(copyBuffer, 0, bytesRead);
                            }
                        }

                        if (_progress.TotalBytesToReceive < 0)
                        {
                            _progress.TotalBytesToReceive = _progress.BytesReceived;
                        }
                        PostProgressChanged(syncContext, _progress);
                    }

                    completionDelegate(memoryStream.ToArray(), null, userToken);
                }
            }
            catch (Exception e)
            {
                exception = e;
            }
            finally
            {
                if (exception != null)
                {
                    completionDelegate(null, exception, userToken);
                }
            }
        }

        // 用于上传时报告进度的自定义 HttpContent
        private class ProgressStreamContent : HttpContent
        {
            private readonly FileStream? _fileStream;
            private readonly byte[]? _buffer;
            private readonly int _chunkSize;
            private readonly byte[]? _header;
            private readonly byte[]? _footer;
            private readonly ProgressData _progress;
            private readonly SynchronizationContext? _syncContext;
            private readonly HttpClientHelper _client;

            public ProgressStreamContent(FileStream? fileStream, byte[]? buffer, int chunkSize,
                byte[]? header, byte[]? footer, ProgressData progress,
                SynchronizationContext? syncContext, HttpClientHelper client)
            {
                _fileStream = fileStream;
                _buffer = buffer;
                _chunkSize = chunkSize;
                _header = header;
                _footer = footer;
                _progress = progress;
                _syncContext = syncContext;
                _client = client;
            }

            protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            {
                if (_header != null)
                {
                    await stream.WriteAsync(_header, 0, _header.Length);
                    _progress.BytesSent += _header.Length;
                    _client.PostProgressChanged(_syncContext, _progress);
                }

                if (_fileStream != null)
                {
                    using (_fileStream)
                    {
                        byte[] buffer = new byte[DefaultCopyBufferLength];
                        while (true)
                        {
                            int bytesRead = await _fileStream.ReadAsync(buffer, 0, buffer.Length);
                            if (bytesRead <= 0) break;

                            await stream.WriteAsync(buffer, 0, bytesRead);
                            _progress.BytesSent += bytesRead;
                            _client.PostProgressChanged(_syncContext, _progress);
                        }
                    }
                }
                else if (_buffer != null)
                {
                    for (int pos = 0; pos < _buffer.Length;)
                    {
                        int toWrite = _buffer.Length - pos;
                        if (_chunkSize != 0 && toWrite > _chunkSize)
                        {
                            toWrite = _chunkSize;
                        }
                        await stream.WriteAsync(_buffer, pos, toWrite);
                        pos += toWrite;
                        _progress.BytesSent += toWrite;
                        _client.PostProgressChanged(_syncContext, _progress);
                    }
                }

                if (_footer != null)
                {
                    await stream.WriteAsync(_footer, 0, _footer.Length);
                    _progress.BytesSent += _footer.Length;
                    _client.PostProgressChanged(_syncContext, _progress);
                }
            }

            protected override bool TryComputeLength(out long length)
            {
                length = (_header?.Length ?? 0) + (_buffer?.Length ?? _fileStream?.Length ?? 0) + (_footer?.Length ?? 0);
                if (_fileStream != null && _progress != null)
                {
                    _progress.TotalBytesToSend = length;
                }
                return true;
            }
        }

        #endregion

        #region Task 异步方法（通过 EAP 方法包装）

        public Task<byte[]> DownloadDataTaskAsync(string address) => DownloadDataTaskAsync(new Uri(address));
        public Task<byte[]> DownloadDataTaskAsync(Uri address)
        {
            var tcs = new TaskCompletionSource<byte[]>(address);
            DownloadDataCompletedEventHandler? handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, args => args.Result, handler,
                (client, h) => client.DownloadDataCompleted -= h);
            DownloadDataCompleted += handler;

            try { DownloadDataAsync(address, tcs); }
            catch
            {
                DownloadDataCompleted -= handler;
                throw;
            }

            return tcs.Task;
        }

        public Task<string> DownloadStringTaskAsync(string address) => DownloadStringTaskAsync(new Uri(address));
        public Task<string> DownloadStringTaskAsync(Uri address)
        {
            var tcs = new TaskCompletionSource<string>(address);
            DownloadStringCompletedEventHandler? handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, args => args.Result, handler,
                (client, h) => client.DownloadStringCompleted -= h);
            DownloadStringCompleted += handler;

            try { DownloadStringAsync(address, tcs); }
            catch
            {
                DownloadStringCompleted -= handler;
                throw;
            }

            return tcs.Task;
        }

        public Task DownloadFileTaskAsync(string address, string fileName) => DownloadFileTaskAsync(new Uri(address), fileName);
        public Task DownloadFileTaskAsync(Uri address, string fileName)
        {
            var tcs = new TaskCompletionSource<object?>(address);
            AsyncCompletedEventHandler? handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, args => (object?)null, handler,
                (client, h) => client.DownloadFileCompleted -= h);
            DownloadFileCompleted += handler;

            try { DownloadFileAsync(address, fileName, tcs); }
            catch
            {
                DownloadFileCompleted -= handler;
                throw;
            }

            return tcs.Task;
        }

        public Task<byte[]> UploadDataTaskAsync(string address, byte[] data) => UploadDataTaskAsync(new Uri(address), "POST", data);
        public Task<byte[]> UploadDataTaskAsync(Uri address, byte[] data) => UploadDataTaskAsync(address, "POST", data);
        public Task<byte[]> UploadDataTaskAsync(string address, string? method, byte[] data) => UploadDataTaskAsync(new Uri(address), method, data);
        public Task<byte[]> UploadDataTaskAsync(Uri address, string? method, byte[] data)
        {
            var tcs = new TaskCompletionSource<byte[]>(address);
            UploadDataCompletedEventHandler? handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, args => args.Result, handler,
                (client, h) => client.UploadDataCompleted -= h);
            UploadDataCompleted += handler;

            try { UploadDataAsync(address, method, data, tcs); }
            catch
            {
                UploadDataCompleted -= handler;
                throw;
            }

            return tcs.Task;
        }

        public Task<string> UploadStringTaskAsync(string address, string data) => UploadStringTaskAsync(new Uri(address), "POST", data);
        public Task<string> UploadStringTaskAsync(Uri address, string data) => UploadStringTaskAsync(address, "POST", data);
        public Task<string> UploadStringTaskAsync(string address, string? method, string data) => UploadStringTaskAsync(new Uri(address), method, data);
        public Task<string> UploadStringTaskAsync(Uri address, string? method, string data)
        {
            var tcs = new TaskCompletionSource<string>(address);
            UploadStringCompletedEventHandler? handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, args => args.Result, handler,
                (client, h) => client.UploadStringCompleted -= h);
            UploadStringCompleted += handler;

            try { UploadStringAsync(address, method, data, tcs); }
            catch
            {
                UploadStringCompleted -= handler;
                throw;
            }

            return tcs.Task;
        }

        public Task<byte[]> UploadFileTaskAsync(string address, string fileName) => UploadFileTaskAsync(new Uri(address), "POST", fileName);
        public Task<byte[]> UploadFileTaskAsync(Uri address, string fileName) => UploadFileTaskAsync(address, "POST", fileName);
        public Task<byte[]> UploadFileTaskAsync(string address, string? method, string fileName) => UploadFileTaskAsync(new Uri(address), method, fileName);
        public Task<byte[]> UploadFileTaskAsync(Uri address, string? method, string fileName)
        {
            var tcs = new TaskCompletionSource<byte[]>(address);
            UploadFileCompletedEventHandler? handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, args => args.Result, handler,
                (client, h) => client.UploadFileCompleted -= h);
            UploadFileCompleted += handler;

            try { UploadFileAsync(address, method, fileName, tcs); }
            catch
            {
                UploadFileCompleted -= handler;
                throw;
            }

            return tcs.Task;
        }

        public Task<byte[]> UploadValuesTaskAsync(string address, NameValueCollection data) => UploadValuesTaskAsync(new Uri(address), "POST", data);
        public Task<byte[]> UploadValuesTaskAsync(Uri address, NameValueCollection data) => UploadValuesTaskAsync(address, "POST", data);
        public Task<byte[]> UploadValuesTaskAsync(string address, string? method, NameValueCollection data) => UploadValuesTaskAsync(new Uri(address), method, data);
        public Task<byte[]> UploadValuesTaskAsync(Uri address, string? method, NameValueCollection data)
        {
            var tcs = new TaskCompletionSource<byte[]>(address);
            UploadValuesCompletedEventHandler? handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, args => args.Result, handler,
                (client, h) => client.UploadValuesCompleted -= h);
            UploadValuesCompleted += handler;

            try { UploadValuesAsync(address, method, data, tcs); }
            catch
            {
                UploadValuesCompleted -= handler;
                throw;
            }

            return tcs.Task;
        }

        public Task<Stream> OpenReadTaskAsync(string address) => OpenReadTaskAsync(new Uri(address));
        public Task<Stream> OpenReadTaskAsync(Uri address)
        {
            var tcs = new TaskCompletionSource<Stream>(address);
            OpenReadCompletedEventHandler? handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, args => args.Result, handler,
                (client, h) => client.OpenReadCompleted -= h);
            OpenReadCompleted += handler;

            try { OpenReadAsync(address, tcs); }
            catch
            {
                OpenReadCompleted -= handler;
                throw;
            }

            return tcs.Task;
        }

        private void HandleCompletion<TAsyncCompletedEventArgs, TCompletionDelegate, T>(
            TaskCompletionSource<T> tcs, TAsyncCompletedEventArgs e,
            Func<TAsyncCompletedEventArgs, T> getResult,
            TCompletionDelegate handler,
            Action<HttpClientHelper, TCompletionDelegate> unregisterHandler)
            where TAsyncCompletedEventArgs : AsyncCompletedEventArgs
        {
            if (e.UserState == tcs)
            {
                try { unregisterHandler(this, handler); }
                finally
                {
                    if (e.Error != null) tcs.TrySetException(e.Error);
                    else if (e.Cancelled) tcs.TrySetCanceled();
                    else tcs.TrySetResult(getResult(e));
                }
            }
        }

        #endregion

        #region EAP 异步方法（使用 DownloadBitsAsync/UploadBitsAsync）

        public void DownloadDataAsync(Uri address) => DownloadDataAsync(address, null);
        public void DownloadDataAsync(Uri address, object? userToken)
        {
            InitWebClientAsync();
            StartOperation();

            var syncContext = SynchronizationContext.Current;
            var request = CreateRequest(address, HttpMethod.Get);

            DownloadBitsAsync(request, new MemoryStream(), syncContext,
                (result, error, state) =>
                {
                    EndOperation();
                    if (syncContext != null)
                    {
                        syncContext.Post(_downloadDataOperationCompleted!,
                            new DownloadDataCompletedEventArgs(result, error, _canceled, state));
                    }
                }, userToken);
        }

        public void DownloadStringAsync(Uri address) => DownloadStringAsync(address, null);
        public void DownloadStringAsync(Uri address, object? userToken)
        {
            InitWebClientAsync();
            StartOperation();

            var syncContext = SynchronizationContext.Current;
            var request = CreateRequest(address, HttpMethod.Get);

            DownloadBitsAsync(request, new MemoryStream(), syncContext,
                (result, error, state) =>
                {
                    EndOperation();
                    string? stringResult = null;
                    if (error == null && result != null)
                    {
                        try
                        {
                            stringResult = GetStringUsingEncoding(result);
                        }
                        catch (Exception e)
                        {
                            error = e;
                        }
                    }
                    if (syncContext != null)
                    {
                        syncContext.Post(_downloadStringOperationCompleted!,
                            new DownloadStringCompletedEventArgs(stringResult, error, _canceled, state));
                    }
                }, userToken);
        }

        public void DownloadFileAsync(Uri address, string fileName) => DownloadFileAsync(address, fileName, null);
        public void DownloadFileAsync(Uri address, string fileName, object? userToken)
        {
            InitWebClientAsync();
            StartOperation();

            var syncContext = SynchronizationContext.Current;
            var request = CreateRequest(address, HttpMethod.Get);
            FileStream? fs = null;
            bool succeeded = false;

            try
            {
                fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                DownloadBitsAsync(request, fs, syncContext,
                    (result, error, state) =>
                    {
                        EndOperation();
                        if (error == null && !_canceled)
                        {
                            succeeded = true;
                        }
                        else if (!succeeded && File.Exists(fileName))
                        {
                            File.Delete(fileName);
                        }
                        if (syncContext != null)
                        {
                            syncContext.Post(_downloadFileOperationCompleted!,
                                new AsyncCompletedEventArgs(error, _canceled, state));
                        }
                    }, userToken);
            }
            catch (Exception)
            {
                fs?.Close();
                if (!succeeded && File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                EndOperation();
                throw;
            }
        }

        public void UploadStringAsync(Uri address, string data) => UploadStringAsync(address, "POST", data, null);
        public void UploadStringAsync(Uri address, string? method, string data) => UploadStringAsync(address, method, data, null);
        public void UploadStringAsync(Uri address, string? method, string data, object? userToken)
        {
            method ??= "POST";
            InitWebClientAsync();
            StartOperation();

            var syncContext = SynchronizationContext.Current;
            var request = CreateRequest(address, new HttpMethod(method));
            byte[] requestData = _encoding.GetBytes(data);
            _progress!.TotalBytesToSend = requestData.Length;

            UploadBitsAsync(request, null, requestData, 0, null, null, syncContext,
                (result, error, state) =>
                {
                    EndOperation();
                    string? stringResult = null;
                    if (error == null && result != null)
                    {
                        try
                        {
                            stringResult = GetStringUsingEncoding(result);
                        }
                        catch (Exception e)
                        {
                            error = e;
                        }
                    }
                    if (syncContext != null)
                    {
                        syncContext.Post(_uploadStringOperationCompleted!,
                            new UploadStringCompletedEventArgs(stringResult, error, _canceled, state));
                    }
                }, userToken);
        }

        public void UploadDataAsync(Uri address, byte[] data) => UploadDataAsync(address, "POST", data, null);
        public void UploadDataAsync(Uri address, string? method, byte[] data) => UploadDataAsync(address, method, data, null);
        public void UploadDataAsync(Uri address, string? method, byte[] data, object? userToken)
        {
            method ??= "POST";
            InitWebClientAsync();
            StartOperation();

            var syncContext = SynchronizationContext.Current;
            var request = CreateRequest(address, new HttpMethod(method));
            _progress!.TotalBytesToSend = data.Length;

            UploadBitsAsync(request, null, data, 0, null, null, syncContext,
                (result, error, state) =>
                {
                    EndOperation();
                    if (syncContext != null)
                    {
                        syncContext.Post(_uploadDataOperationCompleted!,
                            new UploadDataCompletedEventArgs(result, error, _canceled, state));
                    }
                }, userToken);
        }

        public void UploadFileAsync(Uri address, string fileName) => UploadFileAsync(address, "POST", fileName, null);
        public void UploadFileAsync(Uri address, string? method, string fileName) => UploadFileAsync(address, method, fileName, null);
        public void UploadFileAsync(Uri address, string? method, string fileName, object? userToken)
        {
            method ??= "POST";
            InitWebClientAsync();
            StartOperation();

            var syncContext = SynchronizationContext.Current;
            var request = CreateRequest(address, new HttpMethod(method));

            FileStream? fs = null;
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                _progress!.TotalBytesToSend = fs.Length;

                // 为 multipart/form-data 准备 header 和 footer
                string boundary = $"---------------------{DateTime.Now.Ticks:x}";
                request.Headers.TryAddWithoutValidation("Content-Type", $"{UploadFileContentType}; boundary={boundary}");

                string formHeader =
                    "--" + boundary + "\r\n" +
                    "Content-Disposition: form-data; name=\"file\"; filename=\"" + Path.GetFileName(fileName) + "\"\r\n" +
                    "Content-Type: " + DefaultUploadFileContentType + "\r\n" +
                    "\r\n";
                byte[] formHeaderBytes = Encoding.UTF8.GetBytes(formHeader);

                byte[] boundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

                UploadBitsAsync(request, fs, null, 0, formHeaderBytes, boundaryBytes, syncContext,
                    (result, error, state) =>
                    {
                        EndOperation();
                        if (syncContext != null)
                        {
                            syncContext.Post(_uploadFileOperationCompleted!,
                                new UploadFileCompletedEventArgs(result, error, _canceled, state));
                        }
                    }, userToken);
            }
            catch (Exception)
            {
                fs?.Close();
                EndOperation();
                throw;
            }
        }

        public void UploadValuesAsync(Uri address, NameValueCollection data) => UploadValuesAsync(address, "POST", data, null);
        public void UploadValuesAsync(Uri address, string? method, NameValueCollection data) => UploadValuesAsync(address, method, data, null);
        public void UploadValuesAsync(Uri address, string? method, NameValueCollection data, object? userToken)
        {
            method ??= "POST";
            InitWebClientAsync();
            StartOperation();

            var syncContext = SynchronizationContext.Current;
            var request = CreateRequest(address, new HttpMethod(method));

            // 构建表单数据
            var values = new StringBuilder();
            string delimiter = string.Empty;
            foreach (string? name in data.AllKeys)
            {
                values.Append(delimiter);
                values.Append(Uri.EscapeDataString(name));
                values.Append('=');
                values.Append(Uri.EscapeDataString(data[name]));
                delimiter = "&";
            }

            byte[] buffer = Encoding.ASCII.GetBytes(values.ToString());
            request.Headers.TryAddWithoutValidation("Content-Type", UploadValuesContentType);
            _progress!.TotalBytesToSend = buffer.Length;

            UploadBitsAsync(request, null, buffer, 0, null, null, syncContext,
                (result, error, state) =>
                {
                    EndOperation();
                    if (syncContext != null)
                    {
                        syncContext.Post(_uploadValuesOperationCompleted!,
                            new UploadValuesCompletedEventArgs(result, error, _canceled, state));
                    }
                }, userToken);
        }

        public void OpenReadAsync(Uri address) => OpenReadAsync(address, null);
        public void OpenReadAsync(Uri address, object? userToken)
        {
            InitWebClientAsync();
            StartOperation();

            var syncContext = SynchronizationContext.Current;

            Task.Run(async () =>
            {
                Stream? stream = null;
                Exception? error = null;

                try
                {
                    var request = CreateRequest(address, HttpMethod.Get);
                    var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                    stream = await response.Content.ReadAsStreamAsync();
                }
                catch (Exception e)
                {
                    error = e;
                }
                finally
                {
                    EndOperation();
                    if (syncContext != null)
                    {
                        syncContext.Post(_openReadOperationCompleted!,
                            new OpenReadCompletedEventArgs(stream, error, _canceled, userToken));
                    }
                }
            });
        }

        public void CancelAsync()
        {
            _canceled = true;
        }

        #endregion

        #region 辅助方法

        private HttpRequestMessage CreateRequest(Uri address, HttpMethod method)
        {
            Uri finalUri = BuildUri(address);
            var request = new HttpRequestMessage(method, finalUri);

            if (_headers != null)
            {
                foreach (string key in _headers.AllKeys)
                {
                    string value = _headers[key];
                    if (key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase))
                    {
                        request.Headers.TryAddWithoutValidation("User-Agent", value);
                    }
                    else if (key.Equals("Accept", StringComparison.OrdinalIgnoreCase))
                    {
                        request.Headers.Accept.TryParseAdd(value);
                    }
                    else if (!key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    {
                        request.Headers.TryAddWithoutValidation(key, value);
                    }
                }
            }

            return request;
        }

        private Uri BuildUri(Uri address)
        {
            Uri baseUri = address;

            if (!address.IsAbsoluteUri && _baseAddress != null)
            {
                baseUri = new Uri(_baseAddress, address);
            }

            if (_requestParameters != null && _requestParameters.Count > 0)
            {
                var builder = new UriBuilder(baseUri);
                var query = new StringBuilder();

                if (!string.IsNullOrEmpty(builder.Query))
                {
                    query.Append(builder.Query.TrimStart('?'));
                    query.Append('&');
                }

                bool first = query.Length == 0;
                foreach (string key in _requestParameters.AllKeys)
                {
                    if (!first) query.Append('&');
                    query.Append(Uri.EscapeDataString(key));
                    query.Append('=');
                    query.Append(Uri.EscapeDataString(_requestParameters[key]));
                    first = false;
                }

                builder.Query = query.ToString();
                return builder.Uri;
            }

            return baseUri;
        }

        private string GetStringUsingEncoding(byte[] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            Encoding encoding = _encoding;
            int bomLength = 0;

            if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
            {
                encoding = Encoding.UTF8;
                bomLength = 3;
            }
            else if (data.Length >= 2 && data[0] == 0xFF && data[1] == 0xFE)
            {
                encoding = Encoding.Unicode;
                bomLength = 2;
            }
            else if (data.Length >= 2 && data[0] == 0xFE && data[1] == 0xFF)
            {
                encoding = Encoding.BigEndianUnicode;
                bomLength = 2;
            }
            else if (data.Length >= 4 && data[0] == 0xFF && data[1] == 0xFE && data[2] == 0x00 && data[3] == 0x00)
            {
                encoding = Encoding.UTF32;
                bomLength = 4;
            }

            return encoding.GetString(data, bomLength, data.Length - bomLength);
        }

        #endregion

        #region 内部类

        private sealed class ProgressData
        {
            internal long BytesSent;
            internal long TotalBytesToSend = -1;
            internal long BytesReceived;
            internal long TotalBytesToReceive = -1;
            internal bool HasUploadPhase;

            internal void Reset()
            {
                BytesSent = 0;
                TotalBytesToSend = -1;
                BytesReceived = 0;
                TotalBytesToReceive = -1;
                HasUploadPhase = false;
            }
        }

        #endregion

        public void Dispose()
        {
            if (_disposeClient)
            {
                _httpClient?.Dispose();
            }
        }
    }

    #region 事件委托和事件参数类

    public delegate void OpenReadCompletedEventHandler(object sender, OpenReadCompletedEventArgs e);
    public delegate void OpenWriteCompletedEventHandler(object sender, OpenWriteCompletedEventArgs e);
    public delegate void DownloadStringCompletedEventHandler(object sender, DownloadStringCompletedEventArgs e);
    public delegate void DownloadDataCompletedEventHandler(object sender, DownloadDataCompletedEventArgs e);
    public delegate void UploadStringCompletedEventHandler(object sender, UploadStringCompletedEventArgs e);
    public delegate void UploadDataCompletedEventHandler(object sender, UploadDataCompletedEventArgs e);
    public delegate void UploadFileCompletedEventHandler(object sender, UploadFileCompletedEventArgs e);
    public delegate void UploadValuesCompletedEventHandler(object sender, UploadValuesCompletedEventArgs e);
    public delegate void DownloadProgressChangedEventHandler(object sender, DownloadProgressChangedEventArgs e);
    public delegate void UploadProgressChangedEventHandler(object sender, UploadProgressChangedEventArgs e);

    public class OpenReadCompletedEventArgs : AsyncCompletedEventArgs
    {
        private readonly Stream? _result;
        public OpenReadCompletedEventArgs(Stream? result, Exception? exception, bool cancelled, object? userToken)
            : base(exception, cancelled, userToken) { _result = result; }
        public Stream Result { get { RaiseExceptionIfNecessary(); return _result!; } }
    }

    public class OpenWriteCompletedEventArgs : AsyncCompletedEventArgs
    {
        private readonly Stream? _result;
        public OpenWriteCompletedEventArgs(Stream? result, Exception? exception, bool cancelled, object? userToken)
            : base(exception, cancelled, userToken) { _result = result; }
        public Stream Result { get { RaiseExceptionIfNecessary(); return _result!; } }
    }

    public class DownloadStringCompletedEventArgs : AsyncCompletedEventArgs
    {
        private readonly string? _result;
        public DownloadStringCompletedEventArgs(string? result, Exception? exception, bool cancelled, object? userToken)
            : base(exception, cancelled, userToken) { _result = result; }
        public string Result { get { RaiseExceptionIfNecessary(); return _result!; } }
    }

    public class DownloadDataCompletedEventArgs : AsyncCompletedEventArgs
    {
        private readonly byte[]? _result;
        public DownloadDataCompletedEventArgs(byte[]? result, Exception? exception, bool cancelled, object? userToken)
            : base(exception, cancelled, userToken) { _result = result; }
        public byte[] Result { get { RaiseExceptionIfNecessary(); return _result!; } }
    }

    public class UploadStringCompletedEventArgs : AsyncCompletedEventArgs
    {
        private readonly string? _result;
        public UploadStringCompletedEventArgs(string? result, Exception? exception, bool cancelled, object? userToken)
            : base(exception, cancelled, userToken) { _result = result; }
        public string Result { get { RaiseExceptionIfNecessary(); return _result!; } }
    }

    public class UploadDataCompletedEventArgs : AsyncCompletedEventArgs
    {
        private readonly byte[]? _result;
        public UploadDataCompletedEventArgs(byte[]? result, Exception? exception, bool cancelled, object? userToken)
            : base(exception, cancelled, userToken) { _result = result; }
        public byte[] Result { get { RaiseExceptionIfNecessary(); return _result!; } }
    }

    public class UploadFileCompletedEventArgs : AsyncCompletedEventArgs
    {
        private readonly byte[]? _result;
        public UploadFileCompletedEventArgs(byte[]? result, Exception? exception, bool cancelled, object? userToken)
            : base(exception, cancelled, userToken) { _result = result; }
        public byte[] Result { get { RaiseExceptionIfNecessary(); return _result!; } }
    }

    public class UploadValuesCompletedEventArgs : AsyncCompletedEventArgs
    {
        private readonly byte[]? _result;
        public UploadValuesCompletedEventArgs(byte[]? result, Exception? exception, bool cancelled, object? userToken)
            : base(exception, cancelled, userToken) { _result = result; }
        public byte[] Result { get { RaiseExceptionIfNecessary(); return _result!; } }
    }

    public class DownloadProgressChangedEventArgs : ProgressChangedEventArgs
    {
        public DownloadProgressChangedEventArgs(int progressPercentage, object? userToken, long bytesReceived, long totalBytesToReceive)
            : base(progressPercentage, userToken)
        {
            BytesReceived = bytesReceived;
            TotalBytesToReceive = totalBytesToReceive;
        }
        public long BytesReceived { get; }
        public long TotalBytesToReceive { get; }
    }

    public class UploadProgressChangedEventArgs : ProgressChangedEventArgs
    {
        public UploadProgressChangedEventArgs(int progressPercentage, object? userToken, long bytesSent, long totalBytesToSend, long bytesReceived, long totalBytesToReceive)
            : base(progressPercentage, userToken)
        {
            BytesReceived = bytesReceived;
            TotalBytesToReceive = totalBytesToReceive;
            BytesSent = bytesSent;
            TotalBytesToSend = totalBytesToSend;
        }
        public long BytesReceived { get; }
        public long TotalBytesToReceive { get; }
        public long BytesSent { get; }
        public long TotalBytesToSend { get; }
    }

    #endregion
}
