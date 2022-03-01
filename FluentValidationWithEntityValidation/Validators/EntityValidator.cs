using System.Linq.Expressions;
using FluentValidation;
using FluentValidationWithEntityValidation.exceptions;
using FluentValidationWithEntityValidation.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FluentValidationWithEntityValidation.Validators;

public abstract class EntityValidator<TDtoClass, TModel,TContext> : AbstractValidator<TDtoClass> where TContext:DbContext where TModel : class
{
    /// <summary>(<paramref name="context"/>,<paramref name="ignoreList"/>,<paramref name="fieldMappers"/>).</summary>
    /// <param name="context">  database context </param>
    /// <param name="ignoreList"> List of DTO properties to skip </param>
    /// <param name="fieldMappers">Dictionary to map custom properties Names. Key=Entity property Name,Value=DTO property Name </param>
    /// <exception cref="EntityNotFoundException"> throw when TModel isn't found on context</exception>
    /// <exception cref="EntityColumnNotFoundException"> throw when any TDtoClass property isn't found on TModel</exception>
    public EntityValidator(TContext context,ICollection<string> ignoreList,IDictionary<string,string> fieldMappers)
    {
       
        var dtoProperties = typeof(TDtoClass).GetProperties().Select(p => p.Name).ToList();
        var model = context.Model.FindEntityType(typeof(TModel));
        if (model is null)
            throw new EntityNotFoundException(
                $"{typeof(TModel)} couldn't be found on context, please add your {typeof(TModel)} to your context to solve the problem ");
        foreach (var column in model.GetProperties())
        {
            var exist = fieldMappers.TryGetValue(column.Name,out var dtoPropertyName);
            dtoPropertyName ??= column.Name;
            
            if (ignoreList.Contains(column.Name) || !dtoProperties.Contains(exist? dtoPropertyName : column.Name)) continue;

            if (!column.IsNullable && !column.IsPrimaryKey())
            {
                var lambdaExpression = GenerateLambdaExpression(dtoPropertyName);
                RuleFor(lambdaExpression).NotNull();

            }

            if (column.ClrType == typeof(string))
            {
                var maxLength = column.GetMaxLength();
                if (maxLength.HasValue)
                {
                    var lambdaExpression = GenerateLambdaExpression<string>(dtoPropertyName);
                    RuleFor(lambdaExpression).Length(1,maxLength.Value);
                }
                
            }
            
            if (column.IsUniqueIndex())
            {
                var lambdaExpression = GenerateLambdaExpression(dtoPropertyName);
                var ctx = context.Set<TModel>();
                RuleFor(lambdaExpression).IsUniqueSecond(ctx,typeof(TModel),column.Name);
            }
            
            if (column.IsForeignKey())
            {
                var columnName = ExtractColumnName(model, column.Name) ??
                                 throw new EntityColumnNotFoundException($"{column} isn't a property of {model}");
                var property = GetForeignKeyFromModelByName(model, columnName);
                var ctx = context.Set(property.PrincipalEntityType.ClrType) ?? 
                          throw new EntityNotFoundException($"{property.PrincipalEntityType.ClrType} couldn't be found on context");
                var lambdaExpression = GenerateLambdaExpression(dtoPropertyName);
                RuleFor(lambdaExpression).IsForeignKeySecond(ctx,property.PrincipalEntityType.ClrType);
            }
           
        }
    }

    private IForeignKey GetForeignKeyFromModelByName(IEntityType model, string columnName)
    {
        var property = model.GetForeignKeys().First(p => p.ToString()!.Contains(columnName));
        return property;
    }
    private Expression<Func<TDtoClass, dynamic>> GenerateLambdaExpression(string columnName)
    {
        var parameter = Expression.Parameter(typeof(TDtoClass), "item");
        var memeberExpression = Expression.Property(parameter, columnName);
        var lambdaExpression = Expression.Lambda<Func<TDtoClass, dynamic>>(Expression.Convert(memeberExpression, typeof(object)), parameter);
        return lambdaExpression;
    }
    private Expression<Func<TDtoClass, T>> GenerateLambdaExpression<T>(string columnName)
    {
        var parameter = Expression.Parameter(typeof(TDtoClass), "item");
        var memeberExpression = Expression.Property(parameter, columnName);
        var lambdaExpression = Expression.Lambda<Func<TDtoClass, T>>(memeberExpression, parameter);
        return lambdaExpression;
    }
    private string? ExtractColumnName(IEntityType model, string column)
    {
        var property = GetForeignKeyFromModelByName(model, column);
        var propertyToString = property.ToString();
        var columnName =
            propertyToString?[
                (propertyToString.IndexOf("{", StringComparison.Ordinal) + 2)..(propertyToString.IndexOf("}",
                    StringComparison.Ordinal) - 1)];
        return columnName;

    }
   
}