﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RabbitMQWrapper.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("RabbitMQWrapper.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find consumer with name &apos;{0}&apos; in application configuration file..
        /// </summary>
        internal static string ConsumerNameNotFoundError {
            get {
                return ResourceManager.GetString("ConsumerNameNotFoundError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Started consuming from queue &apos;{0}&apos; with RabbitMQ consumer tag &apos;{1}&apos;..
        /// </summary>
        internal static string ConsumptionStartedLogEntry {
            get {
                return ResourceManager.GetString("ConsumptionStartedLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find unique certificate with subject name &apos;{0}&apos;. (Found {1} certificates)..
        /// </summary>
        internal static string CouldNotFindCertificateError {
            get {
                return ResourceManager.GetString("CouldNotFindCertificateError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A fatal error occurred. Shutting down..
        /// </summary>
        internal static string FatalErrorLogEntry {
            get {
                return ResourceManager.GetString("FatalErrorLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Acknowledged message with delivery tag &apos;{0}&apos; from queue &apos;{1}&apos;..
        /// </summary>
        internal static string MessageAcknowledgedLogEntry {
            get {
                return ResourceManager.GetString("MessageAcknowledgedLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid message received with delivery tag &apos;{0}&apos; from queue &apos;{1}&apos;. Validation errors: {2}..
        /// </summary>
        internal static string MessageFailsValidationLogEntry {
            get {
                return ResourceManager.GetString("MessageFailsValidationLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Negatively acknowledged message with delivery tag &apos;{0}&apos; from queue &apos;{1}&apos;..
        /// </summary>
        internal static string MessageNegativelyAcknowledgedLogEntry {
            get {
                return ResourceManager.GetString("MessageNegativelyAcknowledgedLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message with delivery tag &apos;{0}&apos; has been successfully processed..
        /// </summary>
        internal static string MessageProcessedLogEntry {
            get {
                return ResourceManager.GetString("MessageProcessedLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A message with delivery tag &apos;{0}&apos; has been received. Processing....
        /// </summary>
        internal static string MessageReceivedLogEntry {
            get {
                return ResourceManager.GetString("MessageReceivedLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Negatively acknowledged and requeued message with delivery tag &apos;{0}&apos; from queue &apos;{1}&apos;..
        /// </summary>
        internal static string MessageRequeuedLogEntry {
            get {
                return ResourceManager.GetString("MessageRequeuedLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Successfully deserialized and validated message with delivery tag &apos;{0}&apos; from queue &apos;{1}&apos;..
        /// </summary>
        internal static string MessageSuccessfullyReceivedLogEntry {
            get {
                return ResourceManager.GetString("MessageSuccessfullyReceivedLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message with delivery tag &apos;{0}&apos; failed to be processed. {1}.
        /// </summary>
        internal static string ProcessingErrorLogEntry {
            get {
                return ResourceManager.GetString("ProcessingErrorLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Publishing message to exchange &apos;{0}&apos; with routing key &apos;{1}&apos;: {2}.
        /// </summary>
        internal static string PublishingMessageLogEntry {
            get {
                return ResourceManager.GetString("PublishingMessageLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An unknown error occurred when trying to create a temporary queue..
        /// </summary>
        internal static string TemporaryQueueCreationError {
            get {
                return ResourceManager.GetString("TemporaryQueueCreationError", resourceCulture);
            }
        }
    }
}
