using System.Runtime.Serialization;

namespace OrchidsShop.PresentationLayer.Constants;

[DataContract]
public enum EnumOrderStatus
{
    [EnumMember(Value = "PENDING")]
    Pending,
    [EnumMember(Value = "PAID")]
    Paid,
    [EnumMember(Value = "CANCELLED")]
    Cancelled
}