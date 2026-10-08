var mp3Notify = '/Content/Chat/notification-sound.mp3', chat = null, countUsers = 0, counterOnline = null,
    bookIdConst = parseInt($('#bookId').val()),
    chatMessagesForm = null, isConnectToServer = false, updateIntervalReconnect = null;

//Массив с очередью для мониторинга состояния пользователей набирающих текст
var writeListSetIntervalArray = []; //[{UserId,IdInterval}]
//Список пользователей, которые набирают текст
var userWriteListArray = [];
//Таймер для отслеживания массива набирающих текст
var timerWriteListArray;

$(document).ready(function () {    
    counterOnline = $('.usersCounter');
    chat = $.connection.chatHub;
    chatMessages = $('#chatMessages');

    chat.client.messageHasRemoved = function () {
        alert(LocaleJs["Сообщение_успешно_удалено"]);        
    }
    
    chat.client.userHasBlocked = function (userEmail) {
        alert(userEmail + ' ' + LocaleJs["успешно_заблокирован"]);
    }
    
    chat.client.inBlackList = function () {
        alert(LocaleJs["Доступ_к_данной_функции_ограничен_модератором"]);
    }

    chat.client.successReport = function (IdStringSocketId) {
        $('#' + IdStringSocketId).hide('100');
        alert(LocaleJs["Спасибо_Ваша_жалоба_принята"]);
    }

    chat.client.failureReportError = function () {
        alert(LocaleJs["Не_удалось_получить_информацию_о_сообщении"]);
    }

    chat.client.failureReportRetry = function () {
        alert(LocaleJs["Нельзя_повторно_подавать_жалобу"]);
    }

    chat.client.onConnected = function (currentUserMail) {
        successConnection(currentUserMail);        
    }

    chat.client.onUserConnected = function (userEmail) {
        console.log('connected', userEmail);
        toastMess(userEmail+' is Connected', false);
    }
    
    chat.client.onUserDisconnected = function (id, userName) {
        console.log('onUserDisconnected', userName); 
    }

    //Новое сообщение
    chat.client.addMessage = function (text, userEmail, utcTime, idMessage, PageId) {
        //console.log('new message:', text + ' : ' + userEmail + ' : ' + utcTime);
        var btnBlockUser = '',btnRemoveMessage = '';
        if (isHasAccess) {
            btnBlockUser = '<button onclick="blockUser(this)" class="report"><span class="material-icons" aria-hidden="true" title="' + LocaleJs["Заблокировать_пользователя"] +'">person_remove</span></button>';
            btnRemoveMessage = '<button onclick="removeMessage(this)" class="report"><span class="material-icons" aria-hidden="true" title="' + LocaleJs["Удалить_сообщение"] +'">delete</span></button>';
        }
        //Проверка на вложение
        var attachment = getattachmentViewMessage(PageId);

        $(chatMessages).append('<div id="mess_' + idMessage + '" class="comment-box newMessage"> <span class="commenter-name"> <a href="#">' + userEmail + '</a> <span class="comment-time">' + utcTime + ' (' + getTimeZone() + ')' + '</span> </span> <p class="comment-txt more">' + text + '</p> ' + attachment + ' <div class="comment-meta"> ' + btnBlockUser + btnRemoveMessage + ' <button onclick="report(this)" class="report"><span class="material-icons" aria-hidden="true" title="' + LocaleJs["Жалоба"] + '">report</span></button> <button data-id="' + idMessage + '" onclick="replyMessage(this)" class="comment-reply"><span class="material-icons" aria-hidden="true" title="' + LocaleJs["Ответить"] +'">reply</span></button> </div> <div class="comment-box add-comment reply-box"> <span class="commenter-name"> <input type="text" placeholder="Add a public reply" name="Add Comment"> <button type="submit" class="btn btn-default">Reply</button> <button type="cancel" class="btn btn-default reply-popup">Cancel</button> </span> </div> </div>');        
        notifyMessage();
        offsetChat();
    }

    //Ответ на сообщение
    chat.client.replyMessage = function (idMessage, message, UserEmail, utcTime, idMessageNew, PageId) {
        var parentMessage = $('#mess_' + idMessage);       
        if (parentMessage != null && parentMessage != undefined) {
            var btnBlockUser = '', btnRemoveMessage = '';
            if (isHasAccess) {
                btnBlockUser = '<button onclick="blockUser(this)" class="report"><span class="material-icons" aria-hidden="true" title="' + LocaleJs["Заблокировать_пользователя"] +'">person_remove</span></button>';
                btnRemoveMessage = '<button onclick="removeMessage(this)" class="report"><span class="material-icons" aria-hidden="true" title="' + LocaleJs["Удалить_сообщение"] +'">delete</span></button>';
            }

            //Проверка на вложение
            var attachment = getattachmentViewMessage(PageId);

            $(parentMessage).append('<div id="mess_' + idMessageNew + '" class="comment-box replied newMessage"> <span class="commenter-name"> <a href="#">' + UserEmail + '</a> <span class="comment-time">' + utcTime + ' (' + getTimeZone() + ')' + '</span> </span> <p class="comment-txt more">' + message + '</p> ' + attachment + '<div class="comment-meta"> ' + btnBlockUser + btnRemoveMessage + ' <button onclick="report(this)" class="report"><span class="material-icons" aria-hidden="true" title="' + LocaleJs["Жалоба"] + '">report</span></button> <button data-id="' + idMessageNew + '" onclick="replyMessage(this)" class="comment-reply"><span class="material-icons" aria-hidden="true" title="' + LocaleJs["Ответить"] +'">reply</span></button> </div>  </div> </div>');            
            notifyMessage();
            offsetChat();
        }
    }

    chat.client.successSendMessage = function () {
        clearChat();
    }

    chat.client.removeMessageSuccess = function (idMessage) {       
        $('#' + idMessage).remove();
    }

    chat.client.onUserUpdate = function (users) {        
        updateCounter(users);
    }

    //Визуализация отображения набора текста
    timerWriteListArray = setInterval(function () {        
        if (userWriteListArray.length > 0) {
            $('#isWriteMessageView').css('display', 'flex');
        }
        else {
            $('#isWriteMessageView').css('display', 'none');
            writeListSetIntervalArray = [];
        }
    }, 100);

    chat.client.isWriteMessageView = function (info) {
        if (info.BookIdConst != bookIdConst) {
            return;
        }        
        var isHaveUserInList = $('#isWriteMessageViewUsers').find('[user-id="' + info.UserId + '"]');
        if (isHaveUserInList.length < 1) {
            $(isHaveUserInList).remove();
            //Формируем элемент отображения
            //Запускаем обратный отсчет
            var infoUser = '<span user-id="' + info.UserId + '">' + info.UserEmail + '</span>';
            $('#isWriteMessageViewUsers').prepend(infoUser);            
            userWriteListArray.push(info.UserId);
        }

        var timersByUser = writeListSetIntervalArray.filter(x => x.UserId === info.UserId).map(x => x);
        if (timersByUser!=undefined && timersByUser.length > 0) {
            $(timersByUser).each(function () {
                clearTimeout(this.IntervalId);
                writeListSetIntervalArray = writeListSetIntervalArray.filter(item => item.IntervalId !== this.IntervalId);
            });
        }

        var intervalId = setTimeout(removeUserFromMessageView, 2000, info.UserId);
        writeListSetIntervalArray.push({ UserId: info.UserId, IntervalId: intervalId });
    }

    function removeUserFromMessageView(userId) {
        var isHaveUserInList = $('#isWriteMessageViewUsers').find('[user-id="' + userId + '"]');
        if (isHaveUserInList.length > 0) {
            $(isHaveUserInList).remove(); 
            userWriteListArray = userWriteListArray.filter(function (e) { return e !== userId });            
            console.log('userWriteListArray', userWriteListArray);
        }
    }

    $.connection.hub.start().done(function () {
        if (userIsAuth) {
            connect(parseInt($('#UserId').val()), $('#UserCode').val());
        }
        else {
            $('.usersOnline').remove();
        }
    });

    $.connection.hub.disconnected(function () {
        isConnectToServer = false;
        updateIntervalReconnect = setInterval(function () {            
            reconnect();            
        }, 5000);
    });    
});

//Визуализация вложения во входящем сообщении
function getattachmentViewMessage(PageId) {
    if (PageId != null && PageId != undefined) {
        var attachmnetView = '<div class="attachViewElement" onclick="viewAttachment(this)" data-pageId="' + PageId + '"><span class="material-icons"> link </span><span>' + LocaleJs["Ссылка_на_страницу"] +'</span></div>';
        return attachmnetView;
    }
    return '';
}

//Отправка состояния о наборе текста
function isWriteMessage() {
    chat.server.isWriteMessageSendInfo(bookIdConst);
}

function getTimeZone() {
    var timeZone = -(new Date().getTimezoneOffset() / 60);
    return '+'+timeZone;
}

function reconnect() {
    if (!isConnectToServer) {
        //console.log('reconnect');
        $('.bottom_drawer').addClass('notConnected');
        $(counterOnline).html('<span class="material-icons"> signal_cellular_connected_no_internet_4_bar </span>');
        $.connection.hub.start();
        connect(parseInt($('#UserId').val()), $('#UserCode').val());
    }
}

async function successConnection(currentUserMail) {
    $('.bottom_drawer').removeClass('notConnected');
    //console.log('connected to hub', currentUserMail);
    toastMess(LocaleJs["Инициализация_чата_успешно"], false);
    //console.log(chat);
    isConnectToServer = true;
    clearInterval(updateIntervalReconnect);    
}

function connect(userId, username) {
    var pageId = 0;
    chat.server.connect(userId, username, bookIdConst, pageId);
}

function send(text) {
    //Проверка на вложение
    var pageIdLink;
    var attachLink = $('.emojionearea #attachLink:first');
    if (attachLink != null && attachLink.length > 0) {
        pageIdLink = parseInt($(attachLink).attr('data-pageId'));
    }
    if (isAnswer) {
        var text = $('.emojionearea-editor').html();
        chat.server.replyMessage(idMessageAnswer, text, pageIdLink);
    }
    else {
        chat.server.addMessage(text, pageIdLink);
    }
    clearChat();
}

function notifyMessage() {    
    playNotify(mp3Notify);
    newMessageNotify();
    $('.bottom_drawer .create').addClass("newMessage").delay(1000).queue(function () {
        $(this).removeClass("newMessage").dequeue();
    });
}

var skipChat = 0, takeChat = 60;
var tempAnswerArray = [];
function getLastMessageChat(isRemoveLast) {    
    var historyBody = $('#historyMessages');
    //Для ответных сообщений    
    $.ajax({
        url: "/TimesMachine/getLastMessageChat",
        contentType: "application/json",
        data: JSON.stringify({ 'BookId': bookIdConst, "skip": skipChat, "take": takeChat }),
        dataType: "json",
        method: "POST",
        async: true,
        success: function (data, textStatus, xhr) {                   
            if (data.length < takeChat) {
                $('.toTopChatBtn').hide();
            }
            else {
                $('.toTopChatBtn').show();
            }
            if (!isRemoveLast) {

            }
            else {
                $('.comment-body .newMessage').remove();
            }
            if (data != null && data.length > 0) {
                skipChat += data.length;                
                $(data).each(function () {
                    var btnBlockUser = '', btnRemoveMessage = '';
                    if (isHasAccess) {
                        btnBlockUser = '<button onclick="blockUser(this)" class="report"><span class="material-icons" aria-hidden="true" title="' + LocaleJs["Заблокировать_пользователя"] +'">person_remove</span></button>';
                        btnRemoveMessage = '<button onclick="removeMessage(this)" class="report"><span class="material-icons" aria-hidden="true" title="' + LocaleJs["Удалить_сообщение"] +'">delete</span></button>';
                    }                    

                    if (!this.isAnswer) {
                        var findHaveElement = $('#mess_' + this.IdStringSocket);
                        if (findHaveElement.length == 0) {  
                            //Проверка на вложение
                            var attachment = '';
                            if (this.PageId != null) {
                                attachment = getattachmentViewMessage(this.PageId);
                            }
                            $(historyBody).prepend('<div id="mess_' + this.IdStringSocket + '" class="comment-box newMessage"> <span class="commenter-name"> <a href="#">' + this.Username + '</a> <span class="comment-time">' + this.dt + ' (' + getTimeZone() + ')' + '</span> </span> <p class="comment-txt more">' + this.MessageText + '</p> ' + attachment + ' <div class="comment-meta"> ' + btnBlockUser + btnRemoveMessage + ' <button onclick="report(this)" class="report"><span class="material-icons" aria-hidden="true" title="' + LocaleJs["Жалоба"] + '">report</span></button> <button data-id="' + this.IdStringSocket + '" onclick="replyMessage(this)" class="comment-reply"><span class="material-icons" aria-hidden="true" title="' + LocaleJs["Ответить"] + '">reply</span></button> </div> <div class="comment-box add-comment reply-box"> <span class="commenter-name"> <input type="text" placeholder="Add a public reply" name="Add Comment"> <button type="submit" class="btn btn-default">Reply</button> <button type="cancel" class="btn btn-default reply-popup">Cancel</button> </span> </div> </div>');
                        }
                    }
                    else {
                        tempAnswerArray.unshift({ id: this.IdStringSocket, messageAnswerId: this.MessageAnswerId, element: '<div id="mess_' + this.IdStringSocket + '" class="comment-box newMessage"> <span class="commenter-name"> <a href="#">' + this.Username + '</a> <span class="comment-time">' + this.dt + ' (' + getTimeZone() + ')' + '</span> </span> <p class="comment-txt more">' + this.MessageText + '</p> ' + '<div class="attachBlock"></div>' + ' <div class="comment-meta"> ' + btnBlockUser + btnRemoveMessage + ' <button onclick="report(this)" class="report"><span class="material-icons" aria-hidden="true" title="' + LocaleJs["Жалоба"] + '">report</span></button> <button data-id="' + this.IdStringSocket + '" onclick="replyMessage(this)" class="comment-reply"><span class="material-icons" aria-hidden="true" title="' + LocaleJs["Ответить"] + '">reply</span></button> </div> <div class="comment-box add-comment reply-box"> <span class="commenter-name"> <input type="text" placeholder="Add a public reply" name="Add Comment"> <button type="submit" class="btn btn-default">Reply</button> <button type="cancel" class="btn btn-default reply-popup">Cancel</button> </span> </div> </div>', pageId: this.PageId });                        
                    }
                });
                //Проходимся по массиву с ответами
                if (tempAnswerArray.length > 0) {
                    $(tempAnswerArray).each(function () {                        
                        //Находим родителя                          
                        var parentMessage = $('#mess_' + this.messageAnswerId);                        
                        var elementAppend = this.element;
                        //Заполняем вложение
                        var attachment = '';
                        if (this.pageId != null) {
                            attachment = getattachmentViewMessage(this.pageId);                                                   
                            $(parentMessage).append(elementAppend);
                            //Добавляем вложение
                            if (attachment != null) {
                                //Получаем добавленный элемент
                                var attachElement = $('#mess_' + this.id);
                                //Добавляем вложение
                                var t = $($(attachElement).find('.attachBlock')).html(attachment); 
                            }
                        }
                        else {                            
                            $(parentMessage).append(elementAppend);
                        }
                    });
                }

                toastMess(LocaleJs["Элементы_получены"]);
                if (isRemoveLast) {
                    scrollChatToTop();
                }
                else {
                    scrollChatToBottom();
                }
            }
            else {
                //toastMess('Элементов больше нет...');
                $('.toTopChatBtn').hide();
            }
            contentIndicator(false);
        },
        error: function (xhr, textStatus, errorThrown) {
            //toastMess('Элементов больше нет...');           
            $('.toTopChatBtn').hide();            
        }
    });
}

function report(c) { 
    var dr = confirm(LocaleJs["Вы_действительно_желаете_отправить_жалобу_на_сообщение"]);
    if (dr) {
        var IdStringSocketId = $(c).parents('.newMessage').attr('id');
        console.log(IdStringSocketId);
        if (IdStringSocketId != undefined && IdStringSocketId != null) {
            chat.server.reportMessage(IdStringSocketId);
        }
    }
}

function blockUser(c) {
    var dr = confirm(LocaleJs["Вы_действительно_желаете_заблокировать_пользователя"]);
    if (dr) {
        var userEmail = $(c).parents('.newMessage').find('.commenter-name a:first').html();
        console.log(userEmail);
        if (userEmail != undefined && userEmail != null) {
            chat.server.blockUser(userEmail);
        }
    }
}

function removeMessage(c) {
    var dr = confirm(LocaleJs["Вы_действительно_желаете_удалить_сообщение"]);
    if (dr) {
        var IdStringSocketId = $(c).parents('.newMessage').attr('id');
        console.log(IdStringSocketId);
        if (IdStringSocketId != undefined && IdStringSocketId != null) {
            chat.server.deleteMessage(IdStringSocketId);
        }
    }
}

function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}

function AddUser(id, name) {    
    console.log('new user add', name);
}

function updateCounter(users) {
    $(counterOnline).html(users.length);
    updateOnlineTableUsers(users);
}

function updateOnlineTableUsers(users) {
    var table = $('#usersOnlineTableData');
    $(table).empty();
    $(users).each(function () {

        //var haveInUserList = $(users).filter(x => x.UserId === this.UserId);
        //console.log('haveInUserList', haveInUserList);
        var stroke = '<div id="user_' + this.UserId + '" class="usersRow"> <span>' + this.UserEmail + '</span> (' + (this.UserFirstName != null ? this.UserFirstName + ' ' : '') + (this.UserLastName != null ? this.UserLastName : '') + ') </div>';
        $(table).append(stroke);
        //console.log('append', stroke);
    });
}