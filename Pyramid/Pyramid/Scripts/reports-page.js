/* Author: Benjamin Simmons
 * Create date: 10/8/2019
 * Intent: This file is designed to take the Reports.aspx JavaScript functions
 * out of the page itself in order to save room and make the flow more logical.
 */

//This function sets defaults for the criteria bases on the passed default options
function setCriteriaDefaults(reportCriteriaDefaults) {
    //Set the default program (Not in the defaults selection yet)
    var currentProgramFK = String($('[ID$="hfCurrentProgramFK"]').val());

    //If the current program FK exists, select it in the list box
    if (currentProgramFK !== null && currentProgramFK !== undefined && currentProgramFK !== '') {
        var programListItem = lstBxProgram.FindItemByValue(currentProgramFK);
        lstBxProgram.SetSelectedItem(programListItem);
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
            endDate = moment().add(months, 'M').startOf('month');

            //Set the date edit date
            deEndDate.SetDate(endDate.toDate());

        }
        else if (defaultCode.includes('RED-Y')) {
            //Relative end date (Years)
            //Get the years
            years = value.substring(5);

            //Add the years to the end date
            endDate = moment().add(years, 'Y').startOf('year');

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
    if (reportCriteriaOptions.includes('PHC,')) {
        //Show the program list div
        $('#divPHC').removeClass('hidden');

        //Make sure the list boxes shows correctly
        lstBxProgram.AdjustControl();
        lstBxHub.AdjustControl();
        lstBxCohort.AdjustControl();
    }
    else {
        hideAndClearPHC();
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
}

//This function hides and clears all criteria
function hideAndClearCriteria() {
    //Hide and clear criteria options
    hideAndClearPHC();
    hideAndClearSED();
    hideAndClearPIT();
    hideAndClearCR();
    hideAndClearCHI();
    hideAndClearCD();
    hideAndClearEMP();
    hideAndClearTCH();
    hideAndClearCCH();
}

//This function hides and clears the programs, hubs, and cohorts lists
function hideAndClearPHC() {
    //Hide the programs, hubs, and cohorts lists
    $('#divPHC').addClass('hidden');

    //Clear the programs, hubs, and cohorts lists
    lstBxProgram.UnselectAll();
    lstBxHub.UnselectAll();
    lstBxCohort.UnselectAll();
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
            button.hide();

            //Show the loading button
            loadingButton.show();
        }
        else {
            //Call the client validation failed method
            clientValidationFailed();
        }
    }
    else {
        //Tell the user if no report was selected
        showNotification("warning", "No Report Selected", "No report was selected.", 5000);
    }
}

//Validate the program list box
function validatePHCLists(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the program criteria exists
    if (criteriaOptions.includes('PHC,')) {
        //Get the selected programs, hubs, and cohorts
        var selectedPrograms = lstBxProgram.GetSelectedIndices();
        var selectedHubs = lstBxHub.GetSelectedIndices();
        var selectedCohorts = lstBxCohort.GetSelectedIndices();

        //Perform the validation
        if (selectedPrograms.length < 1 && selectedHubs.length < 1 && selectedCohorts.length < 1
            && !optionalCriteriaOptions.includes('PHC,')) {
            e.isValid = false;
            e.errorText = "At least one program, hub, or cohort must be selected!";
        }
        else {
            e.isValid = true;
        }
    }
    else {
        e.isValid = true;
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
        else if (startDate !== null && startDate > new Date()) {
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
        else if (endDate > new Date()) {
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
        else if (pointInTime !== null && pointInTime > new Date()) {
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

//Validate the classroom list box
function validateClassroomList(s, e) {
    //Get the criteria options
    var criteriaOptions = $('[ID$="hfReportToRunCriteriaOptions"]').val();
    var optionalCriteriaOptions = $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val();

    //Only validate if the classroom criteria exists
    if (criteriaOptions.includes('CR,')) {
        //Get the selected
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
        //Get the selected
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
        //Get the selected programs, hubs, and cohorts
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
    if (criteriaOptions.includes('CHI,')) {
        //Get the selected
        var selectedEmployeees = lstBxEmployee.GetSelectedIndices();

        //Perform the validation
        if (selectedEmployeees.length < 1 && !optionalCriteriaOptions.includes('CHI,')) {
            e.isValid = false;
            e.errorText = "At least one employee must be selected!";
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
    if (criteriaOptions.includes('CHI,')) {
        //Get the selected
        var selectedTeacheres = lstBxTeacher.GetSelectedIndices();

        //Perform the validation
        if (selectedTeacheres.length < 1 && !optionalCriteriaOptions.includes('CHI,')) {
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
    if (criteriaOptions.includes('CHI,')) {
        //Get the selected
        var selectedCoaches = lstBxCoach.GetSelectedIndices();

        //Perform the validation
        if (selectedCoaches.length < 1 && !optionalCriteriaOptions.includes('CHI,')) {
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