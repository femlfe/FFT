using Audiofingerprint.Classes;
using NAudio.Wave;
using System.IO;

namespace FFTFull.Test
{
    public class AudioConversionTest
    {
        [Fact]
        public void Conversion_NotWavExtention_ExceptionThrown()
        {
            Assert.Throws<Exception>(() => ¿udio—onversion.Conversion("C:\\Users\\ÍÒ˛¯‡\\Downloads\\Telegram Desktop\\‡Û‰ËÓ\\miau2.mp3"));
        }
        
        [Fact]
        public void ConvertToMono_TwoChannels_OneChannel()
        {
            //Arrange 
            AudioFileReader originalAudio = new AudioFileReader("C:\\Users\\ÍÒ˛¯‡\\Downloads\\Telegram Desktop\\‡Û‰ËÓ\\miau.wav");

            //Act
            int beforeConvert = originalAudio.WaveFormat.Channels;

            originalAudio = ¿udio—onversion.ConvertToMono(originalAudio);

            //Assert 
            Assert.Equal(2, beforeConvert);
            Assert.Equal(1, originalAudio.WaveFormat.Channels);
        }

        [Fact]
        public void ConvertToMono_OneChannel_OneChannel()
        {
            //Arrange 
            AudioFileReader originalAudio = new AudioFileReader("C:\\Users\\ÍÒ˛¯‡\\Downloads\\Telegram Desktop\\‡Û‰ËÓ\\speach1.wav");

            //Act
            int beforeConvert = originalAudio.WaveFormat.Channels;

            originalAudio = ¿udio—onversion.ConvertToMono(originalAudio);

            //Assert 
            Assert.Equal(1, beforeConvert);
            Assert.Equal(1, originalAudio.WaveFormat.Channels);
        }
        
        [Fact]
        public void ChangeTheSamplingRate_48000_41000()
        {
            //Arrange 
            AudioFileReader originalAudio = new AudioFileReader("C:\\Users\\ÍÒ˛¯‡\\Downloads\\Telegram Desktop\\‡Û‰ËÓ\\miau.wav");

            //Act
            int samplingRateBefore = originalAudio.WaveFormat.SampleRate;

            originalAudio = ¿udio—onversion.ChangeTheSamplingRate(originalAudio, 41000);

            //Assert 
            Assert.Equal(48000, samplingRateBefore);
            Assert.Equal(41000, originalAudio.WaveFormat.SampleRate);
        }

        [Fact]
        public void ChangeTheSamplingRate_44100_41000()
        {
            //Arrange 
            AudioFileReader originalAudio = new AudioFileReader("C:\\Users\\ÍÒ˛¯‡\\Downloads\\Telegram Desktop\\‡Û‰ËÓ\\popipo.wav");

            //Act
            int samplingRateBefore = originalAudio.WaveFormat.SampleRate;

            originalAudio = ¿udio—onversion.ChangeTheSamplingRate(originalAudio, 41000);

            //Assert 
            Assert.Equal(44100, samplingRateBefore);
            Assert.Equal(41000, originalAudio.WaveFormat.SampleRate);
        }

        [Fact]
        public void NormalizationOfSignalAmplitude_NotNormalized_Normilized()
        {
            //Arrange 
            AudioFileReader originalAudio = new AudioFileReader("C:\\Users\\ÍÒ˛¯‡\\Downloads\\Telegram Desktop\\‡Û‰ËÓ\\speach2.wav");

            //Act
            float maxAmplitudeBefore = ¿udio—onversion.FindMaxAmplitude(originalAudio);

            AudioFileReader normalizedAudio = ¿udio—onversion.NormalizationOfSignalAmplitude(originalAudio);

            //Assert 
            Assert.True(maxAmplitudeBefore < 1f);
            Assert.Equal(1, ¿udio—onversion.FindMaxAmplitude(normalizedAudio));
        }
    }
}