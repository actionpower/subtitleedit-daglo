using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    // ==================== 요청 모델 ====================

    public class AsyncTranscriptRequest
    {
        [JsonPropertyName("audio")]
        public AudioInfo Audio { get; set; }

        [JsonPropertyName("sttConfig")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SttConfig SttConfig { get; set; }

        [JsonPropertyName("nlpConfig")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public NlpConfig NlpConfig { get; set; }

        [JsonPropertyName("callback")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CallbackConfig Callback { get; set; }

        [JsonPropertyName("callbackUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [Obsolete("Use callback field instead")]
        public string CallbackUrl { get; set; }

        [JsonPropertyName("custom")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object> Custom { get; set; }
    }

    public class AudioInfo
    {
        [JsonPropertyName("source")]
        public AudioSource Source { get; set; }
    }

    public class AudioSource
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    // ==================== STT 설정 ====================

    public class SttConfig
    {
        /// <summary>
        /// 받아쓰기 모델 (기본값: general)
        /// </summary>
        [JsonPropertyName("model")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Model { get; set; }

        /// <summary>
        /// 언어 코드 (기본값: ko-KR)
        /// ko-KR, en-US, mixed, ja-JP, cmn-Hans-CN, de-DE, fr-FR, es-ES, it-IT, nl-NL, sv-SE, hi-IN, ru-RU, vi-VN, th-TH, id-ID, fa-IR, cs-CZ
        /// </summary>
        [JsonPropertyName("language")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Language { get; set; }

        /// <summary>
        /// 키워드 부스트 설정
        /// </summary>
        [JsonPropertyName("keywordBoost")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public KeywordBoost KeywordBoost { get; set; }

        /// <summary>
        /// 멀티 채널 설정
        /// </summary>
        [JsonPropertyName("multiChannel")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MultiChannel MultiChannel { get; set; }

        /// <summary>
        /// 화자 분리 설정
        /// </summary>
        [JsonPropertyName("speakerDiarization")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SpeakerDiarization SpeakerDiarization { get; set; }
    }

    public class KeywordBoost
    {
        /// <summary>
        /// 키워드 부스트 활성화 (필수: true)
        /// </summary>
        [JsonPropertyName("enable")]
        public bool Enable { get; set; }

        /// <summary>
        /// 강조할 키워드 목록
        /// </summary>
        [JsonPropertyName("keywords")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[] Keywords { get; set; }

        /// <summary>
        /// 부스트 강도 (1~15, 기본값: 7)
        /// </summary>
        [JsonPropertyName("boost")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Boost { get; set; }
    }

    public class MultiChannel
    {
        /// <summary>
        /// 채널 수 (1~2)
        /// </summary>
        [JsonPropertyName("channelCount")]
        public int ChannelCount { get; set; }
    }

    public class SpeakerDiarization
    {
        /// <summary>
        /// 화자 분리 활성화 (필수: true)
        /// </summary>
        [JsonPropertyName("enable")]
        public bool Enable { get; set; }

        /// <summary>
        /// 화자 수 힌트 (>= 2)
        /// </summary>
        [JsonPropertyName("speakerCountHint")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? SpeakerCountHint { get; set; }
    }

    // ==================== NLP 설정 ====================

    public class NlpConfig
    {
        /// <summary>
        /// 키워드 추출 설정
        /// </summary>
        [JsonPropertyName("keywordExtraction")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public KeywordExtraction KeywordExtraction { get; set; }

        /// <summary>
        /// 감정 분석 설정
        /// </summary>
        [JsonPropertyName("sentimentAnalysis")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SentimentAnalysis SentimentAnalysis { get; set; }
    }

    public class KeywordExtraction
    {
        /// <summary>
        /// 키워드 추출 활성화 (필수: true)
        /// </summary>
        [JsonPropertyName("enable")]
        public bool Enable { get; set; }

        /// <summary>
        /// 추출할 키워드 최대 개수
        /// </summary>
        [JsonPropertyName("maxCount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxCount { get; set; }
    }

    public class SentimentAnalysis
    {
        /// <summary>
        /// 감정 분석 활성화 (필수: true)
        /// </summary>
        [JsonPropertyName("enable")]
        public bool Enable { get; set; }
    }

    // ==================== 콜백 설정 ====================

    public class CallbackConfig
    {
        /// <summary>
        /// 콜백 URL (http(s) 포함)
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }

        /// <summary>
        /// 콜백 요청 시 포함할 헤더
        /// </summary>
        [JsonPropertyName("headers")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string> Headers { get; set; }
    }

    // ==================== 응답 모델 ====================

    public class TranscriptResponse
    {
        /// <summary>
        /// 요청 ID
        /// </summary>
        [JsonPropertyName("rid")]
        public string Rid { get; set; }

        /// <summary>
        /// 파일 이름
        /// </summary>
        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        /// <summary>
        /// 사용자 정의 객체
        /// </summary>
        [JsonPropertyName("custom")]
        public Dictionary<string, object> Custom { get; set; }
    }

    public class TranscriptResult
    {
        [JsonPropertyName("rid")]
        public string Rid { get; set; }

        /// <summary>
        /// 상태: ai_requested, uploaded, file_processing, transcribing, post_processing, transcribed, transcript_error, file_error
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("sttResults")]
        public SttResult[] SttResults { get; set; }

        [JsonPropertyName("custom")]
        public Dictionary<string, object> Custom { get; set; }
    }

    public class SttResult
    {
        [JsonPropertyName("transcript")]
        public string Transcript { get; set; }

        [JsonPropertyName("words")]
        public Word[] Words { get; set; }

        [JsonPropertyName("keywords")]
        public string[] Keywords { get; set; }

        [JsonPropertyName("sentiment")]
        public string Sentiment { get; set; }

        [JsonPropertyName("sentimentScore")]
        public SentimentScore SentimentScore { get; set; }
    }

    public class Word
    {
        [JsonPropertyName("word")]
        public string Text { get; set; }

        [JsonPropertyName("startTime")]
        public TimeInfo StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public TimeInfo EndTime { get; set; }

        [JsonPropertyName("segmentId")]
        public string SegmentId { get; set; }

        [JsonPropertyName("speaker")]
        public string Speaker { get; set; }
    }

    public class TimeInfo
    {
        [JsonPropertyName("nanos")]
        public long Nanos { get; set; }

        [JsonPropertyName("seconds")]
        public string Seconds { get; set; }
    }

    public class SentimentScore
    {
        [JsonPropertyName("positive")]
        public double Positive { get; set; }

        [JsonPropertyName("negative")]
        public double Negative { get; set; }

        [JsonPropertyName("neutral")]
        public double Neutral { get; set; }
    }

    // ==================== Daglo STT Client ====================

    /// <summary>
    /// Daglo Cloud API Speech-to-Text implementation
    /// </summary>
    public class DagloTranscribe : IAutoTranscriber, IDisposable
    {
        private const string BaseUrl = "https://apis.daglo.ai";
        private string _apiKey;
        private HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public static string StaticName { get; set; } = "Daglo Cloud API Speech-to-Text";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => BaseUrl;
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        public DagloTranscribe()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// 환경 변수 또는 .env 파일에서 Daglo API 키를 읽습니다.
        /// 우선순위: 환경 변수 > .env 파일 > 설정
        /// </summary>
        private static string GetDagloApiKeyFromEnvironment()
        {
            // 1. 환경 변수에서 읽기
            var envKey = Environment.GetEnvironmentVariable("DAGLO_API_KEY");
            if (!string.IsNullOrWhiteSpace(envKey))
            {
                return envKey.Trim();
            }

            // 2. .env 파일에서 읽기 (BaseDirectory/.env 또는 DataDirectory/.env)
            try
            {
                var envFiles = new[]
                {
                    Path.Combine(Configuration.BaseDirectory, ".env"),
                    Path.Combine(Configuration.DataDirectory, ".env")
                };

                foreach (var envFile in envFiles)
                {
                    if (File.Exists(envFile))
                    {
                        var lines = File.ReadAllLines(envFile);
                        foreach (var line in lines)
                        {
                            var trimmedLine = line.Trim();
                            // 주석 무시
                            if (trimmedLine.StartsWith("#", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(trimmedLine))
                            {
                                continue;
                            }

                            // KEY=VALUE 형식 파싱
                            var equalIndex = trimmedLine.IndexOf('=');
                            if (equalIndex > 0 && equalIndex < trimmedLine.Length - 1)
                            {
                                var key = trimmedLine.Substring(0, equalIndex).Trim();
                                var value = trimmedLine.Substring(equalIndex + 1).Trim();
                                
                                // 따옴표 제거
                                if ((value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal)) ||
                                    (value.StartsWith("'", StringComparison.Ordinal) && value.EndsWith("'", StringComparison.Ordinal)))
                                {
                                    value = value.Substring(1, value.Length - 2);
                                }

                                if (key.Equals("DAGLO_API_KEY", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(value))
                                {
                                    return value;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // 파일 읽기 실패 시 무시
            }

            // 3. 기존 설정에서 읽기
            return Configuration.Settings.Tools.DagloApiKey;
        }

        public void Initialize()
        {
            _apiKey = GetDagloApiKeyFromEnvironment();

            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Daglo API key is not configured. Please set DAGLO_API_KEY environment variable, add it to .env file, or configure it in the settings.");

            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.Timeout = TimeSpan.FromMinutes(30);
            _httpClient.BaseAddress = new Uri(BaseUrl + "/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey.Trim());
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return new List<TranslationPair>
            {
                new TranslationPair("KOREAN", "ko-KR"),
                new TranslationPair("ENGLISH", "en-US"),
                new TranslationPair("JAPANESE", "ja-JP"),
                new TranslationPair("CHINESE", "cmn-Hans-CN"),
                new TranslationPair("GERMAN", "de-DE"),
                new TranslationPair("FRENCH", "fr-FR"),
                new TranslationPair("SPANISH", "es-ES"),
                new TranslationPair("ITALIAN", "it-IT"),
                new TranslationPair("DUTCH", "nl-NL"),
                new TranslationPair("SWEDISH", "sv-SE"),
            };
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return GetSupportedSourceLanguages();
        }

        /// <summary>
        /// Transcribe audio file. The 'text' parameter should contain the file path.
        /// </summary>
        public async Task<string> Transcribe(string text, string sourceLanguageCode, string targetLanguageCode)
        {
            if (_httpClient == null)
                throw new InvalidOperationException("Initialize must be called first");

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("File path cannot be null or empty", nameof(text));

            if (!File.Exists(text))
                throw new FileNotFoundException("File not found", text);

            try
            {
                // Convert language code to Daglo format (e.g., "ko" -> "ko-KR", "en" -> "en-US")
                var languageCode = ConvertLanguageCode(sourceLanguageCode);

                var sttConfig = new SttConfig
                {
                    Model = "general",
                    Language = languageCode
                };

                // Upload file and get RID
                var response = await CreateTranscriptFromFileAsync(text, sttConfig, null, null, null).ConfigureAwait(false);
                var rid = response.Rid;

                // Wait for transcription to complete
                var result = await PollUntilTerminalAsync(
                    rid,
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromMinutes(30),
                    null,
                    CancellationToken.None).ConfigureAwait(false);

                // Extract transcript text
                if (result.Status == "transcribed" && result.SttResults != null && result.SttResults.Length > 0)
                {
                    var transcripts = result.SttResults
                        .Where(r => !string.IsNullOrEmpty(r.Transcript))
                        .Select(r => r.Transcript)
                        .ToList();

                    return string.Join(Environment.NewLine, transcripts);
                }

                throw new InvalidOperationException($"Transcription failed with status: {result.Status}");
            }
            catch (HttpRequestException ex)
            {
                Error = ex.Message;
                SeLogger.Error("DagloTranscribe error: " + ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                SeLogger.Error(ex, "DagloTranscribe error: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Convert language code to Daglo format (e.g., "ko" -> "ko-KR")
        /// </summary>
        private string ConvertLanguageCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "ko-KR";

            // If already in correct format, return as is
            if (code.Contains("-"))
                return code;

            // Convert simple codes to full format
            switch (code.ToLowerInvariant())
            {
                case "ko": return "ko-KR";
                case "en": return "en-US";
                case "ja": return "ja-JP";
                case "zh": return "cmn-Hans-CN";
                case "de": return "de-DE";
                case "fr": return "fr-FR";
                case "es": return "es-ES";
                case "it": return "it-IT";
                case "nl": return "nl-NL";
                case "sv": return "sv-SE";
                case "hi": return "hi-IN";
                case "ru": return "ru-RU";
                case "vi": return "vi-VN";
                case "th": return "th-TH";
                case "id": return "id-ID";
                case "fa": return "fa-IR";
                case "cs": return "cs-CZ";
                default: return "ko-KR"; // default
            }
        }

        /// <summary>
        /// 음성 변환 요청 - multipart/form-data 방식 (파일 직접 업로드)
        /// </summary>
        public async Task<TranscriptResponse> CreateTranscriptFromFileAsync(
            string filePath,
            SttConfig sttConfig = null,
            NlpConfig nlpConfig = null,
            CallbackConfig callback = null,
            Dictionary<string, object> custom = null,
            CancellationToken ct = default(CancellationToken))
        {
            if (_httpClient == null)
                throw new InvalidOperationException("Initialize must be called first");

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            var fileInfo = new FileInfo(filePath);
            Debug.WriteLine($"[CreateTranscriptFromFile] 파일 크기: {fileInfo.Length / 1024.0 / 1024.0:F2} MB");

            var multipartContent = new MultipartFormDataContent();
            try
            {
                // 파일 추가 - StreamContent 사용으로 메모리 효율적 처리
                Debug.WriteLine("[CreateTranscriptFromFile] 파일 읽기 시작...");
                var fileStream = File.OpenRead(filePath);
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(GuessMimeType(Path.GetExtension(filePath)));
                multipartContent.Add(fileContent, "file", Path.GetFileName(filePath));
                Debug.WriteLine("[CreateTranscriptFromFile] 파일 스트림 준비 완료");

                // sttConfig 추가
                if (sttConfig != null)
                {
                    var sttConfigJson = JsonSerializer.Serialize(sttConfig, _jsonOptions);
                    multipartContent.Add(new StringContent(sttConfigJson, Encoding.UTF8, "application/json"), "sttConfig");
                }

                // nlpConfig 추가
                if (nlpConfig != null)
                {
                    var nlpConfigJson = JsonSerializer.Serialize(nlpConfig, _jsonOptions);
                    multipartContent.Add(new StringContent(nlpConfigJson, Encoding.UTF8, "application/json"), "nlpConfig");
                }

                // callback 추가
                if (callback != null)
                {
                    var callbackJson = JsonSerializer.Serialize(callback, _jsonOptions);
                    multipartContent.Add(new StringContent(callbackJson, Encoding.UTF8, "application/json"), "callback");
                }

                // custom 추가
                if (custom != null)
                {
                    var customJson = JsonSerializer.Serialize(custom, _jsonOptions);
                    multipartContent.Add(new StringContent(customJson, Encoding.UTF8, "application/json"), "custom");
                }

                Debug.WriteLine("[CreateTranscriptFromFile] HTTP 요청 전송 시작...");
                using (var req = new HttpRequestMessage(HttpMethod.Post, "stt/v1/async/transcripts"))
                {
                    req.Content = multipartContent;
                    using (var resp = await _httpClient.SendAsync(req, ct).ConfigureAwait(false))
                    {
                        Debug.WriteLine($"[CreateTranscriptFromFile] HTTP 응답 받음: {resp.StatusCode}");
                        await EnsureSuccessWithDetails(resp).ConfigureAwait(false);
                        var payload = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var dto = JsonSerializer.Deserialize<TranscriptResponse>(payload, _jsonOptions);
                        if (dto == null) throw new InvalidOperationException("Invalid JSON response (init/file)");
                        Debug.WriteLine($"[CreateTranscriptFromFile] 업로드 완료! RID: {dto.Rid}");
                        return dto;
                    }
                }
            }
            finally
            {
                multipartContent.Dispose();
            }
        }

        /// <summary>
        /// 음성 변환 요청 - application/json 방식 (URL 사용)
        /// </summary>
        public async Task<TranscriptResponse> CreateTranscriptFromUrlAsync(
            AsyncTranscriptRequest request,
            CancellationToken ct = default(CancellationToken))
        {
            if (_httpClient == null)
                throw new InvalidOperationException("Initialize must be called first");

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Audio?.Source?.Url == null)
                throw new ArgumentException("Audio source URL is required", nameof(request));

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var req = new HttpRequestMessage(HttpMethod.Post, "stt/v1/async/transcripts"))
            {
                req.Content = content;
                using (var resp = await _httpClient.SendAsync(req, ct).ConfigureAwait(false))
                {
                    await EnsureSuccessWithDetails(resp).ConfigureAwait(false);
                    var payload = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var dto = JsonSerializer.Deserialize<TranscriptResponse>(payload, _jsonOptions);
                    if (dto == null) throw new InvalidOperationException("Invalid JSON response (init/url)");
                    return dto;
                }
            }
        }

        /// <summary>
        /// 간단한 URL 변환 요청 (오버로드)
        /// </summary>
        public async Task<TranscriptResponse> CreateTranscriptFromUrlAsync(
            string audioUrl,
            SttConfig sttConfig = null,
            NlpConfig nlpConfig = null,
            CallbackConfig callback = null,
            Dictionary<string, object> custom = null,
            CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(audioUrl))
                throw new ArgumentException("Audio URL cannot be null or empty", nameof(audioUrl));

            var request = new AsyncTranscriptRequest
            {
                Audio = new AudioInfo
                {
                    Source = new AudioSource { Url = audioUrl }
                },
                SttConfig = sttConfig,
                NlpConfig = nlpConfig,
                Callback = callback,
                Custom = custom
            };

            return await CreateTranscriptFromUrlAsync(request, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// 변환 결과 조회 (GET)
        /// </summary>
        public async Task<TranscriptResult> GetTranscriptAsync(
            string rid,
            CancellationToken ct = default(CancellationToken))
        {
            if (_httpClient == null)
                throw new InvalidOperationException("Initialize must be called first");

            if (string.IsNullOrWhiteSpace(rid))
                throw new ArgumentException("RID cannot be null or empty", nameof(rid));

            using (var resp = await _httpClient.GetAsync(
                "stt/v1/async/transcripts/" + Uri.EscapeDataString(rid),
                ct).ConfigureAwait(false))
            {
                await EnsureSuccessWithDetails(resp).ConfigureAwait(false);
                var payload = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                var dto = JsonSerializer.Deserialize<TranscriptResult>(payload, _jsonOptions);
                if (dto == null) throw new InvalidOperationException("Invalid JSON response (get)");
                return dto;
            }
        }

        /// <summary>
        /// 변환 완료까지 대기 (폴링) - TimeSpan 기반, 유연한 설정
        /// </summary>
        public async Task<TranscriptResult> PollUntilTerminalAsync(
            string rid,
            TimeSpan? interval = null,
            TimeSpan? timeout = null,
            Action<TranscriptResult> onProgress = null,
            CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(rid))
                throw new ArgumentException("RID cannot be null or empty", nameof(rid));

            var pollInterval = interval ?? TimeSpan.FromSeconds(5);
            var pollTimeout = timeout ?? TimeSpan.FromMinutes(30);
            var start = DateTime.UtcNow;

            var terminals = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "transcribed", "transcript_error", "file_error"
            };

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                var result = await GetTranscriptAsync(rid, ct).ConfigureAwait(false);
                
                if (onProgress != null)
                    onProgress(result);

                var status = result.Status ?? string.Empty;
                if (terminals.Contains(status))
                {
                    if (status == "transcript_error")
                        throw new InvalidOperationException("받아쓰기 중 오류가 발생했습니다.");
                    if (status == "file_error")
                        throw new InvalidOperationException("파일에 오류가 있습니다.");
                    return result;
                }

                if (DateTime.UtcNow - start > pollTimeout)
                    throw new TimeoutException($"Polling timed out after {pollTimeout}");

                await Task.Delay(pollInterval, ct).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 변환 완료까지 대기 (폴링) - 간단한 버전 (초 단위)
        /// </summary>
        public async Task<TranscriptResult> WaitForTranscriptAsync(
            string rid,
            int pollingIntervalSeconds = 5,
            int maxRetries = 120,
            CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(rid))
                throw new ArgumentException("RID cannot be null or empty", nameof(rid));

            var pollInterval = TimeSpan.FromSeconds(pollingIntervalSeconds);
            var maxDuration = TimeSpan.FromSeconds(maxRetries * pollingIntervalSeconds);

            return await PollUntilTerminalAsync(
                rid,
                pollInterval,
                maxDuration,
                s => Debug.WriteLine($"상태: {s.Status}"),
                ct).ConfigureAwait(false);
        }

        private static async Task EnsureSuccessWithDetails(HttpResponseMessage resp)
        {
            if (resp.IsSuccessStatusCode) return;
            var body = resp.Content != null 
                ? await resp.Content.ReadAsStringAsync().ConfigureAwait(false) 
                : "";
            throw new HttpRequestException(
                $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {body}");
        }

        private static string GuessMimeType(string ext)
        {
            if (string.IsNullOrEmpty(ext)) return "application/octet-stream";
            ext = ext.ToLowerInvariant();
            switch (ext)
            {
                case ".wav": return "audio/wav";
                case ".mp3": return "audio/mpeg";
                case ".m4a": return "audio/mp4";
                case ".flac": return "audio/flac";
                case ".aac": return "audio/aac";
                case ".opus": return "audio/opus";
                case ".weba": return "audio/webm";
                case ".mp4": return "video/mp4";
                case ".mov": return "video/quicktime";
                default: return "application/octet-stream";
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        /// <summary>
        /// 파일 업로드를 통한 STT 테스트
        /// </summary>
        public static async Task TestFileUploadAsync(string filePath = null, string language = "ko-KR")
        { 
            if (!File.Exists(filePath))
            {
                Debug.WriteLine($"파일을 찾을 수 없습니다: {filePath}");
                return;
            }

            var transcriber = new DagloTranscribe();
            try
            {
                transcriber.Initialize();

                // STT 설정
                var sttConfig = new SttConfig
                {
                    Model = "general",
                    Language = language
                    // 필요시 추가 설정:
                    // SpeakerDiarization = new SpeakerDiarization { Enable = true },
                    // KeywordBoost = new KeywordBoost 
                    // { 
                    //     Enable = true, 
                    //     Keywords = new[] { "키워드1", "키워드2" },
                    //     Boost = 10
                    // }
                };

                // NLP 설정 (옵션)
                var nlpConfig = new NlpConfig
                {
                    // KeywordExtraction = new KeywordExtraction { Enable = true, MaxCount = 10 },
                    // SentimentAnalysis = new SentimentAnalysis { Enable = true }
                };

                // Custom 데이터 (옵션)
                var custom = new Dictionary<string, object>
                {
                    { "test_source", "file_upload_test" },
                    { "test_timestamp", DateTime.UtcNow.ToString("O") }
                };

                Debug.WriteLine($"=== 파일 업로드 STT 테스트 시작 ===");
                Debug.WriteLine($"파일 경로: {filePath}");

                var fileInfo = new FileInfo(filePath);
                Debug.WriteLine($"파일 크기: {fileInfo.Length / 1024.0 / 1024.0:F2} MB");

                // 1. 파일 업로드 및 요청 시작
                Debug.WriteLine("[1/3] 파일 업로드 시작...");

                var response = await transcriber.CreateTranscriptFromFileAsync(
                    filePath,
                    sttConfig,
                    nlpConfig,
                    callback: null,
                    custom
                ).ConfigureAwait(false);

                Debug.WriteLine($"[1/3] 업로드 완료! RID: {response.Rid}");
                if (!string.IsNullOrEmpty(response.FileName))
                {
                    Debug.WriteLine($"파일명: {response.FileName}");
                }
                
                // 2. 폴링으로 완료 대기
                Debug.WriteLine("[2/3] 변환 완료 대기 중...");
                Debug.WriteLine("(상태는 5초마다 업데이트됩니다)");

                var result = await transcriber.PollUntilTerminalAsync(
                    response.Rid,
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromMinutes(30),
                    onProgress: (progress) =>
                    {  
                        Debug.WriteLine($"  상태: {progress.Status}");
                    }
                ).ConfigureAwait(false);

                // 3. 결과 출력
                Debug.WriteLine("[3/3] 결과 출력");
                Debug.WriteLine($"[3/3] 최종 상태: {result.Status}");

                if (result.Status == "transcribed" && result.SttResults != null && result.SttResults.Length > 0)
                { 
                    var srtContent = DagloTranscribe.ConvertToSrt(result.SttResults);

                    string srtFileName = Path.GetFileNameWithoutExtension(filePath) + ".srt";
                    File.WriteAllText(srtFileName, srtContent, Encoding.UTF8);

                    Debug.WriteLine("\n=== 전사 결과 ===");
                    foreach (var sttResult in result.SttResults)
                    {
                        if (!string.IsNullOrEmpty(sttResult.Transcript))
                        {
                            Debug.WriteLine($"\n전사 텍스트:");
                            Debug.WriteLine($"  {sttResult.Transcript}");
                        }

                        // 키워드가 있으면 출력
                        if (sttResult.Keywords != null && sttResult.Keywords.Length > 0)
                        {
                            Debug.WriteLine($"\n추출된 키워드:");
                            Debug.WriteLine($"  {string.Join(", ", sttResult.Keywords)}");
                        }

                        // 감정 분석 결과가 있으면 출력
                        if (!string.IsNullOrEmpty(sttResult.Sentiment))
                        {
                            Debug.WriteLine($"\n감정 분석:");
                            Debug.WriteLine($"  감정: {sttResult.Sentiment}");
                            if (sttResult.SentimentScore != null)
                            {
                                Debug.WriteLine($"  긍정: {sttResult.SentimentScore.Positive:F2}%");
                                Debug.WriteLine($"  부정: {sttResult.SentimentScore.Negative:F2}%");
                                Debug.WriteLine($"  중립: {sttResult.SentimentScore.Neutral:F2}%");
                            }
                        }

                        // 단어별 타임스탬프가 있으면 출력
                        if (sttResult.Words != null && sttResult.Words.Length > 0)
                        {
                            Debug.WriteLine($"\n단어별 타임스탬프 (처음 10개):");
                            var wordsToShow = sttResult.Words.Take(10);
                            foreach (var word in wordsToShow)
                            {
                                if (word.StartTime != null)
                                {
                                    var startSec = double.TryParse(word.StartTime.Seconds, out var sec) 
                                        ? sec + (word.StartTime.Nanos / 1_000_000_000.0) 
                                        : 0.0;
                                    var speaker = !string.IsNullOrEmpty(word.Speaker) ? $" [화자{word.Speaker}]" : "";
                                    Debug.WriteLine($"  [{startSec:F2}s]{speaker} {word.Text}");
                                }
                            }
                            if (sttResult.Words.Length > 10)
                            {
                                Debug.WriteLine($"  ... 외 {sttResult.Words.Length - 10}개 더");
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("\n전사 결과가 없습니다.");
                    Debug.WriteLine($"상태: {result.Status}");
                }

                Debug.WriteLine("=== 테스트 완료 ===");
            }
            catch (FileNotFoundException ex)
            {
                Debug.WriteLine($"파일 오류: {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"타임아웃: {ex.Message}");
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HTTP 오류: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"오류 발생: {ex.Message}");
                Debug.WriteLine($"스택 추적: {ex.StackTrace}");
            }
            finally
            {
                transcriber?.Dispose();
            }
        }

        /// <summary>
        /// SttResult를 .srt 자막 파일 형식으로 변환합니다.
        /// </summary>
        /// <param name="sttResults">변환할 SttResult 배열</param>
        /// <returns>.srt 형식의 문자열</returns>
        public static string ConvertToSrt(SttResult[] sttResults)
        {
            if (sttResults == null || sttResults.Length == 0)
                return string.Empty;

            var sb = new StringBuilder();
            int subtitleNumber = 1;

            foreach (var sttResult in sttResults)
            {
                if (sttResult == null)
                    continue;

                // Words 배열이 있고 타임스탬프 정보가 있는 경우
                if (sttResult.Words != null && sttResult.Words.Length > 0)
                {
                    // SegmentId로 그룹화하거나, 시간 순서로 정렬
                    var wordsWithTime = sttResult.Words
                        .Where(w => w != null && w.StartTime != null && w.EndTime != null)
                        .OrderBy(w => GetTotalSeconds(w.StartTime))
                        .ToList();

                    if (wordsWithTime.Count > 0)
                    {
                        // SegmentId로 그룹화
                        var segments = wordsWithTime
                            .GroupBy(w => w.SegmentId ?? string.Empty)
                            .ToList();

                        foreach (var segment in segments)
                        {
                            var segmentWords = segment.OrderBy(w => GetTotalSeconds(w.StartTime)).ToList();
                            if (segmentWords.Count == 0)
                                continue;

                            var startTime = segmentWords.First().StartTime;
                            var endTime = segmentWords.Last().EndTime;
                            var text = string.Join(" ", segmentWords.Select(w => w.Text ?? string.Empty).Where(t => !string.IsNullOrWhiteSpace(t)));

                            if (string.IsNullOrWhiteSpace(text))
                                continue;

                            // SRT 형식으로 추가
                            sb.AppendLine(subtitleNumber.ToString());
                            sb.AppendLine($"{FormatTimeCode(startTime)} --> {FormatTimeCode(endTime)}");
                            sb.AppendLine(text);
                            sb.AppendLine();
                            subtitleNumber++;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(sttResult.Transcript))
                    {
                        // Words가 있지만 타임스탬프가 없는 경우, Transcript만 사용
                        // 기본 시간 설정 (0초부터 시작, 3초 지속)
                        sb.AppendLine(subtitleNumber.ToString());
                        sb.AppendLine($"00:00:00,000 --> 00:00:03,000");
                        sb.AppendLine(sttResult.Transcript);
                        sb.AppendLine();
                        subtitleNumber++;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(sttResult.Transcript))
                {
                    // Words 배열이 없는 경우, Transcript만 사용
                    // 기본 시간 설정 (0초부터 시작, 3초 지속)
                    sb.AppendLine(subtitleNumber.ToString());
                    sb.AppendLine($"00:00:00,000 --> 00:00:03,000");
                    sb.AppendLine(sttResult.Transcript);
                    sb.AppendLine();
                    subtitleNumber++;
                }
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// 단일 SttResult를 .srt 자막 파일 형식으로 변환합니다.
        /// </summary>
        /// <param name="sttResult">변환할 SttResult</param>
        /// <returns>.srt 형식의 문자열</returns>
        public static string ConvertToSrt(SttResult sttResult)
        {
            if (sttResult == null)
                return string.Empty;

            return ConvertToSrt(new[] { sttResult });
        }

        /// <summary>
        /// TimeInfo를 초 단위로 변환합니다.
        /// </summary>
        private static double GetTotalSeconds(TimeInfo timeInfo)
        {
            if (timeInfo == null)
                return 0.0;

            var seconds = 0.0;
            if (!string.IsNullOrWhiteSpace(timeInfo.Seconds) && double.TryParse(timeInfo.Seconds, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var sec))
            {
                seconds = sec;
            }

            var nanos = timeInfo.Nanos / 1_000_000_000.0;
            return seconds + nanos;
        }

        /// <summary>
        /// TimeInfo를 SRT 형식의 시간 코드 문자열로 변환합니다 (HH:MM:SS,mmm).
        /// </summary>
        private static string FormatTimeCode(TimeInfo timeInfo)
        {
            if (timeInfo == null)
                return "00:00:00,000";

            var totalSeconds = GetTotalSeconds(timeInfo);
            var totalMilliseconds = (long)(totalSeconds * 1000);

            var hours = totalMilliseconds / 3_600_000;
            var minutes = (totalMilliseconds % 3_600_000) / 60_000;
            var seconds = (totalMilliseconds % 60_000) / 1_000;
            var milliseconds = totalMilliseconds % 1_000;

            return $"{hours:00}:{minutes:00}:{seconds:00},{milliseconds:000}";
        }
    }
}
