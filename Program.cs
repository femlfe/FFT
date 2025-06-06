using Audiofingerprint;


class Program
{
    static void Main(string[] args)
    {
        string inputFilePath = @"C:\Users\ксюша\Downloads\Telegram Desktop\аудио\popipo.wav";
        string inputFilePathS = @"C:\Users\ксюша\Downloads\Telegram Desktop\аудио\popipolen.wav";
        string outputFilePath = @"C:\Users\ксюша\Downloads\Telegram Desktop\fingerprints\fingerprint_";
        string f = @"C:\Users\ксюша\Downloads\Telegram Desktop\fingerprints\fingerprint_71164";
        string s = @"C:\Users\ксюша\Downloads\Telegram Desktop\fingerprints\fingerprint_77666";

        var result = АudioСonversion.ConversionForConsole(inputFilePath);
        var result2 = АudioСonversion.Conversion(inputFilePathS);

        Fingerprints.GenerateFingerprint(result, outputFilePath);
        //Fingerprint.GenerateFingerprint(result2, outputFilePath);

        //Console.WriteLine($"Чистый процент совпадения: {Fingerprint.CompareFingerprints(f, s)}");
    }


   

   
}


