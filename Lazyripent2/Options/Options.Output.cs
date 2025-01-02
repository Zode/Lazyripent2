namespace Lazyripent2;

public static partial class Options
{
	public static List<string> OutputFileFullPaths {get; private set;} = [];
	public static List<string> OutputDirectoryFullPaths {get; private set;} = [];

	/// <summary>
	/// Add an output path/file to the output options. Paths will be scanned for files.
	/// </summary>
	/// <param name="path">path to a file or directory</param>
	/// <exception cref="BadOptionException"></exception>
	public static void AddOutput(string path)
	{
		path = Path.GetFullPath(path);

		if(!IsAllowedOutputPathForMode(path, Mode))
		{
			throw new BadOptionException($"Output not allowed: \"{path}\"");
		}

		if(Directory.Exists(path))
		{
			OutputDirectoryFullPaths.Add(path);
			return;
		}
		else if(!Directory.Exists(path) && !File.Exists(path) && !Path.HasExtension(path))
		{
			if(PromptUser($"The path \"{path}\" does not exist. Do you want to create it?", PromptAllowedOptions.YesNoAlways, PromptOption.Yes))
			{
				Directory.CreateDirectory(path);
				OutputDirectoryFullPaths.Add(path);
			}
			else
			{
				throw new BadOptionException($"No output path: \"{path}\"");
			}

			return;
		}

		OutputFileFullPaths.Add(path);
	}

	/// <summary>
	/// Check if a given path is allowed for the given operation mode. 
	/// </summary>
	/// <param name="path">path to file or directory</param>
	/// <param name="mode">OperationMode to check</param>
	/// <returns>true if allowed</returns>
	private static bool IsAllowedOutputPathForMode(string path, OperationMode mode)
	{
		if(Directory.Exists(path) && !File.Exists(path))
		{
			return true;
		}

		//will prompt later if this is to be created or not
		if(!Directory.Exists(path) && !File.Exists(path) && !Path.HasExtension(path))
		{
			return true;
		}

		path = path.ToLower();
        return mode switch
        {
            OperationMode.ExportEntOnly => path.EndsWith(".ent"),
            OperationMode.ImportEntOnly => path.EndsWith(".bsp"),
            _ => path.EndsWith(".map") || path.EndsWith(".ent") || path.EndsWith(".bsp"),
        };
    }

	/// <summary>
	/// Make files in target path if necessary, will prompt for overwrites.
	/// </summary>
	/// <exception cref="BadOptionException"></exception>
	/// <exception cref="NotImplementedException"></exception>
	public static void MakeOutputs()
	{
		if(OutputDirectoryFullPaths.Count > 0 && OutputFileFullPaths.Count > 0)
		{
			throw new BadOptionException("Not allowed to define both output files and an output directory at the same time");
		}

		if(OutputDirectoryFullPaths.Count > 1)
		{
			throw new BadOptionException("Not allowed to define multiple output directories at the same time");
		}

		List<string> outputMapFullPaths = GetFilesOfType(OutputFileFullPaths, FileType.Map);
		List<string> outputEntFullPaths = GetFilesOfType(OutputFileFullPaths, FileType.Ent);
		List<string> outputBspFullPaths = GetFilesOfType(OutputFileFullPaths, FileType.Bsp);

		List<string> inputMapFullPaths = GetFilesOfType(InputFileFullPaths, FileType.Map);
		List<string> inputEntFullPaths = GetFilesOfType(InputFileFullPaths, FileType.Ent);
		List<string> inputBspFullPaths = GetFilesOfType(InputFileFullPaths, FileType.Bsp);

		if(OutputDirectoryFullPaths.Count > 0)
		{
			//input -> output directory, make up files if they don't exist, confirm overwrite if necessary
			switch(Mode)
			{
				case OperationMode.ApplyRuleFile:
				case OperationMode.StripFgdOnly:
					MakeOrOverwriteOutputs(inputMapFullPaths, ".map", ref outputMapFullPaths);
					MakeOrOverwriteOutputs(inputBspFullPaths, ".bsp", ref outputBspFullPaths);
					MakeOrOverwriteOutputs(inputEntFullPaths, ".ent", ref outputEntFullPaths);
					break;

				case OperationMode.ExportEntOnly:
					MakeOrOverwriteOutputs(inputBspFullPaths, ".ent", ref outputEntFullPaths);
					break;

				case OperationMode.ImportEntOnly:
					MakeOrOverwriteOutputs(inputEntFullPaths, ".bsp", ref outputBspFullPaths);
					break;

				default:
					throw new NotImplementedException($"Unsupported OperationMode {Mode}");
			}

			//apply changes
			OutputFileFullPaths = [..outputMapFullPaths, ..outputEntFullPaths, ..outputBspFullPaths];
			return;
		}

		switch(Mode)
		{
			case OperationMode.ApplyRuleFile:
			case OperationMode.StripFgdOnly:
				if(inputMapFullPaths.Count != outputMapFullPaths.Count)
				{
					throw new BadOptionException("Incorrect amount of .map output(s) for given .map input(s)");
				}

				if(inputEntFullPaths.Count != outputEntFullPaths.Count)
				{
					throw new BadOptionException("Incorrect amount of .ent output(s) for given .ent input(s)");
				}
				
				if(inputBspFullPaths.Count != outputBspFullPaths.Count)
				{
					throw new BadOptionException("Incorrect amount of .bsp output(s) for given .bsp input(s)");
				}

				break;

			case OperationMode.ExportEntOnly:
				if(inputBspFullPaths.Count != outputEntFullPaths.Count)
				{
					throw new BadOptionException("Incorrect amount of .ent output(s) for given .bsp input(s)");
				}
				
				break;

			case OperationMode.ImportEntOnly:
				if(inputEntFullPaths.Count != outputBspFullPaths.Count)
				{
					throw new BadOptionException("Incorrect amount of .bsp output(s) for given .ent input(s)");
				}
				
				break;

			default:
				throw new NotImplementedException($"Unsupported OperationMode {Mode}");
		}
		
		//input -> output, confirm overwrite if necessary
		switch(Mode)
		{
			case OperationMode.ApplyRuleFile:
			case OperationMode.StripFgdOnly:
				OverwriteOutputs(ref outputMapFullPaths);
				OverwriteOutputs(ref outputEntFullPaths);
				OverwriteOutputs(ref outputBspFullPaths);
				break;

			case OperationMode.ExportEntOnly:
				OverwriteOutputs(ref outputEntFullPaths);;
				break;

			case OperationMode.ImportEntOnly:
				OverwriteOutputs(ref outputBspFullPaths);
				break;

			default:
				throw new NotImplementedException($"Unsupported OperationMode {Mode}");
		}

		//apply changes
		OutputFileFullPaths = [..outputMapFullPaths, ..outputEntFullPaths, ..outputBspFullPaths];
	}

	/// <summary>
	/// Validate the given output options, otherwise exit application with an error. 
	/// </summary>
	/// <exception cref="BadOptionException"></exception>
	/// <exception cref="NotImplementedException"></exception>
	public static void ValidateOutputs()
	{
		List<string> mapFullPaths = GetFilesOfType(OutputFileFullPaths, FileType.Map);
		List<string> entFullPaths = GetFilesOfType(OutputFileFullPaths, FileType.Ent);
		List<string> bspFullPaths = GetFilesOfType(OutputFileFullPaths, FileType.Bsp);

		if(!mapFullPaths.GroupBy(x => x).All(g => g.Count() == 1))
		{
			throw new BadOptionException("Output map files contain duplicates");
		}

		if(!entFullPaths.GroupBy(x => x).All(g => g.Count() == 1))
		{
			throw new BadOptionException("Output ent files contain duplicates");
		}

		if(!bspFullPaths.GroupBy(x => x).All(g => g.Count() == 1))
		{
			throw new BadOptionException("Output bsp files contain duplicates");
		}

		switch(Mode)
		{
			case OperationMode.ApplyRuleFile:
			case OperationMode.StripFgdOnly:
				if(OutputFileFullPaths.Count == 0)
				{
					throw new BadOptionException("No output file(s) defined");
				}

				break;

			case OperationMode.ExportEntOnly:
				if(entFullPaths.Count == 0)
				{
					throw new BadOptionException("No output ent file(s) defined");
				}

				break;

			case OperationMode.ImportEntOnly:
				if(bspFullPaths.Count == 0)
				{
					throw new BadOptionException("No output bsp file(s) defined");
				}

				break;
			
			default:
				throw new NotImplementedException($"Unsupported OperationMode {Mode}");
		}

		foreach(string fullPath in bspFullPaths)
		{
			if(!File.Exists(fullPath))
			{
				throw new BadOptionException($"Output target bsp does not exist: \"{fullPath}\"");
			}
		}
	}

	/// <summary>
	/// Make output(s) or ask to overwrite output(s) for a given list of full paths
	/// </summary>
	/// <param name="fullInputPaths">List of full input paths to files</param>
	/// <param name="newExtension">New extension to tive to the made up file</param>
	/// <param name="fullOutputPaths">List of full output paths to files</param>
	/// <exception cref="BadOptionException"></exception>
	private static void MakeOrOverwriteOutputs(List<string> fullInputPaths, string newExtension, ref List<string> fullOutputPaths)
	{
		foreach(string fullInputPath in fullInputPaths)
		{
			string filename = Path.GetFileNameWithoutExtension(fullInputPath);
			string fullOutputPath = Path.Combine(OutputDirectoryFullPaths[0], filename + newExtension);

			if(string.IsNullOrWhiteSpace(filename) || string.IsNullOrWhiteSpace(fullOutputPath))
			{
				throw new BadOptionException($"Bad input path in output: \"{fullInputPath}\"");
			}

			if(!File.Exists(fullInputPath) && newExtension == ".bsp")
			{
				throw new BadOptionException($"Bad input path in output, not allowed to create bsp files from nothing: \"{fullInputPath}\"");
			}

			if(!File.Exists(fullOutputPath) ||
				PromptUser($"The file \"{fullOutputPath}\" already exists. Do you want to overwrite?", PromptAllowedOptions.YesNoAlways, PromptOption.Yes))
			{
				fullOutputPaths.Add(fullOutputPath);
			}
		}
	}

	/// <summary>
	/// Ask to overwrite outputs if necessary, and remove item if overwrite is not allowed.
	/// </summary>
	/// <param name="fullOutputPaths">List of full output paths</param>
	private static void OverwriteOutputs(ref List<string> fullOutputPaths)
	{
		for(int i = fullOutputPaths.Count - 1; i >= 0; i--)
		{
			if(!File.Exists(fullOutputPaths[i]) ||
				PromptUser($"The file \"{fullOutputPaths[i]}\" already exists. Do you want to overwrite?", PromptAllowedOptions.YesNoAlways, PromptOption.Yes))
			{
				continue;
			}

			fullOutputPaths.RemoveAt(i);
		}
	}
}