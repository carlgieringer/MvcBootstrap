namespace MvcBootstrap.Web.Mvc.ModelBinding
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Web.Mvc;

    using MvcBootstrap.Extensions;
    using MvcBootstrap.Reflection;
    using MvcBootstrap.ViewModels;

    public class MvcBootstrapModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException("bindingContext");
            }

            object binding;
            Type bindingModelType = bindingContext.ModelType;

            if (bindingModelType == typeof(decimal))
            {
                binding = BindDecimalWithAnyStyle(bindingContext);
            }
            else if (bindingModelType == typeof(decimal?))
            {
                binding = BindDecimalWithAnyStyle(bindingContext);
            }
            else if (bindingModelType == typeof(DateTime))
            {
                binding = BindDateTime(bindingContext);
            }
            else if (
                // We only want to custom bind IEntitViewModels inside other IEntityViewModels
                bindingModelType.IsAssignableTo(typeof(IEntityViewModel)) && 
                bindingContext.ModelMetadata.ContainerType.IsAssignableTo(typeof(IEntityViewModel)))
            {
                //binding = base.BindModel(controllerContext, bindingContext);
                binding = BindEntityViewModel(bindingContext);
            }
            else if (bindingModelType.IsConstructedGenericTypeFor(typeof(ChoiceCollection<>)))
            {
                binding = BindEntityViewModelCollection(bindingContext);
            }
            else
            {
                binding = base.BindModel(controllerContext, bindingContext);
            }

            return binding;
        }

        private static object BindEntityViewModel(ModelBindingContext bindingContext)
        {
            object binding = null;

            string attemptedValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue;

            if (!string.IsNullOrEmpty(attemptedValue))
            {
                int id = default(int);

                try
                {
                    id = int.Parse(attemptedValue);
                }
                catch (FormatException ex)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex);
                }

                var viewModel = Activator.CreateInstance(bindingContext.ModelType) as IEntityViewModel;
                viewModel.Id = id;
                binding = viewModel;
            }

            return binding;
        }

        private static object BindEntityViewModelCollection(ModelBindingContext bindingContext)
        {
            object binding = null;

            // By convention we append .Id in the html inputs to emphasize that we are referring to entities by their Id.
            string valueKey = string.Format("{0}.Id", bindingContext.ModelName);
            string attemptedValuesString = bindingContext.ValueProvider.GetValue(valueKey).AttemptedValue;

            if (!string.IsNullOrEmpty(attemptedValuesString))
            {
                Type enumerableType = ReflectionHelper.ExtractGenericInterface(bindingContext.ModelType, typeof(IEnumerable<>));

                if (enumerableType == null)
                {
                    return binding;
                }

                Type elementType = enumerableType.GetGenericArguments().First();

                var viewModelsList = new List<object>();

                // When the HTTP params contain more than one key-value pair with the same key, they are joined with commas
                var attemptedValues = attemptedValuesString.Split(',');

                foreach (string attemptedValue in attemptedValues)
                {
                    int id = default(int);
                    bool success = false;

                    try
                    {
                        id = int.Parse(attemptedValue);
                        success = true;
                    }
                    catch (FormatException ex)
                    {
                        bindingContext.ModelState.AddModelError(valueKey, ex);
                    }

                    if (success)
                    {
                        var viewModel = Activator.CreateInstance(elementType) as IEntityViewModel;
                        viewModel.Id = id;
                        viewModelsList.Add(viewModel);
                    }
                }

                // The Microsoft MVC implementations for binding collections not only return the collection, but also
                // try to clear and replace the elements inside bindingContext.Model
                object collection = bindingContext.Model;
                CollectionHelpers.ReplaceCollectionContents(elementType, collection, viewModelsList);
                binding = collection;
            }

            return binding;
        }

        private static class CollectionHelpers
        {
            private static readonly MethodInfo ReplaceCollectionContentsMethod = typeof(CollectionHelpers)
                .GetMethod("ReplaceCollectionImpl", BindingFlags.Static | BindingFlags.NonPublic);

            private static readonly MethodInfo ReplaceDictionaryContentsMethod = typeof(CollectionHelpers)
                .GetMethod("ReplaceDictionaryImpl", BindingFlags.Static | BindingFlags.NonPublic);

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void ReplaceCollectionContents(Type collectionType, object collection, object newContents)
            {
                MethodInfo targetMethod = ReplaceCollectionContentsMethod.MakeGenericMethod(collectionType);
                targetMethod.Invoke(null, new object[] { collection, newContents });
            }

            private static void ReplaceCollectionImpl<T>(ICollection<T> collection, IEnumerable newContents)
            {
                collection.Clear();
                if (newContents != null)
                {
                    foreach (object item in newContents)
                    {
                        // If item is not a T, then probably binding failed; but we still need to fill the index in the collection
                        T itemAsType = (item is T) ? (T)item : default(T);
                        collection.Add(itemAsType);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void ReplaceDictionary(Type keyType, Type valueType, object dictionary, object newContents)
            {
                MethodInfo targetMethod = ReplaceDictionaryContentsMethod.MakeGenericMethod(keyType, valueType);
                targetMethod.Invoke(null, new object[] { dictionary, newContents });
            }

            private static void ReplaceDictionaryImpl<TKey, TValue>(IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<object, object>> newContents)
            {
                dictionary.Clear();
                foreach (KeyValuePair<object, object> item in newContents)
                {
                    // if the item was not a T, some conversion failed. the error message will be propagated,
                    // but in the meanwhile we need to make a placeholder element in the dictionary.
                    TKey castKey = (TKey)item.Key; // this cast shouldn't fail
                    TValue castValue = (item.Value is TValue) ? (TValue)item.Value : default(TValue);
                    dictionary[castKey] = castValue;
                }
            }
        }

        private static object BindDateTime(ModelBindingContext bindingContext)
        {
            object binding = null;

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

        private static object BindDecimalWithAnyStyle(ModelBindingContext bindingContext)
        {
            object binding = null;

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
