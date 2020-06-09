﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mediachase.Commerce.Customers;
using Vipps.Login.Models;

namespace Vipps.Login.Episerver.Commerce
{
    public static class AddressExtensions
    {
        public static CustomerAddress FindVippsAddress(this IEnumerable<CustomerAddress> addresses, VippsAddress address)
        {
           return FindVippsAddress(addresses, address.AddressType);
        }

        public static CustomerAddress FindVippsAddress(this IEnumerable<CustomerAddress> addresses, VippsAddressType addressType)
        {
            return addresses
                .FindAllVippsAddresses()
                .FirstOrDefault(x =>
                    x.GetVippsAddressType().Equals(addressType));
        }

        public static IEnumerable<CustomerAddress> FindAllVippsAddresses(this IEnumerable<CustomerAddress> addresses)
        {
            return addresses.Where(x => x.GetVippsAddressType() != null);
        }

        public static void SetVippsAddressType(this CustomerAddress address, VippsAddressType? addressType)
        {
            address[MetadataConstants.VippsAddressTypeFieldName] = addressType?.ToString();
        }

        public static VippsAddressType? GetVippsAddressType(this CustomerAddress address)
        {
            var stringValue = address[MetadataConstants.VippsAddressTypeFieldName]?.ToString();
            if (Enum.TryParse(stringValue, out VippsAddressType enumValue))
            {
                return enumValue;
            }
            return null;
        }

        public static void MapVippsAddress(
            this CustomerAddress address,
            VippsAddress vippsAddress
        )
        {
            if (vippsAddress == null) throw new ArgumentNullException(nameof(vippsAddress));
            address.Line1 = vippsAddress.StreetAddress;
            address.City = vippsAddress.Region;
            address.PostalCode = vippsAddress.PostalCode;
            address.CountryCode = vippsAddress.Country;
            address.SetVippsAddressType(vippsAddress.AddressType);
        }
    }
}