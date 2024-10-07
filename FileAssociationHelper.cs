using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira
{
    public static class FileAssociationHelper
    {
        public static void RegisterFileAssociation(string extension, string progId, string applicationPath)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\.{extension}"))
            {
                key.SetValue("", progId);
            }

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}"))
            {
                key.SetValue("", "Расчёт Dahmira");
                key.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command").SetValue("", $"\"{applicationPath}\" \"%1\"");
            }
        }

        public static void UnregisterFileAssociation(string extension, string progId)
        {
            Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\.{extension}", false);
            Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{progId}", false);
        }
    }
}
