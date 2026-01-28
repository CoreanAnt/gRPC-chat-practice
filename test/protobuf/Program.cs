using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Course.Protobuf.Test;

Console.WriteLine("welcome to protobuf test");
var emp = new Employee {
    FirstName = "Memi",
    LastName = "Lavi",
    IsRetired = false,
    BirthDate = Timestamp.FromDateTime(DateTime.SpecifyKind(new DateTime(1996, 05, 23), DateTimeKind.Utc)),
    Age = 30,
    //MaritalStatus = Employee.Types.MaritalStatus.Married,
    PreviousEmployers = { "Microsoft", "HP", "Google" },
    CurrentAddress = new Address {
        City = "Seoul",
        ZipCode = "12345"
    },
    Relatives = 
    {
        { "father", "Lee" },
        { "mother", "Kim" }
    }
};

using (var output = File.Create("emp.dat"))
{
    emp.WriteTo(output);
}

Employee empFromFile;
using (var input = File.OpenRead("emp.dat"))
{
    empFromFile=Employee.Parser.ParseFrom(input);
}

Console.WriteLine($"복구된 이름: {empFromFile.FirstName} {empFromFile.LastName}");
Console.WriteLine($"복구된 날짜: {empFromFile.BirthDate}");

Console.WriteLine("Protobuf test complete");