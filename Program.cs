
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using MathNet.Numerics.IntegralTransforms;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using SoundTouch.Net.NAudioSupport;
using System.Linq;
using System.Runtime.ConstrainedExecution;

class Program
{
    static void Main(string[] args)
    {
        string inputFilePath = @"C:\Users\ксюша\Downloads\Telegram Desktop\аудио\popipo.wav";
        string inputFilePathS = @"C:\Users\ксюша\Downloads\Telegram Desktop\аудио\popipolen.wav";
        string outputFilePath = @"C:\Users\ксюша\Downloads\Telegram Desktop\fingerprints\fingerprint_";
        string f = @"C:\Users\ксюша\Downloads\Telegram Desktop\fingerprints\fingerprint_67285";
        string s = @"C:\Users\ксюша\Downloads\Telegram Desktop\fingerprints\fingerprint_41294";

        //var result = АudioСonversion.Conversion(inputFilePath);
        //var result2 = АudioСonversion.Conversion(inputFilePathS);

        //Fingerprint.GenerateFingerprint(result, outputFilePath);
        //Fingerprint.GenerateFingerprint(result2, outputFilePath);

        Console.WriteLine($"Процент схожести: {Fingerprint.CompareFingerprints(f, s)}");
    }

    static class АudioСonversion
    {
        public static AudioFileReader Conversion(string path)
        {
            AudioFileReader originalAudio = new AudioFileReader(path);

            //Console.WriteLine($"Количество каналов до: {originalAudio.WaveFormat.Channels}");

            originalAudio = ConvertToMono(originalAudio);

            //Console.WriteLine($"Количество каналов после: {originalAudio.WaveFormat.Channels}");

            //Console.WriteLine($"\nЧастота дискретизации до: {originalAudio.WaveFormat.SampleRate}");

            originalAudio = ChangeTheSamplingRate(originalAudio, 44100);

            //Console.WriteLine($"Частота дискретизации после: {originalAudio.WaveFormat.SampleRate}");

            //Console.WriteLine($"\nМаксимальная амплитуда до: {FindMaxAmplitude(originalAudio)}");

            AudioFileReader normalizedAudio = NormalizationOfSignalAmplitude(originalAudio);

            //Console.WriteLine($"Максимальная амплитуда после: {FindMaxAmplitude(normalizedAudio)}");



            return normalizedAudio;
        }
        
        private static AudioFileReader ConvertToMono(AudioFileReader audio)
        {
            if (audio.WaveFormat.Channels < 2)
            {
                Console.WriteLine("Файл уже моно.");
                return audio;
            }
            else
            {
                SampleToWaveProvider monoStream = new SampleToWaveProvider(audio.ToMono());

                Console.WriteLine("Преобразование успешно завершено.");
                return ConvertToAudioFileReader(monoStream, audio.Length);
            }
        }
        private static AudioFileReader ChangeTheSamplingRate(AudioFileReader audio, int samplingRate)
        {

            var resampler = new WdlResamplingSampleProvider(audio, samplingRate);
            var waveProvider = new SampleToWaveProvider(resampler);

            return ConvertToAudioFileReader(waveProvider, audio.Length);
        }
        private static AudioFileReader ConvertToAudioFileReader(SampleToWaveProvider audio, long lenth)
        {
            string tempWavPath = Path.GetTempFileName() + ".wav";

            using (var writer = new WaveFileWriter(tempWavPath, audio.WaveFormat))
            {
                byte[] buffer = new byte[lenth * sizeof(float)];
                int bytesRead;

                while ((bytesRead = audio.Read(buffer, 0, buffer.Length)) > 0)
                {
                    writer.Write(buffer, 0, bytesRead);
                }
            }

            var result = new AudioFileReader(tempWavPath);
            return result;
        }
        private static AudioFileReader NormalizationOfSignalAmplitude(AudioFileReader audio)
        {
            float[] buffer = new float[1024];

            float maxAmplitude = FindMaxAmplitude(audio);

            if (maxAmplitude <= 0 || maxAmplitude == 1)
            {
                //Console.WriteLine("Нормализация не требуется.");
                return audio;
            }
            audio.Position = 0;

            string tempWavPath = Path.ChangeExtension(Path.GetTempFileName(), ".wav");

            using (WaveFileWriter writer = new WaveFileWriter(tempWavPath, audio.WaveFormat))
            {
                while (audio.Position < audio.Length)
                {
                    int count = audio.Read(buffer, 0, buffer.Length);
                    if (count == 0) break;

                    for (int i = 0; i < count; i++)
                    {
                        buffer[i] = buffer[i] / maxAmplitude;
                    }

                    writer.WriteSamples(buffer, 0, count);
                }
            }
            return new AudioFileReader(tempWavPath);
        }
        private static float FindMaxAmplitude(AudioFileReader audio)
        {
            float[] buffer = new float[1024];
            float maxAbsValue = 0f;

            long originalPosition = audio.Position;

            audio.Position = 0;

            while (audio.Position < audio.Length)
            {
                int count = audio.Read(buffer, 0, buffer.Length);
                if (count == 0) break;

                for (int i = 0; i < count; i++)
                {
                    float absVal = Math.Abs(buffer[i]);
                    if (absVal > maxAbsValue)
                        maxAbsValue = absVal;
                }
            }
            audio.Position = originalPosition;

            return maxAbsValue;
        }
    }


    static class Fingerprint {

        public static void GenerateFingerprint(AudioFileReader audio, string filePathForSave)
        {
            List<System.Numerics.Complex> complexArray = SpectralAnalysis(audio);
            SaveFingerprintToBinaryFile(GroupPeaksIntoHashes(complexArray), filePathForSave);
        }
        public static double CompareFingerprints(string firstPath, string secondPath)
        {
            byte[] file1Bytes = File.ReadAllBytes(firstPath);
            byte[] file2Bytes = File.ReadAllBytes(secondPath);

            int lenth = file1Bytes.Length < file2Bytes.Length ? file1Bytes.Length : file2Bytes.Length;

            double persantage = 0;
            int numDifrent = 0;

            for (int i = 0; i < lenth; i++)
            {
                if (file1Bytes[i] != file2Bytes[i])
                {
                    numDifrent++;
                }
            }

            persantage = 100 - numDifrent/(lenth / 100);

            return persantage;
        }
        public static double CountRanges(double frequency)
        {
            double num = Math.Floor(frequency / 100)*100;
            double result = frequency - num;

            return (result <= 50) ? num : 100 + num;

            //return (result >= 25 && result <= 50) ? 50 + num : (result < 25) ? num : 100 + num;
        }
        public static List<System.Numerics.Complex> SpectralAnalysis(AudioFileReader audio)
        {
            float[] sample = new float[audio.WaveFormat.SampleRate * (int)audio.TotalTime.TotalSeconds];
            audio.Read(sample, 0, sample.Length);

            int frameSize = 1024;
            int overlap = frameSize / 2;
            int stepSize = frameSize - overlap;

            List<System.Numerics.Complex> allPeaks = new List<System.Numerics.Complex>();
            Dictionary<double, double> frequencyAmplitudeMap = new Dictionary<double, double>();

            for (int i = 0; i < sample.Length - frameSize; i += stepSize)
            {
                float[] frame = new float[frameSize];
                Array.Copy(sample, i, frame, 0, frameSize);
                frame = ApplyHannWindow(frame);

                System.Numerics.Complex[] fftForWindow = FFT(frame);

                double[] amplitude = fftForWindow.Select(c => c.Magnitude).ToArray();
                double[] frequency = Enumerable.Range(0, frameSize)
                                              .Select(k => (double)k * audio.WaveFormat.SampleRate / frameSize)
                                              .Take(frameSize / 2)
                                              .ToArray();

                double meanAmplitude = amplitude.Average();
                double threshold = 1.5 * meanAmplitude;

                var peaks = frequency.Zip(amplitude, (freq, amp) => new { Frequency = freq, Amplitude = amp })
                                     .Where(x => x.Amplitude > threshold)
                                     .OrderByDescending(x => x.Amplitude)
                                     .Take(5)
                                     .ToList();

                foreach (var peak in peaks)
                {
                    allPeaks.Add(new System.Numerics.Complex(peak.Frequency, peak.Amplitude));

                    if (frequencyAmplitudeMap.ContainsKey(peak.Frequency))
                    {
                        frequencyAmplitudeMap[peak.Frequency] += peak.Amplitude;
                    }
                    else
                    {
                        frequencyAmplitudeMap[peak.Frequency] = peak.Amplitude;
                    }
                }
            }
            GenerateHistogram(frequencyAmplitudeMap);

            return allPeaks;
        }
        private static void GenerateHistogram(Dictionary<double, double> frequencyAmplitudeMap)
        {
            int width = 600;
            int height = 400;
            int margin = 50;

            using (Bitmap bitmap = new Bitmap(width, height))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);

                Pen axisPen = new Pen(Color.Black, 2);
                g.DrawLine(axisPen, margin, height - margin, width - margin, height - margin);
                g.DrawLine(axisPen, margin, margin, margin, height - margin);


                var groupedData = frequencyAmplitudeMap
                    .GroupBy(kvp => (int)(kvp.Key / 100) * 100) 
                    .ToDictionary(
                        group => group.Key,
                        group => group.Sum(kvp => kvp.Value) 
                    );

                double maxAmplitude = groupedData.Values.Max();
                int barWidth = 40; 
                int x = margin;

                foreach (var kvp in groupedData.OrderBy(kvp => kvp.Key))
                {
                    int barHeight = (int)((kvp.Value / maxAmplitude) * (height - 2 * margin));
                    Color barColor = Color.FromArgb(0, 0, (int)(255 * (kvp.Value / maxAmplitude))); 
                    g.FillRectangle(new SolidBrush(barColor), x, height - margin - barHeight, barWidth, barHeight);
                    x += barWidth + 10; 
                }

                Font font = new Font("Arial", 10);
                g.DrawString("Частота (Гц)", font, Brushes.Black, width / 2 - 50, height - margin + 20);
                g.DrawString("Амплитуда", font, Brushes.Black, margin - 50, margin - 20);
                g.DrawString("Гистограмма частот", new Font("Arial", 14), Brushes.Black, width / 2 - 100, 10);

                x = margin;
                foreach (var kvp in groupedData.OrderBy(kvp => kvp.Key))
                {
                    string label = $"{kvp.Key} Гц";
                    SizeF labelSize = g.MeasureString(label, font);
                    g.DrawString(label, font, Brushes.Black, x + (barWidth - labelSize.Width) / 2, height - margin + 5);
                    x += barWidth + 10;
                }

                Random random = new Random();
                int num = random.Next(0, 100000);
                // Сохраняем изображение
                bitmap.Save($"C:\\Users\\ксюша\\Downloads\\Telegram Desktop\\img\\frequency_histogram_{num}.png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }
        private static List<uint> GroupPeaksIntoHashes(List<System.Numerics.Complex> peaks)
        {
            List<uint> fingerprint = new List<uint>();

            for (int i = 0; i < peaks.Count - 2; i++)
            {
                double freq1 = CountRanges(peaks[i].Real);

                string hashInput = $"{freq1:F2}";

                uint hash = CalculateHash(hashInput);

                fingerprint.Add(hash);
            }

            return fingerprint;
        }
        private static uint CalculateHash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToUInt32(hashBytes, 0);
            }
        }
        private static void SaveFingerprintToBinaryFile(List<uint> fingerprint, string filePath)
        {
            Random random = new Random();
            int name = random.Next(0, 100000);
            filePath += $"{name}";
            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                foreach (uint hash in fingerprint)
                {
                    Console.WriteLine("");
                    writer.Write(hash);
                }
            }
        }
        private static float[] ApplyHannWindow(float[] frame)
        {
            int N = frame.Length;
            for (int i = 0; i < N; i++)
            {
                float windowValue = 0.5f * (1 - (float)Math.Cos(2 * Math.PI * i / (N - 1)));
                frame[i] *= windowValue;
            }

            return frame;
        }
        private static System.Numerics.Complex[] FFT(float[] signal)
        {
            System.Numerics.Complex[] complexSignal = new System.Numerics.Complex[signal.Length];
            for (int i = 0; i < signal.Length; i++)
            {
                complexSignal[i] = new System.Numerics.Complex(signal[i], 0);
            }
            Fourier.Forward(complexSignal);
            return complexSignal;
        }
    }

   
}


