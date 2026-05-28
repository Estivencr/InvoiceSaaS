using AutoMapper;
using InvoiceSaaS.Application.DTOs.Customer;
using InvoiceSaaS.Application.DTOs.Employee;
using InvoiceSaaS.Application.DTOs.Invoice;
using InvoiceSaaS.Application.DTOs.Product;
using InvoiceSaaS.Application.DTOs.User;
using InvoiceSaaS.Domain.Entities;

namespace InvoiceSaaS.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Customer
        CreateMap<Customer, CustomerResponse>();
        CreateMap<CreateCustomerRequest, Customer>();
        CreateMap<UpdateCustomerRequest, Customer>();

        // Employee
        CreateMap<Employee, EmployeeResponse>()
            .ForMember(d => d.RoleName, o => o.MapFrom(s => s.Role != null ? s.Role.Name : string.Empty));
        CreateMap<CreateEmployeeRequest, Employee>();
        CreateMap<UpdateEmployeeRequest, Employee>();

        // Invoice
        CreateMap<Invoice, InvoiceResponse>()
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null ? s.Customer.Name : string.Empty))
            .ForMember(d => d.CustomerDocument, o => o.MapFrom(s => s.Customer != null ? s.Customer.Document : string.Empty))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s =>
                s.CreatedBy != null ? $"{s.CreatedBy.FirstName} {s.CreatedBy.LastName}".Trim() : string.Empty));

        CreateMap<InvoiceDetail, InvoiceDetailResponse>();
        CreateMap<InvoiceDetailRequest, InvoiceDetail>();

        // Product
        CreateMap<Product, ProductResponse>();
        CreateMap<CreateProductRequest, Product>();
        CreateMap<UpdateProductRequest, Product>();

        // User
        CreateMap<User, UserResponse>()
            .ForMember(d => d.Roles, o => o.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name).ToList()));
    }
}
