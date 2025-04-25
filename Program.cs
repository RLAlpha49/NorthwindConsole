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
  Console.WriteLine("7) Display products");
  Console.WriteLine("8) Display a specific product");
  Console.WriteLine("9) Edit category");
  Console.WriteLine("10) Display all Categories and their active products");
  Console.WriteLine("11) Display a specific Category and its active products");
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
        logger.Error($"Add category failed: Name '{category.CategoryName}' already exists");
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
          logger.Error($"Error adding category: {ex.Message}");
        }
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
    if (!int.TryParse(Console.ReadLine(), out int id) || !query.Any(c => c.CategoryId == id))
    {
      logger.Error("Invalid category selection");
      return;
    }
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
      logger.Info($"Supplier selected: {supplierId}");
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
  else if (choice == "7")
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
  else if (choice == "8")
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
      if (int.TryParse(Console.ReadLine(), out int productId) && products.Any(p => p.ProductId == productId))
      {
        var product = products.First(p => p.ProductId == productId);
        logger.Info($"Viewing product details: {product.ProductName} (ID: {product.ProductId})");
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
        Console.WriteLine("Invalid product selection.");
      }
    }
  }
  else if (choice == "9")
  {
    // Edit category
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
    if (!int.TryParse(Console.ReadLine(), out int categoryId) || !categories.Any(c => c.CategoryId == categoryId))
    {
      logger.Error("Invalid category selection");
      Console.WriteLine("Invalid category selection.");
      return;
    }
    var category = db.Categories.First(c => c.CategoryId == categoryId);
    logger.Info($"Editing Category: {category.CategoryName} (ID: {category.CategoryId})");
    Console.WriteLine($"Editing Category: {category.CategoryName}");

    Console.WriteLine($"Enter Category Name ({category.CategoryName}):");
    string? input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input)) category.CategoryName = input;

    Console.WriteLine($"Enter Description ({category.Description}):");
    input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input)) category.Description = input;

    // Validate
    ValidationContext context = new(category, null, null);
    List<ValidationResult> results = [];
    var isValid = Validator.TryValidateObject(category, context, results, true);
    // Check for unique name (excluding self)
    if (db.Categories.Any(c => c.CategoryId != category.CategoryId && c.CategoryName == category.CategoryName))
    {
      isValid = false;
      results.Add(new ValidationResult("Name exists", ["CategoryName"]));
      logger.Error($"Edit category failed: Name '{category.CategoryName}' already exists");
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
        Console.WriteLine($"Error updating category: {ex.Message}");
      }
    }
    else
    {
      foreach (var result in results)
      {
        logger.Error($"{result.MemberNames.FirstOrDefault() ?? ""} : {result.ErrorMessage}");
        Console.WriteLine($"{result.MemberNames.FirstOrDefault() ?? ""} : {result.ErrorMessage}");
      }
    }
  }
  else if (choice == "10")
  {
    // Display all categories and their related active products
    var db = new DataContext();
    var query = db.Categories.Include("Products").OrderBy(c => c.CategoryId);
    int totalCategories = 0, totalProducts = 0;
    foreach (var category in query)
    {
      var activeProducts = category.Products.Where(p => !p.Discontinued).OrderBy(p => p.ProductName).ToList();
      Console.WriteLine($"{category.CategoryName}");
      foreach (var product in activeProducts)
      {
        Console.WriteLine($"\t{product.ProductName}");
        totalProducts++;
      }
      totalCategories++;
    }
  }
  else if (choice == "11")
  {
    // Display a specific category and its related active products
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
    if (!int.TryParse(Console.ReadLine(), out int categoryId) || !categories.Any(c => c.CategoryId == categoryId))
    {
      Console.WriteLine("Invalid category selection.");
      return;
    }
    var category = db.Categories.Include("Products").First(c => c.CategoryId == categoryId);
    var activeProducts = category.Products.Where(p => !p.Discontinued).OrderBy(p => p.ProductName).ToList();
    Console.WriteLine($"{category.CategoryName}");
    foreach (var product in activeProducts)
    {
      Console.WriteLine($"\t{product.ProductName}");
    }
  }
  else if (string.IsNullOrEmpty(choice))
  {
    break;
  }
  Console.WriteLine();
} while (true);

logger.Info("Program ended");
