using Repac.Data.Models;
using Repac.Data.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace Repac.Data
{
    public static class Constants
    {
        public static bool useBackend = true;

        public static readonly Color lightGreen = Color.FromHex("#D9EAD3");
        public static readonly Color logoGreen = Color.FromHex("#8dc73f");
        public static readonly Color logoBlue = Color.FromHex("#24a9e1");
        public static readonly Color darkRed = Color.FromHex("#CC0000");


        public static readonly Guid scannerId = Guid.Parse("c75cc42d-1295-4cea-ba9e-c0b88bddfa49");

        public static readonly TimeSpan scanDelay = new TimeSpan(4000);

        //Container tags
        public static Dictionary<Guid, String> containerTags = new Dictionary<Guid, String>()
        {
            [Guid.Parse("51a29bbf-d4d3-43c9-b290-d2445611b0d3")] = "3008 33B2 DDD9 0140 0000 0000", //1
            [Guid.Parse("b8a43e30-d24d-43f9-a697-7d6614d6c786")] = "E280 1160 6000 0207 1111 3017", //2
            [Guid.Parse("00469717-ce49-4a04-aa0d-3a0ad9ff8801")] = "E280 1160 6000 0207 1110 F4F7", //3
            [Guid.Parse("d1855273-312a-4c19-91ce-33c9fb8ec5e2")] = "E280 6894 0000 5003 2C91 48FC", //4
            [Guid.Parse("390eedbf-792e-48de-bbb4-d6a6b7e8dd65")] = "E280 6894 0000 4003 2C91 48FD"  //5
        };

        public static Dictionary<String, Guid> containerTagsEPCLookup = containerTags.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        //Admin Keychains
        public static List<Guid> adminKeychains = new List<Guid>()
        {
           Guid.Parse("c55cf3d1-9fd5-435d-b7cc-ea36a1c1bf3c")
        };

        //User Keychains

        public static Dictionary<Guid, UserDTO> userKeychains = new Dictionary<Guid, UserDTO>()
        {
            [Guid.Parse("b1056d6c-7f02-4f46-bdc6-e9feaff54a20")] = new UserDTO { FirstName = "Sasha", LastName = "Artemiev", OwnedCredits = 5, UsedCredits = 0 },
            [Guid.Parse("11b86ae9-9a49-4d2f-a9d9-27dfb1c4863a")] = new UserDTO { FirstName = "Denis", LastName = "Lopatin", OwnedCredits = 1, UsedCredits = 0 }
        };

        public static List<TakeOutLocationDTO> takeOutLocationsCache = new List<TakeOutLocationDTO>()
        {
            new TakeOutLocationDTO { PhoneNumber="4387328466",LocationName = "Danny loc",ResponsiblePersonName="Danny Aubé"},
            new TakeOutLocationDTO { PhoneNumber="4387328477",LocationName = "Justine loc",ResponsiblePersonName="Justine Qui"},
            new TakeOutLocationDTO { PhoneNumber="5142528999",LocationName = "Paranjan loc",ResponsiblePersonName="Paranjan..."},
            new TakeOutLocationDTO { PhoneNumber="4387328599",LocationName = "Sasha loc",ResponsiblePersonName="Sasha Art"},
            new TakeOutLocationDTO { PhoneNumber="5141528555",LocationName = "Denis loc",ResponsiblePersonName="Denis Lopatin"},
            new TakeOutLocationDTO { PhoneNumber="4387328588",LocationName = "Anton loc",ResponsiblePersonName="Anton Bosenko"},
            new TakeOutLocationDTO { PhoneNumber="5142532899",LocationName = "Yulia loc",ResponsiblePersonName="Yulia Chukrii"},
            new TakeOutLocationDTO { PhoneNumber="5144328239",LocationName = "Ray loc",ResponsiblePersonName="Ray Wu"},
            new TakeOutLocationDTO { PhoneNumber="5144362109",LocationName = "Philippe loc",ResponsiblePersonName="Philippe Adib"}
        };

    }
}
