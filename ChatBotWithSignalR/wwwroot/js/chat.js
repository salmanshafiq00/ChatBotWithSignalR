
let toUserId = '';
let selectedGroupId = 0;


// Make scroll bar always at bottom
function scrollToBottom() {
    let messageBody = document.querySelector('#conversationContainer');
    messageBody.scrollTop = messageBody.scrollHeight - messageBody.clientHeight;
}

// function for loading the conversations
let loadConversions = (el, userId) => {
    toUserId = userId;
    $(el).closest('li').siblings('li').removeClass('active');
    $(el).closest('li').addClass('active');
    $(`#loadConversions`).load(`/Chat/Chat/LoadConversationByUserId?toUserId=${userId}`, (response, status) => {
        $('#ToUserId').val(userId);
        scrollToBottom();
        $(`#notify_${toUserId}`).text('');
        $(`#msgContent`).focus();

        // Unread message update
        $.post(`/Chat/Chat/UpdateVisibilityStatus`, { fromUserId: toUserId }, (result) => {
            console.log(result);
        });
    });
}

// function for loading the conversations
let loadConversionsByGroupId = (groupId) => {
    selectedGroupId = groupId;
    $(`#loadConversions`).load(`/Chat/Chat/LoadConversionsByGroupId?groupId=${groupId}`, (response, status) => {
        $('#GroupId').val(groupId);
        scrollToBottom();
        //$(`#notify_${toUserId}`).text('');
        $(`#msgContent`).focus();
        // Unread message update
        //$.post(`/Chat/Chat/UpdateVisibilityStatus`, { fromUserId: toUserId }, (result) => {
        //    console.log(result);
        //});
    });
}


let sendMessage = form => {
    try {
        let msgContent = $(`#msgContent`).val();
        if (!msgContent) {
            toastr.error(`Please write something`, `warning`);
            $(`#msgContent`).focus();
            return;
        }
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (result) {
                if (result.isSuccess) {
                    $(`#messageList`).append(`<div class="ownMessage">
                                                    <pre class="text-white">${msgContent.replace(/\n\r?/g, '<br />')}</pre>
                                                    <span class="time">${result.time}</span>
                                                  </div>`);
                    $(`#msgContent`).val('').trigger('input');
                    $(`#msgContent`).focus();
                    scrollToBottom();
                    //sendMessageToUser(conversation);          // this method is not called here because sendasync method has already called in controller
                }
                else {
                    toastr.error(`Message not sent`, `warning`);
                }
            },
            error: function (err) {
                console.log(err)
            }
        })
        return false;
    } catch (ex) {
        console.log(ex)
    }
}



// receive message from the client
function addMessageToConversation(conversation) {
    // if loadConversions contains messageList div
    if (document.getElementById('loadConversions').contains(document.getElementById('messageList'))) {
        let toUserIdFromHeader = $('#toUserIdFromHeader').text();
        let toGroupIdFromHeader = $('#toGroupIdFromHeader').text();
        console.log(toUserIdFromHeader);
        //console.log(toGroupIdFromHeader);
        console.log(conversation.fromUserId);
        if (conversation.fromUserId == toUserIdFromHeader) {
            $(`#messageList`).append(`<div class="otherMessage">
                                        <pre>${conversation.textMessage}</pre>
                                        <span class="time">${conversation.toShortTime}</span>
                                     </div>`);
            scrollToBottom();

            // Unread message update
            $.post(`/Chat/Chat/UpdateVisibilityStatus`, { fromUserId: conversation.fromUserId }, (result) => {
            });
        }
        else if (conversation.groupId > 0 && conversation.groupId == toGroupIdFromHeader) {
            $(`#messageList`).append(`<div class="otherMessage">
                                        <pre>${conversation.textMessage}</pre>
                                        <span class="time">${conversation.toShortTime}</span>
                                      </div>`);
            scrollToBottom();
        }
        else {
            let notifyBadge = $(`#notify_${conversation.fromUserId}`);
            let notifyCount = $(notifyBadge).text();
            if (notifyCount) {
                $(notifyBadge).text(parseInt(notifyCount) + 1);
            }
            else {
                $(notifyBadge).text(0 + 1);
            }
        }
    } else {
        let notifyBadge = $(`#notify_${conversation.fromUserId}`);
        let notifyCount = $(notifyBadge).text();
        if (notifyCount) {
            $(notifyBadge).text(parseInt(notifyCount) + 1);
        }
        else {
            $(notifyBadge).text(0 + 1);
        }
    }
    var audio = document.getElementById("notification-sound");
    audio.play();
}

// receive message from the Group client
function addMessageToGroupConversation(conversation) {
    if (document.getElementById('loadConversions').contains(document.getElementById('messageList'))) {
        //$(`#messageList`).append(`<li>${conversation.textMessage}</li>`);
        $(`#messageList`).append(`<div class="otherMessage">
                                    <pre>${conversation.textMessage}</pre>
                                    <span class="time">${conversation.toShortTime}</span>
                                 </div>`);
        scrollToBottom();
    } else {
        let notifyBadge = $(`#notify_${conversation.fromUserId}`);
        let notifyCount = $(notifyBadge).text();
        if (notifyCount) {
            $(notifyBadge).text(parseInt(notifyCount) + 1);
        }
        else {
            $(notifyBadge).text(0 + 1);
        }
    }
}

// All Active user make color green other black
function userConnectivity(connectedUserList) {
    let ulIdListList = document.querySelectorAll(`.userlistclass`);
    ulIdListList.forEach((value) => {
        let attrValue = value.getAttribute('id');
        let userIdValue = attrValue.split('_')[1];
        let isInclude = connectedUserList.includes(`${userIdValue}`);
        if (isInclude) {
            $(value).find(`.fa-circle`).removeClass('offline');
            $(value).find(`.fa-circle`).addClass('online');
        } else {
            $(value).find(`.fa-circle`).removeClass('online');
            $(value).find(`.fa-circle`).addClass('offline');
        }
    });
}


// create group
// Get modal for create or update User Group
function getCreateOrUpdateGroupModal(id, title) {
    $.get(`/Chat/Chat/OnGetGroupCreateOrEdit?id=${id}`, (result) => {
        $('#formModal .modal-body').html(result);
        $('#formModal #modalHeaderTitle').html(title);
        $('#formModal').modal('show');
    });
}


// Get modal for create or update User Group
function getCreateOrUpdateUserGroupModal(groupId) {
    $.get(`/Chat/Chat/OnGetCreateOrEdit?groupId=${groupId}`, (result) => {
        $('#formModal .modal-body').html(result);
        $('#formModal').modal('show');
    });
}

// 
function deleteGroup(groupId) {
    var isConfirmed = confirm("Do you want to delete this group?");
    if (isConfirmed) {
        $.post(`/Chat/Chat/DeleteGroup`, { groupId: groupId }, (result) => {
            if (result.isSuccess) {
                //toastr.success(`${result.groupName} is deleted successfully`, `success`);
                alert(`${result.groupName} is deleted successfully`);
                window.location.assign("https://localhost:7077/Chat/Chat/Index")
            }
            else {
                toastr.error(`${result.msg}`);
            }
        });
    }
    else {
        return;
    }
}

//

function getNotifications(notification) {
    $(`#notify-wrapper`).append(`<li><a class="dropdown-item" href="#">${notification.text}</a></li>
`);
}