using System;
using System.Collections.Generic;
using System.Text;

namespace MenuService.Models
{
    public class MealModel
    {
        public string MealId { get; set; }
        public string MealName { get; set; }
        public double Price { get; set; }
        public double PromoPrice { get; set; }
        public string Description { get; set; }
        public VendorBriefModel? Vendor { get; set; }
        public MealCategoryModel? MealCategory { get; set; }
        public List<MealImage>? Images { get; set; }
        public List<MealExtra>? Extras { get; set; }
    }
}
