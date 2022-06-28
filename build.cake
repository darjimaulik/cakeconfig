#addin nuget:?package=Cake.Azure&version=0.3.0
#addin nuget:?package=Cake.Http&version=0.5.0
#addin nuget:?package=Cake.Json&version=3.0.1
#addin nuget:?package=Cake.Powershell&version=0.4.7
#addin nuget:?package=Cake.XdtTransform&version=0.16.0
#addin nuget:?package=Newtonsoft.Json&version=11.0.1


#load "local:?path=CakeScripts/helper-methods.cake"
///////////////////////////////////////////////////////////////////////////////
// VARIABLES
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
//var configuration = Argument("configuration", "Release");
var artifacts = "./dist/";
var testResultsPath = MakeAbsolute(Directory(artifacts + "./test-results"));


var configuration = new Configuration();
var platform = new CakePlatform();
var runtime = new CakeRuntime();
var environment = new CakeEnvironment(platform, runtime);
var cakeConsole = new CakeConsole(environment);
var configJsonFile = Argument<string>("Configuration", "cake-config.json");
var deploymentRootPath ="";
var deployLocal = true;



var myargument = Argument("myargument", "Maulik Darji");

///////////////////////////////////////////////////////////////////////////////
// SETUP
///////////////////////////////////////////////////////////////////////////////




Setup(setupContext =>
{
	cakeConsole.ForegroundColor = ConsoleColor.Green;
	WriteMessage("");
	WriteMessage("Inside the Setup");
	WriteMessage("");
	cakeConsole.ForegroundColor = ConsoleColor.Yellow;
	PrintHeader(ConsoleColor.Green);
	
    var configFile = new FilePath(configJsonFile);
    configuration = DeserializeJsonFromFile<Configuration>(configFile);
    deploymentRootPath = $"{configuration.DeployFolder}";

    WriteMessage($"Using {configuration.BuildConfiguration} build configuration");
});


///////////////////////////////////////////////////////////////////////////////
// SUB TASKS
///////////////////////////////////////////////////////////////////////////////


Task("CleanBuildFolders").Does(() => {
    // Clean project build folders
	WriteMessage ($" Cleaning the {configuration.SourceFolder}/**/obj");
//    CleanDirectories($"{configuration.SourceFolder}/**/obj");
//    CleanDirectories($"{configuration.SourceFolder}/**/bin");

});

Task("Clean")
    .Does(() =>
{
    WriteMessage($"Inside the Clean function to clean the {artifacts} directory!");
    // Todo: Do we need to clean the solution before build?
    // Clean artifacts directory
    CleanDirectory(artifacts);
});
Task("Copy-Configs")
    .Does(()=> {
        cakeConsole.ForegroundColor = ConsoleColor.Green;
		
        var files = GetFiles($"{configuration.SourceFolder}\\**\\code\\**\\app_config\\**\\*.config");
		var destination = deploymentRootPath + "\\App_config_Copy";
	    foreach (var dirItem in files)
		{
			WriteMessage ($" Directory to copy is {dirItem.GetDirectory()}");
            var pathSegments = dirItem.Segments;
			var foundAppConfig = false;
            var destinationPath = "";
            var iCount = 0;
            foreach (var item in pathSegments)
            {
                iCount ++;
                if (foundAppConfig)
                {
                    if (pathSegments.Length == iCount)
                    {
                        var finalPath = $"{destination}\\{destinationPath}\\{item}";
                        WriteMessage($"File is {finalPath}",ConsoleColor.Cyan);
                        CopyFile(dirItem,$"{finalPath}");
                    }
                    else
                    {
                        destinationPath +=  $"\\{item}";
                        var finalPath = destination + destinationPath;
                        if (DirectoryExists(finalPath))
                        {
                            WriteMessage($" Directory exists {finalPath}",ConsoleColor.Green);
                        }
                        else
                        {
                            WriteMessage($" Directory DOES NOT exists {finalPath} creating now .....",ConsoleColor.Red);
                            EnsureDirectoryExists(finalPath);
                        }
                        WriteMessage($"destination Path is {finalPath}");
                    }
                }
                if (item.ToLower() == "app_config")
                {
                    foundAppConfig = true;
                }
            }
		}
}); 


// This is an additional function which copies all the views file in a separate folder 
Task("Copy-Views")
    .Does(()=> {
        cakeConsole.ForegroundColor = ConsoleColor.Green;
		
        var files = GetFiles($"{configuration.SourceFolder}\\**\\code\\**\\views\\**\\*.cshtml");
		var destination = deploymentRootPath + "\\Views_Copy";
	    foreach (var fileItem in files)
		{
			//WriteMessage ($" Directory to copy is {fileItem.GetDirectory()}");
            var pathSegments = fileItem.Segments;
			var foundViews = false;
            var destinationPath = "";
            var iCount = 0;
            foreach (var item in pathSegments)
            {
                iCount ++;
                if (foundViews)
                {
                    if (pathSegments.Length == iCount)
                    {
                        var finalPath = $"{destination}\\{destinationPath}\\{item}";

                        //WriteMessage($"File is {finalPath}",ConsoleColor.Cyan);
                        CopyFile(fileItem,$"{finalPath}");
                        cakeConsole.ResetColor();
                    }
                    else
                    {
                        destinationPath +=  $"\\{item}";
                        var finalPath = destination + destinationPath;
                        if (DirectoryExists(finalPath))
                        {
                            //WriteMessage($" Directory exists {finalPath}",ConsoleColor.Green);
                        }
                        else
                        {

                            //WriteMessage($" Directory DOES NOT exists {finalPath} creating now .....", ConsoleColor.Red);
                            EnsureDirectoryExists(finalPath);

                        }

                        //WriteMessage($"destination Path is {finalPath}");
                    }
                }
                if (item.ToLower() == "views")
                {
                    foundViews = true;
                }
            }
		}
}); 

Task("Copy-Sitecore-Lib")
    .WithCriteria(()=>(configuration.BuildConfiguration == "Local"))
    .Does(()=> {
		
		WriteMessage($"Finding the Sitecore DLLs in {configuration.WebsiteRoot}/bin/Sitecore*.dll");
		
        var files = GetFiles($"{configuration.WebsiteRoot}/bin/Sitecore*.dll");
        var destination = "./lib";
        EnsureDirectoryExists(destination);
		// Maulik Disabled this 
        // CopyFiles(files, destination);
}); 
///////////////////////////////////////////////////////////////////////////////
// MAIN TASKS
///////////////////////////////////////////////////////////////////////////////


Task("Build")
    .IsDependentOn("Clean")
	.IsDependentOn("Build-Solution")
	.IsDependentOn("Copy-Configs")
	.IsDependentOn("Copy-Views")
	.IsDependentOn("Copy-Dlls")
	.IsDependentOn("Full-Publish")
	
    .Does(() =>
{

});
Task("Default")
    .IsDependentOn("Build")
    .Does(() =>
{
	
});

Task("Full-Publish")
.WithCriteria(configuration != null)
.IsDependentOn("CleanBuildFolders")
.IsDependentOn("Publish-All-Projects");


Task("Quick-Deploy")
.WithCriteria(configuration != null)
.IsDependentOn("CleanBuildFolders")
.IsDependentOn("Copy-Sitecore-Lib")
.IsDependentOn("Modify-PublishSettings")
.IsDependentOn("Publish-All-Projects");

Task("Publish-All-Projects")
.IsDependentOn("Publish-Foundation-Projects")
.IsDependentOn("Publish-Feature-Projects")
.IsDependentOn("Publish-Project-Projects");

Task("Copy-DLLs")
.IsDependentOn("Copy-Foundation-DLLs")
.IsDependentOn("Copy-Feature-DLLs")
.IsDependentOn("Copy-Project-DLLs");


Task("Build-Solution")
.Does(() => {
    MSBuild(configuration.SolutionFile, cfg => InitializeMSBuildSettings(cfg));
});




Task("Publish-Foundation-Projects").Does(() => {
    var destination = deploymentRootPath;
    
    PublishProjects(configuration.FoundationSrcFolder, destination);
});

Task("Publish-Feature-Projects").Does(() => {
     var destination = deploymentRootPath;

     PublishProjects(configuration.FeatureSrcFolder, destination);
});

Task("Publish-Project-Projects").Does(() => {
     var destination = deploymentRootPath;
     PublishProjects(configuration.ProjectSrcFolder, destination);
});

Task("Copy-Foundation-DLLs").Does(() => {
    var destination = deploymentRootPath;
    CopyDLLs(configuration.FoundationSrcFolder, destination);
});

Task("Copy-Feature-DLLs").Does(() => {
    var destination = deploymentRootPath;
    CopyDLLs(configuration.FeatureSrcFolder, destination);
});


Task("Copy-Project-DLLs").Does(() => {
    var destination = deploymentRootPath;
    CopyDLLs(configuration.ProjectSrcFolder, destination);
});


Task("Modify-PublishSettings").Does(() => {
    var publishSettingsOriginal = File($"{configuration.ProjectFolder}/publishsettings.targets");
    var destination = $"{configuration.ProjectFolder}/publishsettings.targets.user";

    CopyFile(publishSettingsOriginal,destination);

	var importXPath = "/ns:Project/ns:Import";

    var publishUrlPath = "/ns:Project/ns:PropertyGroup/ns:publishUrl";

    var xmlSetting = new XmlPokeSettings {
        Namespaces = new Dictionary<string, string> {
            {"ns", @"http://schemas.microsoft.com/developer/msbuild/2003"}
        }
    };
    XmlPoke(destination,importXPath,null,xmlSetting);
    XmlPoke(destination,publishUrlPath,$"{configuration.InstanceUrl}",xmlSetting);
});

RunTarget(target);
