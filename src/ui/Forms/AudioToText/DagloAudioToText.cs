using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms.Options;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    public sealed partial class DagloAudioToText : Form
    {
        private readonly string _videoFileName;
        private Subtitle _subtitle;
        private readonly int _audioTrackNumber;
        private bool _cancel;
        private bool _batchMode;
        private int _batchFileNumber;
        private readonly List<string> _filesToDelete;
        private readonly Form _parentForm;
        private bool _useCenterChannelOnly;
        private int _initialWidth = 725;
        private readonly Regex _timeRegexShort = new Regex(@"^\[\d\d:\d\d[\.,]\d\d\d --> \d\d:\d\d[\.,]\d\d\d\]", RegexOptions.Compiled);
        private readonly Regex _timeRegexLong = new Regex(@"^\[\d\d:\d\d:\d\d[\.,]\d\d\d --> \d\d:\d\d:\d\d[\.,]\d\d\d]", RegexOptions.Compiled);
        private readonly Regex _pctWhisper = new Regex(@"^\d+%\|", RegexOptions.Compiled);
        private readonly Regex _pctWhisperFaster = new Regex(@"^\s*\d+%\s*\|", RegexOptions.Compiled);
        private List<ResultText> _resultList;
        private string _languageCode;
        private ConcurrentBag<string> _outputText = new ConcurrentBag<string>();
        private long _startTicks;
        private double _endSeconds;
        private double _showProgressPct = -1;
        private double _lastEstimatedMs = double.MaxValue;
        private VideoInfo _videoInfo;
        private readonly WavePeakData _wavePeaks;
        private readonly List<string> _outputBatchFileNames = new List<string>();
        private IAutoTranscriber _autoTranscriber;

        public bool UnknownArgument { get; set; }
        public bool RunningOnCuda { get; set; }
        public bool IncompleteModel { get; set; }
        public string IncompleteModelName { get; set; }

        private static bool? CudaSomeDevice { get; set; }

        public Subtitle TranscribedSubtitle { get; private set; }

        public DagloAudioToText(string videoFileName, Subtitle subtitle, int audioTrackNumber, Form parentForm, WavePeakData wavePeaks)
        {
            //UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonGenerate);

            _videoFileName = videoFileName;
            _subtitle = subtitle;
            _audioTrackNumber = audioTrackNumber;
            _parentForm = parentForm;
            _wavePeaks = wavePeaks;

            Text = LanguageSettings.Current.AudioToText.Title;
            labelInfo.Text = LanguageSettings.Current.AudioToText.DagloInfo;
            groupBoxModels.Text = LanguageSettings.Current.AudioToText.LanguagesAndModels;
            //labelModel.Text = LanguageSettings.Current.AudioToText.ChooseModel;
            labelChooseLanguage.Text = LanguageSettings.Current.AudioToText.ChooseLanguage;
            //linkLabelOpenModelsFolder.Text = LanguageSettings.Current.AudioToText.OpenModelsFolder;
            //checkBoxTranslateToEnglish.Text = LanguageSettings.Current.AudioToText.TranslateToEnglish;
            //checkBoxUsePostProcessing.Text = LanguageSettings.Current.AudioToText.UsePostProcessing;
            //linkLabelPostProcessingConfigure.Left = checkBoxUsePostProcessing.Right + 1;
            //linkLabelPostProcessingConfigure.Text = LanguageSettings.Current.Settings.Title;
            checkBoxAutoAdjustTimings.Text = LanguageSettings.Current.AudioToText.AutoAdjustTimings;
            buttonGenerate.Text = LanguageSettings.Current.Watermark.Generate;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonBatchMode.Text = LanguageSettings.Current.AudioToText.BatchMode;
            groupBoxInputFiles.Text = LanguageSettings.Current.BatchConvert.Input;
            linkLabelDagloWebSite.Text = LanguageSettings.Current.AudioToText.DagloWebsite;
            buttonAddFile.Text = LanguageSettings.Current.DvdSubRip.Add;
            buttonRemoveFile.Text = LanguageSettings.Current.DvdSubRip.Remove;
            buttonClear.Text = LanguageSettings.Current.DvdSubRip.Clear;
            runOnlyPostProcessingToolStripMenuItem.Text = LanguageSettings.Current.AudioToText.OnlyRunPostProcessing;
            removeTemporaryFilesToolStripMenuItem.Text = LanguageSettings.Current.AudioToText.RemoveTemporaryFiles;
            //buttonAdvanced.Text = LanguageSettings.Current.General.Advanced;
            //SetAdvancedLabel();

            columnHeaderFileName.Text = LanguageSettings.Current.JoinSubtitles.FileName;

            nikseTextBoxApiKey.Text = Configuration.Settings.Tools.DagloApiKey;
            //checkBoxUsePostProcessing.Checked = Configuration.Settings.Tools.VoskPostProcessing;
            //checkBoxAutoAdjustTimings.Checked = Configuration.Settings.Tools.WhisperAutoAdjustTimings;

            _filesToDelete = new List<string>();

            if (string.IsNullOrEmpty(videoFileName))
            {
                // 배치모드는 허용하지 않음
                _batchMode = false;
                buttonBatchMode.Enabled = false;
            }
            else
            {
                listViewInputFiles.Items.Add(videoFileName);
            }

            if (_subtitle == null || _subtitle.Paragraphs.Count == 0)
            {
                runOnlyPostProcessingToolStripMenuItem.Visible = false;
                toolStripSeparatorRunOnlyPostprocessing.Visible = false;
            }
            else
            {
                runOnlyPostProcessingToolStripMenuItem.Visible = true;
                toolStripSeparatorRunOnlyPostprocessing.Visible = true;
            }

            textBoxLog.Visible = false;
            textBoxLog.Dock = DockStyle.Fill;
            labelProgress.Text = string.Empty;
            labelTime.Text = string.Empty;
            listViewInputFiles.Visible = false;
            labelElapsed.Text = string.Empty;
            //labelEngine.Text = LanguageSettings.Current.AudioToText.Engine;
            //labelEngine.Left = comboBoxDagloEngine.Left - labelEngine.Width - 5;

            Init();
        }

        private void Init()
        {
            InitializeLanguageNames(comboBoxLanguages);

            labelFC.Text = string.Empty;

            removeTemporaryFilesToolStripMenuItem.Checked = Configuration.Settings.Tools.WhisperDeleteTempFiles;
            //ContextMenuStrip = contextMenuStripDagloAdvanced;
        }

        private void ButtonGenerate_Click(object sender, EventArgs e)
        {
            var apiKey = nikseTextBoxApiKey.Text?.Trim();
            Configuration.Settings.Tools.DagloApiKey = apiKey;

            if (string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text))
            {
                MessageBox.Show(this, string.Format(LanguageSettings.Current.GoogleTranslate.XRequiresAnApiKey, "Daglo"), Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            /*
            if (string.IsNullOrWhiteSpace(nikseComboBoxUrl.Text))
            {
                MessageBox.Show(this, string.Format("{0} requires an url", "Daglo"), Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            */

            SaveSettings();



            _cancel = false;

            _languageCode = GetLanguage(comboBoxLanguages.Text);

            //_useCenterChannelOnly = Configuration.Settings.General.FFmpegUseCenterChannelOnly &&
            //                        FfmpegMediaInfo.Parse(_videoFileName).HasFrontCenterAudio(_audioTrackNumber);

            IncompleteModel = false;
            ShowProgressBar();

            if (_batchMode)
            {
                if (listViewInputFiles.Items.Count == 0)
                {
                    buttonAddFile_Click(null, null);
                    return;
                }

                timer1.Start();
                GenerateBatch();
                TaskbarList.SetProgressState(_parentForm.Handle, TaskbarButtonProgressFlags.NoProgress);
                timer1.Stop();
                return;
            }

            SetEnabledState(false);

            var mediaInfo = FfmpegMediaInfo.Parse(_videoFileName);
            if (mediaInfo.Tracks.Count(p => p.TrackType == FfmpegTrackType.Audio) == 0)
            {
                MessageBox.Show("No audio track in file: " + _videoFileName);
                SetEnabledState(true);
                return;
            }

            var waveFileName = GenerateWavFile(_videoFileName, _audioTrackNumber);
            if (_cancel)
            {
                SetEnabledState(true);
                return;
            }

            progressBar1.Style = ProgressBarStyle.Blocks;
            timer1.Start();
            var transcript = TranscribeViaDaglo(waveFileName, _videoFileName);

            timer1.Stop();
            if (_cancel && (transcript == null || transcript.Paragraphs.Count == 0 || MessageBox.Show(LanguageSettings.Current.AudioToText.KeepPartialTranscription, Text, MessageBoxButtons.YesNoCancel) != DialogResult.Yes))
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            timer1.Start();
            if (_showProgressPct > 0 && progressBar1.Style == ProgressBarStyle.Blocks)
            {
                _showProgressPct = 100;
                progressBar1.Value = progressBar1.Maximum;
            }

            if (checkBoxAutoAdjustTimings.Checked)
            {
                labelProgress.Text = LanguageSettings.Current.AudioToText.PostProcessing;
            }

            labelTime.Text = string.Empty;
            labelProgress.Refresh();
            Application.DoEvents();


            var postProcessor = new AudioToTextPostProcessor("en")
            {
                ParagraphMaxChars = Configuration.Settings.General.SubtitleLineMaximumLength * 2,
            };


            WavePeakData wavePeaks = null;
            if (checkBoxAutoAdjustTimings.Checked)
            {
                wavePeaks = _wavePeaks ?? MakeWavePeaks();
            }

            if (checkBoxAutoAdjustTimings.Checked && wavePeaks != null)
            {
                transcript = DagloTimingFixer.ShortenLongDuration(transcript);
                // Before timing fix
                PrintSubtitleInfo("Before ShortenViaWavePeaks", transcript);

                transcript = DagloTimingFixer.ShortenViaWavePeaks(transcript, wavePeaks);
                transcript = DagloTimingFixer.AdjustEndViaWavePeaks(transcript, wavePeaks);

                // After timing fix
                PrintSubtitleInfo("After ShortenViaWavePeaks", transcript);
            }


            TranscribedSubtitle = postProcessor.Fix(
                AudioToTextPostProcessor.Engine.Whisper,
                transcript,
                false,
                true, //Configuration.Settings.Tools.WhisperPostProcessingAddPeriods,
                true, //Configuration.Settings.Tools.WhisperPostProcessingMergeLines,
                true, //Configuration.Settings.Tools.WhisperPostProcessingFixCasing,
                true, //Configuration.Settings.Tools.WhisperPostProcessingFixShortDuration,
                true //Configuration.Settings.Tools.WhisperPostProcessingSplitLines
                );

            UpdateLog();
            SeLogger.DagloInfo(textBoxLog.Text);
            if (transcript == null || transcript.Paragraphs.Count == 0)
            {
                IncompleteModelName = "daglo";
            }

            timer1.Stop();

            DialogResult = DialogResult.OK;
        }

        private void SetEnabledState(bool enabled)
        {
            buttonGenerate.Enabled = enabled;
            //buttonDownload.Enabled = enabled;
            //buttonBatchMode.Enabled = enabled;
            //buttonAdvanced.Enabled = enabled;
            //comboBoxLanguages.Enabled = enabled; 
            //linkLabelPostProcessingConfigure.Enabled = enabled;

            progressBar1.Visible = !enabled;
        }

        private void ShowProgressBar()
        {
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            progressBar1.Refresh();
            progressBar1.Top = labelProgress.Bottom + 3;
            labelElapsed.Top = progressBar1.Top - labelElapsed.Height - 3;
            if (!textBoxLog.Visible)
            {
                progressBar1.BringToFront();
            }
        }

        private void EnableGroupBoxInputFiles(bool enabled)
        {

            buttonAddFile.Enabled = enabled;
            buttonRemoveFile.Enabled = enabled;
            buttonClear.Enabled = enabled;
        }

        private void GenerateBatch()
        {
            EnableGroupBoxInputFiles(false);
            _batchFileNumber = 0;
            var errors = new StringBuilder();
            var errorCount = 0;
            _outputText.Add("Batch mode");
            foreach (ListViewItem lvi in listViewInputFiles.Items)
            {
                _batchFileNumber++;
                var videoFileName = lvi.Text;
                listViewInputFiles.SelectedIndices.Clear();
                lvi.Selected = true;
                lvi.EnsureVisible();
                buttonGenerate.Enabled = false;
                //buttonDownload.Enabled = false;
                buttonBatchMode.Enabled = false;
                //buttonAdvanced.Enabled = false;
                //comboBoxModels.Enabled = false;
                comboBoxLanguages.Enabled = false;

                var mediaInfo = FfmpegMediaInfo.Parse(videoFileName);
                if (mediaInfo.Tracks.Count(p => p.TrackType == FfmpegTrackType.Audio) == 0)
                {
                    errors.AppendLine("No audio track in: " + videoFileName);
                    errorCount++;
                    continue;
                }

                var waveFileName = GenerateWavFile(videoFileName, _audioTrackNumber);
                if (!File.Exists(waveFileName))
                {
                    errors.AppendLine("Unable to extract audio from: " + videoFileName);
                    errorCount++;
                    continue;
                }

                _outputText.Add(string.Empty);
                progressBar1.Style = ProgressBarStyle.Blocks;
                var transcript = TranscribeViaDaglo(waveFileName, videoFileName);

                // 에러로 빈 자막이 반환된 경우 다음 파일로 계속
                if (transcript == null || transcript.Paragraphs.Count == 0)
                {
                    if (!_cancel)
                    {
                        errors.AppendLine($"Failed to transcribe: {videoFileName}");
                        errorCount++;
                        continue;
                    }
                }

                if (_cancel)
                {
                    TaskbarList.SetProgressState(_parentForm.Handle, TaskbarButtonProgressFlags.NoProgress);
                    if (!_batchMode)
                    {
                        DialogResult = DialogResult.Cancel;
                    }

                    EnableGroupBoxInputFiles(true);
                    return;
                }

                WavePeakData wavePeaks = null;
                if (checkBoxAutoAdjustTimings.Checked)
                {
                    wavePeaks = _wavePeaks ?? MakeWavePeaks();
                }

                if (checkBoxAutoAdjustTimings.Checked && wavePeaks != null)
                {
                    transcript = DagloTimingFixer.ShortenLongDuration(transcript);
                    transcript = DagloTimingFixer.ShortenViaWavePeaks(transcript, wavePeaks);
                }

                var postProcessor = new AudioToTextPostProcessor(_languageCode)
                {
                    ParagraphMaxChars = Configuration.Settings.General.SubtitleLineMaximumLength * 2,
                };
                TranscribedSubtitle = postProcessor.Fix(
                    AudioToTextPostProcessor.Engine.Whisper,
                    transcript,
                    false,
                    Configuration.Settings.Tools.WhisperPostProcessingAddPeriods,
                    Configuration.Settings.Tools.WhisperPostProcessingMergeLines,
                    Configuration.Settings.Tools.WhisperPostProcessingFixCasing,
                    Configuration.Settings.Tools.WhisperPostProcessingFixShortDuration,
                    Configuration.Settings.Tools.WhisperPostProcessingSplitLines);


                SaveToSourceFolder(videoFileName);
                TaskbarList.SetProgressValue(_parentForm.Handle, _batchFileNumber, listViewInputFiles.Items.Count);
            }

            progressBar1.Visible = false;
            labelTime.Text = string.Empty;

            TaskbarList.StartBlink(_parentForm, 10, 1, 2);

            Activate();
            Focus();
            Application.DoEvents();

            if (errors.Length > 0)
            {
                MessageBox.Show(this, $"{errorCount} error(s)!{Environment.NewLine}{errors}", Text, MessageBoxButtons.OK);
            }

            var fileList = Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, _outputBatchFileNames);
            MessageBox.Show(this, string.Format(LanguageSettings.Current.AudioToText.XFilesSavedToVideoSourceFolder, listViewInputFiles.Items.Count - errorCount) + fileList, Text, MessageBoxButtons.OK);

            EnableGroupBoxInputFiles(true);
            buttonGenerate.Enabled = true;
            //buttonDownload.Enabled = true;
            buttonBatchMode.Enabled = true;
            //buttonAdvanced.Enabled = true;
            DialogResult = DialogResult.Cancel;
        }

        private WavePeakData MakeWavePeaks()
        {
            if (string.IsNullOrEmpty(_videoFileName) || !File.Exists(_videoFileName))
            {
                return null;
            }

            var targetFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
            try
            {
                var process = AddWaveform.GetCommandLineProcess(_videoFileName, -1, targetFile, Configuration.Settings.General.VlcWaveTranscodeSettings, out var encoderName);
                process.Start();
                while (!process.HasExited)
                {
                    Application.DoEvents();
                }

                // check for delay in matroska files
                var delayInMilliseconds = 0;
                var audioTrackNames = new List<string>();
                var mkvAudioTrackNumbers = new Dictionary<int, int>();
                if (_videoFileName.ToLowerInvariant().EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        using (var matroska = new MatroskaFile(_videoFileName))
                        {
                            if (matroska.IsValid)
                            {
                                foreach (var track in matroska.GetTracks())
                                {
                                    if (track.IsAudio)
                                    {
                                        if (track.CodecId != null && track.Language != null)
                                        {
                                            audioTrackNames.Add("#" + track.TrackNumber + ": " + track.CodecId.Replace("\0", string.Empty) + " - " + track.Language.Replace("\0", string.Empty));
                                        }
                                        else
                                        {
                                            audioTrackNames.Add("#" + track.TrackNumber);
                                        }

                                        mkvAudioTrackNumbers.Add(mkvAudioTrackNumbers.Count, track.TrackNumber);
                                    }
                                }

                                if (mkvAudioTrackNumbers.Count > 0)
                                {
                                    delayInMilliseconds = (int)matroska.GetAudioTrackDelayMilliseconds(mkvAudioTrackNumbers[0]);
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        SeLogger.Error(exception, $"Error getting delay from mkv: {_videoFileName}");
                    }
                }

                if (File.Exists(targetFile))
                {
                    using (var waveFile = new WavePeakGenerator(targetFile))
                    {
                        if (!string.IsNullOrEmpty(_videoFileName) && File.Exists(_videoFileName))
                        {
                            return waveFile.GeneratePeaks(delayInMilliseconds, WavePeakGenerator.GetPeakWaveFileName(_videoFileName));
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }

            return null;
        }

        private void SaveToSourceFolder(string videoFileName)
        {
            var format = SubtitleFormat.FromName(Configuration.Settings.General.DefaultSubtitleFormat, new SubRip());
            if (format.GetType() == typeof(AdvancedSubStationAlpha))
            {
                try
                {
                    var info = FfmpegMediaInfo.Parse(videoFileName);
                    if (info.Dimension.Width > 0)
                    {
                        if (string.IsNullOrEmpty(TranscribedSubtitle.Header))
                        {
                            TranscribedSubtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
                        }

                        TranscribedSubtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + info.Dimension.Width.ToString(CultureInfo.InvariantCulture), "[Script Info]", TranscribedSubtitle.Header);
                        TranscribedSubtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + info.Dimension.Height.ToString(CultureInfo.InvariantCulture), "[Script Info]", TranscribedSubtitle.Header);
                    }
                }
                catch
                {
                    // ignore
                }
            }

            var text = TranscribedSubtitle.ToText(format);

            var fileName = Path.Combine(Utilities.GetPathAndFileNameWithoutExtension(videoFileName)) + format.Extension;
            if (File.Exists(fileName))
            {
                fileName = $"{Path.Combine(Utilities.GetPathAndFileNameWithoutExtension(videoFileName))}.{Guid.NewGuid().ToString()}{format.Extension}";
            }

            try
            {
                File.WriteAllText(fileName, text, Encoding.UTF8);
                _outputText.Add("Subtitle written to : " + fileName);
                _outputBatchFileNames.Add(fileName);
            }
            catch
            {
                var dir = Path.GetDirectoryName(fileName);
                if (!FileUtil.IsDirectoryWritable(dir))
                {
                    MessageBox.Show(this, $"SE does not have write access to the folder '{dir}'", MessageBoxIcon.Error);
                }

                throw;
            }
        }



        internal static string GetLanguage(string name)
        {
            var language = DagloLanguage.Languages.FirstOrDefault(l => l.Name == name);
            return language != null ? language.Code : "en";
        }




        public Subtitle TranscribeViaDaglo(string waveFileName, string videoFileName)
        {
            _showProgressPct = -1;
            var model = new DagloModel();
            if (model == null)
            {
                return new Subtitle();
            }

            labelProgress.Text = LanguageSettings.Current.AudioToText.Transcribing;
            if (_batchMode)
            {
                labelProgress.Text = string.Format(LanguageSettings.Current.AudioToText.TranscribingXOfY, _batchFileNumber, listViewInputFiles.Items.Count);
            }
            else
            {
                TaskbarList.SetProgressValue(_parentForm.Handle, 1, 100);
            }


            labelProgress.Refresh();
            Application.DoEvents();
            _resultList = new List<ResultText>();

            var inputFile = waveFileName;
            if (!_useCenterChannelOnly &&
                (videoFileName.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) ||
                 videoFileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)) &&
                _audioTrackNumber <= 0)
            {
                inputFile = videoFileName;
            }



            var task = Task.Run(() => DagloTranscribe.TranscribeFileUploadAsync(inputFile));

            //OutputHandler();

            var sw = Stopwatch.StartNew();
            _outputText.Add($"Calling daglo with : {Environment.NewLine}");
            _startTicks = Stopwatch.GetTimestamp();
            _videoInfo = UiUtil.GetVideoInfo(waveFileName);
            timer1.Start();

            if (!_batchMode)
            {
                ShowProgressBar();
                progressBar1.Style = ProgressBarStyle.Marquee;
            }

            buttonCancel.Visible = true;
            _cancel = false;
            labelProgress.Text = LanguageSettings.Current.AudioToText.Transcribing;

            while (!task.IsCompleted)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
                WindowsHelper.PreventStandBy();

                if (_cancel)
                {
                    //process.Kill();

                    progressBar1.Visible = false;
                    buttonCancel.Visible = false;
                    DialogResult = DialogResult.Cancel;

                    var partialSub = new Subtitle();
                    partialSub.Paragraphs.AddRange(_resultList.OrderBy(p => p.Start).Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());
                    if (partialSub.Paragraphs.Count > 0)
                    {
                        return partialSub;
                    }

                    return null;
                }
            }

            // Task 완료 후 예외 확인
            if (task.IsFaulted)
            {
                progressBar1.Visible = false;
                buttonCancel.Visible = false;

                var exception = task.Exception?.GetBaseException() ?? task.Exception;
                _outputText.Add($"Error during Daglo transcription: {exception?.Message}{Environment.NewLine}");
                SeLogger.Error(exception, "Daglo transcription failed");

                // 사용자에게 오류 메시지 표시
                MessageBox.Show(
                    this,
                    $"{exception?.Message}",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return new Subtitle(); // 빈 자막 반환
            }

            _outputText.Add($"Calling daglo done in {sw.Elapsed}{Environment.NewLine}");

            for (var i = 0; i < 10; i++)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(50);
            }

            if (GetResultFromSrt(waveFileName, videoFileName, out var resultTexts, _outputText, _filesToDelete))
            {
                var subtitle = new Subtitle();
                subtitle.Paragraphs.AddRange(resultTexts.Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());
                return subtitle;
            }

            _outputText?.Add("Loading result from STDOUT" + Environment.NewLine);

            var sub = new Subtitle();
            sub.Paragraphs.AddRange(_resultList.OrderBy(p => p.Start).Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());
            return sub;
        }


        public static bool GetResultFromSrt(string waveFileName, string videoFileName, out List<ResultText> resultTexts, ConcurrentBag<string> outputText, List<string> filesToDelete)
        {
            var srtFileName = waveFileName + ".srt";
            if (!File.Exists(srtFileName) && waveFileName.EndsWith(".wav"))
            {
                srtFileName = waveFileName.Remove(waveFileName.Length - 4) + ".srt";
            }

            var dagloFolder = DagloHelper.GetDagloFolder() ?? string.Empty;
            if (!string.IsNullOrEmpty(dagloFolder) && !File.Exists(srtFileName) && !string.IsNullOrEmpty(videoFileName))
            {
                srtFileName = Path.Combine(dagloFolder, Path.GetFileNameWithoutExtension(videoFileName)) + ".srt";
            }

            if (!File.Exists(srtFileName))
            {
                srtFileName = Path.Combine(dagloFolder, Path.GetFileNameWithoutExtension(waveFileName)) + ".srt";
            }

            if (!File.Exists(srtFileName))
            {
                resultTexts = new List<ResultText>();
                return false;
            }

            var sub = new Subtitle();
            if (File.Exists(srtFileName))
            {
                var rawText = FileUtil.ReadAllLinesShared(srtFileName, Encoding.UTF8);
                new SubRip().LoadSubtitle(sub, rawText, srtFileName);
                outputText?.Add($"Loading result from {srtFileName}{Environment.NewLine}");
            }

            sub.RemoveEmptyLines();

            var results = new List<ResultText>();
            foreach (var p in sub.Paragraphs)
            {
                results.Add(new ResultText
                {
                    Start = (decimal)p.StartTime.TotalSeconds,
                    End = (decimal)p.EndTime.TotalSeconds,
                    Text = p.Text
                });
            }

            resultTexts = results;

            if (File.Exists(srtFileName))
            {
                filesToDelete?.Add(srtFileName);
            }

            return true;
        }


        private string GenerateWavFile(string videoFileName, int audioTrackNumber)
        {
            if (videoFileName.EndsWith(".wav"))
            {
                try
                {
                    using (var waveFile = new WavePeakGenerator(videoFileName))
                    {
                        if (waveFile.Header != null && waveFile.Header.SampleRate == 16000)
                        {
                            return videoFileName;
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            }

            var ffmpegLog = new StringBuilder();
            var outWaveFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
            _filesToDelete.Add(outWaveFile);
            var process = GetFfmpegProcess(videoFileName, audioTrackNumber, outWaveFile);

            process.ErrorDataReceived += (sender, args) =>
            {
                ffmpegLog.AppendLine(args.Data);
            };

            process.StartInfo.RedirectStandardError = true;
            process.Start();
            process.BeginErrorReadLine();

            double seconds = 0;
            buttonCancel.Visible = true;
            try
            {
                process.PriorityClass = ProcessPriorityClass.Normal;
            }
            catch
            {
                // ignored
            }

            _cancel = false;
            string targetDriveLetter = null;
            if (Configuration.IsRunningOnWindows)
            {
                var root = Path.GetPathRoot(outWaveFile);
                if (root.Length > 1 && root[1] == ':')
                {
                    targetDriveLetter = root.Remove(1);
                }
            }

            while (!process.HasExited)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
                seconds += 0.1;
                if (seconds < 60)
                {
                    labelProgress.Text = string.Format(LanguageSettings.Current.AddWaveform.ExtractingSeconds, seconds);
                }
                else
                {
                    labelProgress.Text = string.Format(LanguageSettings.Current.AddWaveform.ExtractingMinutes, (int)(seconds / 60), (int)(seconds % 60));
                }

                Invalidate();
                if (_cancel)
                {
                    process.Kill();
                    progressBar1.Visible = false;
                    buttonCancel.Visible = false;
                    DialogResult = DialogResult.Cancel;
                    return null;
                }

                if (targetDriveLetter != null && seconds > 1 && Convert.ToInt32(seconds) % 10 == 0)
                {
                    try
                    {
                        var drive = new DriveInfo(targetDriveLetter);
                        if (drive.IsReady)
                        {
                            if (drive.AvailableFreeSpace < 50 * 1000000) // 50 mb
                            {
                                labelInfo.ForeColor = Color.Red;
                                labelInfo.Text = LanguageSettings.Current.AddWaveform.LowDiskSpace;
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            Application.DoEvents();
            System.Threading.Thread.Sleep(100);

            if (!File.Exists(outWaveFile))
            {
                SeLogger.WhisperInfo("Generated wave file not found: " + outWaveFile + Environment.NewLine +
                               "ffmpeg: " + process.StartInfo.FileName + Environment.NewLine +
                               "Parameters: " + process.StartInfo.Arguments + Environment.NewLine +
                               "OS: " + Environment.OSVersion + Environment.NewLine +
                               "64-bit: " + Environment.Is64BitOperatingSystem + Environment.NewLine +
                               "ffmpeg exit code: " + process.ExitCode + Environment.NewLine +
                               "ffmpeg log: " + ffmpegLog);
            }

            return outWaveFile;
        }

        private Process GetFfmpegProcess(string videoFileName, int audioTrackNumber, string outWaveFile)
        {
            if (!File.Exists(Configuration.Settings.General.FFmpegLocation) && Configuration.IsRunningOnWindows)
            {
                return null;
            }

            var audioParameter = string.Empty;
            if (audioTrackNumber > 0)
            {
                audioParameter = $"-map 0:a:{audioTrackNumber}";
            }

            labelFC.Text = string.Empty;
            var fFmpegWaveTranscodeSettings = "-i \"{0}\" -vn -ar 16000 -ac 1 -ab 32k -af volume=1.75 -f wav {2} \"{1}\"";
            if (_useCenterChannelOnly)
            {
                fFmpegWaveTranscodeSettings = "-i \"{0}\" -vn -ar 16000 -ab 32k -af volume=1.75 -af \"pan=mono|c0=FC\" -f wav {2} \"{1}\"";
                labelFC.Text = "FC";
            }

            //-i indicates the input
            //-vn means no video output
            //-ar 44100 indicates the sampling frequency.
            //-ab indicates the bit rate (in this example 160kb/s)
            //-af volume=1.75 will boot volume... 1.0 is normal
            //-ac 2 means 2 channels
            // "-map 0:a:0" is the first audio stream, "-map 0:a:1" is the second audio stream

            var exeFilePath = Configuration.Settings.General.FFmpegLocation;
            if (!Configuration.IsRunningOnWindows)
            {
                exeFilePath = "ffmpeg";
            }

            var parameters = string.Format(fFmpegWaveTranscodeSettings, videoFileName, outWaveFile, audioParameter);
            return new Process
            {
                StartInfo = new ProcessStartInfo(exeFilePath, parameters)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                }
            };
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (buttonGenerate.Enabled)
            {
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                _cancel = true;
            }
        }

        private void SaveSettings()
        {
            if (!string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text))
            {
                Configuration.Settings.Tools.DagloApiKey = nikseTextBoxApiKey.Text.Trim();
                Configuration.Settings.Save();
            }
        }

        /*
        private void HandleError(Exception exception, int linesTranslate, Type engineType)
        {
            SeLogger.Error(exception);

            if (nikseTextBoxApiKey.Visible &&
                string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text) &&
                engineType != typeof(MyMemoryApi))
            {
                MessageBox.Show(this, string.Format(LanguageSettings.Current.GoogleTranslate.XRequiresAnApiKey, _autoTranscriber.Name), Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                nikseTextBoxApiKey.Focus();
            }

            var count = 0;
            while (count < 10 && exception.InnerException != null)
            {
                exception = exception.InnerException;
                count++;
            }

            MessageBox.Show(this, exception.Message + Environment.NewLine + exception.StackTrace +
                    Environment.NewLine +
                    _autoTranscriber.Error, MessageBoxIcon.Error);
        }
        */

        private void linkLabelDagloWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenUrl(DagloHelper.GetWebSiteUrl());
        }

        private void AudioToText_FormClosing(object sender, FormClosingEventArgs e)
        {
            TaskbarList.SetProgressState(_parentForm.Handle, TaskbarButtonProgressFlags.NoProgress);

            if (comboBoxLanguages.SelectedItem is DagloLanguage language)
            {
                Configuration.Settings.Tools.DagloLanguageCode = language.Code;
            }

            //Configuration.Settings.Tools.VoskPostProcessing = false;
            Configuration.Settings.Tools.DagloAutoAdjustTimings = checkBoxAutoAdjustTimings.Checked;

            DeleteTemporaryFiles(_filesToDelete);
        }

        public static void DeleteTemporaryFiles(List<string> filesToDelete)
        {
            if (!Configuration.Settings.Tools.DagloDeleteTempFiles)
            {
                return;
            }

            foreach (var fileName in filesToDelete)
            {
                try
                {
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        private void AudioToText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                if (textBoxLog.Visible)
                {
                    textBoxLog.Visible = false;
                }
                else
                {
                    UpdateLog();
                    textBoxLog.Visible = true;
                    textBoxLog.BringToFront();
                }

                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape && buttonGenerate.Enabled)
            {
                DialogResult = DialogResult.Cancel;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#audio_to_text_daglo");
                e.SuppressKeyPress = true;
            }
        }


        private void UpdateLog()
        {
            if (_outputText.IsEmpty)
            {
                return;
            }

            textBoxLog.AppendText(string.Join(Environment.NewLine, _outputText));
            _outputText = new ConcurrentBag<string>();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateLog();

            if (_batchMode)
            {
                var pct = _batchFileNumber * 100.0 / listViewInputFiles.Items.Count;
                SetProgressBarPct(pct);
                return;
            }

            var durationMs = (Stopwatch.GetTimestamp() - _startTicks) / 10_000;

            labelElapsed.Text = new TimeCode(durationMs).ToShortDisplayString();
            if (_endSeconds <= 0 || _videoInfo == null)
            {
                if (_showProgressPct > 0)
                {
                    if (progressBar1.Style != ProgressBarStyle.Blocks)
                    {
                        progressBar1.Style = ProgressBarStyle.Blocks;
                        progressBar1.Maximum = 100;
                    }

                    SetProgressBarPct(_showProgressPct);
                }

                return;
            }

            if (progressBar1.Style != ProgressBarStyle.Blocks)
            {
                progressBar1.Style = ProgressBarStyle.Blocks;
                progressBar1.Maximum = 100;
            }

            _videoInfo.TotalSeconds = Math.Max(_endSeconds, _videoInfo.TotalSeconds);
            var msPerFrame = durationMs / (_endSeconds * 1000.0);
            var estimatedTotalMs = msPerFrame * _videoInfo.TotalMilliseconds;
            var msEstimatedLeft = estimatedTotalMs - durationMs;
            if (msEstimatedLeft > _lastEstimatedMs)
            {
                msEstimatedLeft = _lastEstimatedMs;
            }
            else
            {
                _lastEstimatedMs = msEstimatedLeft;
            }

            if (_showProgressPct > 0)
            {
                SetProgressBarPct(_showProgressPct);
            }
            else
            {
                SetProgressBarPct(_endSeconds * 100.0 / _videoInfo.TotalSeconds);
            }

            labelTime.Text = ProgressHelper.ToProgressTime(msEstimatedLeft);
            labelTime.Refresh();
            BringToFront();
        }

        private void SetProgressBarPct(double pct)
        {
            var p = (int)Math.Round(pct, MidpointRounding.AwayFromZero);

            if (p > progressBar1.Maximum)
            {
                p = progressBar1.Maximum;
            }

            if (p < progressBar1.Minimum)
            {
                p = progressBar1.Minimum;
            }

            progressBar1.Value = p;
            TaskbarList.SetProgressValue(_parentForm.Handle, p, 100);
        }

        private void buttonAddFile_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFileTitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = UiUtil.GetVideoFileFilter(true);
                openFileDialog1.Multiselect = true;
                if (openFileDialog1.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                foreach (var fileName in openFileDialog1.FileNames)
                {
                    AddInputFile(fileName);
                }
            }
        }

        private void buttonRemoveFile_Click(object sender, EventArgs e)
        {
            for (var i = listViewInputFiles.SelectedIndices.Count - 1; i >= 0; i--)
            {
                listViewInputFiles.Items.RemoveAt(listViewInputFiles.SelectedIndices[i]);
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            listViewInputFiles.Items.Clear();
        }

        private void buttonBatchMode_Click(object sender, EventArgs e)
        {
            _batchMode = !_batchMode;
            ShowHideBatchMode();
        }

        private void ShowHideBatchMode()
        {
            int bottom = 100;
            if (_batchMode)
            {
                EnableGroupBoxInputFiles(true);
                Height = bottom + progressBar1.Height + buttonCancel.Height + 470;
                listViewInputFiles.Visible = true;
                buttonBatchMode.Text = LanguageSettings.Current.Split.Basic;
                MinimumSize = new Size(MinimumSize.Width, Height - 75);
                FormBorderStyle = FormBorderStyle.Sizable;
                MaximizeBox = true;
                MinimizeBox = true;
            }
            else
            {
                EnableGroupBoxInputFiles(false);
                var h = bottom + progressBar1.Height + buttonCancel.Height + 110;
                MinimumSize = new Size(MinimumSize.Width, h - 10);
                Height = h;
                Width = _initialWidth;
                listViewInputFiles.Visible = false;
                buttonBatchMode.Text = LanguageSettings.Current.AudioToText.BatchMode;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = true;
            }
        }

        private void AudioToText_Load(object sender, EventArgs e)
        {
            ShowHideBatchMode();
            listViewInputFiles.Columns[0].Width = -2;
        }

        private void AudioToText_Shown(object sender, EventArgs e)
        {
            buttonGenerate.Focus();
            _initialWidth = Width;

            AudioToText_ResizeEnd(null, null);
        }


        private void listViewInputFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (!buttonGenerate.Visible || buttonAddFile.Enabled == false)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void listViewInputFiles_DragDrop(object sender, DragEventArgs e)
        {
            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);

            TaskDelayHelper.RunDelayed(TimeSpan.FromMilliseconds(25), () =>
            {
                listViewInputFiles.BeginUpdate();
                foreach (var fileName in fileNames.OrderBy(Path.GetFileName))
                {
                    if (File.Exists(fileName))
                    {
                        AddInputFile(fileName);
                    }
                }

                listViewInputFiles.EndUpdate();
            });
        }

        private void AddInputFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if ((Utilities.AudioFileExtensions.Contains(ext) || Utilities.VideoFileExtensions.Contains(ext)) && File.Exists(fileName))
            {
                listViewInputFiles.Items.Add(fileName);
            }
        }

        private void AudioToText_ResizeEnd(object sender, EventArgs e)
        {
            listViewInputFiles.AutoSizeLastColumn();
            labelElapsed.Left = progressBar1.Width - labelElapsed.Width + 10;
        }

        private void listViewInputFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (buttonAddFile.Enabled == false)
            {
                return;
            }

            if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control) //Ctrl+V = Paste from clipboard
            {
                e.SuppressKeyPress = true;
                if (Clipboard.ContainsFileDropList())
                {
                    foreach (var fileName in Clipboard.GetFileDropList())
                    {
                        AddInputFile(fileName);
                    }
                }
                else if (Clipboard.ContainsText())
                {
                    foreach (var fileName in Clipboard.GetText().SplitToLines())
                    {
                        AddInputFile(fileName);
                    }
                }
            }
        }

        private void comboBoxLanguages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxLanguages.SelectedIndex > 0 && comboBoxLanguages.Text == LanguageSettings.Current.General.ChangeLanguageFilter)
            {
                using (var form = new DefaultLanguagesChooser(Configuration.Settings.General.DefaultLanguages))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        Configuration.Settings.General.DefaultLanguages = form.DefaultLanguages;
                    }
                }

                InitializeLanguageNames(comboBoxLanguages);
                return;
            }

            //checkBoxTranslateToEnglish.Enabled = comboBoxLanguages.Text.ToLowerInvariant() != "english";
        }

        internal static void InitializeLanguageNames(NikseComboBox comboBox)
        {
            comboBox.Items.Clear();

            var languagesFilled = false;

            if (!string.IsNullOrEmpty(Configuration.Settings.General.DefaultLanguages))
            {
                var favorites = Utilities.GetSubtitleLanguageCultures(true).ToList();
                var languages = DagloLanguage.Languages;
                var languagesToAdd = new List<DagloLanguage>();

                foreach (var dagloLanguage in languages)
                {
                    if (favorites.Any(p => p.TwoLetterISOLanguageName == dagloLanguage.Code) ||
                        favorites.Any(p2 => p2.EnglishName.Contains(dagloLanguage.Name, StringComparison.OrdinalIgnoreCase)) ||
                        favorites.Any(p3 => dagloLanguage.Name.Contains(p3.EnglishName, StringComparison.OrdinalIgnoreCase)))
                    {
                        languagesFilled = true;
                        languagesToAdd.Add(dagloLanguage);
                    }
                }

                comboBox.Items.AddItems(languagesToAdd.OrderBy(p => p.Name));

                var lang = languages.FirstOrDefault(p => p.Code == Configuration.Settings.Tools.DagloLanguageCode);
                comboBox.Text = lang != null ? lang.ToString() : "Korean";
            }

            if (!languagesFilled)
            {
                comboBox.Items.AddItems(DagloLanguage.Languages.OrderBy(p => p.Name));
                var lang = WhisperLanguage.Languages.FirstOrDefault(p => p.Code == Configuration.Settings.Tools.DagloLanguageCode);
                comboBox.Text = lang != null ? lang.ToString() : "Korean";
            }

            // 언어 필터는 사용하지 않음
            //comboBox.Items.Add(LanguageSettings.Current.General.ChangeLanguageFilter);

            if (string.IsNullOrEmpty(comboBox.Text) && comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }
        }


        private void removeTemporaryFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.DagloDeleteTempFiles = !Configuration.Settings.Tools.DagloDeleteTempFiles;
            removeTemporaryFilesToolStripMenuItem.Checked = Configuration.Settings.Tools.DagloDeleteTempFiles;
        }


        private void runOnlyPostProcessingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                labelProgress.Text = LanguageSettings.Current.AudioToText.PostProcessing;

                _languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle);
                var postProcessor = new AudioToTextPostProcessor(_languageCode)
                {
                    ParagraphMaxChars = Configuration.Settings.General.SubtitleLineMaximumLength * 2,
                };

                WavePeakData wavePeaks = null;
                if (checkBoxAutoAdjustTimings.Checked)
                {
                    wavePeaks = _wavePeaks ?? MakeWavePeaks();
                }

                if (checkBoxAutoAdjustTimings.Checked && wavePeaks != null)
                {
                    _subtitle = WhisperTimingFixer.ShortenLongDuration(_subtitle);
                    _subtitle = WhisperTimingFixer.ShortenViaWavePeaks(_subtitle, wavePeaks);
                }
                else
                {
                    return;
                }

                TranscribedSubtitle = postProcessor.Fix(AudioToTextPostProcessor.Engine.Whisper,
                    _subtitle,
                    false,
                    true, //Configuration.Settings.Tools.WhisperPostProcessingAddPeriods,
                    true, //Configuration.Settings.Tools.WhisperPostProcessingMergeLines,
                    true, //Configuration.Settings.Tools.WhisperPostProcessingFixCasing,
                    true, //Configuration.Settings.Tools.WhisperPostProcessingFixShortDuration,
                    true //Configuration.Settings.Tools.WhisperPostProcessingSplitLines
                    );
                DialogResult = DialogResult.OK;
            }
            finally
            {
                buttonGenerate.Enabled = true;
                //buttonDownload.Enabled = true;
                buttonBatchMode.Enabled = false;
                //buttonAdvanced.Enabled = true;
                comboBoxLanguages.Enabled = true;
                //comboBoxModels.Enabled = true;
                //linkLabelPostProcessingConfigure.Enabled = true;
            }
        }


        private void ShowWhisperLogFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UiUtil.OpenFile(SeLogger.GetWhisperLogFilePath());
        }


        public static void ShowPostProcessingSettings(Form owner)
        {
            using (var form = new PostProcessingSettings()
            {
                AddPeriods = Configuration.Settings.Tools.WhisperPostProcessingAddPeriods,
                MergeLines = Configuration.Settings.Tools.WhisperPostProcessingMergeLines,
                SplitLines = Configuration.Settings.Tools.WhisperPostProcessingSplitLines,
                FixCasing = Configuration.Settings.Tools.WhisperPostProcessingFixCasing,
                FixShortDuration = Configuration.Settings.Tools.WhisperPostProcessingFixShortDuration,
            })
            {
                if (form.ShowDialog(owner) == DialogResult.OK)
                {
                    Configuration.Settings.Tools.WhisperPostProcessingAddPeriods = form.AddPeriods;
                    Configuration.Settings.Tools.WhisperPostProcessingMergeLines = form.MergeLines;
                    Configuration.Settings.Tools.WhisperPostProcessingSplitLines = form.SplitLines;
                    Configuration.Settings.Tools.WhisperPostProcessingFixCasing = form.FixCasing;
                    Configuration.Settings.Tools.WhisperPostProcessingFixShortDuration = form.FixShortDuration;
                }
            }
        }

        private void DagloAudioToText_Activated(object sender, EventArgs e)
        {
            BringToFront();
        }

        private void PrintSubtitleInfo(string title, Subtitle subtitle)
        {
            // For debugging
            Debug.WriteLine($"--- {title} ---");
            if (subtitle == null || subtitle.Paragraphs.Count == 0)
            {
                Debug.WriteLine("No paragraphs.");
                return;
            }
            for (int i = 0; i < Math.Min(5, subtitle.Paragraphs.Count); i++)
            {
                var p = subtitle.Paragraphs[i];
                Debug.WriteLine($"{i + 1}: [{p.StartTime}] --> [{p.EndTime}] {p.Text}");
            }
            if (subtitle.Paragraphs.Count > 5)
            {
                Debug.WriteLine($"... {subtitle.Paragraphs.Count - 5} more");
            }
        }
    }
}
