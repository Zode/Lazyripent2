using Lazyripent2.Bsp;
using Lazyripent2.Ent;
using Lazyripent2.Fgd;
using Lazyripent2.Lexer;
using Lazyripent2.Map;
using Lazyripent2.Rule;

namespace Lazyripent2;

public class Program
{
    private static FgdFile? _fgdFile = null;
    private static List<RuleFile> _rules = [];

    public static int Main(string[] args)
    {
        Console.WriteLine("Lazyripent 2.0.0");
        try
        {
            Options.Bind([
                new BindOption("-i", "--input", "input may be a .rule file, .map file, .ent file, .bsp file, or a folder containing .map, .ent, or .bsp files", 1, arg => {
                    if(arg is not null && arg is string value)
                    {
                        Options.AddInput(value);
                    }}),
                new BindOption("-o", "--output", "output may be a .map file, .ent file, .bsp file, or a folder", 1, arg => {
                    if(arg is not null && arg is string value)
                    {
                        Options.AddOutput(value);
                    }}),
                new BindOption("-s", "--strip", "strip output file(s) from default values as assigned in the .fgd file", 1, arg => {
                    if(arg is not null && arg is string value)
                    {
                        Options.SetFgdPath(value);
                    }}),
                new BindOption("-ee", "--export-ent-only", "export .ent file(s) from .bsp file(s) only instead of applying rules", 0, arg => Options.SetMode(OperationMode.ExportEntOnly)),
                new BindOption("-ie", "--import-ent-only", "import .ent file(s) to .bsp file(s) only instead of applying rules", 0, arg => Options.SetMode(OperationMode.ImportEntOnly)),
                new BindOption("-ss", "--strip-only", "strip output file(s) only instead of applying rules", 0, arg => Options.SetMode(OperationMode.StripFgdOnly)),
                new BindOption("-u", "--unattended", "automatically confirm changes instead of asking before applying them", 0, arg => Options.UnattendedMode = true),
                new BindOption("-v", "--verbose", "produce more verbose output", 0, arg => Options.Verbose = true),
                new BindOption("-w", "--warnings-as-fatal", "treat all warnings as fatal and stop the program", 0, arg => Options.WarningsAsFatal = true),
            ]);
            
            if(args.Length == 0)
            {
                ShowUsage();
                return (int)ExitCodes.Success;
            }
       
            HandleOptions(args);
            LoadFGD();
            LoadRules();
            switch(Options.Mode)
            {
                case OperationMode.ExportEntOnly:
                    ExportEntsFromBspFiles();
                    break;

                case OperationMode.ImportEntOnly:
                    ImportEntsToBspFiles();
                    break;

                case OperationMode.StripFgdOnly:
                    StripFgdFromFiles(Options.GetFilesOfType(Options.InputFileFullPaths, FileType.Map),
                        Options.GetFilesOfType(Options.OutputFileFullPaths, FileType.Map));

                    StripFgdFromFiles(Options.GetFilesOfType(Options.InputFileFullPaths, FileType.Ent),
                        Options.GetFilesOfType(Options.OutputFileFullPaths, FileType.Ent));

                    StripFgdFromFiles(Options.GetFilesOfType(Options.InputFileFullPaths, FileType.Bsp),
                        Options.GetFilesOfType(Options.OutputFileFullPaths, FileType.Bsp));
                    break;

                case OperationMode.ApplyRuleFile:
                    //this also strips fgd if applicable
                    ApplyRulesToFiles(Options.GetFilesOfType(Options.InputFileFullPaths, FileType.Map),
                        Options.GetFilesOfType(Options.OutputFileFullPaths, FileType.Map));

                    ApplyRulesToFiles(Options.GetFilesOfType(Options.InputFileFullPaths, FileType.Ent),
                        Options.GetFilesOfType(Options.OutputFileFullPaths, FileType.Ent));

                    ApplyRulesToFiles(Options.GetFilesOfType(Options.InputFileFullPaths, FileType.Bsp),
                        Options.GetFilesOfType(Options.OutputFileFullPaths, FileType.Bsp));
                    break;
            }
        }
        catch(OptionsException ex)
        {
            ExitCode.InvalidCommandLine(ex.Message);
        }
        catch(BadOptionException ex)
        {
            ExitCode.Failure(ex);
        }
        catch(LexerException ex)
        {
            ExitCode.Failure(ex, ex.Caret, ex.LexerSource);
        }
        catch(FileParserException ex)
        {
            if(ex.Lexer is null)
            {
                ExitCode.Failure(ex);
            }
            else
            {
                ExitCode.Failure(ex, ex.Caret, ex.Lexer, ex.TokenIndex);
            }
        }
        catch(FileFormatException ex)
        {
            if(ex.FileParser is null)
            {
                ExitCode.Failure(ex);
            }
            else
            {
                ExitCode.Failure(ex, ex.FileParser);
            }
        }
        catch(RuleBlockException ex)
        {
            ExitCode.Failure(ex, ex.Caret, ex.RuleSource);
        }
        catch(Exception ex) //generic catch-all for everything else
        {
            ExitCode.Failure(ex);
        }

        Console.WriteLine("OK");
        return (int)ExitCodes.Success;
    }

    public static void ShowUsage()
    {
        Console.WriteLine("Usage: lazyripent [options]\n");
        Console.WriteLine(".ent files produced by this program are most likely incompatible with other ripent tools.\n");
        Console.WriteLine("options:");
        Options.DisplayUsage();
        Console.WriteLine("\nexamples:");
        Console.WriteLine("lazyripent -i ./fixspawns.rule -i ./broken_bsp -o ./fixed_bsp");
        Console.WriteLine("\tapply fixspawns.rule ruleset to folder \"broken_bsp\" and write results out to folder \"fixed_bsp\"");
        Console.WriteLine("lazyripent -ee -i ./map1.bsp -i ./map2.bsp -o ./ents");
        Console.WriteLine("\texport .ent files from both bsp files and write them out to folder \"ents\"");
        Console.WriteLine("lazyripent -ss -s ./halflife.fgd -i ./map1.map -o ./map1.map -u");
        Console.WriteLine("\tstrip map1.map file from fgd default keyvalue entries and write it back to the same file in unattended mode");
        Console.WriteLine("\tNote: fgd default entries may not match the game's code, so this may break the level");
    }

    /// <summary>
    /// Hacky man's option handling
    /// </summary>
    /// <param name="args">options</param>
    public static void HandleOptions(string[] args)
    {
        Options.Process(args);

        //by default, we apply rules
        if(Options.Mode == OperationMode.None)
        {
            Options.SetMode(OperationMode.ApplyRuleFile);
        }

        Options.ValidateInputs();
        Options.TemporaryUnattendedMode = false;
        Options.MakeOutputs();
        Options.ValidateOutputs();
        Options.Display();
    }

    public static void ShowWarning(string message, FileParser? fileParser = null)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Warning: {message}");
        Console.ForegroundColor = ConsoleColor.White;

        ExitCode.CheckWarningAsFatal(fileParser);
    }

    private static void LoadFGD()
    {
        if(string.IsNullOrWhiteSpace(Options.FgdFullPath))
        {
            return;
        }

        _fgdFile = new();
        _fgdFile.DeserializeFromFile(Options.FgdFullPath);
    }

    private static void Upgrade(IParseableFile file)
    {
        file.SetEntities(EntityUpgrader.UpgradeKeyValues(file.GetEntities(), out int affected));
        if(Options.Verbose)
		{
			Console.WriteLine($"Upgraded {affected} key(s)");
		}
    }

    private static void StripFGD(IParseableFile file)
    {
        if(_fgdFile is null)
        {
            return;
        }

        file.SetEntities(_fgdFile.StripDefaultKeyvaluesFromEntities(file.GetEntities(), out int stripped));
        Console.WriteLine($"Stripped total of {stripped} keyvalues");
    }

    private static void LoadRules()
    {
		List<string> ruleFullPaths = Options.GetFilesOfType(Options.InputFileFullPaths, FileType.Rule);
        for(int i = 0; i <ruleFullPaths.Count; i++)
        {
            Console.WriteLine($"Processing \"{ruleFullPaths[i]}\"");
            RuleFile ruleFile = new();
            ruleFile.DeserializeFromFile(ruleFullPaths[i]);
            _rules.Add(ruleFile);
        }
    }

    private static void ApplyRules(IParseableFile file)
    {
        for(int i = 0; i < _rules.Count; i++)
        {
            file.SetEntities(_rules[i].ApplyRules(file.GetFileNameNoExt(), file.GetEntities()));
        }
    }

    private static void ExportEntsFromBspFiles()
    {
        List<string> inputBspFullPaths = Options.GetFilesOfType(Options.InputFileFullPaths, FileType.Bsp);
        List<string> outputEntFullPaths = Options.GetFilesOfType(Options.OutputFileFullPaths, FileType.Ent);

        for(int i = 0; i < inputBspFullPaths.Count; i++)
        {
            Console.WriteLine($"Processing \"{inputBspFullPaths[i]}\" -> \"{outputEntFullPaths[i]}\"");

            BspFile bspFile = new();
            bspFile.DeserializeFromFile(inputBspFullPaths[i]);
            Upgrade(bspFile);
            StripFGD(bspFile);

            EntFile entFile = new();
            entFile.SetEntities(bspFile.GetEntities());
            entFile.SerializeToFile(outputEntFullPaths[i]);
        }
    }

    private static void ImportEntsToBspFiles()
    {
        List<string> inputEntFullPaths = Options.GetFilesOfType(Options.InputFileFullPaths, FileType.Ent);
        List<string> outputBspFullPaths = Options.GetFilesOfType(Options.OutputFileFullPaths, FileType.Bsp);

        for(int i = 0; i < inputEntFullPaths.Count; i++)
        {
            Console.WriteLine($"Processing \"{inputEntFullPaths[i]}\" -> \"{outputBspFullPaths[i]}\"");

            EntFile entFile = new();
            entFile.DeserializeFromFile(inputEntFullPaths[i]);
            Upgrade(entFile);
            StripFGD(entFile);

            BspFile bspFile = new();
            bspFile.DeserializeFromFile(outputBspFullPaths[i]);
            bspFile.SetEntities(entFile.GetEntities());
            bspFile.SerializeToFile(outputBspFullPaths[i]);
        }
    }

    private static IParseableFile NewIPraseableFileForPath(string path)
    {
        IParseableFile file;
        path = path.ToLower();
        if(path.EndsWith(".map"))
        {
            file = new MapFile();
        }
        else if(path.EndsWith(".ent"))
        {
            file = new EntFile();
        }
        else if(path.EndsWith(".bsp"))
        {
            file = new BspFile();
        }
        else
        {
            throw new NotImplementedException($"Tried to make new unsupported IParseableFile for path \"{path}\"");
        }

        return file;
    }

    private static void StripFgdFromFiles(List<string> inputFullPaths, List<string> outputFullPaths)
    {
        for(int i = 0; i < inputFullPaths.Count; i++)
        {
            Console.WriteLine($"Processing \"{inputFullPaths[i]}\" -> \"{outputFullPaths[i]}\"");

            IParseableFile file = NewIPraseableFileForPath(inputFullPaths[i]);
            file.DeserializeFromFile(inputFullPaths[i]);
            Upgrade(file);
            StripFGD(file);
            file.SerializeToFile(outputFullPaths[i]);
        }
    }

    private static void ApplyRulesToFiles(List<string> inputFullPaths, List<string> outputFullPaths)
    {
        for(int i = 0; i < inputFullPaths.Count; i++)
        {
            Console.WriteLine($"Processing \"{inputFullPaths[i]}\" -> \"{outputFullPaths[i]}\"");

            IParseableFile file = NewIPraseableFileForPath(inputFullPaths[i]);
            file.DeserializeFromFile(inputFullPaths[i]);
            Upgrade(file);
            ApplyRules(file);
            StripFGD(file);
            file.SerializeToFile(outputFullPaths[i]);
        }
    }
}
