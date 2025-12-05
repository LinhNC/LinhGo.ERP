using AutoMapper;
using LinhGo.ERP.Application.DTOs.Companies;
using LinhGo.ERP.Application.DTOs.Users;
using LinhGo.ERP.Application.DTOs.Customers;
using LinhGo.ERP.Application.DTOs.Products;
using LinhGo.ERP.Application.DTOs.Inventory;
using LinhGo.ERP.Application.DTOs.Orders;
using LinhGo.ERP.Domain.Companies.Entities;
using LinhGo.ERP.Domain.Users.Entities;
using LinhGo.ERP.Domain.Customers.Entities;
using LinhGo.ERP.Domain.Inventory.Entities;
using LinhGo.ERP.Domain.Orders.Entities;

namespace LinhGo.ERP.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Company, CompanyDto>();
        CreateMap<CreateCompanyDto, Company>().ForMember(d => d.Id, opt => opt.Ignore());
        CreateMap<UpdateCompanyDto, Company>().ForMember(d => d.Code, opt => opt.Ignore());

        CreateMap<User, UserDto>();
        CreateMap<CreateUserDto, User>().ForMember(d => d.Id, opt => opt.Ignore()).ForMember(d => d.PasswordHash, opt => opt.Ignore());
        CreateMap<UpdateUserDto, User>();
        CreateMap<UserCompany, UserCompanyDto>().ForMember(d => d.CompanyName, opt => opt.MapFrom(s => s.Company!.Name)).ForMember(d => d.CompanyCode, opt => opt.MapFrom(s => s.Company!.Code));

        CreateMap<Customer, CustomerDto>().ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString())).ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
        CreateMap<Customer, CustomerDetailsDto>().IncludeBase<Customer, CustomerDto>();
        CreateMap<CustomerContact, CustomerContactDto>();
        CreateMap<CustomerAddress, CustomerAddressDto>();
        CreateMap<CreateCustomerDto, Customer>().ForMember(d => d.Id, opt => opt.Ignore()).ForMember(d => d.CompanyId, opt => opt.Ignore());
        CreateMap<UpdateCustomerDto, Customer>().ForMember(d => d.CompanyId, opt => opt.Ignore()).ForMember(d => d.Code, opt => opt.Ignore());

        CreateMap<Product, ProductDto>().ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString())).ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : null));
        CreateMap<Product, ProductDetailsDto>().IncludeBase<Product, ProductDto>();
        CreateMap<ProductVariant, ProductVariantDto>();
        CreateMap<Stock, ProductStockDto>().ForMember(d => d.WarehouseName, opt => opt.MapFrom(s => s.Warehouse!.Name));
        CreateMap<CreateProductDto, Product>().ForMember(d => d.Id, opt => opt.Ignore()).ForMember(d => d.CompanyId, opt => opt.Ignore());
        CreateMap<UpdateProductDto, Product>().ForMember(d => d.CompanyId, opt => opt.Ignore()).ForMember(d => d.Code, opt => opt.Ignore());

        CreateMap<Warehouse, WarehouseDto>();
        CreateMap<CreateWarehouseDto, Warehouse>().ForMember(d => d.Id, opt => opt.Ignore()).ForMember(d => d.CompanyId, opt => opt.Ignore());

        CreateMap<InventoryTransaction, InventoryTransactionDto>().ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString())).ForMember(d => d.ProductCode, opt => opt.MapFrom(s => s.Product!.Code)).ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product!.Name)).ForMember(d => d.FromWarehouse, opt => opt.MapFrom(s => s.FromWarehouse != null ? s.FromWarehouse.Name : null)).ForMember(d => d.ToWarehouse, opt => opt.MapFrom(s => s.ToWarehouse != null ? s.ToWarehouse.Name : null));

        CreateMap<Order, OrderDto>().ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString())).ForMember(d => d.PaymentStatus, opt => opt.MapFrom(s => s.PaymentStatus.ToString())).ForMember(d => d.FulfillmentStatus, opt => opt.MapFrom(s => s.FulfillmentStatus.ToString()));
        CreateMap<Order, OrderDetailsDto>().IncludeBase<Order, OrderDto>();
        CreateMap<OrderItem, OrderItemDto>().ForMember(d => d.FulfillmentStatus, opt => opt.MapFrom(s => s.FulfillmentStatus.ToString()));
        CreateMap<OrderPayment, OrderPaymentDto>().ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
        CreateMap<OrderShipment, OrderShipmentDto>().ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
        CreateMap<CreateOrderDto, Order>().ForMember(d => d.Id, opt => opt.Ignore()).ForMember(d => d.CompanyId, opt => opt.Ignore()).ForMember(d => d.OrderNumber, opt => opt.Ignore()).ForMember(d => d.Items, opt => opt.Ignore());
    }
}

