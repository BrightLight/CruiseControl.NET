using System.IO;
using Exortech.NetReflector;
using Microsoft.Practices.Unity;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.Core.Tasks
{
	[ReflectorType("merge")]
	public class MergeFilesTask
        : TaskBase, ITask
	{
        /// <summary>
        /// The folder to copy the files to.
        /// </summary>
        [ReflectorProperty("target", Required = false)]
        public string TargetFolder { get; set; }

        [ReflectorProperty("files", typeof(MergeFileSerialiserFactory))]
        public MergeFileInfo[] MergeFiles = new MergeFileInfo[0];

        /// <summary>
        /// Allows this task to interact with the file system in a testable way.
        /// </summary>
        [Dependency]
        public IFileSystem FileSystem { get; set; }

        /// <summary>
        /// Allows this task to interact with the logger in a testable way.
        /// </summary>
        [Dependency]
        public ILogger Logger { get; set; }

        /// <summary>
        /// Description used for the visualisation of the buildstage, if left empty the process name will be shown
        /// </summary>
        [ReflectorProperty("description", Required = false)]
        public string Description = string.Empty;

		public void Run(IIntegrationResult result)
		{
            result.BuildProgressInformation.SignalStartRunTask(Description != string.Empty ? Description : "Merging Files");

            // Make sure the target folder is rooted
            var targetFolder = TargetFolder;
            if (!string.IsNullOrEmpty(targetFolder))
            {
                if (!Path.IsPathRooted(targetFolder))
                {
                    targetFolder = Path.Combine(
                        Path.Combine(result.ArtifactDirectory, result.Label),
                        targetFolder);
                }
            }
            else
            {
                targetFolder = Path.Combine(result.ArtifactDirectory, result.Label);
            }

			foreach (var mergeFile in MergeFiles)
			{
                // Get the name of the file
				string fullMergeFile = mergeFile.FileName;
                if (!Path.IsPathRooted(fullMergeFile))
                {
                    fullMergeFile = Path.Combine(result.WorkingDirectory, fullMergeFile);
                }

                // Merge each file
				WildCardPath path = new WildCardPath(fullMergeFile);
                foreach (var fileInfo in path.GetFiles())
                {
                    if (FileSystem.FileExists(fileInfo.FullName))
                    {
                        if (mergeFile.MergeAction == MergeFileInfo.MergeActionType.Merge)
                        {
                            // Add the file to the merge list
                            Logger.Info("Merging file '{0}'", fileInfo);
                            result.AddTaskResultFromFile(fileInfo.FullName);
                        }
                        else
                        {
                            // Copy the file to the target folder
                            FileSystem.EnsureFolderExists(targetFolder);
                            Logger.Info("Copying file '{0}' to '{1}'", fileInfo.Name, targetFolder);
                            FileSystem.Copy(fileInfo.FullName, Path.Combine(targetFolder, fileInfo.Name));
                        }
                    }
                    else
                    {
                        Logger.Warning("File not found '{0}", fileInfo);
                    }
                }
			}
		}
	}
}
