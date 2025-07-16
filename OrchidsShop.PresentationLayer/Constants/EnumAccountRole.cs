using System.Runtime.Serialization;

namespace OrchidsShop.PresentationLayer.Constants;

[DataContract]
public enum EnumAccountRole
{
    [EnumMember(Value = "ADMIN")]
    ADMIN,
    [EnumMember(Value = "CUSTOMER")]
    CUSTOMER
}
