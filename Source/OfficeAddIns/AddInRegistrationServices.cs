using Microsoft.Internal.Tools.TeamMate.Foundation.Win32;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Internal.Tools.TeamMate.Office.AddIns
{
    public class AddInRegistrationServices
    {
        // TODO: Use this to get GUIDs, ask about types in assemblies, etc...
        // I think we stopped using this as it wouldn't allow us to do cross-architecture registration
        // (e.g. registering 32-bit from a 64-bit process)
        // private RegistrationServices registrationServices = new RegistrationServices();

        private static string GetProgIdForType(Type type)
        {
            return Marshal.GenerateProgIdForType(type);
        }

        private static string GetGuidForType(Type type)
        {
            return "{" + Marshal.GenerateGuidForType(type).ToString().ToUpper(CultureInfo.InvariantCulture) + "}";
        }

        public void RegisterOfficeAddIn(Type addInType, LoadBehavior loadBehavior)
        {
            RegisterManagedType(addInType, true);
            RegisterAddInInfo(addInType, loadBehavior);
        }

        public void UnregisterOfficeAddIn(Type addInType)
        {
            UnregisterAddInInfo(addInType);
            UnregisterManagedType(addInType);
        }

        private static void RegisterAddInInfo(Type addInType, LoadBehavior loadBehavior)
        {
            OfficeAddInInfoAttribute info = GetAddInInfo(addInType);
            if (info != null)
            {
                string progId = GetProgIdForType(addInType);
                string path = String.Format(@"Software\Microsoft\Office\{0}\Addins\{1}", info.ApplicationName, progId);

                RegistryKey currentUser = Registry.CurrentUser;
                using (RegistryKey addinKey = currentUser.CreateSubKey(path))
                {
                    if (addinKey != null)
                    {
                        addinKey.SetValue("Description", info.Description);
                        addinKey.SetValue("FriendlyName", info.FriendlyName);
                        addinKey.SetValue("LoadBehavior", loadBehavior, RegistryValueKind.DWord);
                    }
                }
            }
        }

        private static void UnregisterAddInInfo(Type addInType)
        {
            OfficeAddInInfoAttribute info = GetAddInInfo(addInType);
            if (info != null)
            {
                string progId = GetProgIdForType(addInType);
                string path = String.Format(@"Software\Microsoft\Office\{0}\Addins\{1}", info.ApplicationName, progId);

                RegistryKey currentUser = Registry.CurrentUser;
                currentUser.DeleteSubKeyTree(path, false);
            }
        }

        private static OfficeAddInInfoAttribute GetAddInInfo(Type type)
        {
            var attributes = type.GetCustomAttributes(typeof(OfficeAddInInfoAttribute), false);
            return (attributes.Any()) ? (OfficeAddInInfoAttribute)attributes[0] : null;
        }


        [SecurityCritical]
        private void RegisterManagedType(Type type, bool registerCodeBase = false)
        {
            Assembly assembly = type.Assembly;
            string strAsmName = assembly.FullName;
            string strAsmVersion = new AssemblyName(strAsmName).Version.ToString();
            string strAsmCodeBase = null;

            if (registerCodeBase)
            {
                strAsmCodeBase = new Uri(assembly.Location).AbsoluteUri;
            }

            string strRuntimeVersion = assembly.ImageRuntimeVersion;

            RegisterManagedType(type, strAsmName, strAsmVersion, strAsmCodeBase, strRuntimeVersion);
        }

        [SecurityCritical]
        private void RegisterManagedType(Type type, string assemblyFullName, string assemblyVersion, string assemblyCodeBase, string assemblyRuntimeVersion)
        {
            foreach (var currentUser in RegistryViewUtilities.OpenAllCurrentUserKeys())
            {
                using (var classesRoot = currentUser.CreateSubKey(@"Software\Classes"))
                {
                    RegisterManagedType(classesRoot, type, assemblyFullName, assemblyVersion, assemblyCodeBase, assemblyRuntimeVersion);
                }
            }
        }

        [SecurityCritical]
        private void RegisterManagedType(RegistryKey classesRoot, Type type, string strAsmName, string strAsmVersion, string strAsmCodeBase, string strRuntimeVersion)
        {
            string fullTypeName = type.FullName ?? "";
            string guid = GetGuidForType(type);
            string progIdForType = GetProgIdForType(type);
            if (progIdForType != string.Empty)
            {
                using (RegistryKey progIdKey = classesRoot.CreateSubKey(progIdForType))
                {
                    progIdKey.SetValue("", fullTypeName);
                    using (RegistryKey clsidKey = progIdKey.CreateSubKey("CLSID"))
                    {
                        clsidKey.SetValue("", guid);
                    }
                }
            }

            using (RegistryKey clsidKey = classesRoot.CreateSubKey("CLSID"))
            {
                using (RegistryKey classKey = clsidKey.CreateSubKey(guid))
                {
                    classKey.SetValue("", fullTypeName);
                    using (RegistryKey inProcServer32Key = classKey.CreateSubKey("InprocServer32"))
                    {
                        inProcServer32Key.SetValue("", "mscoree.dll");
                        inProcServer32Key.SetValue("ThreadingModel", "Both");
                        inProcServer32Key.SetValue("Class", type.FullName);
                        inProcServer32Key.SetValue("Assembly", strAsmName);
                        inProcServer32Key.SetValue("RuntimeVersion", strRuntimeVersion);
                        if (strAsmCodeBase != null)
                        {
                            inProcServer32Key.SetValue("CodeBase", strAsmCodeBase);
                        }

                        using (RegistryKey versionKey = inProcServer32Key.CreateSubKey(strAsmVersion))
                        {
                            versionKey.SetValue("Class", type.FullName);
                            versionKey.SetValue("Assembly", strAsmName);
                            versionKey.SetValue("RuntimeVersion", strRuntimeVersion);
                            if (strAsmCodeBase != null)
                            {
                                versionKey.SetValue("CodeBase", strAsmCodeBase);
                            }
                        }

                        if (progIdForType != string.Empty)
                        {
                            using (RegistryKey progIdKey = classKey.CreateSubKey("ProgId"))
                            {
                                progIdKey.SetValue("", progIdForType);
                            }
                        }
                    }

                    using (RegistryKey implementedCategoriesKey = classKey.CreateSubKey("Implemented Categories"))
                    {
                        using (implementedCategoriesKey.CreateSubKey("{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}"))
                        {
                        }
                    }
                }
            }

            EnsureManagedCategoryExists(classesRoot);
        }

        private static void EnsureManagedCategoryExists(RegistryKey classesRoot)
        {
            if (!ManagedCategoryExists(classesRoot))
            {
                using (RegistryKey componentCategoriesKey = classesRoot.CreateSubKey("Component Categories"))
                {
                    using (RegistryKey dotNetCategoryKey = componentCategoriesKey.CreateSubKey("{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}"))
                    {
                        dotNetCategoryKey.SetValue("0", ".NET Category");
                    }
                }
            }
        }

        private static bool ManagedCategoryExists(RegistryKey classesRoot)
        {
            using (RegistryKey componentCategoriesKey = classesRoot.OpenSubKey("Component Categories"))
            {
                if (componentCategoriesKey == null)
                {
                    return false;
                }
                using (RegistryKey dotNetCategoryKey = componentCategoriesKey.OpenSubKey("{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}"))
                {
                    if (dotNetCategoryKey == null)
                    {
                        return false;
                    }
                    object categoryValue = dotNetCategoryKey.GetValue("0");
                    if ((categoryValue == null) || (categoryValue.GetType() != typeof(string)))
                    {
                        return false;
                    }
                    string categoryStringValue = (string)categoryValue;
                    if (categoryStringValue != ".NET Category")
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        [SecurityCritical]
        private bool UnregisterManagedType(Type type)
        {
            Assembly assembly = type.Assembly;
            string assemblyFullName = assembly.FullName;
            string assemblyVersion = new AssemblyName(assemblyFullName).Version.ToString();
            return UnregisterManagedType(type, assemblyVersion);
        }

        [SecurityCritical]
        private bool UnregisterManagedType(Type type, string assemblyVersion)
        {
            bool unregistered = false;

            foreach (var currentUser in RegistryViewUtilities.OpenAllCurrentUserKeys())
            {
                using (var classesRoot = currentUser.OpenSubKey(@"Software\Classes", true))
                {
                    if (classesRoot != null)
                    {
                        unregistered |= UnregisterManagedType(classesRoot, type, assemblyVersion);
                    }
                }
            }

            return unregistered;
        }

        [SecurityCritical]
        private bool UnregisterManagedType(RegistryKey classesRoot, Type type, string assemblyVersion)
        {
            bool flag = true;
            string name = GetGuidForType(type);
            string progIdForType = GetProgIdForType(type);
            using (RegistryKey clsidKey = classesRoot.OpenSubKey("CLSID", true))
            {
                if (clsidKey != null)
                {
                    using (RegistryKey classKey = clsidKey.OpenSubKey(name, true))
                    {
                        if (classKey != null)
                        {
                            using (RegistryKey inprocServer32Key = classKey.OpenSubKey("InprocServer32", true))
                            {
                                if (inprocServer32Key != null)
                                {
                                    using (RegistryKey assemblyVersionKey = inprocServer32Key.OpenSubKey(assemblyVersion, true))
                                    {
                                        if (assemblyVersionKey != null)
                                        {
                                            assemblyVersionKey.DeleteValue("Assembly", false);
                                            assemblyVersionKey.DeleteValue("Class", false);
                                            assemblyVersionKey.DeleteValue("RuntimeVersion", false);
                                            assemblyVersionKey.DeleteValue("CodeBase", false);
                                            if ((assemblyVersionKey.SubKeyCount == 0) && (assemblyVersionKey.ValueCount == 0))
                                            {
                                                inprocServer32Key.DeleteSubKey(assemblyVersion);
                                            }
                                        }
                                    }
                                    if (inprocServer32Key.SubKeyCount != 0)
                                    {
                                        flag = false;
                                    }
                                    if (flag)
                                    {
                                        inprocServer32Key.DeleteValue("", false);
                                        inprocServer32Key.DeleteValue("ThreadingModel", false);
                                    }
                                    inprocServer32Key.DeleteValue("Assembly", false);
                                    inprocServer32Key.DeleteValue("Class", false);
                                    inprocServer32Key.DeleteValue("RuntimeVersion", false);
                                    inprocServer32Key.DeleteValue("CodeBase", false);
                                    if ((inprocServer32Key.SubKeyCount == 0) && (inprocServer32Key.ValueCount == 0))
                                    {
                                        classKey.DeleteSubKey("InprocServer32");
                                    }
                                }
                            }
                            if (flag)
                            {
                                classKey.DeleteValue("", false);
                                if (progIdForType != string.Empty)
                                {
                                    using (RegistryKey progIdKey = classKey.OpenSubKey("ProgId", true))
                                    {
                                        if (progIdKey != null)
                                        {
                                            progIdKey.DeleteValue("", false);
                                            if ((progIdKey.SubKeyCount == 0) && (progIdKey.ValueCount == 0))
                                            {
                                                classKey.DeleteSubKey("ProgId");
                                            }
                                        }
                                    }
                                }
                                using (RegistryKey categoriesKey = classKey.OpenSubKey("Implemented Categories", true))
                                {
                                    if (categoriesKey != null)
                                    {
                                        using (RegistryKey key7 = categoriesKey.OpenSubKey("{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}", true))
                                        {
                                            if (((key7 != null) && (key7.SubKeyCount == 0)) && (key7.ValueCount == 0))
                                            {
                                                categoriesKey.DeleteSubKey("{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}");
                                            }
                                        }
                                        if ((categoriesKey.SubKeyCount == 0) && (categoriesKey.ValueCount == 0))
                                        {
                                            classKey.DeleteSubKey("Implemented Categories");
                                        }
                                    }
                                }
                            }
                            if ((classKey.SubKeyCount == 0) && (classKey.ValueCount == 0))
                            {
                                clsidKey.DeleteSubKey(name);
                            }
                        }
                    }
                    if ((clsidKey.SubKeyCount == 0) && (clsidKey.ValueCount == 0))
                    {
                        classesRoot.DeleteSubKey("CLSID");
                    }
                }

                if (!flag || !(progIdForType != string.Empty))
                {
                    return flag;
                }

                using (RegistryKey progIdKey = classesRoot.OpenSubKey(progIdForType, true))
                {
                    if (progIdKey != null)
                    {
                        progIdKey.DeleteValue("", false);
                        using (RegistryKey clsidInProgIdKey = progIdKey.OpenSubKey("CLSID", true))
                        {
                            if (clsidInProgIdKey != null)
                            {
                                clsidInProgIdKey.DeleteValue("", false);
                                if ((clsidInProgIdKey.SubKeyCount == 0) && (clsidInProgIdKey.ValueCount == 0))
                                {
                                    progIdKey.DeleteSubKey("CLSID");
                                }
                            }
                        }
                        if ((progIdKey.SubKeyCount == 0) && (progIdKey.ValueCount == 0))
                        {
                            classesRoot.DeleteSubKey(progIdForType);
                        }
                    }
                    return flag;
                }
            }
        }
    }

    // http://msdn.microsoft.com/en-us/library/bb386106.aspx#LoadBehavior
    public enum LoadBehavior
    {
        Unloaded_DoNotAutoLoad = 0,
        Loaded_DoNotAutoLoad = 1,
        Unloaded_LoadAtStartup = 2,
        Loaded_LoadAtStartup = 3,
        Unloaded_LoadOnDemand = 8,
        Loaded_LoadOnDemand = 9,
        Loaded_LoadFirstTimeThenOnDemand = 16
    }
}
