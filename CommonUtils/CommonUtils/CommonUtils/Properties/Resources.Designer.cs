﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CommonUtils.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CommonUtils.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Configuration Section &apos;{0}&apos; was not found..
        /// </summary>
        internal static string ConfigurationSectionNotFoundError {
            get {
                return ResourceManager.GetString("ConfigurationSectionNotFoundError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find unique certificate with subject name &apos;{0}&apos;, error details - &apos;{1}&apos;.
        /// </summary>
        internal static string CouldNotFindCertificateError {
            get {
                return ResourceManager.GetString("CouldNotFindCertificateError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Directory Not Found: &apos;{0}&apos;.
        /// </summary>
        internal static string DirectoryNotFoundExceptionLogEntry {
            get {
                return ResourceManager.GetString("DirectoryNotFoundExceptionLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Directory must be in UNC format: &apos;{0}&apos;.
        /// </summary>
        internal static string DirectoryNotUncExceptionLogEntry {
            get {
                return ResourceManager.GetString("DirectoryNotUncExceptionLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to File Not Found: &apos;{0}&apos;.
        /// </summary>
        internal static string FileNotFoundExceptionLogEntry {
            get {
                return ResourceManager.GetString("FileNotFoundExceptionLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Authentication element of type &apos;{0}&apos; did not contain required field &apos;{1}&apos;. Rest Client name: &apos;{2}&apos;..
        /// </summary>
        internal static string InvalidAuthenticationConfigurationError {
            get {
                return ResourceManager.GetString("InvalidAuthenticationConfigurationError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Logging could not be initialized because the file &apos;{0}&apos; does not exist in directory &apos;{1}&apos;..
        /// </summary>
        internal static string LoggingConfigurationFileNotFoundError {
            get {
                return ResourceManager.GetString("LoggingConfigurationFileNotFoundError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occurred while trying to move file from: &apos;{0}&apos; to: &apos;{1}&apos; error: &apos;{2}&apos;.
        /// </summary>
        internal static string MovedFileExceptionLogEntry {
            get {
                return ResourceManager.GetString("MovedFileExceptionLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Moved file from: &apos;{0}&apos; to: &apos;{1}&apos;.
        /// </summary>
        internal static string MovedFileLogEntry {
            get {
                return ResourceManager.GetString("MovedFileLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parameter &apos;{0}&apos; is null.
        /// </summary>
        internal static string ParameterIsNullExceptionLogEntry {
            get {
                return ResourceManager.GetString("ParameterIsNullExceptionLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;{0}&apos;:{1:0}ms.
        /// </summary>
        internal static string PerformanceLogMessage {
            get {
                return ResourceManager.GetString("PerformanceLogMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find Rest Client with name &apos;{0}&apos; in application configuration file..
        /// </summary>
        internal static string RestClientNameNotFoundError {
            get {
                return ResourceManager.GetString("RestClientNameNotFoundError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No stub directory found in location &apos;{0}&apos;.
        /// </summary>
        internal static string StubDirectoryNotFoundError {
            get {
                return ResourceManager.GetString("StubDirectoryNotFoundError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ^\/.*$.
        /// </summary>
        internal static string UnixPathRegex {
            get {
                return ResourceManager.GetString("UnixPathRegex", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ^[a-zA-Z]\:.*$|\\\\.*|file\:\/\/.*.
        /// </summary>
        internal static string WindowsPathRegex {
            get {
                return ResourceManager.GetString("WindowsPathRegex", resourceCulture);
            }
        }
    }
}
