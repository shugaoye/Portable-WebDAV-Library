﻿using DecaTec.WebDav.WebDavArtifacts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace DecaTec.WebDav
{
    /// <summary>
    /// Class for WebDAV sessions.
    /// </summary>
    /// <remarks>
    /// <para>This class acts as an abstraction layer between the application and the <see cref="DecaTec.WebDav.WebDavClient"/>, which is used to communicate with the WebDAV server.</para>
    /// <para>If you want to communicate with the WebDAV server directly, you should use the <see cref="DecaTec.WebDav.WebDavClient"/>.</para>
    /// <para>The WebDavSession can be used with a base URL/<see cref="System.Uri"/>. If such a base URL/<see cref="System.Uri"/> is specified, all subsequent operations involving an 
    /// URL/<see cref="System.Uri"/> will be relative to this base URL/<see cref="System.Uri"/>.
    /// If no base URL/<see cref="System.Uri"/> is specified, all operations has the be called with an absolute URL/<see cref="System.Uri"/>.</para>
    /// </remarks>
    /// <example>See the following code to list the content of a directory with the WebDavSession:
    /// <code>
    /// // You have to add a reference to DecaTec.WebDav.Uwp.dll.
    /// //
    /// // The base URL of the WebDAV server.
    /// var webDavServerUrl = "http://www.myserver.com/webdav/";
    ///
    /// // Specify the user credentials and use it to create a WebDavSession instance.
    /// var credentials = new PasswordCredential(webDavServerUrl, "MyUsername", "MyPassword");
    /// var webDavSession = new WebDavSession(@"http://www.myserver.com/webdav/", credentials);
    /// var items = await webDavSession.ListAsync(@"MyFolder/");
    ///
    /// foreach (var item in items)
    /// {
    ///     // Handle the response (list of WebDavSessionListItems), e.g item.Uri is the URL of an item (folder or file).
    /// }
    /// </code>
    /// <para></para>
    /// See the following code which uses locking with a WebDavSession:
    /// <code>
    /// // The base URL of the WebDAV server.
    /// var webDavServerUrl = "http://www.myserver.com/webdav/";
    /// 
    /// // Specify the user credentials and use it to create a WebDavSession instance.
    /// var credentials = new PasswordCredential(webDavServerUrl, "MyUsername", "MyPassword");
    /// var webDavSession = new WebDavSession(@"http://www.myserver.com/webdav/", credentials);
    /// await webDavSession.LockAsync(@"MyFolder/");
    ///
    /// // Create new folder and delete it.
    /// // You DO NOT have to care about that the folder is locked (i.e. you do not have to submit a lock token).
    /// // This is all handled by the WebDavSession itself.
    /// await webDavSession.CreateDirectoryAsync("MyFolder/NewFolder");
    /// await webDavSession.DeleteAsync("MyFolder/NewFolder");
    ///
    /// // Unlock the folder again.
    /// await webDavSession.UnlockAsync(@"MyFolder/");
    ///
    /// // You should always call Dispose on the WebDavSession when it is not longer needed.
    /// // During Dispose, all locks held by the WebDavSession will be automatically unlocked.
    /// webDavSession.Dispose();
    /// </code>
    /// </example>
    /// <seealso cref="DecaTec.WebDav.WebDavClient"/>
    public class WebDavSession : IDisposable
    {
        private readonly WebDavClient webDavClient;
        private readonly ConcurrentDictionary<Uri, PermanentLock> permanentLocks;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of WebDavSession with a default <see cref="System.Net.Http.HttpClientHandler"/>.
        /// </summary>
        /// <param name="passwordCredential">The <see cref="Windows.Security.Credentials.PasswordCredential"/> to use.</param>
        public WebDavSession(PasswordCredential passwordCredential)
            : this(string.Empty, new HttpBaseProtocolFilter() { ServerCredential = passwordCredential })
        {
        }

        /// <summary>
        /// Initializes a new instance of WebDavSession with the given base URL and a default <see cref="System.Net.Http.HttpClientHandler"/>.
        /// </summary>
        /// <param name="baseUrl">The base URL to use for this WebDavSession.</param>
        /// <param name="passwordCredential">The <see cref="Windows.Security.Credentials.PasswordCredential"/> to use.</param>
        public WebDavSession(string baseUrl, PasswordCredential passwordCredential)
            : this(new Uri(baseUrl), new HttpBaseProtocolFilter() { ServerCredential = passwordCredential })
        {
        }

        /// <summary>
        /// Initializes a new instance of WebDavSession with the given base URI and a default <see cref="System.Net.Http.HttpClientHandler"/>.
        /// </summary>
        /// <param name="baseUri">The base URI to use for this WebDavSession.</param>
        /// <param name="passwordCredential">The <see cref="Windows.Security.Credentials.PasswordCredential"/> to use.</param>
        public WebDavSession(Uri baseUri, PasswordCredential passwordCredential)
            : this(baseUri, new HttpBaseProtocolFilter() { ServerCredential = passwordCredential })
        {
        }

        /// <summary>
        /// Initializes a new instance of WebDavSession with the <see cref="System.Net.Http.HttpMessageHandler"/> specified.
        /// </summary>
        /// <param name="httpFilter">The <see cref="Windows.Web.Http.Filters.IHttpFilter"/> to use with this WebDavSession.</param>
        /// <remarks>If credentials are needed, these are part of the <see cref="System.Net.Http.HttpMessageHandler"/> and are specified with it.</remarks>
        public WebDavSession(IHttpFilter httpFilter)
            : this(string.Empty, httpFilter)
        {
        }

        /// <summary>
        /// Initializes a new instance of WebDavSession with the <see cref="System.Net.Http.HttpMessageHandler"/> specified.
        /// </summary>
        /// <param name="baseUrl">The base URL to use for this WebDavSession.</param>
        /// <param name="httpFilter">The <see cref="Windows.Web.Http.Filters.IHttpFilter"/> to use with this WebDavSession.</param>
        /// <remarks>If credentials are needed, these are part of the <see cref="System.Net.Http.HttpMessageHandler"/> and are specified with it.</remarks>
        public WebDavSession(string baseUrl, IHttpFilter httpFilter)
            : this(string.IsNullOrEmpty(baseUrl) ? null : new Uri(baseUrl), httpFilter)
        {
        }

        /// <summary>
        /// Initializes a new instance of WebDavSession with the given base URI and the <see cref="System.Net.Http.HttpMessageHandler"/> specified.
        /// </summary>
        /// <param name="baseUri">The base URI to use for this WebDavSession.</param>
        /// <param name="httpFilter">The <see cref="Windows.Web.Http.Filters.IHttpFilter"/> to use with this WebDavSession.</param>
        /// <remarks>If credentials are needed, these are part of the <see cref="System.Net.Http.HttpMessageHandler"/> and are specified with it.</remarks>
        public WebDavSession(Uri baseUri, IHttpFilter httpFilter)
        {
            this.permanentLocks = new ConcurrentDictionary<Uri, PermanentLock>();
            this.webDavClient = CreateWebDavClient(httpFilter);
            this.BaseUri = baseUri;
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Gets or sets the base <see cref="System.Uri"/> of this WebDavSession.
        /// </summary>
        public Uri BaseUri
        {
            get;
            set;
        }

        #endregion Properties

        #region Public methods

        #region Copy

        /// <summary>
        /// Copies a resource from the source URL to the destination URL (without overwriting).
        /// </summary>
        /// <param name="sourceUrl">The source URL.</param>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> CopyAsync(string sourceUrl, string destinationUrl)
        {
            return await CopyAsync(new Uri(sourceUrl, UriKind.RelativeOrAbsolute), new Uri(destinationUrl, UriKind.RelativeOrAbsolute), false);
        }

        /// <summary>
        /// Copies a resource from the source URI to the destination URI (without overwriting).
        /// </summary>
        /// <param name="sourceUri">The source URI.</param>
        /// <param name="destinationUri">The destination URI.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> CopyAsync(Uri sourceUri, Uri destinationUri)
        {
            return await CopyAsync(sourceUri, destinationUri, false);
        }

        /// <summary>
        /// Copies a resource from the source URL to the destination URL.
        /// </summary>
        /// <param name="sourceUrl">The source URL.</param>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> CopyAsync(string sourceUrl, string destinationUrl, bool overwrite)
        {
            return await CopyAsync(new Uri(sourceUrl, UriKind.RelativeOrAbsolute), new Uri(destinationUrl, UriKind.RelativeOrAbsolute), overwrite);
        }

        /// <summary>
        /// Copies a resource from the source URI to the destination URI.
        /// </summary>
        /// <param name="sourceUri">The source URI.</param>
        /// <param name="destinationUri">The destination URI.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> CopyAsync(Uri sourceUri, Uri destinationUri, bool overwrite)
        {
            sourceUri = UrlHelper.GetAbsoluteUriWithTrailingSlash(this.BaseUri, sourceUri);
            destinationUri = UrlHelper.GetAbsoluteUriWithTrailingSlash(this.BaseUri, destinationUri);
            var lockToken = GetAffectedLockToken(destinationUri);
            var response = await this.webDavClient.CopyAsync(sourceUri, destinationUri, overwrite, WebDavDepthHeaderValue.Infinity, lockToken);
            return response.IsSuccessStatusCode;
        }

        #endregion Copy

        #region Create directory

        /// <summary>
        /// Creates a directory at the URL specified.
        /// </summary>
        /// <param name="url">The URL of the directory to create.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> CreateDirectoryAsync(string url)
        {
            return await CreateDirectoryAsync(new Uri(url, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Creates a directory at the URI specified.
        /// </summary>
        /// <param name="uri">The URI of the directory to create.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> CreateDirectoryAsync(Uri uri)
        {
            uri = UrlHelper.GetAbsoluteUriWithTrailingSlash(this.BaseUri, uri);
            var lockToken = GetAffectedLockToken(uri);
            var response = await this.webDavClient.MkcolAsync(uri, lockToken);
            return response.IsSuccessStatusCode;
        }

        #endregion Create directory

        #region Delete

        /// <summary>
        /// Deletes a directory or file at the URL specified.
        /// </summary>
        /// <param name="url">The URL of the directory or file to delete.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> DeleteAsync(string url)
        {
            return await DeleteAsync(new Uri(url, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Deletes a directory or file at the URI specified.
        /// </summary>
        /// <param name="uri">The URI of the directory or file to delete.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> DeleteAsync(Uri uri)
        {
            uri = UrlHelper.GetAbsoluteUriWithTrailingSlash(this.BaseUri, uri);
            var lockToken = GetAffectedLockToken(uri);
            var response = await this.webDavClient.DeleteAsync(uri, lockToken);
            return response.IsSuccessStatusCode;
        }

        #endregion Delete

        #region Download file

        /// <summary>
        /// Downloads a file from the URL specified.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="localStream">The stream to save the file to.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> DownloadFileAsync(string url, Stream localStream)
        {
            return await DownloadFileAsync(new Uri(url, UriKind.RelativeOrAbsolute), localStream);
        }

        /// <summary>
        ///  Downloads a file from the URI specified.
        /// </summary>
        /// <param name="uri">The URI of the file to download.</param>
        /// <param name="localStream">The stream to save the file to.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> DownloadFileAsync(Uri uri, Stream localStream)
        {
            uri = UrlHelper.GetAbsoluteUriWithTrailingSlash(this.BaseUri, uri);
            var response = await this.webDavClient.GetAsync(uri);

            if (response.Content != null)
            {
                try
                {
                    var contentBuffer = await response.Content.ReadAsBufferAsync();
                    var bArr = contentBuffer.ToArray();
                    await localStream.WriteAsync(bArr, 0, (int)contentBuffer.Length);
                    return true;
                }
                catch (IOException)
                {
                    return false;
                }
            }
            else
                return false;
        }

        #endregion Download file

        #region Exists

        /// <summary>
        /// Checks if a file or directory exists at the URL specified.
        /// </summary>
        /// <param name="url">The URL to check.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> ExistsAsync(string url)
        {
            return await ExistsAsync(new Uri(url, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Checks if a file or directory exists at the URI specified.
        /// </summary>
        /// <param name="uri">The URI to check.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> ExistsAsync(Uri uri)
        {
            uri = UrlHelper.GetAbsoluteUriWithTrailingSlash(this.BaseUri, uri);
            var response = await this.webDavClient.HeadAsync(uri);
            return response.IsSuccessStatusCode;
        }

        #endregion Exists

        #region List

        /// <summary>
        /// Retrieves a list of files and directories of the directory at the URL specified.
        /// </summary>
        /// <param name="url">The URL of the directory which content should be listed.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<IList<WebDavSessionListItem>> ListAsync(string url)
        {
            return await ListAsync(new Uri(url, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Retrieves a list of files and directories of the directory at the URI specified.
        /// </summary>
        /// <param name="uri">The URI of the directory which content should be listed.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<IList<WebDavSessionListItem>> ListAsync(Uri uri)
        {
            uri = UrlHelper.GetAbsoluteUriWithTrailingSlash(this.BaseUri, uri);

            // Do not use an allprop here because some WebDav servers will not return the expected results when using allprop.
            var propFind = PropFind.CreatePropFindWithEmptyProperties("ishidden", "displayname", "name", "getcontenttype", "creationdatespecified", "creationdate", "resourcetype", "getLastmodified", "getcontentlength");
            var response = await this.webDavClient.PropFindAsync(uri, WebDavDepthHeaderValue.One, propFind);

            if (response.StatusCode.ToString() != WebDavStatusCode.MultiStatus.ToString())
                throw new WebDavException(string.Format("Error while executing ListAsync (wrong response status code). Expected status code: 207 (MultiStatus); actual status code: {0} ({1})", (int)response.StatusCode, response.StatusCode));

            var multistatus = await WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content);

            var itemList = new List<WebDavSessionListItem>();

            foreach (var responseItem in multistatus.Response)
            {
                var webDavSessionItem = new WebDavSessionListItem();

                Uri href = null;

                if (!string.IsNullOrEmpty(responseItem.Href))
                {
                    if (Uri.TryCreate(responseItem.Href, UriKind.RelativeOrAbsolute, out href))
                        webDavSessionItem.Uri = href;
                }

                // Skip the folder which contents were requested, only add children.
                if (href != null && uri.ToString().EndsWith(href.ToString(), StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (var item in responseItem.Items)
                {
                    var propStat = item as Propstat;

                    if (propStat == null)
                        continue;

                    // Do not add hidden items.
                    if (propStat.Prop.IsHidden == "1")
                        continue;

                    // Naming priority:
                    // 1. displayname
                    // 2. name
                    // 3. (part of) URI.
                    webDavSessionItem.Name = propStat.Prop.DisplayName;

                    if (string.IsNullOrEmpty(webDavSessionItem.Name))
                        webDavSessionItem.Name = propStat.Prop.Name;

                    if (string.IsNullOrEmpty(webDavSessionItem.Name) && href != null)
                        webDavSessionItem.Name = href.ToString().Split('/').Last(x => !string.IsNullOrEmpty(x));

                    if (!string.IsNullOrEmpty(propStat.Prop.GetContentType))
                        webDavSessionItem.ContentType = propStat.Prop.GetContentType;

                    if (propStat.Prop.CreationDateSpecified && !string.IsNullOrEmpty(propStat.Prop.CreationDate))
                        webDavSessionItem.Created = DateTime.Parse(propStat.Prop.CreationDate, CultureInfo.InvariantCulture);

                    webDavSessionItem.IsDirectory = false;

                    if (propStat.Prop.ResourceType != null)
                        webDavSessionItem.IsDirectory = propStat.Prop.ResourceType.Collection != null;

                    if (!string.IsNullOrEmpty(propStat.Prop.GetLastModified))
                        webDavSessionItem.Modified = DateTime.Parse(propStat.Prop.GetLastModified, CultureInfo.InvariantCulture);

                    if (!string.IsNullOrEmpty(propStat.Prop.GetContentLength))
                        webDavSessionItem.Size = long.Parse(propStat.Prop.GetContentLength, CultureInfo.InvariantCulture);
                }

                itemList.Add(webDavSessionItem);
            }

            return itemList;
        }

        #endregion List

        #region Lock

        /// <summary>
        /// Locks a file or directory at the URL specified.
        /// </summary>
        /// <param name="url">The URL of the file or directory to lock.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> LockAsync(string url)
        {
            return await LockAsync(new Uri(url, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        ///  Locks a file or directory at the URL specified.
        /// </summary>
        /// <param name="uri">The URI of the file or directory to lock.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> LockAsync(Uri uri)
        {
            uri = UrlHelper.GetAbsoluteUriWithTrailingSlash(this.BaseUri, uri);

            if (this.permanentLocks.ContainsKey(uri))
                return true; // Lock already set.

            var lockInfo = new LockInfo();
            lockInfo.LockScope = LockScope.CreateExclusiveLockScope();
            lockInfo.LockType = LockType.CreateWriteLockType();
            var response = await this.webDavClient.LockAsync(uri, WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(), WebDavDepthHeaderValue.Infinity, lockInfo);

            if (!response.IsSuccessStatusCode)
                return false; // Lock already exists.

            var lockToken = WebDavHelper.GetLockTokenFromWebDavResponseMessage(response);

            var prop = await WebDavResponseContentParser.ParsePropResponseContentAsync(response.Content);
            var lockDiscovery = prop.LockDiscovery;

            if (lockDiscovery == null)
                return false;

            var lockGranted = lockDiscovery.ActiveLock.FirstOrDefault(x => uri.ToString().EndsWith(UrlHelper.AddTrailingSlash(x.LockRoot.Href), StringComparison.OrdinalIgnoreCase));

            if (lockGranted == null)
                return false;

            var permanentLock = new PermanentLock(this.webDavClient, lockToken, uri, lockGranted.Timeout);

            if (!this.permanentLocks.TryAdd(uri, permanentLock))
                throw new WebDavException("Lock with lock root " + uri.ToString() + " already exists.");

            return response.IsSuccessStatusCode;
        }

        #endregion Lock

        #region Move

        /// <summary>
        /// Moves a file or directory with the specified URL to another URL (without overwrite)
        /// </summary>
        /// <param name="sourceUrl">The URL of the source.</param>
        /// <param name="destinationUrl">The URL of the destination.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> MoveAsync(string sourceUrl, string destinationUrl)
        {
            return await MoveAsync(new Uri(destinationUrl, UriKind.RelativeOrAbsolute), new Uri(destinationUrl, UriKind.RelativeOrAbsolute), false);
        }

        /// <summary>
        /// Moves a file or directory with the specified URI to another URI (without overwrite).
        /// </summary>
        /// <param name="sourceUri">The URI of the source.</param>
        /// <param name="destinationUri">The URL of the destination.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> MoveAsync(Uri sourceUri, Uri destinationUri)
        {
            return await MoveAsync(destinationUri, destinationUri, false);
        }

        /// <summary>
        /// Moves a file or directory with the specified URL to another URL.
        /// </summary>
        /// <param name="sourceUrl">The URL of the source.</param>
        /// <param name="destinationUrl">The URL of the destination.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> MoveAsync(string sourceUrl, string destinationUrl, bool overwrite)
        {
            return await MoveAsync(new Uri(destinationUrl, UriKind.RelativeOrAbsolute), new Uri(destinationUrl, UriKind.RelativeOrAbsolute), overwrite);
        }

        /// <summary>
        /// Moves a file or directory with the specified URI to another URI.
        /// </summary>
        /// <param name="sourceUri">The URI of the source.</param>
        /// <param name="destinationUri">The URI of the destination.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> MoveAsync(Uri sourceUri, Uri destinationUri, bool overwrite)
        {
            sourceUri = UrlHelper.GetAbsoluteUriWithTrailingSlash(this.BaseUri, sourceUri);
            destinationUri = UrlHelper.GetAbsoluteUriWithTrailingSlash(this.BaseUri, destinationUri);
            var lockTokenSource = GetAffectedLockToken(sourceUri);
            var lockTokenDestination = GetAffectedLockToken(destinationUri);
            var response = await this.webDavClient.MoveAsync(sourceUri, destinationUri, overwrite, lockTokenSource, lockTokenDestination);
            return response.IsSuccessStatusCode;
        }

        #endregion Move

        #region Upload file

        /// <summary>
        /// Uploads a file to the URL specified.
        /// </summary>
        /// <param name="url">The URL of the file to upload.</param>
        /// <param name="localStream">The stream containing the file to upload.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> UploadFileAsync(string url, Stream localStream)
        {
            return await UploadFileAsync(new Uri(url, UriKind.RelativeOrAbsolute), localStream);
        }

        /// <summary>
        /// Uploads a file to the URI specified.
        /// </summary>
        /// <param name="uri">The URI of the file to upload.</param>
        /// <param name="localStream">The stream containing the file to upload.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> UploadFileAsync(Uri uri, Stream localStream)
        {
            uri = UrlHelper.GetAbsoluteUriWithTrailingSlash(this.BaseUri, uri);
            var lockToken = GetAffectedLockToken(uri);
            var content = new HttpStreamContent(localStream.AsInputStream());
            var response = await this.webDavClient.PutAsync(uri, content, lockToken);
            return response.IsSuccessStatusCode;
        }

        #endregion Upload file

        #region Unlock

        /// <summary>
        /// Unlocks a file or directory at the URL specified. 
        /// </summary>
        /// <param name="url">The URL of the file or directory to unlock.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> UnlockAsync(string url)
        {
            return await UnlockAsync(new Uri(url, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Unlocks a file or directory at the URI specified. 
        /// </summary>
        /// <param name="uri">The URI of the file or directory to unlock.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<bool> UnlockAsync(Uri uri)
        {
            uri = UrlHelper.GetAbsoluteUriWithTrailingSlash(this.BaseUri, uri);
            PermanentLock permanentLock;

            if (!this.permanentLocks.TryRemove(uri, out permanentLock))
                return false;

            var result = await permanentLock.UnlockAsync();
            var success = result.IsSuccessStatusCode;

            if (!success)
            {
                // Lock couldn't be removed on WebDav server, add it again in the permanent locks.
                if (!this.permanentLocks.TryAdd(uri, permanentLock))
                    throw new WebDavException("Failed to unlock resource " + uri.ToString()); ;
            }

            return success;
        }

        #endregion Unlock

        #endregion Public methods

        #region Private methods

        private static WebDavClient CreateWebDavClient(IHttpFilter httpFilter)
        {
            return new WebDavClient(httpFilter);
        }

        private LockToken GetAffectedLockToken(Uri uri)
        {
            uri = UrlHelper.GetAbsoluteUriWithTrailingSlash(this.BaseUri, uri);

            foreach (var lockItem in this.permanentLocks)
            {
                var lockUrl = lockItem.Key.ToString();
                var testUrl = uri.ToString();

                if (testUrl.StartsWith(lockUrl))
                    return lockItem.Value.LockToken;
            }

            return null;
        }

        #endregion Private methods

        #region Dispose

        bool disposed = false;

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.

                // Unlock all active locks.
                foreach (var pLock in this.permanentLocks)
                {
                    pLock.Value.Dispose();
                }
            }

            // Free any unmanaged objects here.

            disposed = true;
        }

        #endregion Dispose
    }
}