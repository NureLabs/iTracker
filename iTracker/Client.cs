/*using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTracker;
using Newtonsoft.Json;

public struct Connection
{
    public User user;
    public string accessToken;
    public string refreshToken;
    public Connection(User user, string accessToken, string refreshToken)
    {
        this.accessToken = accessToken;
        this.refreshToken = refreshToken;
        this.user = user;
    }
}

public struct User
{
    public string id;
    public string email;
    public string[] rules;
    public string created;
    public void setEmail(string newEmail) => email = newEmail;
}

public class ClientVM
{
    public bool isConnect { get; private set; }
    public Connection Session { get; private set; }
    public string HOST;
    public string pcToken { get; private set; }
    HttpClient Client;

    public ClientVM(string host, string pcToken = null)
    {
        this.HOST = host;
        Client = new HttpClient();
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
        isConnect = false;
        if(pcToken != null) this.pcToken = pcToken;
    }

    public async Task Authorize(string email, string password)
    {
        if (!String.IsNullOrEmpty(Session.accessToken)) throw new DataException("Session is already exist");
        Client.DefaultRequestHeaders.Accept.Clear();
        var content = new FormUrlEncodedContent(new Dictionary<string, string>() {
          { "email", email},
          { "password", password }
        });
        var awaiter = await Client.PostAsync($"{this.HOST}/user/authorization", content);
        if (!awaiter.IsSuccessStatusCode) throw new ArgumentException("Bed credentials");
        var response = await awaiter.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<Connection>(response);
        Session = new Connection(data.user, data.accessToken, data.refreshToken);
        isConnect = true;
    }
    public async Task SendProcesses(string body, string pcToken = "abcdefghyz")
    {
        if (String.IsNullOrEmpty(Session.accessToken)) throw new DataException("Session forbidden");
        Client.DefaultRequestHeaders.Accept.Clear();
        if (Client.DefaultRequestHeaders.Authorization == null) Client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Session.accessToken);

        var content = new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "pcToken", $"{pcToken}" },
            { "processes", $"{body}" },
        });

        var awaiter = await Client.PostAsync($"{this.HOST}/process", content);
        if (!awaiter.IsSuccessStatusCode) throw new ArgumentException(await awaiter.Content.ReadAsStringAsync());
    }

    public async Task Refresh()
    {
        Client.DefaultRequestHeaders.Accept.Clear();
        Client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={this.Session.refreshToken}; Path=/; HttpOnly;");
        var awaiter = await Client.GetAsync($"{this.HOST}/user/token/refresh");
        if (!awaiter.IsSuccessStatusCode) throw new ArgumentException("Bed credentials");
        var response = await awaiter.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<Connection>(response);
        Session = new Connection(data.user, data.accessToken, data.refreshToken);
        isConnect = true;
    }

    public async Task Exit()
    {
        Client.DefaultRequestHeaders.Accept.Clear();
        var content = new FormUrlEncodedContent(new Dictionary<string, string>() { });
        var awaiter = await Client.PostAsync($"{this.HOST}/user/logout", content);
        isConnect = false;
    }

    public void SetPcToken(string pcToken)
    {
        this.pcToken = pcToken;
    }    
}
*/