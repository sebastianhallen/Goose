// Guids.cs
// MUST match guids.h

namespace Goose
{
    using System;

    static class GuidList
    {

        public const string guidGoosePkgString = "ae74cfb8-8aa8-4c87-b2f2-e54a7977739f";
        public const string guidGooseCmdSetString = "7b5803b1-5a23-4229-886a-991213515664";

        public static readonly Guid guidGooseCmdSet = new Guid(guidGooseCmdSetString);
    };
}