using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DemoApp.Resources;
using ctccrm.OAuthBroker;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DemoApp
{
    public partial class MainPage : PhoneApplicationPage
    {

        public static string CrmServiceUrl = "https://ceeq.crm.dynamics.com/"; // added by user in the settings page

        public string DomainName = "ceeq.onmicrosoft.com"; // added by user in the settings page

        private const string RedirectUri = "http://www.alfapeople.com"; // can be whatewer in a windows phone app

        private const string clientID = "6f5158cf-01b0-43d7-9502-39e98fb50967"; // Registered in Azure instance

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            CRMAuthenticationBroker broker = new CRMAuthenticationBroker(clientID, DomainName, RedirectUri);

            var token = await broker.AcquireToken(AuthBrowser);

            var dataFromRest = await Retrieve(token, new string[] { "Name" }, "AccountSet");

            var dataFromSOAP = await RetrieveMultiple(token, new string[] { "name", "emailaddress1", "telephone1" }, "account");

        }
                /// <summary>
        /// Retrieve entity record data from the organization web service. 
        /// </summary>
        /// <param name="accessToken">The web service authentication access token.</param>
        /// <param name="Columns">The entity attributes to retrieve.</param>
        /// <param name="entity">The target entity for which the data should be retreived.</param>
        /// <returns>Response from the web service.</returns>
        /// <remarks>Builds an OData HTTP request using passed parameters and sends the request to the server.</remarks>
        public static async Task<string> Retrieve(string accessToken, string[] Columns, string entity)
        {
            // Build a list of entity attributes to retrieve as a string.
            string columnsSet = "";
            foreach (string Column in Columns)
            {
                columnsSet += "," + Column;
            }

            // The URL for the OData organization web service.
            string url = CrmServiceUrl + "/XRMServices/2011/OrganizationData.svc/" + entity + "?$select=" + columnsSet.Remove(0, 1) + "";

            // Build and send the HTTP request.
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Method = HttpMethod.Get;

            // Wait for the web service response.
            HttpResponseMessage response;
            response = await httpClient.SendAsync(req);
            var responseBodyAsText = await response.Content.ReadAsStringAsync();

            return responseBodyAsText;
        }

                /// <summary>
        /// Retrieve entity record data from the organization web service. 
        /// </summary>
        /// <param name="accessToken">The web service authentication access token.</param>
        /// <param name="Columns">The entity attributes to retrieve.</param>
        /// <param name="entity">The target entity for which the data should be retreived.</param>
        /// <returns>Response from the web service.</returns>
        /// <remarks>Builds a SOAP HTTP request using passed parameters and sends the request to the server.</remarks>
        public static async Task<string> RetrieveMultiple(string accessToken, string[] Columns, string entity)
        {
            // Build a list of entity attributes to retrieve as a string.
            string columnsSet = string.Empty;
            foreach (string Column in Columns)
            {
                columnsSet += "<b:string>" + Column + "</b:string>";
            }

            // Default SOAP envelope string. This XML code was obtained using the SOAPLogger tool.
            string xmlSOAP =
             @"<s:Envelope xmlns:s='http://schemas.xmlsoap.org/soap/envelope/'>
                <s:Body>
                  <RetrieveMultiple xmlns='http://schemas.microsoft.com/xrm/2011/Contracts/Services' xmlns:i='http://www.w3.org/2001/XMLSchema-instance'>
                    <query i:type='a:QueryExpression' xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts'><a:ColumnSet>
                    <a:AllColumns>false</a:AllColumns><a:Columns xmlns:b='http://schemas.microsoft.com/2003/10/Serialization/Arrays'>" + columnsSet +
                   @"</a:Columns></a:ColumnSet><a:Criteria><a:Conditions /><a:FilterOperator>And</a:FilterOperator><a:Filters /></a:Criteria>
                    <a:Distinct>false</a:Distinct><a:EntityName>" + entity + @"</a:EntityName><a:LinkEntities /><a:Orders />
                    <a:PageInfo><a:Count>0</a:Count><a:PageNumber>0</a:PageNumber><a:PagingCookie i:nil='true' />
                    <a:ReturnTotalRecordCount>false</a:ReturnTotalRecordCount>
                    </a:PageInfo><a:NoLock>false</a:NoLock></query>
                  </RetrieveMultiple>
                </s:Body>
              </s:Envelope>";

            // The URL for the SOAP endpoint of the organization web service.
            string url = CrmServiceUrl + "/XRMServices/2011/Organization.svc/web";

            // Use the RetrieveMultiple CRM message as the SOAP action.
            string SOAPAction = "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/RetrieveMultiple";

            // Create a new HTTP request.
            HttpClient httpClient = new HttpClient();

            // Set the HTTP authorization header using the access token.
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Finish setting up the HTTP request.
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Add("SOAPAction", SOAPAction);
            req.Method = HttpMethod.Post;
            req.Content = new StringContent(xmlSOAP);
            req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml; charset=utf-8");

            // Send the request asychronously and wait for the response.
            HttpResponseMessage response;
            response = await httpClient.SendAsync(req);
            var responseBodyAsText = await response.Content.ReadAsStringAsync();

            return responseBodyAsText;
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}