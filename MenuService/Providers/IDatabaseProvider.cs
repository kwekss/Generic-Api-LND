using MenuService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MenuService.Providers
{
    public interface IDatabaseProvider
    {
        Task<List<MealModel>> GetMealsByVendorId(string id);
    }
}
