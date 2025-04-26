using Microsoft.EntityFrameworkCore;
using NLog;

namespace NorthwindConsole
{
    public static class StatisticsActions
    {
        public static void ShowStatisticsMenu(Logger logger)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("========================================");
                Console.WriteLine("           Statistics Menu");
                Console.WriteLine("========================================");
                Console.WriteLine(" 0) Show all statistics");
                Console.WriteLine(" 1) Product Statistics");
                Console.WriteLine(" 2) Category Statistics");
                Console.WriteLine(" 3) Order Statistics");
                Console.WriteLine(" 4) Customer Statistics");
                Console.WriteLine(" 5) Employee Statistics");
                Console.WriteLine(" 6) Supplier Statistics");
                Console.WriteLine(" [Enter] to return to main menu");
                Console.WriteLine("----------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;
                string? choice = Console.ReadLine();
                Console.Clear();
                switch (choice)
                {
                    case "1":
                        ShowProductStatistics(logger);
                        break;
                    case "2":
                        ShowCategoryStatistics(logger);
                        break;
                    case "3":
                        ShowOrderStatistics(logger);
                        break;
                    case "4":
                        ShowCustomerStatistics(logger);
                        break;
                    case "5":
                        ShowEmployeeStatistics(logger);
                        break;
                    case "6":
                        ShowSupplierStatistics(logger);
                        break;
                    case "0":
                        ShowAllStatistics(logger);
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

        public static void ShowProductStatistics(Logger logger)
        {
            using var db = new Model.DataContext();
            var totalProducts = db.Products.Count();
            var productsPerCategory = db
                .Categories.Select(c => new { c.CategoryName, ProductCount = c.Products.Count })
                .ToList();
            var mostExpensive = db.Products.OrderByDescending(p => p.UnitPrice).FirstOrDefault();
            var leastExpensive = db.Products.OrderBy(p => p.UnitPrice).FirstOrDefault();
            var avgPrice = db.Products.Average(p => p.UnitPrice);
            var discontinuedCount = db.Products.Count(p => p.Discontinued);

            Console.WriteLine("[Product Statistics]");
            Console.WriteLine($"Total products: {totalProducts}");
            Console.WriteLine($"Average price: {avgPrice:C}");
            Console.WriteLine($"Discontinued products: {discontinuedCount}");
            if (mostExpensive != null)
                Console.WriteLine(
                    $"Most expensive: {mostExpensive.ProductName} ({mostExpensive.UnitPrice:C})"
                );
            if (leastExpensive != null)
                Console.WriteLine(
                    $"Least expensive: {leastExpensive.ProductName} ({leastExpensive.UnitPrice:C})"
                );
            Console.WriteLine("Products per category:");
            foreach (var c in productsPerCategory)
                Console.WriteLine($"  {c.CategoryName}: {c.ProductCount}");
            // Bar chart
            var barData = productsPerCategory.ToDictionary(
                c => c.CategoryName,
                c => c.ProductCount
            );
            Console.WriteLine("\n[Bar Chart] Products per Category:");
            ConsoleCharts.PrintBarChart(barData);
            // Histogram for product prices
            var prices = db.Products.Select(p => (int)(p.UnitPrice ?? 0)).ToList();
            if (prices.Count > 0)
            {
                Console.WriteLine("\n[Histogram] Product Price Distribution:");
                ConsoleCharts.PrintHistogram(prices, binCount: 10);
            }
        }

        public static void ShowCategoryStatistics(Logger logger)
        {
            using var db = new Model.DataContext();
            var totalCategories = db.Categories.Count();
            var productsPerCategory = db
                .Categories.Select(c => new { c.CategoryName, ProductCount = c.Products.Count })
                .ToList();
            var max = productsPerCategory.Max(c => c.ProductCount);
            var min = productsPerCategory.Min(c => c.ProductCount);
            var most = productsPerCategory.Where(c => c.ProductCount == max).ToList();
            var fewest = productsPerCategory.Where(c => c.ProductCount == min).ToList();

            Console.WriteLine("[Category Statistics]");
            Console.WriteLine($"Total categories: {totalCategories}");
            Console.WriteLine($"Category with most products ({max}):");
            foreach (var c in most)
                Console.WriteLine($"  {c.CategoryName}");
            Console.WriteLine($"Category with fewest products ({min}):");
            foreach (var c in fewest)
                Console.WriteLine($"  {c.CategoryName}");
        }

        public static void ShowOrderStatistics(Logger logger)
        {
            using var db = new Model.DataContext();
            var totalOrders = db.Orders.Count();
            var orderValues = db
                .Orders.Select(o => o.OrderDetails.Sum(od => od.UnitPrice * od.Quantity))
                .ToList();
            var avgOrderValue = orderValues.Count > 0 ? orderValues.Average() : 0;
            var maxOrderValue = orderValues.Count > 0 ? orderValues.Max() : 0;
            var minOrderValue = orderValues.Count > 0 ? orderValues.Min() : 0;

            Console.WriteLine("[Order Statistics]");
            Console.WriteLine($"Total orders: {totalOrders}");
            Console.WriteLine($"Average order value: {avgOrderValue:C}");
            Console.WriteLine($"Largest order value: {maxOrderValue:C}");
            Console.WriteLine($"Smallest order value: {minOrderValue:C}");
            // Histogram for order values
            if (orderValues.Count > 0)
            {
                Console.WriteLine("\n[Histogram] Order Value Distribution:");
                ConsoleCharts.PrintHistogram(orderValues.Select(v => (int)v), binCount: 10);
            }
            // Bar chart for orders per month
            var ordersPerMonth = db
                .Orders.Where(o => o.OrderDate != null)
                .GroupBy(o => o.OrderDate!.Value.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .OrderBy(g => g.Month)
                .ToDictionary(
                    g =>
                        System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(
                            g.Month
                        ),
                    g => g.Count
                );
            if (ordersPerMonth.Count > 0)
            {
                Console.WriteLine("\n[Bar Chart] Orders per Month:");
                ConsoleCharts.PrintBarChart(ordersPerMonth);
            }
        }

        public static void ShowCustomerStatistics(Logger logger)
        {
            using var db = new Model.DataContext();
            var totalCustomers = db.Customers.Count();
            var topCustomers = db
                .Customers.Select(c => new { c.CompanyName, OrderCount = c.Orders.Count })
                .OrderByDescending(c => c.OrderCount)
                .Take(5)
                .ToList();
            var customersByCountry = db
                .Customers.GroupBy(c => c.Country)
                .Select(g => new { Country = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToList();

            Console.WriteLine("[Customer Statistics]");
            Console.WriteLine($"Total customers: {totalCustomers}");
            Console.WriteLine("Top 5 customers by order count:");
            foreach (var c in topCustomers)
                Console.WriteLine($"  {c.CompanyName}: {c.OrderCount} orders");
            Console.WriteLine("Customers by country:");
            foreach (var c in customersByCountry)
                Console.WriteLine($"  {c.Country}: {c.Count}");
            // Bar chart
            var barData = customersByCountry.ToDictionary(
                c => c.Country ?? "Unknown",
                c => c.Count
            );
            Console.WriteLine("\n[Bar Chart] Customers by Country:");
            ConsoleCharts.PrintBarChart(barData);
            // Bar chart for orders per customer
            var ordersPerCustomer = db
                .Customers.Select(c => new { c.CompanyName, OrderCount = c.Orders.Count })
                .Where(c => c.OrderCount > 0)
                .OrderByDescending(c => c.OrderCount)
                .ToDictionary(c => c.CompanyName, c => c.OrderCount);
            if (ordersPerCustomer.Count > 0)
            {
                Console.WriteLine("\n[Bar Chart] Orders per Customer:");
                ConsoleCharts.PrintBarChart(ordersPerCustomer);
            }
        }

        public static void ShowEmployeeStatistics(Logger logger)
        {
            using var db = new Model.DataContext();
            var totalEmployees = db.Employees.Count();
            var mostOrders = db
                .Employees.Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    OrderCount = e.Orders.Count,
                })
                .OrderByDescending(e => e.OrderCount)
                .FirstOrDefault();
            var now = DateTime.Now;
            var tenures = db
                .Employees.Select(e => EF.Property<DateTime>(e, "HireDate"))
                .ToList()
                .Where(d => d != default)
                .Select(d => (now - d).TotalDays / 365.25)
                .ToList();
            var avgTenure = tenures.Count > 0 ? tenures.Average() : 0;

            Console.WriteLine("[Employee Statistics]");
            Console.WriteLine($"Total employees: {totalEmployees}");
            if (mostOrders != null)
                Console.WriteLine(
                    $"Employee with most orders: {mostOrders.FirstName} {mostOrders.LastName} ({mostOrders.OrderCount} orders)"
                );
            Console.WriteLine($"Average tenure: {avgTenure:F1} years");
            // Histogram for employee tenure
            if (tenures.Count > 0)
            {
                Console.WriteLine("\n[Histogram] Employee Tenure Distribution (years):");
                ConsoleCharts.PrintHistogram(tenures.Select(t => (int)t), binCount: 10);
            }
        }

        public static void ShowSupplierStatistics(Logger logger)
        {
            using var db = new Model.DataContext();
            var totalSuppliers = db.Suppliers.Count();
            var productsPerSupplier = db
                .Suppliers.Select(s => new { s.CompanyName, ProductCount = s.Products.Count })
                .OrderByDescending(s => s.ProductCount)
                .ToList();

            Console.WriteLine("[Supplier Statistics]");
            Console.WriteLine($"Total suppliers: {totalSuppliers}");
            Console.WriteLine("Products per supplier:");
            foreach (var s in productsPerSupplier)
                Console.WriteLine($"  {s.CompanyName}: {s.ProductCount}");
            // Bar chart
            var barDataSupp = productsPerSupplier.ToDictionary(
                s => s.CompanyName,
                s => s.ProductCount
            );
            Console.WriteLine("\n[Bar Chart] Products per Supplier:");
            ConsoleCharts.PrintBarChart(barDataSupp);
        }

        public static void ShowAllStatistics(Logger logger)
        {
            ShowProductStatistics(logger);
            Console.WriteLine();
            ShowCategoryStatistics(logger);
            Console.WriteLine();
            ShowOrderStatistics(logger);
            Console.WriteLine();
            ShowCustomerStatistics(logger);
            Console.WriteLine();
            ShowEmployeeStatistics(logger);
            Console.WriteLine();
            ShowSupplierStatistics(logger);
        }
    }
}
