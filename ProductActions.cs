using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NLog;
using NorthwindConsole.Model;

namespace NorthwindConsole
{
    public static class ProductActions
    {
        public static void AddProduct(Logger logger)
        {
            Product product = new();
            Console.WriteLine("Enter Product Name:");
            product.ProductName = Console.ReadLine()!;
            var db = new DataContext();
            var suppliers = db.Suppliers.OrderBy(s => s.SupplierId).ToList();
            Console.WriteLine("Select Supplier:");
            foreach (var s in suppliers)
            {
                Console.WriteLine($"{s.SupplierId}) {s.CompanyName}");
            }
            if (
                int.TryParse(Console.ReadLine(), out int supplierId)
                && suppliers.Any(s => s.SupplierId == supplierId)
            )
            {
                product.SupplierId = supplierId;
                logger.Info($"Supplier selected: {supplierId}");
            }
            else
            {
                logger.Error("Invalid supplier selection");
                return;
            }
            var categories = db.Categories.OrderBy(c => c.CategoryName).ToList();
            Console.WriteLine("Select Category:");
            foreach (var c in categories)
            {
                Console.WriteLine($"{c.CategoryId}) {c.CategoryName}");
            }
            if (
                int.TryParse(Console.ReadLine(), out int categoryId)
                && categories.Any(c => c.CategoryId == categoryId)
            )
            {
                product.CategoryId = categoryId;
                logger.Info($"Category selected: {categoryId}");
            }
            else
            {
                logger.Error("Invalid category selection");
                return;
            }
            Console.WriteLine("Enter Quantity Per Unit (optional):");
            product.QuantityPerUnit = Console.ReadLine();
            Console.WriteLine("Enter Unit Price (optional):");
            if (decimal.TryParse(Console.ReadLine(), out decimal unitPrice))
                product.UnitPrice = unitPrice;
            Console.WriteLine("Enter Units In Stock (optional):");
            if (short.TryParse(Console.ReadLine(), out short unitsInStock))
                product.UnitsInStock = unitsInStock;
            Console.WriteLine("Enter Units On Order (optional):");
            if (short.TryParse(Console.ReadLine(), out short unitsOnOrder))
                product.UnitsOnOrder = unitsOnOrder;
            Console.WriteLine("Enter Reorder Level (optional):");
            if (short.TryParse(Console.ReadLine(), out short reorderLevel))
                product.ReorderLevel = reorderLevel;
            Console.WriteLine("Is Discontinued? (y/n):");
            string? discontinued = Console.ReadLine();
            product.Discontinued = discontinued?.ToLower() == "y";
            ValidationContext context = new(product, null, null);
            List<ValidationResult> results = [];
            var isValid = Validator.TryValidateObject(product, context, results, true);
            if (isValid)
            {
                try
                {
                    db.Products.Add(product);
                    db.SaveChanges();
                    logger.Info($"Product '{product.ProductName}' added successfully.");
                }
                catch (Exception ex)
                {
                    logger.Error($"Error adding product: {ex.Message}");
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

        public static void EditProduct(Logger logger)
        {
            var db = new DataContext();
            var products = db.Products.OrderBy(p => p.ProductId).ToList();
            Console.WriteLine("Select the Product to edit:");
            foreach (var p in products)
            {
                Console.WriteLine($"{p.ProductId}) {p.ProductName}");
            }
            if (
                !int.TryParse(Console.ReadLine(), out int productId)
                || !products.Any(p => p.ProductId == productId)
            )
            {
                logger.Error("Invalid product selection");
                return;
            }
            var product = db.Products.First(p => p.ProductId == productId);
            Console.WriteLine($"Editing Product: {product.ProductName}");
            Console.WriteLine($"Enter Product Name ({product.ProductName}):");
            string? input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                product.ProductName = input;
            var suppliers = db.Suppliers.OrderBy(s => s.SupplierId).ToList();
            Console.WriteLine($"Select Supplier ({product.SupplierId}):");
            foreach (var s in suppliers)
            {
                Console.WriteLine($"{s.SupplierId}) {s.CompanyName}");
            }
            input = Console.ReadLine();
            if (
                int.TryParse(input, out int supplierId)
                && suppliers.Any(s => s.SupplierId == supplierId)
            )
                product.SupplierId = supplierId;
            var categories = db.Categories.OrderBy(c => c.CategoryName).ToList();
            Console.WriteLine($"Select Category ({product.CategoryId}):");
            foreach (var c in categories)
            {
                Console.WriteLine($"{c.CategoryId}) {c.CategoryName}");
            }
            input = Console.ReadLine();
            if (
                int.TryParse(input, out int categoryId)
                && categories.Any(c => c.CategoryId == categoryId)
            )
                product.CategoryId = categoryId;
            Console.WriteLine($"Enter Quantity Per Unit (optional):");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                product.QuantityPerUnit = input;
            Console.WriteLine($"Enter Unit Price ({product.UnitPrice}):");
            input = Console.ReadLine();
            if (decimal.TryParse(input, out decimal unitPrice))
                product.UnitPrice = unitPrice;
            Console.WriteLine($"Enter Units In Stock ({product.UnitsInStock}):");
            input = Console.ReadLine();
            if (short.TryParse(input, out short unitsInStock))
                product.UnitsInStock = unitsInStock;
            Console.WriteLine($"Enter Units On Order ({product.UnitsOnOrder}):");
            input = Console.ReadLine();
            if (short.TryParse(input, out short unitsOnOrder))
                product.UnitsOnOrder = unitsOnOrder;
            Console.WriteLine($"Enter Reorder Level ({product.ReorderLevel}):");
            input = Console.ReadLine();
            if (short.TryParse(input, out short reorderLevel))
                product.ReorderLevel = reorderLevel;
            Console.WriteLine(
                $"Is Discontinued? (y/n, current: {(product.Discontinued ? "y" : "n")}):"
            );
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                product.Discontinued = input.Equals("y", StringComparison.CurrentCultureIgnoreCase);
            ValidationContext context = new(product, null, null);
            List<ValidationResult> results = [];
            var isValid = Validator.TryValidateObject(product, context, results, true);
            if (isValid)
            {
                try
                {
                    db.SaveChanges();
                    logger.Info($"Product '{product.ProductName}' updated successfully.");
                }
                catch (Exception ex)
                {
                    logger.Error($"Error updating product: {ex.Message}");
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

        public static void DisplayProducts(Logger logger)
        {
            var db = new DataContext();
            Console.WriteLine("Display products:");
            Console.WriteLine("1) All products");
            Console.WriteLine("2) Discontinued products");
            Console.WriteLine("3) Active products");
            string? prodChoice = Console.ReadLine();
            IQueryable<Product> query = db.Products.OrderBy(p => p.ProductName);
            if (prodChoice == "2")
            {
                query = query.Where(p => p.Discontinued);
                logger.Info("Displaying discontinued products");
            }
            else if (prodChoice == "3")
            {
                query = query.Where(p => !p.Discontinued);
                logger.Info("Displaying active products");
            }
            else
            {
                logger.Info("Displaying all products");
            }
            var products = query.ToList();
            logger.Info($"{products.Count} products returned");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{products.Count} records returned");
            Console.ForegroundColor = ConsoleColor.White;
            foreach (var p in products)
            {
                if (p.Discontinued)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{p.ProductName} (Discontinued)");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.WriteLine(p.ProductName);
                }
            }
        }

        public static void DisplayProductDetails(Logger logger)
        {
            var db = new DataContext();
            var products = db.Products.OrderBy(p => p.ProductId).ToList();
            if (products.Count == 0)
            {
                logger.Info("No products found to display");
                Console.WriteLine("No products found.");
            }
            else
            {
                Console.WriteLine("Select a product to view details:");
                foreach (var p in products)
                {
                    Console.WriteLine($"{p.ProductId}) {p.ProductName}");
                }
                if (
                    int.TryParse(Console.ReadLine(), out int productId)
                    && products.Any(p => p.ProductId == productId)
                )
                {
                    var product = products.First(p => p.ProductId == productId);
                    logger.Info(
                        $"Viewing product details: {product.ProductName} (ID: {product.ProductId})"
                    );
                    Console.WriteLine($"ProductId: {product.ProductId}");
                    Console.WriteLine($"ProductName: {product.ProductName}");
                    Console.WriteLine($"SupplierId: {product.SupplierId}");
                    Console.WriteLine($"CategoryId: {product.CategoryId}");
                    Console.WriteLine($"QuantityPerUnit: {product.QuantityPerUnit}");
                    Console.WriteLine($"UnitPrice: {product.UnitPrice}");
                    Console.WriteLine($"UnitsInStock: {product.UnitsInStock}");
                    Console.WriteLine($"UnitsOnOrder: {product.UnitsOnOrder}");
                    Console.WriteLine($"ReorderLevel: {product.ReorderLevel}");
                    Console.WriteLine($"Discontinued: {(product.Discontinued ? "Yes" : "No")}");
                }
                else
                {
                    logger.Error("Invalid product selection");
                }
            }
        }

        public static void DeleteProduct(Logger logger)
        {
            var db = new DataContext();
            var products = db.Products.OrderBy(p => p.ProductId).ToList();
            if (products.Count == 0)
            {
                logger.Info("No products found to delete");
                Console.WriteLine("No products found.");
                return;
            }
            Console.WriteLine("Select the Product to delete:");
            foreach (var p in products)
            {
                Console.WriteLine($"{p.ProductId}) {p.ProductName}");
            }
            if (
                !int.TryParse(Console.ReadLine(), out int productId)
                || !products.Any(p => p.ProductId == productId)
            )
            {
                logger.Error("Invalid product selection");
                return;
            }
            var product = db
                .Products.Include(p => p.OrderDetails)
                .First(p => p.ProductId == productId);
            var orderDetails = db.OrderDetails.Where(od => od.ProductId == productId).ToList();
            foreach (var od in orderDetails)
            {
                db.OrderDetails.Remove(od);
            }
            db.Products.Remove(product);
            try
            {
                db.SaveChanges();
                Console.WriteLine(
                    $"Product '{product.ProductName}' and its related order details deleted successfully."
                );
            }
            catch (Exception ex)
            {
                logger.Error($"Error deleting product: {ex.Message}");
            }
        }
    }
}
