﻿using Microsoft.AspNetCore.Http;
using models;
using System;

namespace TestService.Models
{
    public class TestModel
    {
        public string Prop { get; set; }
        [ApiDoc(Description = "Some description for data object", Default = 1234)]
        public int Id { get; set; }
        public DateTime RequestTime { get; set; }

        public DataModel Data { get; set; }
    }
    public class TestUploadModel
    {
        public string Test { get; set; }

        public IFormFile Image { get; set; }
    }
    public class DataModel
    {
        public string Prop { get; set; }
        public DataSubModel? data { get; set; }
    }
    public class DataSubModel
    {
        public string Prop { get; set; }
    }
}
