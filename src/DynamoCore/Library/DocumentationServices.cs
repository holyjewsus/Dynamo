﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Dynamo.Configuration;
using Dynamo.Interfaces;
using DynamoUtilities;

namespace Dynamo.Engine
{
    internal static class DocumentationServices
    {
        private static Dictionary<string, bool> _triedPaths = new Dictionary<string, bool>();

        private static Dictionary<string, XmlReader> _cached =
            new Dictionary<string, XmlReader>(StringComparer.OrdinalIgnoreCase);

        public static void DestroyCachedData()
        {
            if (_triedPaths != null) // Release references for collection.
                _triedPaths.Clear();

            if (_cached != null) // Release references for collection.
                _cached.Clear();
        }

        public static XmlReader GetForAssembly(string assemblyPath, IPathManager pathManager)
        {
            if (_triedPaths.ContainsKey(assemblyPath))
            {
                return _triedPaths[assemblyPath] ? _cached[assemblyPath] : null;
            }

            var documentationPath = "";
            if (ResolveForAssembly(assemblyPath, pathManager, ref documentationPath))
            {
                Console.WriteLine("we are trying to load documentation from" + documentationPath);
                var c = XmlReader.Create(documentationPath);
                _triedPaths.Add(assemblyPath, true);
                _cached.Add(assemblyPath, c);
                return c;
            }

            _triedPaths.Add(assemblyPath, false);
            return null;
        }

        private static bool ResolveForAssembly(string assemblyLocation,
            IPathManager pathManager, ref string documentationPath)
        {
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyLocation);
            if (pathManager != null)
            {
                pathManager.ResolveLibraryPath(ref assemblyLocation);
            }

            string baseDir = String.Empty;
            if (String.IsNullOrEmpty(assemblyLocation) || !File.Exists(assemblyLocation))
            {
                // Some nodes do not have a corresponding assembly, but their documentation 
                // xml file resides alongside DynamoCore.dll. If the assembly could not be 
                // located, fall back onto using DynamoCoreDirectory.
                baseDir = pathManager.DynamoCoreDirectory;
            }
            else
            {
                // Found the assembly location, search for documentation alongside it.
                baseDir = Path.GetDirectoryName(Path.GetFullPath(assemblyLocation));
            }

            var xmlFileName = assemblyName + ".xml";

            var language = System.Threading.Thread.CurrentThread.CurrentUICulture.ToString();
            var localizedResPath = Path.Combine(baseDir, language);
            documentationPath = Path.Combine(localizedResPath, xmlFileName);

            if (File.Exists(documentationPath))
                return true;

            //try with uppercase XML for linux
            documentationPath = PathHelper.replaceExtension(documentationPath, ".XML");
            if (File.Exists(documentationPath))
                return true;

            //try with the fallback culture
            localizedResPath = Path.Combine(baseDir, Configurations.FallbackUiCulture);
            documentationPath = Path.Combine(localizedResPath, xmlFileName);
            if (File.Exists(documentationPath))
                return true;

            //try with uppercase XML for linux
            documentationPath = PathHelper.replaceExtension(documentationPath, ".XML");
            if (File.Exists(documentationPath))
                return true;

            //try in the base directory
            documentationPath = Path.Combine(baseDir, xmlFileName);
            if (File.Exists(documentationPath))
                return true;

            //try with uppercase XML for linux
            documentationPath = PathHelper.replaceExtension(documentationPath, ".XML");
            return File.Exists(documentationPath);

        }

    }

}
