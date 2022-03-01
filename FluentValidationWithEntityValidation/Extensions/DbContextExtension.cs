using Microsoft.EntityFrameworkCore;
namespace FluentValidationWithEntityValidation.Extensions;




public static class DbContextExtension
{
    public static IQueryable? Set(this DbContext context, Type T)
    {
        // Get the generic type definition
        var method = typeof(DbContext).GetMethods().First(c=>c.Name=="Set");

        // Build a method with the specific type argument you're interested in
        method = method?.MakeGenericMethod(T);
    
        return method?.Invoke(context, null) as IQueryable;
    }
}