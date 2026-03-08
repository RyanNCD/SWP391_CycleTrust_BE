using AutoMapper;
using CycleTrust.Core.Entities;
using CycleTrust.Core.Enums;
using CycleTrust.Application.DTOs.User;
using CycleTrust.Application.DTOs.Catalog;
using CycleTrust.Application.DTOs.Listing;
using CycleTrust.Application.DTOs.Order;
using CycleTrust.Application.DTOs.Review;

namespace CycleTrust.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User
        CreateMap<User, UserDto>()
            .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.ToString()));

        // Catalog
        CreateMap<Brand, BrandDto>();
        CreateMap<CreateBrandRequest, Brand>();
        CreateMap<BikeCategory, CategoryDto>();
        CreateMap<CreateCategoryRequest, BikeCategory>();
        CreateMap<SizeOption, SizeOptionDto>();
        CreateMap<CreateSizeOptionRequest, SizeOption>();

        // Listing
        CreateMap<Listing, ListingDto>()
            .ForMember(d => d.SellerName, opt => opt.MapFrom(s => s.Seller.FullName))
            .ForMember(d => d.BrandName, opt => opt.MapFrom(s => s.Brand != null ? s.Brand.Name : null))
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.SizeLabel, opt => opt.MapFrom(s => s.SizeOption != null ? s.SizeOption.Label : null))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<ListingMedia, ListingMediaDto>()
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()));

        CreateMap<Inspection, InspectionDto>();

        CreateMap<CreateListingRequest, Listing>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => ListingStatus.DRAFT));

        CreateMap<CreateListingMediaRequest, ListingMedia>()
            .ForMember(d => d.Type, opt => opt.MapFrom(s => Enum.Parse<MediaType>(s.Type, true)));

        // Order
        CreateMap<Order, OrderDto>()
            .ForMember(d => d.ListingTitle, opt => opt.MapFrom(s => s.Listing.Title))
            .ForMember(d => d.BuyerName, opt => opt.MapFrom(s => s.Buyer.FullName))
            .ForMember(d => d.SellerName, opt => opt.MapFrom(s => s.Seller.FullName))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        // Review
        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.SellerName, opt => opt.MapFrom(s => s.Seller.FullName));

        CreateMap<CreateReviewRequest, Review>();

        // DepositPolicy
        CreateMap<DepositPolicy, CycleTrust.Application.DTOs.DepositPolicy.DepositPolicyDto>()
            .ForMember(d => d.Mode, opt => opt.MapFrom(s => s.Mode.ToString()));
    }
}
