namespace tretton37.RunCommandOnSave.LessAutoCompiler
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Microsoft.VisualStudio;
	using Microsoft.VisualStudio.Shell.Interop;

	public class LessFileOnSaveListener
	   : IVsFileChangeEvents, IDisposable
	{
		private const uint FileChangeFlags = (uint)_VSFILECHANGEFLAGS.VSFILECHG_Add | (uint)_VSFILECHANGEFLAGS.VSFILECHG_Del | (uint)_VSFILECHANGEFLAGS.VSFILECHG_Size | (uint)_VSFILECHANGEFLAGS.VSFILECHG_Time;
		private readonly SolutionFilesService solutionFilesService;
		private readonly IOnSaveTaskDispatcher onSaveTaskDispatcher;
		private readonly IVsFileChangeEx fileChangeService;
		private readonly IList<MonitoredFile<ProjectFile>> monitoredLessFiles = new List<MonitoredFile<ProjectFile>>();
		private readonly IList<MonitoredFile> monitoredProjects = new List<MonitoredFile>();

		public LessFileOnSaveListener(IVsFileChangeEx fileChangeService, SolutionFilesService solutionFilesService, IOnSaveTaskDispatcher onSaveTaskDispatcher)
		{
			this.solutionFilesService = solutionFilesService;
			this.onSaveTaskDispatcher = onSaveTaskDispatcher;
			this.fileChangeService = fileChangeService;


			this.MonitorLessFileChanges();
			this.MonitorProjectChanges();

		}

		public int FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
		{
			if (rggrfChange.Any(changeType => (uint)_VSFILECHANGEFLAGS.VSFILECHG_Del == changeType))
			{
				this.UnmonitorDeletedLessFiles(rgpszFile);
			}


			this.HandleProjectUpdate(rgpszFile);
			this.HandleLessFileChange(rgpszFile);

			return VSConstants.S_OK;
		}

		public int DirectoryChanged(string pszDirectory)
		{
			return VSConstants.S_OK;
		}

		public void Dispose()
		{
			var cookies = this.monitoredLessFiles.Select(lessFile => lessFile.MonitorCookie)
				  .Concat(this.monitoredProjects.Select(projectFile => projectFile.MonitorCookie));

			foreach (var monitoredFile in cookies)
			{
				this.ForgetFile(monitoredFile);
			}
		}

		private void UnmonitorDeletedLessFiles(IEnumerable<string> deletedFiles)
		{
			var deletedLessFiles = (from deletedFile in deletedFiles
									from monitoredLessFile in this.monitoredLessFiles
									where monitoredLessFile.PathMatches(deletedFile)
									select monitoredLessFile).ToArray();

			foreach (var deletedLessFile in deletedLessFiles)
			{
				this.ForgetFile(deletedLessFile.MonitorCookie);
			}
		}

		private void HandleProjectUpdate(IEnumerable<string> changedFiles)
		{
			var updatedProjects = this.solutionFilesService.Projects
				.Where(project => changedFiles.Contains(project.ProjectFilePath));

			if (updatedProjects.Any())
			{
				this.MonitorLessFileChanges();
			}
		}


		private void HandleLessFileChange(IEnumerable<string> changedFiles)
		{
			var projectPaths = (from changedFile in changedFiles
								where this.LessFileIsMonitored(changedFile)
								let watchedFile = this.FindMonitoredLessFile(changedFile)
								select watchedFile.ProjectPath).Distinct();

			foreach (var projectPath in projectPaths)
			{
				var projectDirectory = Path.GetDirectoryName(projectPath);
				this.onSaveTaskDispatcher.QueueOnChangeTaskFor(projectDirectory);
			}
		}

		private void MonitorProjectChanges()
		{
			var projectFiles = from project in this.solutionFilesService.Projects
							   let projectPath = project.ProjectFilePath
							   where !this.monitoredProjects.Any(monitoredProject => monitoredProject.PathMatches(projectPath))
							   select projectPath;

			foreach (var projectFile in projectFiles)
			{
				if (!this.ProjectFileIsMonitored(projectFile))
				{
					var cookie = this.MonitorFile(projectFile);

					this.monitoredProjects.Add(new MonitoredFile(cookie, projectFile));
				}
			}
		}

		private void MonitorLessFileChanges()
		{
			var unwatchedLessFiles = from project in this.solutionFilesService.Projects
									 from file in project.Files
									 where file.FilePath.EndsWith(".less")
									 where !this.LessFileIsMonitored(file)
									 select file;

			foreach (var lessFile in unwatchedLessFiles)
			{
				if (!this.LessFileIsMonitored(lessFile))
				{
					var cookie = this.MonitorFile(lessFile.FilePath);
					this.monitoredLessFiles.Add(new MonitoredFile<ProjectFile>(cookie, lessFile.FilePath, lessFile));
				}
			}
		}

		private uint MonitorFile(string filePath)
		{
			uint cookie;
			this.fileChangeService.AdviseFileChange(filePath, FileChangeFlags, this, out cookie);
			return cookie;
		}

		private void ForgetFile(uint cookie)
		{
			this.fileChangeService.UnadviseFileChange(cookie);

			var matchingLessFile = this.monitoredLessFiles.SingleOrDefault(lessFile => lessFile.MonitorCookie == cookie);
			var matchingProjectFile = this.monitoredProjects.SingleOrDefault(projectFile => projectFile.MonitorCookie == cookie);

			if (matchingLessFile != null)
			{
				this.monitoredLessFiles.Remove(matchingLessFile);
			}

			if (matchingProjectFile != null)
			{
				this.monitoredProjects.Remove(matchingProjectFile);
			}
		}



		private ProjectFile FindMonitoredLessFile(string filePath)
		{
			var monitoredFile = this.monitoredLessFiles.SingleOrDefault(lessFile => lessFile.PathMatches(filePath));
			if (monitoredFile == null)
			{
				return null;
			}
			return monitoredFile.FileData;
		}

		private bool LessFileIsMonitored(string filePath)
		{
			return this.monitoredLessFiles.Any(watchedFile => watchedFile.PathMatches(filePath));
		}

		private bool ProjectFileIsMonitored(string filePath)
		{
			return this.monitoredProjects.Any(watchedFile => watchedFile.PathMatches(filePath));
		}

		private bool LessFileIsMonitored(ProjectFile file)
		{
			return this.LessFileIsMonitored(file.FilePath);
		}
	}
}
