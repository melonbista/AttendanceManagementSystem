using AttendanceManagementSystem.Helper;
using AttendanceManagementSystem.Models;
using FluentValidation;
using MongoDB.Driver;
using System.Linq.Expressions;

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

        public static IRuleBuilderOptions<T, string> MustBeNumber<T>(this IRuleBuilder<T, string> ruleBuilder, int length)
        {
            return ruleBuilder.Matches("^[0-9]{" + length + "}$");
        }

        public static IRuleBuilderOptions<T, TField> MustBeUnique<T, TDocument, TField>(
            this IRuleBuilder<T, TField> ruleBuilder,
            DbHelper dbHelper,
            Expression<Func<TDocument, TField>> field,
            FilterDefinition<TDocument> filter)
            where TDocument : BaseModel
        {
            return ruleBuilder.Must(value => !dbHelper.RecordExists(Builders<TDocument>.Filter.Eq(field, value) & filter))
                .WithMessage("'{PropertyName}' has already been taken");
        }

        public static IRuleBuilderOptions<T, string> IdMustExist<T, TDocument>(
            this IRuleBuilder<T, string> ruleBuilder,
            DbHelper dbHelper,
            FilterDefinition<TDocument> filter)
            where TDocument : BaseModel
        {
            return ruleBuilder
                .Must(value => dbHelper.IdExists(value, filter))
                .WithMessage("'{PropertyName}' is invalid.");
        }
    }
}
