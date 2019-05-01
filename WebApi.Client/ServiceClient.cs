using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace AKSoftware.WebApi.Client
{
    public class ServiceClient
    {
        public string AccessToken { get; set; }
        public string URL { get; set; }

        // constructor to intialize the values 
        public ServiceClient(string uRL)
        {
            URL = uRL;
        }

        // Default constructor 
        public ServiceClient()
        {

        }

        #region ProfileMethods

        // Method to login and get access token

        /// <summary>
        /// Login a user by verifying the username and password and return the access token for this login
        /// </summary>
        /// <param name="userName">Username </param>
        /// <param name="password">Password </param>
        /// <returns></returns>
        public async Task<bool> LoginUser(string userName, string password)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, URL + "token");

            List<KeyValuePair<string, string>> keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("username", userName));
            keyValues.Add(new KeyValuePair<string, string>("password", password));
            keyValues.Add(new KeyValuePair<string, string>("grant_type", "password"));

            HttpContent content = new FormUrlEncodedContent(keyValues);
            message.Content = content;

            HttpClient client = new HttpClient();

            var response = await client.SendAsync(message);
            string jsonData = await response.Content.ReadAsStringAsync();

            JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
            string accessToken = obj.Value<string>("access_token");
            AccessToken = accessToken;

            if (accessToken != "")
            {
                return false;
            }
            else
                return true;
        }


        // Method to login and get a object with access token 

        /// <summary>
        /// Login a user by verifying a username and password but return it with an object that represents a user in 
        /// your database in the web api 
        /// </summary>
        /// <param name="methodUrl">the URL of the user or object that you want to get</param>
        /// <param name="userName">The username of the user</param>
        /// <param name="password">The password of the user</param>
        /// <returns></returns>
        public async Task<object> LoginUser(string methodUrl, string userName, string password)
        {
            // Get the access token 
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, URL + "token");

            List<KeyValuePair<string, string>> keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("username", userName));
            keyValues.Add(new KeyValuePair<string, string>("password", password));
            keyValues.Add(new KeyValuePair<string, string>("grant_type", "password"));

            HttpContent content = new FormUrlEncodedContent(keyValues);
            message.Content = content;

            HttpClient client = new HttpClient();

            var response = await client.SendAsync(message);
            string jsonData = await response.Content.ReadAsStringAsync();

            JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
            string accessToken = obj.Value<string>("access_token");
            AccessToken = accessToken;


            if (accessToken != "")
            {
                // Get a model 
                client = new HttpClient();
                // Authoirze the user 
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                UserInfo userInfo = new UserInfo
                {
                    Username = userName,
                    Password = password
                };

                // Serialize the user info data to put it inside the content 
                string userInfoJsonData = JsonConvert.SerializeObject(userInfo);

                HttpContent userContent = new StringContent(userInfoJsonData);

                // Send a get request to get the user
                var userResponse = await client.PostAsync(URL + methodUrl, userContent);
                var userJsonData = await userResponse.Content.ReadAsStringAsync();

                var userModel = JsonConvert.DeserializeObject<object>(userJsonData);

                return userModel;
            }
            else
                return null;
        }

        // Method to register 

        /// <summary>
        /// Register a new user in your database by send the username, password and the confirmation of the password
        /// </summary>
        /// <param name="userName">The username of the user</param>
        /// <param name="password">The password of the user</param>
        /// <param name="confirmPassword">The confiramtion of the user's password</param>
        /// <returns></returns>
        public async Task<bool> RegisterUser(string userName, string password, string confirmPassword)
        {
            HttpClient client = new HttpClient();

            // Create the content 
            RegisterBindingModel registerData = new RegisterBindingModel
            {
                Email = userName,
                Password = password,
                ConfirmPassword = confirmPassword
            };

            string jsonData = JsonConvert.SerializeObject(registerData);

            HttpContent content = new StringContent(jsonData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync(URL + "api/account/register", content);

            return response.IsSuccessStatusCode;

        }
        #endregion


        // Methods to invoke normal WebApi methods
        #region NormalMethods
        // Method to invoke a post method 

        /// <summary>
        /// Make an unauthorized POST request to your web api and return a specific model 
        /// </summary>
        /// <typeparam name="T">Your data type that you want to send and get</typeparam>
        /// <param name="methodUrl">The Method name in the URL of your data source in the web api</param>
        /// <param name="model"> The model or object that you want to send</param>
        /// <returns></returns>
        public async Task<T> Post<T>(string methodUrl, object model)
        {
            HttpClient client = new HttpClient();

            // Now serialzize the object to json 
            string jsonData = JsonConvert.SerializeObject(model);

            // Create a content 
            HttpContent content = new StringContent(jsonData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            // Make a request 
            var response = await client.PostAsync(URL + methodUrl, content);
            var responseAsString = await response.Content.ReadAsStringAsync();

            // Deserialize the coming object into a T object 
            T obj = JsonConvert.DeserializeObject<T>(responseAsString);

            return obj;
        }

        // Method to invoke a get method 

        /// <summary>
        /// Make an unauthorized GET request to your web api and return a specific model
        /// </summary>
        /// <typeparam name="T">Your data type that you want to get</typeparam>
        /// <param name="methodUrl">The Method name in the URL of your data source in the web api</param>
        /// <returns></returns>
        public async Task<T> Get<T>(string methodUrl)
        {
            HttpClient client = new HttpClient();

            // Send a request and get the response 
            var response = await client.GetAsync(URL + methodUrl);
            // Read the data 
            var jsonData = await response.Content.ReadAsStringAsync();

            T obj = JsonConvert.DeserializeObject<T>(jsonData);

            return obj;
        }

        /// <summary>
        /// Make an unauthorized GET request to your web api and return the result as a raw json
        /// </summary>
        /// <param name="methodUrl">The Method name in the URL of your data source in the web api</param>
        /// <returns></returns>
        public async Task<string> GetJsonResult(string methodUrl)
        {
            HttpClient client = new HttpClient();

            // Send requant the get the response 
            var response = await client.GetAsync(URL + methodUrl);
            // Read the data 
            var jsonData = await response.Content.ReadAsStringAsync();
            return jsonData;
        }

        // Method to invoke a put method 

        /// <summary>
        /// Make an unauthorized PUT request to your web api to alter a specific data model
        /// </summary>
        /// <typeparam name="T">Your data type that you want to get</typeparam>
        /// <param name="methodUrl">The Method name in the URL of your data source in the web api</param>
        /// <param name="model">The model or object that you want to alter</param>
        /// <param name="id">The id of the object that you want to alter</param>
        /// <returns></returns>
        public async Task<bool> Put<T>(string methodUrl, object model, int id)
        {
            HttpClient client = new HttpClient();

            // Serialize the object
            string jsonData = JsonConvert.SerializeObject(model);

            HttpContent content = new StringContent(jsonData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await client.PutAsync(URL + methodUrl + "/" + id, content);

            return response.IsSuccessStatusCode;
        }


        /// <summary>
        /// Make an unauthorized PUT request to your web api to alter a specific data model
        /// </summary>
        /// <typeparam name="T">Your data type that you want to get</typeparam>
        /// <param name="methodUrl">The Method name in the URL of your data source in the web api</param>
        /// <param name="model">The model or object that you want to alter</param>
        /// <param name="id">The id of the object that you want to alter</param>
        /// <returns></returns>
        public async Task<T> Put<T>(string methodUrl, object model)
        {
            HttpClient client = new HttpClient();

            // Serialize the object
            string jsonData = JsonConvert.SerializeObject(model);

            HttpContent content = new StringContent(jsonData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await client.PutAsync(URL + methodUrl, content);

            // Read the data 
            var responseString = await response.Content.ReadAsStringAsync();

            T obj = JsonConvert.DeserializeObject<T>(responseString);

            return obj; 
        }

        // Method to invoke the delete moethod 

        /// <summary>
        /// Make an unauthorized DELETE request to your web api to delete a specific object
        /// </summary>
        /// <typeparam name="T">Your data type that you want to get</typeparam>
        /// <param name="methodUrl">The Method name in the URL of your data source in the web api</param>
        /// <param name="id">The id of the object that you want to delete</param>
        /// <returns></returns>
        public async Task<T> Delete<T>(string methodUrl, int id)
        {
            HttpClient client = new HttpClient();

            var response = await client.DeleteAsync(URL + methodUrl + "/" + id);
            var jsonData = await response.Content.ReadAsStringAsync();

            T obj = JsonConvert.DeserializeObject<T>(jsonData);

            return obj;
        }


        /// <summary>
        /// Make an unauthorized DELETE request to your web api to delete a specific object
        /// </summary>
        /// <typeparam name="T">Your data type that you want to get</typeparam>
        /// <param name="methodUrl">The Method name in the URL of your data source in the web api</param>
        /// <returns></returns>
        public async Task<T> Delete<T>(string methodUrl, object model)
        {
            HttpClient client = new HttpClient();
            
            string jsonData = JsonConvert.SerializeObject(model);
            HttpContent content = new StringContent(jsonData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await client.DeleteAsync(URL + methodUrl);
            var responseString = await response.Content.ReadAsStringAsync();

            T obj = JsonConvert.DeserializeObject<T>(jsonData);

            return obj;
        }
        #endregion

        // Methods to invoke protected WebApi methods 
        #region ProtectedMethods
        // Method to invoke a post method 

        /// <summary>
        /// Make a protected POST request to your web api and return a specific model 
        /// </summary>
        /// <typeparam name="T">Your data type that you want to send and get</typeparam>
        /// <param name="methodUrl">The Method name in the URL of your data source in the web api</param>
        /// <param name="model"> The model or object that you want to send</param>
        /// <returns></returns>
        public async Task<T> PostProtected<T>(string methodUrl, object model)
        {
            HttpClient client = new HttpClient();

            // Set the access token for the request 
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);

            // Now serialzize the object to json 
            string jsonData = JsonConvert.SerializeObject(model);

            // Create a content 
            HttpContent content = new StringContent(jsonData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            // Make a request 
            var response = await client.PostAsync(URL + methodUrl, content);
            var responseAsString = await response.Content.ReadAsStringAsync();

            // Deserialize the coming object into a T object 
            T obj = JsonConvert.DeserializeObject<T>(responseAsString);

            return obj;
        }

        // Method to invoke a get method protected

        /// <summary>
        /// Make a protected GET request to your web api and return a specific model
        /// </summary>
        /// <typeparam name="T">Your data type that you want to get</typeparam>
        /// <param name="methodUrl">The Method name in the URL of your data source in the web api</param>
        /// <returns></returns>
        public async Task<T> GetProtected<T>(string methodUrl)
        {
            HttpClient client = new HttpClient();

            // Set the access token for the request 
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
            // Send a request and get the response 
            var response = await client.GetAsync(URL + methodUrl);
            // Read the data 
            var jsonData = await response.Content.ReadAsStringAsync();

            T obj = JsonConvert.DeserializeObject<T>(jsonData);

            return obj;
        }

        // Method to invoke the Get and return the json result without desiralizing 
        /// <summary>
        /// Make an authorized GET Request to your data source 
        /// </summary>
        /// <param name="methodUrl">The name of the Method in the URL of your data source</param>
        /// <returns></returns>
        public async Task<string> GetProtectedJsonResult(string methodUrl)
        {
            HttpClient client = new HttpClient();

            // Set the access token for the request 
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
            // Send a request and get the response 
            var response = await client.GetAsync(URL + methodUrl);
            // Read the data 
            var jsonData = await response.Content.ReadAsStringAsync();

            return jsonData;
        }


        // Method to invoke a put method protected

        /// <summary>
        /// Make a protected PUT request to your web api to alter a specific data model
        /// </summary>
        /// <typeparam name="T">Your data type that you want to get</typeparam>
        /// <param name="methodUrl">The Method name in the URL of your data source in the web api</param>
        /// <param name="model">The model or object that you want to alter</param>
        /// <param name="id">The id of the object that you want to alter</param>
        /// <returns></returns>
        public async Task<T> PutProtected<T>(string methodUrl, object model, int id)
        {
            HttpClient client = new HttpClient();

            // Set the access token for the request 
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
            // Serialize the object
            string jsonData = JsonConvert.SerializeObject(model);

            HttpContent content = new StringContent(jsonData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await client.PutAsync(URL + methodUrl + "/" + id, content);

            var responseString = await response.Content.ReadAsStringAsync();

            T obj = JsonConvert.DeserializeObject<T>(responseString);

            return obj;
        }

        /// <summary>
        /// Make a protected PUT request to your web api to alter a specific data model
        /// </summary>
        /// <typeparam name="T">Your data type that you want to get</typeparam>
        /// <param name="methodUrl">The Method name in the URL of your data source in the web api</param>
        /// <param name="model">The model or object that you want to alter</param>
        /// <returns></returns>
        public async Task<T> PutProtected<T>(string methodUrl, object model)
        {
            HttpClient client = new HttpClient();

            // Set the access token for the request 
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
            // Serialize the object
            string jsonData = JsonConvert.SerializeObject(model);

            HttpContent content = new StringContent(jsonData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await client.PutAsync(URL + methodUrl, content);

            var responseString = await response.Content.ReadAsStringAsync();

            T obj = JsonConvert.DeserializeObject<T>(responseString);

            return obj;
        }

        // Method to invoke the delete moethod protected 

        /// <summary>
        /// Make a protected DELETE request to your web api to delete a specific object
        /// </summary>
        /// <typeparam name="T">Your data type that you want to get</typeparam>
        /// <param name="methodUrl">The Method name in the URL of your data source in the web api</param>
        /// <param name="id">The id of the object that you want to delete</param>
        /// <returns></returns>
        public async Task<T> DeleteProtected<T>(string methodUrl, int id)
        {
            HttpClient client = new HttpClient();

            // Set the access token for the request 
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);

            var response = await client.DeleteAsync(URL + methodUrl + "/" + id);
            var jsonData = await response.Content.ReadAsStringAsync();

            T obj = JsonConvert.DeserializeObject<T>(jsonData);

            return obj;
        }


        /// <summary>
        /// Make a protected DELETE request to your web api to delete a specific object
        /// </summary>
        /// <typeparam name="T">Your data type that you want to get</typeparam>
        /// <param name="methodUrl">The Method name in the URL of your data source in the web api</param>
        /// <returns></returns>
        public async Task<T> DeleteProtected<T>(string methodUrl, object model)
        {
            HttpClient client = new HttpClient();

            // Set the access token for the request 
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);

            string jsonData = JsonConvert.SerializeObject(model);
            HttpContent content = new StringContent(jsonData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"); 
            var response = await client.DeleteAsync(URL + methodUrl);
            var responseString = await response.Content.ReadAsStringAsync();

            T obj = JsonConvert.DeserializeObject<T>(jsonData);

            return obj;
        }
        #endregion

    }


    internal class UserInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
