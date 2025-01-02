namespace Lazyripent2;

public static partial class Options
{
	public static List<string> InputFileFullPaths {get; private set;} = [];
	public static string FgdFullPath {get; private set;} = string.Empty;

	/// <summary>
	/// Add an input path/file to input options. Paths will be scanned for files.
	/// </summary>
	/// <param name="path">path to a file or directory</param>
	/// <exception cref="BadOptionException"></exception>
	public static void AddInput(string path)
	{
		path = Path.GetFullPath(path);

		if(!IsAllowedInputPathForMode(path, Mode))
		{
			throw new BadOptionException($"Input not allowed: \"{path}\"");
		}

		if(Directory.Exists(path))
		{
			foreach(string discoveredFile in Directory.GetFiles(path))
			{
				if(!IsAllowedInputPathForMode(discoveredFile, Mode))
				{
					continue;
				}
				
				AddInput(discoveredFile);
			}

			return;
		}
		
		if(!File.Exists(path))
		{
			throw new BadOptionException($"Input file or directory does not exist: \"{path}\"");
		}

		InputFileFullPaths.Add(path);
	}

	/// <summary>
	/// check if a given file is allowed for the given operation mode. 
	/// </summary>
	/// <param name="path">path to file</param>
	/// <param name="mode">OperationMode to check</param>
	/// <returns>true if allowed</returns>
	private static bool IsAllowedInputPathForMode(string path, OperationMode mode)
	{
		if(Directory.Exists(path) && !File.Exists(path))
		{
			return true;
		}

		path = path.ToLower();
        return mode switch
        {
            OperationMode.ExportEntOnly => path.EndsWith(".bsp"),
            OperationMode.ImportEntOnly => path.EndsWith(".ent"),
            OperationMode.StripFgdOnly => path.EndsWith(".map") || path.EndsWith(".ent") || path.EndsWith(".bsp"),
            _ =>  path.EndsWith(".rule") ||  path.EndsWith(".map") || path.EndsWith(".ent") || path.EndsWith(".bsp"),
        };
    }

	/// <summary>
	/// Set the FGD File path.
	/// </summary>
	/// <param name="path">path to .fgd file</param>
	public static void SetFgdPath(string path)
	{
		path = Path.GetFullPath(path);

		if(!File.Exists(path))
		{
			throw new BadOptionException($"fgd file does not exist: \"{path}\"");
		}

		//stop accidental .ent/.bsp/.map in fgd switch
		if(!path.ToLower().EndsWith(".fgd"))
		{
			throw new BadOptionException($"fgd file is not a fgd file: \"{path}\"");
		}

		if(UnattendedMode)
		{
			FgdFullPath = path;
			return;
		}

		if(PromptUser("Stripping the default fgd keyvalues from the level file can break the level. Do you want to continue?",
			PromptAllowedOptions.YesNo, PromptOption.No))
		{
			FgdFullPath = path;
		}
	}

	/// <summary>
	/// Validate the given input options
	/// </summary>
	/// <exception cref="BadOptionException"></exception>
	/// <exception cref="NotImplementedException"></exception>
	public static void ValidateInputs()
	{
		List<string> ruleFullPaths = GetFilesOfType(InputFileFullPaths, FileType.Rule);
		List<string> mapFullPaths = GetFilesOfType(InputFileFullPaths, FileType.Map);
		List<string> entFullPaths = GetFilesOfType(InputFileFullPaths, FileType.Ent);
		List<string> bspFullPaths = GetFilesOfType(InputFileFullPaths, FileType.Bsp);

		if(!ruleFullPaths.GroupBy(x => x).All(g => g.Count() == 1))
		{
			throw new BadOptionException("Input rule files contain duplicates");
		}

		if(!entFullPaths.GroupBy(x => x).All(g => g.Count() == 1))
		{
			throw new BadOptionException("Input ent files contain duplicates");
		}

		if(!mapFullPaths.GroupBy(x => x).All(g => g.Count() == 1))
		{
			throw new BadOptionException("Input map files contain duplicates");
		}

		if(!bspFullPaths.GroupBy(x => x).All(g => g.Count() == 1))
		{
			throw new BadOptionException("Input bsp files contain duplicates");
		}

		switch(Mode)
		{
			case OperationMode.ApplyRuleFile:
				if(ruleFullPaths.Count == 0)
				{
					throw new BadOptionException("No input rule file(s) defined");
				}

				if(GetFilesNotOfType(InputFileFullPaths, FileType.Rule).Count == 0)
				{
					throw new BadOptionException("No input target file(s) defined");
				}

				break;

			case OperationMode.ExportEntOnly:
				if(bspFullPaths.Count == 0)
				{
					throw new BadOptionException("No input bsp file(s) defined");
				}

				break;

			case OperationMode.ImportEntOnly:
				if(entFullPaths.Count == 0)
				{
					throw new BadOptionException("No input ent file(s) defined");
				}

				break;

			case OperationMode.StripFgdOnly:
				if(string.IsNullOrWhiteSpace(FgdFullPath))
				{
					throw new BadOptionException("No input fgd file defined");
				}

				if(GetFilesNotOfType(InputFileFullPaths, FileType.Rule).Count == 0)
				{
					throw new BadOptionException("No input target file(s) defined");
				}
				
				break;
				
			default:
				throw new NotImplementedException($"Unsupported OperationMode {Mode}");
		} 
	}
}