﻿using System;
using Mediachase.Commerce.Customers;
using Vipps.Login.Models;

namespace Vipps.Login.Episerver.Commerce
{
    public static class CustomerContactExtensions
    {
        public static void SetVippsSubject(this CustomerContact contact, Guid? subjectGuid)
        {
            contact[MetadataConstants.VippsSubjectGuidFieldName] = subjectGuid;
        }

        public static Guid? GetVippsSubject(this CustomerContact contact)
        {
            return contact[MetadataConstants.VippsSubjectGuidFieldName] as Guid?;
        }

        
    }
}