using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace com.sorlov.eidprovider
{
    internal class EIDResourceManager
    {
        internal static byte[] ResolveResource(string resourceName)
        {
            if (resourceName.ToLower().StartsWith("builtin://")) {
                resourceName = resourceName.SmartReplace("builtin://", "", StringComparison.OrdinalIgnoreCase);
                string[] names = resourceName.Split("/");

                if (names.Length != 2) return null;

                return GetResource(names[0], names[1]);

            } else {
                return GetFile(resourceName);
            }
        }

        internal static byte[] GetFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {

                string fileData = File.ReadAllText(filePath);
                return Encoding.UTF8.GetBytes(fileData);
            }
            catch
            {
                return null;
            }
        }

        internal static byte[] GetResource(string folder, string fileName)
        {
            try
            {

                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("com.sorlov.eidprovider.resources." + folder + "." + fileName))
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }

            }
            catch
            {
                return null;
            }
        }

    }
}
