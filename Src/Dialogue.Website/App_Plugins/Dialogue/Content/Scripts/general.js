$(function () {

    // Sort the date of the member
    SortWhosOnline();

    // We add the post click events like this, so we can reattach when we do the show more posts
    AddPostClickEvents();

    // Attach files click handler
    ShowFileUploadClickHandler();
    DisplayWaitForPostUploadClickHandler();

    // Attach Approve Click Handers
    ApproveHandlers();

    // **** GLOBAL MESSAGES **** //
    var globalMessage = $('div.globalmessageholder');
    if (globalMessage.length > 0) {
        globalMessage.delay(7000).fadeOut();
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
            url: app_base + 'umbraco/Surface/DialoguePollSurface/UpdatePoll',
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

    // **** Email subscription Start **** //
    $(".emailsubscription").click(function (e) {
        e.preventDefault();
        var entityId = $(this).attr('rel');
        $(this).hide();
        var subscriptionType = $(this).find('span').attr('rel');

        var subscribeEmailViewModel = new Object();
        subscribeEmailViewModel.Id = entityId;
        subscribeEmailViewModel.SubscriptionType = subscriptionType;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(subscribeEmailViewModel);

        $.ajax({
            url: app_base + 'umbraco/Surface/DialogueEmailSurface/Subscribe',
            type: 'POST',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                $(".emailunsubscription").fadeIn();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });

    });

    $(".emailunsubscription").click(function (e) {
        e.preventDefault();
        var entityId = $(this).attr('rel');
        $(this).hide();
        var subscriptionType = $(this).find('span').attr('rel');

        var unSubscribeEmailViewModel = new Object();
        unSubscribeEmailViewModel.Id = entityId;
        unSubscribeEmailViewModel.SubscriptionType = subscriptionType;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(unSubscribeEmailViewModel);

        $.ajax({
            url: app_base + 'umbraco/Surface/DialogueEmailSurface/UnSubscribe',
            type: 'POST',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                $(".emailsubscription").fadeIn();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });
    });
    // **** Email subscription End **** //

    //** Topic More Posts **//
    $(".showmoreposts").click(function (e) {
        e.preventDefault();
        var topicId = $('#topicId').val();
        var pageIndex = $('#pageIndex');
        var totalPages = parseInt($('#totalPages').val());
        var activeText = $('span.smpactive');
        var loadingText = $('span.smploading');
        var showMoreLink = $(this);

        activeText.hide();
        loadingText.show();

        var getMorePostsViewModel = new Object();
        getMorePostsViewModel.TopicId = topicId;
        getMorePostsViewModel.PageIndex = pageIndex.val();
        getMorePostsViewModel.Order = $.QueryString["order"];

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(getMorePostsViewModel);

        $.ajax({
            url: app_base + 'umbraco/Surface/DialogueTopicSurface/AjaxMorePosts',
            type: 'POST',
            dataType: 'html',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {

                // Now add the new posts
                showMoreLink.before(data);

                // Update the page index value
                var newPageIdex = (parseInt(pageIndex.val()) + parseInt(1));
                pageIndex.val(newPageIdex);

                // If the new pageindex is greater than the total pages, then hide the show more button
                if (newPageIdex > totalPages) {
                    showMoreLink.hide();
                }

                // Lastly reattch the click events
                AddPostClickEvents();
                ShowFileUploadClickHandler();
                DisplayWaitForPostUploadClickHandler();
                activeText.show();
                loadingText.hide();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                activeText.show();
                loadingText.hide();
            }
        });
    });

    //**** Private Messages ****//
    $(".privatemessagedelete").click(function (e) {
        e.preventDefault();
        var linkClicked = $(this);
        var messageId = linkClicked.data('messageid');
        var deletePrivateMessageViewModel = new Object();
        deletePrivateMessageViewModel.Id = messageId;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(deletePrivateMessageViewModel);

        $.ajax({
            url: app_base + 'umbraco/Surface/DialogueMessageSurface/Delete',
            type: 'POST',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                // deleted, remove table row
                RemovePrivateMessageTableRow(linkClicked);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });
    });

});

function ApproveHandlers() {

    $(".approvemember").click(function (e) {
        e.preventDefault();

        var linkClicked = $(this);
        var id = linkClicked.data('memberid');

        // create viewmodel
        var viewModel = new Object();
        viewModel.Id = id;
        var strung = JSON.stringify(viewModel);

        $.ajax({
            url: app_base + 'umbraco/Surface/DialogueMemberSurface/ApproveMember',
            type: 'POST',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function () {
                linkClicked.parents('tr').first().fadeOut();                
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });
    });

    $(".approvepost").click(function (e) {
        e.preventDefault();

        var linkClicked = $(this);
        var id = linkClicked.data('postid');

        // create viewmodel
        var viewModel = new Object();
        viewModel.Id = id;
        var strung = JSON.stringify(viewModel);

        $.ajax({
            url: app_base + 'umbraco/Surface/DialoguePostSurface/ApprovePost',
            type: 'POST',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function () {
                linkClicked.parents('tr').first().fadeOut();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });
    });

    $(".approvetopic").click(function (e) {
        e.preventDefault();

        var linkClicked = $(this);
        var id = linkClicked.data('topicid');

        // create viewmodel
        var viewModel = new Object();
        viewModel.Id = id;
        var strung = JSON.stringify(viewModel);

        $.ajax({
            url: app_base + 'umbraco/Surface/DialogueTopicSurface/ApproveTopic',
            type: 'POST',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function () {
                linkClicked.parents('tr').first().fadeOut();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });
    });

}

function RemovePrivateMessageTableRow(linkClicked) {
    linkClicked.parents('tr').first().fadeOut();
}

function DisplayWaitForPostUploadClickHandler() {
    var postUploadButton = $('.postuploadbutton');
    if (postUploadButton.length > 0) {
        postUploadButton.click(function (e) {
            var uploadHolder = $(this).closest("div.postuploadholder");
            var ajaxSpinner = uploadHolder.find("span.ajaxspinner");
            ajaxSpinner.show();
            $(this).hide();
        });
    }
}

function ShowFileUploadClickHandler() {
    var attachButton = $('.postshowattach');
    if (attachButton.length > 0) {
        attachButton.click(function (e) {
            e.preventDefault();
            var postHolder = $(this).closest("div.post");
            var uploadHolder = postHolder.find("div.postuploadholder");
            uploadHolder.toggle();
        });
    }
}


function SortWhosOnline() {
    $.getJSON(app_base + 'umbraco/Surface/DialogueMemberSurface/LastActiveCheck');
}

// *** Votes ***//

function AddPostClickEvents() {

    $(".post .issolution").click(function (e) {
        e.preventDefault();
        var solutionHolder = $(this);
        var postId = solutionHolder.data('postid');
        var postDiv = solutionHolder.closest('.post');

        var markAsSolutionViewModel = new Object();
        markAsSolutionViewModel.Post = postId;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(markAsSolutionViewModel);

        $.ajax({
            url: app_base + 'umbraco/Surface/DialogueVoteSurface/MarkAsSolution',
            type: 'POST',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            success: function (data) {

                // Sort the button out
                solutionHolder
                .removeClass("issolution")
                .addClass("issolution-solved")
                .attr('title', '')
                .unbind("click")
                .html(data);

                // Add solution true to the post
                postDiv.removeClass("solution-false").addClass("solution-true");

                $('.issolution').hide();

                BadgeSolution(postId);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });

    });

    $(".post a.vote").click(function (e) {
        e.preventDefault();
        var voteLink = $(this);
        var postId = voteLink.data('postid');
        var voteType = voteLink.data('votetype');
        var isVoteUp = (voteType == "up");
        var numberHolder = voteLink.find('span');
        var postHolder = voteLink.closest('.post');
        var totalVoteCount = postHolder.find('.posttotalvotecount');


        var ajaxUrl = "umbraco/Surface/DialogueVoteSurface/PostVote";

        var voteViewModel = new Object();
        voteViewModel.Post = postId;
        voteViewModel.IsVoteUp = isVoteUp;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(voteViewModel);

        $.ajax({
            url: app_base + ajaxUrl,
            type: 'POST',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            success: function (data) {
                // Data returned is comma seperated
                var splitData = data.split(',');
                numberHolder.html(splitData[0]);
                totalVoteCount.html(splitData[1]);
                BadgeVote(postId, isVoteUp);

                // Add disable class to vote pills and remove click
                var allVoteButtons = voteLink.closest('.post-actions').find('.vote');
                allVoteButtons.addClass('disabled').removeClass("vote").unbind("click");
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });
    });

    $(".post a.favorite").click(function (e) {
        e.preventDefault();
        var favLink = $(this);
        var postId = favLink.data('postid');

        var ajaxUrl = "umbraco/Surface/DialogueFavouriteSurface/FavouritePost";

        var voteViewModel = new Object();
        voteViewModel.PostId = postId;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(voteViewModel);

        $.ajax({
            url: app_base + ajaxUrl,
            type: 'POST',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            success: function (data) {
                var favButton = favLink.closest('.post-actions').find('.favorite');
                favButton.text(data);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });
    });

    $(".post a.removefav").click(function (e) {
        e.preventDefault();
        var favLink = $(this);
        var postId = favLink.data('postid');

        var ajaxUrl = "umbraco/Surface/DialogueFavouriteSurface/FavouritePost";

        var viewModel = new Object();
        viewModel.PostId = postId;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(viewModel);

        $.ajax({
            url: app_base + ajaxUrl,
            type: 'POST',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            success: function (data) {
                favLink.closest('.post').remove();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });
    });
}


//function SuccessfulThumbUp(karmascore) {
//    var currentKarma = parseInt($(karmascore).text());
//    $(karmascore).text((currentKarma + 1));
//}

//function SuccessfulThumbDown(karmascore) {
//    var currentKarma = parseInt($(karmascore).text());
//    $(karmascore).text((currentKarma - 1));
//}

function BadgeSolution(postId) {

    // Ajax call to post the view model to the controller
    var markAsSolutionBadgeViewModel = new Object();
    markAsSolutionBadgeViewModel.PostId = postId;

    var ajaxUrl = "umbraco/Surface/DialogueBadgeSurface/MarkAsSolution";

    // Ajax call to post the view model to the controller
    var strung = JSON.stringify(markAsSolutionBadgeViewModel);

    $.ajax({
        url: app_base + ajaxUrl,
        type: 'POST',
        data: strung,
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            // No need to do anything
        },
        error: function (xhr, ajaxOptions, thrownError) {
            ShowUserMessage("Error: " + xhr.status + " " + thrownError);
        }
    });
}

function BadgeVote(postId, isVoteUp) {

    // Ajax call to post the view model to the controller
    var voteBadgeViewModel = new Object();
    voteBadgeViewModel.PostId = postId;

    var ajaxUrl = "";
    if (isVoteUp) {
        ajaxUrl = "umbraco/Surface/DialogueBadgeSurface/VoteUpPost";
    } else {
        ajaxUrl = "umbraco/Surface/DialogueBadgeSurface/VoteDownPost";
    }

    // Ajax call to post the view model to the controller
    var strung = JSON.stringify(voteBadgeViewModel);

    $.ajax({
        url: app_base + ajaxUrl,
        type: 'POST',
        data: strung,
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            // No need to do anything
        },
        error: function (xhr, ajaxOptions, thrownError) {
            ShowUserMessage("Error: " + xhr.status + " " + thrownError);
        }
    });
}

/*---- END VOTES -------*/

// **** POLL FUNCTIONS START **** //
function AddNewPollAnswer(counter) {
    var editMode = $('#EditMode').length > 0;
    var editExtraDataDot = "";
    var editExtraDataUnderscore = "";
    if (editMode) {
        editExtraDataDot = "EditPostViewModel.";
        editExtraDataUnderscore = "EditPostViewModel_";
    }

    var placeHolder = $('#pollanswerplaceholder').val();
    var liHolder = $(document.createElement('li')).attr("id", 'answer' + counter);
    liHolder.html('<input type="text" name="' + editExtraDataDot + 'PollAnswers[' + counter + '].Answer" id="' + editExtraDataUnderscore + 'PollAnswers_' + counter + '_Answer" value="" placeholder="' + placeHolder + '" class="form-control" />');
    liHolder.appendTo(".pollanswerlist");
}


function UserPost() {

    $.ajax({
        url: app_base + 'umbraco/Surface/DialogueBadgeSurface/Post',
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
        $('div.alert').delay(7000).fadeOut();
    }
}
function AjaxPostSuccess() {
    // Grab the span the newly added post is in
    var postHolder = $('#newpostmarker');

    // Now add a new span after with the key class
    // In case the user wants to add another ajax post straight after
    postHolder.after('<span id="newpostmarker"></span>');

    // Finally chnage the name of this element so it doesn't insert it into the same one again
    postHolder.attr('id', 'tonystarkrules');

    // And more finally clear the post box
    $('.createpost').val('');
    if ($(".bbeditorholder textarea").length > 0) {
        $(".bbeditorholder textarea").data("sceditor").val('');
    }
    if ($('.wmd-input').length > 0) {
        $(".wmd-input").val('');
        $(".wmd-preview").html('');
    }
    if (typeof tinyMCE != "undefined") {
        tinyMCE.activeEditor.setContent('');
    }

    // Re-enable the button
    $('#createpostbutton').attr("disabled", false);

    // Finally do an async badge check
    UserPost();

    // Attached the upload click events
    ShowFileUploadClickHandler();
    DisplayWaitForPostUploadClickHandler();
    AddPostClickEvents();    
}

function AjaxPostBegin() {
    $('#createpostbutton').attr("disabled", true);
}

function AjaxPostError(message) {
    ShowUserMessage(message);
    $('#createpostbutton').attr("disabled", false);
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
