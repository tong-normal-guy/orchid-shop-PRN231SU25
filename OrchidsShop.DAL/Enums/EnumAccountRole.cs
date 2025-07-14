using System.Runtime.Serialization;

namespace OrchidsShop.DAL.Enums;

[DataContract]
public enum EnumAccountRole
{
    [EnumMember(Value = "ADMIN")]
    ADMIN,
    [EnumMember(Value = "CUSTOMER")]
    CUSTOMER
}
