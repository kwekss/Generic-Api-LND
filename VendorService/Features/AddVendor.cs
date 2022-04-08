using helpers;
using helpers.Atttibutes;
using models;
using System;
using System.Collections.Generic;
using System.Text;
using VendorService.Models;
using VendorService.Providers;

namespace VendorService.Features
{
    [Feature(Name = "AddVendor")]
    public class AddVendorFeature : BaseServiceFeature
    {
        private readonly IDatabaseProvider _databaseProvider;

        public AddVendorFeature(IDatabaseProvider databaseProvider) : base()
        {
            _databaseProvider = databaseProvider;
        }

        [Entry]
        public ApiResponse AddVendor([FromJsonBody] InputVendorModel payload)
        {
            var review = _databaseProvider.InsertVendor(payload);
            return new ApiResponse { Success = true, ResponseMessage = $"{ FeatureName } from {Service} and the Payload is {payload} " };
        }

    }
}
