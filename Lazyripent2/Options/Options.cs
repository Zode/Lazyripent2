using System.Globalization;
using System.Text;

namespace Lazyripent2;

public static partial class Options
{
	private static Dictionary<string, BindOption> _bindSet = [];
	public static OperationMode Mode {get; private set;} = OperationMode.None;
	public static bool UnattendedMode {get; set;} = false;
	public static bool TemporaryUnattendedMode {get; set;} = false;
	public static bool Verbose {get; set;} = false;
	public static bool WarningsAsFatal {get; set;} = false;

	/// <summary>
	/// Print output settings to console if verbose mode is enabled.
	/// </summary>
	public static void Display()
	{
		if(!Verbose)
		{
			return;
		}

		Console.WriteLine("============");
		switch(Mode)
		{
			case OperationMode.ApplyRuleFile:
				Console.WriteLine("Operation mode: apply rule file(s)");
				break;

			case OperationMode.ExportEntOnly:
				Console.WriteLine("Operation mode: export .ent files only");
				break;

			case OperationMode.ImportEntOnly:
				Console.WriteLine("Operation mode: import .ent files only");
				break;

			case OperationMode.StripFgdOnly:
				Console.WriteLine("Operation mode: strip default values with .fgd only");
				break;

			default:
				throw new NotImplementedException($"Unsupported OperationMode {Mode}");
		}

		Console.WriteLine($"Unattended mode: {UnattendedMode}");
		Console.WriteLine($"Warnings as fatal: {WarningsAsFatal}");

		if(string.IsNullOrWhiteSpace(FgdFullPath))
		{
			Console.WriteLine("Fgd file: none");
		}
		else
		{
			Console.WriteLine($"Fgd file: {Path.GetFileName(FgdFullPath)}");
		}

		List<string> outputMapFullPaths = GetFilesOfType(OutputFileFullPaths, FileType.Map);
		List<string> outputEntFullPaths = GetFilesOfType(OutputFileFullPaths, FileType.Ent);
		List<string> outputBspFullPaths = GetFilesOfType(OutputFileFullPaths, FileType.Bsp);

		List<string> inputRuleFullPaths = GetFilesOfType(InputFileFullPaths, FileType.Rule);
		List<string> inputMapFullPaths = GetFilesOfType(InputFileFullPaths, FileType.Map);
		List<string> inputEntFullPaths = GetFilesOfType(InputFileFullPaths, FileType.Ent);
		List<string> inputBspFullPaths = GetFilesOfType(InputFileFullPaths, FileType.Bsp);

		Console.WriteLine("============");
		Console.WriteLine($"Input rule files: {ConstructDisplayStringForPaths(inputRuleFullPaths)}");
		Console.WriteLine($"Input map files: {ConstructDisplayStringForPaths(inputMapFullPaths)}");
		Console.WriteLine($"Input ent files: {ConstructDisplayStringForPaths(inputEntFullPaths)}");
		Console.WriteLine($"Input bsp files: {ConstructDisplayStringForPaths(inputBspFullPaths)}");
		Console.WriteLine("============");
		Console.WriteLine($"Output map files: {ConstructDisplayStringForPaths(outputMapFullPaths)}");
		Console.WriteLine($"Output ent files: {ConstructDisplayStringForPaths(outputEntFullPaths)}");
		Console.WriteLine($"Output bsp files: {ConstructDisplayStringForPaths(outputBspFullPaths)}");
		Console.WriteLine("============");
	}

	/// <summary>
	/// Produce a nice to display string from list of full paths.
	/// </summary>
	/// <param name="fullPaths">List of full paths</param>
	/// <returns>"none" or list of filenames with extensions</returns>
	private static string ConstructDisplayStringForPaths(List<string> fullPaths)
	{
		if(fullPaths.Count == 0)
		{
			return "none";
		}

		StringBuilder sb = new();
		for(int i = 0; i < fullPaths.Count; i++)
		{
			sb.Append(Path.GetFileName(fullPaths[i]));

			if(i != fullPaths.Count - 1)
			{
				sb.Append(", ");
			}
		}

		return sb.ToString();
	}

	/// <summary>
	/// Prompt the user for confirmation, will halt application until proper answer is given unless UnattendedMode is enabled.
	/// Will enable TemporaryUnattendedMode if "always" is chosen.
	/// </summary>
	/// <param name="promptMessage">Message shown</param>
	/// <param name="options">Options presented</param>
	/// <param name="defaultOption">Default choice when enter is hit</param>
	/// <returns>True if allowed by user, or if either UnattendedMode or TemporaryUnattendedMode are enabled</returns>
	/// <exception cref="NotImplementedException"></exception>
	public static bool PromptUser(string promptMessage, PromptAllowedOptions options, PromptOption defaultOption)
	{
		if(UnattendedMode || TemporaryUnattendedMode)
		{
			return true;
		}

		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.Write(promptMessage);
		string optionsString = options switch
        {
            PromptAllowedOptions.YesNo => " [y/n] ",
            PromptAllowedOptions.YesNoAlways => " [y/n/a] ",
            _ => throw new NotImplementedException($"Unsupported PromptOption {options}"),
        };

        optionsString = defaultOption switch
        {
            PromptOption.Yes => optionsString.Replace('y', 'Y'),
            PromptOption.No => optionsString.Replace('n', 'N'),
            PromptOption.Always => optionsString.Replace('a', 'A'),
            _ => throw new NotImplementedException($"Unsupportd PromptOptionsDefault {defaultOption}"),
        };

		Console.Write(optionsString);
		PromptOption chosenOption = defaultOption;
		restartInputLoop:
		while(true)
		{
			ConsoleKeyInfo keyInfo = Console.ReadKey(false);
			switch(keyInfo.Key)
			{
				case ConsoleKey.Y:
					chosenOption = PromptOption.Yes;
					goto breakInputLoop;

				case ConsoleKey.N:
					chosenOption = PromptOption.No;
					goto breakInputLoop;

				case ConsoleKey.A:
					chosenOption = PromptOption.Always;
					goto breakInputLoop;

				case ConsoleKey.Enter:
					goto breakInputLoop;
			}
		}

		breakInputLoop:
		bool returnBool = false;
		switch(options)
		{
			case PromptAllowedOptions.YesNo:
				switch(chosenOption)
				{
					case PromptOption.Yes:
						returnBool = true;
						break;

					case PromptOption.No:
						returnBool = false;
						break;

					default:
						goto restartInputLoop;
				}

				break;

			case PromptAllowedOptions.YesNoAlways:
				switch(chosenOption)
				{
					case PromptOption.Yes:
						returnBool = true;
						break;

					case PromptOption.No:
						returnBool = false;
						break;

					case PromptOption.Always:
						TemporaryUnattendedMode = true;
						returnBool = true;
						break;

					default:
						goto restartInputLoop;
				}

				break;

			default:
				throw new NotImplementedException($"Unsupported PromptAllowedOptions {options}");
		}

		Console.ForegroundColor = ConsoleColor.White;
		Console.Write('\n');
		return returnBool;
	}

	/// <summary>
	/// return a list of filetype for a given list
	/// </summary>
	/// <param name="fullFilePaths"></param>
	/// <param name="fileType"></param>
	/// <returns></returns>
	public static List<string> GetFilesOfType(List<string> fullFilePaths, FileType fileType)
	{
		return fullFilePaths.Where(x => x.ToLower().EndsWith(fileType.GetExtension())).ToList();
	}

	/// <summary>
	/// return a list of every other filetype for a given list
	/// </summary>
	/// <param name="fullFilePaths"></param>
	/// <param name="fileType"></param>
	/// <returns></returns>
	public static List<string> GetFilesNotOfType(List<string> fullFilePaths, FileType fileType)
	{
		return fullFilePaths.Where(x => !x.ToLower().EndsWith(fileType.GetExtension())).ToList();
	}

	/// <summary>
	/// Bind command line options
	/// </summary>
	/// <param name="binds"></param>
	/// <exception cref="ArgumentException"></exception>
	public static void Bind(List<BindOption> binds)
	{
		foreach(BindOption bind in binds)
		{
			if(string.IsNullOrWhiteSpace(bind.PrimaryBind))
			{
				throw new ArgumentException($"Primary option bind must be something other than null or whitespace"); 
			}
		
			if(string.IsNullOrWhiteSpace(bind.SecondaryBind))
			{
				throw new ArgumentException($"Secondary option bind must be something other than null or whitespace"); 
			}
			
			if(bind.PrimaryBind[0] != '-')
			{
				throw new ArgumentException($"Primary option bind \"{bind.PrimaryBind}\" must start with a hyphen"); 
			}

			if(bind.SecondaryBind[0] != '-' && bind.SecondaryBind[1] != '-')
			{
				throw new ArgumentException($"Secondary option bind \"{bind.PrimaryBind}\" must start with two hyphens"); 
			}

			if(_bindSet.ContainsKey(bind.PrimaryBind))
			{
				throw new ArgumentException($"Primary option bind \"{bind.PrimaryBind}\" already exists"); 
			}

			if(_bindSet.ContainsKey(bind.SecondaryBind))
			{
				throw new ArgumentException($"Secondary option bind \"{bind.PrimaryBind}\" already exists"); 
			}

			_bindSet.Add(bind.PrimaryBind, bind);
			_bindSet.Add(bind.SecondaryBind, bind);
		}
	}

	/// <summary>
	/// Set the operation mode
	/// </summary>
	/// <param name="mode"></param>
	/// <exception cref="BadOptionException"></exception>
	public static void SetMode(OperationMode mode)
	{
		if(Mode != OperationMode.None)
		{
			throw new BadOptionException("Operation mode is ambiguous, you are defining multiple operation modes");
		}

		Mode = mode;
	}

	/// <summary>
	/// Process the command line
	/// </summary>
	/// <param name="args"></param>
	/// <exception cref="OptionsException"></exception>
	public static void Process(string[] args)
	{
		//split the options if they are somehow fed as a single string (eg. "-i test.map" -> "-i", "test.map")
		List<string> newArgs = [];
		foreach(string arg in args)
		{
			string[] splitArg = arg.Split(' ');
			if(splitArg.Length <= 1)
			{
				newArgs.Add(arg);
				continue;
			}

			if(!_bindSet.TryGetValue(splitArg[0], out BindOption? bind))
			{
				newArgs.Add(arg);
				continue;
			}

			foreach(string newArg in splitArg)
			{
				newArgs.Add(newArg);
			}
		}

		for(int i = 0; i < newArgs.Count; i++)
        {
			string arg = newArgs[i].ToLower();

			if(!_bindSet.TryGetValue(arg, out BindOption? bind))
			{
				throw new OptionsException($"Unknown option \"{newArgs[i]}\"");
			}

			if(bind.Consume == 0)
			{
				bind.Action(null);
				continue;
			}

			if(i + bind.Consume >= newArgs.Count)
			{
				throw new OptionsException($"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(bind.SecondaryBind.Replace("-", ""))} is missing an arugment");
			}

			for(int j = 1; j < bind.Consume - 1; j++)
			{
				if(newArgs[i + j].StartsWith('-'))
				{
					throw new OptionsException($"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(bind.SecondaryBind.Replace("-", ""))} is missing an arugment");
				}
			}

			if(bind.Consume == 1)
			{
				bind.Action(newArgs[i + 1]);
				i++;
				continue;
			}

			List<string> argsToBind = [];
			for(int j = 1; j < bind.Consume - 1; j++)
			{
				argsToBind.Add(newArgs[i + j]);
			}

			bind.Action(argsToBind);
			i += bind.Consume;
		}
	}
	
	/// <summary>
	/// Display options and their use
	/// </summary>
	public static void DisplayUsage()
	{
		foreach(string key in _bindSet.Keys)
		{
			//skip secondarys
			if(key[1] == '-')
			{
				continue;
			}

			BindOption bind = _bindSet[key];
			StringBuilder sb = new();
			for(int i = 0; i < bind.Consume; i++)
			{
				sb.Append("arg");
				if(i < bind.Consume - 1)
				{
					sb.Append(' ');
				}
			}
			string argString = sb.ToString();
			string primary = $"{bind.PrimaryBind} {argString}";
			string secondary = $"{bind.SecondaryBind} {argString}";
			Console.WriteLine($"{primary,-6}  or {secondary,-20}  {bind.Description}");
		}
	}
}