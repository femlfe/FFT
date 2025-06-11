using Audiofingerprint.Services;
using NAudio.Wave;
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Menu.ShowMenu();
    }

    public static class Menu
    {
        public static void ShowMenu()
        {
            string outputFilePath = @"C:\Users\ксюша\Downloads\Telegram Desktop\fingerprints";

            while (true)
            {
                Console.WriteLine("Меню:");
                Console.WriteLine("1. Сгенерировать отпечаток аудио");
                Console.WriteLine("2. Сравнить два отпечатка на сходство");
                Console.WriteLine("3. Найти самый похожий отпечаток в директории");
                Console.WriteLine("4. Выход");
                Console.Write("Выберите опцию (1, 2, 3 или 4): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        GenerateFingerprintOption(outputFilePath);
                        break;

                    case "2":
                        CompareFingerprintsOption();
                        break;

                    case "3":
                        FindMostSimilarFileOption();
                        break;

                    case "4":
                        Console.WriteLine("Выход из программы.");
                        return;

                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте снова.");
                        break;
                }
            }
        }

        private static void GenerateFingerprintOption(string output)
        {
            try
            {
                Console.Write("Введите путь к аудиофайлу: ");
                string audioFilePath = Console.ReadLine();

                if (!File.Exists(audioFilePath))
                {
                    Console.WriteLine("Файл не найден. Попробуйте снова.");
                    return;
                }
                if (Path.GetExtension(audioFilePath) != ".wav")
                {
                    Console.WriteLine("Программа обрабатывает только .wav файлы");
                    return;
                }

                FingerprintService fingerprintService = new FingerprintService();

                fingerprintService.GenerateFingerprint(audioFilePath, output);

                Console.WriteLine("Отпечаток успешно сгенерирован и сохранен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private static void CompareFingerprintsOption()
        {
            try
            {
                Console.Write("Введите путь к первому файлу опечатка: ");
                string firstPath = Console.ReadLine();

                if (!File.Exists(firstPath))
                {
                    Console.WriteLine("Первый файл не найден. Попробуйте снова.");
                    return;
                }
                if (Path.GetExtension(firstPath) != ".bin")
                {
                    Console.WriteLine("Программа сравнивает только .bin файлы");
                    return;
                }
                Console.Write("Введите путь ко второму файлу опечатка: ");
                string secondPath = Console.ReadLine();

                if (!File.Exists(secondPath))
                {
                    Console.WriteLine("Второй файл не найден. Попробуйте снова.");
                    return;
                }
                if (Path.GetExtension(secondPath) != ".bin")
                {
                    Console.WriteLine("Программа сравнивает только .bin файлы");
                    return;
                }

                FingerprintService fingerprintService = new FingerprintService();
                double similarity = fingerprintService.CompareFingerprints(firstPath, secondPath);

                Console.WriteLine($"Процент совпадения отпечатков: {similarity:F2}%");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private static void FindMostSimilarFileOption()
        {
            try
            {
                Console.Write("Введите путь к файлу, с которым нужно сравнить: ");
                string targetFilePath = Console.ReadLine();

                if (!File.Exists(targetFilePath))
                {
                    Console.WriteLine("Файл не найден. Попробуйте снова.");
                    return;
                }
                if (Path.GetExtension(targetFilePath) != ".bin")
                {
                    Console.WriteLine("Программа сравнивает только .bin файлы");
                    return;
                }

                string directoryPath = "C:\\Users\\ксюша\\Downloads\\Telegram Desktop\\fingerprints";


                string[] files = Directory.GetFiles(directoryPath);
                string mostSimilarFile = null;
                double maxSimilarity = 0;

                foreach (string file in files)
                {

                    FingerprintService fingerprintService = new FingerprintService();
                    double similarity = fingerprintService.CompareFingerprints(targetFilePath, file);
                    Console.WriteLine($"Сравнение с {Path.GetFileName(file)}: {similarity:F2}%");

                    if (similarity > maxSimilarity)
                    {
                        maxSimilarity = similarity;
                        mostSimilarFile = file;
                    }
                }

                if (mostSimilarFile != null)
                {
                    Console.WriteLine($"Самый похожий файл: {Path.GetFileName(mostSimilarFile)} (Сходство: {maxSimilarity:F2}%)");
                }
                else
                {
                    Console.WriteLine("Подходящих файлов не найдено.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}
