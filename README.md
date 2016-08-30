# BrickFtpWebApi
A Web API implementation of BrickFTP's REST API

The unit test demonstrates how the code can be used:

    [TestFixture]
    internal class BrickFtpTest
    {
        string domain = "test.brickftp.com";
        string username = "test";
        string password = "TestUser";

        // After having created the session, the idea is that you stuff it somewhere (a cache or db) and you
        // reuse it for the rest of the day or until it is invalid, and you have to re-authenticate. For the
        // purpose of this unit test, we are not reusing the session.
        [Test]
        public void TestConnectToBrickFtp()
        {
            Session session = BrickFtp.Login(domain, username, password);
            Assert.IsNotNull(session);
        }

        [Test]
        public void TestListFolder()
        {//GET /folders/
            Session session = BrickFtp.Login(domain, username, password);
            string path = "/";
            List<FtpItem> folders = BrickFtp.ListFolder(session, path);
            foreach (var f in folders)
            {
                Console.Out.WriteLine("List: " + f.path);
            }
        }

        [Test]
        public void TestListFolderWithConstraints()
        {//GET /folders/
            //var operationPath = "/api/rest/v1/folders/test/Testtt?&per_page=1";
            //var operationPath = "/api/rest/v1/folders/test/Testtt?&sort_by[modified_at_datetime]=desc";
            //var operationPath = "/api/rest/v1/folders/test/Testtt?&search=201606030";
            Session session = BrickFtp.Login(domain, username, password);
            string path = "/Testtt";
            List<FtpItem> folders = BrickFtp.ListFolderWithConstraints(session, path, searchText: "201606030");
            foreach (var f in folders)
            {
                Console.Out.WriteLine("List: " + f.path);
            }
        }

        [Test]
        public void TestCreateFolder()
        {//POST /folders/:path_and_folder_name
            
            Session session = BrickFtp.Login(domain, username, password);
            var path = "/Testtt/NewFolderTest";
            BrickFtp.CreateFolder(session, path);
        }

        [Test]
        public void TestDeleteFolder()
        {//DELETE /files/:path_or_filename

            Session session = BrickFtp.Login(domain, username, password);
            var path = "/Testtt/NewFolderTest";
            BrickFtp.DeleteFolder(session, path);
        }

        [Test]
        public void TestGetFolder()
        {   
            Session session = BrickFtp.Login(domain, username, password);
            var path = "/Testtt";

            var items = BrickFtp.GetFolderStats(session, path);
            foreach (var item in items)
            {
                Console.Out.WriteLine("Type: " + item.type+" | Path: "+item.path);
            }
        }


        [Test]
        public void TestGetFile()
        {   //GET /files/:path_and_filename
            Session session = BrickFtp.Login(domain, username, password);
            var filePath = "/Testtt/Forecast_MG_GwyntYMor_21_0_2016060200.csv";

            FtpFileContent fileWithContent = BrickFtp.GetFile(session, filePath);
            Assert.IsTrue(fileWithContent.Content.Length > 0);
        }
        
        [Test]
        public void TestUploadFile()
        {   //POST /files/:path_and_filename

            string file = @"C:\temp\Forecast_MG_GwyntYMor_84_0_2016060400.csv";
            var localFile = new FileInfo(file);
            Session session = BrickFtp.Login(domain, username, password);
            BrickFtp.UploadFile(session, localFile, "/Testtt/NewFolderTest/Forecast_MG_GwyntYMor_84_0_2016060400.csv");
        }

        [Test]
        public void TestDeleteFile()
        {   //DELETE /files/:path_or_filename

            Session session = BrickFtp.Login(domain, username, password);
            BrickFtp.DeleteFile(session, "/Testtt/NewFolderTest/Forecast_MG_GwyntYMor_84_0_2016060400.csv");
        }

        [Test]
        public void TestMoveFile()
        {   //POST /files/:source_path_and_filename

            Session session = BrickFtp.Login(domain, username, password);
            BrickFtp.CopyFileOrFolder(session, "/Testtt/NewFolderTest/Forecast_MG_GwyntYMor_84_0_2016060400.csv", "/Testtt/Forecast_MG_GwyntYMor_84_0_2016060400.csv", false);
        }

        [Test]
        public void TestMoveFolder()
        {   //POST /files/:source_path_and_filename

            Session session = BrickFtp.Login(domain, username, password);
            BrickFtp.CopyFileOrFolder(session, "/Testtt/NewFolderTest/YetAnotherTestFolder", "/Testtt/YetAnotherTestFolder", false);
        }

        [Test]
        public void TestCopyFile()
        {   //POST /files/:source_path_and_filename

            Session session = BrickFtp.Login(domain, username, password);
            BrickFtp.CopyFileOrFolder(session, "/Testtt/Forecast_MG_GwyntYMor_84_0_2016060400.csv", "/Testtt/NewFolderTest/Forecast_MG_GwyntYMor_84_0_2016060400.csv");
        }

        [Test]
        public void TestCopyFolder()
        {   //POST /files/:source_path_and_filename

            Session session = BrickFtp.Login(domain, username, password);
            BrickFtp.CopyFileOrFolder(session, "/Testtt/YetAnotherTestFolder", "/Testtt/NewFolderTest/YetAnotherTestFolder");
        }
    }
