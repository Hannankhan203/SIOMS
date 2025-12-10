using SIOMS.Models;

namespace SIOMS.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            // Seed Categories
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Electronics", Description = "Electronic devices and components" },
                    new Category { Name = "Clothing", Description = "Apparel and accessories" },
                    new Category { Name = "Home & Garden", Description = "Home improvement and garden supplies" },
                    new Category { Name = "Books", Description = "Books and publications" },
                    new Category { Name = "Sports", Description = "Sports equipment and gear" }
                };
                
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }
            
            // Seed Suppliers
            if (!context.Suppliers.Any())
            {
                var suppliers = new List<Supplier>
                {
                    new Supplier { 
                        CompanyName = "Tech Supplies Inc.", 
                        ContactPerson = "John Smith",
                        Phone = "555-0101",
                        Email = "john@techsupplies.com",
                        Address = "123 Tech Street, City"
                    },
                    new Supplier { 
                        CompanyName = "Fashion Source Ltd.", 
                        ContactPerson = "Sarah Johnson",
                        Phone = "555-0102",
                        Email = "sarah@fashionsource.com",
                        Address = "456 Fashion Ave, City"
                    }
                };
                
                context.Suppliers.AddRange(suppliers);
                await context.SaveChangesAsync();
            }
            
            // Seed Products
            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    new Product { 
                        Name = "Wireless Mouse",
                        Description = "Ergonomic wireless mouse with USB receiver",
                        SKU = "ELEC-001",
                        CategoryId = 1,
                        SupplierId = 1,
                        BuyingPrice = 15.99m,
                        SellingPrice = 29.99m,
                        StockQuantity = 50,
                        MinimumStockLevel = 10
                    },
                    new Product { 
                        Name = "T-Shirt (Medium)",
                        Description = "Cotton t-shirt, various colors",
                        SKU = "CLOTH-001",
                        CategoryId = 2,
                        SupplierId = 2,
                        BuyingPrice = 8.50m,
                        SellingPrice = 19.99m,
                        StockQuantity = 100,
                        MinimumStockLevel = 20
                    }
                };
                
                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }
        }
    }
}