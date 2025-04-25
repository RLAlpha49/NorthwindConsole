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
                Console.WriteLine("  1) Display categories");
                Console.WriteLine("  2) Add category");
                Console.WriteLine("  3) Edit category");
                Console.WriteLine("  4) Delete category");
                Console.WriteLine();
                Console.WriteLine("  5) Display Category and related products");
                Console.WriteLine("  6) Display all Categories and their related products");
                Console.WriteLine("  7) Display all Categories and their active products");
                Console.WriteLine("  8) Display a specific Category and its active products");
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[Products]");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  9) Display products");
                Console.WriteLine(" 10) Add product");
                Console.WriteLine(" 11) Edit product");
                Console.WriteLine(" 12) Delete product");
                Console.WriteLine(" 13) Display a specific product");
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
                        CategoryActions.DisplayCategories(logger);
                        break;
                    case "2":
                        CategoryActions.AddCategory(logger);
                        break;
                    case "3":
                        CategoryActions.EditCategory(logger);
                        break;
                    case "4":
                        CategoryActions.DeleteCategory(logger);
                        break;
                    case "5":
                        CategoryActions.DisplayCategoryAndProducts(logger);
                        break;
                    case "6":
                        CategoryActions.DisplayAllCategoriesAndProducts(logger);
                        break;
                    case "7":
                        CategoryActions.DisplayAllCategoriesAndActiveProducts(logger);
                        break;
                    case "8":
                        CategoryActions.DisplayCategoryAndActiveProducts(logger);
                        break;
                    case "9":
                        ProductActions.DisplayProducts(logger);
                        break;
                    case "10":
                        ProductActions.AddProduct(logger);
                        break;
                    case "11":
                        ProductActions.EditProduct(logger);
                        break;
                    case "12":
                        ProductActions.DeleteProduct(logger);
                        break;
                    case "13":
                        ProductActions.DisplayProductDetails(logger);
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
