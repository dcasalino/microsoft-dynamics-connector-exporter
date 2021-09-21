using Microsoft.Dynamics.Integration.Client.Core.ConfigurationService;
using MicrosoftDynamicsConnectorExporter.Params;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel.Description;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MicrosoftDynamicsConnectorExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            string gccVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Connector Controller ver. {gccVersion} - for {Parameters.Customer}");
            Console.ResetColor();

            ConfigurationServiceContractClient cfg = new ConfigurationServiceContractClient();

            if (!string.IsNullOrEmpty(Parameters.Username) && !string.IsNullOrEmpty(Parameters.Password) && !string.IsNullOrEmpty(Parameters.Domain))
                cfg.ClientCredentials.Windows.ClientCredential = new NetworkCredential(Parameters.Username, Parameters.Password, Parameters.Domain);

            var applicationIntegrations = cfg.GetIntegrations();

            int iIdx = 1;

            Dictionary<int, Tuple<Guid, string, ApplicationIntegration>> integrations = new Dictionary<int, Tuple<Guid, string, ApplicationIntegration>>();
            Dictionary<int, Tuple<Guid, string, SiteIntegration>> siteIntegrations = new Dictionary<int, Tuple<Guid, string, SiteIntegration>>();

            foreach (ApplicationIntegration ai in applicationIntegrations)
            {
                Console.WriteLine($"{iIdx} - {ai.Description}");
                integrations.Add(iIdx, new Tuple<Guid, string, ApplicationIntegration>(ai.Id, ai.Description, ai));

                iIdx++;
            }

            Console.Write("Seleziona indice integrazione: ");

            string result = string.Empty;

            if (args.Count() == 0)
                result = Console.ReadLine();
            else
            {
                result = Convert.ToString(integrations.FirstOrDefault(x => x.Value.Item1 == new Guid(args[0])).Key);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(result);
                Console.ResetColor();
            }

            Guid selectedIntegrationId = Guid.Empty;

            try
            {
                selectedIntegrationId = integrations[Convert.ToInt32(result)].Item1;
            }
            catch (Exception ex) { }

            if (selectedIntegrationId == Guid.Empty)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Bad index selected");

                Environment.Exit(1);
            }

            string selectedIntegrationDescription = integrations[Convert.ToInt32(result)].Item2;
            ApplicationIntegration selectedIntegration = integrations[Convert.ToInt32(result)].Item3;

            iIdx = 1;
            foreach (SiteIntegration si in selectedIntegration.SiteIntegrations)
            {
                Console.WriteLine($"{iIdx} - {si.Description}");
                siteIntegrations.Add(iIdx, new Tuple<Guid, string, SiteIntegration>(si.Id, si.Description, si));

                iIdx++;
            }

            Console.Write("Seleziona indice flussi: ");

            if (args.Count() == 0)
                result = Console.ReadLine();
            else
            {
                result = Convert.ToString(siteIntegrations.FirstOrDefault(x => x.Value.Item1 == new Guid(args[1])).Key);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(result);
                Console.ResetColor();
            }

            Guid selectedSiteIntegrationId = siteIntegrations[Convert.ToInt32(result)].Item1;
            string selectedSiteIntegrationDescription = siteIntegrations[Convert.ToInt32(result)].Item2;
            SiteIntegration selectedSiteIntegration = siteIntegrations[Convert.ToInt32(result)].Item3;

            string timestampFolderName = $"{Parameters.Customer} - {DateTime.Now.ToString("yyyyMMddhhmmss")} - {selectedSiteIntegrationDescription.Replace("\\", "-")}";
            string mapConfigurationTimestampFolderName = $"{timestampFolderName}\\Map";

            string folderName = $"{Parameters.Customer} - {selectedSiteIntegrationDescription.Replace("\\", "-")}";
            string mapConfigurationFolderName = $"{folderName}\\Map";

            string sBatch = $"@echo off\r\ncls\r\ngcc.exe {selectedIntegrationId} {selectedSiteIntegrationId}";
            System.IO.File.WriteAllText($"{folderName}.bat", sBatch);

            string indexName = $"{Parameters.Customer}_{selectedSiteIntegrationDescription.Replace("\\", "_").Replace(" ", string.Empty)}_Index.txt";
            if (!System.IO.Directory.Exists(timestampFolderName))
                System.IO.Directory.CreateDirectory(timestampFolderName);

            if (!System.IO.Directory.Exists(mapConfigurationTimestampFolderName))
                System.IO.Directory.CreateDirectory(mapConfigurationTimestampFolderName);

            if (System.IO.Directory.Exists(folderName))
                System.IO.Directory.Delete(folderName, true);

            System.IO.Directory.CreateDirectory(folderName);
            System.IO.Directory.CreateDirectory(mapConfigurationFolderName);

            string dynamicsNavBaseUrl = string.Empty;
            if (selectedSiteIntegration.Adapter1.Name.Equals("Microsoft Dynamics NAV"))
                dynamicsNavBaseUrl = selectedSiteIntegration.Adapter1.Settings.FirstOrDefault(x => x.Name == "BaseWebServicesUrl").Value;

            string sPreviousFolderName = string.Empty;

            // Recupero il nome dell'ultima cartella presente nel file indice.
            if (System.IO.File.Exists(indexName))
            {
                List<string> index = System.IO.File.ReadAllLines(indexName).ToList();

                if (index.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Nessuna possibilità di confronto con flussi esistenti.");
                    Console.ResetColor();
                }
                else
                {
                    sPreviousFolderName = index.Last();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Confronto flussi attivo con definizione {sPreviousFolderName}.");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Nessuna possibilità di confronto con flussi esistenti.");
                Console.ResetColor();
            }

            foreach (MapWithDetail map in selectedSiteIntegration.Maps.OrderBy(x => x.Name))
            {
                StringBuilder stringBuilder = new StringBuilder();
                MapDefinition mapDef = null;
                EntryValidationResult validationResult = null;


                try
                {
                    mapDef = cfg.GetMapDefinition(map.Id);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{map.Name}");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    stringBuilder.AppendLine(ex.ToString());

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error reading definition for map: {map.Name}");
                    Console.ResetColor();
                }

                if (mapDef != null)
                {
                    validationResult = cfg.ValidateEntries(map.Id, mapDef.Entries);

                    stringBuilder.AppendLine($"# Version: {gccVersion}");
                    stringBuilder.AppendLine($"# Source: {mapDef.SourceDefinition.Types[0].Name}");
                    stringBuilder.AppendLine($"# Destination: {mapDef.DestinationDefinition.Types[0].Name}");

                    if (string.IsNullOrEmpty(dynamicsNavBaseUrl))
                    {
                        string dynamicsNavWebServiceDefinitionUrl = string.Empty;
                        if (mapDef.DestinationDefinition.RootField.Definition.DisplayName.StartsWith("NAV"))
                            dynamicsNavWebServiceDefinitionUrl = $"{dynamicsNavBaseUrl}Page/Integration_{mapDef.DestinationDefinition.RootField.Definition.Name}";

                        if (string.IsNullOrEmpty(dynamicsNavWebServiceDefinitionUrl))
                            if (mapDef.SourceDefinition.RootField.Definition.DisplayName.StartsWith("NAV"))
                                dynamicsNavWebServiceDefinitionUrl = $"{dynamicsNavBaseUrl}Page/Integration_{mapDef.SourceDefinition.RootField.Definition.Name}";
                    }

                    #region "Filters"
                    FilterDefinition filters = cfg.GetFilterDefinition(map.Id);
                    if (filters.RootFilterGroup.Elements.Count > 0)
                        stringBuilder.AppendLine($"# Filters #");

                    foreach (FilterElement f in filters.RootFilterGroup.Elements)
                    {
                        FilterCriterion filterCriterion = ((FilterCriterion)f);
                        string comparator = string.Empty;

                        try
                        {
                            comparator = filterCriterion.ComparatorFullName.Split(':')[1];
                        }
                        catch (Exception ex)
                        {
                            comparator = "?";
                        }

                        stringBuilder.AppendLine($"{filterCriterion.FilterGate.ToString()} {filterCriterion.SourceFullName} {comparator} {filterCriterion.EntryValue}");
                    }

                    if (filters.RootFilterGroup.Elements.Count > 0)
                        stringBuilder.AppendLine($"# End Filters #");
                    #endregion

                    stringBuilder.AppendLine($"# Map #");

                    foreach (var mEntry in mapDef.Entries.OrderBy(x => x.FullName))
                    {
                        string entryValue = Convert.ToString(mEntry.Value);

                        if (!string.IsNullOrEmpty(entryValue))
                        {
                            Entry e = null;

                            if (validationResult != null)
                                e = validationResult.ProcessedEntries.FirstOrDefault(x => x.FullName == mEntry.FullName);

                            string sSourceDefinition = string.Empty;

                            if (e != null)
                                try
                                {
                                    SourceValue sourceValue = (SourceValue)e.Value;
                                    sSourceDefinition = sourceValue.FullName;
                                }
                                catch (Exception ex) { }

                            string sEntryOutput = string.Empty;
                            if (!string.IsNullOrEmpty(sSourceDefinition))
                                sEntryOutput = $"{mapDef.DestinationDefinition.Types[0].Name}.{mEntry.FullName} := {mapDef.SourceDefinition.Types[0].Name}.{entryValue} | {sSourceDefinition}";
                            else
                                sEntryOutput = $"{mapDef.DestinationDefinition.Types[0].Name}.{mEntry.FullName} := {mapDef.SourceDefinition.Types[0].Name}.{entryValue}";

                            stringBuilder.AppendLine(sEntryOutput);
                        }
                    }

                    stringBuilder.AppendLine($"# End Map #");
                }

                //Hash del contentuto del file
                string fileContentHash = Hash.GetHashString(stringBuilder.ToString());

                //Aggiungo l'HASH al file
                stringBuilder.AppendLine($"# Hash: {fileContentHash}");

                string sCurrentFluxFileName = $"{timestampFolderName}\\{map.Name}.txt";
                System.IO.File.WriteAllText(sCurrentFluxFileName, stringBuilder.ToString());

                if (!string.IsNullOrEmpty(sPreviousFolderName))
                {
                    string sPreviousFluxFileName = $"{sPreviousFolderName}\\{map.Name}.txt";
                    if (System.IO.File.Exists(sPreviousFluxFileName))
                    {
                        if (!System.IO.File.ReadAllText(sPreviousFluxFileName)
                            .Equals(System.IO.File.ReadAllText(sCurrentFluxFileName)))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Modifica rilevata sul flusso {map.Name}.");

                            //Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Nessuna modifica rilevata sul flusso {map.Name}.");

                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"Nuovo flusso configurato {map.Name}");

                        Console.ResetColor();
                    }
                }

                byte[] xmlConfiguration = cfg.ExportMap(map.Id);
                System.IO.File.WriteAllBytes($"{mapConfigurationTimestampFolderName}\\{map.Name}.map", xmlConfiguration);


            }

            System.IO.File.AppendAllText(indexName, $"{timestampFolderName}\r\n");

            DirectoryInfo directoryInfo = new DirectoryInfo(timestampFolderName);
            FileInfo[] allGeneratedFiles = directoryInfo.GetFiles();
            foreach (FileInfo fileInfo in allGeneratedFiles)
            {
                string tempPath = Path.Combine(folderName, fileInfo.Name);
                fileInfo.CopyTo(tempPath, true);
            }

            directoryInfo = new DirectoryInfo(mapConfigurationTimestampFolderName);
            allGeneratedFiles = directoryInfo.GetFiles();
            foreach (FileInfo fileInfo in allGeneratedFiles)
            {
                string tempPath = Path.Combine(mapConfigurationFolderName, fileInfo.Name);
                fileInfo.CopyTo(tempPath, true);
            }

            Console.ResetColor();
            Console.WriteLine("Operazione completata.");

            if (args.Count() == 0)
                Console.ReadLine();
        }
    }
}
