namespace FluentValidationWithEntityValidation.exceptions;

public class EntityColumnNotFoundException:ArgumentException
{
    public EntityColumnNotFoundException(string ex):base(message:ex)
    {
        
    }
}