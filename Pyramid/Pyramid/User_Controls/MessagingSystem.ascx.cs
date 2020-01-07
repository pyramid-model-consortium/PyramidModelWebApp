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
        protected void Page_Load(object sender, EventArgs e)
        {

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
            if (!String.IsNullOrWhiteSpace(hfMessageType.Value) && hfMessage.Value.Contains(message) == false)
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
    }
}