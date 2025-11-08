using NutriTrack_Domains.Tables.UsersTb;

namespace NutriTrack_Domains.Interfaces.NutritionCalculator
{
    public interface INutritionCalculatorService
    {
        void CalcularPlanoNutricional(Users usuario);
    }
}
