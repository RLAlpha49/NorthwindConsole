using NLog;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using NorthwindConsole.Model;
using System.ComponentModel.DataAnnotations;
string path = Directory.GetCurrentDirectory() + "//nlog.config";

// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();

logger.Info("Program started");

do
{
  Console.WriteLine("1) Display categories");
  Console.WriteLine("2) Add category");
  Console.WriteLine("3) Display Category and related products");
  Console.WriteLine("4) Display all Categories and their related products");
  Console.WriteLine("5) Add product");
  Console.WriteLine("6) Edit product");
  Console.WriteLine("Enter to quit");
  string? choice = Console.ReadLine();
  Console.Clear();
  logger.Info("Option {choice} selected", choice);

  if (choice == "1")
  {
    // display categories
    var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json");

    var config = configuration.Build();

    var db = new DataContext();
    var query = db.Categories.OrderBy(p => p.CategoryName);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"{query.Count()} records returned");
    Console.ForegroundColor = ConsoleColor.Magenta;
    foreach (var item in query)
    {
      Console.WriteLine($"{item.CategoryName} - {item.Description}");
    }
    Console.ForegroundColor = ConsoleColor.White;
  }
  else if (choice == "2")
  {
    // Add category
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
      // check for unique name
      if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
      {
        // generate validation error
        isValid = false;
        results.Add(new ValidationResult("Name exists", ["CategoryName"]));
      }
      else
      {
        logger.Info("Validation passed");
        // TODO: save category to db
      }
    }
    if (!isValid)
    {
      foreach (var result in results)
      {
        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
      }
    }
  }
  else if (choice == "3")
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
    int id = int.Parse(Console.ReadLine()!);
    Console.Clear();
    logger.Info($"CategoryId {id} selected");
    Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id)!;
    Console.WriteLine($"{category.CategoryName} - {category.Description}");
    foreach (Product p in category.Products)
    {
      Console.WriteLine($"\t{p.ProductName}");
    }
  }
  else if (choice == "4")
  {
    var db = new DataContext();
    var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
    foreach (var item in query)
    {
      Console.WriteLine($"{item.CategoryName}");
      foreach (Product p in item.Products)
      {
        Console.WriteLine($"\t{p.ProductName}");
      }
    }
  }
  else if (choice == "5")
  {
    // Add product
    Product product = new();
    Console.WriteLine("Enter Product Name:");
    product.ProductName = Console.ReadLine()!;

    // Select Supplier
    var db = new DataContext();
    var suppliers = db.Suppliers.OrderBy(s => s.CompanyName).ToList();
    Console.WriteLine("Select Supplier:");
    foreach (var s in suppliers)
    {
      Console.WriteLine($"{s.SupplierId}) {s.CompanyName}");
    }
    if (int.TryParse(Console.ReadLine(), out int supplierId) && suppliers.Any(s => s.SupplierId == supplierId))
    {
      product.SupplierId = supplierId;
    }
    else
    {
      logger.Error("Invalid supplier selection");
      return;
    }

    // Select Category
    var categories = db.Categories.OrderBy(c => c.CategoryName).ToList();
    Console.WriteLine("Select Category:");
    foreach (var c in categories)
    {
      Console.WriteLine($"{c.CategoryId}) {c.CategoryName}");
    }
    if (int.TryParse(Console.ReadLine(), out int categoryId) && categories.Any(c => c.CategoryId == categoryId))
    {
      product.CategoryId = categoryId;
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
        logger.Error($"{result.MemberNames.FirstOrDefault() ?? ""} : {result.ErrorMessage}");
      }
    }
  }
  else if (choice == "6")
  {
    var db = new DataContext();
    var products = db.Products.OrderBy(p => p.ProductId).ToList();
    Console.WriteLine("Select the Product to edit:");
    foreach (var p in products)
    {
      Console.WriteLine($"{p.ProductId}) {p.ProductName}");
    }
    if (!int.TryParse(Console.ReadLine(), out int productId) || !products.Any(p => p.ProductId == productId))
    {
      logger.Error("Invalid product selection");
      return;
    }
    var product = db.Products.First(p => p.ProductId == productId);
    Console.WriteLine($"Editing Product: {product.ProductName}");

    Console.WriteLine($"Enter Product Name ({product.ProductName}):");
    string? input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input)) product.ProductName = input;

    // Edit Supplier
    var suppliers = db.Suppliers.OrderBy(s => s.SupplierId).ToList();
    Console.WriteLine($"Select Supplier ({product.SupplierId}):");
    foreach (var s in suppliers)
    {
      Console.WriteLine($"{s.SupplierId}) {s.CompanyName}");
    }
    input = Console.ReadLine();
    if (int.TryParse(input, out int supplierId) && suppliers.Any(s => s.SupplierId == supplierId))
      product.SupplierId = supplierId;

    // Edit Category
    var categories = db.Categories.OrderBy(c => c.CategoryName).ToList();
    Console.WriteLine($"Select Category ({product.CategoryId}):");
    foreach (var c in categories)
    {
      Console.WriteLine($"{c.CategoryId}) {c.CategoryName}");
    }
    input = Console.ReadLine();
    if (int.TryParse(input, out int categoryId) && categories.Any(c => c.CategoryId == categoryId))
      product.CategoryId = categoryId;

    Console.WriteLine($"Enter Quantity Per Unit ({product.QuantityPerUnit}):");
    input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input)) product.QuantityPerUnit = input;

    Console.WriteLine($"Enter Unit Price ({product.UnitPrice}):");
    input = Console.ReadLine();
    if (decimal.TryParse(input, out decimal unitPrice)) product.UnitPrice = unitPrice;

    Console.WriteLine($"Enter Units In Stock ({product.UnitsInStock}):");
    input = Console.ReadLine();
    if (short.TryParse(input, out short unitsInStock)) product.UnitsInStock = unitsInStock;

    Console.WriteLine($"Enter Units On Order ({product.UnitsOnOrder}):");
    input = Console.ReadLine();
    if (short.TryParse(input, out short unitsOnOrder)) product.UnitsOnOrder = unitsOnOrder;

    Console.WriteLine($"Enter Reorder Level ({product.ReorderLevel}):");
    input = Console.ReadLine();
    if (short.TryParse(input, out short reorderLevel)) product.ReorderLevel = reorderLevel;

    Console.WriteLine($"Is Discontinued? (y/n, current: {(product.Discontinued ? "y" : "n")}):");
    input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input)) product.Discontinued = input.Equals("y", StringComparison.CurrentCultureIgnoreCase);

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
        logger.Error($"{result.MemberNames.FirstOrDefault() ?? ""} : {result.ErrorMessage}");
      }
    }
  }
  else if (string.IsNullOrEmpty(choice))
  {
    break;
  }
  Console.WriteLine();
} while (true);

logger.Info("Program ended");
