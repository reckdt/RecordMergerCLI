using System;
using System.IO;
using Xunit;

namespace RecordMerger.Tests
{
    public class Tests
    {
        public FileInfo[] GetFileInfos(string[] fileNames)
        {
            var fileInfos = new FileInfo[fileNames.Length];

            for (int i = 0; i < fileNames.Length; i++)
            {
                var path = Path.Join(Directory.GetCurrentDirectory(), "data", fileNames[i]);
                var fileInfo = new FileInfo(path);
                fileInfos[i] = fileInfo;
            }

            return fileInfos;
        }

        public string Format(string str)
        {
            str = str.Replace("\r","").Trim();
            str = str.Replace("\n","").Trim();
            str = str.Replace(" ","").Trim();

            return str;
        }

        [Fact]
        public void Test1File()
        {
            var fileNames = new string[] { "input1" };
            var fileInfos = GetFileInfos(fileNames);
            var sort = new string[] { }; 

            var startup = new StartupCLI();
            var csv = startup.Run(fileInfos, sort, null);
            
            var expected = @"LastName,FirstName,Email,FavoriteColor,DateOfBirth
                             Smith,John,johnsmith@example.com,Blue,1/1/1990
                             Smith,Jane,janesmith@example.com,Green,2/2/1990";

            Assert.Equal(Format(expected), Format(csv));                     
        }

        [Fact]
        public void TestFavoriteColorAscLastNameAsc()
        {
            var fileNames = new string[] { "input1", "input2", "input3" };
            var fileInfos = GetFileInfos(fileNames);
            var sort = new string[] { "FavoriteColor:asc", "LastName:asc" }; 

            var startup = new StartupCLI();
            var csv = startup.Run(fileInfos, sort, null);
            
            var expected = @"LastName,FirstName,Email,FavoriteColor,DateOfBirth
                             Miller,Jessica,jessicamiller@example.com,Blue,3/3/1989
                             Smith,John,johnsmith@example.com,Blue,1/1/1990
                             Smith,Jane,janesmith@example.com,Green,2/2/1990
                             Williams,Nick,nickwilliams@example.com,Purple,12/12/1985
                             Miller,Mike,mikemiller@example.com,Red,5/5/1993
                             Davis,John,johndavis@example.com,Teal,11/1/1986";

            Assert.Equal(Format(expected), Format(csv));                     
        }

        [Fact]
        public void TestDateOfBirthAsc()
        {
            var fileNames = new string[] { "input1", "input2", "input3" };
            var fileInfos = GetFileInfos(fileNames);
            var sort = new string[] { "DateOfBirth:asc" }; 

            var startup = new StartupCLI();
            var csv = startup.Run(fileInfos, sort, null);

            var expected = @"LastName,FirstName,Email,FavoriteColor,DateOfBirth
                             Williams,Nick,nickwilliams@example.com,Purple,12/12/1985
                             Davis,John,johndavis@example.com,Teal,11/1/1986
                             Miller,Jessica,jessicamiller@example.com,Blue,3/3/1989
                             Smith,John,johnsmith@example.com,Blue,1/1/1990
                             Smith,Jane,janesmith@example.com,Green,2/2/1990
                             Miller,Mike,mikemiller@example.com,Red,5/5/1993";

            Assert.Equal(Format(expected), Format(csv));                     
        }

        [Fact]
        public void TestLastNameDesc()
        {
            var fileNames = new string[] { "input1", "input2", "input3" };
            var fileInfos = GetFileInfos(fileNames);
            var sort = new string[] { "LastName:desc" }; 

            var startup = new StartupCLI();
            var csv = startup.Run(fileInfos, sort, null);

            var expected = @"LastName,FirstName,Email,FavoriteColor,DateOfBirth
                             Williams,Nick,nickwilliams@example.com,Purple,12/12/1985
                             Smith,John,johnsmith@example.com,Blue,1/1/1990
                             Smith,Jane,janesmith@example.com,Green,2/2/1990
                             Miller,Mike,mikemiller@example.com,Red,5/5/1993
                             Miller,Jessica,jessicamiller@example.com,Blue,3/3/1989
                             Davis,John,johndavis@example.com,Teal,11/1/1986";

            Assert.Equal(Format(expected), Format(csv));             
        }

        [Fact]
        public void TestInvalidDelimiter()
        {
            var fileNames = new string[] { "inputerror1" };
            var fileInfos = GetFileInfos(fileNames);
            var sort = new string[] { }; 

            var startup = new StartupCLI();

            try
            {
                var csv = startup.Run(fileInfos, sort, null);  
                Assert.True(false, "An exception should have been thrown.");
            }
            catch (Exception e)
            {
                Assert.Equal("Header does not contain a valid delimiter, only '|', ',', and ' ' accepted.", e.Message);
            }      
        }

        [Fact]
        public void TestNonMatchingColumnNames()
        {
            var fileNames = new string[] { "inputerror2", "inputerror3" };
            var fileInfos = GetFileInfos(fileNames);
            var sort = new string[] { }; 

            var startup = new StartupCLI();

            try
            {
                var csv = startup.Run(fileInfos, sort, null);  
                Assert.True(false, "An exception should have been thrown.");
            }
            catch (Exception e)
            {
                Assert.Equal("Column names throughout files do not match.", e.Message);
            }      
        }
        
        [Fact]
        public void Test3Sorts()
        {
            var fileNames = new string[] { "input1" };
            var fileInfos = GetFileInfos(fileNames);
            var sort = new string[] { "FirstName:asc", "LastName:asc", "FavoriteColor:asc"}; 

            var startup = new StartupCLI();

            try
            {
                var csv = startup.Run(fileInfos, sort, null);  
                Assert.True(false, "An exception should have been thrown.");
            }
            catch (Exception e)
            {
                Assert.Equal("Only allowed to sort by a max of 2 columns.", e.Message);
            }      
        }

        [Fact]
        public void TestWrongSort()
        {
            var fileNames = new string[] { "input1" };
            var fileInfos = GetFileInfos(fileNames);
            var sort = new string[] { "MiddleName:asc" }; 

            var startup = new StartupCLI();

            try
            {
                var csv = startup.Run(fileInfos, sort, null);  
                Assert.True(false, "An exception should have been thrown.");
            }
            catch (Exception e)
            {
                Assert.Equal("Sort column, MiddleName, does not exist.", e.Message);
            }      
        }

        [Fact]
        public void TestWrongOrderBy()
        {
            var fileNames = new string[] { "input1" };
            var fileInfos = GetFileInfos(fileNames);
            var sort = new string[] { "LastName:greaterthan" }; 

            var startup = new StartupCLI();

            try
            {
                var csv = startup.Run(fileInfos, sort, null);  
                Assert.True(false, "An exception should have been thrown.");
            }
            catch (Exception e)
            {
                Assert.Equal("Acceptable sorts are only 'asc' or 'desc', not greaterthan.", e.Message);
            }      
        }
    }
}
