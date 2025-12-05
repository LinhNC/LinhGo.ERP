using LinhGo.ERP.Web.Core.Interfaces;
using LinhGo.ERP.Web.Core.Models;

namespace LinhGo.ERP.Web.Core.Services;

public class SystemMenuService : ISystemMenuService
{
    public async Task<IEnumerable<PanelMenu>> GetAllPanelMenus()
    {
        // Currently hardcoded menus; in a real application, these might be fetched from a database or configuration file
        return
        [
            new PanelMenu
            {
                Name = "dashboard",
                Icon = "",
                Path = "/dashboard",
                Title = "Dashboard",
                Description = "Overall metrics and business insights",
                Children = new[]
                {
                    new PanelMenu
                    {
                        Name = "overview",
                        Icon = "",
                        Path = "/dashboard/overview",
                        Title = "Overview",
                        Description = "Summary KPIs and performance"
                    }
                }
            },

            // INVENTORY
            new PanelMenu
            {
                Name = "inventory",
                Icon = "\ue9ce",
                Title = "Inventory",
                Description = "Manage products, stock levels, and warehouses",
                Children =
                [
                    new PanelMenu
                    {
                        Name = "products",
                        Icon = "",
                        Path = "/inventory/products",
                        Title = "Products",
                        Description = "Product catalog and item details"
                    },
                    new PanelMenu
                    {
                        Name = "categories",
                        Icon = "",
                        Path = "/inventory/categories",
                        Title = "Categories",
                        Description = "Product categories and grouping"
                    },
                    new PanelMenu
                    {
                        Name = "stock",
                        Icon = "",
                        Path = "/inventory/stock",
                        Title = "Stock Levels",
                        Description = "Real-time stock levels and availability"
                    },
                    new PanelMenu
                    {
                        Name = "adjustments",
                        Icon = "",
                        Path = "/inventory/adjustments",
                        Title = "Adjustments",
                        Description = "Manual stock adjustments"
                    },
                    new PanelMenu
                    {
                        Name = "warehouses",
                        Icon = "",
                        Path = "/inventory/warehouses",
                        Title = "Warehouses",
                        Description = "Warehouse and location setup"
                    }
                ]
            },

            // SALES
            new PanelMenu
            {
                Name = "sales",
                Icon = "",
                Title = "Sales",
                Description = "Manage sales orders, quotations, deliveries",
                Children =
                [
                    new PanelMenu
                    {
                        Name = "customers",
                        Icon = "",
                        Path = "/sales/customers",
                        Title = "Customers",
                        Description = "Customer list and profiles"
                    },
                    new PanelMenu
                    {
                        Name = "quotes",
                        Icon = "",
                        Path = "/sales/quotes",
                        Title = "Quotations",
                        Description = "Sales quotations to customers"
                    },
                    new PanelMenu
                    {
                        Name = "orders",
                        Icon = "",
                        Path = "/sales/orders",
                        Title = "Sales Orders",
                        Description = "Customer orders management"
                    },
                    new PanelMenu
                    {
                        Name = "deliveries",
                        Icon = "",
                        Path = "/sales/deliveries",
                        Title = "Delivery Orders",
                        Description = "Product delivery management"
                    }
                ]
            },

            // PURCHASES
            new PanelMenu
            {
                Name = "purchases",
                Icon = "",
                Title = "Purchases",
                Description = "Manage suppliers and purchase orders",
                Children =
                [
                    new PanelMenu
                    {
                        Name = "suppliers",
                        Icon = "",
                        Path = "/purchases/suppliers",
                        Title = "Suppliers",
                        Description = "Supplier list and profiles"
                    },
                    new PanelMenu
                    {
                        Name = "purchase-orders",
                        Icon = "",
                        Path = "/purchases/orders",
                        Title = "Purchase Orders",
                        Description = "Manage your procurement orders"
                    },
                    new PanelMenu
                    {
                        Name = "receiving",
                        Icon = "",
                        Path = "/purchases/receiving",
                        Title = "Receiving",
                        Description = "Receive products from suppliers"
                    }
                ]
            },

            // REPORTS
            new PanelMenu
            {
                Name = "reports",
                Icon = "",
                Title = "Reports",
                Description = "Analytics and business insights",
                Children =
                [
                    new PanelMenu
                    {
                        Name = "sales-reports",
                        Icon = "",
                        Path = "/reports/sales",
                        Title = "Sales Reports",
                        Description = "Sales analytics and summaries"
                    },
                    new PanelMenu
                    {
                        Name = "inventory-reports",
                        Icon = "",
                        Path = "/reports/inventory",
                        Title = "Inventory Reports",
                        Description = "Stock movement and valuation"
                    },
                    new PanelMenu
                    {
                        Name = "purchase-reports",
                        Icon = "",
                        Path = "/reports/purchases",
                        Title = "Purchase Reports",
                        Description = "Procurement analytics"
                    }
                ]
            },

            // SETTINGS
            new PanelMenu
            {
                Name = "settings",
                Icon = "",
                Title = "Settings",
                Description = "System configurations and user management",
                Children =
                [
                    new PanelMenu
                    {
                        Name = "users",
                        Icon = "",
                        Path = "/settings/users",
                        Title = "Users & Permissions",
                        Description = "Manage user accounts and access rights"
                    },
                    new PanelMenu
                    {
                        Name = "integrations",
                        Icon = "",
                        Path = "/settings/integrations",
                        Title = "Integrations",
                        Description = "API keys, webhooks, and connected apps"
                    },
                    new PanelMenu
                    {
                        Name = "system",
                        Icon = "",
                        Path = "/settings/system",
                        Title = "System Settings",
                        Description = "Localization, currency, tax, etc."
                    }
                ]
            }
        ];
    }

    public async Task<IEnumerable<PanelMenu>> FilterPanelMenus(string term)
    {
        var allMenus = await GetAllPanelMenus();
        if (string.IsNullOrEmpty(term))
            return allMenus;

        bool Contains(string? value)
        {
            return value != null && value.Contains(term, StringComparison.OrdinalIgnoreCase);
        }

        bool Filter(PanelMenu menu)
        {
            return Contains(menu.Name) || (menu.Tags != null && menu.Tags.Any(Contains));
        }

        bool DeepFilter(PanelMenu menu)
        {
            return Filter(menu) || menu.Children?.Any(Filter) == true;
        }

        return allMenus.Where(category => category.Children?.Any(DeepFilter) == true || Filter(category))
            .Select(category => new PanelMenu
            {
                Name = category.Name,
                Path = category.Path,
                Icon = category.Icon,
                Expanded = true,
                Title = category.Title,
                Description = category.Description,
                Children = category.Children?.Where(DeepFilter)
                    .Select(m => new PanelMenu
                        {
                            Name = m.Name,
                            Path = m.Path,
                            Icon = m.Icon,
                            Expanded = true,
                            Children = m.Children,
                            Title = m.Title,
                            Description = m.Description
                        }
                    )
                    .ToArray()
            }).ToList();
    }

    public async Task<PanelMenu?> GetCurrentPanelMenu(Uri uri)
    {
        var allMenus = await GetAllPanelMenus();
        return Flatten(allMenus)
            .FirstOrDefault(menu => menu.Path == uri.AbsolutePath || $"/{menu.Path}" == uri.AbsolutePath);

        IEnumerable<PanelMenu> Flatten(IEnumerable<PanelMenu> e)
        {
            return e.SelectMany(c => c.Children != null ? Flatten(c.Children) : new[] { c });
        }
    }
}