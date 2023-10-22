using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;

namespace NameCheapDNSUpdate
{
    class Program
    {


        //globals
        public static int intCheckTimerSEC = Convert.ToInt32(Environment.GetEnvironmentVariable("intCheckTimerSEC").Trim());  //environmental variable, interval of how long to wait to check again
        public static int intCheckTimerMS = intCheckTimerSEC * 1000;  //same as above just for Milliseconds for the timer

        public static string strPublicIP = "0.0.0.0";  //initial public IP, bogus to force a check at startup.

        public static bool firstRun = true;  //fist run of program



        //API details
        public static string strDomain = "";  //domain to process
        public static List<string> strHosts = new List<string>();  //list of hosts to process
        public static string strDynamicDNSPassword = "";  //api password
        

        //OLD from main api code
        //public static string strAPIUser = Environment.GetEnvironmentVariable("apiUser");    //api user name
        //public static string strAPIKey = Environment.GetEnvironmentVariable("apiKey");      //api user key/password
        //public static string strUserName = Environment.GetEnvironmentVariable("userName");  //api username
        //public static string strClientIP = Environment.GetEnvironmentVariable("clientIP");  //client IP for request.
        //public static string strTTL = Environment.GetEnvironmentVariable("TTL");            //"DNS TTL";




        static void Main(string[] args)
        {
            Console.WriteLine("Program Begin/FirstRun: " + System.DateTime.Now.ToString() +" UTC");

            //startup tests
            Console.WriteLine("Startup Tests Begin: " + System.DateTime.Now.ToString() + " UTC");
            bool startupReady = startupCheck();


            if (startupReady)
            {
                //startup 
                var tempCurrentIP = getPublicIP();


                //set a domain
                domain domainToRun = new domain();

                domainToRun.fullName = strDomain;
                domainToRun.Name = strDomain.Substring(0, strDomain.LastIndexOf("."));
                domainToRun.TLD = strDomain.Substring(strDomain.LastIndexOf(".") + 1, (strDomain.Length - strDomain.LastIndexOf(".") - 1));
                

                //check current IP list

                //planned endless loop for automation
                while (1 == 1)
                {

                    //get the current public IP address
                    Console.WriteLine("Program Run: " + System.DateTime.Now.ToString() + " UTC");
                    Console.WriteLine("Checking Public IP...");

                    //resolve current host name
                    strPublicIP = ResolveHostName(strHosts.FirstOrDefault(),domainToRun);

                    //get the current public IP
                    tempCurrentIP = getPublicIP();



                    //was there an error geting the public IP?
                    if (tempCurrentIP != "-99")
                    { 

                        //are they the same? then don't need to do anything Or is it the first run?
                        if ((strPublicIP != tempCurrentIP)|| firstRun)
                        {
                            //reset firstRun
                            if (!firstRun)
                            {
                                Console.WriteLine("IPs do not match, updating DNS");
                            }
                            else
                            {
                                Console.WriteLine("First Program run, completing program");
                                firstRun = false;
                                
                            }


                            //set the IP to the global
                            strPublicIP = tempCurrentIP;

                           
                            //if we have hosts
                            if (strHosts != null)
                            {
                               setHostsIP(strHosts, domainToRun, strPublicIP, strDynamicDNSPassword);
                            }

                            //update hosts DNS
                            Console.WriteLine("DNS Update Complete");

                            

                        }
                        else
                        {
                            Console.WriteLine("IPs match, nothing to do");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error getting public IP, doing nothing.");
                    }


                    //sleep for 60 seconds and repeat process
                    Console.WriteLine("Sleeping for " + intCheckTimerSEC + " seconds.");
                    Thread.Sleep(intCheckTimerMS);


                }
            }

        }

        public static bool startupCheck()
        {

            //check startup config.
            bool returnVal = true;


            //inializations
            //intCheckTimerSEC
            try
            {
                intCheckTimerSEC = Convert.ToInt32(Environment.GetEnvironmentVariable("intCheckTimerSEC").Trim());  //environmental variable, interval of how long to wait to check again
            }
            catch (Exception e)
            {
                returnVal = false;
                Console.WriteLine("Error Processing intCheckTimerSEC ENV.");

            }

            //strDomain
            try
            {
                strDomain = Environment.GetEnvironmentVariable("domain").Trim(); //split domains on semicolon
            }
            catch (Exception e)
            {
                returnVal = false;
                Console.WriteLine("Error Processing domain ENV.");
            }

            //strHosts
            try
            {
                strHosts = Environment.GetEnvironmentVariable("hosts").Trim().Split(";").ToList(); //split domains on semicolon
            }
            catch (Exception e)
            {
                returnVal = false;
                Console.WriteLine("Error Processing hosts ENV.");
            }

            //strDynamicDNSPassword
            try
            {
                strDynamicDNSPassword = Environment.GetEnvironmentVariable("dynamicDNSPassword").Trim();    //api password
            }
            catch (Exception e)
            {
                returnVal = false;
                Console.WriteLine("Error Processing strDynamicDNSPassword ENV.");
            }
        

           

            //no blanks
            if (strDomain == "" )
            {
                Console.WriteLine("strDomain environment variable cannnot be blank.");
                returnVal = false;
            }
            if (strHosts.Count == 0)
            {
                Console.WriteLine("strHosts environment variable cannnot be blank.");
                returnVal = false;
            }
            if (strDynamicDNSPassword == "" || strDynamicDNSPassword == null)
            {
                Console.WriteLine("dynamicDNSPassword environment variable cannnot be blank.");
                returnVal = false;
            }

            //from old main api code
            //if (strAPIUser == "" || strAPIUser == null)
            //{
            //    Console.WriteLine("apiUser environment variable cannnot be blank.");
            //    returnVal = false;
            //}
            //if (strAPIKey == "" || strAPIKey == null)
            //{
            //    Console.WriteLine("strAPIKey environment variable cannnot be blank.");
            //    returnVal = false;
            //}
            //if (strUserName == "" || strUserName == null)
            //{
            //    Console.WriteLine("userName environment variable cannnot be blank.");
            //    returnVal = false;
            //}
            //if (strClientIP == "" || strClientIP == null)
            //{
            //    Console.WriteLine("clientIp environment variable cannnot be blank.");
            //    returnVal = false;
            //}
            //if (strTTL == "" || strTTL == null)
            //{
            //    Console.WriteLine("TTL environment variable cannnot be blank.");
            //    returnVal = false;
            //}

            if (returnVal == false)
            {
                Console.WriteLine("System Startup Checks failed, please checked the errors and restart.");
            }
        

            return returnVal;

        }

        public static string getPublicIP()
        {

            //get current public IP


            //credentials
            //NetworkCredential credentials = GetNetworkCredentials();

            string retval = "";

            string URL = @"https://api.ipify.org/?format=json";

            Console.WriteLine("Checking Public IP From: " + URL);


            try
            {
                //get current public IP
                Task<string> task = Task.Run(async () => await SendRequestGet(URL));
                task.Wait();
                string content = task.Result;
                PublicIP response = JsonConvert.DeserializeObject<PublicIP>(content);

                retval = response.ip;

                Console.WriteLine("Public IP: " + response.ip);
            }
            catch (Exception e)
            {
                Console.WriteLine("Public IP get failure: " + e.Message);

                retval = "-99";
            }


            return retval;


        }

        public static string ResolveHostName(string host, domain domain)
        {
            IPHostEntry hostEntry;

            //full hostname
            var hostName = host + "." + domain.fullName;

            Console.WriteLine("Attempting to resolve " + hostName);

            try
            {
                //check DNS resolution
                hostEntry = Dns.GetHostEntry(hostName);


            }
            catch (Exception e)
            {
                Console.WriteLine("resolution of " + hostName + " failed.");

                return "-99";
            }
            

            if (hostEntry == null)
            {
                Console.WriteLine("resolution of " + hostName + " failed.");

                return "-99";
            }
            else
            {

                Console.WriteLine(hostName +" resolves to: " + hostEntry.AddressList[0].ToString());
                return hostEntry.AddressList[0].ToString();
            }

            ////you might get more than one ip for a hostname since 
            ////DNS supports more than one record

            //if (hostEntry.AddressList.Length > 0)
            //{
            //    var ip = hostEntry.AddressList[0];
            //    Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            //    s.Connect(ip, 80);
            //}

            //return hostEntry.AddressList[0].Address.ToString();
        }


        //OLD CODE FOR MAIN API
        //public static List<Host> getHostsList(string domain, string strSLD, string strTLD)
        //{

        //    Console.WriteLine("Getting Hosts List for " + domain);


        //    string URL = "https://api.namecheap.com/xml.response?";

        //    //query parameters
        //    URL += "ApiUser=" + strAPIUser;
        //    URL += "&APIKey=" + strAPIKey;
        //    URL += "&UserName=" + strUserName;
        //    URL += "&Command=namecheap.domains.dns.getHosts";
        //    URL += "&ClientIp=" + strClientIP;
        //    URL += "&SLD=" + strSLD;
        //    URL += "&TLD=" + strTLD;

        //    Console.WriteLine("Get Hosts APi Request: " + URL);

        //    try
        //    {
        //        //get host lists
        //        Task<string> task = Task.Run(async () => await SendRequestGet(URL));
        //        task.Wait();
        //        string content = task.Result;

        //        ApiResponse deserializedHostData = new ApiResponse();

        //        //API response to object
        //        XmlSerializer serializer = new XmlSerializer(typeof(ApiResponse));
        //        using (StringReader reader = new StringReader(content))
        //        {
        //            deserializedHostData = (ApiResponse)serializer.Deserialize(reader);


        //            Console.WriteLine("Host List Complete");

        //            return deserializedHostData.CommandResponse.DomainDNSGetHostsResult.Host;

        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Unable to retrieve hosts list.  Error: " + e.Message);

        //        return null;
        //    }


        //}

        public static void setHostsIP(List<string> hosts, domain domainToRun, string newIPAddress, string appPassword)
        {



            //query parameters
            //host=test33&domain=andse.space
            //URL += "host=" + host = test33 &

            //loop through each domain and set the Update URL
            //for (int i = 0; i < hosts.Count; i++)
            foreach(var host in hosts)
            {
                Console.WriteLine("Updating host: " + host + domainToRun.fullName);

                //update the IP for all hosts on the domain
                string URL = @"https://dynamicdns.park-your-domain.com/update?";

                URL += "host=" + host;
                URL += "&domain=" + domainToRun.fullName;
                URL += "&password=" + appPassword;
                URL += "&ip=" + newIPAddress;



                ////OLD from main api
                //URL += "&HostName" + (i + 1) + "=" + hosts[i].Name;
                //URL += "&RecordType" + (i + 1) + "=" + hosts[i].Type;
                //URL += "&Address" + (i + 1) + "=" + newIPAddress;
                //URL += "&TTL" + (i + 1) + "=" + strTTL;

                Console.WriteLine("Update DNS URL: " + URL);

                try
                {

                    //set host lists
                    Task<string> task = Task.Run(async () => await SendRequestPost(URL));
                    task.Wait();
                    string content = task.Result;


                    Interfaceresponse deserializedContentData = new Interfaceresponse();



                    //API response to object
                    XmlSerializer serializer = new XmlSerializer(typeof(Interfaceresponse));
                    using (StringReader reader = new StringReader(content))
                    {
                        deserializedContentData = (Interfaceresponse)serializer.Deserialize(reader);

                        if (deserializedContentData.ErrCount > 0)
                        {

                            Console.WriteLine("Update Errors:" + deserializedContentData.Errors);
                            Console.WriteLine("Full Return Content: " + content);
                        }
                        else
                        {
                            Console.WriteLine("Update Success");
                        }


                    }


                    //PostResponse.ApiPostResponse deserializedContentData = new PostResponse.ApiPostResponse();



                    ////API response to object
                    //XmlSerializer serializer = new XmlSerializer(typeof(PostResponse.ApiPostResponse));
                    //using (StringReader reader = new StringReader(content))
                    //{
                    //    deserializedContentData = (PostResponse.ApiPostResponse)serializer.Deserialize(reader);

                    //    Console.WriteLine("Update Success Rsponse:" + deserializedContentData.CommandResponse.DomainDNSSetHostsResult.IsSuccess);

                    //}


                    Console.WriteLine("Host List Complete");

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in Post Request: " + e.Message);
                }



            }

            

        }

        public static async Task<string> SendRequestGet(string url)
        {

            HttpClientHandler clientHandler = new HttpClientHandler();

            //ignores SSL errors, we're using a self signed cert in dev.
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            //clientHandler.Credentials = credentials;

            HttpClient _client = new HttpClient(clientHandler);


            string content = "";
            try
            {

                HttpResponseMessage response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\tERROR {0}", ex.Message + ". Inner exception: " + ex.InnerException.Message);
            }

            return content;
        }

        public static async Task<string> SendRequestPost(string URL)
        {
            //var values = new Dictionary<string, string>
            //  {
            //      { "thing1", "hello" },
            //      { "thing2", "world" }
            //  };

            //var content = new FormUrlEncodedContent(values);
            //var response = await client.PostAsync("http://www.example.com/recepticle.aspx", content);

            HttpClient client = new HttpClient();

            var response = await client.GetAsync(URL);   //.PostAsync(URL, null);

            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }
    }
    public class PublicIP
    {
        public string ip { get; set; }
        public string country { get; set; }
        public string cc { get; set; }
    }

    public class Domain
    {
        public string TLD { get; set; }
        public string SLD { get; set; }

        public string host { get; set; }

        public string dnsType { get; set; }

        public string dns { get; set; }
    }

    


}
