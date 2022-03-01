namespace FluentValidationWithEntityValidation.exceptions;

public class EntityNotFoundException:ArgumentException
{
    public EntityNotFoundException(string ex):base(message: ex)
    {
        
    }
}