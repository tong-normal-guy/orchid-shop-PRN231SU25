using System.Runtime.Serialization;

namespace OrchidsShop.DAL.Enums;
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