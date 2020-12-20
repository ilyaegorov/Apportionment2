using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
//using NUnit.Framework;
using Apportionment2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Apportionment2Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TestMethod2()
        {
            Console.WriteLine("ok");
            Debug.Print("text");
            Assert.AreEqual(1,2);
        }

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
    }
}
