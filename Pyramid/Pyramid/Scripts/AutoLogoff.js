/*
  This code warns the user of an impending session timeout.
  If the user responds to the prompt, the session will be renewed through a partial postback.
  If the user does not respond, they will be logged out moments before the session expires. 

  Requirements:

  1.On the page where the script is referenced, the following markup must be included:
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>  -- Only need script manager if there is none.
        <asp:UpdatePanel ID="dummyPanel" runat="server"></asp:UpdatePanel>
        <div id="logWarning" class="modal fade">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">Warning!</h4>
                    </div>
                    <div class="modal-body">
                        <span>For security purposes, your session will expire in <span id="showSeconds">120</span> seconds</span>
                        <br />
                        <br />
                        <span>Are you still there?</span>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" onclick="resetTimer()">Yes</button>
                    </div>
                </div>
            </div>
        </div>

  2. On the page where the script is referenced, in the document.ready function, start the timer:
     startAutoLogoff();

*/
var timer; //the timeout object for the warning modal
var logoutTimer; //The timeout object for the logout
var sessionTimeout; //the amount of time allowed between postbacks
var timeToTimeoutInterval; //The interval object for displaying the number of seconds until timeout

//called in document ready of master pages to begin process.
function startAutoLogoff() {
    try {
        //Clear the timeouts and intervals
        clearTimeout(timer);
        clearTimeout(logoutTimer);
        clearInterval(timeToTimeoutInterval);
    }
    catch (e) {
        //Do nothing
    }

    //page is loading, session has been reset. So set the cookie.
    setCookie();

    //Give users 18 minutes before displaying 2 minute Warning window and convert to milliseconds.
    sessionTimeout = 18 * 60000;

    startTimer(sessionTimeout);

    if (document.addEventListener) document.addEventListener("visibilitychange", visibilityChanged);
}

//sets cookie with current time.
function setCookie() {
    var stamp = Date.now();
    document.cookie = "time=" + stamp + "; path=/";
}

function startTimer(sessionTimeout) {
    timer = setTimeout(warning, sessionTimeout);
}

//resetTimer() occurs when the user clicks Yes in the logout warning dialog. 
//The partial postback resets the session and cancels the automatic logout timer set in the warning() function.
function resetTimer() {
    jQuery('#logWarning').modal('hide');
    startAutoLogoff();
    __doPostBack(jQuery('[id$="dummyPanel"]').attr('id'), '');
}

//returns the time the cookie was set, ie. the time the session was reset.
function getCookie() {
    var cname = "time=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) === ' ') c = c.substring(1, c.length);
        if (c.indexOf(cname) === 0) {
            return c.substring(cname.length, c.length);
        }
    }
    return null;
}

function visibilityChanged() {

    clearTimeout(timer);
    clearTimeout(logoutTimer);
    clearInterval(timeToTimeoutInterval);

    if (!document.hidden) {
        //The document is visible so figure out how much time is left in the session.
        var timeLapsed = Date.now() - getCookie("time");

        //if the time lapsed is greater than the session time, log the user out.
        if (timeLapsed > sessionTimeout + 120000) {
            logOutUser();
        }
        //else there's still time left so set the timer with the remaining session time.
        else
            startTimer(sessionTimeout - timeLapsed);
    }
}

//When the warning div pops up, automatic logout is set to 2min.
function warning() {
    var secondsDisplay = document.getElementById('showSeconds');
    var seconds = 120;

    jQuery('#logWarning').modal({ backdrop: 'static', keyboard: false });

    logoutTimer = setTimeout(logOutUser, seconds * 1000);

    //This interval displays seconds countdown in warning div.
    timeToTimeoutInterval = setInterval(function () {
        seconds--;
        secondsDisplay.innerHTML = seconds;
    }, 1000);
}

function logOutUser() {
    clearTimeout(timer);
    clearTimeout(logoutTimer);
    clearInterval(timeToTimeoutInterval);
    jQuery('#logWarning').modal('hide');
    //delete cookie by setting its expiration to a past date.
    document.cookie = "time=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
    window.location = "/SessionExpired.aspx";
}