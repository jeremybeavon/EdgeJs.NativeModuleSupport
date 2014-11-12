using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace EdgeJs.NativeModuleSupport
{
    public sealed class RewriteNodeLib : Task
    {
        [Required]
        public ITaskItem[] ProjectFiles { get; set; }

        [Required]
        public string EdgeDirectory { get; set; }

        [Required]
        public string Platform { get; set; }

        private string NewNodeLib
        {
            get { return Path.Combine(EdgeDirectory, Platform == "x64" ? "x64" : "x86", "node.lib"); }
        }

        public override bool Execute()
        {
            foreach (ITaskItem projectFile in ProjectFiles)
            {
                UpdateAdditionalDependencies(projectFile.ItemSpec);
            }

            return true;
        }

        private static ProjectItemDefinitionElement FindLinkNode(ProjectItemDefinitionGroupElement itemDefinitionGroup)
        {
            return itemDefinitionGroup.ItemDefinitions.FirstOrDefault(definition => definition.ItemType == "Link");
        }

        private static ProjectMetadataElement FindAdditionalDependenciesNode(ProjectItemDefinitionElement itemDefinition)
        {
            return itemDefinition.Metadata.FirstOrDefault(metadata => metadata.Name == "AdditionalDependencies");
        }

        private void UpdateAdditionalDependencies(string projectFile)
        {
            ProjectRootElement root = ProjectRootElement.Open(projectFile);
            bool isLinkElementFound = false;
            bool isAdditionalDependenciesElementFound = false;
            foreach (ProjectItemDefinitionGroupElement itemDefinitionGroup in root.ItemDefinitionGroups)
            {
                ProjectItemDefinitionElement itemDefinition = FindLinkNode(itemDefinitionGroup);
                if (itemDefinition == null)
                {
                    continue;
                }

                isLinkElementFound = true;
                ProjectMetadataElement metadata = FindAdditionalDependenciesNode(itemDefinition);
                if (metadata == null)
                {
                    continue;
                }

                isAdditionalDependenciesElementFound = true;
                UpdateAdditionalDependencies(metadata);
            }

            if (!isLinkElementFound)
            {
                Log.LogWarning("No Link elements found in {0}", projectFile);
            }
            else if (!isAdditionalDependenciesElementFound)
            {
                Log.LogWarning("No Link/AdditionalDependencies elements found in {0}", projectFile);
            }
            else
            {
                root.Save();
            }
        }

        private void UpdateAdditionalDependencies(ProjectMetadataElement metadata)
        {
            IList<string> additionalDependencies = metadata.Value.Split(';').ToList();
            string nodeDependency = additionalDependencies.FirstOrDefault(dependency => dependency.Contains("node.lib"));
            string newNodeDependency = string.Format("\"{0}\"", NewNodeLib);
            if (nodeDependency == null)
            {
                additionalDependencies.Add(newNodeDependency);
            }
            else
            {
                additionalDependencies[additionalDependencies.IndexOf(nodeDependency)] = newNodeDependency;
            }

            metadata.Value = string.Join(";", additionalDependencies);
        }
    }
}
