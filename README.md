# MvcBootstrap
#### A framework for Rapid Application Development using ASP.NET MVC, Entity Framework, and Twitter Bootstrap

This .NET solution/project provides a framework for rapidly creating .NET web applications using Domain-Driven Design and Test-Driven 
Design.  There is an [example application](https://github.com/carlgieringer/MvcBootstrap.ExampleApp) illustrating its features and
best-practices.

This framework is in development and currently offers no guarantee as to backwards-compatibility.  When and if the project reaches a 1.0
version, then the 1.0 features will be maintained with backwards compatibility.

Current features of interest are built-in CRUD actions with integrated Entity/ViewModel mapping provided by 
[BaseBootstrapController](https://github.com/carlgieringer/MvcBootstrap/blob/master/MvcBootstrap/Web/Mvc/Controllers/BootstrapControllerBase.cs).
This controller also offers automated support for Optimistic Concurrency in conjunction with 
[IEntity](https://github.com/carlgieringer/MvcBootstrap/blob/master/MvcBootstrap/Models/IEntity.cs) and 
[IEntityViewModel](https://github.com/carlgieringer/MvcBootstrap/blob/master/MvcBootstrap/ViewModels/IEntityViewModel.cs)
(basically, when a concurrent update occurs, the ConcurrentlyEdited property of IEntityViewModel will be non-null and set
to an instance of IEntityViewModel containing the current database values.  Your views can display both a message and 
the current values to let the user resolve the collision.)