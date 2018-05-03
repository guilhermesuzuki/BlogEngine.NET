using System.Collections.Generic;

namespace BlogEngine.Core.FileSystem
{
	public class FileResponse
	{
		public FileResponse()
		{
			Files = new List<FileInstance>();
			Path = string.Empty;
		}
		public IEnumerable<FileInstance> Files { get; set; }
		public string Path { get; set; }
	}

	public class FileInstance
	{
		public bool IsChecked { get; set; }
		public int SortOrder { get; set; }
		public string Created { get; set; }
		public string Name { get; set; }
		public string FileSize { get; set; }
		public FileType FileType { get; set; }
		public string FilePath { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string FullPath
		{
			get
			{
				switch (this.FileType)
				{
					case FileType.File:
					case FileType.Image:
						return $"{Utils.RelativeWebRoot.TrimEnd('/')}/files/{this.FilePath.TrimStart('/')}";
					default:
						return this.FilePath;
				}
			}
		}

		/// <summary>
		/// HTML Tag, ready to be used by the HTML editor
		/// </summary>
		public string HTML
		{
			get
			{
				switch (this.FileType)
				{
					case FileType.Image:
						return $"<img src='{this.FullPath}' alt='{this.Name} ({this.FileSize})' title='{this.Name}' />";
					case FileType.File:
						return $"<p><a href='{this.FullPath}' target='_blank' alt='{this.Name} ({this.FileSize})' title='{this.Name}'>{this.Name} ({this.FileSize})</a></p>";
				}

				return string.Empty;
			}
		}

		/// <summary>
		/// Image assosiated with file extension
		/// </summary>
		public string ImgPlaceholder
		{
			get
			{
				switch (this.FileType)
				{
					case FileType.File:
						return getPlaceholder(Name);
				}
				
				return string.Empty;
			}
		}

		static string getPlaceholder(string name)
		{
			var file = name.ToLower().Trim();

			if (file.EndsWith(".zip") || file.EndsWith(".gzip") || file.EndsWith(".7zip") || file.EndsWith(".rar"))
			{
				return "fas fa-file-archive";
			}
			if (file.EndsWith(".doc") || file.EndsWith(".docx"))
			{
				return "fas fa-file-word";
			}
			if (file.EndsWith(".xls") || file.EndsWith(".xlsx"))
			{
				return "fas fa-file-excel";
			}
			if (file.EndsWith(".pdf"))
			{
				return "fas fa-file-pdf";
			}
			if (file.EndsWith(".txt"))
			{
				return "fas fa-file-alt";
			}

			return "fas fa-file";
		}

	}

	public enum FileType
	{
		Directory,
		File,
		Image,
		None
	}
}
