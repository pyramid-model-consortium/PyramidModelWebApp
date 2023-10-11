/* Author: Benjamin Simmons
 * Create date: 10/8/2019
 * Intent: This file is designed to take the Reports.aspx JavaScript functions
 * out of the page itself in order to save room and make the flow more logical.
 */

//This function sets defaults for the criteria bases on the passed default options
function setCriteriaDefaults(reportCriteriaDefaults) {
    //Set the default program/hub/state
    var currentProgramFK = String($('[ID$="hfCurrentProgramFK"]').val());
    var currentHubFK = String($('[ID$="hfCurrentHubFK"]').val());
    var currentStateFK = String($('[ID$="hfCurrentStateFK"]').val());

    if ($('#divPrograms').hasClass('hidden') === false && lstBxProgram.GetVisible() === true) {
        //The program selection is available, set the default to the current program
        if (currentProgramFK !== null && currentProgramFK !== undefined && currentProgramFK !== '') {
            //If the current program FK exists, select it in the list box
            var programListItem = lstBxProgram.FindItemByValue(currentProgramFK);
            lstBxProgram.SetSelectedItem(programListItem);
        }
    }
    else if ($('#divHubs').hasClass('hidden') === false && lstBxHub.GetVisible() === true) {
        //The hub selection is available, set the default to the current hub
        if (currentHubFK !== null && currentHubFK !== undefined && currentHubFK !== '') {
            //If the current hub FK exists, select it in the list box
            var hubListItem = lstBxHub.FindItemByValue(currentHubFK);
            lstBxHub.SetSelectedItem(hubListItem);
        }
    }
    else if ($('#divStates').hasClass('hidden') === false && lstBxState.GetVisible() === true) {
        //The state selection is available, set the default to the current state
        if (currentStateFK !== null && currentStateFK !== undefined && currentStateFK !== '') {
            //If the current state FK exists, select it in the list box
            var stateListItem = lstBxState.FindItemByValue(currentStateFK);
            lstBxState.SetSelectedItem(stateListItem);
        }
    }

    //Get an array of all the criteria defaults
    var defaultsArray = reportCriteriaDefaults.toUpperCase().split(',');

    //To hold the defaults
    var startDate = moment();
    var endDate = moment();
    var pointInTimeDate = moment();
    var months = '';
    var years = '';
    var absoluteDate = '';

    //Check each criteria default
    $.each(defaultsArray, function (index, value) {
        //Get the default code
        var defaultCode = String(value);

        //Check to see what criteria default this is
        if (defaultCode.includes('RSD-M')) {
            //Relative start date (Months)
            //Get the months
            months = value.substring(5);

            //Add the months to the start date
            startDate = moment().add(months, 'M').startOf('month');

            //Set the date edit date
            deStartDate.SetDate(startDate.toDate());
        }
        else if (defaultCode.includes('RSD-Y')) {
            //Relative start date (Years)
            //Get the years
            years = value.substring(5);

            //Add the years to the start date
            startDate = moment().add(years, 'Y').startOf('year');

            //Set the date edit date
            deStartDate.SetDate(startDate.toDate());
        }
        else if (defaultCode.includes('ASD')) {
            //Absolute start date (MM/dd/yyyy format)
            //Get the absolute date
            absoluteDate = value.substring(3);

            //Set the start date
            startDate = moment(absoluteDate);

            //Set the date edit date
            deStartDate.SetDate(startDate.toDate());
        }
        else if (defaultCode.includes('RED-M')) {
            //Relative end date (Months)
            //Get the months
            months = value.substring(5);

            //Add the months to the end date
            endDate = moment().add(months, 'M').endOf('month');

            //Set the date edit date
            deEndDate.SetDate(endDate.toDate());

        }
        else if (defaultCode.includes('RED-Y')) {
            //Relative end date (Years)
            //Get the years
            years = value.substring(5);

            //Add the years to the end date
            endDate = moment().add(years, 'Y').endOf('year');

            //Set the date edit date
            deEndDate.SetDate(endDate.toDate());

        }
        else if (defaultCode.includes('AED')) {
            //Absolute end date (MM/dd/yyyy format)
            //Get the absolute date
            absoluteDate = value.substring(3);

            //Set the end date
            endDate = moment(absoluteDate);

            //Set the date edit date
            deEndDate.SetDate(endDate.toDate());
        }
        else if (defaultCode.includes('RPIT-M')) {
            //Relative point in time date (Months)
            //Get the months
            months = value.substring(6);

            //Add the months to the point in time date
            pointInTimeDate = moment().add(months, 'M').startOf('month');

            //Set the date edit date
            dePointInTime.SetDate(pointInTimeDate.toDate());
        }
        else if (defaultCode.includes('RPIT-Y')) {
            //Relative point in time date (Years)
            //Get the years
            years = value.substring(6);

            //Add the years to the point in time date
            pointInTimeDate = moment().add(years, 'Y').startOf('year');

            //Set the date edit date
            dePointInTime.SetDate(pointInTimeDate.toDate());
        }
        else if (defaultCode.includes('APIT')) {
            //Absolute point in time date (MM/dd/yyyy format)
            //Get the absolute date
            absoluteDate = value.substring(4);

            //Set the point in time date
            pointInTimeDate = moment(absoluteDate);

            //Set the date edit date
            dePointInTime.SetDate(pointInTimeDate.toDate());
        }
        else if (defaultCode.includes('RY')) {
            //Relative year
            //Get the years
            years = value.substring(2);

            //Add the years to the point in time date
            pointInTimeDate = moment().add(years, 'Y').startOf('year');

            //Set the date edit date
            ddYear.SetValue(pointInTimeDate.year());
        }
        else if (defaultCode === 'SD-NOW') {
            //Default the start date to now
            var currentDate = new Date();

            //Set the date edit date
            deStartDate.SetDate(currentDate);
        }
        else if (defaultCode === 'ED-NOW') {
            //Default the end date to now
            currentDate = new Date();

            //Set the date edit date
            deEndDate.SetDate(currentDate);
        }
        else if (defaultCode === 'PIT-NOW') {
            //Default the point in time date to now
            currentDate = new Date();

            //Set the date edit date
            dePointInTime.SetDate(currentDate);
        }
    });
}

//This function shows/hides the criteria based on the passed criteria options
function setCriteriaVisibility(reportCriteriaOptions) {
    //Program list
    if (reportCriteriaOptions.includes('PRG,')) {
        //Show the program list div
        $('#divPrograms').removeClass('hidden');

        //Make sure the list boxes shows correctly
        lstBxProgram.AdjustControl();
    }
    else {
        hideAndClearPRG();
    }

    //Hub list
    if (reportCriteriaOptions.includes('HUB,')) {
        //Show the hub list div
        $('#divHubs').removeClass('hidden');

        //Make sure the list box shows correctly
        lstBxHub.AdjustControl();
    }
    else {
        hideAndClearHUB();
    }

    //Cohort list
    if (reportCriteriaOptions.includes('COH,')) {
        //Show the cohort list div
        $('#divCohorts').removeClass('hidden');

        //Make sure the list box shows correctly
        lstBxCohort.AdjustControl();
    }
    else {
        hideAndClearCOH();
    }

    //State list
    if (reportCriteriaOptions.includes('ST,')) {
        //Show the state list div
        $('#divStates').removeClass('hidden');

        //Make sure the list box shows correctly
        lstBxState.AdjustControl();
    }
    else {
        hideAndClearST();
    }

    //Start and end dates
    if (reportCriteriaOptions.includes('SED,')) {
        $('#divStartEndDates').removeClass('hidden');
    }
    else {
        hideAndClearSED();
    }

    //Point in time date
    if (reportCriteriaOptions.includes('PIT,')) {
        $('#divPointInTimeDate').removeClass('hidden');
    }
    else {
        hideAndClearPIT();
    }

    //Year
    if (reportCriteriaOptions.includes('YRS,')) {
        $('#divYears').removeClass('hidden');
    }
    else {
        hideAndClearYRS();
    }

    //BIR profile group dropdown
    if (reportCriteriaOptions.includes('BPG,')) {
        $('#divBIRProfileGroup').removeClass('hidden');
    }
    else {
        hideAndClearBPG();
    }

    //BIR profile item dropdown
    if (reportCriteriaOptions.includes('BPI,')) {
        $('#divBIRProfileItem').removeClass('hidden');
    }
    else {
        hideAndClearBPI();
    }

    //Report Focus dropdown
    if (reportCriteriaOptions.includes('RF,')) {
        $('#divReportFocus').removeClass('hidden');
    }
    else {
        hideAndClearRF();
    }

    //Classroom list
    if (reportCriteriaOptions.includes('CR,')) {
        //Show the classroom list div
        $('#divClassrooms').removeClass('hidden');

        //Make sure the list box shows correctly
        lstBxClassroom.AdjustControl();
    }
    else {
        hideAndClearCR();
    }

    //Child list
    if (reportCriteriaOptions.includes('CHI,')) {
        //Show the child list div
        $('#divChildren').removeClass('hidden');

        //Make sure the list box shows correctly
        lstBxChild.AdjustControl();
    }
    else {
        hideAndClearCHI();
    }

    //Child demographic lists
    if (reportCriteriaOptions.includes('CD,')) {
        //Show the div
        $('#divChildDemographics').removeClass('hidden');

        //Make sure the list boxes shows correctly
        lstBxRace.AdjustControl();
        lstBxEthnicity.AdjustControl();
        lstBxGender.AdjustControl();
    }
    else {
        hideAndClearCD();
    }

    //Employee list
    if (reportCriteriaOptions.includes('EMP,')) {
        //Show the employee list div
        $('#divEmployees').removeClass('hidden');

        //Make sure the list box shows correctly
        lstBxEmployee.AdjustControl();
    }
    else {
        hideAndClearEMP();
    }

    //Employee role dropdown
    if (reportCriteriaOptions.includes('EMR,')) {
        $('#divEmployeeRole').removeClass('hidden');
    }
    else {
        hideAndClearEMR();
    }

    //Teacher list
    if (reportCriteriaOptions.includes('TCH,')) {
        //Show the teacher list div
        $('#divTeachers').removeClass('hidden');

        //Make sure the list box shows correctly
        lstBxTeacher.AdjustControl();
    }
    else {
        hideAndClearTCH();
    }

    //Coach list
    if (reportCriteriaOptions.includes('CCH,')) {
        //Show the coach list div
        $('#divCoaches').removeClass('hidden');

        //Make sure the list box shows correctly
        lstBxCoach.AdjustControl();
    }
    else {
        hideAndClearCCH();
    }

    //Problem Behavior list
    if (reportCriteriaOptions.includes('PB,')) {
        //Show the Problem Behavior list div
        $('#divProblemBehaviors').removeClass('hidden');

        //Make sure the list box shows correctly
        lstBxProblemBehavior.AdjustControl();
    }
    else {
        hideAndClearPB();
    }

    //Activity list
    if (reportCriteriaOptions.includes('ACT,')) {
        //Show the Activity list div
        $('#divActivities').removeClass('hidden');

        //Make sure the list box shows correctly
        lstBxActivity.AdjustControl();
    }
    else {
        hideAndClearACT();
    }

    //Others Involved list
    if (reportCriteriaOptions.includes('OI,')) {
        //Show the Others Involved list div
        $('#divOthersInvolved').removeClass('hidden');

        //Make sure the list box shows correctly
        lstBxOthersInvolved.AdjustControl();
    }
    else {
        hideAndClearOI();
    }

    //Possible Motivation list
    if (reportCriteriaOptions.includes('PM,')) {
        //Show the Possible Motivation list div
        $('#divPossibleMotivations').removeClass('hidden');

        //Make sure the list box shows correctly
        lstBxPossibleMotivation.AdjustControl();
    }
    else {
        hideAndClearPM();
    }

    //Strategy Response list
    if (reportCriteriaOptions.includes('SR,')) {
        //Show the Strategy Response list div
        $('#divStrategyResponses').removeClass('hidden');

        //Make sure the list box shows correctly
        lstBxStrategyResponse.AdjustControl();
    }
    else {
        hideAndClearSR();
    }

    //Admin Follow-up list
    if (reportCriteriaOptions.includes('AFU,')) {
        //Show the Admin Follow-up list div
        $('#divAdminFollowUps').removeClass('hidden');

        //Make sure the list box shows correctly
        lstBxAdminFollowUp.AdjustControl();
    }
    else {
        hideAndClearAFU();
    }
}

//This function hides and clears all criteria
function hideAndClearCriteria() {
    //Hide and clear criteria options
    hideAndClearPRG();
    hideAndClearHUB();
    hideAndClearCOH();
    hideAndClearST();
    hideAndClearSED();
    hideAndClearPIT();
    hideAndClearYRS();
    hideAndClearBPG();
    hideAndClearBPI();
    hideAndClearRF();
    hideAndClearCR();
    hideAndClearCHI();
    hideAndClearCD();
    hideAndClearEMP();
    hideAndClearEMR();
    hideAndClearTCH();
    hideAndClearCCH();
    hideAndClearPB();
    hideAndClearACT();
    hideAndClearOI();
    hideAndClearPM();
    hideAndClearSR();
    hideAndClearAFU();
}

//This function hides and clears the program list
function hideAndClearPRG() {
    //Hide the program list
    $('#divPrograms').addClass('hidden');

    //Clear the program list
    lstBxProgram.UnselectAll();
}

//This function hides and clears the hub list
function hideAndClearHUB() {
    //Hide the hub list
    $('#divHubs').addClass('hidden');

    //Clear the hub list
    lstBxHub.UnselectAll();
}

//This function hides and clears the cohort list
function hideAndClearCOH() {
    //Hide the cohort list
    $('#divCohorts').addClass('hidden');

    //Clear the cohort list
    lstBxCohort.UnselectAll();
}

//This function hides and clears the state list
function hideAndClearST() {
    //Hide the state list
    $('#divStates').addClass('hidden');

    //Clear the state list
    lstBxState.UnselectAll();
}

//This function hides and clears the start and end dates
function hideAndClearSED() {
    //Hide the start and end date div
    $('#divStartEndDates').addClass('hidden');

    //Clear the start and end dates
    deStartDate.SetValue(null);
    deEndDate.SetValue(null);
}

//This function hides and clears the point in time date
function hideAndClearPIT() {
    //Hide the point in time div
    $('#divPointInTimeDate').addClass('hidden');

    //Clear the date selector
    dePointInTime.SetValue(null);
}

//This function hides and clears the years combo box
function hideAndClearYRS() {
    //Hide the year div
    $('#divYears').addClass('hidden');

    //Clear the combo box
    ddYear.SetSelectedIndex(-1);
}

//This function hides and clears the BIR profile group combo box
function hideAndClearBPG() {
    //Hide the BIR profile group div
    $('#divBIRProfileGroup').addClass('hidden');

    //Clear the combo box
    ddBIRProfileGroup.SetSelectedIndex(-1);
}

//This function hides and clears the BIR profile item combo box
function hideAndClearBPI() {
    //Hide the BIR profile item div
    $('#divBIRProfileItem').addClass('hidden');

    //Clear the combo box
    ddBIRProfileItem.SetSelectedIndex(-1);
}

//This function hides and clears the report focus combo box
function hideAndClearRF() {
    //Hide the report focus div
    $('#divReportFocus').addClass('hidden');

    //Clear the combo box
    ddReportFocus.SetSelectedIndex(-1);

    //Hide the warning
    $('#divReportFocusWarning').addClass('hidden').html('');
}

//This function hides and clears the classroom list
function hideAndClearCR() {
    //Hide the classroom list
    $('#divClassrooms').addClass('hidden');

    //Clear the classroom list
    lstBxClassroom.UnselectAll();
}

//This function hides and clears the child list
function hideAndClearCHI() {
    //Hide the child list
    $('#divChildren').addClass('hidden');

    //Clear the child list
    lstBxChild.UnselectAll();
}

//This function hides and clears the child demographic lists
function hideAndClearCD() {
    //Hide the lists
    $('#divChildDemographics').addClass('hidden');

    //Clear the lists
    lstBxRace.UnselectAll();
    lstBxEthnicity.UnselectAll();
    lstBxGender.UnselectAll();
    ddIEP.SetSelectedIndex(-1);
    ddDLL.SetSelectedIndex(-1);
}

//This function hides and clears the employee list
function hideAndClearEMP() {
    //Hide the employee list
    $('#divEmployees').addClass('hidden');

    //Clear the employee list
    lstBxEmployee.UnselectAll();
}

//This function hides and clears the BIR profile item combo box
function hideAndClearEMR() {
    //Hide the BIR profile item div
    $('#divEmployeeRole').addClass('hidden');

    //Clear the combo box by setting to the first option
    ddEmployeeRole.SetSelectedIndex(0);
}

//This function hides and clears the teacher list
function hideAndClearTCH() {
    //Hide the teacher list
    $('#divTeachers').addClass('hidden');

    //Clear the teacher list
    lstBxTeacher.UnselectAll();
}

//This function hides and clears the coach list
function hideAndClearCCH() {
    //Hide the coach list
    $('#divCoaches').addClass('hidden');

    //Clear the coach list
    lstBxCoach.UnselectAll();
}

//This function hides and clears the Problem Behavior list
function hideAndClearPB() {
    //Hide the Problem Behavior list
    $('#divProblemBehaviors').addClass('hidden');

    //Clear the Problem Behavior list
    lstBxProblemBehavior.UnselectAll();
}

//This function hides and clears the Activity list
function hideAndClearACT() {
    //Hide the Activity list
    $('#divActivities').addClass('hidden');

    //Clear the Activity list
    lstBxActivity.UnselectAll();
}

//This function hides and clears the Others Involved list
function hideAndClearOI() {
    //Hide the Others Involved list
    $('#divOthersInvolved').addClass('hidden');

    //Clear the Others Involved list
    lstBxOthersInvolved.UnselectAll();
}

//This function hides and clears the Possible Motivation list
function hideAndClearPM() {
    //Hide the Possible Motivation list
    $('#divPossibleMotivations').addClass('hidden');

    //Clear the Possible Motivation list
    lstBxPossibleMotivation.UnselectAll();
}

//This function hides and clears the Strategy Response list
function hideAndClearSR() {
    //Hide the Strategy Response list
    $('#divStrategyResponses').addClass('hidden');

    //Clear the Strategy Response list
    lstBxStrategyResponse.UnselectAll();
}

//This function hides and clears the Admin Follow-up list
function hideAndClearAFU() {
    //Hide the Admin Follow-up list
    $('#divAdminFollowUps').addClass('hidden');

    //Clear the Admin Follow-up list
    lstBxAdminFollowUp.UnselectAll();
}

//This function executes when the user clicks the run report button
function btnRunReportClick(s, e) {
    //Get the report to run class
    var reportClass = $('[ID$="hfReportToRunClass"]').val();

    //Make sure a report was selected
    if (reportClass !== null && reportClass !== undefined && reportClass.length > 1) {
        //Make sure validation passes
        if (ASPxClientEdit.ValidateGroup('vgCriteria')) {
            //Animate the report list
            $('#divReportGridview').removeClass(enterAnimation).addClass(exitAnimation);

            //Wait 500 ms to show the viewer and remove the list
            setTimeout(function () {
                $('#divReportGridview').hide();
                $('#divReportViewer').show().removeClass(exitAnimation).addClass(enterAnimation);
            }, 500);

            //Show the return to list button
            $('#btnReturnToList').show().removeClass(exitAnimation).addClass(enterAnimation);

            //Get the run report button and loading button
            var button = $('[ID$="btnRunReport"]');
            var loadingButton = $('[ID$="btnReportLoading"]');

            //Hide the run report button
            button.addClass('hidden');

            //Show the loading button
            loadingButton.removeClass('hidden');
        }
        else {
            //Prevent postback
            e.processOnServer = false;

            //Call the client validation failed method
            clientValidationFailed();
        }
    }
    else {
        //Prevent postback
        e.processOnServer = false;

        //Tell the user if no report was selected
        showNotification("warning", "No Report Selected", "No report was selected.", 5000);
    }
}

//This function executes when the user clicks the export report button
function btnExportReportClick(s, e) {
    //Get the report to run class
    var reportClass = $('[ID$="hfReportToRunClass"]').val();

    //Make sure a report was selected
    if (reportClass !== null && reportClass !== undefined && reportClass.length > 1) {
        //Make sure validation passes
        if (ASPxClientEdit.ValidateGroup('vgCriteria')) {
            //Get the export report button and loading button
            var button = $('[ID$="btnExportReport"]');
            var loadingButton = $('[ID$="btnReportLoading"]');

            //Hide the export report button
            button.addClass('hidden');

            //Show the loading button
            loadingButton.removeClass('hidden');

            //Since the download won't fire the document.ready, re-show the buttons and show a notification after a period of time
            setTimeout(function () {
                //Show the export report button
                button.removeClass('hidden');

                //Hide the loading button
                loadingButton.addClass('hidden');

                //Show a notification
                showNotification("success", "Report Exporting", "The report is exporting and should be downloaded soon.  Please remain on this page until the download completes and you see the file in your computer's download folder.", 5000);
            }, 1000);
        }
        else {
            //Prevent postback
            e.processOnServer = false;

            //Call the client validation failed method
            clientValidationFailed();
        }
    }
    else {
        //Prevent postback
        e.processOnServer = false;

        //Tell the user if no report was selected
        showNotification("warning", "No Report Selected", "No report was selected.", 5000);
    }
}

//Validate the start date field
function validateStartDate(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the start and end date criteria exists
    if (criteriaOptions.includes('SED,')) {
        //Get the start date and end date
        var startDate = deStartDate.GetDate();
        var endDate = deEndDate.GetDate();

        //Perform the validation
        if (startDate === null && !optionalCriteriaOptions.includes('SED,')) {
            e.isValid = false;
            e.errorText = "Start Date is required!";
        }
        else if (startDate === null && endDate !== null) {
            e.isValid = false;
            e.errorText = "Start Date is required if the End Date is selected!";
        }
        else if (startDate !== null && endDate !== null && startDate >= endDate) {
            e.isValid = false;
            e.errorText = "Start Date must be before the End Date!";
        }
        else if (startDate !== null && startDate > (moment().endOf('day')).toDate()) {
            e.isValid = false;
            e.errorText = "Start Date cannot be in the future!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the end date field
function validateEndDate(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the start and end date criteria exists
    if (criteriaOptions.includes('SED,')) {
        //Get the start date and end date
        var startDate = deStartDate.GetDate();
        var endDate = deEndDate.GetDate();

        //Perform the validation
        if (endDate === null && !optionalCriteriaOptions.includes('SED,')) {
            e.isValid = false;
            e.errorText = "End Date is required!";
        }
        else if (endDate === null && startDate !== null) {
            e.isValid = false;
            e.errorText = "End Date is required if the Start Date is selected!";
        }
        else if (endDate !== null && startDate !== null && startDate >= endDate) {
            e.isValid = false;
            e.errorText = "End Date must be after the Start Date!";
        }
        else if (endDate > (moment().endOf('day')).toDate()) {
            e.isValid = false;
            e.errorText = "End Date cannot be in the future!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the point in time date field
function validatePointInTime(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the start and end date criteria exists
    if (criteriaOptions.includes('PIT,')) {
        //Get the point in time
        var pointInTime = dePointInTime.GetDate();

        //Perform the validation
        if (pointInTime === null && !optionalCriteriaOptions.includes('PIT,')) {
            e.isValid = false;
            e.errorText = "Point In Time is required!";
        }
        else if (pointInTime !== null && pointInTime > (moment().endOf('day')).toDate()) {
            e.isValid = false;
            e.errorText = "Point In Time cannot be in the future!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the year combo box
function validateYear(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the year criteria exists
    if (criteriaOptions.includes('YRS,')) {
        //Get the year
        var year = ddYear.GetSelectedIndex();

        //Perform the validation
        if (year === -1 && !optionalCriteriaOptions.includes('YRS,')) {
            e.isValid = false;
            e.errorText = "A year must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the program, hub, cohort, and state list boxes
function validatePHCSList(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //To hold the necessary items
    var numAvailableItems = 0;
    var unselectedItems = [];

    //Only validate if the program criteria exists
    if (criteriaOptions.includes('PRG,') && lstBxProgram.GetVisible() === true) {
        //Increment the number of available items
        numAvailableItems++;

        //Get the selected programs
        var selectedPrograms = lstBxProgram.GetSelectedIndices();

        //If there are none selected, add to the list of invalid items
        if (selectedPrograms.length < 1) {
            unselectedItems.push("program");
        }
    }

    if (criteriaOptions.includes('HUB,') && lstBxHub.GetVisible() === true) {
        //Increment the number of available items
        numAvailableItems++;

        //Get the selected hubs
        var selectedHubs = lstBxHub.GetSelectedIndices();

        //If there are none selected, add to the list of invalid items
        if (selectedHubs.length < 1) {
            unselectedItems.push("hub");
        }
    }

    if (criteriaOptions.includes('COH,') && lstBxCohort.GetVisible() === true) {
        //Increment the number of available items
        numAvailableItems++;

        //Get the selected cohorts
        var selectedCohorts = lstBxCohort.GetSelectedIndices();

        //If there are none selected, add to the list of invalid items
        if (selectedCohorts.length < 1) {
            unselectedItems.push("cohort");
        }
    }

    if (criteriaOptions.includes('ST,') && lstBxState.GetVisible() === true) {
        //Increment the number of available items
        numAvailableItems++;

        //Get the selected states
        var selectedStates = lstBxState.GetSelectedIndices();

        //If there are none selected, add to the list of invalid items
        if (selectedStates.length < 1) {
            unselectedItems.push("state");
        }
    }

    //If there are available items, and they are all unselected, this is invalid
    if (numAvailableItems > 0 && unselectedItems.length === numAvailableItems) {
        //Set the validator to invalid
        e.isValid = false;

        //To hold the error text
        var errorText = "At least one ";

        //Loop through the non-selected items
        for (let i = 0; i < unselectedItems.length; i++) {

            if (i === (unselectedItems.length - 1)) {
                errorText += unselectedItems[i] + " must be selected!";
            }
            else {
                errorText += unselectedItems[i] + " or ";
            }
        }

        //Set the error text
        e.errorText = errorText;
    }
    else {
        e.isValid = true;
    }
}

//Validate the BIR profile group combo box
function validateBIRProfileGroup(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the BIR profile group criteria exists
    if (criteriaOptions.includes('BPG,')) {
        //Get the group
        var group = ddBIRProfileGroup.GetSelectedIndex();

        //Perform the validation
        if (group === -1 && !optionalCriteriaOptions.includes('BPG,')) {
            e.isValid = false;
            e.errorText = "A BIR Profile Group must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the BIR Profile item combo box
function validateBIRProfileItem(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the BIR Profile item criteria exists
    if (criteriaOptions.includes('BPI,')) {
        //Get the item
        var item = ddBIRProfileItem.GetSelectedIndex();

        //Perform the validation
        if (item === -1 && !optionalCriteriaOptions.includes('BPI,')) {
            e.isValid = false;
            e.errorText = "A BIR Profile Item must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the report focus combo box
function validateReportFocus(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the report focus criteria exists
    if (criteriaOptions.includes('RF,')) {
        //Get the item
        var item = ddReportFocus.GetSelectedIndex();

        //Perform the validation
        if (item === -1 && !optionalCriteriaOptions.includes('RF,')) {
            e.isValid = false;
            e.errorText = "A Report Focus must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the classroom list box
function validateClassroomList(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the classroom criteria exists
    if (criteriaOptions.includes('CR,')) {
        //Get the selected classrooms
        var selectedClassrooms = lstBxClassroom.GetSelectedIndices();

        //Perform the validation
        if (selectedClassrooms.length < 1 && !optionalCriteriaOptions.includes('CR,')) {
            e.isValid = false;
            e.errorText = "At least one classroom must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the child list box
function validateChildList(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the child criteria exists
    if (criteriaOptions.includes('CHI,')) {
        //Get the selected children
        var selectedChildren = lstBxChild.GetSelectedIndices();

        //Perform the validation
        if (selectedChildren.length < 1 && !optionalCriteriaOptions.includes('CHI,')) {
            e.isValid = false;
            e.errorText = "At least one child must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the child demographics lists
function validateCDLists(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the program criteria exists
    if (criteriaOptions.includes('CD,')) {
        //Get the selected demographic information
        var selectedRaces = lstBxRace.GetSelectedIndices();
        var selectedEthnicities = lstBxEthnicity.GetSelectedIndices();
        var selectedGenders = lstBxGender.GetSelectedIndices();
        var selectedIEP = ddIEP.GetSelectedIndex();
        var selectedDLL = ddDLL.GetSelectedIndex();

        //Perform the validation
        if (selectedRaces.length < 1 && selectedEthnicities.length < 1 && selectedGenders.length < 1
            && selectedIEP === -1 && selectedDLL === -1
            && !optionalCriteriaOptions.includes('CD,')) {
            e.isValid = false;
            e.errorText = "At least one child demographic (Race, Ethnicity, Gender, IEP, or DLL) must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the employee list box
function validateEmployeeList(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the employee criteria exists
    if (criteriaOptions.includes('EMP,')) {
        //Get the selected employees and employee role
        var selectedEmployees = lstBxEmployee.GetSelectedIndices();

        //Perform the validation
        if (selectedEmployees.length < 1 && !optionalCriteriaOptions.includes('EMP,')) {
            e.isValid = false;
            e.errorText = "At least one professional must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the BIR profile group combo box
function validateEmployeeRole(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the employee role criteria exists
    if (criteriaOptions.includes('EMR,')) {
        //Get the role
        var employeeRole = ddEmployeeRole.GetSelectedIndex();

        //Perform the validation
        if (employeeRole === -1 && !optionalCriteriaOptions.includes('EMR,')) {
            e.isValid = false;
            e.errorText = "A professional role must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the teacher list box
function validateTeacherList(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the teacher criteria exists
    if (criteriaOptions.includes('TCH,')) {
        //Get the selected teachers
        var selectedTeachers = lstBxTeacher.GetSelectedIndices();

        //Perform the validation
        if (selectedTeachers.length < 1 && !optionalCriteriaOptions.includes('TCH,')) {
            e.isValid = false;
            e.errorText = "At least one teacher must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the coach list box
function validateCoachList(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the coach criteria exists
    if (criteriaOptions.includes('CCH,')) {
        //Get the selected coaches
        var selectedCoaches = lstBxCoach.GetSelectedIndices();

        //Perform the validation
        if (selectedCoaches.length < 1 && !optionalCriteriaOptions.includes('CCH,')) {
            e.isValid = false;
            e.errorText = "At least one coach must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the Problem Behavior list box
function validateProblemBehaviorList(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the Problem Behavior criteria exists
    if (criteriaOptions.includes('PB,')) {
        //Get the selected Problem Behaviors
        var selectedProblemBehaviors = lstBxProblemBehavior.GetSelectedIndices();

        //Perform the validation
        if (selectedProblemBehaviors.length < 1 && !optionalCriteriaOptions.includes('PB,')) {
            e.isValid = false;
            e.errorText = "At least one Problem Behavior must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the Activity list box
function validateActivityList(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the Activity criteria exists
    if (criteriaOptions.includes('ACT,')) {
        //Get the selected Activities
        var selectedActivities = lstBxActivity.GetSelectedIndices();

        //Perform the validation
        if (selectedActivities.length < 1 && !optionalCriteriaOptions.includes('ACT,')) {
            e.isValid = false;
            e.errorText = "At least one Activity must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the Others Involved list box
function validateOthersInvolvedList(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the Others Involved criteria exists
    if (criteriaOptions.includes('OI,')) {
        //Get the selected Others Involved
        var selectedOthersInvolved = lstBxOthersInvolved.GetSelectedIndices();

        //Perform the validation
        if (selectedOthersInvolved.length < 1 && !optionalCriteriaOptions.includes('OI,')) {
            e.isValid = false;
            e.errorText = "At least one Others Involved must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the Possible Motivation list box
function validatePossibleMotivationList(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the Possible Motivation criteria exists
    if (criteriaOptions.includes('PM,')) {
        //Get the selected Possible Motivations
        var selectedPossibleMotivations = lstBxPossibleMotivation.GetSelectedIndices();

        //Perform the validation
        if (selectedPossibleMotivations.length < 1 && !optionalCriteriaOptions.includes('PM,')) {
            e.isValid = false;
            e.errorText = "At least one Possible Motivation must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the Strategy Response list box
function validateStrategyResponseList(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the Strategy Response criteria exists
    if (criteriaOptions.includes('SR,')) {
        //Get the selected Strategy Responses
        var selectedStrategyResponses = lstBxStrategyResponse.GetSelectedIndices();

        //Perform the validation
        if (selectedStrategyResponses.length < 1 && !optionalCriteriaOptions.includes('SR,')) {
            e.isValid = false;
            e.errorText = "At least one Strategy Response must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}

//Validate the Admin Follow-up list box
function validateAdminFollowUpList(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the Admin Follow-up criteria exists
    if (criteriaOptions.includes('PB,')) {
        //Get the selected Admin Follow-ups
        var selectedAdminFollowUps = lstBxAdminFollowUp.GetSelectedIndices();

        //Perform the validation
        if (selectedAdminFollowUps.length < 1 && !optionalCriteriaOptions.includes('PB,')) {
            e.isValid = false;
            e.errorText = "At least one Admin Follow-up must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
    }
}