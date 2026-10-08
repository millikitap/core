using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using RectImageArchiveMillikitap.Models;
using RectImageArchiveMillikitap.Models.User;
using RectImageArchiveMillikitap.ModelsMillikitap.SignalRModel;

namespace RectImageArchiveMillikitap.Hubs
{    
    public class ChatHub : Hub
    {
        static List<UserSignalR> Users = new List<UserSignalR>();    
        

        public async Task Connect(int UserId, string UserCode, int bookId, int? pageId)
        {
            using (var Db = new ModelAppBaseMillikitap())
            {
                var checkUser = await Db?.Users?.Where(c => c.UserId == UserId && c.ActivationCode.ToString() == UserCode)?.FirstOrDefaultAsync();
                if (checkUser != null)
                {
                    var id = Context.ConnectionId;
                    await JoinRoom(bookId.ToString(), bookId, pageId, checkUser, id);
                    Clients.Caller.onConnected(checkUser.Email);
                    Clients.Group(bookId.ToString()).onUserConnected(checkUser.Email);
                    await UpdateUsers(bookId);
                }
            }
        }

        public async Task UpdateUsers(int bookId)
        {
            var users = Users?.Where(c => c.BookId == bookId);
            await Clients.Group(bookId.ToString()).onUserUpdate(users?.Select(c=>new { c.UserId, c.UserFirstName, c.UserLastName, c.UserEmail, c.PageId }));
        }

        public async Task JoinRoom(string roomName, int bookId, int? pageId,  User user, string connectionId)
        {
            var haveUser = Users?.Where(c => c.ConnectionId == Context.ConnectionId)?.FirstOrDefault();
            if(haveUser==null)
            {
                Users.Add(new UserSignalR
                {
                    BookId = bookId,
                    PageId = pageId,
                    UserFirstName = user?.FirstName,
                    UserLastName = user?.LastName,
                    ConnectionId = connectionId,
                    UserCode = user.ActivationCode.ToString(),
                    UserEmail = user.Email,
                    UserId= user.UserId                    
                });
                await Groups.Add(Context.ConnectionId, roomName);
                await UpdateUsers(bookId);
            }
        }

        public async Task LeaveRoom(int bookId)
        {
            var haveUser = Users?.Where(c => c.ConnectionId == Context.ConnectionId)?.FirstOrDefault();
            if (haveUser != null)
            {
                Users.Remove(haveUser);
                await Clients.Group(haveUser.BookId.ToString()).onUserDisconnected("user disconnected");                
            }
            await UpdateUsers(bookId);
        }
                
        public override async Task OnDisconnected(bool stopCalled)
        {
            var item = Users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null)
            {
                await LeaveRoom(item.BookId);                
            }
            await base.OnDisconnected(stopCalled);
        }

        /// <summary>
        /// Проверка пользователя на возможность писать в чат
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="BookId"></param>
        /// <returns></returns>
        private async Task<bool> canWriteToChat (int UserId, int BookId)
        {
            using (var Db = new ModelAppBaseMillikitap())
            {
                var userInBlackList = await Db.MessagesChatBlackList?.Where(c => c.BookId == BookId && c.UserId == UserId)?.FirstOrDefaultAsync();
                if(userInBlackList!=null)
                {
                    Clients.Caller.inBlackList();
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Новое сообщение
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task addMessage(string message, int? PageId)
        {
            var id = Context.ConnectionId;
            var userCalling = Users?.Where(c => c.ConnectionId == id)?.FirstOrDefault();
            if (userCalling != null)
            {
                if(await canWriteToChat(userCalling.UserId,userCalling.BookId))
                {
                    string clearText = message.Replace("&nbsp;", string.Empty).Replace("<br>", string.Empty)?.Replace("<div>",string.Empty)?.Replace("</div>",string.Empty).Trim();
                    if (string.IsNullOrEmpty(clearText))
                    {
                        return;
                    }
                    var utcTime = Helper.getUTCStringTime(Helper.dtFormat.timeOnly);
                    var idMessage = Helper.CreateMD5(userCalling + utcTime);
                    await Clients.Group(userCalling.BookId.ToString()).addMessage(message, userCalling.UserEmail, utcTime, idMessage, PageId);
                    await UpdateUsers(userCalling.BookId);
                    await saveMessageToHistory(idMessage, message, false, null, userCalling.UserId, userCalling.BookId, PageId);
                    Clients.Caller.successSendMessage();
                }                
            }
        }

        /// <summary>
        /// Ответить на сообщение
        /// </summary>
        /// <param name="idMessage"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task replyMessage(string idMessage, string message, int? PageId)
        {
            var id = Context.ConnectionId;
            var userCalling = Users?.Where(c => c.ConnectionId == id)?.FirstOrDefault();
            if (userCalling != null)
            {
                if (await canWriteToChat(userCalling.UserId, userCalling.BookId))
                {
                    string clearText = message.Replace("&nbsp;", string.Empty).Replace("<br>", string.Empty)?.Replace("<div>", string.Empty)?.Replace("</div>", string.Empty).Trim();
                    if (string.IsNullOrEmpty(clearText))
                    {
                        return;
                    }
                    var utcTime = Helper.getUTCStringTime(Helper.dtFormat.timeOnly);
                    var idMessageNew = Helper.CreateMD5(userCalling + utcTime);
                    await Clients.Group(userCalling.BookId.ToString()).replyMessage(idMessage, message, userCalling.UserEmail, utcTime, idMessageNew, PageId);
                    await UpdateUsers(userCalling.BookId);
                    await saveMessageToHistory(idMessageNew, message, true, idMessage, userCalling.UserId, userCalling.BookId, PageId);
                    Clients.Caller.successSendMessage();
                }                
            }
        }

        /// <summary>
        /// Пожаловаться на сообщение
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task reportMessage(string IdStringSocketId)
        {
            var IdStringSocketIdStr = IdStringSocketId.Replace("mess_", string.Empty);
            var id = Context.ConnectionId;
            var userCalling = Users?.Where(c => c.ConnectionId == id)?.FirstOrDefault();            
            if (userCalling != null)
            {
                if (await canWriteToChat(userCalling.UserId, userCalling.BookId))
                {
                    using (var Db = new ModelAppBaseMillikitap())
                    {
                        //Подает жалобу
                        var UserIdVote = userCalling.UserId;
                        //Получаем информацию о сообщении
                        var messageInfo = await Db?.ChatMessages?.Where(c => c.IdStringSocket == IdStringSocketIdStr)?.FirstOrDefaultAsync();
                        if(messageInfo!=null)
                        {
                            //Кто писал сообщение
                            var UserIdMessageSender = messageInfo.UserId;
                            //Отправлял ли жалобу
                            var UserHasVoteToUser = await Db?.MessagesChatComplaints?.Where(c => c.UserIdVote == UserIdVote && c.IdStringSocketId == IdStringSocketIdStr && c.BookId == userCalling.BookId)?.FirstOrDefaultAsync();
                            if(UserHasVoteToUser!=null)
                            {
                                //Пользователь уже отправлял жалобу
                                Clients.Caller.failureReportRetry();
                            }
                            else
                            {
                                //Заносим жалобу в таблицу
                                Db.MessagesChatComplaints.Add(new MessagesChatComplaints
                                {
                                    BookId = userCalling.BookId,
                                    IdStringSocketId = IdStringSocketIdStr,
                                    UserIdVote = UserIdVote,
                                    UserHasVote = messageInfo.UserId,
                                    DtReport = DateTime.Now
                                });
                                await Db.SaveChangesAsync();
                                //Отправляем уведомление об успешной жалобе
                                Clients.Caller.successReport(IdStringSocketId);
                            }
                        }
                        else
                        {
                            //Информации о сообщении нет
                            Clients.Caller.failureReportError();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Заблокировать пользователя
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task blockUser(string userEmail)
        {
            var id = Context.ConnectionId;
            var userCalling = Users?.Where(c => c.ConnectionId == id)?.FirstOrDefault();
            var isAccessEditor = await checkAccessEditor();
            if (isAccessEditor)
            {
                using (var Db = new ModelAppBaseMillikitap())
                {
                    var blockedUser = await Db?.Users?.Where(c => c.Username == userEmail)?.FirstOrDefaultAsync();
                    if (blockedUser != null)
                    {
                        Db.MessagesChatBlackList.Add(new MessagesChatBlackList
                        {
                            BookId = userCalling.BookId,
                            UserId = blockedUser.UserId
                        });
                        await Db.SaveChangesAsync();
                    }
                }
                Clients.Caller.userHasBlocked(userEmail);
            }
        }

        /// <summary>
        /// Удалить сообщение
        /// </summary>        
        /// <returns></returns>
        public async Task deleteMessage(string IdStringSocketId)
        {
            var IdStringSocketIdStr = IdStringSocketId.Replace("mess_", string.Empty);
            var id = Context.ConnectionId;            
            var isAccessEditor = await checkAccessEditor();
            if (isAccessEditor)
            {
                using (var Db = new ModelAppBaseMillikitap())
                {
                    var message = await Db?.ChatMessages?.Where(c => c.IdStringSocket == IdStringSocketIdStr)?.FirstOrDefaultAsync();
                    if(message!=null)
                    {
                        Db?.ChatMessages.Remove(message);                        
                        await Db.SaveChangesAsync();
                        var userCalling = Users?.Where(c => c.ConnectionId == id)?.FirstOrDefault();
                        await Clients.Group(userCalling.BookId.ToString()).removeMessageSuccess(IdStringSocketId);
                        Clients.Caller.messageHasRemoved();
                    }
                }
            }
        }

        /// <summary>
        /// Сохранение сообщения в журнал
        /// </summary>
        /// <param name="IdStringSocket"></param>
        /// <param name="MessageText"></param>
        /// <returns></returns>
        public async Task saveMessageToHistory(string IdStringSocket, string MessageText, bool isAnswer, string MessageAnswerId, int UserId, int BookId, int? PageId)
        {
            var now = DateTime.UtcNow;
            using (var Db = new ModelAppBaseMillikitap())
            {
                Db.ChatMessages.Add(new ChatMessages
                {
                    IdStringSocket = IdStringSocket,
                    MessageText = MessageText,
                    dt = now,
                    UserId = UserId,
                    isAnswer = isAnswer,
                    MessageAnswerId = MessageAnswerId,
                    BookId = BookId,
                    PageId = PageId
                });
                await Db.SaveChangesAsync();
            }
        }


        private async Task <bool> checkAccessEditor()
        {
            using (var Db = new ModelAppBaseMillikitap())
            {
                var id = Context.ConnectionId;
                var userCalling = Users?.Where(c => c.ConnectionId == id)?.FirstOrDefault();
                var userFromDb = await Db?.Users?.Where(c => c.UserId == userCalling.UserId)?.FirstOrDefaultAsync();
                if (userFromDb != null)
                {
                    var res = from a in userFromDb.Roles
                              where a.RoleName.Contains(EnumTypes.Roles.Admin) || a.RoleName.Contains(EnumTypes.Roles.Editor) || a.RoleName.Contains(EnumTypes.Roles.Expert)
                              select a;
                    if (!res.Any())
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }            
        }

        /// <summary>
        /// Визуализация набора текста в дискуссии
        /// </summary>
        /// <param name="bookIdConst"></param>       
        /// <returns></returns>
        public async Task isWriteMessageSendInfo(int bookIdConst)
        {
            var id = Context.ConnectionId;
            var userCalling = Users?.Where(c => c.ConnectionId == id)?.FirstOrDefault();
            if (userCalling != null)
            {
                if (await canWriteToChat(userCalling.UserId, userCalling.BookId))
                {                    
                    await Clients.Group(userCalling.BookId.ToString()).isWriteMessageView(new { UserId = userCalling.UserId, UserEmail = userCalling.UserEmail, BookIdConst = bookIdConst });                    
                }
            }
        }
    }
}