﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ForumApi.Common;
using ForumApi.Models;

namespace ForumApi.Controllers
{
    public class ArticleController : ApiController
    {
        private readonly ForumEntities db = new ForumEntities();

        /// <summary>
        /// 查询所有文章 GET api/article
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/article")]
        public ResponseData<object> ShowAllArticles()
        {
            ResponseData<object> responseData;

            try
            {
                // 联合查询 { 标题, 内容, 发布时间, 点赞数, 访问量, 作者昵称 }
                var articles = from a in db.ArticleTb
                               where a.isDel == false
                               from u in db.RoleTb
                               where u.roleId == a.authorId
                               select new { a.title, a.content, a.publishTime, a.likeCount, a.viewCount, u.nickName };

                if (articles != null)
                {
                    responseData = ResponseHelper<object>.SendSuccessResponse(articles.AsEnumerable<object>());
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
        /// 发布文章 POST api/article/publish
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/article/publish")]
        public ResponseData<ArticleTb> PublishNewArticle([FromBody] ArticlePostData postData)
        {
            ResponseData<ArticleTb> responseData;

            if (SessionHelper.IsExist(postData.Guid))
            {

                ArticleTb article = new ArticleTb()
                {
                    title = postData.Title,
                    content = postData.Content,
                    publishTime = DateTime.Now,
                    authorId = postData.AuthorId
                };

                try
                {
                    db.ArticleTb.Add(article);
                    if (db.SaveChanges() > 0)
                    {
                        responseData = ResponseHelper<ArticleTb>.SendSuccessResponse();
                    }
                    else
                    {
                        responseData = ResponseHelper<ArticleTb>.SendErrorResponse("发布失败");
                    }
                }
                catch (Exception ex)
                {
                    responseData = ResponseHelper<ArticleTb>.SendErrorResponse(ex.Message);
                }
            }
            else
            {
                responseData = ResponseHelper<ArticleTb>.SendErrorResponse("未登录", Models.StatusCode.OPERATION_ERROR);
            }

            return responseData;
        }
    }

    /// <summary>
    /// 文章操作需要接收的数据
    /// </summary>
    public class ArticlePostData
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int AuthorId { get; set; }
        public string Guid { get; set; }
    }
}
