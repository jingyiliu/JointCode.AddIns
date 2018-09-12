//using System.Collections.Generic;
//using System.Text.RegularExpressions;
//using System.Web;
//using System.Web.Routing;

//namespace JointCode.AddIns.Mvc.System
//{
//    public class JcRoute : Route
//    {
//        public JcRoute(string url, IRouteHandler routeHandler) 
//            : base(url, routeHandler)
//        {
//        }

//        public JcRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
//            : base(url, defaults, routeHandler)
//        {
//        }

//        public JcRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler) 
//            : base(url, defaults, constraints, routeHandler)
//        {
//        }

//        public JcRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler) 
//            : base(url, defaults, constraints, dataTokens, routeHandler)
//        {
//        }

//        public override RouteData GetRouteData(HttpContextBase httpContext)
//        {
//            var virtualPath = httpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(2) + httpContext.Request.PathInfo;
//            //RouteValueDictionary values = this._parsedRoute.Match(virtualPath, this.Defaults);
//            //if (values == null)
//            //{
//            //    return null;
//            //}
//            //RouteData data = new RouteData(this, this.RouteHandler);
//            //if (!this.ProcessConstraints(httpContext, values, RouteDirection.IncomingRequest))
//            //{
//            //    return null;
//            //}
//            //foreach (KeyValuePair<string, object> pair in values)
//            //{
//            //    data.Values.Add(pair.Key, pair.Value);
//            //}
//            //if (this.DataTokens != null)
//            //{
//            //    foreach (KeyValuePair<string, object> pair2 in this.DataTokens)
//            //    {
//            //        data.DataTokens[pair2.Key] = pair2.Value;
//            //    }
//            //}
//            //return data;

//            var parts = SplitUrlToPathSegmentStrings(virtualPath);
//            return null;

//            //// 构造 regex
//            //domainRegex = CreateRegex(Domain);
//            //pathRegex = CreateRegex(Url);

//            //// 请求信息
//            //string requestDomain = httpContext.Request.Headers["host"];
//            //if (!string.IsNullOrEmpty(requestDomain))
//            //{
//            //    if (requestDomain.IndexOf(":") > 0)
//            //    {
//            //        requestDomain = requestDomain.Substring(0, requestDomain.IndexOf(":"));
//            //    }
//            //}
//            //else
//            //{
//            //    requestDomain = httpContext.Request.Url.Host;
//            //}
//            //string requestPath = httpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(2) + httpContext.Request.PathInfo;

//            //// 匹配域名和路由
//            //Match domainMatch = domainRegex.Match(requestDomain);
//            //Match pathMatch = pathRegex.Match(requestPath);

//            //// 路由数据
//            //RouteData data = null;
//            //if (domainMatch.Success && pathMatch.Success)
//            //{
//            //    data = new RouteData(this, RouteHandler);

//            //    // 添加默认选项
//            //    if (Defaults != null)
//            //    {
//            //        foreach (KeyValuePair<string, object> item in Defaults)
//            //        {
//            //            data.Values[item.Key] = item.Value;
//            //        }
//            //    }

//            //    // 匹配域名路由
//            //    for (int i = 1; i < domainMatch.Groups.Count; i++)
//            //    {
//            //        Group group = domainMatch.Groups[i];
//            //        if (group.Success)
//            //        {
//            //            string key = domainRegex.GroupNameFromNumber(i);

//            //            if (!string.IsNullOrEmpty(key) && !char.IsNumber(key, 0))
//            //            {
//            //                if (!string.IsNullOrEmpty(group.Value))
//            //                {
//            //                    data.Values[key] = group.Value;
//            //                }
//            //            }
//            //        }
//            //    }

//            //    // 匹配域名路径
//            //    for (int i = 1; i < pathMatch.Groups.Count; i++)
//            //    {
//            //        Group group = pathMatch.Groups[i];
//            //        if (group.Success)
//            //        {
//            //            string key = pathRegex.GroupNameFromNumber(i);

//            //            if (!string.IsNullOrEmpty(key) && !char.IsNumber(key, 0))
//            //            {
//            //                if (!string.IsNullOrEmpty(group.Value))
//            //                {
//            //                    data.Values[key] = group.Value;
//            //                }
//            //            }
//            //        }
//            //    }
//            //}

//            //return data;
//        }

//        Regex CreateRegex(string source)
//        {
//            // 替换
//            source = source.Replace("/", @"\/?");
//            source = source.Replace(".", @"\.?");
//            source = source.Replace("-", @"\-?");
//            source = source.Replace("{", @"(?<");
//            source = source.Replace("}", @">([a-zA-Z0-9_]*))");
//            return new Regex("^" + source + "$");
//        }

//        static List<string> SplitUrlToPathSegmentStrings(string url)
//        {
//            var list = new List<string>();
//            if (!string.IsNullOrEmpty(url))
//            {
//                int index;
//                for (int i = 0; i < url.Length; i = index + 1)
//                {
//                    index = url.IndexOf('/', i);
//                    if (index == -1)
//                    {
//                        string str2 = url.Substring(i);
//                        if (str2.Length > 0)
//                            list.Add(str2);
//                        return list;
//                    }
//                    var item = url.Substring(i, index - i);
//                    if (item.Length > 0)
//                        list.Add(item);
//                    list.Add("/");
//                }
//            }
//            return list;
//        }

//        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
//        {
//            return base.GetVirtualPath(requestContext, RemoveDomainTokens(values));
//        }

//        RouteValueDictionary RemoveDomainTokens(RouteValueDictionary values)
//        {
//            //Regex tokenRegex = new Regex(@"({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?");
//            //Match tokenMatch = tokenRegex.Match(Domain);
//            //for (int i = 0; i < tokenMatch.Groups.Count; i++)
//            //{
//            //    Group group = tokenMatch.Groups[i];
//            //    if (group.Success)
//            //    {
//            //        string key = group.Value.Replace("{", "").Replace("}", "");
//            //        if (values.ContainsKey(key))
//            //            values.Remove(key);
//            //    }
//            //}

//            return values;
//        }
//    }
//}
