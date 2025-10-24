using NutriTrack_Domains.Tables.UsersTb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriTrack_Domains.Interfaces.NutritionCalculator
{
    public interface INutritionCalculatorService
    {
        void CalcularPlanoNutricional(Users usuario);
    }
}
