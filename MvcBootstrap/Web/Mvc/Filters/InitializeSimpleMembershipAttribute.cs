namespace MvcBootstrap.Web.Mvc.Filters
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Reflection;
    using System.Threading;
    using System.Web.Mvc;

    using MvcBootstrap.Models;
    using MvcBootstrap.Util;

    using TEMTDomain.StaticLib;

    using WebMatrix.WebData;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InitializeSimpleMembershipAttribute : ActionFilterAttribute
    {
        #region Static Members

        private static SimpleMembershipInitializer initializer;

        private static object initializerLock = new object();

        private static bool isInitialized;

        #endregion


        #region Fields

        private readonly Type contextType;

        private readonly Type userProfileType;

        private readonly string connectionStringName;

        #endregion


        public InitializeSimpleMembershipAttribute(Type contextType, Type userProfileType, string connectionStringName)
        {
            this.contextType = contextType;
            this.userProfileType = userProfileType;
            this.connectionStringName = connectionStringName;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Ensure ASP.NET Simple Membership is initialized only once per app start
            Func<SimpleMembershipInitializer> valueFactory = () =>
                {
                    var membershipInitializer = new SimpleMembershipInitializer();
                    var method = typeof(SimpleMembershipInitializer)
                        .GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance)
                        .MakeGenericMethod(this.contextType, this.userProfileType);
                    method.Invoke(membershipInitializer, new object[] { null, this.connectionStringName });
                    return membershipInitializer;
                };
            LazyInitializer.EnsureInitialized(ref initializer, ref isInitialized, ref initializerLock, valueFactory);
        }

        private class SimpleMembershipInitializer
        {
            public void Initialize<TContext, TUserProfile>(
                IDatabaseInitializer<TContext> databaseInitializer,
                string connectionStringName)
                where TContext : DbContext
                where TUserProfile : IUserProfile
            {
                Database.SetInitializer(databaseInitializer);

                try
                {
                    using (var context = DependencyResolver.Current.GetService<DbContext>())
                    {
                        if (!context.Database.Exists())
                        {
                            // Create the SimpleMembership database without Entity Framework migration schema
                            ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
                        }
                    }

                    string userTableName = StringHelper.Pluralize(typeof(TUserProfile).Name, Count.Plural);
                    string userIdColumn = Of<IUserProfile>.CodeNameFor(p => p.Id);
                    string userNameColumn = Of<IUserProfile>.CodeNameFor(p => p.Username);
                    WebSecurity.InitializeDatabaseConnection(
                        connectionStringName,
                        userTableName,
                        userIdColumn,
                        userNameColumn,
                        autoCreateTables: true);
                }
                catch (Exception ex)
                {
                    const string Message = "The ASP.NET Simple Membership database could not be initialized.  "
                                           + "For more information, please see http://go.microsoft.com/fwlink/?LinkId=256588";
                    throw new InvalidOperationException(Message, ex);
                }
            }


        }
    }
}
