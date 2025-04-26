using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NLog;
using NorthwindConsole.Model;

namespace NorthwindConsole
{
    public static class CategoryActions
    {
        public static void DisplayCategories(Logger logger)
        {
            var db = new DataContext();
            var query = db.Categories.OrderBy(p => p.CategoryName);
            int count = query.Count();
            logger.Info($"{count} categories returned");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{count} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName} - {item.Description}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void AddCategory(Logger logger)
        {
            Category category = new();
            Console.WriteLine("Enter Category Name:");
            category.CategoryName = Console.ReadLine()!;
            Console.WriteLine("Enter the Category Description:");
            category.Description = Console.ReadLine();
            ValidationContext context = new(category, null, null);
            List<ValidationResult> results = [];
            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
                var db = new DataContext();
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", ["CategoryName"]));
                    logger.Error(
                        $"Add category failed: Name '{category.CategoryName}' already exists"
                    );
                }
                else
                {
                    logger.Info("Validation passed");
                    try
                    {
                        db.Categories.Add(category);
                        db.SaveChanges();
                        logger.Info($"Category '{category.CategoryName}' added successfully.");
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Error adding category");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(
                            "An error occurred while adding the category. Please try again."
                        );
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }
            if (!isValid)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var result in results)
                {
                    logger.Error(
                        $"{result.MemberNames.FirstOrDefault() ?? ""} : {result.ErrorMessage}"
                    );
                    Console.WriteLine(result.ErrorMessage);
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void DisplayCategoryAndProducts(Logger logger)
        {
            var db = new DataContext();
            var query = db.Categories.OrderBy(p => p.CategoryId);
            Console.WriteLine("Select the category whose products you want to display:");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            if (
                !int.TryParse(Console.ReadLine(), out int id) || !query.Any(c => c.CategoryId == id)
            )
            {
                logger.Error("Invalid category selection");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid selection. Please enter a valid category number.");
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }
            Console.Clear();
            logger.Info($"CategoryId {id} selected");
            Category category = db
                .Categories.Include("Products")
                .FirstOrDefault(c => c.CategoryId == id)!;
            Console.WriteLine($"{category.CategoryName} - {category.Description}");
            foreach (Product p in category.Products)
            {
                Console.WriteLine($"\t{p.ProductName}");
            }
        }

        public static void DisplayAllCategoriesAndProducts(Logger logger)
        {
            var db = new DataContext();
            var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName}");
                foreach (Product p in item.Products)
                {
                    if (p.Discontinued)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\t{p.ProductName} (Discontinued)");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        Console.WriteLine($"\t{p.ProductName}");
                    }
                }
            }
        }

        public static void EditCategory(Logger logger)
        {
            var db = new DataContext();
            var categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
            if (categories.Count == 0)
            {
                logger.Info("No categories found to edit");
                Console.WriteLine("No categories found.");
                return;
            }
            Console.WriteLine("Select the Category to edit:");
            foreach (var c in categories)
            {
                Console.WriteLine($"{c.CategoryId}) {c.CategoryName}");
            }
            if (
                !int.TryParse(Console.ReadLine(), out int categoryId)
                || !categories.Any(c => c.CategoryId == categoryId)
            )
            {
                logger.Error("Invalid category selection");
                return;
            }
            var category = db.Categories.First(c => c.CategoryId == categoryId);
            logger.Info($"Editing Category: {category.CategoryName} (ID: {category.CategoryId})");
            Console.WriteLine($"Editing Category: {category.CategoryName}");
            Console.WriteLine($"Enter Category Name ({category.CategoryName}):");
            string? input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                category.CategoryName = input;
            Console.WriteLine($"Enter Description ({category.Description}):");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                category.Description = input;
            ValidationContext context = new(category, null, null);
            List<ValidationResult> results = [];
            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (
                db.Categories.Any(c =>
                    c.CategoryId != category.CategoryId && c.CategoryName == category.CategoryName
                )
            )
            {
                isValid = false;
                results.Add(new ValidationResult("Name exists", ["CategoryName"]));
                logger.Error(
                    $"Edit category failed: Name '{category.CategoryName}' already exists"
                );
            }
            if (isValid)
            {
                try
                {
                    db.SaveChanges();
                    logger.Info($"Category '{category.CategoryName}' updated successfully.");
                    Console.WriteLine($"Category '{category.CategoryName}' updated successfully.");
                }
                catch (Exception ex)
                {
                    logger.Error($"Error updating category: {ex.Message}");
                }
            }
            else
            {
                foreach (var result in results)
                {
                    logger.Error(
                        $"{result.MemberNames.FirstOrDefault() ?? ""} : {result.ErrorMessage}"
                    );
                }
            }
        }

        public static void DisplayAllCategoriesAndActiveProducts(Logger logger)
        {
            var db = new DataContext();
            var query = db.Categories.Include("Products").OrderBy(c => c.CategoryId);
            foreach (var category in query)
            {
                var activeProducts = category
                    .Products.Where(p => !p.Discontinued)
                    .OrderBy(p => p.ProductName)
                    .ToList();
                Console.WriteLine($"{category.CategoryName}");
                foreach (var product in activeProducts)
                {
                    Console.WriteLine($"\t{product.ProductName}");
                }
            }
        }

        public static void DisplayCategoryAndActiveProducts(Logger logger)
        {
            var db = new DataContext();
            var categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
            if (categories.Count == 0)
            {
                Console.WriteLine("No categories found.");
                return;
            }
            Console.WriteLine("Select the Category to view active products:");
            foreach (var c in categories)
            {
                Console.WriteLine($"{c.CategoryId}) {c.CategoryName}");
            }
            if (
                !int.TryParse(Console.ReadLine(), out int categoryId)
                || !categories.Any(c => c.CategoryId == categoryId)
            )
            {
                Console.WriteLine("Invalid category selection.");
                return;
            }
            var category = db.Categories.Include("Products").First(c => c.CategoryId == categoryId);
            var activeProducts = category
                .Products.Where(p => !p.Discontinued)
                .OrderBy(p => p.ProductName)
                .ToList();
            Console.WriteLine($"{category.CategoryName}");
            foreach (var product in activeProducts)
            {
                Console.WriteLine($"\t{product.ProductName}");
            }
        }

        public static void DeleteCategory(Logger logger)
        {
            var db = new DataContext();
            var categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
            if (categories.Count == 0)
            {
                logger.Info("No categories found to delete");
                return;
            }
            Console.WriteLine("Select the Category to delete:");
            foreach (var c in categories)
            {
                Console.WriteLine($"{c.CategoryId}) {c.CategoryName}");
            }
            if (
                !int.TryParse(Console.ReadLine(), out int categoryId)
                || !categories.Any(c => c.CategoryId == categoryId)
            )
            {
                logger.Error("Invalid category selection");
                return;
            }
            var category = db
                .Categories.Include(c => c.Products)
                .ThenInclude(p => p.OrderDetails)
                .First(c => c.CategoryId == categoryId);
            foreach (var product in category.Products.ToList())
            {
                foreach (var od in product.OrderDetails.ToList())
                {
                    db.OrderDetails.Remove(od);
                }
                db.Products.Remove(product);
            }
            db.Categories.Remove(category);
            try
            {
                db.SaveChanges();
                logger.Info(
                    $"Category '{category.CategoryName}', its products, and their order details deleted successfully."
                );
            }
            catch (Exception ex)
            {
                logger.Error($"Error deleting category: {ex.Message}");
            }
        }

        public static void ShowCategoryMenu(Logger logger)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("========================================");
                Console.WriteLine("           Category Menu");
                Console.WriteLine("========================================");
                Console.WriteLine(" 1) Display categories");
                Console.WriteLine(" 2) Add category");
                Console.WriteLine(" 3) Edit category");
                Console.WriteLine(" 4) Delete category");
                Console.WriteLine(" 5) Display Category and related products");
                Console.WriteLine(" 6) Display all Categories and their related products");
                Console.WriteLine(" 7) Display all Categories and their active products");
                Console.WriteLine(" 8) Display a specific Category and its active products");
                Console.WriteLine(" [Enter] to return to main menu");
                Console.WriteLine("----------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;
                string? choice = Console.ReadLine();
                Console.Clear();
                switch (choice)
                {
                    case "1":
                        DisplayCategories(logger);
                        break;
                    case "2":
                        AddCategory(logger);
                        break;
                    case "3":
                        EditCategory(logger);
                        break;
                    case "4":
                        DeleteCategory(logger);
                        break;
                    case "5":
                        DisplayCategoryAndProducts(logger);
                        break;
                    case "6":
                        DisplayAllCategoriesAndProducts(logger);
                        break;
                    case "7":
                        DisplayAllCategoriesAndActiveProducts(logger);
                        break;
                    case "8":
                        DisplayCategoryAndActiveProducts(logger);
                        break;
                    case null:
                    case "":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }
    }
}
