using Audiofingerprint.Classes;
using Audiofingerprint.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTFull.Test
{
    public class FingerprintsTest
    {
        [Fact]
        public void CompareFingerprints_Same_100Persantage()
        {
            //Arrange 

            string outputFilePath = @"C:\Users\ксюша\Downloads\Telegram Desktop\fingerprints\fingerprint_";
            FingerprintService fingerprintService = new FingerprintService();
            string firstPath = "C:\\Users\\ксюша\\Downloads\\Telegram Desktop\\fingerprints\\fingerprint_speach2_73558.bin";
            string secondPath = "C:\\Users\\ксюша\\Downloads\\Telegram Desktop\\fingerprints\\fingerprint_speach2_73558.bin";

            //Act

            double persntg = fingerprintService.CompareFingerprints(firstPath, secondPath);

            //Assert 

            Assert.Equal(100, persntg); 
        }

        [Fact]
        public void CompareFingerprints_Difrent_100Persantage()
        {
            //Arrange 

            string outputFilePath = @"C:\Users\ксюша\Downloads\Telegram Desktop\fingerprints\fingerprint_";
            FingerprintService fingerprintService = new FingerprintService();
            string firstPath = "C:\\Users\\ксюша\\Downloads\\Telegram Desktop\\fingerprints\\fingerprint_speach2_73558.bin";
            string secondPath = "C:\\Users\\ксюша\\Downloads\\Telegram Desktop\\fingerprints\\fingerprint_speach2_73558.bin";
            double persntg = fingerprintService.CompareFingerprints(firstPath, secondPath);

            //Assert 

            Assert.True(persntg>50);
        }
    }
}
