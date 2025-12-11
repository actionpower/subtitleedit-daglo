using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public static class DagloTimingFixer
    {
        private static int SecondsToSampleIndex(double seconds, int sampleRate)
        {
            return (int)Math.Round(seconds * sampleRate, MidpointRounding.AwayFromZero);
        }

        private static double FindPercentage(double startSeconds, double endSeconds, WavePeakData wavePeaks)
        {
            var min = Math.Max(0, SecondsToSampleIndex(startSeconds, wavePeaks.SampleRate));
            var max = Math.Min(wavePeaks.Peaks.Count, SecondsToSampleIndex(endSeconds, wavePeaks.SampleRate));

            var maxPeak = int.MinValue;
            var count = 0;
            var total = 0;
            for (var i = min; i < max; i++)
            {
                var v = wavePeaks.Peaks[i].Abs;
                count++;
                total += v;
                if (v > maxPeak)
                {
                    maxPeak = v;
                }
            }

            if (count == 0)
            {
                return -1;
            }

            var pctAvg = (total / (double)count) * 100.0 / wavePeaks.HighestPeak;
            var pctMax = maxPeak * 100.0 / wavePeaks.HighestPeak;
            return (pctAvg + pctMax + pctMax) / 3.0;
        }

        public static Subtitle ShortenViaWavePeaks(Subtitle subtitle, WavePeakData wavePeaks)
        {
            var minDurationMs = 600;

            var s = new Subtitle(subtitle);
            const double percentageMax = 7.0;

            for (var index = 0; index < s.Paragraphs.Count; index++)
            {
                var p = s.Paragraphs[index];
                var oldP = new Paragraph(p);
                var prevEndSecs = -1.0;
                if (index > 0)
                {
                    prevEndSecs = s.Paragraphs[index].EndTime.TotalSeconds;
                }

                // Find nearest silence
                var startPos = p.StartTime.TotalSeconds;
                var pctHere = FindPercentage(startPos - 0.05, startPos + 0.05, wavePeaks);
                if (Math.Abs(pctHere - (-1)) < 0.01)
                {
                    if (p.DurationTotalMilliseconds < minDurationMs)
                    {
                        s.Paragraphs[index] = oldP;
                    }

                    return s;
                }

                if (pctHere > percentageMax)
                {
                    var startPosBack = startPos;
                    var startPosForward = startPos;
                    for (var ms = 50; ms < 255; ms += 50)
                    {
                        var pct = FindPercentage(startPosBack - 0.05, startPosBack + 0.05, wavePeaks);
                        if (Math.Abs(pct - (-1)) < 0.01)
                        {
                            if (p.DurationTotalMilliseconds < minDurationMs)
                            {
                                s.Paragraphs[index] = oldP;
                            }

                            return s;
                        }

                        if (pct < percentageMax + 1 && p.DurationTotalSeconds < 5)
                        {
                            startPosBack -= 0.025;
                            var pct2 = FindPercentage(startPosBack - 0.05, startPosBack + 0.05, wavePeaks);
                            if (pct2 < pct && pct2 >= 0)
                            {
                                var x = startPosBack;
                                if (x < 0)
                                {
                                    x = 0;
                                }

                                if (x > prevEndSecs)
                                {
                                    p.StartTime.TotalMilliseconds = x;
                                }
                            }
                            else
                            {
                                var x = startPosBack + 0.025;
                                if (x < 0)
                                {
                                    x = 0;
                                }

                                if (x > prevEndSecs)
                                {
                                    p.StartTime.TotalMilliseconds = x;
                                }
                            }

                            break;
                        }

                        startPosBack -= 0.05;



                        var pctF = FindPercentage(startPosForward - 0.05, startPosForward + 0.05, wavePeaks);
                        if (Math.Abs(pctF - (-1)) < 0.01)
                        {
                            if (p.DurationTotalMilliseconds < minDurationMs)
                            {
                                s.Paragraphs[index] = oldP;
                            }

                            return s;
                        }

                        if (pctF < percentageMax)
                        {
                            startPosForward -= 0.025;
                            var pct2 = FindPercentage(startPosForward - 0.05, startPosForward + 0.05, wavePeaks);
                            if (pct2 < pctF && pct2 >= 0)
                            {
                                p.StartTime.TotalSeconds = startPosForward;
                            }
                            else
                            {
                                p.StartTime.TotalSeconds = startPosForward + 0.025;
                            }

                            break;
                        }

                        startPosForward += 0.05;
                    }
                }

                // find next non-silence
                startPos = p.StartTime.TotalSeconds;
                pctHere = FindPercentage(startPos - 0.05, startPos + 0.05, wavePeaks);
                if (Math.Abs(pctHere - (-1)) < 0.01)
                {
                    if (p.DurationTotalMilliseconds < minDurationMs)
                    {
                        s.Paragraphs[index] = oldP;
                    }

                    return s;
                }

                if (pctHere < percentageMax)
                {
                    var startPosForward = p.StartTime.TotalSeconds;
                    while (pctHere < percentageMax && startPos < p.EndTime.TotalSeconds - 1)
                    {
                        pctHere = FindPercentage(startPosForward - 0.05, startPosForward + 0.05, wavePeaks);
                        if (Math.Abs(pctHere - (-1)) < 0.01)
                        {
                            if (p.DurationTotalMilliseconds < 1000)
                            {
                                s.Paragraphs[index] = oldP;
                            }

                            return s;
                        }

                        p.StartTime.TotalSeconds = startPosForward;
                        if (pctHere >= percentageMax)
                        {
                            startPosForward -= 0.025;
                            var pct2 = FindPercentage(startPosForward - 0.05, startPosForward + 0.05, wavePeaks);
                            if (pct2 < pctHere && pct2 >= 0)
                            {
                                p.StartTime.TotalSeconds -= 0.025;

                                pctHere = pct2;
                                startPosForward -= 0.025;
                                pct2 = FindPercentage(startPosForward - 0.05, startPosForward + 0.05, wavePeaks);
                                if (pct2 < pctHere && pct2 >= 0)
                                {
                                    p.StartTime.TotalSeconds -= 0.025;
                                }
                            }

                            break;
                        }

                        startPosForward += 0.05;
                    }
                }

                if (p.DurationTotalMilliseconds < minDurationMs)
                {
                    s.Paragraphs[index] = oldP;
                }
            }

            return s;
        }


        /// <summary>
        /// 종료 시점을 주변 무음까지 앞당기거나, 충분히 조용하면 뒤로 늘립니다.
        /// startPos 보정과 동일한 방식으로 endPos를 조정하며, 다음 자막과 최대 표시 시간 한도를 지킵니다.
        /// </summary>
        public static Subtitle AdjustEndViaWavePeaks(Subtitle subtitle, WavePeakData wavePeaks)
        {
            var minDurationMs = 600;
            const double percentageMax = 7.0;
            var s = new Subtitle(subtitle);

            for (var index = 0; index < s.Paragraphs.Count; index++)
            {
                var p = s.Paragraphs[index];
                var oldP = new Paragraph(p);
                var nextStartSecs = index < s.Paragraphs.Count - 1
                    ? s.Paragraphs[index + 1].StartTime.TotalSeconds
                    : double.MaxValue;
                var extendedForward = false;

                // 현재 종료 지점의 에너지 확인
                var endPos = p.EndTime.TotalSeconds;
                var pctHere = FindPercentage(endPos - 0.05, endPos + 0.05, wavePeaks);
                if (Math.Abs(pctHere - (-1)) < 0.01)
                {
                    if (p.DurationTotalMilliseconds < minDurationMs)
                    {
                        s.Paragraphs[index] = oldP;
                    }

                    return s;
                }

                // 음성이 강하면 가까운 무음으로 앞당김 (shorten)
                if (pctHere > percentageMax)
                {
                    var endPosBack = endPos;
                    for (var ms = 50; ms < 255; ms += 50)
                    {
                        var pct = FindPercentage(endPosBack - 0.05, endPosBack + 0.05, wavePeaks);
                        if (Math.Abs(pct - (-1)) < 0.01)
                        {
                            if (p.DurationTotalMilliseconds < minDurationMs)
                            {
                                s.Paragraphs[index] = oldP;
                            }

                            return s;
                        }

                        if (pct < percentageMax + 1 && p.DurationTotalSeconds < 5)
                        {
                            endPosBack -= 0.025;
                            var pct2 = FindPercentage(endPosBack - 0.05, endPosBack + 0.05, wavePeaks);
                            if (pct2 < pct && pct2 >= 0)
                            {
                                var x = endPosBack;
                                var minEnd = p.StartTime.TotalSeconds + (minDurationMs / 1000.0);
                                if (x < minEnd)
                                {
                                    x = minEnd;
                                }

                                if (x < nextStartSecs)
                                {
                                    p.EndTime.TotalSeconds = x;
                                }
                            }
                            else
                            {
                                var x = endPosBack + 0.025;
                                var minEnd = p.StartTime.TotalSeconds + (minDurationMs / 1000.0);
                                if (x < minEnd)
                                {
                                    x = minEnd;
                                }

                                if (x < nextStartSecs)
                                {
                                    p.EndTime.TotalSeconds = x;
                                }
                            }

                            break;
                        }

                        endPosBack -= 0.05;
                    }
                }

                // 무음이 계속된다면 종료 지점을 뒤로 늘려줌 (extend)
                var maxAllowedEnd = Math.Min(
                    nextStartSecs - 0.05,
                    p.StartTime.TotalSeconds + (Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds / 1000.0));

                endPos = p.EndTime.TotalSeconds;
                pctHere = FindPercentage(endPos - 0.05, endPos + 0.05, wavePeaks);
                if (Math.Abs(pctHere - (-1)) < 0.01)
                {
                    if (p.DurationTotalMilliseconds < minDurationMs)
                    {
                        s.Paragraphs[index] = oldP;
                    }

                    return s;
                }

                if (pctHere < percentageMax && endPos < maxAllowedEnd)
                {
                    var endPosForward = endPos;
                    double lastQuietPos = endPos;
                    while (endPosForward + 0.05 < maxAllowedEnd)
                    {
                        endPosForward += 0.05;
                        pctHere = FindPercentage(endPosForward - 0.05, endPosForward + 0.05, wavePeaks);
                        if (Math.Abs(pctHere - (-1)) < 0.01)
                        {
                            break;
                        }

                        if (pctHere < percentageMax)
                        {
                            lastQuietPos = endPosForward;
                            continue;
                        }

                        // 음성이 다시 감지되면 직전 무음 지점까지 확장
                        break;
                    }

                    var newEnd = Math.Min(lastQuietPos, maxAllowedEnd);
                    if (newEnd > p.EndTime.TotalSeconds)
                    {
                        p.EndTime.TotalSeconds = newEnd;
                        extendedForward = true;
                    }
                }

                // 현재 종료 지점이 여전히 음성 구간이라면, 다음 무음까지 확장
                endPos = p.EndTime.TotalSeconds;
                pctHere = FindPercentage(endPos - 0.05, endPos + 0.05, wavePeaks);
                if (Math.Abs(pctHere - (-1)) < 0.01)
                {
                    if (p.DurationTotalMilliseconds < minDurationMs)
                    {
                        s.Paragraphs[index] = oldP;
                    }

                    return s;
                }

                if (pctHere >= percentageMax && endPos < maxAllowedEnd)
                {
                    var probePos = endPos;
                    double lastVoicePos = endPos;
                    double? firstQuietPos = null;
                    var quietStreak = 0;

                    while (probePos + 0.05 < maxAllowedEnd)
                    {
                        probePos += 0.05;
                        var pct = FindPercentage(probePos - 0.05, probePos + 0.05, wavePeaks);
                        if (Math.Abs(pct - (-1)) < 0.01)
                        {
                            break;
                        }

                        if (pct >= percentageMax)
                        {
                            lastVoicePos = probePos;
                            quietStreak = 0;
                            firstQuietPos = null;
                            continue;
                        }

                        // 무음이 연속으로 감지되면 종료 지점을 여기로 설정
                        quietStreak++;
                        if (firstQuietPos == null)
                        {
                            firstQuietPos = probePos;
                        }
                        if (quietStreak >= 2)
                        {
                            break;
                        }
                    }

                    var candidateEnd = firstQuietPos ?? Math.Min(lastVoicePos, maxAllowedEnd);
                    candidateEnd = Math.Min(candidateEnd, maxAllowedEnd);
                    if (candidateEnd > p.EndTime.TotalSeconds)
                    {
                        p.EndTime.TotalSeconds = candidateEnd;
                        extendedForward = true;
                    }
                }

                // 앞당기기: 무음이 충분히 있는 곳까지 이동 (기존 shorten 흐름 유지)
                endPos = p.EndTime.TotalSeconds;
                pctHere = FindPercentage(endPos - 0.05, endPos + 0.05, wavePeaks);
                if (Math.Abs(pctHere - (-1)) < 0.01)
                {
                    if (p.DurationTotalMilliseconds < minDurationMs)
                    {
                        s.Paragraphs[index] = oldP;
                    }

                    return s;
                }

                if (!extendedForward && pctHere < percentageMax)
                {
                    var endPosBack = p.EndTime.TotalSeconds;
                    while (pctHere < percentageMax && endPosBack > p.StartTime.TotalSeconds + 0.5)
                    {
                        pctHere = FindPercentage(endPosBack - 0.05, endPosBack + 0.05, wavePeaks);
                        if (Math.Abs(pctHere - (-1)) < 0.01)
                        {
                            if (p.DurationTotalMilliseconds < minDurationMs)
                            {
                                s.Paragraphs[index] = oldP;
                            }

                            return s;
                        }

                        p.EndTime.TotalSeconds = endPosBack;
                        if (pctHere >= percentageMax)
                        {
                            endPosBack += 0.025;
                            var pct2 = FindPercentage(endPosBack - 0.05, endPosBack + 0.05, wavePeaks);
                            if (pct2 < pctHere && pct2 >= 0)
                            {
                                p.EndTime.TotalSeconds += 0.025;

                                pctHere = pct2;
                                endPosBack += 0.025;
                                pct2 = FindPercentage(endPosBack - 0.05, endPosBack + 0.05, wavePeaks);
                                if (pct2 < pctHere && pct2 >= 0)
                                {
                                    p.EndTime.TotalSeconds += 0.025;
                                }
                            }

                            break;
                        }

                        // 종료 지점이 다음 자막 시작을 침범하지 않도록 제한
                        if (p.EndTime.TotalSeconds >= nextStartSecs)
                        {
                            p.EndTime.TotalSeconds = nextStartSecs - 0.01;
                            break;
                        }

                        endPosBack -= 0.05;
                    }
                }

                if (p.DurationTotalMilliseconds < minDurationMs)
                {
                    s.Paragraphs[index] = oldP;
                }
            }

            return s;
        }


        public static Subtitle ShortenLongDuration(Subtitle subtitle)
        {
            var s = new Subtitle(subtitle);

            foreach (var p in s.Paragraphs)
            {
                if (p.DurationTotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    p.StartTime.TotalMilliseconds = p.EndTime.TotalMilliseconds - Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
                }
            }

            return s;
        }
    }
}
