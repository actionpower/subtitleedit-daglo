// ============================================================================
// 헬로비전(HelloVision) 자막 변환 룰 (Daglo STT words -> 자막)
//
// Daglo STT 결과(word: 텍스트/화자/시작·종료 시각)를 헬로비전 납품 규격에
// 맞는 자막 단위로 조립한다. 호출 지점: DagloTranscribe.ConvertToSrt.
//
// 파라미터:
//   maxCharsPerLine : 한 줄 최대 글자수(한글 기준). 한 줄 최대 바이트 = maxCharsPerLine * 2.
//   maxLines        : 한 자막 최대 줄 수. 한 자막 최대 바이트 = maxLines * 한 줄 최대 바이트.
//   (헬로비전 기본값: 25글자 / 2줄 -> 한 줄 50바이트, 한 자막 100바이트)
//
// 길이는 "글자 수"가 아니라 CP949(ANSI) 저장 기준 "바이트"로 센다:
//   ASCII = 1바이트, 그 외(한글·한자·자모·전각문자 등) = 2바이트.
//   예) "LG 트윈스" = L(1)+G(1)+공백(1)+트(2)+윈(2)+스(2) = 9바이트
//
// 처리 단계:
//   1) 덩어리(chunk) 나누기 - 화자 변경 / 문장 끝(. ?) / 누적이 한 자막 최대 바이트를
//      넘을 때 분리. (마침표 앞뒤가 모두 숫자면 소수점으로 보고 끊지 않음. 예: 6.3)
//   2) 덩어리 -> 자막(들) - (전체 바이트 / 최대 줄 수)를 목표로 최대 줄 수까지 균등 분배.
//      어절 경계 때문에 한 줄 최대 바이트를 넘는 배치밖에 없으면 그 직전까지 자막을
//      확정하고 남은 어절을 재귀 처리. 어절(word) 중간은 절대 자르지 않는다.
//   3) 타임코드 - 각 자막의 첫 word.start / 마지막 word.end 실제 값 사용.
//
// 결과 보장: 모든 줄은 한 줄 최대 바이트 이하(단일 어절이 그보다 긴 극단 케이스 제외),
//            모든 자막은 최대 줄 수 이하, 타임코드는 word 단위 실제 값.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public class HelloVisionWord
    {
        public string Text { get; set; }
        public string Speaker { get; set; }
        public double StartSeconds { get; set; }
        public double EndSeconds { get; set; }
    }

    public class HelloVisionSubtitle
    {
        public List<string> Lines { get; set; }        // 화면 줄들 (최대 줄 수 이하)
        public double StartSeconds { get; set; }        // 첫 word 의 start
        public double EndSeconds { get; set; }          // 마지막 word 의 end
    }

    public static class HelloVisionRule
    {
        /// <summary>
        /// CP949(ANSI) 저장 기준 바이트 길이: ASCII = 1바이트, 그 외 = 2바이트.
        /// (CP949 에 없는 문자는 저장 시 '?' 1바이트가 되지만, 넉넉하게 2로 센다)
        /// </summary>
        public static int ByteLen(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return 0;
            }

            var n = 0;
            foreach (var ch in s)
            {
                n += ch <= 0x7F ? 1 : 2;
            }

            return n;
        }

        /// <summary>
        /// 문장 끝(. 또는 ?) 판정.
        /// "6.3" 같은 소수는 어절이 숫자로 끝나므로 이 판정에 걸리지 않는다.
        /// ("3.14." 처럼 소수 뒤에 마침표가 붙으면 문장 끝이 맞다)
        /// </summary>
        public static bool EndsSentence(string word)
        {
            var w = (word ?? string.Empty).Trim();
            if (w.Length == 0)
            {
                return false;
            }

            var last = w[w.Length - 1];
            return last == '.' || last == '?';
        }

        // ============================================================
        // 메인 진입점: words -> 자막 리스트
        // ============================================================
        public static List<HelloVisionSubtitle> Convert(
            List<HelloVisionWord> words, int maxCharsPerLine, int maxLines)
        {
            var result = new List<HelloVisionSubtitle>();
            if (words == null || words.Count == 0)
            {
                return result;
            }

            maxCharsPerLine = Math.Max(1, maxCharsPerLine);
            maxLines = Math.Max(1, maxLines);
            var maxLineBytes = 2 * maxCharsPerLine;          // 한 줄 최대 바이트 (한글 글자수 * 2)
            var maxSubtitleBytes = maxLines * maxLineBytes;  // 한 자막 최대 바이트

            // 텍스트가 비어 있는 word 는 제외하고, 트림된 텍스트를 미리 계산
            var items = words
                .Where(w => w != null && !string.IsNullOrWhiteSpace(w.Text))
                .Select(w => new WordItem { Word = w, Text = w.Text.Trim() })
                .ToList();

            // ---- 1단계: 자막 덩어리(chunk) 나누기 ----
            var chunks = new List<List<WordItem>>();
            var cur = new List<WordItem>();
            var curBytes = 0; // cur 를 공백으로 이어 붙인 텍스트의 바이트 수

            foreach (var item in items)
            {
                var wordBytes = ByteLen(item.Text);

                // (a) 화자 변경 -> 끊기
                if (cur.Count > 0 && cur[cur.Count - 1].Word.Speaker != item.Word.Speaker)
                {
                    chunks.Add(cur);
                    cur = new List<WordItem>();
                    curBytes = 0;
                }

                // (c) 이 어절을 더하면 한 자막 최대 바이트 초과 -> 직전 어절까지 확정
                if (cur.Count > 0 && curBytes + 1 + wordBytes > maxSubtitleBytes)
                {
                    chunks.Add(cur);
                    cur = new List<WordItem>();
                    curBytes = 0;
                }

                curBytes += cur.Count > 0 ? 1 + wordBytes : wordBytes;
                cur.Add(item);

                // (b) 문장 끝 -> 끊기
                if (EndsSentence(item.Text))
                {
                    chunks.Add(cur);
                    cur = new List<WordItem>();
                    curBytes = 0;
                }
            }

            if (cur.Count > 0)
            {
                chunks.Add(cur);
            }

            // ---- 2단계: 각 덩어리를 자막(들)으로 나누고, word 실제 시각으로 타임코드 ----
            foreach (var chunk in chunks)
            {
                var parts = new List<Part>();
                Split(chunk.Select(p => p.Text).ToList(), 0, maxLineBytes, maxLines, parts);
                foreach (var part in parts)
                {
                    result.Add(new HelloVisionSubtitle
                    {
                        Lines = part.Lines,
                        StartSeconds = chunk[part.WordStart].Word.StartSeconds,
                        EndSeconds = chunk[part.WordEnd].Word.EndSeconds,
                    });
                }
            }

            return result;
        }

        private class WordItem
        {
            public HelloVisionWord Word;
            public string Text;
        }

        // 내부 결과 구조 (WordStart/WordEnd 는 덩어리 내 어절 인덱스)
        private class Part
        {
            public List<string> Lines;
            public int WordStart;
            public int WordEnd;
        }

        // ============================================================
        // 2단계: 어절 목록 -> 자막(들). 재귀 균등 분배.
        // ============================================================
        private static void Split(List<string> segWords, int baseIdx, int maxLineBytes, int maxLines, List<Part> results)
        {
            if (segWords.Count == 0)
            {
                return;
            }

            var joined = string.Join(" ", segWords);
            var totalBytes = ByteLen(joined);

            // (가) 전체가 한 줄 최대 바이트 안에 들어가면 1줄 자막
            if (totalBytes <= maxLineBytes)
            {
                results.Add(new Part { Lines = new List<string> { joined }, WordStart = baseIdx, WordEnd = baseIdx + segWords.Count - 1 });
                return;
            }

            // (나) 목표 = 전체 바이트 / 최대 줄 수, 앞에서부터 최대 줄 수까지 균등 분배
            //      (각 줄은 한 줄 최대 바이트 이하)
            var targetBytes = (double)totalBytes / maxLines;
            var lines = new List<string>();
            var i = 0;

            while (i < segWords.Count && lines.Count < maxLines)
            {
                var fill = FillLine(segWords, i, targetBytes, maxLineBytes);
                lines.Add(fill.Line);
                i += fill.WordCount;
                if (fill.HitLineCap)
                {
                    break;
                }
            }

            if (i >= segWords.Count)
            {
                // 어절이 전부 담김 -> 자막 확정
                results.Add(new Part { Lines = lines, WordStart = baseIdx, WordEnd = baseIdx + segWords.Count - 1 });
                return;
            }

            // (다) 한 줄 최대 바이트 제약에 걸렸거나, 최대 줄 수가 다 차고도 어절이 남음
            //      -> 지금까지의 줄로 자막을 확정하고 남은 어절을 재귀 처리
            results.Add(new Part { Lines = lines, WordStart = baseIdx, WordEnd = baseIdx + i - 1 });
            Split(segWords.Skip(i).ToList(), baseIdx + i, maxLineBytes, maxLines, results);
        }

        private class FillResult
        {
            public string Line;
            public int WordCount;
            public bool HitLineCap;
        }

        // 한 줄 채우기: 목표(targetBytes)까지 채우되 한 줄 최대(maxLineBytes)는 넘지 않음
        private static FillResult FillLine(List<string> words, int start, double targetBytes, int maxLineBytes)
        {
            var line = string.Empty;
            var lineBytes = 0;
            var i = start;
            var hitLineCap = false;

            while (i < words.Count)
            {
                var wordBytes = ByteLen(words[i]);
                var candBytes = lineBytes == 0 ? wordBytes : lineBytes + 1 + wordBytes;

                if (candBytes > maxLineBytes)
                {
                    if (lineBytes == 0)
                    {
                        // 단일 어절이 한 줄 최대 바이트를 넘는 극단 케이스:
                        // 어절 중간은 자를 수 없으므로 그 어절만으로 한 줄을 만든다
                        line = words[i];
                        i++;
                    }

                    hitLineCap = true;
                    break;
                }

                // 이미 담은 게 있고, 이 어절을 더하면 목표 초과 -> 다음 줄로
                if (lineBytes != 0 && candBytes > targetBytes)
                {
                    break;
                }

                line = lineBytes == 0 ? words[i] : line + " " + words[i];
                lineBytes = candBytes;
                i++;

                if (lineBytes >= targetBytes)
                {
                    break; // 목표 도달
                }
            }

            return new FillResult { Line = line, WordCount = i - start, HitLineCap = hitLineCap };
        }
    }
}
