$(function () {

    // Sort the date of the member
    SortWhosOnline();

    // **** GLOBAL MESSAGES **** //
    var globalMessage = $('div.globalmessageholder');
    if (globalMessage.length > 0) {
        globalMessage.delay(4000).fadeOut();
    }
    // **** GLOBAL MESSAGES ENDS **** //

    // **** BADGE SCRIPTS **** //
    if ($.QueryString["postbadges"] == "true") {
        // Do a post badge check
        UserPost();
    }
    // **** BADGE SCRIPTS END **** //

    // **** START POLL SCRIPTS **** //
    // Remove the polls
    $(".removepollbutton").click(function (e) {
        e.preventDefault();
        //Firstly Show the Poll Section
        $('.pollanswerholder').hide();
        $('.pollanswerlist').html("");
        // Hide this button now
        $(this).hide();
        // Show the add poll button
        $(".createpollbutton").show();
        counter = 0;
    });

    // Create Polls
    $(".createpollbutton").click(function (e) {
        e.preventDefault();
        //Firstly Show the Poll Section
        $('.pollanswerholder').show();
        // Now add in the first row
        AddNewPollAnswer(counter);
        counter++;
        // Hide this button now
        $(this).hide();
        // Show the remove poll button
        $(".removepollbutton").show();
    });

    // Add a new answer
    $(".addanswer").click(function (e) {
        e.preventDefault();
        AddNewPollAnswer(counter);
        counter++;
        //ShowHideRemovePollAnswerButton(counter);
    });

    // Remove a poll answer
    $(".removeanswer").click(function (e) {
        e.preventDefault();
        if (counter > 0) {
            counter--;
            $("#answer" + counter).remove();
            //ShowHideRemovePollAnswerButton(counter);
        }
    });

    // Poll vote radio button click
    $(".pollanswerselect").click(function () {
        //Firstly Show the submit poll button
        $('.pollvotebuttonholder').show();
        // set the value of the hidden input to the answer value
        var answerId = $(this).data("answerid");
        $('.selectedpollanswer').val(answerId);
    });

    $(".pollvotebutton").click(function (e) {
        e.preventDefault();

        var pollId = $('#Poll_Id').val();
        var answerId = $('.selectedpollanswer').val();

        var updatePollViewModel = new Object();
        updatePollViewModel.PollId = pollId;
        updatePollViewModel.AnswerId = answerId;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(updatePollViewModel);

        $.ajax({
            url: app_base + 'Poll/UpdatePoll',
            type: 'POST',
            dataType: 'html',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                $(".pollcontainer").html(data);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });

    });
    // **** START POLL SCRIPTS END **** //
});


function SortWhosOnline() {
    $.getJSON(app_base + 'umbraco/Surface/DialogueMembersSurface/LastActiveCheck');
}

// **** POLL FUNCTIONS START **** //
function AddNewPollAnswer(counter) {
    var placeHolder = $('#pollanswerplaceholder').val();
    var liHolder = $(document.createElement('li')).attr("id", 'answer' + counter);
    liHolder.html('<input type="text" name="PollAnswers[' + counter + '].Answer" id="PollAnswers_' + counter + '_Answer" value="" placeholder="' + placeHolder + '" class="form-control" />');
    liHolder.appendTo(".pollanswerlist");
}


function UserPost() {

    $.ajax({
        url: app_base + 'Badge/Post',
        type: 'POST',
        success: function (data) {
            // No need to do anything
        },
        error: function (xhr, ajaxOptions, thrownError) {
            ShowUserMessage("Error: " + xhr.status + " " + thrownError);
        }
    });
}
// **** POLL FUNCTIONS END **** //

//**** MISC ****//
function ShowUserMessage(message) {
    if (message != null) {
        var jsMessage = $('#jsquickmessage');
        var toInject = "<div class=\"alert alert-block alert-info fade in\"><a href=\"#\" data-dismiss=\"alert\" class=\"close\">&times;<\/a>" + message + "<\/div>";
        jsMessage.html(toInject);
        jsMessage.show();
        $('div.alert').delay(2200).fadeOut();
    }
}

(function ($) {
    $.QueryString = (function (a) {
        if (a == "") return {};
        var b = {};
        for (var i = 0; i < a.length; ++i) {
            var p = a[i].split('=');
            if (p.length != 2) continue;
            b[p[0]] = decodeURIComponent(p[1].replace(/\+/g, " "));
        }
        return b;
    })(window.location.search.substr(1).split('&'));
})(jQuery);
//**** MISC END ****//