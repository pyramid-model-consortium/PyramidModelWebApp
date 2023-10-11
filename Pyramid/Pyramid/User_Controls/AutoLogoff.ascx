<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AutoLogoff.ascx.cs" Inherits="Pyramid.User_Controls.AutoLogoff" %>

<div id="divAutoLogoffControl">
	<asp:UpdatePanel ID="dummyPanel" runat="server"></asp:UpdatePanel>

	<div id="divLogoutWarningModal" class="modal fade">
		<div class="modal-dialog" role="document">
			<div class="modal-content">
				<div class="modal-header">
					<h4 class="modal-title">Warning!</h4>
				</div>
				<div class="modal-body">
					<span>For security purposes, your session will expire in <span id="spanSecondsUntilLogout">120</span> seconds</span>
					<br />
					<br />
					<span>Are you still there?</span>
				</div>
				<div class="modal-footer">
					<button id="btnConfirmThere" type="button" class="btn btn-primary"><i class="fas fa-check"></i>&nbsp;Yes</button>
				</div>
			</div>
		</div>
	</div>
</div>

<script type="text/javascript">
	var timer; //the timeout object for the warning modal
	var logoutTimer; //The timeout object for the logout
	var sessionTimeout; //the amount of time allowed between postbacks
	var timeToTimeoutInterval; //The interval object for displaying the number of seconds until timeout

	//This function starts the auto logoff process
	function startAutoLogoff() {
		//Set the logoff cookie
		setLogoffCookie();

		//Give users 18 minutes before displaying 2 minute Warning window and convert to milliseconds.
		sessionTimeout = (18) * 60000;

		//Start the logout timer
		startLogoffWarningTimer(sessionTimeout);

        //Add an event listener for loss of focus on this tab
        $(document).off('visibilitychange', tabVisibilityChanged);
        $(document).on('visibilitychange', tabVisibilityChanged);
	}

	//This function sets the cookie necessary for the auto logoff
	function setLogoffCookie() {
		//Get the current time
		var stamp = Date.now();

		//Add the time to the cookie (need to use cookie for visibility change event)
		Cookies.set('timestamp', stamp, { sameSite: 'strict' });
	}

	//This function starts the timer for display of the warning modal
	function startLogoffWarningTimer(sessionTimeout) {
		//Clear the timer if it exists already
		if (timer) {
			clearTimeout(timer);
			timer = null;
		}

		//Set the timer
		timer = setTimeout(showLogoffWarningModal, sessionTimeout);
	}

	//This function causes a partial postback to reset the session and restarts the auto logoff
	function resetAutoLogoff() {
		//Clear the timeouts and intervals
		clearLogoffTimeoutsAndIntervals();

		//Hide the modal
		$('#divAutoLogoffControl #divLogoutWarningModal').modal('hide');

		//Start the logoff again
		startAutoLogoff();

		//Do a partial postback
		__doPostBack($('#divAutoLogoffControl [ID$="dummyPanel"]').attr('id'), '');
	}

	//This function gets the time that the auto logoff began
	function getLogoffCookieValue() {
		//Get the value
		return Cookies.get('timestamp');
	}

	//This function clears the timeouts and intervals
	function clearLogoffTimeoutsAndIntervals() {
		//Clear the warning timer
		clearTimeout(timer);
		timer = null;

		//Clear the logout timer
		clearTimeout(logoutTimer);
		logoutTimer = null;

		//Clear the timeout interval
		clearInterval(timeToTimeoutInterval);
		timeToTimeoutInterval = null;
	}

	//This function is fired when the visiblity of this tab changed
	function tabVisibilityChanged() {
		//Clear the timeouts and intervals
		clearLogoffTimeoutsAndIntervals();

		//Check to see if the tab is visible
		if (!document.hidden) {
			//The tab is visible so figure out how much time is left in the session.
			var timeElapsed = Date.now() - getLogoffCookieValue();

			//Check to see if the amount of time elapsed since the timer started is over the session timeout
			//plus 120 seconds (warning time)
			if (timeElapsed > sessionTimeout + 120000) {
				//If the time elapsed is greater than the session time plus warning time, log the user out
				logOutUser();
			}
			else {
				//Else there's still time left so set the timer with the remaining session time
				startLogoffWarningTimer(sessionTimeout - timeElapsed);
			}
		}
	}

	//When the warning div pops up, automatic logout is set to 2min.
	function showLogoffWarningModal() {
		var secondsDisplay = $('#divAutoLogoffControl #spanSecondsUntilLogout');
		var seconds = 120;

		//Display the seconds until timeout
		secondsDisplay.html(seconds);

		//Show the warning modal
		$('#divAutoLogoffControl #divLogoutWarningModal').modal('show');

		//Clear the logout timer if it already exists
		if (logoutTimer) {
			clearTimeout(logoutTimer);
			logoutTimer = null;
		}

		//Set the new logout timer
		logoutTimer = setTimeout(logOutUser, seconds * 1000);

		//Clear the timeout interval if it already exists
		if (timeToTimeoutInterval) {
			clearInterval(timeToTimeoutInterval);
			timeToTimeoutInterval = null;
		}

		//Set the timeout interval that displays seconds countdown
		timeToTimeoutInterval = setInterval(function () {
			//Reduce the seconds variable by 1 each second
			seconds--;

			//Display the seconds until timeout
			secondsDisplay.html(seconds);
		}, 1000);
	}

	//This function logs out the user
	function logOutUser() {
		//Clear the timeouts and intervals
		clearLogoffTimeoutsAndIntervals();

		//Hide the warning modal
		$('#divAutoLogoffControl #divLogoutWarningModal').modal('hide');

		//Remove the cookie and send the user to the SessionExpired page
		Cookies.remove('timestamp');
		window.location = "/SessionExpired.aspx";
	}

	//This function checks for user activity after five minutes and,
	//if activity is detected, it resets the auto logoff timer
	function setupUserActivityCheck() {
		//Wait five minutes
		setTimeout(function () {
			//Add a focus event to all form controls
			$('select, input, textarea').one('focus', userActivityDetected);
		}, 300000);
	}

	//This function runs when user activity is detected
	function userActivityDetected() {
		//Log the activity
		console.log('User activity detected');

		//Reset the timer
		resetAutoLogoff();

		//Remove the focus event that calls this function
		$('select, input, textarea').off('focus', userActivityDetected);

		//Call the setupUserActivityCheck function again
		setupUserActivityCheck();
	}

	//Set up an event for document ready
    $(document).ready(function () {
        //Call the startAutoLogoff method on partial postback
        var requestManager = Sys.WebForms.PageRequestManager.getInstance();
        requestManager.add_endRequest(startAutoLogoff);

		//Start the auto logoff
		startAutoLogoff();

		//Add a click event to the modal button
		$('#divAutoLogoffControl #btnConfirmThere').on('click', function () {
			//Reset the logout timer
			resetAutoLogoff();
		});

		//Initialize the modal
		$('#divAutoLogoffControl #divLogoutWarningModal').modal({ backdrop: 'static', keyboard: false, show: false });

		//Set up the user activity check
		setupUserActivityCheck();
	});
</script>