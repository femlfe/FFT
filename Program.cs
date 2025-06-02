
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using MathNet.Numerics.IntegralTransforms;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        string inputFilePath = @"C:\Users\ксюша\Downloads\Telegram Desktop\аудио\аудио\miau.wav";
        var result = АudioСonversion.Conversion(inputFilePath);
        SpectralAnalysis(result);
    }

    static class АudioСonversion
    {
        public static AudioFileReader Conversion(string path)
        {
            AudioFileReader originalAudio = new AudioFileReader(path);

            Console.WriteLine($"Количество каналов до: {originalAudio.WaveFormat.Channels}");

            originalAudio = ConvertToMono(originalAudio);

            Console.WriteLine($"Количество каналов после: {originalAudio.WaveFormat.Channels}");

            Console.WriteLine($"\nЧастота дискретизации до: {originalAudio.WaveFormat.SampleRate}");

            originalAudio = ChangeTheSamplingRate(originalAudio, 44100);

            Console.WriteLine($"Частота дискретизации после: {originalAudio.WaveFormat.SampleRate}");

            Console.WriteLine($"\nМаксимальная амплитуда до: {FindMaxAmplitude(originalAudio)}");

            AudioFileReader normalizedAudio = NormalizationOfSignalAmplitude(originalAudio);

            Console.WriteLine($"Максимальная амплитуда после: {FindMaxAmplitude(normalizedAudio)}");

            NormalizationOfSignalAmplitude(normalizedAudio);

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
                Console.WriteLine("Нормализация не требуется.");
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

    private static List<System.Numerics.Complex> SpectralAnalysis(AudioFileReader audio)
    {
        float[] sample = new float[audio.WaveFormat.SampleRate * (int)audio.TotalTime.TotalSeconds];
        audio.Read(sample, 0, sample.Length);

        int frameSize = 1024;
        int overlap = frameSize / 2;
        int stepSize = frameSize - overlap;

        List<System.Numerics.Complex> allPeaks = new List<System.Numerics.Complex>();

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
            }
        }

        Console.WriteLine("Все пики (частота + амплитуда):");
        foreach (var peak in allPeaks)
        {
            Console.WriteLine($"{peak.Real} Hz, {peak.Imaginary} dB");
        }
        return allPeaks;
    }

    private static List<uint> GroupPeaksIntoHashes(List<System.Numerics.Complex> peaks)
    {
        List<uint> fingerprint = new List<uint>();

        for (int i = 0; i < peaks.Count - 2; i++)
        {
            double freq1 = peaks[i].Real;
            double freq2 = peaks[i + 1].Real;
            double freq3 = peaks[i + 2].Real;

            string hashInput = $"{freq1:F2},{freq2:F2},{freq3:F2}";
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
        using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
        {
            foreach (uint hash in fingerprint)
            {
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


