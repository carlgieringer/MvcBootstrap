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
    public class BootstrapActionVisibility : Dictionary<BootstrapActions, bool>, IDictionary<BootstrapActions, bool>
    {
        public bool ContainsKey(string bootstrapActionString)
        {
            BootstrapActions action;
            if (Enum.TryParse(bootstrapActionString, out action))
            {
                return base.ContainsKey(action);
            }

            return false;
        }

        public bool TryGetValue(string bootstrapActionString, out bool visible)
        {
            BootstrapActions action;
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
                BootstrapActions action;
                if (!Enum.TryParse(bootstrapActionString, out action))
                {
                    throw new InvalidOperationException("{0} is not a valid {1}".F(bootstrapActionString, typeof(BootstrapActions).Name));
                }

                return base[action];
            }

            set
            {
                BootstrapActions action;
                if (!Enum.TryParse(bootstrapActionString, out action))
                {
                    throw new InvalidOperationException("{0} is not a valid {1}".F(bootstrapActionString, typeof(BootstrapActions).Name));
                }

                base[action] = value;
            }
        }
    }
}
