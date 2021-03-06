﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bestiary
{
    static class ApplicationPaths
    {
        public static string GetDataDirectory()
        {
#if DEBUG
            return "..\\..\\";
#else
            return Directory.GetCurrentDirectory();
#endif
        }

        public static string GetResourcesDirectory()
        {
            return Path.Combine(GetDataDirectory(), "Resources");
        }

        public static string GetUserDataPath()
        {
            return Path.Combine(GetDataDirectory(), "User Data\\UserData.xml");
        }

        public static string GetUserDirectory()
        {
            return Path.Combine(GetDataDirectory(), "User Data");
        }

        public static string GetDisplayIconDirectory()
        {
            return Path.Combine(GetResourcesDirectory(), "DisplayIcons");
        }

        public static string GetViewIconDirectory()
        {
            return Path.Combine(GetResourcesDirectory(), "ViewIcons");
        }

        public static string GetFamiliarDataDirectory()
        {
            return Path.Combine(GetResourcesDirectory(), "FamiliarData");
        }

        public static string GetVersionPath()
        {
            return Path.Combine("..\\", "Resources", "manifest.txt");
        }

        public static string GetNewFamsPath()
        {
            return Path.Combine(GetFamiliarDataDirectory(), "New.txt");
        }

        public static string GetSettingsPath()
        {
            return Path.Combine(GetUserDirectory(), "Settings.txt");
        }
    }
}
