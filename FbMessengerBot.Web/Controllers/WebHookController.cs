using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FbMessengerHook.Models;
using System.Text;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;


namespace FbMessengerBot.Controllers
{
    public class WebHookController : Controller
    {
        const string token = "Page Access Token";
        public string Index()
        {
            if (Request.QueryString["hub.verify_token"] == "Verify Token")
            {
                return Request.QueryString["hub.challenge"];
            }
            return "Error";
        }
        [HttpPost]
        public void Index(FbMessengerResponseModel.RootObject model)
        {
            foreach (var entry in model.entry)
            {
                foreach (var item in entry.messaging)
                {
                    if (item.message != null && item.message.text != null)
                    {
                        var message = item.message.text;
                        SendMessageText(item.sender.id, message);
                    }
                }
            }
        }

        private void SendMessageText(string senderId, string message)
        {
            string URI = "https://graph.facebook.com/v2.6/me/messages?access_token=" + token;

            FbMessengerRequestModel requestModel = new FbMessengerRequestModel();
            requestModel.recipient = new Recipient() { id = senderId };
            requestModel.message = new Message() { text = message };
            string myParameters = new JavaScriptSerializer().Serialize(requestModel);
            var http = (HttpWebRequest)WebRequest.Create(new Uri(URI));
            http.Accept = "application/json";
            http.ContentType = "application/json";
            http.Method = "POST";

            string parsedContent = myParameters;
            ASCIIEncoding encoding = new ASCIIEncoding();
            Byte[] bytes = encoding.GetBytes(parsedContent);

            Stream newStream = http.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            var response = http.GetResponse();

            var stream = response.GetResponseStream();
            var sr = new StreamReader(stream);
            var content = sr.ReadToEnd();
        }
    }
}
