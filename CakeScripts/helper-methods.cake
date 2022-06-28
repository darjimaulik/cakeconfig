using System.Text.RegularExpressions;

/*===============================================
================= HELPER METHODS ================
===============================================*/

public class Configuration
{
    private MSBuildToolVersion _msBuildToolVersion;    

    public string WebsiteRoot {get;set;}
    public string XConnectRoot {get;set;}
    public string InstanceUrl {get;set;}
    public string SolutionName {get;set;}
    public string ProjectFolder {get;set;}
    public string BuildConfiguration {get;set;}
    public string MessageStatisticsApiKey {get;set;}
    public string MarketingDefinitionsApiKey {get;set;}
    public bool RunCleanBuilds {get;set;}
	public int DeployExmTimeout {get;set;}
    public string DeployFolder {get;set;}      
    public string Version {get;set;}
    public string Topology {get;set;}
    public bool CDN {get;set;}
	public bool SXA {get;set;}
    public string DeploymentTarget{get;set;}
    
    public string BuildToolVersions 
    {
        set 
        {
            if(!Enum.TryParse(value, out this._msBuildToolVersion))
            {
                this._msBuildToolVersion = MSBuildToolVersion.Default;
            }
        }
    }

    public string SourceFolder => $"{ProjectFolder}\\src";
    public string FoundationSrcFolder => $"{SourceFolder}\\Foundation";
    public string FeatureSrcFolder => $"{SourceFolder}\\Feature";
    public string ProjectSrcFolder => $"{SourceFolder}\\Project";

    public string SolutionFile => $"{ProjectFolder}\\{SolutionName}";
    public MSBuildToolVersion MSBuildToolVersion => this._msBuildToolVersion;
    public string BuildTargets => this.RunCleanBuilds ? "Clean;Build" : "Build";
}

public void PrintHeader(ConsoleColor foregroundColor)
{
    cakeConsole.ForegroundColor = foregroundColor;
    WriteMessage("     "); 
    WriteMessage("     "); 
    WriteMessage(@"   ) )       /\                  ");
    WriteMessage(@"  =====     /  \                 ");                     
    WriteMessage(@" _|___|____/ __ \____________    ");
    WriteMessage(@"|:::::::::/ ==== \:::::::::::|   ");
    WriteMessage(@"|:::::::::/ ====  \::::::::::|   ");
    WriteMessage(@"|::::::::/__________\:::::::::|  ");
    WriteMessage(@"|_________|  ____  |_________|                                ");
    WriteMessage(@"| ______  | / || \ | _______ |        _   _");
    WriteMessage(@"||  |   | | ====== ||   |   ||       | | | |");
    WriteMessage(@"||--+---| | |    | ||---+---||       | |_| | ___  _ __ ___   ___");
    WriteMessage(@"||__|___| | |   o| ||___|___||       |  _  |/ _ \| '_ ` _ \ / _ \");
    WriteMessage(@"|======== | |____| |=========|       | | | | (_) | | | | | |  __/");
    WriteMessage(@"(^^-^^^^^- |______|-^^^--^^^)        \_| |_/\___/|_| |_| |_|\___|");
    WriteMessage(@"(,, , ,, , |______|,,,, ,, ,)");                                    
    WriteMessage(@"','',,,,'  |______|,,,',',;;");                                     
	WriteMessage(@"     "); 
    WriteMessage(@"     "); 
    WriteMessage(@" --------------------  ------------------");
    WriteMessage("   " + "The source code modified and simplified by Maulik Darji.");
    WriteMessage("   " + "The source code is from Habitat Home, tools and processes are examples of Sitecore Features.");
    WriteMessage("   " + "The Habitat Home source code, tools and processes are examples of Sitecore Features.");
    WriteMessage("   " + "Habitat Home is not supported by Sitecore and should be used at your own risk.");
    WriteMessage("     "); 
    WriteMessage("     ");
    cakeConsole.ResetColor();
}

public void PublishProjects(string rootFolder, string publishRoot)
{
	var path = $"{rootFolder}\\**\\code\\*.csproj";
    var projects = GetFiles(path);
    cakeConsole.ForegroundColor =  ConsoleColor.Red;
    WriteMessage(@" --------------------  ------------------");
    WriteMessage(@" --------------------  ------------------");
    WriteMessage("Publishing " + rootFolder + " to " + publishRoot);
  	WriteMessage("Publishing THE " + path + " to " + publishRoot);
	cakeConsole.ResetColor();
	
    foreach (var project in projects)
    {
		try{
			WriteMessage("Publishing inside the project " + rootFolder + " to " + publishRoot);
			WriteMessage ($"Project path is {project}");
            WriteMessage(@" --------------------  ------------------");
            WriteMessage(@" --------------------  ------------------");
            cakeConsole.ResetColor();
		}
		catch(Exception ex)
		{ 
			WriteMessage($"Exception occurred in the Try block {ex.Message}");
		}
        MSBuild(project, cfg => InitializeMSBuildSettings(cfg)
                                   .WithTarget(configuration.BuildTargets)
                                   .WithProperty("DeployOnBuild", "true")
                                   .WithProperty("DeployDefaultTarget", "WebPublish")
                                   .WithProperty("WebPublishMethod", "FileSystem")
                                   .WithProperty("DeleteExistingFiles", "false")
                                   .WithProperty("publishUrl", publishRoot)
                                   .WithProperty("BuildProjectReferences", "false")
                                   );
    }
}

public void CopyDLLs(string rootFolder, string publishRoot)
{
   		WriteMessage($"Finding the Project DLLs in {rootFolder}/code*/bin/*.dll");
		
        var files = GetFiles($"{rootFolder}\\**\\code\\**\\bin\\*ProjectName*.dll");
        //var destination = "./lib";
		var destination = deploymentRootPath + "\\copiedDll\\";
		
		WriteMessage($"Finding the Project DLLs in {rootFolder}\\**\\code\\**\\bin\\*ProjectName*.dll  to the {destination}");


        EnsureDirectoryExists(destination);
        CopyFiles(files, destination);
}

public void RebuildIndex(string indexName)
{
    var url = $"{configuration.InstanceUrl}utilities/indexrebuild.aspx?index={indexName}";
    string responseBody = HttpGet(url);
}

public MSBuildSettings InitializeMSBuildSettings(MSBuildSettings settings)
{
    settings.SetConfiguration(configuration.BuildConfiguration)
            .SetVerbosity(Verbosity.Minimal)
            .SetMSBuildPlatform(MSBuildPlatform.Automatic)
            .SetPlatformTarget(PlatformTarget.MSIL)
            .UseToolVersion(configuration.MSBuildToolVersion);
            /*.WithRestore();*/
    return settings;
}

public void CreateFolder(string folderPath)
{
    if (!DirectoryExists(folderPath))
    {
        CreateDirectory(folderPath);
    }
}

public void WriteMessage(string Message, ConsoleColor Color = ConsoleColor.White)
{
    if (Color != ConsoleColor.White)
        cakeConsole.ForegroundColor = Color;
    cakeConsole.WriteLine(Message);
    if (Color != ConsoleColor.White)
        cakeConsole.ResetColor();
}

public void WriteError(string errorMessage)
{
    cakeConsole.ForegroundColor = ConsoleColor.Red;
    cakeConsole.WriteError(errorMessage);
    cakeConsole.ResetColor();
}