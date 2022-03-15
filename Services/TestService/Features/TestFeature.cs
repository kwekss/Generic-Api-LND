using helpers;
using Microsoft.AspNetCore.Http;
using models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestService.Features
{
    public class TestFeature : BaseServiceFeature
    { 
        public override string FeatureName => "ProvisionBundle";
        public override string Service => "Bundles";

        public TestFeature(IHttpContextAccessor httpContext) :base(httpContext)
        {

        }
        public override async Task<ApiResponse> ExecuteFeature()
        {
            return new ApiResponse { Success = true, ResponseMessage = $"I am { FeatureName } from {Service}."};
        }
    }
}
