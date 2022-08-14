using System.Text.RegularExpressions;

const string Output = "Output";
Regex UsingRegex = new Regex(@"using (?<using>[^\(].*);");

if (!args.Any())
{
    Console.WriteLine("The .csproj directory is missing");
    return;
}

var path = args[0];
var directoryInfo = new DirectoryInfo(path);
foreach (var categoryDirectory in directoryInfo.GetDirectories())
{
    if (Regex.IsMatch(categoryDirectory.Name, "bin|obj"))
    {
        continue;
    }

    foreach (var projectDirectory in categoryDirectory.GetDirectories())
    {
        WriteProjectToFile(projectDirectory, categoryDirectory.Name);
    }
}

void WriteProjectToFile(DirectoryInfo projectDirectory, string categoryName)
{
    var projectName = projectDirectory.Name;
    var directoryPath = Path.Combine(Output, categoryName);
    var path = Path.Combine(directoryPath, $"{projectName}.txt");

    if (!Directory.Exists(directoryPath))
    {
        Directory.CreateDirectory(directoryPath);
    }

    var allFiles = CollectFiles(projectDirectory);
    var usings = GatherAllUsings(allFiles).Distinct().OrderBy(a => a);

    using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
    {
        using (var writer = new StreamWriter(stream))
        {
            foreach (var usingPath in usings)
            {
                writer.WriteLine($"using {usingPath};");
            }
            writer.WriteLine();

            CombineSourceFiles(projectDirectory.GetFiles(), writer);

            foreach (var directory in projectDirectory.GetDirectories())
            {
                CombineSourceFiles(directory.GetFiles(), writer);
            }
        }
    }
}

void CombineSourceFiles(FileInfo[] files, StreamWriter writer)
{
    foreach (var file in files)
    {
        var lines = File.ReadAllLines(file.FullName);

        foreach (var line in lines)
        {
            if (line.StartsWith("using"))
            {
                continue;
            }

            writer.WriteLine(line);
        }
    }
}

IEnumerable<string> GatherAllUsings(IEnumerable<string> allFiles)
{
    var usings = new List<string>();
    foreach (var file in allFiles)
    {
        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
        {
            using (var reader = new StreamReader(stream))
            {
                var line = reader.ReadLine();

                while (line is not null)
                {
                    var match = UsingRegex.Match(line);

                    if (!match.Success)
                    {
                        break;
                    }

                    var group = match.Groups["using"];
                    if (group.Success)
                    {
                        usings.Add(group.Value);
                    }

                    line = reader.ReadLine();
                }
            }
        }
    }

    return usings;
}

IEnumerable<string> CollectFiles(DirectoryInfo projectDirectory)
{
    var allFiles = new List<string>();
    foreach (var file in projectDirectory.GetFiles())
    {
        allFiles.Add(file.FullName);
    }

    foreach (var directory in projectDirectory.GetDirectories())
    {
        foreach (var file in directory.GetFiles())
        {
            allFiles.Add(file.FullName);
        }
    }

    return allFiles;
}