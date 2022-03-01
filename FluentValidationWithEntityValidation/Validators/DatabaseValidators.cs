using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using FluentValidation;


namespace FluentValidationWithEntityValidation.Validators;

public static class DatabaseValidators {
    public static IRuleBuilderOptions<T, TElement> IsUnique<T, TElement,TEntity>(this IRuleBuilder<T, TElement> ruleBuilder,
        IQueryable query,string propertyName)
    {
       
        return ruleBuilder.Must(t =>
        {
            var item = Expression.Parameter(typeof(TEntity), "item");
            var prop = Expression.Property(item, propertyName);
            var value = Expression.Constant(t);
            var equal = Expression.Equal(prop, value);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(equal, item);
            var exist = query.Any(lambda);
            return !exist;
        }).WithMessage($"{propertyName} must be unique");
    }
    
    public static IRuleBuilderOptions<T, TElement> IsUniqueSecond<T, TElement>(this IRuleBuilder<T, TElement> ruleBuilder,
        IQueryable query,Type model, string propertyName)
    {
        // var parameter = Expression.Parameter(typeof(MyClass));
       
        return ruleBuilder.Must(t =>
        {
            var item = Expression.Parameter(model, "item");
            var prop = Expression.Property(item, propertyName);
            var value = Expression.Constant(t);
            var equal = Expression.Equal(prop, value);
            var lambda = Expression.Lambda(equal, item);
            var exist = query.Any(lambda);
            return !exist;
        }).WithMessage($"{propertyName} must be unique");
    }
    
    public static IRuleBuilderOptions<T, TElement> IsForeignKey<T, TElement, TEntity>(this IRuleBuilder<T, TElement> ruleBuilder,
        IQueryable<TEntity> query, string propertyName="Id") where TEntity:class
    {
        // var parameter = Expression.Parameter(typeof(MyClass));
       
        return ruleBuilder.Must(t =>
        {
            var item = Expression.Parameter(typeof(TEntity), "item");
            var prop = Expression.Property(item, propertyName);
            var value = Expression.Constant(t);
            var equal = Expression.Equal(prop, value);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(equal, item);
            var exist = query.Any(lambda);
            return exist;
        }).WithMessage($"{propertyName} is not valid");
    }
    
    public static IRuleBuilderOptions<T, TElement> IsForeignKeySecond<T, TElement>(this IRuleBuilder<T, TElement> ruleBuilder,
        IQueryable query,Type model, string propertyName="Id")
    {
        // var parameter = Expression.Parameter(typeof(MyClass));
       
        return ruleBuilder.Must(t =>
        {
            var item = Expression.Parameter(model, "item");
            var prop = Expression.Property(item, propertyName);
            var value = Expression.Constant(t);
            var equal = Expression.Equal(prop, value);
            var lambda = Expression.Lambda(equal, item);
            var exist = query.Any(lambda);
            return exist;
        }).WithMessage($"{propertyName} is not valid");
    }

}
