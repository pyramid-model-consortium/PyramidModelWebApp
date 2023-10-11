using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Pyramid.User_Controls
{
    public partial class MessagingSystem : System.Web.UI.UserControl
    {
        public List<Message> MessageQueue
        {
            get
            {
                List<Message> returnObject =  (List<Message>)HttpContext.Current.Session["MsgSysQueue"];
                
                if(returnObject == null)
                {
                    returnObject = new List<Message>();
                }

                return returnObject;
            }

            set
            {
                if(value == null)
                {
                    HttpContext.Current.Session.Remove("MsgSysQueue");
                }
                else
                {
                    HttpContext.Current.Session["MsgSysQueue"] = value;
                }
            }
        }

        public class Message
        {
            public string MessageType { get; set; }
            public string MessageTitle { get; set; }
            public string MessageContents { get; set; }
            public int MessageDuration { get; set; }

            public Message()
            {

            }

            public Message(string type, string title, string message, int duration)
            {
                MessageType = type;
                MessageTitle = title;
                MessageContents = message;
                MessageDuration = duration;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if(!Page.IsPostBack)
            {
                //Get the current queue
                List<Message> currentMessageQueue = MessageQueue;

                //Check if the queue has contents
                if(currentMessageQueue != null && currentMessageQueue.Count > 0)
                {
                    //Loop through the queue
                    foreach(Message messageToDisplay in currentMessageQueue)
                    {
                        //Show the message
                        ShowMessageToUser(messageToDisplay.MessageType, messageToDisplay.MessageTitle, messageToDisplay.MessageContents, messageToDisplay.MessageDuration);
                    }

                    //Delete the messages in the session
                    MessageQueue = null;
                }
            }
        }

        /// <summary>
        /// This method uses hidden fields to pass a message to the front-end
        /// to show to the user
        /// </summary>
        /// <param name="type">Can be any valid bootstrap alert type (e.g. success, danger)</param>
        /// <param name="title">The title of the message</param>
        /// <param name="message">The message contents</param>
        /// <param name="duration">The number of milliseconds the message should remain visible</param>
        public void ShowMessageToUser(string type, string title, string message, int duration)
        {
            if (!string.IsNullOrWhiteSpace(hfMessageType.Value) && hfMessage.Value.Contains(message) == false)
            {
                hfMessageType.Value += "*sep*" + type;
                hfMessageTitle.Value += "*sep*" + title;
                hfMessage.Value += "*sep*" + message;
                hfMessageDuration.Value += "*sep*" + duration.ToString();
            }
            else
            {
                hfMessageType.Value = type;
                hfMessageTitle.Value = title;
                hfMessage.Value = message;
                hfMessageDuration.Value = duration.ToString();
            }
        }

        /// <summary>
        /// This method uses hidden fields to pass a message to the front-end
        /// to show to the user
        /// </summary>
        /// <param name="messageType">Can be any valid bootstrap alert type (e.g. success, danger)</param>
        /// <param name="messageTitle">The title of the message</param>
        /// <param name="messageContents">The message contents</param>
        /// <param name="messageDuration">The number of milliseconds the message should remain visible</param>
        public void AddMessageToQueue(string messageType, string messageTitle, string messageContents, int messageDuration)
        {
            //Make sure the message is valid
            if(!string.IsNullOrWhiteSpace(messageType) && !string.IsNullOrWhiteSpace(messageTitle) && !string.IsNullOrWhiteSpace(messageContents) && messageDuration > 0)
            {
                //Get the current queue
                List<Message> currentMessageQueue = MessageQueue;

                //Don't allow duplication
                if (currentMessageQueue == null || currentMessageQueue.Where(m => m.MessageContents == messageContents).Count() == 0)
                {
                    //Add the message to the current queue
                    currentMessageQueue.Add(new Message()
                    {
                        MessageType = messageType,
                        MessageTitle = messageTitle,
                        MessageContents = messageContents,
                        MessageDuration = messageDuration
                    });

                    //Set the session queue
                    MessageQueue = currentMessageQueue;
                }
            }
        }
    }
}