using NLog;

namespace NorthwindConsole
{
    public static class MainMenu
    {
        public static void Run()
        {
            string path = Directory.GetCurrentDirectory() + "//nlog.config";
            var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();
            logger.Info("Program started");
            do
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("========================================");
                Console.WriteLine("      Northwind Console Main Menu");
                Console.WriteLine("========================================");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Categories]");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  1) View Category Options");
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[Products]");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  2) View Product Options");
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("[Statistics]");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("  3) View Statistics Options");
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("  [Enter] to quit");
                Console.WriteLine("----------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;
                string? choice = Console.ReadLine();
                Console.Clear();
                logger.Info("Option {choice} selected", choice);

                switch (choice)
                {
                    case "1":
                        CategoryActions.ShowCategoryMenu(logger);
                        break;
                    case "2":
                        ProductActions.ShowProductMenu(logger);
                        break;
                    case "3":
                        StatisticsActions.ShowStatisticsMenu(logger);
                        break;
                    case null:
                    case "":
                        logger.Info("Program ended");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
                Console.WriteLine();
            } while (true);
        }
    }
}
