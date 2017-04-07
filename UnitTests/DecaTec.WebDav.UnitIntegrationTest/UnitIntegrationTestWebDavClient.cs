using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DecaTec.WebDav.WebDavArtifacts;
using System.Net.Http;
using System.IO;
using System.Net;
using RichardSzalay.MockHttp;
using DecaTec.WebDav.UnitTest;
using System.Collections.Generic;

namespace DecaTec.WebDav.UnitIntegrationTest
{   
    [TestClass]
    public class UnitIntegrationTestWebDavClient
    {
        private const string UserName = "testuser";
        private const string Password = "testpassword";
        private const string WebDavRootFolder = @"http://127.0.0.1/webdav";

        private const string ContentTypeApplicationXml = "application/xml; charset=utf-8";

        private const string TestFile = @"TextFile1.txt";
        private const string TestCollection = "TestCollection";

        private WebDavClient CreateWebDavClient(MockHttpMessageHandler mockHandler)
        {
            var credentials = new NetworkCredential(UserName, Password);

            var httpClientHandler = new HttpClientHandler()
            {
                Credentials = credentials,
                PreAuthenticate = true
            };

            var debugHttpMessageHandler = new DebugHttpMessageHandler(httpClientHandler);
            debugHttpMessageHandler.InnerHandler = mockHandler;
            var wdc = new WebDavClient(debugHttpMessageHandler);
            return wdc;
        }

        #region PropFind

        [TestMethod]
        public void UIT_WebDavClient_PropFind_AllProp()
        {
            var mockHandler = new MockHttpMessageHandler();
            var requestContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:propfind xmlns:D=\"DAV:\"><D:allprop /></D:propfind>";
            var requestHeaders = new List<KeyValuePair<string, string>>();
            var xx = WebDavDepthHeaderValue.Infinity.ToString();
            requestHeaders.Add(new KeyValuePair<string, string>(WebDavConstants.Depth, WebDavDepthHeaderValue.Infinity.ToString()));
            requestHeaders.Add(new KeyValuePair<string, string>(HttpHeaderNames.ContentType, ContentTypeApplicationXml));
            var responseContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:multistatus xmlns:D=\"DAV:\"><D:response><D:href>http://127.0.0.1/webdav</D:href><D:propstat><D:status>HTTP/1.1 200 OK</D:status><D:prop><D:getcontenttype/><D:getlastmodified>Fri, 07 Apr 2017 16:56:40 GMT</D:getlastmodified><D:lockdiscovery/><D:ishidden>0</D:ishidden><D:supportedlock><D:lockentry><D:lockscope><D:exclusive/></D:lockscope><D:locktype><D:write/></D:locktype></D:lockentry><D:lockentry><D:lockscope><D:shared/></D:lockscope><D:locktype><D:write/></D:locktype></D:lockentry></D:supportedlock><D:getetag/><D:displayname>/</D:displayname><D:getcontentlanguage/><D:getcontentlength>0</D:getcontentlength><D:iscollection>1</D:iscollection><D:creationdate>2017-04-06T09:32:20.983Z</D:creationdate><D:resourcetype><D:collection/></D:resourcetype></D:prop></D:propstat></D:response></D:multistatus>";
            mockHandler.When(WebDavRootFolder).WithContent(requestContent).WithHeaders(requestHeaders).Respond(HttpStatusCode.OK, new StringContent(responseContent));

            var client = CreateWebDavClient(mockHandler);
            PropFind pf = PropFind.CreatePropFindAllProp();
            var response = client.PropFindAsync(WebDavRootFolder, WebDavDepthHeaderValue.Infinity, pf).Result;
            var propFindResponseSuccess = response.IsSuccessStatusCode;
            var multistatus = WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content).Result;

            Assert.IsTrue(propFindResponseSuccess);
            Assert.IsNotNull(multistatus);
        }

        [TestMethod]
        public void UIT_WebDavClient_PropFind_NamedProperties()
        {
            var mockHandler = new MockHttpMessageHandler();
            var requestContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:propfind xmlns:D=\"DAV:\"><D:prop><D:name /></D:prop></D:propfind>";
            var requestHeaders = new List<KeyValuePair<string, string>>();
            var xx = WebDavDepthHeaderValue.Infinity.ToString();
            requestHeaders.Add(new KeyValuePair<string, string>(WebDavConstants.Depth, WebDavDepthHeaderValue.Infinity.ToString()));
            requestHeaders.Add(new KeyValuePair<string, string>(HttpHeaderNames.ContentType, ContentTypeApplicationXml));
            var responseContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:multistatus xmlns:D=\"DAV:\"><D:response><D:href>http://127.0.0.1/webdav</D:href><D:propstat><D:status>HTTP/1.1 200 OK</D:status><D:prop><D:getcontenttype/><D:getlastmodified>Fri, 07 Apr 2017 16:56:40 GMT</D:getlastmodified><D:lockdiscovery/><D:ishidden>0</D:ishidden><D:supportedlock><D:lockentry><D:lockscope><D:exclusive/></D:lockscope><D:locktype><D:write/></D:locktype></D:lockentry><D:lockentry><D:lockscope><D:shared/></D:lockscope><D:locktype><D:write/></D:locktype></D:lockentry></D:supportedlock><D:getetag/><D:displayname>/</D:displayname><D:getcontentlanguage/><D:getcontentlength>0</D:getcontentlength><D:iscollection>1</D:iscollection><D:creationdate>2017-04-06T09:32:20.983Z</D:creationdate><D:resourcetype><D:collection/></D:resourcetype></D:prop></D:propstat></D:response></D:multistatus>";
            mockHandler.When(WebDavRootFolder).WithContent(requestContent).WithHeaders(requestHeaders).Respond(HttpStatusCode.OK, new StringContent(responseContent));

            var client = CreateWebDavClient(mockHandler);
            PropFind pf = PropFind.CreatePropFindWithEmptyProperties("name");
            var response = client.PropFindAsync(WebDavRootFolder, WebDavDepthHeaderValue.Infinity, pf).Result;
            var propFindResponseSuccess = response.IsSuccessStatusCode;
            var multistatus = WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content).Result;

            Assert.IsTrue(propFindResponseSuccess);
            Assert.IsNotNull(multistatus);
        }

        [TestMethod]
        public void UIT_WebDavClient_PropFind_PropName()
        {
            var mockHandler = new MockHttpMessageHandler();
            var requestContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:propfind xmlns:D=\"DAV:\"><D:propname /></D:propfind>";
            var requestHeaders = new List<KeyValuePair<string, string>>();
            var xx = WebDavDepthHeaderValue.Infinity.ToString();
            requestHeaders.Add(new KeyValuePair<string, string>(WebDavConstants.Depth, WebDavDepthHeaderValue.Infinity.ToString()));
            requestHeaders.Add(new KeyValuePair<string, string>(HttpHeaderNames.ContentType, ContentTypeApplicationXml));
            var responseContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:multistatus xmlns:D=\"DAV:\"><D:response><D:href>http://127.0.0.1/webdav</D:href><D:propstat><D:status>HTTP/1.1 200 OK</D:status><D:prop><D:getcontenttype/><D:getlastmodified/><D:lockdiscovery/><D:ishidden/><D:supportedlock/><D:getetag/><D:displayname/><D:getcontentlanguage/><D:getcontentlength/><D:iscollection/><D:creationdate/><D:resourcetype/></D:prop></D:propstat></D:response></D:multistatus>";
            mockHandler.When(WebDavRootFolder).WithContent(requestContent).WithHeaders(requestHeaders).Respond(HttpStatusCode.OK, new StringContent(responseContent));

            var client = CreateWebDavClient(mockHandler);
            PropFind pf = PropFind.CreatePropFindWithPropName();
            var response = client.PropFindAsync(WebDavRootFolder, WebDavDepthHeaderValue.Infinity, pf).Result;
            var propFindResponseSuccess = response.IsSuccessStatusCode;
            var multistatus = WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content).Result;

            Assert.IsTrue(propFindResponseSuccess);
            Assert.IsNotNull(multistatus);
        }

        #endregion PropFind

        //    #region PropPatch / put / delete file

        //    [TestMethod]
        //    public void UIT_WebDavClient_PropPatch()
        //    {
        //        var client = CreateWebDavClientWithDebugHttpMessageHandler();
        //        var testFile = UriHelper.CombineUrl(this.webDavRootFolder, TestFile, true);

        //        // Put file.
        //        var content = new StreamContent(File.OpenRead(TestFile));
        //        var response = client.PutAsync(testFile, content).Result;
        //        var putResponseSuccess = response.IsSuccessStatusCode;

        //        // PropPatch (set).
        //        var propertyUpdate = new PropertyUpdate();
        //        var set = new Set();
        //        var prop = new Prop();
        //        prop.DisplayName = "TestFileDisplayName";
        //        set.Prop = prop;
        //        propertyUpdate.Items = new object[] { set };
        //        response = client.PropPatchAsync(testFile, propertyUpdate).Result;
        //        var propPatchResponseSuccess = response.IsSuccessStatusCode;

        //        // PropFind.
        //        PropFind pf = PropFind.CreatePropFindWithEmptyProperties("displayname");
        //        response = client.PropFindAsync(testFile, WebDavDepthHeaderValue.Zero, pf).Result;
        //        var propFindResponseSuccess = response.IsSuccessStatusCode;
        //        var multistatus = (Multistatus)WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content).Result;
        //        var displayName = ((Propstat)multistatus.Response[0].Items[0]).Prop.DisplayName;
        //        // IIS ignores display name and always puts the file name as display name.
        //        var displayNameResult = "TestFileDisplayName" == displayName || TestFile == displayName;

        //        // PropPatch (remove).
        //        propertyUpdate = new PropertyUpdate();
        //        var remove = new Remove();
        //        prop = Prop.CreatePropWithEmptyProperties("displayname");
        //        remove.Prop = prop;
        //        propertyUpdate.Items = new object[] { remove };
        //        response = client.PropPatchAsync(testFile, propertyUpdate).Result;
        //        var propPatchRemoveResponseSuccess = response.IsSuccessStatusCode;
        //        multistatus = (Multistatus)WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content).Result;
        //        var multistatusResult = ((Propstat)multistatus.Response[0].Items[0]).Prop.DisplayName;

        //        // Delete file.
        //        response = client.DeleteAsync(testFile).Result;
        //        var deleteResponseSuccess = response.IsSuccessStatusCode;

        //        Assert.IsTrue(putResponseSuccess);
        //        Assert.IsTrue(propPatchResponseSuccess);
        //        Assert.IsTrue(propFindResponseSuccess);
        //        Assert.IsTrue(displayNameResult);
        //        Assert.IsTrue(propPatchRemoveResponseSuccess);
        //        Assert.AreEqual(string.Empty, multistatusResult);
        //        Assert.IsTrue(deleteResponseSuccess);
        //    }

        //    #endregion PropPatch / put / delete file

        //    #region Mkcol / delete collection

        //    [TestMethod]
        //    public void UIT_WebDavClient_Mkcol()
        //    {
        //        var client = CreateWebDavClientWithDebugHttpMessageHandler();
        //        var testCollection = UriHelper.CombineUrl(this.webDavRootFolder, TestCollection, true);

        //        // Create collection.
        //        var response = client.MkcolAsync(testCollection).Result;
        //        var mkColResponseSuccess = response.IsSuccessStatusCode;

        //        // PropFind.
        //        PropFind pf = PropFind.CreatePropFindAllProp();
        //        response = client.PropFindAsync(this.webDavRootFolder, WebDavDepthHeaderValue.Infinity, pf).Result;
        //        var propFindResponseSuccess = response.IsSuccessStatusCode;

        //        var multistatus = (Multistatus)WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content).Result;

        //        bool collectionFound = false;

        //        foreach (var item in multistatus.Response)
        //        {
        //            if (item.Href.EndsWith(TestCollection + "/"))
        //            {
        //                collectionFound = true;
        //                break;
        //            }
        //        }

        //        // Delete collection.
        //        response = client.DeleteAsync(testCollection).Result;
        //        var deleteResponseSuccess = response.IsSuccessStatusCode;

        //        Assert.IsTrue(mkColResponseSuccess);
        //        Assert.IsTrue(propFindResponseSuccess);
        //        Assert.IsTrue(collectionFound);
        //        Assert.IsTrue(deleteResponseSuccess);
        //    }

        //    #endregion Mkcol / delete collection

        //    #region Get

        //    [TestMethod]
        //    public void UIT_WebDavClient_Get()
        //    {
        //        var client = CreateWebDavClientWithDebugHttpMessageHandler();
        //        var testFile = UriHelper.CombineUrl(this.webDavRootFolder, TestFile, true);

        //        // Put file.
        //        var content = new StreamContent(File.OpenRead(TestFile));
        //        var response = client.PutAsync(testFile, content).Result;
        //        var putResponseSuccess = response.IsSuccessStatusCode;

        //        // Get file.
        //        response = client.GetAsync(testFile).Result;
        //        var getResponseSuccess = response.IsSuccessStatusCode;

        //        var responseContent = response.Content.ReadAsStringAsync().Result;
        //        var readResponseContent = response.Content.ReadAsStringAsync().Result;

        //        // Delete file.
        //        response = client.DeleteAsync(testFile).Result;
        //        var deleteResponseSuccess = response.IsSuccessStatusCode;

        //        Assert.IsTrue(putResponseSuccess);
        //        Assert.IsTrue(getResponseSuccess);
        //        Assert.AreEqual("This is a test file for WebDAV.", readResponseContent);
        //        Assert.IsTrue(deleteResponseSuccess);
        //    }

        //    #endregion Get

        //    #region Copy

        //    [TestMethod]
        //    public void UIT_WebDavClient_Copy()
        //    {
        //        var client = CreateWebDavClientWithDebugHttpMessageHandler();
        //        var testCollectionSource = UriHelper.CombineUrl(this.webDavRootFolder, TestCollection, true);
        //        var testCollectionDestination = UriHelper.CombineUrl(this.webDavRootFolder, TestCollection + "2", true);
        //        var testFile = UriHelper.CombineUrl(testCollectionSource, TestFile, true);

        //        // Create source collection.
        //        var response = client.MkcolAsync(testCollectionSource).Result;
        //        var mkColResponseSuccess = response.IsSuccessStatusCode;

        //        // Put file.
        //        var content = new StreamContent(File.OpenRead(TestFile));
        //        response = client.PutAsync(testFile, content).Result;
        //        var putResponseSuccess = response.IsSuccessStatusCode;

        //        // Copy.
        //        response = client.CopyAsync(testCollectionSource, testCollectionDestination).Result;
        //        var copyResponseSuccess = response.IsSuccessStatusCode;

        //        // PropFind.
        //        PropFind pf = PropFind.CreatePropFindAllProp();
        //        response = client.PropFindAsync(testCollectionDestination, WebDavDepthHeaderValue.Infinity, pf).Result;
        //        var propFindResponseSuccess = response.IsSuccessStatusCode;

        //        var multistatus = (Multistatus)WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content).Result;

        //        bool collectionfound = false;

        //        foreach (var item in multistatus.Response)
        //        {
        //            if (item.Href.EndsWith(TestFile))
        //            {
        //                collectionfound = true;
        //                break;
        //            }
        //        }

        //        // Delete source and destination.
        //        response = client.DeleteAsync(testCollectionSource).Result;
        //        var deleteSourceResponseSuccess = response.IsSuccessStatusCode;

        //        response = client.DeleteAsync(testCollectionDestination).Result;
        //        var deleteDestinationResponseSuccess = response.IsSuccessStatusCode;

        //        Assert.IsTrue(mkColResponseSuccess);
        //        Assert.IsTrue(putResponseSuccess);
        //        Assert.IsTrue(copyResponseSuccess);
        //        Assert.IsTrue(propFindResponseSuccess);
        //        Assert.IsTrue(collectionfound);
        //        Assert.IsTrue(deleteSourceResponseSuccess);
        //        Assert.IsTrue(deleteDestinationResponseSuccess);
        //    }

        //    #endregion Copy

        //    #region Move

        //    [TestMethod]
        //    public void UIT_WebDavClient_Move()
        //    {
        //        var client = CreateWebDavClientWithDebugHttpMessageHandler();
        //        var testCollectionSource = UriHelper.CombineUrl(this.webDavRootFolder, TestCollection, true);
        //        var testCollectionDestination = UriHelper.CombineUrl(this.webDavRootFolder, TestCollection + "2", true);
        //        var testFile = UriHelper.CombineUrl(testCollectionSource, TestFile, true);

        //        // Create source collection.
        //        var response = client.MkcolAsync(testCollectionSource).Result;
        //        var mkColResponseSuccess = response.IsSuccessStatusCode;

        //        // Put file.
        //        var content = new StreamContent(File.OpenRead(TestFile));
        //        response = client.PutAsync(testFile, content).Result;
        //        var putResponseSuccess = response.IsSuccessStatusCode;

        //        // Move.
        //        response = client.MoveAsync(testCollectionSource, testCollectionDestination).Result;
        //        var moveResponseSuccess = response.IsSuccessStatusCode;

        //        // PropFind.
        //        PropFind pf = PropFind.CreatePropFindAllProp();
        //        response = client.PropFindAsync(this.webDavRootFolder, WebDavDepthHeaderValue.Infinity, pf).Result;
        //        var propFindResponseSuccess = response.IsSuccessStatusCode;

        //        var multistatus = (Multistatus)WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content).Result;

        //        bool foundCollection1 = false;
        //        bool foundCollection2 = false;

        //        foreach (var item in multistatus.Response)
        //        {
        //            if (item.Href.EndsWith(TestCollection + "/"))
        //                foundCollection1 = true;

        //            if (item.Href.EndsWith(TestCollection + "2/"))
        //                foundCollection2 = true;
        //        }

        //        // Delete source and destination.
        //        // Delete file.
        //        response = client.DeleteAsync(testCollectionDestination).Result;
        //        var deleteResponseSuccess = response.IsSuccessStatusCode;

        //        Assert.IsTrue(mkColResponseSuccess);
        //        Assert.IsTrue(putResponseSuccess);
        //        Assert.IsTrue(moveResponseSuccess);
        //        Assert.IsTrue(propFindResponseSuccess);
        //        Assert.IsFalse(foundCollection1);
        //        Assert.IsTrue(foundCollection2);
        //        Assert.IsTrue(deleteResponseSuccess);
        //    }

        //    #endregion Move

        //    #region Lock / unlock

        //    [TestMethod]
        //    public void UIT_WebDavClient_LockRefreshLockUnlock()
        //    {
        //        var client = CreateWebDavClientWithDebugHttpMessageHandler();

        //        // Lock.
        //        var lockInfo = new LockInfo();
        //        lockInfo.LockScope = LockScope.CreateExclusiveLockScope();
        //        lockInfo.LockType = LockType.CreateWriteLockType();
        //        lockInfo.OwnerHref = "test@test.com";
        //        var response = client.LockAsync(this.webDavRootFolder, WebDavTimeoutHeaderValue.CreateWebDavTimeout(TimeSpan.FromSeconds(15)), WebDavDepthHeaderValue.Infinity, lockInfo).Result;
        //        var lockResponseSuccess = response.IsSuccessStatusCode;
        //        LockToken lockToken = WebDavHelper.GetLockTokenFromWebDavResponseMessage(response);

        //        // Refresh lock.
        //        response = client.RefreshLockAsync(this.webDavRootFolder, WebDavTimeoutHeaderValue.CreateWebDavTimeout(TimeSpan.FromSeconds(10)), lockToken).Result;
        //        var refreshLockResponseSuccess = response.IsSuccessStatusCode;

        //        // Unlock.
        //        response = client.UnlockAsync(this.webDavRootFolder, lockToken).Result;
        //        var unlockResponseSuccess = response.IsSuccessStatusCode;

        //        Assert.IsTrue(lockResponseSuccess);
        //        Assert.IsNotNull(lockToken);
        //        Assert.IsTrue(refreshLockResponseSuccess);
        //        Assert.IsTrue(unlockResponseSuccess);
        //    }

        //    [TestMethod]
        //    public void UIT_WebDavClient_LockAndPutWithoutToken()
        //    {
        //        var client = CreateWebDavClientWithDebugHttpMessageHandler();

        //        // Lock.
        //        var lockInfo = new LockInfo();
        //        lockInfo.LockScope = LockScope.CreateExclusiveLockScope();
        //        lockInfo.LockType = LockType.CreateWriteLockType();
        //        lockInfo.OwnerHref = "test@test.com";
        //        var response = client.LockAsync(this.webDavRootFolder, WebDavTimeoutHeaderValue.CreateWebDavTimeout(TimeSpan.FromSeconds(15)), WebDavDepthHeaderValue.Infinity, lockInfo).Result;
        //        var lockResponseSuccess = response.IsSuccessStatusCode;

        //        LockToken lockToken = WebDavHelper.GetLockTokenFromWebDavResponseMessage(response);

        //        // Put file (without lock token) -> this should fail.
        //        var content = new StreamContent(File.OpenRead(TestFile));
        //        var requestUrl = UriHelper.CombineUrl(this.webDavRootFolder, TestFile, true);
        //        response = client.PutAsync(requestUrl, content).Result;
        //        var putResponseSuccess = response.IsSuccessStatusCode;

        //        // Unlock.
        //        response = client.UnlockAsync(this.webDavRootFolder, lockToken).Result;
        //        var unlockResponseSuccess = response.IsSuccessStatusCode;

        //        Assert.IsTrue(lockResponseSuccess);
        //        Assert.IsNotNull(lockToken);
        //        Assert.IsFalse(putResponseSuccess);
        //        Assert.IsTrue(unlockResponseSuccess);
        //    }

        //    [TestMethod]
        //    public void UIT_WebDavClient_LockAndPutWithToken()
        //    {
        //        var client = CreateWebDavClientWithDebugHttpMessageHandler();

        //        // Lock.
        //        var lockInfo = new LockInfo();
        //        lockInfo.LockScope = LockScope.CreateExclusiveLockScope();
        //        lockInfo.LockType = LockType.CreateWriteLockType();
        //        lockInfo.OwnerHref = "test@test.com";
        //        var response = client.LockAsync(this.webDavRootFolder, WebDavTimeoutHeaderValue.CreateWebDavTimeout(TimeSpan.FromSeconds(15)), WebDavDepthHeaderValue.Infinity, lockInfo).Result;
        //        var lockResponseSuccess = response.IsSuccessStatusCode;
        //        LockToken lockToken = WebDavHelper.GetLockTokenFromWebDavResponseMessage(response);

        //        // Put file.
        //        var content = new StreamContent(File.OpenRead(TestFile));
        //        var requestUrl = UriHelper.CombineUrl(this.webDavRootFolder, TestFile, true);
        //        response = client.PutAsync(requestUrl, content, lockToken).Result;
        //        var putResponseSuccess = response.IsSuccessStatusCode;

        //        // Delete file.
        //        response = client.DeleteAsync(requestUrl, lockToken).Result;
        //        var deleteResponseSuccess = response.IsSuccessStatusCode;

        //        // Unlock.
        //        response = client.UnlockAsync(this.webDavRootFolder, lockToken).Result;
        //        var unlockResponseSuccess = response.IsSuccessStatusCode;

        //        Assert.IsTrue(lockResponseSuccess);
        //        Assert.IsNotNull(lockToken);
        //        Assert.IsTrue(putResponseSuccess);
        //        Assert.IsTrue(deleteResponseSuccess);
        //        Assert.IsTrue(unlockResponseSuccess);
        //    }

        //    #endregion Lock / unlock
    }
}