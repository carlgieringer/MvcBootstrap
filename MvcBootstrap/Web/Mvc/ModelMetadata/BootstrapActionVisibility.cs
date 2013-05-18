namespace MvcBootstrap.Web.Mvc.ModelMetadata
{
    using System;
    using System.Collections.Generic;

    using MvcBootstrap.Extensions;
    using MvcBootstrap.Web.Mvc.Controllers;

    /// <summary>
    /// A class implementing <see cref="IDictionary{BootstrapAction, Boolean}"/> for associating
    /// a built-in MvcBootstrap controller action with a visibility.
    /// </summary>
    public class BootstrapActionVisibility : Dictionary<BootstrapAction, bool>, IDictionary<BootstrapAction, bool>
    {
        public bool ContainsKey(string bootstrapActionString)
        {
            BootstrapAction action;
            if (Enum.TryParse(bootstrapActionString, out action))
            {
                return base.ContainsKey(action);
            }

            return false;
        }

        public bool TryGetValue(string bootstrapActionString, out bool visible)
        {
            BootstrapAction action;
            if (Enum.TryParse(bootstrapActionString, out action))
            {
                return base.TryGetValue(action, out visible);
            }

            visible = default(bool);
            return false;
        }

        public bool this[string bootstrapActionString]
        {
            get
            {
                BootstrapAction action;
                if (!Enum.TryParse(bootstrapActionString, out action))
                {
                    throw new InvalidOperationException("{0} is not a valid {1}".F(bootstrapActionString, typeof(BootstrapAction).Name));
                }

                return base[action];
            }

            set
            {
                BootstrapAction action;
                if (!Enum.TryParse(bootstrapActionString, out action))
                {
                    throw new InvalidOperationException("{0} is not a valid {1}".F(bootstrapActionString, typeof(BootstrapAction).Name));
                }

                base[action] = value;
            }
        }
    }
}
