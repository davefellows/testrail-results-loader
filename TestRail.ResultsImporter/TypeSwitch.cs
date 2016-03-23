// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeSwitch.cs" company="Microsoft">
//   Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
//   Class that allows switching on types, initial starting code from stackoverflow
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace TestRail.ResultsImporter
{
    /// <summary>
    /// Class that allows switching on types, initial starting code from stackoverflow
    /// </summary>
    public static class TypeSwitch
    {
        /// <summary>
        /// Type switch statement
        /// </summary>
        /// <param name="source">
        /// The object to perform type switch on
        /// </param>
        /// <param name="cases">
        /// The different cases of actions to perform. Note that
        /// more generic types should be put at the end of the case collection
        /// </param>
        public static void Switch(object source, params CaseInformation[] cases)
        {
            var type = source.GetType();
            CaseInformation defaultEntry = null;

            foreach (var entry in cases)
            {
                // Checks for the default case or whether the object is of the specific
                // type or inherited type
                if (entry.IsDefault)
                {
                    defaultEntry = entry;
                }
                else if (entry.Target.IsAssignableFrom(type))
                {
                    entry.Action(source);
                    break;
                }
            }

            if (defaultEntry != null)
            {
                defaultEntry.Action(source);
            }
        }

        /// <summary>
        /// Specific case for object type
        /// </summary>
        /// <typeparam name="T">
        /// Object type for case
        /// </typeparam>
        /// <param name="action">
        /// Action to perform
        /// </param>
        /// <returns>
        /// Case information object
        /// </returns>
        public static CaseInformation Case<T>(Action action)
        {
            return new CaseInformation()
            {
                Action = x => action(),
                Target = typeof(T)
            };
        }

        /// <summary>
        /// Specific case for object type
        /// </summary>
        /// <typeparam name="T">
        /// Object type for case
        /// </typeparam>
        /// <param name="action">
        /// Action to perform
        /// </param>
        /// <returns>
        /// Case information object
        /// </returns>
        public static CaseInformation Case<T>(Action<T> action)
        {
            return new CaseInformation()
            {
                Action = (x) => action((T)x),
                Target = typeof(T)
            };
        }

        /// <summary>
        /// Default case information construction
        /// </summary>
        /// <param name="action">
        /// Action to perform
        /// </param>
        /// <returns>
        /// Default case information object
        /// </returns>
        public static CaseInformation Default(Action action)
        {
            return new CaseInformation()
            {
                Action = x => action(),
                IsDefault = true
            };
        }

        /// <summary>
        /// Case info structure
        /// </summary>
        public class CaseInformation
        {
            /// <summary>
            /// Gets or sets a value indicating whether the case
            /// is the default case
            /// </summary>
            public bool IsDefault { get; set; }

            /// <summary>
            /// Gets or sets the target type for the case
            /// </summary>
            public Type Target { get; set; }

            /// <summary>
            /// Gets or sets the action to take when case is found
            /// </summary>
            public Action<object> Action { get; set; }
        }
    }
}
