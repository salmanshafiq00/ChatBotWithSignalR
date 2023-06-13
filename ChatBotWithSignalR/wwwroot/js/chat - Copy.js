﻿
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
        $('.footer').removeClass('d-none');
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
        $('.footer').removeClass('d-none');
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
        let files = $('#Files').get(0);
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (result) {
                if (result.isSuccess) {
                    setConversationToCaller(result, msgContent, files);
                    $(`#msgContent`).val('').trigger('input');
                    $(`#msgContent`).focus();
                    $('#Files').val("");
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

        if (conversation.fromUserId == toUserIdFromHeader) {
            setConversationToChatBox(conversation)
            scrollToBottom();

            // Unread message update
            $.post(`/Chat/Chat/UpdateVisibilityStatus`, { fromUserId: conversation.fromUserId }, (result) => {
            });
        }
        else if (conversation.groupId > 0 && conversation.groupId == toGroupIdFromHeader) {
            setConversationToChatBox(conversation)
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
    }// if loadConversions does not contain messageList div
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
function getCreateOrUpdateGroupModal(id, title = 'Title') {
    $.get(`/Chat/Chat/OnGetGroupCreateOrEdit?id=${id}`, (result) => {
        $('#formModal #modalHeaderTitle').html(title);
        $('#formModal .modal-body').html(result);
        $('#formModal').modal('show');
    });
}


// Get modal for create or update User Group
function getCreateOrUpdateUserGroupModal(groupId, title = '') {
    $.get(`/Chat/Chat/OnGetCreateOrEdit?groupId=${groupId}`, (result) => {
        $('#formModal #modalHeaderTitle').html(title);
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
                alert(`Group(${result.groupName}) is deleted successfully`);
                location.reload();
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

// Set conversation to chatbox
function setConversationToChatBox(conversation) {
    if (conversation.textMessage) {
        $(`#messageList`).append(`<div class="otherMessage">
                                        <p id="toUserHeader">${conversation.fromUserName}</p>
                                        <pre>${conversation.textMessage}</pre>
                                        <span class="time">${conversation.toShortTime}</span>
                                     </div>`);
    }

    if (conversation.conversationFiles.length > 0) {
        for (var i = 0; i < conversation.conversationFiles.length; i++) {
            let fileType = conversation.conversationFiles[i].fileType.split('/')[0];
            let fileName = conversation.conversationFiles[i].fileName;
            let fileSrc =  conversation.conversationFiles[i].fileUrl;

            //$(`#messageList`).append(` <div class="otherMessageImg">
            //                    <p id="toUserHeader">${conversation.fromUserName}</p>
            //                    <a onclick="showImageToModal('${conversation.conversationFiles[i].fileUrl}')">
            //                        <img src="${conversation.conversationFiles[i].fileUrl}" alt="file" class="img-fluid img-thumbnail ml-0 mr-0 coversationImg">
            //                    </a>
            //                    <span class="time">${conversation.toShortTime}</span>
            //                </div>`);

            if (fileType == 'image') {
                $(`#messageList`).append(` <div class="otherMessageImg">
                                <p id="toUserHeader">${conversation.fromUserName}</p>
                                <a onclick="showImageToModal('${fileSrc}')">
                                    <img src="${fileSrc}" alt="file" class="img-fluid img-thumbnail ml-0 mr-0 coversationImg">
                                </a>
                                <span class="time">${conversation.toShortTime}</span>
                            </div>`);
            }
            else if (fileType == 'video') {
                $(`#messageList`).append(` <div class="otherMessageImg">
                                <p id="toUserHeader">${conversation.fromUserName}</p>
                                <video controls class="coversationImg">
                                        <source src="${fileSrc}">
                                    </video>
                                <span class="time">${conversation.toShortTime}</span>
                            </div>`);
            }
            else if (fileType == 'audio') {
                $(`#messageList`).append(` <div class="otherMessageImg">
                              <p id="toUserHeader">${conversation.fromUserName}</p>
                                <audio controls>
                                    <source src="${fileSrc}">
                                </audio>
                               <span class="time">${conversation.toShortTime}</span>
                            </div>`);
            }
            else {
                $(`#messageList`).append(` <div class="otherMessageImg">
                                <p id="toUserHeader"><i class="fa-solid fa-file-lines"></i> ${conversation.fromUserName}</p>
                                <a onclick="showFileInModal('${fileSrc}')">
                                    <p class="text-white">${fileName}</p>
                                </a>
                                <span class="time">${conversation.toShortTime}</span>
                            </div>`);
            }
        }
    }

    let fileExtension = '';
}

// Set conversation to caler
function setConversationToCaller(result, msgContent, files) {
    if (msgContent) {
        $(`#messageList`).append(`<div class="ownMessage">
                                                    <pre class="text-white text-wrap">${msgContent.replace(/\n\r?/g, '<br />')}</pre>
                                                    <span class="time">${result.time}</span>
                                                  </div>`);

    }
    if (files.files.length > 0) {
        debugger;
        for (var i = 0; i < files.files.length; i++) {
            let fileSrc = URL.createObjectURL(files.files[i])
            let fileType = files.files[i].type.split('/')[0];
            let fileName = files.files[i].name;

            if (fileType == 'image') {
                $(`#messageList`).append(` <div class="ownMessageImg">
                                <a onclick="showImageToModal('${fileSrc}')">
                                    <img src="${fileSrc}" alt="file" class="img-fluid img-thumbnail ml-0 mr-0 coversationImg">
                                </a>
                                <span class="time">${result.time}</span>
                            </div>`);
            }
            else if (fileType == 'video') {
                $(`#messageList`).append(` <div class="ownMessageImg">
                                 <p class="text-white">${fileName}</p>
                                <video controls class="coversationImg">
                                    <source src="${fileSrc}">
                                </video>
                                <span class="time">${result.time}</span>
                            </div>`);
            }
            else if (fileType == 'audio') {
                $(`#messageList`).append(` <div class="ownMessageImg">
                                <audio controls>
                                    <source src="${fileSrc}">
                                </audio>
                                <span class="time">${result.time}</span>
                            </div>`);
            }
            else {
                $(`#messageList`).append(` <div class="ownMessageImg">
                                <a onclick="showFileInModal('${fileSrc}')">
                                    <p class="text-white"><i class="fa-solid fa-file-lines"></i> ${fileName}</p>
                                </a>
                                <span class="time">${result.time}</span>
                            </div>`);
            }
        }
    }
}

// Set Notification 

function getNotifications(notification) {
    debugger;
    $(`#notify-wrapper`).append(`<li>
                                    <a class="dropdown-item" href="#">${notification.text}</a>
                                </li>
                                <div class="dropdown-divider"></div>
    `);
}