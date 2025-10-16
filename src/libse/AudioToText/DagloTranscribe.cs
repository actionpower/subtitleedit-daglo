using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    /// <summary>
    /// Google translate via Google Cloud V2 API - see https://cloud.google.com/translate/
    /// </summary>
    public class DagloTranscribe : IAutoTranscriber, IDisposable
    {
        private string _apiKey;
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "Daglo Cloud API Speech-to-Text";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://apis.daglo.ai/";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        public void Initialize()
        {
            _apiKey = Configuration.Settings.Tools.DagloApiKey;
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.BaseAddress = new Uri(this.Url);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
              
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "DeepL-Auth-Key " + _apiKey.Trim()); 
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return new List<TranslationPair>
            {
                new TranslationPair("ENGLISH", "en"),
                new TranslationPair("KOREAN", "ko"),
            };
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return new List<TranslationPair>
            {                
                new TranslationPair("ENGLISH", "en"),
                new TranslationPair("KOREAN", "ko"),
            };
        }


        private static TranslationPair MakeTranslationPair(string name, string code)
        {
            return new TranslationPair(name, code, code);
        }

        private static TranslationPair MakeTranslationPair(string name, string code, bool hasFormality)
        {
            return new TranslationPair(name, code, hasFormality);
        }

        public async Task<string> Transcribe(string text, string sourceLanguageCode, string targetLanguageCode)
        {
            int[] retryDelays = { 555, 3007, 7013 };
            HttpResponseMessage result = null;
            string resultContent = null;
            for (var attempt = 0; attempt <= retryDelays.Length; attempt++)
            {
                var postContent = MakeContent(text, sourceLanguageCode, targetLanguageCode);
                result = await _httpClient.PostAsync("/v2/translate", postContent);
                resultContent = await result.Content.ReadAsStringAsync();

                if (!ShouldRetry(result, resultContent) || attempt == retryDelays.Length)
                {
                    break;
                }

                await Task.Delay(retryDelays[attempt]);
            }

            if (!result.IsSuccessStatusCode)
            {
                SeLogger.Error("DeepLTranslate error: " + resultContent);
            }

            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                Error = resultContent;
                throw new Exception("Forbidden! " + Environment.NewLine + Environment.NewLine + resultContent);
            }

            try
            {
                var resultList = new List<string>();
                var parser = new JsonParser();
                var x = (Dictionary<string, object>)parser.Parse(resultContent);
                foreach (var k in x.Keys)
                {
                    if (x[k] is List<object> mainList)
                    {
                        foreach (var mainListItem in mainList)
                        {
                            if (mainListItem is Dictionary<string, object> innerDic)
                            {
                                foreach (var transItem in innerDic.Keys)
                                {
                                    if (transItem == "text")
                                    {
                                        var s = innerDic[transItem].ToString();
                                        resultList.Add(s);
                                    }
                                }
                            }
                        }
                    }
                }

                return string.Join(Environment.NewLine, resultList);
            }
            catch (Exception ex)
            {
                SeLogger.Error(ex, "DeepLTranslate.Translate: " + ex.Message + Environment.NewLine + resultContent);
                throw;
            }
        }

        public static bool ShouldRetry(HttpResponseMessage result, string resultContent)
        {
            const int httpStatusCodeTooManyRequests = 429;

            return result.StatusCode == HttpStatusCode.ServiceUnavailable ||
                   (int)result.StatusCode == httpStatusCodeTooManyRequests ||
                   (result != null && resultContent.Contains("<head><title>429 Too Many Requests</title></head>", StringComparison.Ordinal));
        }

        private FormUrlEncodedContent MakeContent(string text, string sourceLanguageCode, string targetLanguageCode)
        {
            var array = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("text", text),
                new KeyValuePair<string, string>("target_lang", targetLanguageCode),
                new KeyValuePair<string, string>("source_lang", sourceLanguageCode),
            };

            var targetLanguages = GetSupportedTargetLanguages();
            /*
            var hasFormality = targetLanguages.Find(x => x.Code.Equals(targetLanguageCode, StringComparison.OrdinalIgnoreCase))?.HasFormality ?? false;
            if (hasFormality && !string.IsNullOrEmpty(_formality))
            {
                array.Add(new KeyValuePair<string, string>("formality", _formality));
            }
            */

            return new FormUrlEncodedContent(array);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    
    }
}
