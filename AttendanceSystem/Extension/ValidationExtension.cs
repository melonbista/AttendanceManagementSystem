using FluentValidation;

namespace AttendanceManagementSystem.Extension
{
    public static class ValidationExtension
    {
        public static IRuleBuilderOptions<T, double> MustBeLongitude<T>(this IRuleBuilder<T, double> ruleBuilder)
        {
            return ruleBuilder.InclusiveBetween(-180, 180);
        }

        public static IRuleBuilderOptions<T, double> MustBeLatitude<T>(this IRuleBuilder<T, double> ruleBuilder)
        {
            return ruleBuilder.InclusiveBetween(-90, 90);
        }

        public static IRuleBuilderOptions<T, string> MustBeNumber<T>(this IRuleBuilder<T, string> ruleBuilder,int length)
        {
            return ruleBuilder.Matches("^[0-9]{"+ length +"}$");
        }
    }
}
