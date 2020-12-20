using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestApp
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Console.WriteLine("start");

            string pathToBD = @"D:\VSProjects\Apportion.V2\Trips.db";
            string pathToBDAndriod =
                @"D:\VSProjects\Apportion.V2\Apportionment2\Apportionment2\Apportionment2.Android\Assets\Trips.db";
            string pathToBDiOs =
                @"D:\VSProjects\Apportion.V2\Apportionment2\Apportionment2\Apportionment2.iOS\Resources\Trips.db";
            string pathToBDUwp =
                @"D:\VSProjects\Apportion.V2\Apportionment2\Apportionment2\Apportionment2.UWP\Assets\Trips.db";

            FileInfo fileInf = new FileInfo(pathToBD);

            if (fileInf.Exists)
            {
                fileInf.CopyTo(pathToBDAndriod, true);
                fileInf.CopyTo(pathToBDiOs, true);
                fileInf.CopyTo(pathToBDUwp, true);
                // альтернатива с помощью класса File
                // File.Copy(path, newPath, true);
            }

            Console.WriteLine("ok");
        }

        [TestMethod]
        public void CopyReportsHtml()
        {
            Console.WriteLine("start");

            string pathToBD = @"D:\VSProjects\Apportion.V2\ReportTemplateCurrency.html";
            string pathToBDAndriod =
                @"D:\VSProjects\Apportion.V2\Apportionment2\Apportionment2\Apportionment2.Android\Assets\ReportTemplateCurrency.html";
            string pathToBDiOs =
                @"D:\VSProjects\Apportion.V2\Apportionment2\Apportionment2\Apportionment2.iOS\Resources\ReportTemplateCurrency.html";
            string pathToBDUwp =
                @"D:\VSProjects\Apportion.V2\Apportionment2\Apportionment2\Apportionment2.UWP\Assets\ReportTemplateCurrency.html";

            FileInfo fileInf = new FileInfo(pathToBD);

            if (fileInf.Exists)
            {
                fileInf.CopyTo(pathToBDAndriod, true);
                fileInf.CopyTo(pathToBDiOs, true);
                fileInf.CopyTo(pathToBDUwp, true);
                // альтернатива с помощью класса File
                // File.Copy(path, newPath, true);
            }

            Console.WriteLine("ok");
        }

        [TestMethod]
        public void CopyReportsButtons()
        {
            Console.WriteLine("start");
            List<string> buttons = new List<string>{ "Add.png" , "Save.png", "Cancel.png", "Delete.png" };
           // string buttonName = "Add.png";
            //string buttonName = "Save.png";
            //string buttonName = "Cancel.png";
            foreach (var buttonName in buttons)
            {
                string pathToBD = @"D:\VSProjects\Apportion.V2\" + buttonName;
                string pathToBDAndriod =
                    @"D:\VSProjects\Apportion.V2\Apportionment2\Apportionment2\Apportionment2.Android\Resources\drawable\" +
                    buttonName;
                string pathToBDiOs =
                    @"D:\VSProjects\Apportion.V2\Apportionment2\Apportionment2\Apportionment2.iOS\Resources\" +
                    buttonName;
                string pathToBDUwp =
                    @"D:\VSProjects\Apportion.V2\Apportionment2\Apportionment2\Apportionment2.UWP\" + buttonName;

                FileInfo fileInf = new FileInfo(pathToBD);

                if (fileInf.Exists)
                {
                    fileInf.CopyTo(pathToBDAndriod, true);
                    fileInf.CopyTo(pathToBDiOs, true);
                    fileInf.CopyTo(pathToBDUwp, true);
                    // альтернатива с помощью класса File
                    // File.Copy(path, newPath, true);
                }
            }

            Console.WriteLine("ok");
        }

        [TestMethod]
        public void TestSqlCommand()
        {
            string p = "asdfaf -asdf";
            string tableName = "Costs";
            string result = $"update {tableName} set Sync = Sync where id = {p}";
            Console.WriteLine(result);
        }
    }
}
