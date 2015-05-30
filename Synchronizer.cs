using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Contacts;
using Google.GData.Client;
using Google.GData.Contacts;

namespace GoogleContactSync
{
    class Synchronizer
    {
        internal static string UserName;
        public ContactsRequest ContactsRequest { get; private set; }
        public Collection<Contact> GoogleContacts { get; private set; }

        string Folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\GoogleContactSyncSeta\\";

        public void LoginToGoogle(string username)
        {
            UserName = username;

            //OAuth2 for all services
            List<String> scopes = new List<string>();

            //Contacts-Scope
            scopes.Add("https://www.google.com/m8/feeds");

            ////Notes-Scope
            //scopes.Add("https://docs.google.com/feeds/");
            ////scopes.Add("https://docs.googleusercontent.com/");
            ////scopes.Add("https://spreadsheets.google.com/feeds/");

            ////Calendar-Scope
            ////scopes.Add("https://www.googleapis.com/auth/calendar");
            //scopes.Add(CalendarService.Scope.Calendar);

            //take user credentials
            UserCredential credential;

            //load client secret from ressources
            byte[] jsonSecrets = Properties.Resources.client_secrets;

            using (var stream = new MemoryStream(jsonSecrets))
            {

                var fDs = new FileDataStore(Folder + "\\Authen\\" + UserName, true);

                var clientSecrets = GoogleClientSecrets.Load(stream);

                credential = GCSMOAuth2WebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets.Secrets,
                    scopes.ToArray(),
                    username,
                    CancellationToken.None,
                    fDs).
                    Result;

                var initializer = new Google.Apis.Services.BaseClientService.Initializer();
                initializer.HttpClientInitializer = credential;

                OAuth2Parameters parameters = new OAuth2Parameters
                {
                    ClientId = clientSecrets.Secrets.ClientId,
                    ClientSecret = clientSecrets.Secrets.ClientSecret,

                    // Note: AccessToken is valid only for 60 minutes
                    AccessToken = credential.Token.AccessToken,
                    RefreshToken = credential.Token.RefreshToken
                };

                RequestSettings settings = new RequestSettings("Google Contact Sync - Seta International", parameters);

                ContactsRequest = new ContactsRequest(settings);
            }

        }

        public void LoadContacts()
        {
            LoadGoogleContacts();
        }

        private void LoadGoogleContacts()
        {
            LoadGoogleContacts(null);
        }

        private Contact LoadGoogleContacts(AtomId id)
        {
            string message = "Error Loading Google Contacts. Cannot connect to Google.\r\nPlease ensure you are connected to the internet. If you are behind a proxy, change your proxy configuration!";

            Contact ret = null;
            try
            {
                GoogleContacts = new Collection<Contact>();

                ContactsQuery query = new ContactsQuery(ContactsQuery.CreateContactsUri("default"));
                query.NumberToRetrieve = 256;
                query.StartIndex = 0;

                //Group group = GetGoogleGroupByName(myContactsGroup);
                //if (group != null)
                //    query.Group = group.Id;

                //query.ShowDeleted = false;
                //query.OrderBy = "lastmodified";

                Feed<Contact> feed = ContactsRequest.Get<Contact>(query);

                while (feed != null)
                {
                    foreach (Contact a in feed.Entries)
                    {
                        GoogleContacts.Add(a);
                        if (id != null && id.Equals(a.ContactEntry.Id))
                            ret = a;
                    }
                    query.StartIndex += query.NumberToRetrieve;
                    feed = ContactsRequest.Get<Contact>(feed, FeedRequestType.Next);
                }

                Logger.Log(UserName + " : " + GoogleContacts.Count + " contact(s)", EventType.Debug);
            }
            catch (System.Net.WebException ex)
            {
                Logger.Log(ex.Message, EventType.Error);

                //throw new GDataRequestException(message, ex);
            }
            catch (System.NullReferenceException ex)
            {
                Logger.Log(ex.Message, EventType.Error);

                //Logger.Log(message, EventType.Error);
                //throw new GDataRequestException(message, new System.Net.WebException("Error accessing feed", ex));
            }

            return ret;
        }

        //public Group GetGoogleGroupByName(string name)
        //{
        //    foreach (Group group in GoogleGroups)
        //    {
        //        if (group.Title == name)
        //            return group;
        //    }
        //    return null;
        //}
    }
}
