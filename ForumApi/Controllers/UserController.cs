﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ForumApi.Models;
using ForumApi.Common;
using System.Web;
using System.Web.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace ForumApi.Controllers
{
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        private readonly ForumApiEntities db = new ForumApiEntities();

        /// <summary>
        /// 展示所有User
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [HttpGet]
        public ResponseData<object> ShowAllUser(int pageSize, int pageIndex)
        {
            ResponseData<object> responseData;

            try
            {
                var userList = from u in db.RoleTb
                               where u.isDel == false
                               select new
                               {
                                   u.roleId,
                                   u.account,
                                   u.avatarUrl,
                                   u.powerNum,
                                   u.nickName
                               };

                int totalCount = userList.Count();
                int totalPages = Convert.ToInt32(Math.Ceiling((double)totalCount / pageSize));

                if (userList != null)
                {
                    // 按热度排序
                    userList =
                        userList
                        .OrderBy(u => u.roleId)
                        .Skip((pageIndex - 1) * pageSize).Take(pageSize);

                    List<object> res = new List<object>()
                    {
                        new { userList, totalCount, totalPages }
                    };

                    responseData = ResponseHelper<object>.SendSuccessResponse(res);
                }
                else
                {
                    responseData = ResponseHelper<object>.SendErrorResponse("暂无文章数据");
                }
            }
            catch (Exception ex)
            {
                responseData = ResponseHelper<object>.SendErrorResponse(ex.Message);
            }

            return responseData;
        }

        /// <summary>
        /// 注册 POST api/user/register
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("register")]
        public ResponseData<RoleTb> UserRegister([FromBody] RoleTb entity)
        {
            ResponseData<RoleTb> responseData;

            RoleTb user = new RoleTb
            {
                account = entity.account,
                avatarUrl = "avatar//4f56e38d-1734-4f27-8e95-d542dedddfc8.jpg",
                nickName = entity.nickName,
                pwd = entity.pwd
            };

            try
            {
                db.RoleTb.Add(user);
                if (db.SaveChanges() > 0)
                {
                    responseData = ResponseHelper<RoleTb>.SendSuccessResponse();
                }
                else
                {
                    responseData = ResponseHelper<RoleTb>.SendErrorResponse("添加失败");
                }
            }
            catch (Exception ex)
            {
                if (db.RoleTb.Where(u => u.isDel == false && u.account == user.account) != null)
                {
                    responseData = ResponseHelper<RoleTb>.SendErrorResponse("账号已存在", Models.StatusCode.OPERATION_ERROR);
                }
                else
                {
                    responseData = ResponseHelper<RoleTb>.SendErrorResponse(ex.Message);
                }
            }

            return responseData;
        }

        /// <summary>
        /// 登陆 api/user/login
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("login")]
        public ResponseData<object> UserLogin([FromBody] RoleTb entity)
        {
            ResponseData<object> responseData;

            RoleTb user = new RoleTb
            {
                account = entity.account,
                pwd = entity.pwd
            };

            try
            {
                var loginUser =
                    db.RoleTb
                    .Where(u => u.isDel == false && u.account == user.account && u.pwd == user.pwd)
                    .FirstOrDefault();

                if (loginUser != null)
                {
                    string guid = Guid.NewGuid().ToString();

                    var loginUserMsg = ResponseHelper<object>.SetLoginMsg(guid, loginUser.account, loginUser.roleId, loginUser.powerNum);

                    HttpContext.Current.Session[guid] = loginUser.account;
                    HttpContext.Current.Session.Timeout = 30;

                    responseData = ResponseHelper<object>.SendSuccessResponse(loginUserMsg);
                }
                else
                {
                    responseData = ResponseHelper<object>.SendErrorResponse("登陆失败，请检查账号或密码是否正确");
                }
            }
            catch (Exception ex)
            {
                responseData = ResponseHelper<object>.SendErrorResponse("登陆失败: " + ex.Message);
            }

            return responseData;
        }

        /// <summary>
        /// 展示某人详细信息 GET api/user/show/{userId}
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("show/{userId}")]
        public ResponseData<object> ShowUserMessage(int userId)
        {
            ResponseData<object> responseData;

            try
            {
                var user = from u in db.RoleTb
                           where u.roleId == userId
                           select new
                           {
                               u.roleId,
                               u.nickName,
                               u.avatarUrl,
                               u.account,
                               u.powerNum
                           };

                var likeList = from l in db.LikeTb
                               where l.userId == userId
                               from a in db.ArticleTb
                               where a.articleId == l.articleId
                               select new
                               {
                                   a.articleId,
                                   a.title
                               };

                if (user != null)
                {
                    List<object> res = new List<object>()
                    {
                        new { user, likeList }
                    };

                    responseData = ResponseHelper<object>.SendSuccessResponse(res);
                }
                else
                {
                    responseData = ResponseHelper<object>.SendErrorResponse("未找到此用户");
                }
            }
            catch (Exception ex)
            {
                responseData = ResponseHelper<object>.SendErrorResponse(ex.Message);
            }

            return responseData;
        }

        /// <summary>
        /// 修改用户昵称/密码 POST api/user/update/{userId}
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userPostData"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("update/{userId}")]
        public ResponseData<object> UpdateUserMessage(int userId, [FromBody] UserPostData userPostData)
        {
            ResponseData<object> responseData;

            if (SessionHelper.IsExist(userPostData.Guid))
            {
                RoleTb user = db.RoleTb.Where(u => u.isDel == false && u.roleId == userId).FirstOrDefault();

                if (user != null)
                {
                    user.nickName = userPostData.NickName ?? user.nickName;
                    user.pwd = userPostData.Pwd ?? user.pwd;

                    try
                    {
                        db.Entry(user).State = System.Data.Entity.EntityState.Modified;

                        if (db.SaveChanges() > 0)
                        {
                            responseData = ResponseHelper<object>.SendSuccessResponse();
                        }
                        else
                        {
                            responseData = ResponseHelper<object>.SendErrorResponse("修改失败");
                        }
                    }
                    catch (Exception ex)
                    {
                        responseData = ResponseHelper<object>.SendErrorResponse(ex.Message);
                    }
                }
                else
                {
                    responseData = ResponseHelper<object>.SendErrorResponse("未找到该用户");
                }
            }
            else
            {
                responseData = ResponseHelper<object>.SendErrorResponse("未登录", Models.StatusCode.OPERATION_ERROR);
            }

            return responseData;
        }

        /// <summary>
        /// 获取某人权限等级 GET api/user/getpower/{userId}
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("getPower/{userId}")]
        public ResponseData<int> GetUserPower(int userId)
        {
            ResponseData<int> responseData;

            try
            {
                var powerNum = from u in db.RoleTb
                               where u.roleId == userId
                               select u.powerNum;

                if (powerNum != null)
                {

                    responseData = ResponseHelper<int>.SendSuccessResponse(powerNum.AsEnumerable());
                }
                else
                {
                    responseData = ResponseHelper<int>.SendErrorResponse("未找到此用户");
                }
            }
            catch (Exception ex)
            {
                responseData = ResponseHelper<int>.SendErrorResponse(ex.Message);
            }

            return responseData;
        }

        /// <summary>
        /// 管理员修改用户权限 POST api/user/power/{userId}
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userPostData"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("power/{userId}")]
        public ResponseData<object> ChangeUserPower(int userId, [FromBody] UserPostData userPostData)
        {
            ResponseData<object> responseData;

            if (SessionHelper.IsExist(userPostData.Guid))
            {
                string adminAccount = HttpContext.Current.Session[userPostData.Guid].ToString();

                RoleTb admin = db.RoleTb.Where(u => u.isDel == false && u.account == adminAccount).FirstOrDefault();

                //判断是否为管理员
                if (admin != null && admin.powerNum == 99)
                {
                    RoleTb user = db.RoleTb.Where(u => u.isDel == false && u.roleId == userId).FirstOrDefault();

                    // 判断要操作的用户是否存在
                    if (user != null)
                    {
                        user.powerNum = userPostData.PowerNum;
                        try
                        {
                            db.Entry(user).State = System.Data.Entity.EntityState.Modified;

                            if (db.SaveChanges() > 0)
                            {
                                responseData = ResponseHelper<object>.SendSuccessResponse();
                            }
                            else
                            {
                                responseData = ResponseHelper<object>.SendErrorResponse("修改失败");
                            }
                        }
                        catch (Exception ex)
                        {
                            responseData = ResponseHelper<object>.SendErrorResponse(ex.Message);
                        }
                    }
                    else
                    {
                        responseData = ResponseHelper<object>.SendErrorResponse("未找到该用户");
                    }
                }
                else
                {
                    responseData = ResponseHelper<object>.SendErrorResponse("用户登陆失效或权限不足", Models.StatusCode.OPERATION_ERROR);
                }
            }
            else
            {
                responseData = ResponseHelper<object>.SendErrorResponse("未登录", Models.StatusCode.OPERATION_ERROR);
            }

            return responseData;
        }

        /// <summary>
        /// 上传头像 POST api/user/upload
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("upload")]
        public async Task<ResponseData<object>> UploadAvatar()
        {
            ResponseData<object> responseData;

            string userGuid = HttpContext.Current.Request.Form["guid"];

            if (SessionHelper.IsExist(userGuid))
            {
                string adminAccount = HttpContext.Current.Session[userGuid].ToString();

                RoleTb user = db.RoleTb.Where(u => u.account == adminAccount).FirstOrDefault();

                if (user != null)
                {
                    string uploadFolderPath = HostingEnvironment.MapPath("~/avatars");

                    if (!Directory.Exists(uploadFolderPath))
                    {
                        Directory.CreateDirectory(uploadFolderPath);
                    }

                    Guid imgGuid = Guid.NewGuid();

                    List<string> files = new List<string>();
                    var provider = new WithExtensionMultipartFormDataStreamProvider(uploadFolderPath, imgGuid);
                    try
                    {
                        // Read the form data.
                        await Request.Content.ReadAsMultipartAsync(provider);

                        // This illustrates how to get the file names.

                        foreach (var file in provider.FileData)
                        {
                            string fileName = Path.Combine("avatars", Path.GetFileName(file.LocalFileName));
                            files.Add(fileName);

                            user.avatarUrl = fileName;
                            db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                        }
                        if (db.SaveChanges() > 0)
                        {
                            responseData = ResponseHelper<object>.SendSuccessResponse(files);
                        }
                        else
                        {
                            responseData = ResponseHelper<object>.SendErrorResponse("头像上传失败");
                        }
                    }
                    catch (Exception ex)
                    {
                        responseData = ResponseHelper<object>.SendErrorResponse(ex.Message);
                    }
                }
                else
                {
                    responseData = ResponseHelper<object>.SendErrorResponse("登陆失效", Models.StatusCode.OPERATION_ERROR);
                }
            }
            else
            {
                responseData = ResponseHelper<object>.SendErrorResponse("未登录", Models.StatusCode.OPERATION_ERROR);
            }

            return responseData;
        }

        [HttpGet]
        [Route("getId")]
        public ResponseData<int> GetUserIdByAccount(string account)
        {
            ResponseData<int> responseData;

            try
            {
                var userId = from u in db.RoleTb
                             where u.account == account
                             select u.roleId;

                if (userId != null)
                {

                    responseData = ResponseHelper<int>.SendSuccessResponse(userId.AsEnumerable());
                }
                else
                {
                    responseData = ResponseHelper<int>.SendErrorResponse("未找到此用户");
                }
            }
            catch (Exception ex)
            {
                responseData = ResponseHelper<int>.SendErrorResponse(ex.Message);
            }

            return responseData;
        }
    }

    public class UserPostData
    {
        public string Guid { get; set; }
        public int UserId { get; set; }
        public string Account { get; set; }
        public string NickName { get; set; }
        public string Pwd { get; set; }
        public int PowerNum { get; set; }
        public string AvatarUrl { get; set; }
    }
}
