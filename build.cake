#tool "dotnet:?package=GitVersion.Tool&version=6.0.2"
#tool "nuget:?package=NuGet.CommandLine&version=6.11.0"
#tool "nuget:?package=dotnet-sonarscanner&version=9.0.0"

#addin "nuget:?package=Cake.Sonar&version=1.1.33"

var target = Argument("target", "Default");
var sonarLogin = Argument("sonarLogin", EnvironmentVariable("SONAR_LOGIN") ?? "");
var nugetApiKey = Argument("nugetApiKey", EnvironmentVariable("NUGET_API_KEY") ?? "");

//////////////////////////////////////////////////////////////////////
//    Build Variables
/////////////////////////////////////////////////////////////////////
var solution = "./CHG.Extensions.Security.sln";
var project = "./src/CHG.Extensions.Security.Txt.csproj";
var outputDir = MakeAbsolute(Directory("./buildArtifacts/"));
var outputDirNuget = outputDir.Combine("NuGet");
var sonarProjectKey = "CHG-MERIDIAN_CHG.Extensions.Security.Txt";
var sonarUrl = "https://sonarcloud.io";
var sonarOrganization = "chg-meridian";

var outputDirTemp = outputDir.Combine("Temp");
var outputDirTests = outputDirTemp.Combine("Tests");

var codeCoverageResultFilePath = MakeAbsolute(outputDirTests).Combine("**/").CombineWithFilePath("coverage.opencover.xml");
var testResultsPath = MakeAbsolute(outputDirTests).CombineWithFilePath("*.trx");

var nugetPublishFeed = "https://api.nuget.org/v3/index.json";

var isLocalBuild = BuildSystem.IsLocalBuild;
var isMasterBranch = StringComparer.OrdinalIgnoreCase.Equals("refs/heads/master", BuildSystem.GitHubActions.Environment.Workflow.Ref);
var isPullRequest = BuildSystem.GitHubActions.Environment.PullRequest.IsPullRequest;
var gitHubEvent = EnvironmentVariable("GITHUB_EVENT_NAME");
var isReleaseCreation = string.Equals(gitHubEvent, "release");
var runSonar = !string.IsNullOrWhiteSpace(sonarLogin);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Setup(context =>
{
    Information($"Local build: {isLocalBuild}");
    Information($"Master branch: {isMasterBranch}");
    Information($"Pull request: {isPullRequest}");
    Information($"Run sonar: {runSonar}");
    Information($"ref: {BuildSystem.GitHubActions.Environment.Workflow.Ref}");
    Information($"Is release creation: {isReleaseCreation}");
});

Task("Clean")
    .Description("Removes the output directory")
    .Does(() =>
    {

        EnsureDirectoryDoesNotExist(outputDir, new DeleteDirectorySettings
        {
            Recursive = true,
            Force = true
        });
        CreateDirectory(outputDir);
    });

GitVersion versionInfo = null;
Task("Version")
    .Description("Retrieves the current version from the git repository")
    .Does(() =>
    {

        versionInfo = GitVersion(new GitVersionSettings
        {
            UpdateAssemblyInfo = false
        });

        Information("Major:\t\t\t\t\t" + versionInfo.Major);
        Information("Minor:\t\t\t\t\t" + versionInfo.Minor);
        Information("Patch:\t\t\t\t\t" + versionInfo.Patch);
        Information("MajorMinorPatch:\t\t\t" + versionInfo.MajorMinorPatch);
        Information("SemVer:\t\t\t\t\t" + versionInfo.SemVer);
        Information("LegacySemVer:\t\t\t\t" + versionInfo.LegacySemVer);
        Information("LegacySemVerPadded:\t\t\t" + versionInfo.LegacySemVerPadded);
        Information("AssemblySemVer:\t\t\t\t" + versionInfo.AssemblySemVer);
        Information("FullSemVer:\t\t\t\t" + versionInfo.FullSemVer);
        Information("InformationalVersion:\t\t\t" + versionInfo.InformationalVersion);
        Information("BranchName:\t\t\t\t" + versionInfo.BranchName);
        Information("Sha:\t\t\t\t\t" + versionInfo.Sha);
        Information("NuGetVersionV2:\t\t\t\t" + versionInfo.NuGetVersionV2);
        Information("NuGetVersion:\t\t\t\t" + versionInfo.NuGetVersion);
        Information("CommitsSinceVersionSource:\t\t" + versionInfo.CommitsSinceVersionSource);
        Information("CommitsSinceVersionSourcePadded:\t" + versionInfo.CommitsSinceVersionSourcePadded);
        Information("CommitDate:\t\t\t\t" + versionInfo.CommitDate);
    });

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .Does(() =>
    {

        var msBuildSettings = new DotNetMSBuildSettings()
        {
            Version = versionInfo.AssemblySemVer,
            InformationalVersion = versionInfo.InformationalVersion,
            PackageVersion = versionInfo.SemVer
        }.WithProperty("PackageOutputPath", outputDirNuget.FullPath);

        var settings = new DotNetBuildSettings
        {
            Configuration = "Release",
            MSBuildSettings = msBuildSettings
        };

        DotNetBuild(solution, settings);
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {

        var settings = new DotNetTestSettings
        {
            Configuration = "Release",
            Loggers = new[] { "trx;" },
            ResultsDirectory = outputDirTests,
            Collectors = new[] { "XPlat Code Coverage" },
            ArgumentCustomization = a => a.Append("-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover"),
            NoBuild = true
        };

        DotNetTest(solution, settings);
    });

Task("SonarBegin")
    .WithCriteria(runSonar)
    .Does(() =>
    {
        SonarBegin(new SonarBeginSettings
        {
            Key = sonarProjectKey,
            Url = sonarUrl,
            Organization = sonarOrganization,
            Token = sonarLogin,
            UseCoreClr = true,
            VsTestReportsPath = testResultsPath.ToString(),
            OpenCoverReportsPath = codeCoverageResultFilePath.ToString(),
            ArgumentCustomization = args => args.Append("/d:sonar.scanner.skipJreProvisioning=true")
             .Append("/d:sonar.scanner.scanAll=false") // disable Multi-Language analysis
        });
    });

Task("SonarEnd")
    .WithCriteria(runSonar)
    .Does(() =>
    {
        SonarEnd(new SonarEndSettings
        {
            Token = sonarLogin
        });
    });

Task("Publish")
     .WithCriteria(isReleaseCreation)
     .IsDependentOn("Test")
    .IsDependentOn("Version")
    .Description("Pushes the created NuGet packages to nuget.org")
    .Does(() =>
    {

        // Get the paths to the packages.
        var packages = GetFiles(outputDirNuget + "*.nupkg");

        // Push the package and symbols
        NuGetPush(packages, new NuGetPushSettings
        {
            Source = nugetPublishFeed,
            ApiKey = nugetApiKey,
            SkipDuplicate = true
        });
    });

Task("Default")
    .IsDependentOn("SonarBegin")
    .IsDependentOn("Test")
    .IsDependentOn("SonarEnd")
    .IsDependentOn("Publish");

RunTarget(target);
