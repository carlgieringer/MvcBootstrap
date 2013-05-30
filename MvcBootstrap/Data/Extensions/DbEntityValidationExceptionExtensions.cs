namespace MvcBootstrap.Data.Extensions
{
    using System.Data.Entity.Validation;
    using System.Data.Objects;
    using System.Linq;

    public static class DbEntityValidationExceptionExtensions
    {
        public static string ToErrorMessage(this DbEntityValidationException ex)
        {
            var entityErrorsMessages = ex.EntityValidationErrors.Select(errs =>
                string.Format(
                    @"({0} ""{1}""): {2}",
                    ObjectContext.GetObjectType(errs.Entry.Entity.GetType()).Name,
                    errs.Entry.Entity.ToString(),
                    string.Join(
                        ", ", 
                        errs.ValidationErrors.Select(err => string.Format("{0}: {1}", err.PropertyName, err.ErrorMessage)))));
            return string.Join("; ", entityErrorsMessages);
        }
    }
}
