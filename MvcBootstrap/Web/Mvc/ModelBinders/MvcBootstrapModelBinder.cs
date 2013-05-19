namespace MvcBootstrap.Web.Mvc.ModelBinders
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web.Mvc;

    public class MvcBootstrapModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            object binding = null;

            if (bindingContext.ModelType == typeof(decimal))
            {
                string modelName = bindingContext.ModelName;
                string attemptedValue = bindingContext.ValueProvider.GetValue(modelName).AttemptedValue;

                try
                {
                    binding = decimal.Parse(attemptedValue, NumberStyles.Any);
                }
                catch (FormatException ex)
                {
                    bindingContext.ModelState.AddModelError(modelName, ex);
                }

                return binding;
            }
            else if (bindingContext.ModelType == typeof(decimal?))
            {
                string modelName = bindingContext.ModelName;
                string attemptedValue = bindingContext.ValueProvider.GetValue(modelName).AttemptedValue;

                if (!string.IsNullOrEmpty(attemptedValue))
                {
                    try
                    {
                        binding = decimal.Parse(attemptedValue, NumberStyles.Any);
                    }
                    catch (FormatException ex)
                    {
                        bindingContext.ModelState.AddModelError(modelName, ex);
                    }
                }

                return binding;
            }
            else if (bindingContext.ModelType == typeof(DateTime))
            {
                string modelName = bindingContext.ModelName;
                string attemptedValue = bindingContext.ValueProvider.GetValue(modelName).AttemptedValue;

                if (!string.IsNullOrEmpty(attemptedValue))
                {
                    try
                    {
                        binding = DateTime.Parse(attemptedValue);
                    }
                    catch (FormatException ex)
                    {
                        bindingContext.ModelState.AddModelError(modelName, ex);
                    }
                }

                return binding;
            }
            else
            {
                binding = base.BindModel(controllerContext, bindingContext);
            }

            return binding;
        }

        protected override void BindProperty(
            ControllerContext controllerContext, 
            ModelBindingContext bindingContext, 
            PropertyDescriptor propertyDescriptor)
        {
            if (propertyDescriptor.PropertyType == typeof(DateTime))
            {
                string propertyName = propertyDescriptor.Name;
                string attemptedValue = bindingContext.ValueProvider.GetValue(propertyName).AttemptedValue;

                if (!string.IsNullOrEmpty(attemptedValue))
                {
                    try
                    {
                        var date = DateTime.Parse(attemptedValue);
                    }
                    catch (FormatException ex)
                    {
                        bindingContext.ModelState.AddModelError(propertyName, ex);
                    }
                }
            }
            base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
        }
    }
}
