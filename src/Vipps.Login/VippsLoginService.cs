﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using IdentityModel;
using Newtonsoft.Json;
using Vipps.Login.Models;

namespace Vipps.Login
{
    public class VippsLoginService : IVippsLoginService
    {
        private const string NorwegianLanguageTag = "no";
        public const string VippsTestApi = "https://apitest.vipps.no/";
        public const string VippsProdApi = "https://api.vipps.no/";

        public virtual VippsUserInfo GetVippsUserInfo(IIdentity identity)
        {
            return GetVippsUserInfo(identity as ClaimsIdentity);
        }

        public virtual VippsUserInfo GetVippsUserInfo(ClaimsIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            if (!IsVippsIdentity(identity))
            {
                return null;
            }

            var subjectClaim = identity.FindFirst(ClaimTypes.NameIdentifier) ??
                               identity.FindFirst(JwtClaimTypes.Subject);
            if (!Guid.TryParse(subjectClaim?.Value, out var subject))
            {
                return null;
            }

            return new VippsUserInfo
            {
                Sub = subject,
                BirthDate = ParseDate(identity.FindFirst(ClaimTypes.DateOfBirth) ??
                                      identity.FindFirst(JwtClaimTypes.BirthDate)),
                Email = ParseString(identity.FindFirst(ClaimTypes.Email) ?? identity.FindFirst(JwtClaimTypes.Email)),
                EmailVerified = ParseBool(identity.FindFirst(JwtClaimTypes.EmailVerified)),
                FamilyName = ParseString(identity.FindFirst(ClaimTypes.Surname) ??
                                         identity.FindFirst(JwtClaimTypes.FamilyName)),
                GivenName = ParseString(identity.FindFirst(ClaimTypes.GivenName) ??
                                        identity.FindFirst(JwtClaimTypes.GivenName)),
                Name = ParseString(identity.FindFirst(ClaimTypes.Name) ?? identity.FindFirst(JwtClaimTypes.Name)),
                PhoneNumber = ParseString(identity.FindFirst(JwtClaimTypes.PhoneNumber) ??
                                          identity.FindFirst(ClaimTypes.HomePhone) ??
                                          identity.FindFirst(ClaimTypes.MobilePhone) ??
                                          identity.FindFirst(ClaimTypes.OtherPhone)),
                Nnin = identity.FindFirst(VippsClaimTypes.Nnin)?.Value,
                Addresses = GetVippsAddresses(identity)
            };
        }

        public virtual bool IsVippsIdentity(IIdentity identity)
        {
            return IsVippsIdentity(identity as ClaimsIdentity);
        }

        public virtual bool IsVippsIdentity(ClaimsIdentity identity)
        {
            if (identity == null)
            {
                return false;
            }
            var issuer = identity.FindFirst(JwtClaimTypes.Issuer);
            var validIssuers = GetValidIssuers();
            if (issuer == null)
            {
                return false;
            }
            return validIssuers.Any(validIssuer => issuer.Value.StartsWith(validIssuer));
        }

        protected virtual IEnumerable<VippsAddress> GetVippsAddresses(ClaimsIdentity identity)
        {
            return identity
                .FindAll(JwtClaimTypes.Address)
                .Select(DeserializeAddress);
        }

        protected virtual IEnumerable<string> GetValidIssuers()
        {
            yield return VippsTestApi;
            yield return VippsProdApi;
            if (!string.IsNullOrWhiteSpace(VippsLoginConfig.Authority))
            {
                yield return VippsLoginConfig.Authority;
            }
        }

        protected virtual VippsAddress DeserializeAddress(Claim claim)
        {
            if (string.IsNullOrWhiteSpace(claim?.Value))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<VippsAddress>(claim.Value);
        }

        protected virtual string ParseString(Claim stringClaim)
        {
            return stringClaim?.Value;
        }

        protected virtual bool ParseBool(Claim boolClaim)
        {
            return bool.TryParse(boolClaim?.Value, out var verified) && verified;
        }

        protected virtual DateTime ParseDate(Claim dateClaim)
        {
            DateTime.TryParse(dateClaim?.Value,
                CultureInfo.GetCultureInfoByIetfLanguageTag(NorwegianLanguageTag), DateTimeStyles.None,
                out var date);
            return date;
        }
    }
}