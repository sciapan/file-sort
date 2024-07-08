using FileGenerator;

const string pathToTestFile = @"D:\random.txt";

Console.WriteLine("Required file size in GB's:");
 
double.TryParse(Console.ReadLine(), out var fileSize);
if (fileSize <= 0)
{
    Console.WriteLine("Not correct file size specified. Press any key to exit.");
}
else
{
    var fileGenerator = new Generator(pathToTestFile, fileSize);
    fileGenerator.GenerateFile();

    Console.WriteLine("File generated. Press any key to exit.");
}

Console.ReadKey();