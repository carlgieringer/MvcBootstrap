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
    using MvcBootstrap.Properties;
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
                binding = BindEntityViewModel(bindingContext);
            }
            else if (bindingModelType.IsConstructedGenericTypeOfDefinition(typeof(Choice<>)))
            {
                binding = BindChoice(bindingContext);
            }
            else if (bindingModelType.IsConstructedGenericTypeOfDefinition(typeof(Choices<>)))
            {
                binding = BindChoices(bindingContext);
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
                if (viewModel != null)
                {
                    viewModel.Id = id;
                    binding = viewModel;
                }
            }

            return binding;
        }

        private static object BindChoice(ModelBindingContext bindingContext)
        {
            Type choiceType = bindingContext.ModelType.GetGenericArguments().FirstOrDefault();
            if (choiceType == null)
            {
                return null;
            }

            string valueKey = string.Format("{0}.Id", bindingContext.ModelName);
            var provider = bindingContext.ValueProvider.GetValue(valueKey);

            string attemptedValue = provider != null ?
                provider.AttemptedValue :
                null;

            var choice = bindingContext.Model ??
                Activator.CreateInstance(typeof(Choice<>).MakeGenericType(choiceType));

            if (!string.IsNullOrEmpty(attemptedValue))
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
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex);
                }

                if (success)
                {
                    var viewModel = Activator.CreateInstance(choiceType) as IEntityViewModel;
                    if (viewModel != null)
                    {
                        viewModel.Id = id;
                    }

                    GenericHelpers.SetChoiceSelection(choiceType, choice, viewModel);
                }
            }

            return choice;
        }

        private static object BindChoices(ModelBindingContext bindingContext)
        {
            // By convention we append .Id in the html inputs to emphasize that we are referring to entities by their Id.
            string valueKey = string.Format("{0}.Id", bindingContext.ModelName);
            var provider = bindingContext.ValueProvider.GetValue(valueKey);

            string attemptedValuesString = provider != null ?
                provider.AttemptedValue :
                null;

            Type choicesType = bindingContext.ModelType.GetGenericArguments().FirstOrDefault();

            if (choicesType == null)
            {
                return null;
            }

            var choices = bindingContext.Model ??
                Activator.CreateInstance(typeof(Choices<>).MakeGenericType(choicesType));

            if (!string.IsNullOrEmpty(attemptedValuesString))
            {
                var viewModelsList = Activator.CreateInstance(typeof(List<>).MakeGenericType(choicesType)) as IList;

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
                        var viewModel = Activator.CreateInstance(choicesType) as IEntityViewModel;
                        viewModel.Id = id;
                        viewModelsList.Add(viewModel);
                    }
                }

                GenericHelpers.SetChoicesSelection(choicesType, choices, viewModelsList);
            }

            return choices;
        }

        private static class GenericHelpers
        {
            private static readonly MethodInfo SetChoiceSelectionMethod = typeof(GenericHelpers)
                .GetMethod("SetChoiceSelectionImpl", BindingFlags.Static | BindingFlags.NonPublic);

            private static readonly MethodInfo SetChoicesSelectionMethod = typeof(GenericHelpers)
                .GetMethod("SetChoicesSelectionImpl", BindingFlags.Static | BindingFlags.NonPublic);

            private static readonly MethodInfo ReplaceCollectionContentsMethod = typeof(GenericHelpers)
                .GetMethod("ReplaceCollectionImpl", BindingFlags.Static | BindingFlags.NonPublic);

            private static readonly MethodInfo ReplaceDictionaryContentsMethod = typeof(GenericHelpers)
                .GetMethod("ReplaceDictionaryImpl", BindingFlags.Static | BindingFlags.NonPublic);

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void SetChoiceSelection(Type choiceType, object choice, object selection)
            {
                MethodInfo targetMethod = SetChoiceSelectionMethod.MakeGenericMethod(choiceType);
                targetMethod.Invoke(null, new object[] { choice, selection });
            }

            private static void SetChoiceSelectionImpl<T>(Choice<T> choice, T selection) 
                where T : class, IEntityViewModel
            {
                choice.Selection = selection;
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void SetChoicesSelection(Type choiceType, object choices, object selection)
            {
                MethodInfo targetMethod = SetChoicesSelectionMethod.MakeGenericMethod(choiceType);
                targetMethod.Invoke(null, new object[] { choices, selection });
            }

            private static void SetChoicesSelectionImpl<T>(Choices<T> choice, IEnumerable<T> selections) 
                where T : class, IEntityViewModel
            {
                choice.Selections = selections;
            }

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
            //if (propertyDescriptor.PropertyType == typeof(DateTime))
            //{
            //    string propertyName = propertyDescriptor.Name;
            //    var provider = bindingContext.ValueProvider.GetValue(propertyName);
            //    if (provider != null)
            //    {
            //        string attemptedValue = provider.AttemptedValue;

            //        if (!string.IsNullOrEmpty(attemptedValue))
            //        {
            //            try
            //            {
            //                var date = DateTime.Parse(attemptedValue);
            //            }
            //            catch (FormatException ex)
            //            {
            //                bindingContext.ModelState.AddModelError(propertyName, ex);
            //            }
            //        }
            //    }
            //}

            base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
        }
    }
}
